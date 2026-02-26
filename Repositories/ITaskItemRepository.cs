using NotatApp.Models;

namespace NotatApp.Repositories
{
    public interface ITaskItemRepository
    {
        Task<List<TaskItem>> GetUserTasksAsync(string userId);
        Task<TaskItem?> GetTaskByIdAsync(int id, string userId);

        Task AddTaskAsync(TaskItem task);
        Task UpdateTaskAsync(TaskItem task);
        Task DeleteTaskAsync(TaskItem task);
    }
}