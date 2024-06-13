using System.ComponentModel.DataAnnotations.Schema;

namespace API.Entities;

[Table("Photos")]
public class Photo
{
    public int Id { get; set; }
    public string Url { get; set; }
    public bool IsMain { get; set; }
    public string PublicId { get; set; }
    public int AppUserId { get; set; } // Added to Define not-nullable relationship with AppUser Table during CodeFirst EF Migration
    public AppUser AppUser { get; set; } // Added to Define not-nullable relationship with AppUser Table during CodeFirst EF Migration
}
