using System;
using KS.Fiks.Arkiv.Models.V1.Arkivstruktur;
using KS.Fiks.Arkiv.Models.V1.Innsyn.Hent.Registrering;
using KS.Fiks.Arkiv.Models.V1.Kodelister;
using KS.Fiks.Arkiv.Models.V1.Metadatakatalog;
using Kode = KS.Fiks.Arkiv.Models.V1.Metadatakatalog.Kode;

namespace KS.FiksProtokollValidator.WebAPI.KlientValidator.Engines.FiksArkiv
{
    public class RegistreringHentResultatGenerator
    {
        private const string DokumentbeskrivelseOpprettetAvDefault = "Foo";
        private const int JournalsekvensnummerDefault = 1;
        private const int DokumentbeskrivelseDokumentnummerDefault = 1;
        private const string DokumentbeskrivelseTilknyttetAvDefault = "foo";
        private const string DokumentobjektOpprettetAvDefault = "foo";
        private const string DokumentobjektSjekksumDefault = "foo";
        private const string DokumentobjektSjekksumAlgoritmeDefault = "MD5";
        private const int DokumentobjektFilstoerrelseDefault = 100;
        private const string SaksbehandlerKorrespondansepartDefault = "Ingrid Mottaker";

        public static RegistreringHentResultat Create(RegistreringHent registreringHent)
        {
            var journalpost = CreateJournalpost();
            return Create(registreringHent, journalpost);
        }
        
        public static RegistreringHentResultat Create(RegistreringHent registreringHent, Journalpost journalpost)
        {
            if (registreringHent.ReferanseTilRegistrering.ReferanseEksternNoekkel != null)
            {
                journalpost.ReferanseEksternNoekkel = new EksternNoekkel()
                {   
                    Fagsystem = registreringHent.ReferanseTilRegistrering.ReferanseEksternNoekkel.Fagsystem,
                    Noekkel = registreringHent.ReferanseTilRegistrering.ReferanseEksternNoekkel.Noekkel
                };
            } else if (registreringHent.ReferanseTilRegistrering.SystemID != null)
            {
                journalpost.SystemID = registreringHent.ReferanseTilRegistrering.SystemID;    
            }

            return new RegistreringHentResultat()
            {
                Journalpost = journalpost
            };
        }
        
        public static Journalpost CreateHentJournalpostFraArkivmeldingJournalpost (KS.Fiks.Arkiv.Models.V1.Arkivering.Arkivmelding.Journalpost arkivmeldingJournalpost)
        {
            var jp = new Journalpost()
            {
                SystemID = arkivmeldingJournalpost.SystemID ?? new SystemID() { Value = Guid.NewGuid().ToString() }, 
                OpprettetAv = arkivmeldingJournalpost.OpprettetAv,
                ArkivertAv = arkivmeldingJournalpost.ArkivertAv,
                ReferanseEksternNoekkel = new EksternNoekkel()
                {
                    Fagsystem = arkivmeldingJournalpost.ReferanseEksternNoekkel?.Fagsystem,
                    Noekkel = arkivmeldingJournalpost.ReferanseEksternNoekkel?.Noekkel
                },
                Tittel = arkivmeldingJournalpost.Tittel,
                Journalaar = DateTime.Now.Year,
                Journalsekvensnummer = arkivmeldingJournalpost.Journalsekvensnummer ?? JournalsekvensnummerDefault,
                Journalpostnummer = arkivmeldingJournalpost.Journalpostnummer ?? DateTime.Now.Year + DateTime.Now.Millisecond,
                Journalposttype = new Journalposttype()
                {
                    KodeProperty = arkivmeldingJournalpost.Journalposttype.KodeProperty
                },
                Journalstatus = new Journalstatus()
                {
                    KodeProperty = arkivmeldingJournalpost.Journalstatus.KodeProperty
                },
                Journaldato = arkivmeldingJournalpost.Journaldato ?? new DateTime(),
                DokumentetsDato = DateTime.Now.Date,
                MottattDato = DateTime.Now,
            };

            if (arkivmeldingJournalpost.ReferanseForelderMappe != null)
            {
                if (arkivmeldingJournalpost.ReferanseForelderMappe.SystemID != null)
                {
                    jp.ReferanseForelderMappe = new ReferanseTilMappe()
                    {
                        SystemID = new SystemID()
                        {
                            Label = arkivmeldingJournalpost.ReferanseForelderMappe.SystemID.Label,
                            Value = arkivmeldingJournalpost.ReferanseForelderMappe.SystemID.Value
                        }

                    };
                }
            }

            if (arkivmeldingJournalpost.Arkivdel != null)
            {
                jp.Arkivdel = new Kode()
                {
                    Beskrivelse = arkivmeldingJournalpost.Arkivdel.Beskrivelse,
                    KodeProperty = arkivmeldingJournalpost.Arkivdel.KodeProperty
                };
            }

            foreach (var korrespondansepart in arkivmeldingJournalpost.Korrespondansepart)
            {
                jp.Korrespondansepart.Add(new Korrespondansepart()
                {
                    KorrespondansepartID = korrespondansepart.KorrespondansepartID,
                    Korrespondanseparttype = new Korrespondanseparttype()
                    {
                        KodeProperty = korrespondansepart.Korrespondanseparttype.KodeProperty
                    },
                    KorrespondansepartNavn = korrespondansepart.KorrespondansepartNavn,
                    AdministrativEnhet = korrespondansepart.AdministrativEnhet,
                    Saksbehandler = korrespondansepart.Saksbehandler
                });
            }

            foreach (var dokumentbeskrivelse in arkivmeldingJournalpost.Dokumentbeskrivelse)
            {
                var nyDokumentbeskrivelse = new Dokumentbeskrivelse()
                {
                    SystemID = dokumentbeskrivelse.SystemID ?? new SystemID() { Value = Guid.NewGuid().ToString() },
                    Dokumenttype = new Dokumenttype()
                    {
                        KodeProperty = dokumentbeskrivelse.Dokumenttype.KodeProperty

                    },
                    Dokumentstatus = new Dokumentstatus()
                    {
                        KodeProperty = dokumentbeskrivelse.Dokumentstatus.KodeProperty
                    },
                    Tittel = dokumentbeskrivelse.Tittel,
                    OpprettetDato = dokumentbeskrivelse.OpprettetDato ?? new DateTime(),
                    OpprettetAv = dokumentbeskrivelse.OpprettetAv ?? DokumentbeskrivelseOpprettetAvDefault,
                    TilknyttetRegistreringSom = new TilknyttetRegistreringSom()
                    {
                        KodeProperty = dokumentbeskrivelse.TilknyttetRegistreringSom.KodeProperty
                    },
                    Dokumentnummer = dokumentbeskrivelse.Dokumentnummer ?? DokumentbeskrivelseDokumentnummerDefault,
                    TilknyttetDato = dokumentbeskrivelse.TilknyttetDato ?? new DateTime(),
                    TilknyttetAv = dokumentbeskrivelse.TilknyttetAv ?? DokumentbeskrivelseTilknyttetAvDefault
                };
                
                //TODO legg til part, merknad fra "lagret" journalpost
                
                foreach (var dokumentobjekt in dokumentbeskrivelse.Dokumentobjekt)
                {
                    nyDokumentbeskrivelse.Dokumentobjekt.Add(new Dokumentobjekt()
                    {
                        SystemID = dokumentobjekt.SystemID ?? new SystemID() {Value = Guid.NewGuid().ToString()},
                        Versjonsnummer = dokumentobjekt.Versjonsnummer ?? 1,
                        Variantformat = new Variantformat()
                        {
                            KodeProperty = dokumentobjekt.Variantformat.KodeProperty
                        },
                        Format = new Format()
                        {
                            KodeProperty = dokumentobjekt.Format.KodeProperty
                        },
                        Filnavn = dokumentobjekt.Filnavn,
                        OpprettetDato = dokumentobjekt.OpprettetDato ?? new DateTime(),
                        OpprettetAv = dokumentobjekt.OpprettetAv ?? DokumentobjektOpprettetAvDefault,
                        ReferanseDokumentfil = dokumentobjekt.ReferanseDokumentfil,
                        Sjekksum = dokumentobjekt.Sjekksum ?? DokumentobjektSjekksumDefault,
                        SjekksumAlgoritme = dokumentobjekt.SjekksumAlgoritme ?? DokumentobjektSjekksumAlgoritmeDefault,
                        Filstoerrelse = dokumentobjekt.Filstoerrelse ?? DokumentobjektFilstoerrelseDefault 
                    });
                }
                jp.Dokumentbeskrivelse.Add(nyDokumentbeskrivelse);
            }
            return jp;
        }

        public static Journalpost CreateJournalpost()
        {
            return new Journalpost()
            {
                SystemID = new SystemID() { Value = Guid.NewGuid().ToString() },
                OpprettetAv = "En brukerid",
                ArkivertAv = "En brukerid",
                ReferanseForelderMappe = new ReferanseTilMappe() {
                    SystemID = new SystemID() { Label = "", Value = Guid.NewGuid().ToString() },
                },
                ReferanseEksternNoekkel = new EksternNoekkel()
                {
                    Fagsystem = "Fagsystem X",
                    Noekkel = Guid.NewGuid().ToString()
                },
                Dokumentbeskrivelse =
                {
                    new Dokumentbeskrivelse()
                    {
                        SystemID = new SystemID() { Value = Guid.NewGuid().ToString() },
                        Dokumenttype = new Dokumenttype()
                        {
                            KodeProperty= "SØKNAD"
                        },
                        Dokumentstatus = new Dokumentstatus()
                        {
                            KodeProperty= "F"
                        },
                        Dokumentnummer = 1,
                        TilknyttetDato = new DateTime(),
                        TilknyttetAv = DokumentbeskrivelseTilknyttetAvDefault,
                        Tittel = "Rekvisisjon av oppmålingsforretning",
                        TilknyttetRegistreringSom = new TilknyttetRegistreringSom()
                        {
                            KodeProperty= "H"
                        },
                        OpprettetDato = DateTime.Now,
                        OpprettetAv = DokumentbeskrivelseOpprettetAvDefault,
                        Dokumentobjekt =
                        {
                            new Dokumentobjekt()
                            {
                                SystemID = new SystemID() { Value = Guid.NewGuid().ToString() },
                                Versjonsnummer = 1,
                                Variantformat = new Variantformat()
                                {
                                    KodeProperty = VariantformatKoder.Arkivformat.Verdi
                                },
                                Format = new Format()
                                {
                                    KodeProperty = "pdf"
                                },
                                Filnavn = "Test.pdf",
                                OpprettetDato = DateTime.Now,
                                OpprettetAv = DokumentobjektOpprettetAvDefault,
                                ReferanseDokumentfil = "test.pdf",
                                Sjekksum = DokumentobjektSjekksumDefault,
                                SjekksumAlgoritme = DokumentobjektSjekksumAlgoritmeDefault,
                                Filstoerrelse = DokumentobjektFilstoerrelseDefault
                            }
                        }
                    }
                },
                Tittel = "Internt notat",
                Korrespondansepart =
                {
                    new Korrespondansepart()
                    {
                        Korrespondanseparttype = new Korrespondanseparttype()
                        {
                            KodeProperty= "IM"
                        },
                        KorrespondansepartNavn = "Oppmålingsetaten",
                        AdministrativEnhet = new AdministrativEnhet()
                        {
                            Navn = "Oppmålingsetaten", 
                        },
                        Saksbehandler = new Saksbehandler()
                        {
                            Navn = SaksbehandlerKorrespondansepartDefault
                        }
                    }
                },
                Journalaar = DateTime.Now.Year,
                Journalsekvensnummer = 1,
                Journalpostnummer = int.Parse(DateTime.Now.Year.ToString() + DateTime.Now.Millisecond.ToString()),
                Journalposttype = new Journalposttype()
                {
                    KodeProperty= "X"
                },
                Journalstatus = new Journalstatus()
                {
                    KodeProperty= "F"
                },
                DokumentetsDato = DateTime.Now.Date,
                MottattDato = DateTime.Now,
            };
        }
    }
}