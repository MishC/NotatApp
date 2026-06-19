using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace NotatApp.Models
{
public class DiaryPage
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int DiaryEntryId { get; set; }

    [ForeignKey(nameof(DiaryEntryId))]
    public DiaryEntry DiaryEntry { get; set; } = default!;

    [Range(1, 100)]
    public int PageNumber { get; set; } = 1;

    [StringLength(20000)]
    public string? Content { get; set; }

    [StringLength(500)]
    public string? ImagePath { get; set; }

    [StringLength(100)]
    public string? ImageContentType { get; set; }

    [StringLength(255)]
    public string? ImageFileName { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public DateTime? ImageUploadedAt { get; set; }
}

public class CreateDiaryPageDto
{
    [Range(1, 100)]
    public int PageNumber { get; set; }

    [StringLength(20000)]
    public string? Content { get; set; }

    public IFormFile? Image { get; set; }
}

public class UpdateDiaryPageDto
{
    [StringLength(20000)]
    public string? Content { get; set; }

    public IFormFile? Image { get; set; }

    public bool RemoveImage { get; set; } = false;
}
}