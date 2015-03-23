using System.Runtime.Serialization;

namespace LeagueSharp.Sandbox.Shared
{
    [DataContract]
    public class LoginCredentials
    {
        [DataMember]
        public string Password { get; set; }

        [DataMember]
        public string User { get; set; }
    }
}