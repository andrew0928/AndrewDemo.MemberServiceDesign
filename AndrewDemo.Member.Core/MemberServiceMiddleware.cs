using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using AndrewDemo.Member.Contracts;

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

        public async Task Invoke(HttpContext context, MemberServiceToken token, MemberStateMachine fsm)
        {
            if (context.Request.Headers.TryGetValue("authorization", out var values) == false) goto next;
            if (string.IsNullOrEmpty(values.FirstOrDefault())) goto next;
            if (values.FirstOrDefault().StartsWith(_bearerText, StringComparison.OrdinalIgnoreCase) == false) goto next;

            var tokenText = values.FirstOrDefault().Substring(_bearerText.Length);
            MemberServiceTokenHelper.BuildToken(token, tokenText);

            // Members only
            if (context.Request.RouteValues["controller"] as string != "Members") goto next;

            int id = 0;
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

            if (id == 0)
            {
                if (fsm.CanExecute(actionName, token.IdentityType) == false)
                {
                    context.Response.StatusCode = 500;
                    return;
                }
            }

        next:
            await _next(context);
        }
    }

}
