using System;
using System.Collections.Generic;
using System.Text;

namespace AndrewDemo.Member.Core
{
    public class MemberServiceActionAttribute : Attribute
    {
        public string ActionName { get; set; }
    }
}
