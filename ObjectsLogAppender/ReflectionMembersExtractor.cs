using System;
using System.Collections.Generic;

namespace ObjectsLogAppender
{
    public class ReflectionMembersExtractor : BasicMembersExtractor
    {
        #region ctors

        public ReflectionMembersExtractor() : base()
        {
        }

        public ReflectionMembersExtractor(Dictionary<string, List<string>> typeToMembers, char membersChainIndicator) : base(typeToMembers, membersChainIndicator)
        {
            
        }
        #endregion

        #region abstract impl
        protected override bool GetMemberValue(object extractFrom, Type type, string memberName, out object value)
        {
            if (extractFrom == null)
            {
                value = null;
                return false;
            }
            return extractFrom.GetFieldOrProeprtyValue(type, memberName,out value);
        }
        #endregion
    }
}