using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;


namespace NotatApp.Models
{
    public class RecommendedSong
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200, MinimumLength = 1)]
        public string Title { get; set; } = string.Empty;

        [StringLength(200)]
        public string Artist { get; set; } = string.Empty;

        public string? Link {get;set;}

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public string? Model {get; set;}

        public bool? Like {get;set;}

        public string? Style {get;set;}

        public string? Country{get;set;}

    

        [Required]
        [ForeignKey(nameof(DiaryEntry))]
        public int DiaryEntryId { get; set; } = default!;

        [ValidateNever]
        [JsonIgnore]
        public DiaryEntry DiaryEntry { get; set; } = default!;
    }

        public class AiSongRequest
    {
        public int DiaryEntryId { get; set; }
        public string? Style { get; set; }

        public string? Country {get;set;}
    }
   
}
