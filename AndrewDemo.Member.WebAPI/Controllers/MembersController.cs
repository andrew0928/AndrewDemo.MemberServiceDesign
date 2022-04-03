using AndrewDemo.Member.Contracts;
using AndrewDemo.Member.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MembersController : ControllerBase
    {
        private MemberService _service;
        private MemberServiceToken _token;

        public MembersController(MemberServiceToken token, MemberService service)
        {
            this._service= service;
            this._token = token;
        }


        #region major API(s), 執行後狀態會因而改變
        [HttpPost]
        [Route("register")]
        [MemberServiceAction(ActionName = "register")]
        public object Register()
        {
            throw new NotImplementedException();
        }

        [HttpPost]
        [Route("import")]
        [MemberServiceAction(ActionName = "import")]
        public object Import()
        {
            throw new NotImplementedException();
        }

        [HttpPost]
        [Route("{id:int:min(1)}/activate")]
        [MemberServiceAction(ActionName = "activate")]
        public object Activate(int id, string reason)
        {
            throw new NotImplementedException();
        }

        [HttpPost]
        [Route("{id:int:min(1)}/lock")]
        [MemberServiceAction(ActionName = "lock")]
        public object Lock(int id, string reason)
        {
            throw new NotImplementedException();
        }


        [HttpPost]
        [Route("{id:int:min(1)}/unlock")]
        [MemberServiceAction(ActionName = "unlock")]
        public object UnLock(int id, string reason)
        {
            throw new NotImplementedException();
        }

        [HttpPost]
        [Route("{id:int:min(1)}/soft_delete")]
        [MemberServiceAction(ActionName = "soft-delete")]
        public object SoftDelete(int id, string reason)
        {
            throw new NotImplementedException();
        }

        [HttpPost]
        [Route("{id:int:min(1)}/delete")]
        [MemberServiceAction(ActionName = "delete")]
        public object Delete(int id, string reason)
        {
            throw new NotImplementedException();
        }
        #endregion


        #region domain / aggraton API(s), 會因為狀態決定能否執行，不會直接改變狀態 (除非內部呼叫了 major APIs)
        [HttpGet]
        [Route("{id:int:min(1)}/generate_validate_number")]
        [MemberServiceAction(ActionName = "generate-validate-number")]
        public object GenerateValidateNumber(int id)
        {
            throw new NotImplementedException();
        }

        [HttpGet]
        [Route("{id:int:min(1)}/check_validation_number/{validateNumber}")]
        [MemberServiceAction(ActionName = "confirm-validate-number")]
        public object ConfirmValidateNumber(int id, string validateNumber)
        {
            throw new NotImplementedException();
        }

        [HttpPost]
        [Route("check_password")]
        [MemberServiceAction(ActionName = "check-password")]
        public object CheckPassword(string name, string password)
        {
            throw new NotImplementedException();
        }

        [HttpGet]
        [Route("{id:int:min(1)}")]
        [MemberServiceAction(ActionName = "get-member")]
        public MemberModel GetMember(int id)
        {
            return new MemberModel()
            {
                Id = id,
                Name = $"user{id}",
                PasswordHash = null,
                State = MemberState.ACTIVATED
            };
        }

        [HttpGet]
        [MemberServiceAction(ActionName = "get-members")]
        public IEnumerable<MemberModel> GetMembers()
        {
            return this._service.GetMembers();
        }
        #endregion

    }


    //public class MemberServiceActionRuleAttribute : Attribute
    //{
    //    public string Name { get; set; }
    //    public MemberState InitState { get; set; }
    //    public MemberState? FinalState { get; set; }
    //    public string AllowIdentityType { get; set; }
    //    public bool IsFireEvent { get; set; }
    //}



}
