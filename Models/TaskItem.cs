using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace NotatApp.Models
{
public class TaskItem
{
    [Key]
    public int Id { get; set; }

    [Required, StringLength(500, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Content { get; set; }

    public DateTime StartTimeUtc { get; set; }
    public DateTime EndTimeUtc { get; set; }

    public bool IsDone { get; set; } = false;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }

    [Required]
    [ForeignKey(nameof(User))]
    public string UserId { get; set; } = default!;
}

public class CreateTaskDto
{
    [Required, StringLength(100, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;

    [StringLength(20)]
    public string? Content { get; set; }

    [Required]
    public DateTime StartTimeUtc { get; set; }

    [Required]
    public DateTime EndTimeUtc { get; set; }

    public DateTime? CreatedAtUtc { get; set; } = DateTime.UtcNow;
}

public class UpdateTaskDto
{
    public string? Title { get; set; }
    public string? Content { get; set; }
    public bool? IsDone { get; set; }

    public DateTime? StartTimeUtc { get; set; }
    public DateTime? EndTimeUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
}
