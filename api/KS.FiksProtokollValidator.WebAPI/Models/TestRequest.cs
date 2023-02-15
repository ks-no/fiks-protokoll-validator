using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KS.FiksProtokollValidator.WebAPI.Models
{
    public class TestRequest
    {
        [Key]
        public string RecipientId { get; set; }

        public string SessionId { get; set; }
        
        public string Protocol { get; set; }

        [Required]
        public List<string> SelectedTestCaseIds { get; set; }
    }
}
