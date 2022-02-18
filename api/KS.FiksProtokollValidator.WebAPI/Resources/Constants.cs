using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KS.FiksProtokollValidator.WebAPI.Resources
{
    public class PayloadNames
    {
        public static readonly Dictionary<string, string> Dictionary = new Dictionary<string, string>() {
            {"no.ks.fiks.arkiv.v1", "arkivmelding.xml" },
            {"no.ks.fiks.gi.arkivintegrasjon.oppdatering.basis.arkivmelding.v1", "arkivmelding.xml" },
            { "no.ks.fiks.gi.plan.klient", "payload.json" },
            { "no.ks.fiks.politisk.behandling.klient.v1", "payload.json" },
            { "no.ks.fiks.politisk.behandling.tjener.v1", "payload.json" },
        };
    }
}
