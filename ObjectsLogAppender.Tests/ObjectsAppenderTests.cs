using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
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

        [Test]
        public void LogComplexClass_ClassConfigurationWithInnerProperties_ShouldWriteTwoSpecificMembers()
        {

            string classNameToLog = "ComplexClass";
            var membersList = new List<string>() { "CoolNamesInList", "Y>X>_intXPrivateField" };
            ILog logger =
                GetConfiguredLog(new Dictionary<string, List<string>>() { { classNameToLog, membersList } }, serializeUnknownObjects: true);


            var objectToWrite = new ComplexClass()
            {
                BlaBla = "bla",
                CoolNamesInList = new string[] { "eliran", "moyal" },
                CoolNumbersInArray = new int[] { 1, 2, 4, 8, 16, 32, 64 },
                Y = new ClassY()
                {
                    IntY = 13,
                    StringY = "Y Y Y",
                    X = new ClassX()
                    {
                        IntX = 12,
                        StringX = "X X X"
                    }
                }

            };
            objectToWrite.Y.X.SetPrivateInt(14);
        
            logger.Debug(objectToWrite);

            MemoryAppender appender = GetMemoryAppender();

            LoggingEvent[] loggedEvents = appender.GetEvents();

            CollectionAssert.IsNotEmpty(loggedEvents);
            LoggingEvent loggingEvent = loggedEvents[0];
            Assert.That(loggingEvent.Level, Is.EqualTo(Level.Debug));
            string firstMemberStringRepresentation = "CoolNamesInList=[\"eliran\",\"moyal\"]";
            string secondMemberStringRepresentation = "_intXPrivateField=14";
            string oneWay = string.Format("{0};{1}",firstMemberStringRepresentation,secondMemberStringRepresentation);
            string otherWay = string.Format("{1};{0}", firstMemberStringRepresentation, secondMemberStringRepresentation); ;

            Assert.That(loggingEvent.RenderedMessage, Is.EqualTo(oneWay).Or.EqualTo(otherWay));
        }

/*
        [Test]
        public void TimeTest_ComplexClass()
        {

            string classNameToLog = "ComplexClass";
            var membersList = new List<string>() { "CoolNamesInList", "Y>X>_intXPrivateField" };
            ILog logger =
                GetConfiguredLog(new Dictionary<string, List<string>>() { { classNameToLog, membersList } }, serializeUnknownObjects: true);

            
            var objectToWrite = new ComplexClass()
            {
                BlaBla = "bla",
                CoolNamesInList = new string[] { "eliran", "moyal" },
                CoolNumbersInArray = new int[] { 1, 2, 4, 8, 16, 32, 64 },
                Y = new ClassY()
                {
                    IntY = 13,
                    StringY = "Y Y Y",
                    X = new ClassX()
                    {
                        IntX = 12,
                        StringX = "X X X"
                    }
                }

            };
            objectToWrite.Y.X.SetPrivateInt(14);
            decimal numberOfIterations = 500000;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            for (int i = 0; i < numberOfIterations; i++)
            {
                logger.Debug(objectToWrite);
            }
            stopwatch.Stop();
            long elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
            Assert.IsTrue(true);
        }
        */

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
    public class ComplexClass
    {
        public string BlaBla { get; set; }
        public int[] CoolNumbersInArray { get; set; }
        public string[] CoolNamesInList { get; set; }
        public ClassY Y { get; set; }
    }
    public class ClassY
    {
        public string StringY { get; set; }
        public int IntY { get; set; }
        public ClassX X { get; set; }
    }
    public class ClassX
    {
        public string StringX { get; set; }
        public int IntX { get; set; }

        public int IntXField;

        private int _intXPrivateField;

        public void SetPrivateInt(int someValue)
        {
            _intXPrivateField = someValue;
        }
    }

    #endregion 
}
