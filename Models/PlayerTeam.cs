using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BeepApp_API.Models
{
    public class PlayerTeam
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Player")]
        public Guid PlayerId { get; set; }
        public virtual Player Player { get; set; }

        [ForeignKey("Team")]
        public Guid TeamId { get; set; }
        public virtual Team Team { get; set; }
    }
}
