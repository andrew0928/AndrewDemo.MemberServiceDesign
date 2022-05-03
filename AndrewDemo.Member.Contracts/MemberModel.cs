using System;
using System.Collections.Generic;
using System.Text;

namespace AndrewDemo.Member.Contracts
{
    public class MemberModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public string PasswordHash { get; set; }

        public MemberState State { get; set; }



        public int FailedLoginAttemptsCount { get; set; }

        public string ValidateNumber { get; set; }


        public MemberModel Clone()
        {
            return this.MemberwiseClone() as MemberModel;
        }
    }

}
