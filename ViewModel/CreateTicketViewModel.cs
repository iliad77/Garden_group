using System.ComponentModel.DataAnnotations;

namespace Garden_group.ViewModels
{
    public class CreateTicketViewModel
    {
        [Required, StringLength(100)]
        public string Subject { get; set; } = "";

        [Required]
        public string IncidentType { get; set; } = "";

        [Required]
        public string Priority { get; set; } = "";

        [Required, StringLength(2000)]
        public string Description { get; set; } = "";


    }
}