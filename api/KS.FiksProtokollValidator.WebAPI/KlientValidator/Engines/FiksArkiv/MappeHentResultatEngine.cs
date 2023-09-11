using KS.Fiks.Arkiv.Models.V1.Arkivstruktur;
using KS.Fiks.Arkiv.Models.V1.Innsyn.Hent.Mappe;

namespace KS.FiksProtokollValidator.WebAPI.KlientValidator.Engines.FiksArkiv
{
    public class MappeHentResultatEngine
    {
        public static MappeHentResultat Create(MappeHent mappeHent)
        {
            return new MappeHentResultat()
            {
                Mappe = CreateMappe(mappeHent)
            };
        }

        public static Mappe CreateMappe(MappeHent mappeHent)
        {
            return new Mappe()
            {
                SystemID = mappeHent.ReferanseTilMappe.SystemID
            };
        }
    }
}