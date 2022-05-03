using AndrewDemo.Member.Contracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AndrewDemo.Member.Core.Tests
{
    [TestClass]
    public class MemberServiceTest
    {
        private MemberRepo _repo = null;
        private MemberStateMachine _fsm = null;

        [TestInitialize]
        public void Init()
        {
            this._repo = new MemberRepo();
            this._fsm = new MemberStateMachine();

            // token, staff | andrew | 2022/04/04 ~ +3 years
            //MemberServiceToken token = MemberServiceToken.BuildToken("eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJTVEFGRiIsInN1YiI6ImFuZHJldyIsImp0aSI6IkFERTQzOUM0MjQyQjQwNEQ4NDAyRjQ0MjVEMDJDMkVGIiwiaWF0IjoxNjQ4OTk1MzY2Ljg3OTM3NiwiZXhwIjoxNzQzNjg5NzY2Ljg3OTM3ODZ9.BJbVQE2gHEpu39cz-9PQix8bHn5-GFBOriP80bi6fpo18T2nG636EeApFNd9sgcTAyf-9vYFEetUACALSU27qA");

            // token, user | webui | 2022/04/04 ~ +3 years
            MemberServiceToken token = MemberServiceTokenHelper.BuildToken("eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJVU0VSIiwic3ViIjoiV2ViVUkiLCJqdGkiOiJFNTMyM0FBNTU4MjY0OUQ3QUJDOUZFODQxMjkwMDFDMiIsImlhdCI6MTY0ODk5Njc0MS42MDQ1MTY1LCJleHAiOjE3NDM2OTExNDEuNjA0NzE2fQ.RynDs43NEjMXfnMPbAKqEr2MBqI1oub2X-4xEuve5Q21tMYcZAXPn60fe0wdJLO0uJUAeRTxS0HdBOR70zmAsA");
            MemberService service = new MemberService(token, this._fsm, this._repo);

            MemberModel m = null;

            // andrew
            m = service.Register("andrew", "0000", "andrew@123.net");
            if (m != null) service.Activate(m.Id, m.ValidateNumber);

            // nancy
            m = service.Register("nancy", "0000", "nancy@456.com");
            if (m != null) service.Activate(m.Id, m.ValidateNumber);

            // peter
            m = service.Register("peter", "0000", "peter@789.idv.tw");

            // annie
            m = service.Register("annie", "0000", "annie@012.org");
        }



        [TestMethod]
        public void BasicScenario1_NewMemberLifeCycleTest()
        {
            // �e�x�� token, �� user �ާ@�ϥΪ����v
            MemberServiceToken web_token = MemberServiceTokenHelper.BuildToken("eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJVU0VSIiwic3ViIjoiV2ViVUkiLCJqdGkiOiJFNTMyM0FBNTU4MjY0OUQ3QUJDOUZFODQxMjkwMDFDMiIsImlhdCI6MTY0ODk5Njc0MS42MDQ1MTY1LCJleHAiOjE3NDM2OTExNDEuNjA0NzE2fQ.RynDs43NEjMXfnMPbAKqEr2MBqI1oub2X-4xEuve5Q21tMYcZAXPn60fe0wdJLO0uJUAeRTxS0HdBOR70zmAsA");
            MemberService service_for_web = new MemberService(web_token, this._fsm, this._repo);

            // ��x�� token, �� staff �ާ@�ϥΪ����v
            MemberServiceToken staff_token = MemberServiceTokenHelper.BuildToken("eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJTVEFGRiIsInN1YiI6ImFuZHJldyIsImp0aSI6IkFERTQzOUM0MjQyQjQwNEQ4NDAyRjQ0MjVEMDJDMkVGIiwiaWF0IjoxNjQ4OTk1MzY2Ljg3OTM3NiwiZXhwIjoxNzQzNjg5NzY2Ljg3OTM3ODZ9.BJbVQE2gHEpu39cz-9PQix8bHn5-GFBOriP80bi6fpo18T2nG636EeApFNd9sgcTAyf-9vYFEetUACALSU27qA");
            MemberService service_for_staff = new MemberService(staff_token, this._fsm, this._repo);


            // brian ���U�s�b��, ���o���ҽX
            var m = service_for_web.Register("brian", "1234", "brian@gogo.go");
            Assert.IsNotNull(m);
            Assert.IsFalse(string.IsNullOrEmpty(m.ValidateNumber));
            Assert.AreEqual(m.State, MemberState.CREATED);

            int id = m.Id;
            string number = m.ValidateNumber;
            m = null;


            // �|���q�L���ҡA�L�k�n�J
            Assert.IsFalse(service_for_web.CheckPassword(id, "1234"));
            //Assert.ThrowsException<MemberStateMachineException>(() =>
            //{
            //    service_for_web.CheckPassword(id, "1234");
            //});

            // �q�L���ҡA���s�n�J
            Assert.IsTrue(service_for_web.Activate(id, number));
            Assert.AreEqual(service_for_web.GetMember(id).State, MemberState.ACTIVATED);
            Assert.IsTrue(service_for_web.CheckPassword(id, "1234"));
            number = null;

            // ��J���~�K�X�T���A�b���|�Q��w
            Assert.IsFalse(service_for_web.CheckPassword(id, "5678"));
            Assert.AreEqual(service_for_web.GetMember(id).State, MemberState.ACTIVATED);

            Assert.IsFalse(service_for_web.CheckPassword(id, "5678"));
            Assert.AreEqual(service_for_web.GetMember(id).State, MemberState.ACTIVATED);

            Assert.IsFalse(service_for_web.CheckPassword(id, "5678"));
            Assert.AreEqual(service_for_web.GetMember(id).State, MemberState.DEACTIVED);


            // Brian �p���ȪA�A�J�A�q��x���s�o�e���ҽX
            number = service_for_staff.GenerateValidateNumber(id);
            Assert.IsFalse(string.IsNullOrEmpty(number));

            // ���o�s���ҽX�A���]�K�X
            Assert.IsTrue(service_for_web.ResetPasswordWithValidateNumber(id, "8888", number));
            Assert.IsTrue(service_for_web.CheckPassword(id, "8888"));

            // �@�����`�ABrian �u�O��­n��K�X
            Assert.IsTrue(service_for_web.ResetPasswordWithCheckOldPassword(id, "9527", "8888"));
            Assert.IsTrue(service_for_web.CheckPassword(id, "9527"));

            // �o�ͪ��p�ABrian �Q�ȪA�{�w���H�W�ϥΪ̡A��w�b���A�j��ﱼ�K�X
            Assert.IsTrue(service_for_staff.Lock(id, "tag as bad user"));
            Assert.IsTrue(service_for_staff.ForceResetPassword(id, "0000"));

            // �L�ץηs�±K�X���L�k�n�J
            Assert.IsFalse(service_for_web.CheckPassword(id, "8888"));
            Assert.IsFalse(service_for_web.CheckPassword(id, "0000"));
            Assert.IsFalse(service_for_web.CheckPassword(id, "8888"));
            Assert.IsFalse(service_for_web.CheckPassword(id, "8888"));
            Assert.IsFalse(service_for_web.CheckPassword(id, "8888"));
            Assert.IsFalse(service_for_web.CheckPassword(id, "8888"));
        }

        [TestMethod]
        public void BasicScenario2_ModelIsolationTest()
        {
            // ��x�� token, �� staff �ާ@�ϥΪ����v
            MemberServiceToken staff_token = MemberServiceTokenHelper.BuildToken("eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJTVEFGRiIsInN1YiI6ImFuZHJldyIsImp0aSI6IkFERTQzOUM0MjQyQjQwNEQ4NDAyRjQ0MjVEMDJDMkVGIiwiaWF0IjoxNjQ4OTk1MzY2Ljg3OTM3NiwiZXhwIjoxNzQzNjg5NzY2Ljg3OTM3ODZ9.BJbVQE2gHEpu39cz-9PQix8bHn5-GFBOriP80bi6fpo18T2nG636EeApFNd9sgcTAyf-9vYFEetUACALSU27qA");
            MemberService service_for_staff = new MemberService(staff_token, this._fsm, this._repo);

            var m1 = service_for_staff.GetMemberByName("andrew");
            m1.FailedLoginAttemptsCount++;

            var m2 = service_for_staff.GetMemberByName("andrew");

            Assert.AreEqual(m1.FailedLoginAttemptsCount, 1);
            Assert.AreEqual(m2.FailedLoginAttemptsCount, 0);
        }
    }
}
