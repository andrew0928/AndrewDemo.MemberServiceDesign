using AndrewDemo.Member.Contracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace AndrewDemo.Member.Core
{
    public class MemberRepo
    {


        public MemberRepo(int seed = 0, string importFilePath = null)
        {
            this._seed = seed;
            if (importFilePath != null) this.Import(importFilePath);
        }

        internal int _seed = 0;
        internal Dictionary<int, MemberModel> _members = new Dictionary<int, MemberModel>();
        //internal Dictionary<int, int> _members_password_error_counts = new Dictionary<int, int>();
        //internal Dictionary<int, (string number, DateTime expireTime)> _members_pending_validations = new Dictionary<int, (string number, DateTime expireTime)>();
        internal Dictionary<int, object> _members_syncroot = new Dictionary<int, object>();

        internal int GetNewID()
        {
            return Interlocked.Increment(ref this._seed);
        }

        public bool Import(string file)
        {
            if (file == null) return false;
            if (File.Exists(file) == false) return false;

            int maxid = 0;
            foreach (string line in File.ReadAllLines(file))
            {
                var member = JsonSerializer.Deserialize<MemberModel>(line);
                this._members.Add(member.Id, member);

                maxid = Math.Max(maxid, member.Id);
            }
            this._seed = Math.Max(this._seed, maxid);
            return true;
        }
        public bool Export(string file)
        {
            if (file == null) return false;
            if (File.Exists(file) == true) return false;
            File.AppendAllLines(file, this.ExportLines());
            return true;
        }
        private IEnumerable<string> ExportLines()
        {
            foreach(var member in this._members.Values)
            {
                yield return JsonSerializer.Serialize<MemberModel>(member);
            }
            yield break;
        }
    }

}
