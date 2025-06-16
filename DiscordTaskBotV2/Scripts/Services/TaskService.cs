using System.Text.Json;
using DiscordTaskBot.Models;

namespace DiscordTaskBot.Services
{
    public class TaskService
    {
        private const string FilePath = "tasks.json";
        private const string ArchiveFilePath = "tasksarchive.json";

        private Dictionary<string, TaskData> tasks = new();

        private readonly SemaphoreSlim _lock = new(1, 1);

        public async Task LoadTasksAsync()
        {
            await _lock.WaitAsync();
            try
            {
                if (!File.Exists(FilePath))
                    return;

                var json = await File.ReadAllTextAsync(FilePath);
                tasks = JsonSerializer.Deserialize<Dictionary<string, TaskData>>(json) ?? new();
            }
            finally
            {
                _lock.Release();
            }
        }

        private async Task SaveTasks()
        {
            string json = JsonSerializer.Serialize(tasks, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            await File.WriteAllTextAsync(FilePath, json);
        }

        public async Task<string> AddTask(TaskData task)
        {
            await _lock.WaitAsync();
            try
            {
                var id = Guid.NewGuid().ToString();

                tasks.Add(id, task);
                await SaveTasks();

                return id;
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task RemoveTask(string taskID)
        {
            await _lock.WaitAsync();
            try
            {
                if (tasks.Remove(taskID)) await SaveTasks();
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task IncreaseTaskState(string taskID)
        {
            await _lock.WaitAsync();
            try
            {
                if (!tasks.ContainsKey(taskID))
                    return;

                if (tasks[taskID].State < Enum.GetValues<TaskStates>().Max())
                {
                    tasks[taskID].State += 1;
                    await SaveTasks();
                }
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task<Dictionary<string, TaskData>> GetAllTasksAsync()
        {
            await _lock.WaitAsync();
            try
            {
                return new Dictionary<string, TaskData>(tasks); // kopia, żeby nie wyciekał stan wewnętrzny
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task<TaskData?> GetTaskByIDAsync(string taskID)
        {
            await _lock.WaitAsync();
            try
            {
                tasks.TryGetValue(taskID, out var result);
                return result;
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task UpdateTaskLocationAsync(string taskID, ulong newChannelID, ulong newMessageID)
        {
            await _lock.WaitAsync();
            try
            {
                if (tasks.TryGetValue(taskID, out var task))
                {
                    task.ChannelID = newChannelID;
                    task.MessageID = newMessageID;
                    await SaveTasks();
                }
            }
            finally { _lock.Release(); }
        }
    }
}