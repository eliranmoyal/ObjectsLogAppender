using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ObjectsLogAppender
{
    public static class Extensions
    {
        public static bool GetFieldOrProeprtyValue(this object obj,Type type ,string memberName, out object value)
        {
            //default value
            value = null;
            //input validation
            if (type == null || string.IsNullOrEmpty(memberName) || obj == null) 
                return false;
            //include private and public members
            MemberInfo memberInfo = type.GetMember(memberName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).FirstOrDefault();
            //no such member
            if (memberInfo == null)
                return false;
            //supports property or field
            if (memberInfo.MemberType == MemberTypes.Property)
            {
                value = ((PropertyInfo)memberInfo).GetValue(obj);
            }
            if (memberInfo.MemberType == MemberTypes.Field)
            {
                value = ((FieldInfo)memberInfo).GetValue(obj);
            }
            return true;
        }

    }
}
