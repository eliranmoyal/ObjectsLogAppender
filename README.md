ObjectsLogAppender
==================

Custom appender for log4net. Gives you the ability to write any object to log.

You can configure the properties that you want to write to log.

example of usage
Assume we have Class SomeClass with some properties and we want only to write LastName and ID.
Also we have ComplexClass with array,list,property Of Class Y that have IntY property, and a property of Class X that have StringX property
the result of writing:
```c#
 var complexObject = new ComplexClass()
            {
                CoolNamesInList = new string[] {"eliran", "moyal"},
                CoolNumbersInArray = new int[] {1, 2, 4, 8, 16, 32, 64},
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
 log.Debug(complexObject);
 
 var eliran= new SomeClass()
            {
                FirstName = "eliran",
                LastName = "moyal",
                ID = 11

            };

  log.Debug(eliran);
  
  ```
  
  the result will be:
 
2014-08-06 22:41:39,183 [1] DEBUG Log4ChecksConsole.LoggingClass LastName="moyal";ID=11
2014-08-06 22:41:39,210 [1] DEBUG Log4ChecksConsole.LoggingClass CoolNumbersInArray=[1,2,4,8,16,32,64];CoolNamesInList=["eliran","moyal"];IntY=13;StringX="X X X"

            
with configuration:
  <log4net>
    
    <root>
      <level value="ALL" />
      <appender-ref ref="ObjectsAppender" />
    </root>

    <appender name="ObjectsAppender" type="ObjectsLogAppender.ObjectsAppender, ObjectsLogAppender">
      <Classes value="SomeClass={LastName;ID}*ComplexClass={CoolNumbersInArray;CoolNamesInList;Y>IntY;Y>X>StringX}" />
      <PropNameAndValueSeperator value ="=" />
      <SeperatorBetweenProps value =";" />
      <appender-ref ref="LogFileAppender"/>
      <layout type="log4net.Layout.PatternLayout,log4net">
        <param name="ConversionPattern" value="%d{ABSOLUTE} %-5p %c{1}:%L - %m%n" />
      </layout>
    </appender>

    <appender name="LogFileAppender" type="log4net.Appender.RollingFileAppender">
      <param name="File" value="MyFirstLogger.log"/>
      <lockingModel type="log4net.Appender.FileAppender+MinimalLock" />
      <appendToFile value="true" />
      <rollingStyle value="Size" />
      <maxSizeRollBackups value="2" />
      <maximumFileSize value="1MB" />
      <staticLogFileName value="true" />
      <layout type="log4net.Layout.PatternLayout">
        <param name="ConversionPattern" value="%d [%t] %-5p %c %m%n"/>
      </layout>
    </appender>

  </log4net>

  
  TODO:
  1. handle configuration change on runtime
  2. support fields + private fields
  
