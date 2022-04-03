using System;
using System.Collections.Generic;
using System.Text;

namespace AndrewDemo.Member.Contracts
{
    public enum MemberState : int
    {
        UNDEFINED = 0,

        START,
        CREATED,
        ACTIVATED,
        DEACTIVED,
        ARCHIVED,
        END
    }
}
