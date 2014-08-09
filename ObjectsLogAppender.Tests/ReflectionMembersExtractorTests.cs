using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace ObjectsLogAppender.Tests
{
    [TestFixture]
    public class ReflectionMembersExtractorTests : ReflectionMembersExtractor
    {

        [TestFixtureTearDown]
        public void TearDownPerTest()
        {
            RemoveAllClassesMapping();
        }

        [Test]
        public void GetMemberValue_BasicPublicProperty_ReturnPublicPropertyValue()
        {
            AddClassMapping("TestClassWithOnePublicProperty",new List<string>(){"IntProperty"});
            const int propertyValue = 2;
            var testClass = new TestClassWithOnePublicProperty()
            {
                IntProperty = propertyValue
            };
            object memberValue;
            bool successfulExtraction = GetMemberValue(testClass, testClass.GetType(), "IntProperty", out memberValue);

            Assert.IsTrue(successfulExtraction);

            Assert.AreEqual(memberValue, propertyValue);
        }

        [Test]
        public void GetMemberValue_BasicProtectedProperty_ReturnProtectedPropertyValue()
        {
            AddClassMapping("TestClassWithOneProtectedProperty", new List<string>() { "IntProperty" });
            const int propertyValue = 2;
            var testClass = new TestClassWithOneProtectedProperty();
            testClass.SetProtectedPropertyValue(propertyValue);
            object memberValue;
            bool successfulExtraction = GetMemberValue(testClass, testClass.GetType(), "IntProperty", out memberValue);

            Assert.IsTrue(successfulExtraction);

            Assert.AreEqual(memberValue, propertyValue);
        }

        [Test]
        public void GetMemberValue_BasicPrivateProperty_ReturnPrivatePropertyValue()
        {
            AddClassMapping("TestClassWithOnePrivateProperty", new List<string>() { "IntProperty" });
            const int propertyValue = 2;
            var testClass = new TestClassWithOnePrivateProperty();
            testClass.SetPrivatePropertyValue(propertyValue);
            object memberValue;
            bool successfulExtraction = GetMemberValue(testClass, testClass.GetType(), "IntProperty", out memberValue);

            Assert.IsTrue(successfulExtraction);

            Assert.AreEqual(memberValue, propertyValue);
        }


        [Test]
        public void GetMemberValue_BasicPublicField_ReturnPublicFieldValue()
        {
            AddClassMapping("TestCalssWithOnePublicField", new List<string>() { "IntField" });
            const int fieldValue = 2;
            var testClass = new TestCalssWithOnePublicField();
            testClass.IntField = fieldValue;
            object memberValue;
            bool successfulExtraction = GetMemberValue(testClass, testClass.GetType(), "IntField", out memberValue);

            Assert.IsTrue(successfulExtraction);

            Assert.AreEqual(memberValue, fieldValue);
        }

        [Test]
        public void GetMemberValue_BasicPrivateField_ReturnPrivateFieldValue()
        {
            AddClassMapping("TestCalssWithOnePrivateField", new List<string>() { "_intField" });
            const int fieldValue = 2;
            var testClass = new TestCalssWithOnePrivateField();
            testClass.SetPrivateFieldValue(fieldValue);
            object memberValue;
            bool successfulExtraction = GetMemberValue(testClass, testClass.GetType(), "_intField", out memberValue);

            Assert.IsTrue(successfulExtraction);

            Assert.AreEqual(memberValue, fieldValue);
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
    #endregion
}
