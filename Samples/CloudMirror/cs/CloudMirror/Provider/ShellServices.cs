using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage.Provider;
using Windows.Win32;
using Windows.Win32.System.Com;

namespace CloudMirror
{
    sealed class ShellServices
    {
        public static void InitAndStartServiceTask()
        {
            Task.Run(() =>
            {
                RegisterClass<ThumbnailProvider>();
                RegisterClass<TestExplorerCommandHandler>();
                RegisterWinRTClass<CustomStateProvider>(new Dictionary<Guid, Func<object, IntPtr>>()
                {
                    { 
                        typeof(IStorageProviderItemPropertySource).GUID,
                        (obj) => WinRT.MarshalInterface<IStorageProviderItemPropertySource>.FromManaged((IStorageProviderItemPropertySource)obj)
                    }
                });
                RegisterWinRTClass<UriSource>(new Dictionary<Guid, Func<object, IntPtr>>()
                {
                    {
                        typeof(IStorageProviderUriSource).GUID,
                        (obj) => WinRT.MarshalInterface<IStorageProviderUriSource>.FromManaged((IStorageProviderUriSource)obj)
                    }
                });

                AutoResetEvent dummyEvent = new AutoResetEvent(false);
                dummyEvent.WaitOne();
            });
        }

        private static void RegisterClass<T>() where T : new()
        {
            RegisterClassObject(typeof(T).GUID, new BasicClassFactory<T>());
        }

        private static void RegisterWinRTClass<T>(Dictionary<Guid, Func<object, IntPtr>> marshalFuncByGuid) where T : new()
        {
            RegisterClassObject(typeof(T).GUID, new WinRTClassFactory<T>(marshalFuncByGuid));
        }

        private static void RegisterClassObject(Guid clsid, object factory)
        {
            int hr = PInvoke.CoRegisterClassObject(in clsid, factory, CLSCTX.CLSCTX_LOCAL_SERVER, (int)REGCLS.REGCLS_MULTIPLEUSE, out uint _);
            if (hr < 0)
            {
                Marshal.ThrowExceptionForHR(hr);
            }
        }
    }
}
