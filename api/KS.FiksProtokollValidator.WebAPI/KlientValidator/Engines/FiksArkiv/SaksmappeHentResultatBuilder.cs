using System;
using KS.Fiks.Arkiv.Models.V1.Arkivstruktur;
using KS.Fiks.Arkiv.Models.V1.Innsyn.Hent.Mappe;
using KS.Fiks.Arkiv.Models.V1.Kodelister;
using KS.Fiks.Arkiv.Models.V1.Metadatakatalog;

namespace KS.FiksProtokollValidator.WebAPI.KlientValidator.Engines.FiksArkiv
{
    public class SaksmappeHentResultatBuilder
    {
        private Saksmappe _mappe;

        public static SaksmappeHentResultatBuilder Init()
        {
            return new SaksmappeHentResultatBuilder();
        }

        public MappeHentResultat Build()
        {
            return new MappeHentResultat()
            {
                Mappe = _mappe
            };
        }

        public SaksmappeHentResultatBuilder WithSaksmappe(MappeHent mappeHent)
        {
            _mappe = new Saksmappe()
            {
                SystemID = new SystemID() { Value = mappeHent.ReferanseTilMappe.SystemID != null ? mappeHent.ReferanseTilMappe.SystemID.Value  : Guid.NewGuid().ToString() },
                MappeID = Guid.NewGuid().ToString(),
                Tittel = "En tittel",
                OpprettetDato = DateTime.Now.Subtract(TimeSpan.FromDays(1)),
                OpprettetAv = "Testbruker",
                Saksaar = DateTime.Now.Year,
                Sakssekvensnummer = 1,
                Saksdato = DateTime.Now.Subtract(TimeSpan.FromDays(1)),
                AdministrativEnhet = new AdministrativEnhet()
                {
                    Navn = "Test Testesen"
                },
                Saksansvarlig = new Saksansvarlig()
                {
                    Identifikator = "identifikator"
                },
                Saksstatus = new Saksstatus()
                {
                    KodeProperty = SaksstatusKoder.Avsluttet.Verdi,
                }
            };
            return this;
        }
    }
}