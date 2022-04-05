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
        public MemberModel Register(string name, string password, string email)
        {
            return this._service.Register(name, password, email);
        }


        [HttpPost]
        [Route("{id:int:min(1)}/activate")]
        [MemberServiceAction(ActionName = "activate")]
        public string Activate(int id, string number)
        {
            if (this._service.Activate(id, number))
            {
                return "OK";
            }

            return "FAIL";
        }

        [HttpPost]
        [Route("{id:int:min(1)}/lock")]
        [MemberServiceAction(ActionName = "lock")]
        public string Lock(int id, string reason)
        {
            if (this._service.Lock(id, reason))
            {
                return "OK";
            }

            return "FAIL";
        }


        [HttpPost]
        [Route("{id:int:min(1)}/unlock")]
        [MemberServiceAction(ActionName = "unlock")]
        public string UnLock(int id, string reason)
        {
            throw new NotImplementedException();
        }

        [HttpPost]
        [Route("{id:int:min(1)}/soft-delete")]
        [MemberServiceAction(ActionName = "soft-delete")]
        public string SoftDelete(int id, string reason)
        {
            if (this._service.Delete(id, reason))
            {
                return "OK";
            }

            return "FAIL";
        }

        [HttpPost]
        [Route("{id:int:min(1)}/delete")]
        [MemberServiceAction(ActionName = "delete")]
        public string Delete(int id, string reason)
        {
            if (this._service.Delete(id, reason))
            {
                return "OK";
            }

            return "FAIL";
        }
        #endregion


        #region domain / aggraton API(s), 會因為狀態決定能否執行，不會直接改變狀態 (除非內部呼叫了 major APIs)
        [HttpGet]
        [Route("{id:int:min(1)}/generate-validate-number")]
        [MemberServiceAction(ActionName = "generate-validate-number")]
        public string GenerateValidateNumber(int id)
        {
            return this._service.GenerateValidateNumber(id);
        }


        [HttpPost]
        [Route("check-password")]
        [MemberServiceAction(ActionName = "check-password")]
        public string CheckPassword(string name, string password)
        {
            var m = this._service.GetMemberByName(name);
            if (this._service.CheckPassword(m.Id, password))
            {
                return "OK";
            }
            return "FAIL";
        }

        [HttpGet]
        [Route("{id:int:min(1)}")]
        [MemberServiceAction(ActionName = "get-member")]
        public MemberModel GetMember(int id)
        {
            return this._service.GetMember(id);
        }

        [HttpGet]
        [MemberServiceAction(ActionName = "get-members")]
        public IEnumerable<MemberModel> GetMembers()
        {
            return this._service.GetMembers();
        }
        #endregion

    }
}
