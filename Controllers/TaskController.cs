using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotatApp.Models;
using NotatApp.Services;
using NotatApp.Extensions;

namespace NotatApp.Controllers
{
    [ApiController]
    [Route("api/tasks")]
    [Authorize]
    public class TaskController : ControllerBase
    {
        private readonly ITaskItemService _taskService;

        public TaskController(ITaskItemService taskService)
        {
            _taskService = taskService;
        }

        [HttpGet("health")]
        [AllowAnonymous]
        public ActionResult<string> HealthCheck()
            => Ok("Task Service is running.");

        // GET /api/tasks
        [HttpGet]
        public async Task<ActionResult> GetAllTasks()
        {
            var tasks = await _taskService.GetAllTasksAsync(User.GetUserId());
            return Ok(tasks);
        }

        // GET /api/tasks/pending
        [HttpGet("pending")]
        public async Task<ActionResult> GetPendingTasks()
        {
            var tasks = await _taskService.GetPendingTasksAsync(User.GetUserId());
            return Ok(tasks);
        }

        // GET /api/tasks/done
        [HttpGet("done")]
        public async Task<ActionResult> GetDoneTasks()
        {
            var tasks = await _taskService.GetDoneTasksAsync(User.GetUserId());
            return Ok(tasks);
        }

        // GET /api/tasks/overdues
        [HttpGet("overdues")]
        public async Task<ActionResult> GetOverdueTasks()
        {
            var tasks = await _taskService.GetOverdueTasksAsync(User.GetUserId());
            return Ok(tasks);
        }

        // GET /api/tasks/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult> GetTaskById(int id)
        {
            var task = await _taskService.GetTaskByIdAsync(id, User.GetUserId());

            if (task is null)
                return NotFound();

            return Ok(task);
        }

        // GET /api/tasks/overdue/{id}
        [HttpGet("overdue/{id:int}")]
        public async Task<ActionResult<bool>> IsOverdue(int id)
        {
            var result = await _taskService.IsTaskOverdueAsync(id, User.GetUserId());
            return Ok(result);
        }

        // POST /api/tasks
        // body: { "title": "Task Title", "startTimeUtc": "...", "endTimeUtc": "...", "content": "..." }
        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] CreateTaskDto dto)
        {
            var userId = User.GetUserId();

            var task = await _taskService.CreateTaskAsync(dto, userId);
            return CreatedAtAction(nameof(GetTaskById), new { id = task.Id }, task);
        }

        // PUT /api/tasks/{id}
        // body: { "title": "Updated", "content": "...", "isDone": true, "startTimeUtc": "...", "endTimeUtc": "..." }
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateTask(int id, [FromBody] UpdateTaskDto dto)
        {
            var updated = await _taskService.UpdateTaskAsync(id, dto, User.GetUserId());

            if (!updated)
                return NotFound();

            return NoContent();
        }

        // DELETE /api/tasks/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var deleted = await _taskService.DeleteTaskAsync(id, User.GetUserId());

            if (!deleted)
                return NotFound();

            return NoContent();
        }
    }
}