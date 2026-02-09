using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace NotatApp.Models
{
public class TaskItem
{
    [Key]
    public int Id { get; set; }

    [Required, StringLength(100, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;

    [StringLength(50)]
    public string? Content { get; set; }

    public DateTime StartTimeUtc { get; set; }
    public DateTime EndTimeUtc { get; set; }

    public bool IsDone { get; set; } = false;

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
}

public class UpdateTaskDto
{
    public string? Title { get; set; }
    public string? Content { get; set; }
    public bool? IsDone { get; set; }

    public DateTime? StartTimeUtc { get; set; }
    public DateTime? EndTimeUtc { get; set; }
}
}
