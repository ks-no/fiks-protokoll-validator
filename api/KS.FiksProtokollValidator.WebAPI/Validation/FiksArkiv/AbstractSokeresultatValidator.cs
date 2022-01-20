using KS.Fiks.IO.Arkiv.Client.Models.Innsyn.Sok;

namespace KS.FiksProtokollValidator.WebAPI.Validation.FiksArkiv
{
    public class AbstractSokeResultatValidator : AbstractXmlValidator
    {
        public static bool isDateSokeFelt(SokFelt parameterFelt)
        {
            switch (parameterFelt)
            {
                case SokFelt.JournalpostPeriodDokumentetsdato:
                case SokFelt.JournalpostPeriodForfallsdato:
                case SokFelt.JournalpostPeriodJournaldato:
                case SokFelt.DokumentbeskrivelsePeriodOpprettetDato:
                case SokFelt.MappePeriodOpprettetDato:
                case SokFelt.MappePeriodAvsluttetDato:
                case SokFelt.DokumentbeskrivelsePeriodSkjermingPeriodSkjermingOpphoererDato:
                case SokFelt.RegistreringPeriodOpprettetDato:
                case SokFelt.SakPeriodSaksdato:
                case SokFelt.RegistreringPeriodSkjermingPeriodSkjermingOpphoererDato:
                case SokFelt.MappePeriodSkjermingPeriodSkjermingOpphoererDato:    
                case SokFelt.JournalpostPeriodSaksaar:
                case SokFelt.SakPeriodSaksaar:
                case SokFelt.JournalpostPeriodJournalaar:
                    return true;
                default:
                    return false;
            }
        }
    }
}