using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using MiniConsole.Views;

namespace MiniConsole.Configuration
{
    public static class ConfigLoader
    {
        private const string ConfigFileName = "appsettings.json";

        public static AppSettings Load()
        {
            try
            {
                if (File.Exists(ConfigFileName))
                {
                    string json = File.ReadAllText(ConfigFileName);
                    return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
                }

                var defaultSettings = new AppSettings();
                var options = new JsonSerializerOptions { WriteIndented = true };
                File.WriteAllText(ConfigFileName, JsonSerializer.Serialize(defaultSettings, options));
                return defaultSettings;
            }
            catch (Exception ex)
            {
                ConsoleRenderer.PrintWarning($"Failed to load {ConfigFileName}, using defaults. Details: {ex.Message}");
                return new AppSettings();
            }
        }
    }
}
