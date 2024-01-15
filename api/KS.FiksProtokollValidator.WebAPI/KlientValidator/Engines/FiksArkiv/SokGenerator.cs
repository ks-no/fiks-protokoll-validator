using KS.Fiks.Arkiv.Models.V1.Arkivstruktur;
using KS.Fiks.Arkiv.Models.V1.Innsyn.Sok;
using KS.Fiks.Arkiv.Models.V1.Meldingstyper;
using KS.FiksProtokollValidator.WebAPI.KlientValidator.Models;

namespace KS.FiksProtokollValidator.WebAPI.KlientValidator.Engines.FiksArkiv
{
    public class SokGenerator
    {
        public static Melding CreateSokResponseMelding(Sok sok) =>
            sok.Sokdefinisjon.Responstype switch
            {
                Responstype.Minimum =>
                    new Melding
                    {
                        FileName = "resultat.xml",
                        MeldingsType = FiksArkivMeldingtype.SokResultatMinimum,
                        ResultatMelding = SokeresultatGenerator.CreateSokeResultatMinimum(sok.Sokdefinisjon)
                    },
                Responstype.Noekler =>
                    new Melding
                    {
                        FileName = "resultat.xml",
                        MeldingsType = FiksArkivMeldingtype.SokResultatNoekler,
                        ResultatMelding = SokeresultatGenerator.CreateSokeResultatNoekler(),
                    },
                Responstype.Utvidet =>
                    new Melding
                    {
                        FileName = "resultat.xml",
                        MeldingsType = FiksArkivMeldingtype.SokResultatUtvidet,
                        ResultatMelding = SokeresultatGenerator.CreateSokeResultatUtvidet(sok.Sokdefinisjon)
                    },
                _ =>
                    new Melding
                    {
                        FileName = "resultat.xml",
                        MeldingsType = FiksArkivMeldingtype.SokResultatMinimum,
                        ResultatMelding = SokeresultatGenerator.CreateSokeResultatMinimum(sok.Sokdefinisjon),
                    }
            };

    }
}