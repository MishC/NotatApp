using NotatApp.Models;
using NotatApp.Repositories;

namespace NotatApp.Services
{
    public class TaskItemService : ITaskItemService
    {
        private readonly ITaskItemRepository _repository;

        public TaskItemService(ITaskItemRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<TaskItem>> GetAllTasksAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("userId is required", nameof(userId));

            var tasks = await _repository.GetUserTasksAsync(userId);

            // dáš si vlastné triedenie; tu napr. podľa start času
            return [.. tasks.OrderBy(t => t.StartTimeUtc)];
        }

        public async Task<List<TaskItem>> GetPendingTasksAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("userId is required", nameof(userId));

            var tasks = await _repository.GetUserTasksAsync(userId);

            return [.. tasks
                .Where(t => !t.IsDone)
                .OrderBy(t => t.StartTimeUtc)];
        }

        public async Task<List<TaskItem>> GetDoneTasksAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("userId is required", nameof(userId));

            var tasks = await _repository.GetUserTasksAsync(userId);

            return [.. tasks
                .Where(t => t.IsDone)
                .OrderByDescending(t => t.EndTimeUtc)];
        }

        public async Task<List<TaskItem>> GetOverdueTasksAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("userId is required", nameof(userId));

            var tasks = await _repository.GetUserTasksAsync(userId);

            var nowUtc = DateTime.UtcNow;

            return [.. tasks
                .Where(t => !t.IsDone && t.EndTimeUtc < nowUtc)
                .OrderBy(t => t.EndTimeUtc)];
        }

        public Task<TaskItem?> GetTaskByIdAsync(int id, string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("userId is required", nameof(userId));

            return _repository.GetTaskByIdAsync(id, userId);
        }

        public async Task<TaskItem> CreateTaskAsync(CreateTaskDto dto, string userId)
        {
            if (dto is null)
                throw new ArgumentNullException(nameof(dto));

            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("UserId is required.", nameof(userId));

            if (string.IsNullOrWhiteSpace(dto.Title))
                throw new ArgumentException("Title is required.", nameof(dto.Title));

            if (dto.Title.Length > 100)
                throw new ArgumentException("Title cannot be longer than 100 characters.", nameof(dto.Title));

            // (voliteľné) ak chceš rovnaké limity ako pri Note:
            if (dto.Content != null && dto.Content.Length > 20)
                throw new ArgumentException("Content cannot be longer than 20 characters.", nameof(dto.Content));

            if (dto.EndTimeUtc < dto.StartTimeUtc)
                throw new ArgumentException("EndTimeUtc must be >= StartTimeUtc.");

            var task = new TaskItem
            {
                Title = dto.Title,
                Content = dto.Content,
                StartTimeUtc = DateTime.SpecifyKind(dto.StartTimeUtc, DateTimeKind.Utc),
                EndTimeUtc = DateTime.SpecifyKind(dto.EndTimeUtc, DateTimeKind.Utc),
                IsDone = false,
                UserId = userId
            };

            await _repository.AddTaskAsync(task);
            return task;
        }

        public async Task<bool> UpdateTaskAsync(int id, UpdateTaskDto dto, string userId)
        {
            if (dto is null)
                throw new ArgumentNullException(nameof(dto));

            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("userId is required", nameof(userId));

            var task = await _repository.GetTaskByIdAsync(id, userId);
            if (task == null)
                return false;

            if (dto.Title != null)
            {
                if (string.IsNullOrWhiteSpace(dto.Title))
                    throw new ArgumentException("Title cannot be empty.", nameof(dto.Title));
                if (dto.Title.Length > 100)
                    throw new ArgumentException("Title cannot be longer than 100 characters.", nameof(dto.Title));

                task.Title = dto.Title;
            }

            if (dto.Content != null)
            {
                if (dto.Content.Length > 20)
                    throw new ArgumentException("Content cannot be longer than 20 characters.", nameof(dto.Content));

                task.Content = dto.Content;
            }

            if (dto.IsDone.HasValue)
                task.IsDone = dto.IsDone.Value;

            if (dto.StartTimeUtc.HasValue)
                task.StartTimeUtc = DateTime.SpecifyKind(dto.StartTimeUtc.Value, DateTimeKind.Utc);

            if (dto.EndTimeUtc.HasValue)
                task.EndTimeUtc = DateTime.SpecifyKind(dto.EndTimeUtc.Value, DateTimeKind.Utc);

            if (task.EndTimeUtc < task.StartTimeUtc)
                throw new ArgumentException("EndTimeUtc must be >= StartTimeUtc.");

            try
            {
                await _repository.UpdateTaskAsync(task);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while updating the task.", ex);
            }
        }

        public async Task<bool> DeleteTaskAsync(int id, string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                    throw new ArgumentException("userId is required", nameof(userId));

                var existing = await _repository.GetTaskByIdAsync(id, userId);
                if (existing == null)
                    return false;

                await _repository.DeleteTaskAsync(existing);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while deleting the task.", ex);
            }
        }

        public async Task<bool> IsTaskOverdueAsync(int id, string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("userId is required", nameof(userId));

            var task = await _repository.GetTaskByIdAsync(id, userId);
            if (task == null)
                return false;

            return !task.IsDone && task.EndTimeUtc < DateTime.UtcNow;
        }
    }
}