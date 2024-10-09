using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BeepApp_API.Models
{
    public class DOTest
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Players")]
        public int PlayerId { get; set; }

        public double DefenceAvg { get; set; }
        public double OffenceAvg { get; set; }
        public double DefenceListLength { get; set; }
        public double OffenceListLength { get; set; }
        public DateTime CreationDate { get; set; }
    }
}
