using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace NotatApp.Models
{
public class TaskItem
{
    [Key]
    public int Id { get; set; }

    [Required, StringLength(100, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Content { get; set; }

    public DateTime StartTimeUtc { get; set; }
    public DateTime EndTimeUtc { get; set; }

    public bool IsDone { get; set; } = false;

    public bool IsArchived { get; set; } = false;

    [Required]
    [ForeignKey("User")]
    public string UserId { get; set; } = default!;

    [ValidateNever]
    [JsonIgnore]
    public User User { get; set; } = default!;
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
