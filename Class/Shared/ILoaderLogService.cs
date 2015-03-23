using System.ServiceModel;

namespace LeagueSharp.Sandbox.Shared
{
    [ServiceContract]
    public interface ILoaderLogService
    {
        [OperationContract]
        void Debug(string s);

        [OperationContract]
        void DebugFormat(string s, params object[] param);

        [OperationContract]
        void Info(string s);

        [OperationContract]
        void InfoFormat(string s, params object[] param);

        [OperationContract]
        void Warn(string s);

        [OperationContract]
        void WarnFormat(string s, params object[] param);

        [OperationContract]
        void Error(string s);

        [OperationContract]
        void ErrorFormat(string s, params object[] param);

        [OperationContract]
        void Fatal(string s);

        [OperationContract]
        void FatalFormat(string s, params object[] param);
    }
}