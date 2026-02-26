using NotatApp.Models;

namespace NotatApp.Services
{
    public interface ITaskItemService
    {
        Task<List<TaskItem>> GetAllTasksAsync(string userId);
        Task<List<TaskItem>> GetPendingTasksAsync(string userId);
        Task<List<TaskItem>> GetDoneTasksAsync(string userId);
        Task<List<TaskItem>> GetOverdueTasksAsync(string userId);

        Task<TaskItem?> GetTaskByIdAsync(int id, string userId);

        Task<TaskItem> CreateTaskAsync(CreateTaskDto dto, string userId);
        Task<bool> UpdateTaskAsync(int id, UpdateTaskDto dto, string userId);
        Task<bool> DeleteTaskAsync(int id, string userId);

        Task<bool> IsTaskOverdueAsync(int id, string userId);
    }
}