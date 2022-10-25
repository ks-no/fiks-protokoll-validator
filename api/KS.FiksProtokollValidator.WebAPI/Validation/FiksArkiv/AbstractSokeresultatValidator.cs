using KS.Fiks.Arkiv.Models.V1.Innsyn.Sok;

namespace KS.FiksProtokollValidator.WebAPI.Validation.FiksArkiv
{
    public class AbstractSokeResultatValidator : AbstractXmlValidator
    {
        public static bool isDateSokeFelt(SaksmappeSokefelt sokefelt)
        {
            switch (sokefelt)
            {
                case SaksmappeSokefelt.MappeOpprettetDato:
                case SaksmappeSokefelt.MappeAvsluttetDato:
                case SaksmappeSokefelt.MappeSkjermingSkjermingOpphoererDato:
                case SaksmappeSokefelt.SakSaksaar:
                case SaksmappeSokefelt.SakSaksdato:
                case SaksmappeSokefelt.MappeEndretDato:
                    return true;
                default:
                    return false;
            }
        }
    }
}