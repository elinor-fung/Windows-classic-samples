using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Windows.Storage.Provider;

namespace CloudMirror
{
    [ComVisible(true)]
    [Guid("8189A68C-2ECF-4565-88D7-A898C8C1D74A")]
    [ComDefaultInterface(typeof(IStorageProviderItemPropertySource))]
    public sealed class CustomStateProvider : IStorageProviderItemPropertySource
    {
        public IEnumerable<StorageProviderItemProperty> GetItemProperties(string itemPath)
        {
            if (itemPath.Length % 2 == 0)
            {
                return Array.Empty<StorageProviderItemProperty>();
            }

            var ret =  new StorageProviderItemProperty[]
            {
                new StorageProviderItemProperty()
                {
                    Id = 2,
                    Value = "Value2",
                    // This icon is just for the sample. You should provide your own branded icon here
                    IconResource = "shell32.dll,-14"
                }
            };

            return ret;
        }
    }
}
