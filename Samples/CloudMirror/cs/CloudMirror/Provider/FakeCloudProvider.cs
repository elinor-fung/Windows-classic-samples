using System;
using System.Threading.Tasks;
using Windows.Win32.System.Search;

namespace CloudMirror
{
    public static class FakeCloudProvider
    {
        public static async Task<bool> Start()
        {
            // Stage 1: Setup
            //--------------------------------------------------------------------------------------------
            // The client folder (syncroot) must be indexed in order for states to properly display
            AddFolderToSearchIndexer(ProviderFolderLocations.ClientFolder);
            // Start up the task that registers and hosts the services for the shell (such as custom states, menus, etc)
            ShellServices.InitAndStartServiceTask();
            // Register the provider with the shell so that the Sync Root shows up in File Explorer
            await CloudProviderRegistrar.RegisterWithShell();
            // Hook up callback methods (in this class) for transferring files between client and server
            ConnectSyncRootTransferCallbacks();
            // Create the placeholders in the client folder so the user sees something
            await Placeholders.Create(ProviderFolderLocations.ServerFolder, string.Empty, ProviderFolderLocations.ClientFolder);

            // Stage 2: Running
            //--------------------------------------------------------------------------------------------
            // The file watcher loop for this sample will run until the user presses Ctrl-C.
            // The file watcher will look for any changes on the files in the client (syncroot) in order
            // to let the cloud know.
            CloudProviderSyncRootWatcher.WatchAndWait();

            // Stage 3: Done Running-- caused by CTRL-C
            //--------------------------------------------------------------------------------------------
            // Unhook up those callback methods
            DisconnectSyncRootTransferCallbacks();

            // A real sync engine should NOT unregister the sync root upon exit.
            // This is just to demonstrate the use of StorageProviderSyncRootManager::Unregister.
            CloudProviderRegistrar.Unregister();

            return true;
        }

        // Registers the callbacks in the table at the top of this file so that the methods above
        // are called for our fake provider
        private static unsafe void ConnectSyncRootTransferCallbacks()
        {
            try
            {
                // TODO: Connect to the sync root using Cloud File API
            }
            catch (Exception e)
            {
                Console.WriteLine($"Could not connect to sync root: {e}");
            }
        }

        // Unregisters the callbacks in the table at the top of this file so that
        // the client doesn't Hindenburg
        private static void DisconnectSyncRootTransferCallbacks()
        {
            Console.WriteLine("Shutting down");
            try
            {
                // TODO: Disconnect from to the sync root using Cloud File API
            }
            catch (Exception e)
            {
                Console.WriteLine($"Could not disconnect the sync root: {e}");
            }
        }

        private static unsafe void AddFolderToSearchIndexer(string folder)
        {
            Type type = Type.GetTypeFromCLSID(typeof(CSearchManager).GUID);
            var searchManager = (ISearchManager)Activator.CreateInstance(type);
            ISearchCatalogManager catalogManager;

            const string index = "SystemIndex";
            fixed (char* catalog = index)
            {
                searchManager.GetCatalog(catalog, out catalogManager);
            }

            ISearchCrawlScopeManager crawlScopeManager;
            catalogManager.GetCrawlScopeManager(out crawlScopeManager);

            string url = $"file:///{folder}";
            fixed (char* urlPtr = url)
            {
                crawlScopeManager.AddDefaultScopeRule(urlPtr, true, (uint)FOLLOW_FLAGS.FF_INDEXCOMPLEXURLS);
            }

            crawlScopeManager.SaveAll();
        }
    }
}
