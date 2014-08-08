﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        public string Classes { get; set; }
        public string MemberNameAndValueSeperator { get; set; }
        public string SeperatorBetweenMembers { get; set; }
        #endregion

    
        #region private members

        private IMembersExtractor _membersExtractor;
        #endregion

        #region ctor
        public ObjectsAppender()
        {
            _membersExtractor = new ReflectionMembersExtractor();
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

            var jsonSerlizaer = new JavaScriptSerializer();
            List<string> membersList;
            
            //if we don't have the type in extractor or we don't have memberList just do json
            //maby move to configuation ? serliaze if unknown type?
            if (!_membersExtractor.GetClassMapping(typeName, out membersList) || membersList == null || membersList.Count == 0)
            {
                var json = jsonSerlizaer.Serialize(loggingEvent.MessageObject);
                CallAllAppenders(CreateNewLoggingEvent(loggingEvent, json));
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
                string jsonMemberValue = jsonSerlizaer.Serialize(memberValue);
                //add member to log 
                logMembersList.Add(realMemberName + MemberNameAndValueSeperator + jsonMemberValue);
            }
            string logMessage = string.Join(SeperatorBetweenMembers, logMembersList);
            CallAllAppenders(CreateNewLoggingEvent(loggingEvent, logMessage));
        }

        public override void ActivateOptions()
        {
            base.ActivateOptions();
            Initialize();
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

            if (Classes == null)
                return;
            //string should look like this ClassName={ClassProperty1;ClassMember2;..}*ClassName2={ClassMember1;ClassProperty2;}*..
            foreach (var classString in Classes.Split('*'))
            {
                string[] classAndMembers = classString.Split('=');
                //validation: should be [ClassName,{ClassProperty1;ClassProperty2}]
                if (classAndMembers.Length != 2) continue;
                string className = classAndMembers[0];
                string allMembers = classAndMembers[1];
                //validation: members should be {ClassProperty1;ClassProperty2}
                if (!allMembers.StartsWith("{") || !allMembers.EndsWith("}"))
                    continue;
                string allMembersWithoutBarkets = classAndMembers[1].Substring(1, classAndMembers[1].Length - 2);

                _membersExtractor.AddClassMapping(className, allMembersWithoutBarkets.Split(';').ToList());
            }
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
