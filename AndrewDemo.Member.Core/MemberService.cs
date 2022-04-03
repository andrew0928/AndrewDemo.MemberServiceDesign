using AndrewDemo.Member.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewDemo.Member.Core
{
    public class MemberService
    {
        private MemberServiceToken _token;
        private MemberStateMachine _fsm;
        private MemberRepo _repo;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="token">(scoped)</param>
        /// <param name="fsm">(singleton)</param>
        /// <param name="repo">(singleton)</param>
        public MemberService(MemberServiceToken token, MemberStateMachine fsm, MemberRepo repo)
        {
            this._token = token;
            this._fsm = fsm;
            this._repo = repo;
        }




        #region major API(s), 執行後狀態會因而改變
        public MemberModel Register(string name, string password, string email)
        {
            FSMRuleCheck(null, "register");

            throw new NotImplementedException();
        }

        public MemberModel Import(MemberModel member)
        {
            FSMRuleCheck(null, "import");

            //if (this._repo._members.ContainsKey(member.Id)) throw new InvalidOperationException("Member exists.");

            object syncroot = new object();
            int newid = this._repo.GetNewID();

            lock (this._repo)
            {
                //if (this._repo._members.ContainsKey(member.Id)) throw new InvalidOperationException("Member exists.");
                member.Id = newid;
                this._repo._members.Add(member.Id, member);
                this._repo._members_syncroot.Add(member.Id, syncroot);
            }

            return member;
        }

        public bool Activate(int id, string reason)
        {
            FSMRuleCheck(id, "activate");

            throw new NotImplementedException();
        }

        public bool Lock(int id, string reason)
        {
            FSMRuleCheck(id, "lock");

            throw new NotImplementedException();
        }


        public bool UnLock(int id, string reason)
        {
            FSMRuleCheck(id, "unlock");

            throw new NotImplementedException();
        }

        public bool SoftDelete(int id, string reason)
        {
            FSMRuleCheck(id, "soft-delete");

            throw new NotImplementedException();
        }

        public bool Delete(int id, string reason)
        {
            FSMRuleCheck(id, "delete");


            throw new NotImplementedException();
        }
        #endregion


        #region domain / aggraton API(s), 會因為狀態決定能否執行，不會直接改變狀態 (除非內部呼叫了 major APIs)
        public string GenerateValidateNumber(int id)
        {
            FSMRuleCheck(id, "generate-validate-number");

            throw new NotImplementedException();
        }

        public bool ConfirmValidateNumber(int id, string validateNumber)
        {
            FSMRuleCheck(id, "confirm-validate-number");

            throw new NotImplementedException();
        }

        public bool CheckPassword(string name, string password)
        {
            var x = (from m in this._repo._members.Values where m.Name == name select m).FirstOrDefault();
            if (x == null) return false;

            FSMRuleCheck(x.Id, "check-password");


            throw new NotImplementedException();
        }

        public MemberModel GetMember(int id)
        {
            FSMRuleCheck(id, "get-member");

            
            return (this._repo._members[id]);
        }

        public IQueryable<MemberModel> GetMembers()
        {
            FSMRuleCheck(null, "get-members");
            //Required_IdentityType("STAFF");

            return this._repo._members.Values.AsQueryable();
        }
        #endregion





        //private void Required_IdentityType(string type)
        //{
        //    if (this._token.IdentityType == type) return;
            
        //    throw new InvalidOperationException($"Only {type} can do this.");
        //}

        //private void Required_CurrentState(int id, MemberState state)
        //{
        //    if (this._repo._members.ContainsKey(id) && this._repo._members[id].State == state) return;

        //    throw new InvalidOperationException($"Member not exist or its current STATE is not {state}.");
        //}

        private void FSMRuleCheck(int? id, string actionName)
        {
            if (id != null && this._repo._members.ContainsKey(id.Value) == false) throw new InvalidOperationException($"Member(id: {id}) not exist.");

            if (id != null)
            {
                if (this._fsm.CanExecute(
                    this._repo._members[id.Value].State,
                    actionName,
                    this._token.IdentityType).result == false) throw new InvalidOperationException($"");
            }
            else
            {
                if (this._fsm.CanExecute(actionName, this._token.IdentityType) == false) throw new InvalidOperationException($"");
            }

            return;
        }
    }
}
