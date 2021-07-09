using System;
using System.IO;

namespace CloudMirror
{
    class ProviderFolderLocations
    {
        public static string ClientFolder { get; private set; }
        public static string ServerFolder { get; private set; }

        public static bool Init()
        {
            ServerFolder = PromptForFolderPath("\"Server in the Fluffy Cloud\" Location");
            if (string.IsNullOrEmpty(ServerFolder))
                return false;

            ClientFolder = PromptForFolderPath("\"Syncroot (Client)\" Location");
            if (string.IsNullOrEmpty(ClientFolder))
                return false;

            return true;
        }

        private static string PromptForFolderPath(string title)
        {
            Console.WriteLine($"Enter {title}:");

            // Restore last location used
            Windows.Foundation.Collections.IPropertySet settings = Windows.Storage.ApplicationData.Current.LocalSettings.Values;
            if (settings.TryGetValue(title, out object lastLocationMaybe))
            {
                Console.WriteLine($"  Last used location: {lastLocationMaybe}");
                return (string)lastLocationMaybe;
            }

            string location = Console.ReadLine();
            if (!Directory.Exists(location))
            {
                Console.WriteLine("Directory does not exist.");
                return string.Empty;
            }

            // Save the last location
            settings[title] = location;
            return location;
        }
    }
}
