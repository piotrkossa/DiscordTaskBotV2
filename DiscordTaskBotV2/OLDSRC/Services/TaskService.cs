using LiteDB.Async;
using DiscordTaskBot.Models;

namespace DiscordTaskBot.Services
{
    public class TaskService
    {
        private readonly LiteDatabaseAsync _liteDatabase;
        private readonly ILiteCollectionAsync<TaskData> _taskCollection;

        public TaskService()
        {
            _liteDatabase = new LiteDatabaseAsync(@"tasks.db");
            _taskCollection = _liteDatabase.GetCollection<TaskData>("tasks");
            _taskCollection.EnsureIndexAsync(x => x.ID).Wait();
        }
        

        public async Task<string> AddTaskAsync(TaskData task)
        {
            await _taskCollection.UpsertAsync(task);
            return task.ID;
        }

        public async Task RemoveTaskAsync(string taskID)
        {
            await _taskCollection.DeleteAsync(taskID);
        }

        public async Task IncreaseTaskStateAsync(string taskID)
        {
            TaskData taskData = await _taskCollection.FindByIdAsync(taskID);
            if (taskData == null) return;

            var maxState = Enum.GetValues<TaskStates>().Max();
            if (taskData.State < maxState)
            {
                taskData.State += 1;
                await _taskCollection.UpdateAsync(taskData);
                Console.WriteLine((int)taskData.State);
            }
        }

        public async Task<Dictionary<string, TaskData>> GetAllTasksAsync()
        {
            var tasks = await _taskCollection.FindAllAsync();
            return tasks.ToDictionary(t => t.ID, t => t);
        }

        public async Task<TaskData?> GetTaskByIDAsync(string taskID)
        {
            var id = await _taskCollection.FindByIdAsync(taskID);
            return id;
        }

        public async Task UpdateTaskLocationAsync(string taskID, ulong newChannelID, ulong newMessageID)
        {
            var task = await _taskCollection.FindByIdAsync(taskID);
            if (task != null)
            {
                task.ChannelID = newChannelID;
                task.MessageID = newMessageID;
                await _taskCollection.UpdateAsync(task);

                await MoveTaskToArchiveAsync(taskID);
            }
        }

        private async Task MoveTaskToArchiveAsync(string taskID)
        {
            var task = await _taskCollection.FindByIdAsync(taskID);
            if (task == null) return;

            var archiveCollection = _liteDatabase.GetCollection<TaskData>("tasks_archive");

            await archiveCollection.UpsertAsync(task);
            await _taskCollection.DeleteAsync(taskID);
        }

        public async Task AddDaysToTaskAsync(string taskID, int days)
        {
            var task = await _taskCollection.FindByIdAsync(taskID);
            if (task == null) return;

            task.CompletionDate = task.CompletionDate.AddDays(days);
            await _taskCollection.UpdateAsync(task);
        }
    }
}
