using Microsoft.Win32;
using System.Configuration;
using System.Data;
using System.Windows;

namespace ReCall___
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static void SetStartup ( bool enable )
        {

            string appName = "ReCall";
            string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            if (enable)
                key.SetValue(appName, $"\"{exePath}\"");
            else
                key.DeleteValue(appName, false);
        }
    }

}

