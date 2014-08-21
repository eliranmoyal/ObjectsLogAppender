using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace ObjectsLogAppender.Tests
{
    [TestFixture]
    public class ReflectionMembersExtractorTests
    {
        public ReflectionMembersExtractor Extractor { get; set; }

        [SetUp]
        public void Init()
        {
            Extractor = new ReflectionMembersExtractor {MembersChainIndicator = '>'};
        }

        [TestFixtureTearDown]
        public void TearDownPerTest()
        {
            Extractor.RemoveAllClassesMapping();
        }

        [Test]
        public void GetMemberValue_BasicPublicProperty_ReturnPublicPropertyValue()
        {
            Extractor.AddClassMapping("TestClassWithOnePublicProperty", new List<string>(){"IntProperty"});
            const int propertyValue = 2;
            var testClass = new TestClassWithOnePublicProperty()
            {
                IntProperty = propertyValue
            };
            object memberValue;
            bool successfulExtraction = Extractor.GetMemberValue(testClass, testClass.GetType(), "IntProperty", out memberValue);

            Assert.IsTrue(successfulExtraction);

            Assert.AreEqual(memberValue, propertyValue);
        }

        [Test]
        public void GetMemberValue_BasicProtectedProperty_ReturnProtectedPropertyValue()
        {
            Extractor.AddClassMapping("TestClassWithOneProtectedProperty", new List<string>() { "IntProperty" });
            const int propertyValue = 2;
            var testClass = new TestClassWithOneProtectedProperty();
            testClass.SetProtectedPropertyValue(propertyValue);
            object memberValue;
            bool successfulExtraction = Extractor.GetMemberValue(testClass, testClass.GetType(), "IntProperty", out memberValue);

            Assert.IsTrue(successfulExtraction);

            Assert.AreEqual(memberValue, propertyValue);
        }

        [Test]
        public void GetMemberValue_BasicPrivateProperty_ReturnPrivatePropertyValue()
        {
            Extractor.AddClassMapping("TestClassWithOnePrivateProperty", new List<string>() { "IntProperty" });
            const int propertyValue = 2;
            var testClass = new TestClassWithOnePrivateProperty();
            testClass.SetPrivatePropertyValue(propertyValue);
            object memberValue;
            bool successfulExtraction = Extractor.GetMemberValue(testClass, testClass.GetType(), "IntProperty", out memberValue);

            Assert.IsTrue(successfulExtraction);

            Assert.AreEqual(memberValue, propertyValue);
        }


        [Test]
        public void GetMemberValue_BasicPublicField_ReturnPublicFieldValue()
        {
            Extractor.AddClassMapping("TestCalssWithOnePublicField", new List<string>() { "IntField" });
            const int fieldValue = 2;
            var testClass = new TestCalssWithOnePublicField();
            testClass.IntField = fieldValue;
            object memberValue;
            bool successfulExtraction = Extractor.GetMemberValue(testClass, testClass.GetType(), "IntField", out memberValue);

            Assert.IsTrue(successfulExtraction);

            Assert.AreEqual(memberValue, fieldValue);
        }

        [Test]
        public void GetMemberValue_BasicPrivateField_ReturnPrivateFieldValue()
        {
            Extractor.AddClassMapping("TestCalssWithOnePrivateField", new List<string>() { "_intField" });
            const int fieldValue = 2;
            var testClass = new TestCalssWithOnePrivateField();
            testClass.SetPrivateFieldValue(fieldValue);
            object memberValue;
            bool successfulExtraction = Extractor.GetMemberValue(testClass, testClass.GetType(), "_intField", out memberValue);

            Assert.IsTrue(successfulExtraction);

            Assert.AreEqual(memberValue, fieldValue);
        }


        [Test]
        public void GetMemberValue_NullTest_ReturnFalseAndNull()
        {
            object memberValue;
            bool successfulExtraction = Extractor.GetMemberValue(null, null, "intField", out memberValue);
            Assert.IsFalse(successfulExtraction);
            Assert.IsNull(memberValue);

        }

        [Test]
        public void ExtractMemberValue_OneLevelNestedClass_ReturnPropertyInsideClass()
        {
            Extractor.AddClassMapping("OneLevelNestedClass", new List<string>() { "InnerClass>IntProperty" });
            const int innerPropertyValue = 2;
            var testClass = new OneLevelNestedClass()
            {
                InnerClass = new TestClassWithOnePublicProperty()
                {
                    IntProperty = innerPropertyValue
                }
            };
          
            object memberValue;
            string realName;
            bool successfulExtraction = Extractor.ExtractMemberValue(testClass, "InnerClass>IntProperty", out memberValue, out realName);

            Assert.IsTrue(successfulExtraction);

            Assert.AreEqual("IntProperty",realName);
            Assert.AreEqual(memberValue, innerPropertyValue);
        }

        [Test]
        public void ExtractMemberValue_TwoLevelNestedClass_ReturnPropertyInsideClass()
        {
            Extractor.AddClassMapping("TwoLevelNestedClass", new List<string>() { "InnerClass>IntProperty" });
            const int innerPropertyValue = 2;
            var testClass = new TwoLevelNestedClass()
            {
                NestedClass = new OneLevelNestedClass()
                {
                    InnerClass = new TestClassWithOnePublicProperty()
                    {
                        IntProperty = innerPropertyValue
                    }
                }
            };

            object memberValue;
            string realName;
            bool successfulExtraction = Extractor.ExtractMemberValue(testClass, "NestedClass>InnerClass>IntProperty", out memberValue, out realName);

            Assert.IsTrue(successfulExtraction);

            Assert.AreEqual("IntProperty", realName);
            Assert.AreEqual(memberValue, innerPropertyValue);
        }
    
    }


    #region test classes
    internal class TestClassWithOnePublicProperty
    {
        public int IntProperty { get; set; }
    }

    internal class TestClassWithOneProtectedProperty
    {
        protected int IntProperty { get; set; }

        public void SetProtectedPropertyValue(int value)
        {
            IntProperty = value;
        }
    }

    internal class TestClassWithOnePrivateProperty
    {
        private int IntProperty { get; set; }

        public void SetPrivatePropertyValue(int value)
        {
            IntProperty = value;
        }
    }

    internal class TestCalssWithOnePublicField
    {
        public int IntField;
    }

    internal class TestCalssWithOnePrivateField
    {
        private int _intField;
        public void SetPrivateFieldValue(int value)
        {
            _intField = value;
        }
    }

    internal class OneLevelNestedClass
    {
        public TestClassWithOnePublicProperty InnerClass { get; set; }
    }
    internal class TwoLevelNestedClass
    {
        public OneLevelNestedClass NestedClass { get; set; }
    }
    #endregion
}
