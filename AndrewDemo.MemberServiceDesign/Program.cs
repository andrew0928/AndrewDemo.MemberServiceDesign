using System;
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

        public event MemberServiceEventHandler OnMemberRegistered;
        public event MemberServiceEventHandler OnMemberEmailVerified;
        public event MemberServiceEventHandler OnMemberLocked;
        public event MemberServiceEventHandler OnMemberArchived;
        public event MemberServiceEventHandler OnMemberActivated;



        public bool Register()
        {
            if (this.TryExecute("Register") == false) return false;
            try
            {
                // TODO: do domain action here.

                this.CompleteExecute();
                this.OnMemberRegistered?.Invoke(this, null);
                return true;
            }
            catch
            {
                this.CancelExecute();
                throw;
            }
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

        public bool Remove()
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
            if (check.result == false) return false;
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
        START = 0,
        END = 1,

        REGISTERED = 2,
        VERIFIED = 3,
        LOCKING = 4,
        ARCHIVED = 5,

        UNDEFINED = -1,
    }


    public abstract class StateMachineBase<TEnum>
    {
        public abstract (bool result, TEnum finalState) TryExecute(TEnum currentState, string actionName);
    }

    public class MemberStateMachine //: StateMachineBase<MemberStateEnum>
    {
        
        public MemberStateMachine()
        {

        }

        public (bool result, MemberStateEnum finalState) TryExecute(MemberStateEnum currentState, string actionName)
        {
            return (true, MemberStateEnum.UNDEFINED);
        }
    }
}
