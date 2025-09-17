using Hackathon.Data;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hackathon.Models
{
    public class PostParticipant
    {
        [Key]
        public int Id { get; set; }
        public int PostId { get; set; }
        public string UserId { get; set; }

        /// <summary>
        /// สถานะที่ระบุว่าผู้เข้าร่วมได้รับการอนุมัติหรือไม่
        /// </summary>
        public bool IsApproved { get; set; }

        /// <summary>
        /// สถานะที่ระบุว่าผู้เข้าร่วมถูกปฏิเสธหรือไม่
        /// </summary>
        public bool IsRejected { get; set; }

        public DateTime JoinedAt { get; set; }

        [ForeignKey("PostId")]
        public virtual Post Post { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }
    }
}
