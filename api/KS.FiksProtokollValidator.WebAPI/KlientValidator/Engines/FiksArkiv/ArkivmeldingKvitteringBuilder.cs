using System;
using KS.Fiks.Arkiv.Models.V1.Arkivering.Arkivmelding;
using KS.Fiks.Arkiv.Models.V1.Arkivering.Arkivmeldingkvittering;
using KS.Fiks.Arkiv.Models.V1.Metadatakatalog;

namespace KS.FiksProtokollValidator.WebAPI.KlientValidator.Engines.FiksArkiv
{
    public class ArkivmeldingKvitteringBuilder
    {
        private MappeKvittering _mappeKvittering;
        private RegistreringKvittering _registreringKvittering;
        
        public static ArkivmeldingKvitteringBuilder Init()
        {
            return new ArkivmeldingKvitteringBuilder();
        }
        
        public ArkivmeldingKvittering Build()
        {
            var kvittering = new ArkivmeldingKvittering();
            kvittering.MappeKvittering = _mappeKvittering;
            kvittering.RegistreringKvittering = _registreringKvittering;
            return kvittering;
        }
        
        public ArkivmeldingKvitteringBuilder WithSaksmappe(Mappe mappe)
        {
            _mappeKvittering = new SaksmappeKvittering
            {
                SystemID = mappe.SystemID ?? new SystemID() {Label = "", Value = Guid.NewGuid().ToString()},
                OpprettetDato = DateTime.Now,
                Saksaar = DateTime.Now.Year,
                Sakssekvensnummer = new Random().Next(),
                ReferanseEksternNoekkel = new EksternNoekkel()
                {
                    Fagsystem = mappe.ReferanseEksternNoekkel.Fagsystem,
                    Noekkel = mappe.ReferanseEksternNoekkel.Noekkel
                }
            };
            if (mappe.ReferanseForeldermappe != null)
            {
                _mappeKvittering.ReferanseForeldermappe = new ReferanseTilMappe()
                {
                    SystemID = mappe.ReferanseForeldermappe?.SystemID,
                };
            }
            return this;
        }

        public ArkivmeldingKvitteringBuilder WithJournalpost(Registrering journalpost)
        {
            _registreringKvittering = new JournalpostKvittering
            {
                SystemID = journalpost.SystemID ?? new SystemID() {Label = "", Value = Guid.NewGuid().ToString()},
                Journalaar = DateTime.Now.Year,
                Journalsekvensnummer = new Random().Next(),
                Journalpostnummer = new Random().Next(1, 100),
                ReferanseEksternNoekkel = new EksternNoekkel()
                {
                    Fagsystem = journalpost.ReferanseEksternNoekkel.Fagsystem,
                    Noekkel = journalpost.ReferanseEksternNoekkel.Noekkel
                }
            };
            return this;
        }
    }
}