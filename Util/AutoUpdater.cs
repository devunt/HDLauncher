using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HDLauncher
{
    internal static class AutoUpdater
    {
        public static async Task Update()
        {

            var updaterWindow = new UpdaterWindow();

            try
            {
                var tempdir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Constants.APPNAME, Constants.UPDATE_TEMP_DIRPATH);
                var batchpath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Constants.APPNAME, "update.bat");

                if (Directory.Exists(tempdir))
                {
                    Directory.Delete(tempdir, true);
                }
                Directory.CreateDirectory(tempdir);

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders
                        .TryAddWithoutValidation("User-Agent", Constants.LAUNCHER_USER_AGENT);

                    var json = await client.GetJson($"https://api.github.com/repos/{Constants.GITHUB_REPO}/releases/latest");
                    decimal version = Convert.ToDecimal(json.tag_name.ToObject<string>().Substring(1));

                    if (version <= Constants.VERSION)
                        return;

                    updaterWindow.Show();

                    string url = null;
                    foreach (var asset in json.assets)
                    {
                        if (asset.name == $"HDLauncher.v{version}.zip")
                        {
                            url = asset.browser_download_url;
                            break;
                        }
                    }

                    Sentry.Report("Update started");

                    var exepath = Process.GetCurrentProcess().MainModule.FileName;

                    var response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();

                    var stream = await response.Content.ReadAsStreamAsync();
                    using (var zip = ZipStorer.Open(stream, FileAccess.Read))
                    {
                        var dir = zip.ReadCentralDir();
                        foreach (var entry in dir)
                        {
                            var filename = entry.FilenameInZip;
                            if (filename == "HDLauncher.exe")
                            {
                                filename = Path.GetFileName(exepath);
                            }

                            zip.ExtractFile(entry, Path.Combine(tempdir, filename));
                        }
                    }

                    var currentdir = Path.GetDirectoryName(exepath);

                    File.WriteAllText(batchpath,
                        "@echo off\r\n" +
                        "title HDLauncher Updater\r\n" +
                        "echo Updating HDLauncher...\r\n" +
                        "ping 127.0.0.1 -n 3 > nul\r\n" +
                        $"move /y \"{tempdir}\\*\" \"{currentdir}\" > nul\r\n" +
                        $"\"{exepath}\"\r\n" +
                        "echo Running HDLauncher...\r\n",
                    Encoding.Default);

                    var si = new ProcessStartInfo();
                    si.FileName = batchpath;
                    si.CreateNoWindow = true;
                    si.UseShellExecute = false;
                    si.WindowStyle = ProcessWindowStyle.Hidden;

                    Process.Start(si);

                    Application.Current.Shutdown();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("업데이트 도중 문제 발생!\n\n" + ex, "에러!", MessageBoxButton.OK, MessageBoxImage.Error);
                Sentry.ReportAsync(ex, new { Action = "On Update" });
            }
            finally
            {
                updaterWindow.Close();
            }
        }
    }
}