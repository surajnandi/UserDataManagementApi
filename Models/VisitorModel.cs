using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace UserdataManagement.Models
{
    public class VisitorModel
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("ip_address")]
        public string IpAddress { get; set; }

        [Column("device_name")]
        public string DeviceName { get; set; }

        [Column("browser_name")]
        public string BrowserName { get; set; }

        [Column("visited_time", TypeName = "timestamp without time zone")]
        public DateTime VisitedTime { get; set; }

        [Column("visitor_details", TypeName = "jsonb")]
        public string VisitorDetails { get; set; }

        [Column("visitor_count")]
        public long VisitorCount { get; set; }
    }
}
