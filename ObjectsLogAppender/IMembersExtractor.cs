using System.Collections.Generic;
using System.ComponentModel;

namespace ObjectsLogAppender
{
    public interface IMembersExtractor
    {

        char MembersChainIndicator { get; set; }

        void AddClassMapping(string className, List<string> members);
        bool RemoveClassMapping(string className);
        void RemoveAllClassesMapping();
        bool GetClassMapping(string className,out List<string> membersList);

        /// <summary>
        /// Extract member value from an object
        /// </summary>
        /// <param name="extractFrom">the object to extract the member from</param>
        /// <param name="memberName">the member name to be extracted </param>
        /// <param name="memberValue">the member value extracted - null as default</param>
        /// <param name="realMemberName">if simple the memberName , if chain the last memberName</param>
        /// <returns>if extraction was successfull</returns>
        bool ExtractMemberValue(object extractFrom, string memberName, out object memberValue,out string realMemberName);
    }
}