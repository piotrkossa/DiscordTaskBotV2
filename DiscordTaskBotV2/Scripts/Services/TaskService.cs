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
                if (!tasks.TryGetValue(taskID, out TaskData? value))
                    return;

                if (value.State < Enum.GetValues<TaskStates>().Max())
                {
                    value.State += 1;
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
                    await MoveTaskToArchive(taskID);
                }
            }
            finally { _lock.Release(); }
        }

        private async Task MoveTaskToArchive(string taskID)
        {
            if (!tasks.ContainsKey(taskID)) return;

            Dictionary<string, TaskData> archivedTasks = new();

            string json;

            if (File.Exists(ArchiveFilePath))
            {
                json = await File.ReadAllTextAsync(ArchiveFilePath);
                archivedTasks = JsonSerializer.Deserialize<Dictionary<string, TaskData>>(json) ?? new();
            }

            archivedTasks.Add(taskID, tasks[taskID]);

            json = JsonSerializer.Serialize(archivedTasks, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            await File.WriteAllTextAsync(ArchiveFilePath, json);

            tasks.Remove(taskID);

            await SaveTasks();
        }

        public async Task AddDaysToTask(string taskID, int days)
        {
            await _lock.WaitAsync();
            try
            {
                if (tasks.ContainsKey(taskID))
                {
                    tasks[taskID].CompletionDate = tasks[taskID].CompletionDate.AddDays(days);
                }
            }
            finally { _lock.Release(); }
        }
    }
}