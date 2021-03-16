using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KS.FiksProtokollValidator.WebAPI.Models
{
    public class TestSession
    {
        [Key]
        public Guid Id { get; set; }

        [Required, DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; }

        [Required]
        public Guid RecipientId { get; set; }

        public List<FiksRequest> FiksRequests { get; set; }

        [NotMapped]
        public ICollection<string> SelectedTestCaseIds { get; set; }
    }
}
