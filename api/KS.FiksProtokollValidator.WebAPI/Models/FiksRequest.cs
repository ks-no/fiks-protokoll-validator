using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DateTime = System.DateTime;

namespace KS.FiksProtokollValidator.WebAPI.Models
{
    public class FiksRequest
    {
        [Key]
        public Guid Id { get; set; }
        
        public Guid MessageGuid { get; set; }

        [Required, DataType(DataType.DateTime)]
        public DateTime SentAt { get; set; }

        public TestCase TestCase { get; set; }
        
        public FiksRequestPayload CustomPayloadFile { get; set; }

        public List<FiksResponse> FiksResponses { get; set; }

        [NotMapped]
        public DateTime ReSentAt { get; set; }

        [NotMapped]
        public int NumberOfReSendings { get; set; }

        [NotMapped]
        public bool IsFiksResponseValidated { get; set; }

        [NotMapped]
        public List<string> FiksResponseValidationErrors { get; set; }
    }
}
