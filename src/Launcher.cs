using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Windows.Forms;

internal static class Launcher
{
    [STAThread]
    private static int Main()
    {
        try
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string runtimeDir = Path.Combine(appData, "Manoonchai Keyboard Portable", "runtime");
            Directory.CreateDirectory(runtimeDir);

            ExtractPayload(runtimeDir);

            string ahk = Path.Combine(runtimeDir, "AutoHotkeyA32.exe");
            string script = Path.Combine(runtimeDir, "pkl.ahk");

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = ahk,
                Arguments = Quote(script),
                WorkingDirectory = runtimeDir,
                UseShellExecute = false
            };
            Process.Start(startInfo);
            return 0;
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Manoonchai Keyboard Portable could not start.\r\n\r\n" + ex.Message,
                "Manoonchai Keyboard Portable",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return 1;
        }
    }

    private static void ExtractPayload(string runtimeDir)
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        using (Stream stream = assembly.GetManifestResourceStream("Manoonchai.Payload.zip"))
        {
            if (stream == null)
                throw new InvalidOperationException("Embedded payload was not found.");

            using (ZipArchive archive = new ZipArchive(stream, ZipArchiveMode.Read))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    string destination = Path.GetFullPath(Path.Combine(runtimeDir, entry.FullName));
                    if (!destination.StartsWith(runtimeDir, StringComparison.OrdinalIgnoreCase))
                        continue;

                    if (string.IsNullOrEmpty(entry.Name))
                    {
                        Directory.CreateDirectory(destination);
                        continue;
                    }

                    string directory = Path.GetDirectoryName(destination);
                    if (!string.IsNullOrEmpty(directory))
                        Directory.CreateDirectory(directory);

                    entry.ExtractToFile(destination, true);
                }
            }
        }
    }

    private static string Quote(string value)
    {
        return "\"" + value.Replace("\"", "\\\"") + "\"";
    }
}
