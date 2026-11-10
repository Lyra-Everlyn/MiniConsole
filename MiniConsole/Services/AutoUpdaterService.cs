using MiniConsole.Views;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MiniConsole.Services;

public static class AutoUpdaterService
{
    private const string GithubRepo = "Lyra-Everlyn/MiniConsole";
    private static readonly string CurrentVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "1.0.0";

    public static async Task CheckAndApplyUpdateAsync()
    {
        CleanUpOldVersion();

        string? downloadUrl = null;
        string cleanLatestVer = "";

        try
        {
            using var checkClient = new HttpClient();
            checkClient.DefaultRequestHeaders.Add("User-Agent", "MiniConsole-Updater");
            checkClient.Timeout = TimeSpan.FromSeconds(5);

            string apiUrl = $"https://api.github.com/repos/{GithubRepo}/releases/latest";
            string response = await checkClient.GetStringAsync(apiUrl);

            using var doc = JsonDocument.Parse(response);
            var root = doc.RootElement;

            string latestTag = root.GetProperty("tag_name").GetString() ?? "";
            cleanLatestVer = latestTag.TrimStart('v', 'V');

            if (!IsNewerVersion(cleanLatestVer, CurrentVersion))
            {
                return;
            }

            if (root.TryGetProperty("assets", out var assets) && assets.GetArrayLength() > 0)
            {
                foreach (var asset in assets.EnumerateArray())
                {
                    string name = asset.GetProperty("name").GetString() ?? "";
                    if (name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                    {
                        downloadUrl = asset.GetProperty("browser_download_url").GetString();
                        break;
                    }
                }
            }
        }
        catch
        {
            return;
        }

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"[UPDATE AVAILABLE] New version v{cleanLatestVer} is ready! (Current: v{CurrentVersion})");
        Console.WriteLine("Press [Y] to update automatically now, or any other key to skip.");
        Console.ResetColor();

        var key = Console.ReadKey(true).Key;
        if (key == ConsoleKey.Y)
        {
            if (string.IsNullOrEmpty(downloadUrl))
            {
                ConsoleRenderer.PrintError("No .exe file found in the latest GitHub Release assets.");
                return;
            }

            try
            {
                using var downloadClient = new HttpClient();
                downloadClient.DefaultRequestHeaders.Add("User-Agent", "MiniConsole-Updater");
                downloadClient.Timeout = TimeSpan.FromMinutes(10);

                await DownloadAndReplaceExecutableAsync(downloadClient, downloadUrl);
            }
            catch (Exception ex)
            {
                ConsoleRenderer.PrintError($"Update failed: {ex.Message}");
                Thread.Sleep(2000);
            }
        }
    }

    private static async Task DownloadAndReplaceExecutableAsync(HttpClient client, string downloadUrl)
    {
        string? currentExePath = Environment.ProcessPath;
        if (string.IsNullOrEmpty(currentExePath)) return;

        string newExePath = currentExePath + ".new";
        string oldExePath = currentExePath + ".old";

        using (var response = await client.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead))
        {
            response.EnsureSuccessStatusCode();
            long? totalBytes = response.Content.Headers.ContentLength;

            await using var contentStream = await response.Content.ReadAsStreamAsync();
            await using var fileStream = new FileStream(newExePath, FileMode.Create, FileAccess.Write, FileShare.None, 65536, true);

            byte[] buffer = new byte[65536];
            long totalRead = 0;
            int bytesRead;

            while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                await fileStream.WriteAsync(buffer, 0, bytesRead);
                totalRead += bytesRead;

                if (totalBytes.HasValue && totalBytes.Value > 0)
                {
                    double percent = (double)totalRead / totalBytes.Value * 100.0;
                    double readMb = totalRead / 1048576.0;
                    double totalMb = totalBytes.Value / 1048576.0;

                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write($"\r[Updater] Downloading: {percent,6:F2}% ({readMb:F1} MB / {totalMb:F1} MB)   ");
                    Console.ResetColor();
                }
            }
        }

        Console.WriteLine();
        ConsoleRenderer.PrintSystemEvent("[Updater] Applying update and restarting...", ConsoleColor.Green);

        if (File.Exists(oldExePath)) File.Delete(oldExePath);

        File.Move(currentExePath, oldExePath);
        File.Move(newExePath, currentExePath);

        Process.Start(new ProcessStartInfo
        {
            FileName = currentExePath,
            WorkingDirectory = Path.GetDirectoryName(currentExePath) ?? "",
            UseShellExecute = true
        });

        Environment.Exit(0);
    }

    private static void CleanUpOldVersion()
    {
        try
        {
            string? currentExePath = Environment.ProcessPath;
            if (!string.IsNullOrEmpty(currentExePath))
            {
                string oldExePath = currentExePath + ".old";
                if (File.Exists(oldExePath))
                {
                    File.Delete(oldExePath);
                }
            }
        }
        catch { }
    }

    private static bool IsNewerVersion(string latest, string current)
    {
        if (Version.TryParse(latest, out var vLatest) && Version.TryParse(current, out var vCurrent))
        {
            return vLatest > vCurrent;
        }
        return false;
    }
}