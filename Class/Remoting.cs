using System;
using System.ServiceModel;
using LeagueSharp.Sandbox.Shared;

namespace LeagueSharp.Loader.Class
{
    internal class Remoting
    {
        private static ServiceHost _loaderServiceHost;
        private static ServiceHost _logServiceHost;

        public static void Init()
        {
            _loaderServiceHost = ServiceFactory.CreateService<ILoaderService, LoaderService>();
            _loaderServiceHost.Faulted += OnLoaderServiceFaulted;
            _logServiceHost = ServiceFactory.CreateService<ILoaderLogService, LoaderService>();
            _logServiceHost.Faulted += OnLogServiceFaulted;
        }

        private static void OnLoaderServiceFaulted(object sender, EventArgs eventArgs)
        {
            Console.WriteLine("ILoaderService faulted, trying restart");
            _loaderServiceHost.Faulted -= OnLoaderServiceFaulted;
            _loaderServiceHost.Abort();

            try
            {
                _loaderServiceHost = ServiceFactory.CreateService<ILoaderService, LoaderService>();
                _loaderServiceHost.Faulted += OnLoaderServiceFaulted;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static void OnLogServiceFaulted(object sender, EventArgs eventArgs)
        {
            Console.WriteLine("ILoaderLogService faulted, trying restart");
            _loaderServiceHost.Faulted -= OnLogServiceFaulted;
            _loaderServiceHost.Abort();

            try
            {
                _logServiceHost = ServiceFactory.CreateService<ILoaderLogService, LoaderService>();
                _loaderServiceHost.Faulted += OnLogServiceFaulted;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}