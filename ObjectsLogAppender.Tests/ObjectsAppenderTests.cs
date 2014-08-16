using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using log4net.Config;
using NUnit.Framework;

namespace ObjectsLogAppender.Tests
{
    [TestFixture]
    public class ObjectsAppenderTests
    {

        [TearDown]
        public void TearDown()
        {
            File.Delete("test.log");
        }

        [Test]
        public void LogSomeString_NoClassesConfiguration_ShouldWriteOurStringToFile()
        {
            // ILog logger = GetConfiguredLog(new Dictionary<string, List<string>>() {{"bla", new List<string>() {"bla2"}}});
            ILog logger = GetConfiguredLog(new Dictionary<string, List<string>>());
            string stringToLog = "very wired string to log";
            logger.Debug(stringToLog);
            Thread.Sleep(500);
            using (FileStream fileStream = File.Open("test.log", FileMode.Open))
            {
                using (StreamReader streamReader = new StreamReader(fileStream))
                {
                    string line = streamReader.ReadLine();
                    Assert.IsNotNullOrEmpty(line);
                    Assert.IsTrue(line.Contains("DEBUG"));
                    Assert.IsTrue(line.Contains(stringToLog));
                }
            }
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
            Thread.Sleep(500);
            using (FileStream fileStream = File.Open("test.log", FileMode.Open))
            {
                using (StreamReader streamReader = new StreamReader(fileStream))
                {
                    string line = streamReader.ReadLine();
                    Assert.IsNotNullOrEmpty(line);
                    Assert.IsTrue(line.Contains("DEBUG"));
                    Assert.IsTrue(line.Contains("SomeInt=" + integerToLog));
                    Assert.IsFalse(line.Contains(stringNotToLog));
                }
            }
        }

        private ILog GetConfiguredLog(Dictionary<string, List<string>> classesToMembers)
        {
            //create configuration from classToMembers
            /* <Class>
                 <name value="SomeClass" />
                <member value="FirstName" />
                <member value="ID" />
            </Class>*/
            string classAndMembersConfiguration = "";
            foreach (KeyValuePair<string, List<string>> classToMembers in classesToMembers)
            {
                classAndMembersConfiguration += "<Class> ";
                classAndMembersConfiguration += "<name value='" + classToMembers.Key + "' /> ";
                foreach (var member in classToMembers.Value)
                {
                    classAndMembersConfiguration += "<member value='" + member + "' /> ";
                }
                classAndMembersConfiguration += "</Class> ";


            }
            string configurationString = @"
                <log4net>
    
                <root>
                    <level value='ALL' />
                    <appender-ref ref='ObjectsAppender' />
                </root>

                <appender name='ObjectsAppender' type='ObjectsLogAppender.ObjectsAppender, ObjectsLogAppender'>
                    <MemberNameAndValueSeperator value ='=' />
                    <SeperatorBetweenMembers value =';' />
                    <SerializeUnknownObjects value='False' /> " +
                      classAndMembersConfiguration + @"
    
                    <appender-ref ref='LogFileAppender'/>
                    <layout type='log4net.Layout.PatternLayout,log4net'>
                    <param name='ConversionPattern' value='%d{ABSOLUTE} %-5p %c{1}:%L - %m%n' />
                    </layout>
                </appender>

                <appender name='LogFileAppender' type='log4net.Appender.RollingFileAppender'>
                    <param name='File' value='test.log'/>
                    <lockingModel type='log4net.Appender.FileAppender+MinimalLock' />
                    <appendToFile value='true' />
                    <rollingStyle value='Size' />
                    <maxSizeRollBackups value='2' />
                    <maximumFileSize value='1MB' />
                    <staticLogFileName value='true' />
                    <layout type='log4net.Layout.PatternLayout'>
                    <param name='ConversionPattern' value='%d [%t] %-5p %c %m%n'/>
                    </layout>
                </appender>

                </log4net>";
            XmlConfigurator.Configure(new MemoryStream(Encoding.UTF8.GetBytes(configurationString)));
            return LogManager.GetLogger("Test");
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
