using MiniConsole.Configuration;

namespace MiniConsole.Services
{
    public static class LogPathLocator
    {
        public static string? FindTriggerLogFile(AppSettings settings)
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            foreach (string folder in settings.TargetDataFolders)
            {
                string candidatePath = Path.Combine(appDataPath, folder, "data", "ss", "TriggerScript.log");
                if (File.Exists(candidatePath))
                {
                    return candidatePath;
                }
            }

            return null;
        }
    }
}
