using System;
using System.Collections.Generic;
using System.Threading;

namespace AndrewDemo.MemberServiceDesign
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var ms = new MemberService();

            ms.Register();
            ms.Register();
        }
    }




    public class MemberService
    {
        public MemberStateEnum State { get; private set; } = MemberStateEnum.START;

        // event(s)
        public delegate void MemberServiceEventHandler(object sender, EventArgs e);

        public event MemberServiceEventHandler OnMemberCreated;
        public event MemberServiceEventHandler OnMemberActivated;
        public event MemberServiceEventHandler OnMemberDeactivated;
        public event MemberServiceEventHandler OnMemberArchived;

        public event MemberServiceEventHandler OnMemberRegisterCompleted;  // hook


        public bool Register()
        {
            if (this.TryExecute("Register") == false) return false;
            try
            {
                // TODO: do domain action here.

                this.CompleteExecute();
                this.OnMemberCreated?.Invoke(this, null);
                this.OnMemberRegisterCompleted?.Invoke(this, null);
                return true;
            }
            catch
            {
                this.CancelExecute();
                throw;
            }
        }

        public bool Import()
        {
            if (this.TryExecute("Import") == false) return false;
            try
            {
                // TODO: do domain action here.

                this.CompleteExecute();
                this.OnMemberCreated?.Invoke(this, null);
                return true;
            }
            catch
            {
                this.CancelExecute();
                throw;
            }
        }

        public bool Activate()
        {
            throw new NotImplementedException();
        }

        public bool Lock()
        {
            throw new NotImplementedException();
        }

        public bool Remove()
        {
            throw new NotImplementedException();
        }












        public bool ValidateEmail()
        {
            throw new NotImplementedException();
        }

        private int _check_password_fail_count = 0;
        public bool CheckPassword(string username, string password)
        {
            bool result = false;

            // TODO: do domain action here.


            if (result == true)
            {
                this._check_password_fail_count = 0;
            }
            else
            {
                this._check_password_fail_count++;
            }

            if (this._check_password_fail_count >= 3) this.CheckPasswordFailOverLimit();
            return result;
        }

        private void CheckPasswordFailOverLimit()
        {
            throw new NotImplementedException();
        }


        public bool ResetPassword()
        {
            throw new NotImplementedException();

        }





        private Mutex _state_mutex = new Mutex();
        private MemberStateEnum _state_from = MemberStateEnum.UNDEFINED;
        private MemberStateEnum _state_to = MemberStateEnum.UNDEFINED;
        private MemberStateMachine _state_machine = new MemberStateMachine();

        private bool TryExecute(string actionName)
        {
            var check = this._state_machine.TryExecute(this.State, actionName);
            if (check.result == false)
            {
                Console.WriteLine($"WARNING:: Can NOT execute action({actionName}) in current state({this.State}).");
                return false;
            }
            if (this._state_mutex.WaitOne(0) == false) return false;

            // enter mutex
            this._state_from = this.State;
            this._state_to = check.finalState;
            return true;
        }

        private bool CompleteExecute()
        {
            if (this.State != this._state_from) return false;   // optimistic lock

            this.State = this._state_to;

            this._state_from = this._state_to = MemberStateEnum.UNDEFINED;
            this._state_mutex.ReleaseMutex();
            return true;
        }

        private bool CancelExecute()
        {
            this._state_from = this._state_to = MemberStateEnum.UNDEFINED;
            this._state_mutex.ReleaseMutex();
            return true;
        }
    }

    public enum MemberStateEnum : int
    {
        START = 1000,
        END = 1001,

        CREATED = 1002,
        ACTIVATED = 1003,
        DEACTIVATED = 1004,
        ARCHIVED = 1005,

        UNDEFINED = 0,
    }


    public abstract class StateMachineBase<TEnum>
    {
        protected Dictionary<(TEnum currentState, string actionName), TEnum> _state_transits = null;

        public virtual (bool result, TEnum finalState) TryExecute(TEnum currentState, string actionName)
        {
            if (this._state_transits.TryGetValue((currentState, actionName), out var result) == false) return (false, default(TEnum));
            return (true, result);
        }
    }

    public class MemberStateMachine : StateMachineBase<MemberStateEnum>
    {
        public MemberStateMachine()
        {
            this._state_transits = new Dictionary<(MemberStateEnum currentState, string actionName), MemberStateEnum>()
            {
                { (MemberStateEnum.START, "Register"), MemberStateEnum.CREATED },
                { (MemberStateEnum.START, "Import"), MemberStateEnum.CREATED },

                { (MemberStateEnum.CREATED, "Activate"), MemberStateEnum.ACTIVATED },

                { (MemberStateEnum.ACTIVATED, "Lock"), MemberStateEnum.DEACTIVATED },
                
                { (MemberStateEnum.DEACTIVATED, "UnLock"), MemberStateEnum.ACTIVATED },

                { (MemberStateEnum.ACTIVATED, "Remove"), MemberStateEnum.ARCHIVED },

                { (MemberStateEnum.ARCHIVED, "Archive"), MemberStateEnum.END }// 腦補
            };
        }
    }
}
