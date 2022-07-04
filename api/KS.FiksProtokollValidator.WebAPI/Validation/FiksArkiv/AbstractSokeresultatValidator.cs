using KS.Fiks.Arkiv.Models.V1.Innsyn.Sok;

namespace KS.FiksProtokollValidator.WebAPI.Validation.FiksArkiv
{
    public class AbstractSokeResultatValidator : AbstractXmlValidator
    {
        public static bool isDateSokeFelt(SokFelt parameterFelt)
        {
            switch (parameterFelt)
            {
                case SokFelt.JournalpostDokumentetsdato:
                case SokFelt.JournalpostForfallsdato:
                case SokFelt.JournalpostJournaldato:
                case SokFelt.DokumentbeskrivelseOpprettetDato:
                case SokFelt.MappeOpprettetDato:
                case SokFelt.MappeAvsluttetDato:
                case SokFelt.DokumentbeskrivelseSkjermingSkjermingOpphoererDato:
                case SokFelt.RegistreringOpprettetDato:
                case SokFelt.SakSaksdato:
                case SokFelt.RegistreringSkjermingSkjermingOpphoererDato:
                case SokFelt.MappeSkjermingSkjermingOpphoererDato:    
                case SokFelt.JournalpostSaksaar:
                case SokFelt.SakSaksaar:
                case SokFelt.JournalpostJournalaar:
                    return true;
                default:
                    return false;
            }
        }
    }
}