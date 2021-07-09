using System;
using System.Threading.Tasks;

namespace CloudMirror
{
    class Program
    {
        static async Task Main(string[] _)
        {
            // Get client/server folders from user
            if (!ProviderFolderLocations.Init())
                return;

            Console.WriteLine("Press ctrl-C to stop gracefully");
            Console.WriteLine("-------------------------------");
            try
            {
                bool started = await FakeCloudProvider.Start();
                if (!started)
                {
                    Console.WriteLine("Failed to start");
                }
            }
            catch (Exception e)
            {
                CloudProviderSyncRootWatcher.Stop();
                CloudProviderRegistrar.Unregister();

                Console.WriteLine(e);
                Console.ReadLine();
            }
        }
    }
}
