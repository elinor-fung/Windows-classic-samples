using System;
using System.IO;
using System.Runtime.InteropServices;
using Windows.Storage.Provider;

namespace CloudMirror
{
    [ComVisible(true)]
    [Guid("F64E794B-CD29-4029-8A4D-C411B0D415A3")]
    class UriSource : IStorageProviderUriSource
    {
        private const string ContentIdPrefix = "http://cloudmirror.example.com/contentId/";
        private const string ContentUriPrefix = "http://cloudmirror.example.com/contentUri/";

        public void GetContentInfoForPath(string path, StorageProviderGetContentInfoForPathResult result)
        {
            string fileName = Path.GetFileName(path);
            result.ContentId = $"{ContentIdPrefix}{fileName}";
            result.ContentUri = $"{ContentUriPrefix}{fileName}?StorageProviderId=TestStorageProvider";
            result.Status = StorageProviderUriSourceStatus.Success;
        }

        public void GetPathForContentUri(string contentUri, StorageProviderGetPathForContentUriResult result)
        {
            result.Status = StorageProviderUriSourceStatus.FileNotFound;

            if (!contentUri.StartsWith(ContentUriPrefix))
                return;

            string localPath = Path.Combine(ProviderFolderLocations.ClientFolder, contentUri[ContentUriPrefix.Length..contentUri.IndexOf('?')]);
            if (File.Exists(localPath) || Directory.Exists(localPath))
            {
                result.Path = localPath;
                result.Status = StorageProviderUriSourceStatus.Success;
            }
        }
    }
}
