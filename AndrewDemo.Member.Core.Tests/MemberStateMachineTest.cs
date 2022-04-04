using AndrewDemo.Member.Contracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace AndrewDemo.Member.Core.Tests
{
    [TestClass]
    public class MemberStateMachineTest
    {
        private MemberStateMachine _fsm = null;

        [TestInitialize]
        public void Init()
        {
            this._fsm = new MemberStateMachine();
        }


        [TestMethod]
        public void BasicTest_Scenario1()
        {
            // user 只有在 activated 狀態下可以通過密碼驗證
            Assert.IsTrue(this._fsm.CanExecute(MemberState.ACTIVATED, "check-password", "USER").result);
            Assert.IsFalse(this._fsm.CanExecute(MemberState.DEACTIVED, "check-password", "USER").result);

            // staff 不被允許驗證使用者密碼
            Assert.IsFalse(this._fsm.CanExecute(MemberState.ACTIVATED, "check-password", "STAFF").result);
            Assert.IsFalse(this._fsm.CanExecute(MemberState.DEACTIVED, "check-password", "STAFF").result);
        }
    }
}
