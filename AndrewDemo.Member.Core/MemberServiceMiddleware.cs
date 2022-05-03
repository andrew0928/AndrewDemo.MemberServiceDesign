using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using AndrewDemo.Member.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace AndrewDemo.Member.Core
{
    public class MemberServiceMiddleware
    {
        private readonly RequestDelegate _next;
        private const string _bearerText = "Bearer ";

        public MemberServiceMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        // if return false, bypass next middleware chain.
        private bool PreProcessMemberService(HttpContext context, MemberServiceToken token, MemberStateMachine fsm, MemberService service)
        {
            if (context.Request.Headers.TryGetValue("authorization", out var values) == false) return true;
            if (string.IsNullOrEmpty(values.FirstOrDefault())) return true;
            if (values.FirstOrDefault().StartsWith(_bearerText, StringComparison.OrdinalIgnoreCase) == false) return true;

            var tokenText = values.FirstOrDefault().Substring(_bearerText.Length);
            MemberServiceTokenHelper.BuildToken(token, tokenText);

            // Members only
            if (context.Request.RouteValues["controller"] as string != "Members") return true;

            int? id = null;
            if (context.Request.RouteValues.ContainsKey("id"))
            {
                id = int.Parse(context.Request.RouteValues["id"] as string);
            }

            string actionName = null;
            var ep = context.GetEndpoint();
            if (ep != null)
            {
                MemberServiceActionAttribute action = (
                    from x in ep.Metadata
                    where x is MemberServiceActionAttribute
                    select x as MemberServiceActionAttribute).FirstOrDefault();
                Console.WriteLine($"Action: {action.ActionName}");
                actionName = action.ActionName;
            }

            service.FSMRuleCheck(id, actionName);
            return true;
        }

        public async Task Invoke(HttpContext context, MemberServiceToken token, MemberStateMachine fsm, MemberService service)
        {
            try
            {
                if (this.PreProcessMemberService(context, token, fsm, service))
                {
                    await _next(context);
                }
            }
            catch (MemberServiceException e)
            {
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync("MemberStateMachineException: " + e.Message);
            }
        }
    }

}
