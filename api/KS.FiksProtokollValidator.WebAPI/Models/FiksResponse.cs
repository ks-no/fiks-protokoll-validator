﻿using KS.Fiks.IO.Client.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KS.FiksProtokollValidator.WebAPI.Models
{
    public class FiksResponse
    {
        [Key]
        public int Id { get; set; }

        [Required, DataType(DataType.DateTime)]
        public DateTime ReceivedAt { get; set; }

        [Required]
        public string Type { get; set; }
        
        [Required]
        public bool IsAsiceVerified { get; set; }
        
        [Required]
        public string PayloadErrors { get; set; }

        public List<FiksPayload> FiksPayloads { get; set; }
    }
}
