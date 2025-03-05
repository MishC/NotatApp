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
        [StringLength(100, MinimumLength = 3)]
        public string Title { get; set; }

        [StringLength(1000)]
        public string? Content { get; set; }

        public bool IsArchived { get; set; } = false;

        [ForeignKey("Folder")]
        public int FolderId { get; set; }

        [ValidateNever]
        [JsonIgnore]
        public Folder? Folder { get; set; }
    }
}
