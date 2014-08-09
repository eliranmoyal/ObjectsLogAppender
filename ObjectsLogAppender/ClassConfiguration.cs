using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectsLogAppender
{
    public class ClassConfiguration
    {
        #region props
        public string Name { get; set; }
        public List<string> MembersList { get { return _members; } }
        #endregion

        #region private members
        private List<string> _members { get; set; }
        #endregion

        #region ctor
        public ClassConfiguration()
        {
            _members = new List<string>();
        }
        #endregion

        #region configuration handling
        public void AddMember(string member)
        {
            _members.Add(member);
        }

        public void RemoveMember(string member)
        {
            _members.Remove(member);
        }
        #endregion
    }
}
