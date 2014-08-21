using System.Collections.Generic;
using System.Linq;
using log4net;
using log4net.Appender;
using NUnit.Framework;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;

namespace ObjectsLogAppender.Tests
{
    [TestFixture]
    public class ObjectsAppenderTests
    {
        [TearDown]
        public void Teardown()
        {
            LogManager.Shutdown();
        }

        [Test]
        public void LogSomeString_NoClassesConfiguration_ShouldWriteOurStringToFile()
        {
            // ILog logger = GetConfiguredLog(new Dictionary<string, List<string>>() {{"bla", new List<string>() {"bla2"}}});
            ILog logger = GetConfiguredLog(new Dictionary<string, List<string>>());
            string stringToLog = "very wired string to log";
            logger.Debug(stringToLog);

            MemoryAppender appender = GetMemoryAppender();
            LoggingEvent[] loggedEvents = appender.GetEvents();
            CollectionAssert.IsNotEmpty(loggedEvents);
            var loggedEvent = loggedEvents[0];
            Assert.That(loggedEvent.Level, Is.EqualTo(Level.Debug));
            Assert.That(loggedEvent.RenderedMessage, Is.EqualTo(stringToLog));
        }

        [Test]
        public void LogSimpleClass_OneClassWithOneMemberConfiguration_ShouldWriteOnlyThisMember()
        {
            ILog logger =
                GetConfiguredLog(new Dictionary<string, List<string>>()
                {
                    {"SimpleClass", new List<string>() {"SomeInt"}}
                });

            string stringNotToLog = "very wired string to log";
            int integerToLog = 12481632;

            var objectToWrite = new SimpleClass()
            {
                SomeString = stringNotToLog,
                SomeInt = integerToLog
            };

            logger.Debug(objectToWrite);
            
            MemoryAppender appender = GetMemoryAppender();
            LoggingEvent[] loggedEvents = appender.GetEvents();
            CollectionAssert.IsNotEmpty(loggedEvents);
            var loggedEvent = loggedEvents[0];
            Assert.That(loggedEvent.Level, Is.EqualTo(Level.Debug));
            string stringToLog = "SomeInt=" + integerToLog;
            Assert.That(loggedEvent.RenderedMessage, Is.StringContaining(stringToLog));
            Assert.That(loggedEvent.RenderedMessage, Is.Not.StringContaining(stringNotToLog));
        }

        [Test]
        public void LogSimpleClass_NoClassConfigurationWithSerliazeUnknownTrue_ShouldWriteJsonOfThisClass()
        {
            ILog logger =
                GetConfiguredLog(new Dictionary<string, List<string>>(),serializeUnknownObjects:true);

            string stringToLog = "very wired string to log";
            int integerToLog = 12481632;

            var objectToWrite = new SimpleClass()
            {
                SomeString = stringToLog,
                SomeInt = integerToLog
            };
            logger.Debug(objectToWrite);

            MemoryAppender appender = GetMemoryAppender();

            LoggingEvent[] loggedEvents = appender.GetEvents();

            CollectionAssert.IsNotEmpty(loggedEvents);
            LoggingEvent loggingEvent = loggedEvents[0];
            Assert.That(loggingEvent.Level, Is.EqualTo(Level.Debug));
            string oneWay = "{\"SomeInt\":12481632,\"SomeString\":\"very wired string to log\"}";
            string otherWay = "{\"SomeString\":\"very wired string to log\",\"SomeInt\":12481632}";

            Assert.That(loggingEvent.RenderedMessage, Is.EqualTo(oneWay).Or.EqualTo(otherWay));
        }


        [Test]
        public void LogSimpleClass_NoClassConfigurationWithSerliazeUnknownFalse_ShouldNotWriteAnything()
        {
            ILog logger =
                GetConfiguredLog(new Dictionary<string, List<string>>(), serializeUnknownObjects: false);

            string stringNotToLog = "very wired string to log";
            int integerNotToLog = 12481632;

            var objectToWrite = new SimpleClass()
            {
                SomeString = stringNotToLog,
                SomeInt = integerNotToLog
            };
            logger.Debug(objectToWrite);

            MemoryAppender appender = GetMemoryAppender();

            LoggingEvent[] loggingEvents = appender.GetEvents();
            CollectionAssert.IsEmpty(loggingEvents);
        }

        private ILog GetConfiguredLog(Dictionary<string, List<string>> classesToMembers, bool serializeUnknownObjects = true)
        {
            ILog result = LogManager.GetLogger("Test");

            Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository();

            PatternLayout patternLayout = new PatternLayout();
            patternLayout.ConversionPattern = "%d [%t] %-5p %c %m%n";
            patternLayout.ActivateOptions();

            ObjectsAppender objectsAppender = new ObjectsAppender();

            foreach (KeyValuePair<string, List<string>> classToMembers in classesToMembers)
            {
                ClassConfiguration classConfiguration = new ClassConfiguration();

                classConfiguration.Name = classToMembers.Key;

                foreach (string members in classToMembers.Value)
                {
                    classConfiguration.AddMember(members);
                }

                objectsAppender.AddClass(classConfiguration);
            }

            objectsAppender.SerializeUnknownObjects = serializeUnknownObjects;

            MemoryAppender memory = new MemoryAppender();
            memory.ActivateOptions();

            objectsAppender.AddAppender(memory);
            objectsAppender.ActivateOptions();

            Logger casted = (Logger) result.Logger;
            casted.AddAppender(objectsAppender);

            casted.Level = Level.Debug;
            hierarchy.Configured = true;
            
            return result;
        }

        private static MemoryAppender GetMemoryAppender()
        {
            return LogManager.GetRepository().GetAppenders().OfType<MemoryAppender>()
                             .FirstOrDefault();
        }
    }

    #region testClasses

    public class SimpleClass
    {
        public string SomeString { get; set; }
        public int SomeInt { get; set; }
    }

    #endregion 
}
