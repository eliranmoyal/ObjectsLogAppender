ObjectsLogAppender
==================

Custom appender for log4net. Gives you the ability to write any object to log.
<br>
You can configure the members that you want to write to log.
<br>
<br>
####Supports:
* Properties
* Public fields
* Private fields
<br>

####Example of usage:
<br>
Assume we have Class SomeClass with some properties and we want only to write LastName and ID.<br>
Also we have ComplexClass with array,list,property Of Class Y that have IntY property, and a property of Class X that have StringX property
 and also have private int field<br>
The Code
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
 complexObject.Y.X.SetPrivateInt(14);
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
 
2014-08-07 21:22:57,489 [1] DEBUG Log4ChecksConsole.LoggingClass LastName="moyal";ID=11
<br>
2014-08-07 21:22:57,517 [1] DEBUG Log4ChecksConsole.LoggingClass CoolNumbersInArray=[1,2,4,8,16,32,64];CoolNamesInList=["eliran","moyal"];IntY=13;StringX="X X X";_intXPrivateField=14
            
####with configuration:
 <log4net>
    
    <root>
      <level value="ALL" />
      <appender-ref ref="ObjectsAppender" />
    </root>

    <appender name="ObjectsAppender" type="ObjectsLogAppender.ObjectsAppender, ObjectsLogAppender">
      <Classes value="SomeClass={LastName;ID}*ComplexClass={CoolNumbersInArray;CoolNamesInList;Y>IntY;Y>X>StringX;Y>X>_intXPrivateField}" />
      <MemberNameAndValueSeperator value ="=" />
      <SeperatorBetweenMembers value =";" />
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

  
  
  
