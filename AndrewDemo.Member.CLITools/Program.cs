using AndrewDemo.Member.Contracts;
using AndrewDemo.Member.Core;
using System;

namespace AndrewDemo.Member.CLITools
{
    internal class Program
    {
        static void Main(string[] args)
        {
            GenToken();
            RepoImportExport();
        }

        static void GenToken()
        {
            //var token = new MemberServiceToken()
            //{
            //    ID = Guid.NewGuid().ToString("N").ToUpper(),
            //    IdentityType = "USER",
            //    IdentityName = "WebUI",
            //    CreateTime = DateTime.UtcNow,
            //    ExpireTime = DateTime.UtcNow.AddYears(3)
            //};
            foreach(var pair in new (string type, string name)[] { ("USER", "WebUI"), ("STAFF", "andrew") })
            {
                Console.WriteLine($"Token({pair.type}, {pair.name}):");
                Console.WriteLine("".PadRight(80, '='));
                Console.WriteLine(MemberServiceToken.CreateToken(pair.type, pair.name));
                Console.WriteLine();
            }
        }

        static void RepoImportExport()
        {
            MemberRepo repo = new MemberRepo(0, "init-database.jsonl");

            // token, staff | andrew | 2022/04/04 ~ +3 years
            MemberServiceToken token = MemberServiceToken.BuildToken("eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJTVEFGRiIsInN1YiI6ImFuZHJldyIsImp0aSI6IkFERTQzOUM0MjQyQjQwNEQ4NDAyRjQ0MjVEMDJDMkVGIiwiaWF0IjoxNjQ4OTk1MzY2Ljg3OTM3NiwiZXhwIjoxNzQzNjg5NzY2Ljg3OTM3ODZ9.BJbVQE2gHEpu39cz-9PQix8bHn5-GFBOriP80bi6fpo18T2nG636EeApFNd9sgcTAyf-9vYFEetUACALSU27qA");
            MemberStateMachine fsm = new MemberStateMachine();
            MemberService serv = new MemberService(token, fsm, repo);

            serv.Import(new MemberModel() { Name = "andrew", Email = "andrew@ch.net", State = MemberState.ACTIVATED });
            serv.Import(new MemberModel() { Name = "nancy", State = MemberState.ACTIVATED });
            serv.Import(new MemberModel() { Name = "peter", State = MemberState.ACTIVATED });
            serv.Import(new MemberModel() { Name = "annie", State = MemberState.ACTIVATED });

            repo.Export("result-database.jsonl");
        }
    }
}
