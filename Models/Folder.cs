using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;


namespace NotatApp.Models
{
    [Microsoft.EntityFrameworkCore.Index(nameof(Name), IsUnique = true)]
    public class Folder
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(20, MinimumLength = 2)]
        public string? Name { get; set; }

        public string? UserId { get; set; } = null;  //update: added userId, as user can make a new folder

        [ValidateNever]
        [JsonIgnore]
        public List<Note> Notes { get; set; } = new List<Note>(); //EF Navigation Object, you can use it to include related notes
    }

    public class CreateFolderDto
    {
        [Required, StringLength(20, MinimumLength = 1)]
        public string Name { get; set; } = string.Empty;
    }

    public class UpdateFolderDto
    {
        [Required, StringLength(20, MinimumLength = 1)]
        public string Name { get; set; } = string.Empty;
    }
}
