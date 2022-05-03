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
        public IActionResult Register([FromForm]string name, [FromForm] string password, [FromForm] string email)
        {
            return this.Ok(this._service.Register(name, password, email));
        }


        [HttpPost]
        [Route("{id:int:min(1)}/activate")]
        [MemberServiceAction(ActionName = "activate")]
        public IActionResult Activate(int id, [FromForm]string number)
        {
            this._service.Activate(id, number);
            return this.Ok();
        }

        [HttpPost]
        [Route("{id:int:min(1)}/lock")]
        [MemberServiceAction(ActionName = "lock")]
        public IActionResult Lock(int id, [FromForm] string reason)
        {
            this._service.Lock(id, reason);
            return this.Ok();
        }


        [HttpPost]
        [Route("{id:int:min(1)}/unlock")]
        [MemberServiceAction(ActionName = "unlock")]
        public IActionResult UnLock(int id, [FromForm] string reason)
        {
            throw new NotImplementedException();
        }

        [HttpPost]
        [Route("{id:int:min(1)}/soft-delete")]
        [MemberServiceAction(ActionName = "soft-delete")]
        public IActionResult SoftDelete(int id, [FromForm] string reason)
        {
            this._service.Delete(id, reason);
            return this.Ok();
        }

        [HttpPost]
        [Route("{id:int:min(1)}/delete")]
        [MemberServiceAction(ActionName = "delete")]
        public IActionResult Delete(int id, [FromForm] string reason)
        {
            this._service.Delete(id, reason);
            return this.Ok();
        }
        #endregion


        #region domain / aggraton API(s), 會因為狀態決定能否執行，不會直接改變狀態 (除非內部呼叫了 major APIs)
        [HttpGet]
        [Route("{id:int:min(1)}/generate-validate-number")]
        [MemberServiceAction(ActionName = "generate-validate-number")]
        public IActionResult GenerateValidateNumber(int id)
        {
            return this.Ok(this._service.GenerateValidateNumber(id));
        }


        [HttpPost]
        [Route("check-password")]
        [MemberServiceAction(ActionName = "check-password")]
        public IActionResult CheckPassword([FromForm] string name, [FromForm] string password)
        {
            var m = this._service.GetMemberByName(name);
            if (m == null) return this.StatusCode(403);

            if (this._service.CheckPassword(m.Id, password))
            {
                return Ok();
            }
            return this.StatusCode(403); ;
        }

        [HttpGet]
        [Route("{id:int:min(1)}")]
        [MemberServiceAction(ActionName = "get-member")]
        public IActionResult GetMember(int id)
        {
            return this.Ok(this._service.GetMember(id));
        }

        [HttpGet]
        [MemberServiceAction(ActionName = "get-members")]
        //public IEnumerable<MemberModel> GetMembers()
        public IActionResult GetMembers()
        {
            return this.Ok(this._service.GetMembers());
        }
        #endregion

    }
}
