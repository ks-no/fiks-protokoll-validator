using System.ComponentModel.DataAnnotations;

namespace KS.FiksProtokollValidator.WebAPI.TjenerValidator.Models
{
    public class FiksResponseTest
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string PayloadQuery { get; set; }

        [Required]
        public string ExpectedValue { get; set; }

        [Required]
        public SearchValueType ValueType { get; set; }
    }
}
