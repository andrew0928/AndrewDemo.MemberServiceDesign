using AndrewDemo.Member.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAPI.Controllers;

namespace WebAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddSingleton<MemberRepo>(new MemberRepo(0, @"init-database.jsonl"));
            services.AddSingleton<MemberStateMachine>();
            services.AddScoped<MemberServiceToken>();
            services.AddScoped<MemberService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseMiddleware<MemberServiceMiddleware>();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

        }
    }

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
            MemberServiceToken.BuildToken(token, tokenText);

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
