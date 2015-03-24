using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using LeagueSharp.Loader.Data;
using LeagueSharp.Sandbox.Shared;

namespace LeagueSharp.Loader.Class
{
    public class LoaderService : ILoaderService, ILoaderLogService
    {
        public List<LSharpAssembly> GetAssemblyList(int pid)
        {
            return Config.Instance.SelectedProfile.InstalledAssemblies
                .Where(a => a.InjectChecked && a.Type != AssemblyType.Library)
                .Select(assembly => new LSharpAssembly
            {
                Name = assembly.Name,
                PathToBinary = assembly.PathToBinary
            }).ToList();
        }

        public Configuration GetConfiguration(int pid)
        {
            var reload = 0;
            var recompile = 0;
            var antiAfk = false;
            var console = false;
            var towerRange = false;
            var extendedZoom = false;

            try
            {
                reload = KeyInterop.VirtualKeyFromKey(Config.Instance.Hotkeys.SelectedHotkeys.First(h => h.Name == "Reload").Hotkey);
                recompile = KeyInterop.VirtualKeyFromKey(Config.Instance.Hotkeys.SelectedHotkeys.First(h => h.Name == "CompileAndReload").Hotkey);
                antiAfk = Config.Instance.Settings.GameSettings.First(s => s.Name == "Anti-AFK").SelectedValue == "True";
                console = Config.Instance.Settings.GameSettings.First(s => s.Name == "Debug Console").SelectedValue == "True";
                towerRange = Config.Instance.Settings.GameSettings.First(s => s.Name == "Display Enemy Tower Range").SelectedValue == "True";
                extendedZoom = Config.Instance.Settings.GameSettings.First(s => s.Name == "Extended Zoom").SelectedValue == "True";
            }
            catch
            {
                // ignored
            }

            return new Configuration
            {
                DataDirectory = Directories.AppDataDirectory,
                LeagueSharpDllPath = PathRandomizer.LeagueSharpDllPath,
                LibrariesDirectory = Directories.CoreDirectory,
                ReloadKey = reload,
                ReloadAndRecompileKey = recompile,
                UnloadKey = 0x75,
                AntiAfk = antiAfk,
                Console = console,
                TowerRange = towerRange,
                ExtendedZoom = false,
                Permissions = null
            };
        }

        public void Recompile(int pid)
        {
            var targetAssemblies =
                    Config.Instance.SelectedProfile.InstalledAssemblies.Where(
                        a => a.InjectChecked || a.Type == AssemblyType.Library).ToList();

            foreach (var assembly in targetAssemblies)
            {
                if (assembly.Type == AssemblyType.Library)
                {
                    assembly.Compile();
                }
            }

            foreach (var assembly in targetAssemblies)
            {
                if (assembly.Type != AssemblyType.Library)
                {
                    assembly.Compile();
                }
            }
        }

        public void Debug(string s)
        {
        }

        public void DebugFormat(string s, params object[] param)
        {
        }

        public void Info(string s)
        {
        }

        public void InfoFormat(string s, params object[] param)
        {
        }

        public void Warn(string s)
        {
        }

        public void WarnFormat(string s, params object[] param)
        {
        }

        public void Error(string s)
        {
        }

        public void ErrorFormat(string s, params object[] param)
        {
        }

        public void Fatal(string s)
        {
        }

        public void FatalFormat(string s, params object[] param)
        {
        }
    }
}
