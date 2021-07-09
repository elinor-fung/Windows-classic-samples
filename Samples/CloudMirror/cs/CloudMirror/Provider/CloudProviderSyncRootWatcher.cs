using Microsoft.Win32.SafeHandles;
using System;
using System.IO;
using System.Threading;
using Windows.Win32;
using Windows.Win32.Storage.FileSystem;
using Windows.Win32.Storage.CloudFilters;

namespace CloudMirror
{
    class CloudProviderSyncRootWatcher
    {
        private static ManualResetEvent stopEvent = new ManualResetEvent(false);
        public static void WatchAndWait()
        {
            Console.CancelKeyPress += OnCancelKeyPress;

            using (var watcher = new FileSystemWatcher(ProviderFolderLocations.ClientFolder))
            {
                watcher.Changed += OnSyncRootFileChanged;
                watcher.Created += OnSyncRootFileChanged;
                watcher.EnableRaisingEvents = true;
                while (true)
                {
                    if (stopEvent.WaitOne(1000))
                    {
                        break;
                    }
                }

                watcher.Changed -= OnSyncRootFileChanged;
                watcher.Created -= OnSyncRootFileChanged;
            }

            Console.CancelKeyPress -= OnCancelKeyPress;
        }

        private static unsafe void OnSyncRootFileChanged(object sender, FileSystemEventArgs e)
        {
            string path = e.FullPath;
            Console.WriteLine($"Processing change for {path}");

            FileAttributes attr = File.GetAttributes(path);
            if (attr.HasFlag(FileAttributes.Directory))
                return;

            const int FILE_ATTRIBUTE_PINNED = 0x00080000;
            const int FILE_ATTRIBUTE_UNPINNED = 0x00100000;

            SafeFileHandle placeholder = PInvoke.CreateFile(path, 0, FILE_SHARE_MODE.FILE_SHARE_READ, null, FILE_CREATION_DISPOSITION.OPEN_EXISTING, 0, null);
            if (((int)attr & FILE_ATTRIBUTE_PINNED) != 0)
            {
                Console.WriteLine($"Hydrating file {path}");
                PInvoke.CfHydratePlaceholder(placeholder, 0, long.MaxValue, CF_HYDRATE_FLAGS.CF_HYDRATE_FLAG_NONE, null);
            }
            else if (((int)attr & FILE_ATTRIBUTE_UNPINNED) != 0)
            {
                Console.WriteLine($"Deydrating file {path}");
                PInvoke.CfDehydratePlaceholder(placeholder, 0, long.MaxValue, CF_DEHYDRATE_FLAGS.CF_DEHYDRATE_FLAG_NONE, null);
            }
        }

        private static void OnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            Stop();
        }

        public static void Stop()
        {
            stopEvent.Set();
        }
    }
}
