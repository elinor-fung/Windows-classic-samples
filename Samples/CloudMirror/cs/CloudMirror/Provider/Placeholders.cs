using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Provider;
using Windows.Win32;
using Windows.Win32.Storage.CloudFilters;
using Windows.Win32.Storage.FileSystem;

namespace CloudMirror
{
    class Placeholders
    {
        public static async Task Create(string sourcePath, string sourceSubDir, string destPath)
        {
            StorageProviderItemProperty[] customProperties = new StorageProviderItemProperty[]
            {
                new StorageProviderItemProperty()
                {
                    Id = 1,
                    Value = "Value1",
                    // This icon is just for the sample. You should provide your own branded icon here
                    IconResource = "shell32.dll,-44"
                }
            };

            string destDir = Path.Combine(destPath, sourceSubDir);
            DirectoryInfo sourceDir = new DirectoryInfo(Path.Combine(sourcePath, sourceSubDir));
            foreach (FileSystemInfo info in sourceDir.EnumerateFileSystemInfos())
            {
                string relativeName = Path.Combine(sourceSubDir, info.Name);
                if (!TryCreatePlaceholder(info, relativeName, destDir))
                    continue;

                try
                {
                    Console.WriteLine($"Applying custom state for {relativeName}");

                    string fullPath = Path.Combine(destPath, relativeName);
                    StorageFile item = await StorageFile.GetFileFromPathAsync(fullPath);
                    await StorageProviderItemProperties.SetAsync(item, customProperties);

                    if (info is DirectoryInfo)
                    {
                        await Create(sourcePath, relativeName, destPath);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Failed to set custom state on {relativeName}: {e}");
                    throw;
                }
            }
        }

        private static unsafe bool TryCreatePlaceholder(FileSystemInfo info, string relativeName, string fullDestPath)
        {
            fixed (char* fileName = info.Name)
            fixed (char* relativeNamePtr = relativeName)
            {
                CF_PLACEHOLDER_CREATE_INFO cloudEntry = new CF_PLACEHOLDER_CREATE_INFO()
                {
                    FileIdentity = relativeNamePtr,
                    FileIdentityLength = (uint)(relativeName.Length * sizeof(char)),
                    RelativeFileName = fileName,
                    Flags = CF_PLACEHOLDER_CREATE_FLAGS.CF_PLACEHOLDER_CREATE_FLAG_MARK_IN_SYNC,
                    FsMetadata = new CF_FS_METADATA()
                    {
                        BasicInfo = new FILE_BASIC_INFO()
                        {
                            FileAttributes = (uint)info.Attributes,
                            CreationTime = info.CreationTime.ToFileTime(),
                            LastWriteTime = info.LastWriteTime.ToFileTime(),
                            LastAccessTime = info.LastAccessTime.ToFileTime(),
                            ChangeTime = info.LastWriteTime.ToFileTime()
                        },
                    }
                };

                if (info is DirectoryInfo)
                {
                    cloudEntry.Flags |= CF_PLACEHOLDER_CREATE_FLAGS.CF_PLACEHOLDER_CREATE_FLAG_DISABLE_ON_DEMAND_POPULATION;
                }
                else if (info is FileInfo fileInfo)
                {
                    cloudEntry.FsMetadata.FileSize = fileInfo.Length;
                }

                try
                {
                    Console.WriteLine($"Creating placeholder for {relativeName}");
                    PInvoke.CfCreatePlaceholders(fullDestPath, new CF_PLACEHOLDER_CREATE_INFO[] { cloudEntry }, CF_CREATE_FLAGS.CF_CREATE_FLAG_NONE, null);
                }
                catch (Exception e)
                {
                    // winrt::to_hresult() will eat the exception if it is a result of winrt::check_hresult,
                    // otherwise the exception will get rethrown and this method will crash out as it should
                    Console.WriteLine($"Failed to create placeholder for {relativeName}: {e}");
                    // Eating it here lets other files still get a chance. Not worth crashing the sample, but
                    // certainly noteworthy for production code
                    return false;
                }

                return true;
            }
        }
    }
}
