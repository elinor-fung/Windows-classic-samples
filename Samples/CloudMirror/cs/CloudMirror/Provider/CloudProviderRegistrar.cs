using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Storage;
using Windows.Storage.Provider;

namespace CloudMirror
{
    class CloudProviderRegistrar
    {
        private const string StorageProviderId = "TestStorageProvider";
        private const string StorageProviderAccount = "TestAccount1";

        public static async Task RegisterWithShell()
        {
            try
            {
                StorageProviderSyncRootInfo info = new StorageProviderSyncRootInfo()
                {
                    Id = GetSyncRootId(),
                    Path = await StorageFolder.GetFolderFromPathAsync(ProviderFolderLocations.ClientFolder),
                    DisplayNameResource = "TestStorageProviderDisplayName",
                    // This icon is just for the sample. You should provide your own branded icon here
                    IconResource = "%SystemRoot%\\system32\\charmap.exe,0",
                    HydrationPolicy = StorageProviderHydrationPolicy.Full,
                    HydrationPolicyModifier = StorageProviderHydrationPolicyModifier.None,
                    PopulationPolicy = StorageProviderPopulationPolicy.AlwaysFull,
                    InSyncPolicy = StorageProviderInSyncPolicy.FileCreationTime | StorageProviderInSyncPolicy.DirectoryCreationTime,
                    Version = "1.0.0",
                    ShowSiblingsAsGroup = false,
                    HardlinkPolicy = StorageProviderHardlinkPolicy.None,
                    RecycleBinUri = new Uri("http://cloudmirror.example.com/recyclebin"),
                    Context = CryptographicBuffer.ConvertStringToBinary($"{ProviderFolderLocations.ServerFolder}->{ProviderFolderLocations.ClientFolder}", BinaryStringEncoding.Utf8),
                };

                IList<StorageProviderItemPropertyDefinition> customStates = info.StorageProviderItemPropertyDefinitions;
                customStates.Add(new StorageProviderItemPropertyDefinition() { DisplayNameResource = "CustomStateName1", Id = 1 });
                customStates.Add(new StorageProviderItemPropertyDefinition() { DisplayNameResource = "CustomStateName2", Id = 2 });
                customStates.Add(new StorageProviderItemPropertyDefinition() { DisplayNameResource = "CustomStateName3", Id = 3 });

                StorageProviderSyncRootManager.Register(info);

                // Give the cache some time to invalidate
                System.Threading.Thread.Sleep(1000);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Could not register the sync root: {e}");
            }
        }

        public static void Unregister()
        {
            try
            {
                StorageProviderSyncRootManager.Unregister(GetSyncRootId());
            }
            catch (Exception e)
            {
                Console.WriteLine($"Could not unregister the sync root: {e}");
            }
        }

        private static string GetSyncRootId()
        {
            return $"{StorageProviderId}!{WindowsIdentity.GetCurrent().User}!{StorageProviderAccount}";
        }
    }
}
