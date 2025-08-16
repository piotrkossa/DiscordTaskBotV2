namespace DiscordTaskBot.Configuration
{
    public static class EnvLoader
    {
        public static void LoadEnv(string envPath)
        {
            if (!File.Exists(envPath))
            {
                Console.Error.Write(".env File Not Found!");
                return;
            }

            foreach (var line in File.ReadAllLines(envPath))
            {
                var parts = line.Split('=', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length != 2) continue;

                Environment.SetEnvironmentVariable(parts[0], parts[1]);
            }
        }
    }
}