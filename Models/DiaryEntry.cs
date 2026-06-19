using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace NotatApp.Models
{
  public class DiaryEntry
{
    [Key]
    public int Id { get; set; }

    [Required, StringLength(150)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public DateOnly Date { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    [Required]
    [ForeignKey(nameof(User))]
    public string UserId { get; set; } = default!;

    public List<DiaryPage> Pages { get; set; } = [];
}

    // DTOs for incoming request bodies (from user/frontend)
public class CreateDiaryEntryDto
{
    [Required, StringLength(150)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public DateOnly Date { get; set; }

    [StringLength(20000)]
    public string? Content { get; set; }

    public IFormFile? Image { get; set; }
}

    public class UpdateDiaryEntryDto
    {   [StringLength(150)]
        public string? Title { get; set; } 
        public DateOnly? Date { get; set; }

   

    }
}
