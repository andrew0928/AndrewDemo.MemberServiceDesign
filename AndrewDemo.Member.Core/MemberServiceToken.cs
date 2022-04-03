using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace AndrewDemo.Member.Core
{
    public class MemberServiceToken
    {
        public bool IsInitialized { get; internal set; }

        // JWT claim: iss, issuer
        // type, USER | STAFF
        public string IdentityType { get; internal set; }

        // JWT claim: sub, subject
        // who, iosapp | androidapp | webui | {staff}@chicken-house.net
        public string IdentityName { get; internal set; }

        // JWT claim: jti, JWT ID
        public string ID { get; internal set; }

        // JWT claim: iat, issue at
        public DateTime CreateTime { get; internal set; }

        // JWT claim: exp, expiration
        public DateTime ExpireTime { get; internal set; }






        public static MemberServiceToken BuildToken(string tokenText, bool check_expiration = true)
        {
            MemberServiceToken token = new MemberServiceToken();
            return BuildToken(token, tokenText);
        }

        public static MemberServiceToken BuildToken(MemberServiceToken token, string tokenText)
        {
            string payload = Jose.JWT.Decode(tokenText, _jwt_key, Jose.JwsAlgorithm.HS512);
            var _x = JsonSerializer.Deserialize<jwt_spec>(payload);

            // TODO: load RSA key from Secret Manager or Key Store
            //MemberServiceToken token = new MemberServiceToken();

            token.IsInitialized = true;

            token.ID = _x.jti;
            token.IdentityType = _x.iss;
            token.IdentityName = _x.sub;
            token.CreateTime = ConvertUnixTimeStamp(_x.iat);
            token.ExpireTime = ConvertUnixTimeStamp(_x.exp);

            return token;
        }


        private static readonly byte[] _jwt_key = new byte[] { 0x06, 0x07, 0x04, 0x01 };   // 6741, base64: BgcEAQ==

        //public static (string type, string name) RestoreToken(string token)
        //{
        //    string payload = Jose.JWT.Decode(token, _jwt_key, Jose.JwsAlgorithm.HS512);

        //    var t = JsonSerializer.Deserialize<x>(payload);

        //    return (t.iss, t.sub);
        //}

        public static string CreateToken(string identityType, string identityName)
        {
            string payload = JsonSerializer.Serialize<jwt_spec>(new jwt_spec()
            {
                iss = identityType,
                sub = identityName,
                jti = Guid.NewGuid().ToString("N").ToUpper(),
                iat = ConvertUnixTimeStamp(DateTime.UtcNow),
                exp = ConvertUnixTimeStamp(DateTime.UtcNow.AddYears(3))
            });

            return Jose.JWT.Encode(
                payload,
                _jwt_key, //new byte[] { 0x06, 0x07, 0x04, 0x01 },
                Jose.JwsAlgorithm.HS512);
        }

        private static double ConvertUnixTimeStamp(DateTime utcTime)
        {
            return utcTime.Subtract(new DateTime(1970,1,1,0,0,0,0,DateTimeKind.Utc)).TotalSeconds;
        }

        private static DateTime ConvertUnixTimeStamp(double value)
        {
            return (new DateTime(1970, 1, 1, 10, 0, 0, 0, DateTimeKind.Utc)).AddSeconds(value);
        }

    }

    internal class jwt_spec
    {
        public string iss { get; set; }
        public string sub { get; set; }
        public string jti { get; set; }
        public double iat { get; set; }
        public double exp { get; set; }
    }
}
