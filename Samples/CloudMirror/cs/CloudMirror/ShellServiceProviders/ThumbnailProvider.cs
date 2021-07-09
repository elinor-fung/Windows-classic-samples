using System;
using System.IO;
using System.Runtime.InteropServices;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.Shell;

namespace CloudMirror
{
    [ComVisible(true)]
    [Guid("C6438A70-67D4-4ADA-A399-039B867C58FB")]
    [ComDefaultInterface(typeof(IThumbnailProvider))]
    sealed class ThumbnailProvider : IInitializeWithItem, IThumbnailProvider
    {
        private string _targetPath;
        private string _sourcePath;

        public unsafe void Initialize(IShellItem psi, uint grfMode)
        {
            PWSTR filePath;
            psi.GetDisplayName(SIGDN.SIGDN_FILESYSPATH, &filePath);
            Console.WriteLine($"Initializing thumbnail provider for '{filePath}'");

            _targetPath = filePath.ToString();
            string relativePath = Path.GetRelativePath(ProviderFolderLocations.ClientFolder, _targetPath);
            _sourcePath = Path.Combine(ProviderFolderLocations.ServerFolder, relativePath);
        }

        public unsafe void GetThumbnail(uint cx, HBITMAP* phbmp, WTS_ALPHATYPE* pdwAlpha)
        {
            *phbmp = default;
            *pdwAlpha = WTS_ALPHATYPE.WTSAT_UNKNOWN;
            Console.WriteLine($"Getting thumbnail for target '{_targetPath}', source '{_sourcePath}'");
        }
    }
}
