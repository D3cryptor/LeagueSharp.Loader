using System.Collections.Generic;
using System.ServiceModel;

namespace LeagueSharp.Sandbox.Shared
{
    [ServiceContract]
    public interface ILoaderService
    {
        [OperationContract]
        List<LSharpAssembly> GetAssemblyList(int pid);

        [OperationContract]
        Configuration GetConfiguration(int pid);

        [OperationContract]
        void Recompile(int pid);
    }
}