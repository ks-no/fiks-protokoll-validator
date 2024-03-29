using System.ComponentModel.DataAnnotations;

namespace KS.FiksProtokollValidator.WebAPI.TjenerValidator.Models
{
    public class FiksExpectedResponseMessageType
    {
        [Key] public int Id { get; set; }

        [Required] public string ExpectedResponseMessageType { get; set; }
    }
}