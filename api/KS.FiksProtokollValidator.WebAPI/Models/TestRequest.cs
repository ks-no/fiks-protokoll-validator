using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DateTime = System.DateTime;

namespace KS.FiksProtokollValidator.WebAPI.Models
{
    public class TestRequest
    {
        [Key]
        public string RecipientId { get; set; }

        [Required]
        public List<string> SelectedTestCaseIds { get; set; }
    }
}
