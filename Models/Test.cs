using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BeepApp_API.Models
{
    public enum TestMode
    {
        test1,
        test2,
        test3
    }

    public class Test
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Players")]
        public int PlayerId { get; set; }

        public double v02Max { get; set; }
        public double Speed { get; set; }
        public double Distance { get; set; }
        public string Score { get; set; }
        public DateTime CreationDate { get; set; }
        public TestMode TestMode { get; set; }
    }
}
