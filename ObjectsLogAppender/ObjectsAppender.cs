using System;
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
        public string PropNameAndValueSeperator { get; set; }
        public string SeperatorBetweenProps { get; set; }
        #endregion

        #region private members
      
        private Dictionary<string, List<string>> _typeToProperties;
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
            List<string> propList;
            //if we don't have the type in configuration or we don't have propList just do json
            if (!_typeToProperties.TryGetValue(typeName, out propList) || propList == null || propList.Count == 0)
            {
                var json = jsonSerlizaer.Serialize(loggingEvent.MessageObject);
                CallAllAppenders(CreateNewLoggingEvent(loggingEvent, json));
                return;
            }

            var logPropsList = new List<string>();
            //for each property in configuration get propName And Value
            foreach (var propName in propList)
            {
                Type type = messageType;
                object obj = messageObject;
                string realPropertyName = propName;


                //treat nestedProperties
                if (propName.Contains(">"))
                {
                    //if drilling failed move to next property
                    if (!DrillToLastProperty(propName, ref type, ref obj, ref realPropertyName))
                        continue;

                }
                string jsonPropValue;
                //if we cant find the property move to next property.
                if (!GetJsonStringOfProperty(type, realPropertyName, obj, jsonSerlizaer, out jsonPropValue)) continue;
                //add prop to log 
                logPropsList.Add(realPropertyName + PropNameAndValueSeperator + jsonPropValue);
            }
            string logMessage = string.Join(SeperatorBetweenProps, logPropsList);
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
            if (string.IsNullOrEmpty(PropNameAndValueSeperator))
                PropNameAndValueSeperator = "=";
            if (string.IsNullOrEmpty(SeperatorBetweenProps))
                SeperatorBetweenProps = ";";

            //type to props init
            _typeToProperties = new Dictionary<string, List<string>>();
            if (Classes == null)
                return;
            //string should look like this ClassName={ClassProperty1;ClassProperty2;..}*ClassName2={ClassProperty1;ClassProperty2;}*..
            foreach (var classString in Classes.Split('*'))
            {
                string[] classAndProps = classString.Split('=');
                //validation: should be [ClassName,{ClassProperty1;ClassProperty2}]
                if (classAndProps.Length != 2) continue;
                string className = classAndProps[0];
                string allProps = classAndProps[1];
                //validation: props should be {ClassProperty1;ClassProperty2}
                if (!allProps.StartsWith("{") || !allProps.EndsWith("}"))
                    continue;
                string allPropsWithoutBarkets = classAndProps[1].Substring(1, classAndProps[1].Length - 2);

                _typeToProperties.Add(className, allPropsWithoutBarkets.Split(';').ToList());


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

        #region reflectionMethods
        
        private bool DrillToLastProperty(string propName, ref Type type, ref object obj, ref string lastPropertyName)
        {
            string[] nesting = propName.Split('>');
            bool validChain = true;

            //move to the last link of the chain 
            for (int i = 0; i < nesting.Length - 1; i++)
            {
                string currentProperty = nesting[i];
                obj = GetPropertyValue(type, currentProperty, obj);
                if (obj == null)
                {
                    validChain = false;
                    break;
                }
                type = obj.GetType();

            }
            lastPropertyName = nesting.Last();
            return validChain;

        }

        private bool GetJsonStringOfProperty(Type type, string propName, object obj, JavaScriptSerializer jsonSerlizaer,
    out string jsonPropValue)
        {
            var propValue = GetPropertyValue(type, propName, obj);
            if (propValue == null)
            {
                jsonPropValue = null;
                return false;
            }
            jsonPropValue = jsonSerlizaer.Serialize(propValue);
            return true;
        }

        private object GetPropertyValue(Type type, string propName, object obj)
        {
            PropertyInfo propertyInfo = type.GetProperty(propName);
            //validation: no such property
            if (propertyInfo == null) return null;


            return propertyInfo.GetValue(obj, null);
        }
        #endregion

        #endregion
    }
}
