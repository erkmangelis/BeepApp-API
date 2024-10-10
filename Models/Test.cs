using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BeepApp_API.Models
{
    public enum TestMode
    {
        YoyoTab = 1,
        MASTab = 2,
        ShuttleRunTab = 3,
        DefenceOffenceTab = 4,
        ConconiTab = 5
    }


    public class Test
    {
        [Key]
        public Guid Id { get; set; }

        [ForeignKey("Players")]
        public Guid PlayerId { get; set; }

        [ForeignKey("Organizations")]
        public int OrganizationId { get; set; }

        public double vo2Max { get; set; }
        public double Speed { get; set; }
        public double Distance { get; set; }
        public string Score { get; set; }
        public DateTime CreatedAt { get; set; }
        public TestMode TestMode { get; set; }
    }
}
