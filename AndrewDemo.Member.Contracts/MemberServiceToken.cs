using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

[assembly: InternalsVisibleTo("AndrewDemo.Member.Core")]
namespace AndrewDemo.Member.Contracts
{
    
    public class MemberServiceToken
    {
        public bool IsInitialized { get; internal set; }

        // JWT claim: iss, issuer
        // type, USER | STAFF
        public string IdentityType { get; internal set; }

        // JWT claim: sub, subject
        // who, iosapp | androidapp | webui | {staff}@chicken-house.net
        public string IdentityName { get; internal set; }

        // JWT claim: jti, JWT ID
        public string ID { get; internal set; }

        // JWT claim: iat, issue at
        public DateTime CreateTime { get; internal set; }

        // JWT claim: exp, expiration
        public DateTime ExpireTime { get; internal set; }
    }

}
