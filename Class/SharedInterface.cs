using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using LeagueSharp.Loader.Data;

namespace LeagueSharp.Loader.Class
{
    public class Loader_API : Shared.ILoaderApi
    {
        public List<string> GetAssemblyPathList()
        {
            return Config.Instance.SelectedProfile.InstalledAssemblies.Where(a => a.InjectChecked && a.Type != AssemblyType.Library).Select(assembly => assembly.PathToBinary).ToList();
        }

        public void Recompile()
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

        public Tuple<int, int> GetHotkeys()
        {
            return new Tuple<int, int>(KeyInterop.VirtualKeyFromKey(Config.Instance.Hotkeys.SelectedHotkeys.First(h => h.Name == "Reload").Hotkey),
                                       KeyInterop.VirtualKeyFromKey(Config.Instance.Hotkeys.SelectedHotkeys.First(h => h.Name == "CompileAndReload").Hotkey));
        }

        public string GetLibrariesDirectory()
        {
            return Directories.CoreDirectory;
        }

        public string GetLeagueSharpDllName()
        {
            return PathRandomizer.LeagueSharpDllName;
        }

    };
}
