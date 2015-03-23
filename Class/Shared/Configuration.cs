using System.Runtime.Serialization;
using System.Security;

namespace LeagueSharp.Sandbox.Shared
{
    [DataContract]
    public class Configuration
    {
        [DataMember]
        public bool AntiAfk { get; set; }

        [DataMember]
        public bool Console { get; set; }

        [DataMember]
        public string DataDirectory { get; set; }

        [DataMember]
        public bool ExtendedZoom { get; set; }

        [DataMember]
        public int MenuKey { get; set; }

        [DataMember]
        public int MenuToggleKey { get; set; }

        [DataMember]
        public PermissionSet Permissions { get; set; }

        [DataMember]
        public int ReloadAndRecompileKey { get; set; }

        [DataMember]
        public int ReloadKey { get; set; }

        [DataMember]
        public bool TowerRange { get; set; }

        [DataMember]
        public int UnloadKey { get; set; }

        public override string ToString()
        {
            return string.Format("DataDirectory:{0}\n" +
                                 "MenuKey:{1}\n" +
                                 "MenuToggleKey:{2}\n" +
                                 "AntiAfk:{3}\n" +
                                 "Console:{4}\n" +
                                 "ExtendedZoom:{5}\n" +
                                 "TowerRange:{6}\n" +
                                 "ReloadKey:{7}\n" +
                                 "ReloadAndRecompileKey:{8}\n" +
                                 "ReloadAndRecompileKey:{9}\n",
                DataDirectory,
                MenuKey,
                MenuToggleKey,
                AntiAfk,
                Console,
                ExtendedZoom,
                TowerRange,
                ReloadKey,
                ReloadAndRecompileKey,
                UnloadKey);
        }
    }
}