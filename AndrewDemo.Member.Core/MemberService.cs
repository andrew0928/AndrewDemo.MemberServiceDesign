using AndrewDemo.Member.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace AndrewDemo.Member.Core
{
    public class MemberService
    {
        private readonly MemberServiceToken _token;
        private readonly MemberStateMachine _fsm;
        private readonly MemberRepo _repo;

        public event EventHandler<MemberServiceEventArgs> OnStateChanged;
        //public event EventHandler<MemberServiceEventArgs> OnActionExecCompleted;

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

            // for degug only
            this.OnStateChanged += MemberService_OnStateChanged;
        }

        private void MemberService_OnStateChanged(object sender, MemberServiceEventArgs e)
        {
            Console.WriteLine($"* OnStateChanged Event: Member({e.AssoicatedMember.Id}) state({e.InitState} => {e.FinalState}) via action({e.ActionName}).");
        }




        #region major API(s), 執行後狀態會因而改變


        private bool SafeChangeState(int id, string actionName, Func<MemberModel, bool> func)
        {
            if (this._repo._members.ContainsKey(id) == false) throw new MemberServiceException($"MS: id({id}) not exist."); //return false;

            MemberState initState;
            MemberState finalState;
            lock (this._repo._members_syncroot[id])
            {
                var check = this._fsm.CanExecute(
                    this._repo._members[id].State,
                    actionName,
                    this._token.IdentityType);
                if (check.result == false) return false;

                var model = this._repo._members[id].Clone();
                initState = model.State;

                if (func(model) == false)
                {
                    Console.WriteLine($"* SafeChangeState Fail: func() return false. model was not updated.");
                    //return false;
                    throw new MemberServiceException($"* SafeChangeState Fail: func() return false. model was not updated.");
                }

                if (model.State != check.finalState)
                {
                    Console.WriteLine($"* SafeChangeState Fail: state change was not match FSM. model was not updated.");
                    //return false; //throw new InvalidOperationException("state change was not allowed by FSM.");
                    throw new MemberServiceException($"* SafeChangeState Fail: state change was not match FSM. model was not updated.");
                }

                this._repo._members[id] = model.Clone();
                finalState = model.State;
            }

            // fire state change event
            if (initState != finalState)
            {
                this.OnStateChanged?.Invoke(this, new MemberServiceEventArgs()
                {
                    EventType = "StateChange",
                    ActionName = actionName,
                    InitState = initState,
                    FinalState = finalState,
                    AssoicatedMember = this._repo._members[id].Clone()
                });
            }

            return true;
        }


        public bool Activate(int id, string validateNumber)
        {
            bool result = this.SafeChangeState(id, "activate", (m) =>
            {
                if (m.ValidateNumber == null || m.ValidateNumber != validateNumber) return false;

                m.State = MemberState.ACTIVATED;
                m.ValidateNumber = null;
                return true;
            });
            if (result == false) return false;
            return true;
        }

        public bool Lock(int id, string reason)
        {
            bool result = this.SafeChangeState(id, "lock", (m) =>
            {
                m.State = MemberState.DEACTIVED;
                return true;
            });
            if (result == false) return false;
            return true;
        }


        public bool UnLock(int id, string reason)
        {
            bool result = this.SafeChangeState(id, "unlock", (m) =>
            {
                m.State = MemberState.DEACTIVED;
                return true;
            });
            if (result == false) return false;
            return true;
        }

        public bool SoftDelete(int id, string reason)
        {
            bool result = this.SafeChangeState(id, "soft-delete", (m) =>
            {
                m.State = MemberState.ARCHIVED;
                return true;
            });
            if (result == false) return false;
            return true;
        }

        public bool Delete(int id, string reason)
        {
            bool result = this.SafeChangeState(id, "activate", (m) =>
            {
                m.State = MemberState.END;
                return true;
            });
            if (result == false) return false;

            this._repo._members.Remove(id);
            this._repo._members_syncroot.Remove(id);

            return true;
        }
        #endregion


        #region domain / aggraton API(s), 會因為狀態決定能否執行，不會直接改變狀態 (除非內部呼叫了 major APIs)
        public string GenerateValidateNumber(int id)
        {
            if (FSMRuleCheck(id, "generate-validate-number") == false) return null;

            Random rnd = new Random();
            string number = rnd.Next(10000000, 99999999).ToString();

            lock (this._repo._members_syncroot[id])
            {
                var m = this._repo._members[id];
                m.ValidateNumber = number;
            }

            return number;
        }


        public bool CheckPassword(int id, string password)
        {
            if (FSMRuleCheck(id, "check-password", false) == false) return false;

            var m = this._repo._members[id];
            bool check_result = this.ComparePassword(password, m.PasswordHash);

            if (check_result)
            {
                m.FailedLoginAttemptsCount = 0;
            }
            else
            {
                m.FailedLoginAttemptsCount++;
                if (m.FailedLoginAttemptsCount >= 3) this.Lock(id, "wrong password over 3 times.");
            }

            return check_result;
        }

        public bool ResetPasswordWithCheckOldPassword(int id, string newPassword, string oldPassword)
        {
            if (FSMRuleCheck(id, "reset-password-with-old-password") == false) return false;

            lock (this._repo._members_syncroot[id])
            {
                var m = this._repo._members[id];
                if (this.ComparePassword(oldPassword, m.PasswordHash) == false) return false;

                m.PasswordHash = Convert.ToBase64String(this.ComputePasswordHash(newPassword));
                m.FailedLoginAttemptsCount = 0;
                m.ValidateNumber = null;
            }
            return true;
        }
        public bool ResetPasswordWithValidateNumber(int id, string newPassword, string validateNumber)
        {
            //if (FSMRuleCheck(id, "reset-password-with-validate-number")== false) return false;

            //lock (this._repo._members_syncroot[id])
            //{
            //    var m = this._repo._members[id];
            //    if (string.IsNullOrEmpty(m.ValidateNumber) || m.ValidateNumber != validateNumber) return false;

            //    m.PasswordHash = Convert.ToBase64String(this.ComputePasswordHash(newPassword));
            //    m.FailedLoginAttemptsCount = 0;
            //    m.ValidateNumber = null;
            //}

            bool result = this.SafeChangeState(id, "reset-password-with-validate-number", m =>
            {
                if (string.IsNullOrEmpty(m.ValidateNumber) || m.ValidateNumber != validateNumber) return false;

                m.PasswordHash = Convert.ToBase64String(this.ComputePasswordHash(newPassword));
                m.FailedLoginAttemptsCount = 0;
                m.ValidateNumber = null;
                m.State = MemberState.ACTIVATED;

                return true;
            });
            if (result == false) return false;

            return true;
        }
        public bool ForceResetPassword(int id, string newPassword)
        {
            if (FSMRuleCheck(id, "force-reset-password") == false) return false;

            lock (this._repo._members_syncroot[id])
            {
                var m = this._repo._members[id];
                m.PasswordHash = Convert.ToBase64String(this.ComputePasswordHash(newPassword));
                m.FailedLoginAttemptsCount = 0;
                m.ValidateNumber = null;
            }
            return true;
        }





        public MemberModel Register(string name, string password, string email)
        {
            if (FSMRuleCheck(null, "register") == false) return null;

            object syncroot = new object();
            MemberModel member = new MemberModel()
            {
                Id = this._repo.GetNewID(),
                State = MemberState.START
            };

            lock (this._repo)
            {
                var m = (from x in this._repo._members.Values where x.Name == name select x).FirstOrDefault();
                if (m != null) throw new MemberServiceException($"Member (name: {name}) existed.");

                this._repo._members.Add(member.Id, member);
                this._repo._members_syncroot.Add(member.Id, syncroot);
            }

            Random rnd = new Random();
            string number = rnd.Next(10000000, 99999999).ToString();

            bool result = this.SafeChangeState(member.Id, "register", (m) =>
            {
                m.Name = name;
                m.PasswordHash = Convert.ToBase64String(this.ComputePasswordHash(password));
                m.Email = email;
                m.State = MemberState.CREATED;
                m.ValidateNumber = number;

                return true;
            });
            if (result == false) return null;

            //this.GenerateValidateNumber(member.Id);

            //return this.GetMember(member.Id);
            return this._repo._members[member.Id].Clone();
        }

        public MemberModel Import(MemberModel member)
        {
            if (FSMRuleCheck(null, "import")==false) return null;

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

            return member.Clone();
        }

        public MemberModel GetMember(int id)
        {
            if (FSMRuleCheck(id, "get-member") == false) return null;

            return (this._repo._members[id].Clone());
        }

        public MemberModel GetMemberByName(string name)
        {
            var m = (from x in this._repo._members.Values where x.Name == name select x).FirstOrDefault();
            if (m == null) return null;
            return this.GetMember(m.Id);
        }

        public MemberModel GetMemberByEmail(string email)
        {
            var m = (from x in this._repo._members.Values where x.Email == email select x).FirstOrDefault();
            return this.GetMember(m.Id);
        }

        public IQueryable<MemberModel> GetMembers()
        {
            if (FSMRuleCheck(null, "get-members") == false) return null;

            //Required_IdentityType("STAFF");

            return this._repo._members.Values.Select((m) => { return m.Clone(); }).AsQueryable();
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

        internal bool FSMRuleCheck(int? id, string actionName, bool throw_exception = true)
        {
            if (id != null && this._repo._members.ContainsKey(id.Value) == false)
            {
                Console.WriteLine($"* FSM: Member(id: {id}) not exist.");
                if (throw_exception)
                {
                    throw new MemberServiceException($"* FSM: Member(id: {id}) not exist.");
                }
                return false;
            }

            if (id != null)
            {
                if (this._fsm.CanExecute(
                    this._repo._members[id.Value].State,
                    actionName,
                    this._token.IdentityType).result == false)
                {
                    Console.WriteLine($"* FSM: FSM rule check fail.");
                    if (throw_exception)
                    {
                        throw new MemberServiceException($"* FSM: FSM rule check fail.");
                    }
                    return false;
                }
            }
            else
            {
                if (this._fsm.CanExecute(actionName, this._token.IdentityType) == false)
                {
                    Console.WriteLine($"* FSM: FSM rule check fail.");
                    if (throw_exception)
                    {
                        throw new MemberServiceException($"* FSM: FSM rule check fail.");
                    }
                    return false;
                }
            }

            return true;
        }

        private byte[] ComputePasswordHash(string password)
        {
            return HashAlgorithm.Create("MD5").ComputeHash(Encoding.Unicode.GetBytes(password));
        }

        private bool ComparePassword(string password, string passwordHash)
        {
            byte[] hash1 = this.ComputePasswordHash(password);
            byte[] hash2 = Convert.FromBase64String(passwordHash);

            if (hash1.Length != hash2.Length) return false;
            for (int i = 0; i < hash1.Length; i++)
            {
                if (hash1[i] != hash2[i]) return false;
            }

            return true;
        }
    }

    public class MemberServiceException : Exception
    {
        public MemberServiceException(string message) : base(message)
        {
        }
    }
}
