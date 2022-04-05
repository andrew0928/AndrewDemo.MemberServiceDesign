using System;
using System.Collections.Generic;
using System.Text;

namespace AndrewDemo.Member.Contracts
{
    public class MemberServiceEventArgs : EventArgs
    {
        public string EventType { get; set; } // state change, action hook, misc
        //public string EventName { get; set; }
        public MemberState InitState { get; set; }
        public MemberState FinalState { get; set; }
        public string ActionName { get; set; }
        //public string AssoicatedToken { get; set; }
        public MemberModel AssoicatedMember { get; set; }
    }
}
