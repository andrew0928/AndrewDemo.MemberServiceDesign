using AndrewDemo.Member.Contracts;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace AndrewDemo.Member.Core
{
    public class MemberStateMachine
    {
        // state transit:

        // action metadata ( allow role, allow init state, [final state], fire event )
        //private Dictionary<string, (MemberState? initState, MemberState? finalState, string[] allowIdentityTypes)> _fsmext = new Dictionary<string, (MemberState initState, MemberState finalState, string[] allowIdentityTypes)>();
        private List<(string actionName, MemberState? initState, MemberState? finalState, string[] allowIdentityTypes)> _fsmext = new List<(string actionName, MemberState? initState, MemberState? finalState, string[] allowIdentityTypes)>();

        public MemberStateMachine()
        {
            this._fsmext.Add(("activate", MemberState.CREATED, MemberState.ACTIVATED, new string[] { "USER" }));
            this._fsmext.Add(("lock", MemberState.ACTIVATED, MemberState.DEACTIVED, new string[] { "USER", "STAFF" }));
            this._fsmext.Add(("unlock", MemberState.DEACTIVED, MemberState.ACTIVATED, new string[] { "USER", "STAFF" }));
            this._fsmext.Add(("soft-delete", MemberState.ACTIVATED, MemberState.ARCHIVED, new string[] { "USER", "STAFF" }));
            this._fsmext.Add(("soft-delete", MemberState.DEACTIVED, MemberState.ARCHIVED, new string[] { "STAFF" }));
            this._fsmext.Add(("delete", MemberState.START, MemberState.END, new string[] { "STAFF" }));
                            
            this._fsmext.Add(("generate-validate-number", MemberState.CREATED, null, new string[] { "USER", "STAFF" }));
            this._fsmext.Add(("generate-validate-number", MemberState.START, null, new string[] { "USER", "STAFF" }));
            this._fsmext.Add(("generate-validate-number", MemberState.DEACTIVED, null, new string[] { "STAFF" }));
            this._fsmext.Add(("confirm-validate-number", MemberState.CREATED, null, new string[] { "USER" }));
            this._fsmext.Add(("confirm-validate-number", MemberState.ACTIVATED, null, new string[] { "USER" }));
            this._fsmext.Add(("confirm-validate-number", MemberState.DEACTIVED, null, new string[] { "USER" }));
            this._fsmext.Add(("reset-password-with-old-password", MemberState.ACTIVATED, null, new string[] { "USER" }));
            this._fsmext.Add(("reset-password-with-validate-number", MemberState.ARCHIVED, null, new string[] { "USER" }));
            this._fsmext.Add(("reset-password-with-validate-number", MemberState.DEACTIVED, null, new string[] { "USER" }));
            this._fsmext.Add(("force-reset-password", MemberState.ACTIVATED, null, new string[] { "STAFF" }));
            this._fsmext.Add(("force-reset-password", MemberState.DEACTIVED, null, new string[] { "STAFF" }));
                            
            this._fsmext.Add(("register", null, null, new string[] { "USER" }));
            this._fsmext.Add(("import", null, null, new string[] { "STAFF" }));
            this._fsmext.Add(("get-members", null, null, new string[] { "STAFF" }));
            this._fsmext.Add(("check-password", null, null, new string[] { "USER" }));
            this._fsmext.Add(("get-member", null, null, new string[] { "USER", "STAFF" }));
        }


        public virtual (bool result, MemberState? initState, MemberState? finalState) CanExecute(MemberState currentState, string actionName, string identityType)
        {
            foreach(var x in (from r in this._fsmext where r.actionName == actionName && (r.initState == null || r.initState == currentState) && r.allowIdentityTypes.Contains(identityType) select r))
            {
                return (true, currentState, x.finalState);
            }

            return (false, null, null);
        }

        public virtual bool CanExecute(string actionName, string identityType)
        {
            foreach (var x in (from r in this._fsmext where r.actionName == actionName && r.initState == null && r.finalState == null && r.allowIdentityTypes.Contains(identityType) select r))
            {
                return (true);
            }

            return false;
        }
    }
}
