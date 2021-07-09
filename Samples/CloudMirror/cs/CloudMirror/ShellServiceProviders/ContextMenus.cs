using System;
using System.Runtime.InteropServices;
using Windows.Storage;
using Windows.Storage.Provider;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.System.Com;
using Windows.Win32.UI.Shell;

namespace CloudMirror
{
    [ComVisible(true)]
    [Guid("BAF1DD7E-1878-4737-BA01-885DF393101C")]
    class TestExplorerCommandHandler : IExplorerCommand
    {
        private const string Title = "TestCommand";

        public unsafe void GetTitle(IShellItemArray psiItemArray, PWSTR* ppszName)
        {
            fixed (char* title = Title)
            {
                *ppszName = new PWSTR((char*)Marshal.StringToCoTaskMemUni(Title));
            }
        }

        public unsafe void GetState(IShellItemArray psiItemArray, BOOL fOkToBeSlow, uint* pCmdState)
        {
            *pCmdState = (uint)_EXPCMDSTATE.ECS_ENABLED;
        }

        public async void Invoke(IShellItemArray psiItemArray, IBindCtx pbc)
        {
            Console.WriteLine("Cloud Provider Command received");

            //
            // Set a new custom state on the selected files
            //
            StorageProviderItemProperty[] customProperties = new StorageProviderItemProperty[]
            {
                new StorageProviderItemProperty()
                {
                    Id = 3,
                    Value = "Value3",
                    // This icon is just for the sample. You should provide your own branded icon here
                    IconResource = "shell32.dll,-259"
                }
            };

            uint count;
            unsafe
            {
                psiItemArray.GetCount(&count);
            }

            for (uint i = 0; i < count; i++)
            {
                IShellItem shellItem;
                psiItemArray.GetItemAt(i, out shellItem);

                PWSTR fullPath;
                unsafe
                {
                    shellItem.GetDisplayName(SIGDN.SIGDN_FILESYSPATH, &fullPath);
                }

                StorageFile item = await StorageFile.GetFileFromPathAsync(fullPath.ToString());
                await StorageProviderItemProperties.SetAsync(item, customProperties);

                unsafe
                {
                    PInvoke.SHChangeNotify(SHCNE_ID.SHCNE_UPDATEITEM, SHCNF_FLAGS.SHCNF_PATH, fullPath.Value, null);
                }
            }
        }

        public unsafe void GetFlags(uint* pFlags)
        {
            *pFlags = (uint)_EXPCMDFLAGS.ECF_DEFAULT;
        }

        public unsafe void GetIcon(IShellItemArray psiItemArray, PWSTR* ppszIcon) => throw new NotImplementedException();
        public unsafe void GetToolTip(IShellItemArray psiItemArray, PWSTR* ppszInfotip) => throw new NotImplementedException();
        public unsafe void GetCanonicalName(Guid* pguidCommandName) => throw new NotImplementedException();
        public void EnumSubCommands(out IEnumExplorerCommand ppEnum) => throw new NotImplementedException();
    }
}
