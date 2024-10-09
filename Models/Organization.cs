using System.ComponentModel.DataAnnotations;

namespace BeepApp_API.Models
{
    public class Organization
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
