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
            foreach(var pair in new (string type, string name)[] {
                ("USER", "WebUI"), 
                ("STAFF", "andrew") 
            })
            {
                Console.WriteLine($"Token({pair.type}, {pair.name}):");
                Console.WriteLine("".PadRight(80, '='));
                Console.WriteLine(MemberServiceTokenHelper.CreateToken(pair.type, pair.name));
                Console.WriteLine();
            }
        }

        static void RepoImportExport()
        {
            MemberRepo repo = new MemberRepo();

            // token, staff | andrew | 2022/04/04 ~ +3 years
            //MemberServiceToken token = MemberServiceTokenHelper.BuildToken("eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJTVEFGRiIsInN1YiI6ImFuZHJldyIsImp0aSI6IkFERTQzOUM0MjQyQjQwNEQ4NDAyRjQ0MjVEMDJDMkVGIiwiaWF0IjoxNjQ4OTk1MzY2Ljg3OTM3NiwiZXhwIjoxNzQzNjg5NzY2Ljg3OTM3ODZ9.BJbVQE2gHEpu39cz-9PQix8bHn5-GFBOriP80bi6fpo18T2nG636EeApFNd9sgcTAyf-9vYFEetUACALSU27qA");

            // token, user | webui | 2022/04/04 ~ +3 years
            MemberServiceToken token = MemberServiceTokenHelper.BuildToken("eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJVU0VSIiwic3ViIjoiV2ViVUkiLCJqdGkiOiJFNTMyM0FBNTU4MjY0OUQ3QUJDOUZFODQxMjkwMDFDMiIsImlhdCI6MTY0ODk5Njc0MS42MDQ1MTY1LCJleHAiOjE3NDM2OTExNDEuNjA0NzE2fQ.RynDs43NEjMXfnMPbAKqEr2MBqI1oub2X-4xEuve5Q21tMYcZAXPn60fe0wdJLO0uJUAeRTxS0HdBOR70zmAsA");
            MemberStateMachine fsm = new MemberStateMachine();
            MemberService serv = new MemberService(token, fsm, repo);

            MemberModel m = null;

            // andrew
            m = serv.Register("andrew", "0000", "andrew@123.net");
            if (m != null) serv.Activate(m.Id, m.ValidateNumber);

            // nancy
            m = serv.Register("nancy", "0000", "nancy@456.com");
            if (m != null) serv.Activate(m.Id, m.ValidateNumber);

            // peter
            m = serv.Register("peter", "0000", "peter@789.idv.tw");

            // annie
            m = serv.Register("annie", "0000", "annie@012.org");


            repo.Export("result-database.jsonl");
        }
    }
}
