using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectsLogAppender
{
    public abstract class BasicMembersExtractor : IMembersExtractor
    {
        #region members
        private Dictionary<string, List<string>> _typeToMembers;
        #endregion

        #region properties
        public char MembersChainIndicator { get; set; }  
        #endregion

        #region ctors
        protected BasicMembersExtractor()
        {
            _typeToMembers = new Dictionary<string, List<string>>();
        }

        protected BasicMembersExtractor(Dictionary<string, List<string>> typeToMembers, char membersChainIndicator)
        {
            _typeToMembers = typeToMembers;
            MembersChainIndicator = membersChainIndicator;
        }

        #endregion

        #region IMembersExtractor implementation

        public bool ExtractMemberValue(object extractFrom, string memberName, out object memberValue,out string realMemberName)
        {
            //default
            memberValue = null;
            realMemberName = memberName;
            Type type = extractFrom.GetType();
            string typeName = type.Name;

            List<string> membersList;

            //can't extract from unknown type , or type without members
            if (!_typeToMembers.TryGetValue(typeName, out membersList) || membersList == null || membersList.Count == 0)
            {
                return false;
            }

            if (memberName.Contains(MembersChainIndicator))
            {
                var membersChain = memberName.Split(MembersChainIndicator);
                realMemberName = membersChain.Last();
                return DrillToLastMember(extractFrom, membersChain, out memberValue);
            }
            return GetMemberValue(extractFrom, type, memberName,out  memberValue);

        }

        #region class mapping functions
        public void AddClassMapping(string className, List<string> members)
        {
            _typeToMembers.Add(className, members);
        }

        public bool RemoveClassMapping(string className)
        {
            return _typeToMembers.Remove(className);
        }

        public void RemoveAllClassesMapping()
        {
            _typeToMembers = new Dictionary<string, List<string>>();
        }
        public bool GetClassMapping(string className, out List<string> membersList)
        {
            return _typeToMembers.TryGetValue(className, out membersList);
        }
        
        #endregion

        #endregion

        #region abstract
        protected abstract bool GetMemberValue(object extractFrom,Type type, string memberName, out object value);
        #endregion

        #region private functions
        private bool DrillToLastMember(object extractFrom, string[] membersChain, out object memberValue)
        {
            bool validChain = true;
            Type currentType = extractFrom.GetType();
            object currentObj = extractFrom;
            memberValue = null;

            //move to the last link of the chain 
            for (int i = 0; i < membersChain.Length-1; i++)
            {
                object currentMemberValue;
                string currentMember = membersChain[i];
                bool successfullExtraction = GetMemberValue(currentObj,currentType , currentMember, out currentMemberValue);
                //we want to stop if value is null cause we cant drill more.
                if (!successfullExtraction || currentMemberValue == null)
                {
                    validChain = false;
                    break;
                }
                currentObj = currentMemberValue;
                currentType = currentObj.GetType();

            }
            string lastMemberName = membersChain.Last();
            if (!validChain) 
                return false;


            return GetMemberValue(currentObj, currentType, lastMemberName, out memberValue); ;
        }
        #endregion
    }
}
