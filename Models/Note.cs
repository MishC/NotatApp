using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace NotatApp.Models
{
    public class Note
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 1)]  // Max 100 length
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Content { get; set; }

        public bool IsArchived { get; set; } = false;

        public DateOnly? ScheduledAt { get; set; }


        [ForeignKey("Folder")]
        public int? FolderId { get; set; }

        [ValidateNever]
        [JsonIgnore]
        public Folder? Folder { get; set; } //EF Navigation Object

        public int OrderIndex { get; set; } = 0;

        [Required]
        [ForeignKey("User")]
        public string UserId { get; set; } = default!;

        [ValidateNever]
        [JsonIgnore]
        public User User { get; set; } = default!;
    }

    //From Body Frontend

    public class CreateNoteDto
    {
        [Required, StringLength(100, MinimumLength = 1)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Content { get; set; }

        public int? FolderId { get; set; }
        public DateOnly? ScheduledAt { get; set; }

    }

    public class UpdateNoteDto
    {
        public string? Title { get; set; }
        public string? Content { get; set; }
        public int? FolderId { get; set; }
        public bool? IsDone { get; set; }
        public DateOnly? ScheduledAt { get; set; }

    }
}
