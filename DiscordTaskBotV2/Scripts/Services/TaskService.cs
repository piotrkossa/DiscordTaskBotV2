using LiteDB;
using DiscordTaskBot.Models;

namespace DiscordTaskBot.Services
{
    public class TaskService
    {
        private readonly LiteDatabase _liteDatabase;
        private readonly ILiteCollection<TaskData> _taskCollection;
        private readonly SemaphoreSlim _lock = new(1, 1);

        public TaskService()
        {
            _liteDatabase = new LiteDatabase(@"tasks.db");
            _taskCollection = _liteDatabase.GetCollection<TaskData>("tasks");
            _taskCollection.EnsureIndex(x => x.ID);
        }

        public async Task LoadTasksAsync()
        {
            await Task.CompletedTask;
        }

        private async Task SaveTaskAsync(TaskData task)
        {
            await _lock.WaitAsync();
            try
            {
                _taskCollection.Upsert(task);
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task<string> AddTask(TaskData task)
        {
            await _lock.WaitAsync();
            try
            {
                _taskCollection.Upsert(task);
                return task.ID;
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
                _taskCollection.Delete(taskID);
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
                var task = _taskCollection.FindById(taskID);
                if (task == null) return;

                var maxState = Enum.GetValues<TaskStates>().Max();
                if (task.State < maxState)
                {
                    task.State += 1;
                    _taskCollection.Update(task);
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
                var tasks = _taskCollection.FindAll().ToDictionary(t => t.ID, t => t);
                return tasks;
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
                return _taskCollection.FindById(taskID);
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
                var task = _taskCollection.FindById(taskID);
                if (task != null)
                {
                    task.ChannelID = newChannelID;
                    task.MessageID = newMessageID;
                    _taskCollection.Update(task);

                    await MoveTaskToArchive(taskID);
                }
            }
            finally
            {
                _lock.Release();
            }
        }

        private async Task MoveTaskToArchive(string taskID)
        {
            await _lock.WaitAsync();
            try
            {
                var task = _taskCollection.FindById(taskID);
                if (task == null) return;

                var archiveCollection = _liteDatabase.GetCollection<TaskData>("tasks_archive");

                archiveCollection.Upsert(task);
                _taskCollection.Delete(taskID);
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task AddDaysToTask(string taskID, int days)
        {
            await _lock.WaitAsync();
            try
            {
                var task = _taskCollection.FindById(taskID);
                if (task == null) return;

                task.CompletionDate = task.CompletionDate.AddDays(days);
                _taskCollection.Update(task);
            }
            finally
            {
                _lock.Release();
            }
        }
    }
}
