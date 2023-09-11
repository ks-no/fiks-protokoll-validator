using KS.FiksProtokollValidator.WebAPI.TjenerValidator.Models;

namespace KS.FiksProtokollValidator.WebAPI.TjenerValidator.Validation
{
    public interface IFiksResponseValidator
    {
        void Validate(TestSession testSession);
    }
}
