using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    };
}
