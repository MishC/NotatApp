using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;


namespace NotatApp.Models
{
    public class Folder
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string Name { get; set; }
        
        [ValidateNever]
        [JsonIgnore]   
        public List<Note> Notes { get; set; } = new List<Note>();
    }
}
