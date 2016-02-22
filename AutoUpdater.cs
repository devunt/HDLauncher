using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace HDLauncher
{
    static class AutoUpdater
    {
        public static async Task Update()
        {
            try
            {
                File.Delete(Constants.UPDATE_TEMP_FILENAME);

                using (var client = new HttpClient() { BaseAddress = new Uri(Constants.UPDATE_BASE_URL) })
                {
                    var response = await client.GetAsync(Constants.UPDATE_VERCHECK_URL);
                    var versionResp = await response.Content.ReadAsStringAsync();
                    var version = decimal.Parse(versionResp);
                    
                    if (version <= Constants.VERSION)
                    {
                        return;
                    }

                    UpdaterWindow updaterWindow = new UpdaterWindow();
                    updaterWindow.Show();
                    
                    var path = Process.GetCurrentProcess().MainModule.FileName;

                    File.Move(path, Constants.UPDATE_TEMP_FILENAME);

                    response = await client.GetAsync(Constants.UPDATE_FILE_DIR + version + Constants.UPDATE_FILE_EXT);

                    response.EnsureSuccessStatusCode();

                    var stream = await response.Content.ReadAsStreamAsync();

                    using (var fstream = File.Open(path, FileMode.Create))
                    {
                        stream.CopyTo(fstream);
                    }

                    updaterWindow.Close();                    

                    Process.Start(new ProcessStartInfo(path));
                    Application.Current.Shutdown();
                }
            }
            catch { }
        }
    }
}