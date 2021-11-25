using KS.Fiks.IO.Client.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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
