using System.Text.Json;
using Discord;
using Discord.WebSocket;
using Quartz;
using Quartz.Impl.Matchers;

public class JobExecutionTrackerService
{
    private readonly DiscordSocketClient _client;
    private readonly IScheduler _scheduler;
    private Dictionary<string, DateTime> _lastRunTimes;
    private readonly string _filePath = "jobExecution.json";

    private readonly object _fileLock = new();

    public JobExecutionTrackerService(DiscordSocketClient client, ISchedulerFactory schedulerFactory)
    {
        _client = client;

        _scheduler = schedulerFactory.GetScheduler().Result;
        _scheduler.Start().Wait();

        if (File.Exists(_filePath))
        {
            var json = File.ReadAllText(_filePath);
            _lastRunTimes = JsonSerializer.Deserialize<Dictionary<string, DateTime>>(json) ?? new();
        }
        else
        {
            _lastRunTimes = new();
        }

        _client.Ready += OnClientReadyAsync;

        if (_client.ConnectionState == ConnectionState.Connected)
        {
            _ = OnClientReadyAsync();
        }
    }

    public bool WasRunWithin(string jobName, TimeSpan timeSpan)
    {
        if (_lastRunTimes.TryGetValue(jobName, out var lastRun))
        {
            return lastRun > DateTime.Now - timeSpan;
        }
        return false;
    }

    public void MarkAsRun(string jobName)
    {
        lock (_fileLock)
        {
            _lastRunTimes[jobName] = DateTime.Now;
            var json = JsonSerializer.Serialize(_lastRunTimes, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, json);
        }
    }

    private async Task OnClientReadyAsync()
    {
        foreach (var jobKey in await _scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup()))
        {
            var jobName = jobKey.Name;
            if (!WasRunWithin(jobName, TimeSpan.FromHours(24)))
            {
                Console.WriteLine($"Triggering job '{jobName}' via Quartz scheduler...");
                await _scheduler.TriggerJob(jobKey);
                MarkAsRun(jobName);
            }
        }
    }
}