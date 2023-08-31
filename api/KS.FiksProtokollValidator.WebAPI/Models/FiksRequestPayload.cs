﻿using System.ComponentModel.DataAnnotations;

namespace KS.FiksProtokollValidator.WebAPI.Models
{
    public class FiksRequestPayload
    {
        [Key]
        public int Id { get; set; }

        public string Filename { get; set; }

        public byte[] Payload { get; set; }
    }
}
