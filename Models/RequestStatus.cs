using System.ComponentModel.DataAnnotations;

namespace WebApi.Models
{
    public class RequestStatus
    {
        public int StatusId { get; set; } = 0;
        public int RequestId { get; set; } = 0;
        public int UserId { get; set; } = 0;

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime Date { get; set; } = DateTime.Now;
        public TimeSpan Time { get; set; } = TimeSpan.Zero;
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Time")]
        public string FormattedTime => Time.ToString(@"hh\:mm");
    }
}
