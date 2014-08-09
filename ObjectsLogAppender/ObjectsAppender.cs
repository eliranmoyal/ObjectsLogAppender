using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using log4net.Appender;
using log4net.Core;

namespace ObjectsLogAppender
{
    public class ObjectsAppender : ForwardingAppender
    {
        #region configuration props
        public string MemberNameAndValueSeperator { get; set; }
        public string SeperatorBetweenMembers { get; set; }
        public bool SerializeUnknownObjects { get; set; }
        #endregion
    
        #region private members
        private IMembersExtractor _membersExtractor;
        private IObjectSerliazer _serliazer;
        //initialize from configuration. using AddClass function
        private List<ClassConfiguration> _classesConfigurations = new List<ClassConfiguration>(); 
        #endregion

        #region ctors
        public ObjectsAppender()
        {
            _membersExtractor = new ReflectionMembersExtractor();
            _serliazer = new JsonSerliazer();
        }

        public ObjectsAppender(IMembersExtractor membersExtractor, IObjectSerliazer serliazer)
        {
            _membersExtractor = membersExtractor;
            _serliazer = serliazer;
        }

        #endregion

        #region ForwardingAppender overrides
        protected override void Append(LoggingEvent loggingEvent)
        {
            object messageObject = loggingEvent.MessageObject;
            Type messageType = messageObject.GetType();
            var typeName = messageType.Name;

            //normal logging
            if (typeName.ToLower() == "string")
            {
                CallAllAppenders(loggingEvent);
                return;
            }

           
            List<string> membersList;
            
            //if we don't have the type in extractor or we don't have memberList just serialize
            
            if (!_membersExtractor.GetClassMapping(typeName, out membersList) || membersList == null || membersList.Count == 0)
            {
                if (SerializeUnknownObjects)
                {
                    var serliazedObjectString = _serliazer.SerializeObject(loggingEvent.MessageObject);
                    CallAllAppenders(CreateNewLoggingEvent(loggingEvent, serliazedObjectString));
                }
                return;
            }

            var logMembersList = new List<string>();
            //for each member in configuration get memberName And Value
            foreach (var memberName in membersList)
            {
                object memberValue;
                string realMemberName;
                if(!_membersExtractor.ExtractMemberValue(messageObject, memberName,out memberValue,out realMemberName))
                    continue;
              
                //if we cant find the member move to next member.
                string serliazedMemberValue = _serliazer.SerializeObject(memberValue);
                //add member to log 
                logMembersList.Add(realMemberName + MemberNameAndValueSeperator + serliazedMemberValue);
            }
            string logMessage = string.Join(SeperatorBetweenMembers, logMembersList);
            CallAllAppenders(CreateNewLoggingEvent(loggingEvent, logMessage));
        }

        public override void ActivateOptions()
        {
            base.ActivateOptions();
            Initialize();
        }


        public void AddClass(ClassConfiguration classConfiguration)
        {
            _classesConfigurations.Add(classConfiguration);
        }

        #endregion

        #region private functions

        private void Initialize()
        {
            //defaults init
            if (string.IsNullOrEmpty(MemberNameAndValueSeperator))
                MemberNameAndValueSeperator = "=";
            if (string.IsNullOrEmpty(SeperatorBetweenMembers))
                SeperatorBetweenMembers = ";";

            //type to members init
            _membersExtractor.RemoveAllClassesMapping();
            _membersExtractor.MembersChainIndicator = '>';

            if (_classesConfigurations == null || _classesConfigurations.Count == 0)
                return;

            _classesConfigurations.ForEach(config=>_membersExtractor.AddClassMapping(config.Name,config.MembersList));
        }

        private LoggingEvent CreateNewLoggingEvent(LoggingEvent loggingEvent, string newData)
        {
            var loggingData = loggingEvent.GetLoggingEventData();
            loggingData.Message = newData;
            return new LoggingEvent(loggingData);
        }
        private void CallAllAppenders(LoggingEvent loggingEvent)
        {
            foreach (var appender in Appenders)
            {
                appender.DoAppend(loggingEvent);
            }
        }


        #endregion
    }
}
