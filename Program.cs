using System;
using System.Threading.Tasks;

namespace ServiceDaemon
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("[INFO] I'm a daemon to regularly restart your service!");
            ServiceDaemonOption option = new ServiceDaemonOption();
            option.RestartSpan = TimeSpan.FromMinutes(10);
            // option.RestartSpan = TimeSpan.FromHours(24);
            option.RetrySpan = TimeSpan.FromMinutes(1);
            ServiceDaemon daemon = new ServiceDaemon();
            Task task = daemon.StartAsync(option);
            task.Wait();
        }
    }
}
