using LiteDB;
using DiscordTaskBot.Models;

namespace DiscordTaskBot.Services
{
    public class TaskService
    {
        private readonly LiteDatabase _liteDatabase;
        private readonly ILiteCollection<TaskData> _taskCollection;

        public TaskService()
        {
            _liteDatabase = new LiteDatabase(@"tasks.db");
            _taskCollection = _liteDatabase.GetCollection<TaskData>("tasks");
            _taskCollection.EnsureIndex(x => x.ID);
        }
        

        public string AddTask(TaskData task)
        {
            _taskCollection.Upsert(task);
            return task.ID;
        }

        public void RemoveTask(string taskID)
        {
            _taskCollection.Delete(taskID);
        }

        public void IncreaseTaskState(string taskID)
        {
            var task = _taskCollection.FindById(taskID);
            if (task == null) return;

            var maxState = Enum.GetValues<TaskStates>().Max();
            if (task.State < maxState)
            {
                task.State += 1;
                _taskCollection.Update(task);
                Console.WriteLine((int)task.State);
            }
        }

        public Dictionary<string, TaskData> GetAllTasks()
        {
            var tasks = _taskCollection.FindAll().ToDictionary(t => t.ID, t => t);
            return tasks;
        }

        public TaskData? GetTaskByID(string taskID)
        {
            return _taskCollection.FindById(taskID);
        }

        public void UpdateTaskLocation(string taskID, ulong newChannelID, ulong newMessageID)
        {
            var task = _taskCollection.FindById(taskID);
            if (task != null)
            {
                task.ChannelID = newChannelID;
                task.MessageID = newMessageID;
                _taskCollection.Update(task);

                MoveTaskToArchive(taskID);
            }
        }

        private void MoveTaskToArchive(string taskID)
        {
            var task = _taskCollection.FindById(taskID);
            if (task == null) return;

            var archiveCollection = _liteDatabase.GetCollection<TaskData>("tasks_archive");

            archiveCollection.Upsert(task);
            _taskCollection.Delete(taskID);
        }

        public void AddDaysToTask(string taskID, int days)
        {
            var task = _taskCollection.FindById(taskID);
            if (task == null) return;

            task.CompletionDate = task.CompletionDate.AddDays(days);
            _taskCollection.Update(task);
        }
    }
}
