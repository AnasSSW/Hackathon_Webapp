using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Hackathon.Data;

namespace Hackathon.Models
{
    public class Post
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "กรุณาใส่หัวข้อ")]
        [StringLength(200)]
        [Display(Name = "หัวข้อ")]
        public string Title { get; set; }

        [Required(ErrorMessage = "กรุณาใส่เนื้อหา")]
        [Display(Name = "เนื้อหา")]
        public string Content { get; set; }

        public DateTime CreatedAt { get; set; }

        [Display(Name = "ลิงก์รูปภาพ (URL)")]
        public string? ImageUrl { get; set; }

        [Display(Name = "ทักษะที่ต้องการ (คั่นด้วย ,)")]
        public string? RequiredExpertise { get; set; }

        [Display(Name = "วันหมดอายุ")]
        [DataType(DataType.DateTime)]
        public DateTime? ExpirationDate { get; set; }

        [Display(Name = "สถานะโพสต์")]
        public bool IsClosed { get; set; } = false;

        public string? AuthorId { get; set; }

        [ForeignKey("AuthorId")]
        public virtual ApplicationUser? Author { get; set; }

        public virtual ICollection<PostParticipant> Participants { get; set; } = new List<PostParticipant>();

        [Display(Name = "จำนวนสมาชิกที่รับ")]
        [Required(ErrorMessage = "กรุณาระบุจำนวนสมาชิกที่รับ")]
        [Range(1, 100, ErrorMessage = "จำนวนสมาชิกต้องอยู่ระหว่าง 1 ถึง 100")]
        public int MaxParticipants { get; set; } = 10;
    }
}
