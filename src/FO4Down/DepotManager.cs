using SteamKit2.GC.CSGO.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FO4Down
{
    public static class DepotManager
    {
        private static List<Depot> depots;
        static DepotManager()
        {
            depots = new List<Depot>
            {
                // <Base Game>
                new (377161, 7497069378349273908, "Fallout Content a", DepotTarget.Game), // 
                new (377162, 5847529232406005096, "Fallout.exe", DepotTarget.Game), //
                new (377163, 5819088023757897745, "Fallout Content b", DepotTarget.Game), // 

                new (377164, 2178106366609958945, "Fallout in English", "english", DepotTarget.Game), //
                new (377165, 7549549550652702123, "Fallout in French", "french", DepotTarget.Game),
                new (377166, 6854162778963425477, "Fallout in German", "german", DepotTarget.Game),
                new (377167, 783101348965844295 , "Fallout in Italian", "italian", DepotTarget.Game),
                new (377168, 7717372852115364102, "Fallout in Spanish", "spanish", DepotTarget.Game),
                new (393880, 8378357397609964253, "Fallout in Polish", "polish", DepotTarget.Game),
                new (393881, 9220466047319762009, "Fallout in Russian", "russian", DepotTarget.Game),
                new (393882, 7540680803954664080, "Fallout in Portuguese", "portuguese", DepotTarget.Game),
                new (393883, 6742459130628608886, "Fallout in Traditional Chinese", "Traditional Chinese", DepotTarget.Game),
                new (393884, 3455288010746962666, "Fallout in Japanese", "japanese", DepotTarget.Game),

                // <Base DLCs>
                new (435880, 1255562923187931216, "Wasteland Workshop", DepotTarget.RequiredDlc), // 
                new (435870, 1691678129192680960, "Automatron", DepotTarget.RequiredDlc), // 
                new (435871, 5106118861901111234, "Automatron English", "english", DepotTarget.RequiredDlc), //
                new (435876, 6475533526946306248, "Automatron Polish", "polish", DepotTarget.RequiredDlc),
                new (404091, 6756984187996423348, "Automatron Japanese", "japanese", DepotTarget.RequiredDlc),
                new (435874, 9032251103390457158, "Automatron Italian", "italian", DepotTarget.RequiredDlc),
                new (435878, 8276750634369029613, "Automatron Portuguese", "portuguese", DepotTarget.RequiredDlc),
                new (435873, 2207548206398235202, "Automatron German", "german", DepotTarget.RequiredDlc),
                new (435875, 2953236065717816833, "Automatron Spanish", "spanish", DepotTarget.RequiredDlc),
                new (435879, 367504569468547727, "Automatron Traditional Chinese", "traditional chinese", DepotTarget.RequiredDlc),
                new (435872, 5590419866095647350, "Automatron French", "french", DepotTarget.RequiredDlc),
                new (435877, 7266521576458366233, "Automatron Russian", "russian", DepotTarget.RequiredDlc), //
                
                // <Extended DLCs>
                new (480630, 5527412439359349504, "Contraptions Workshop", DepotTarget.AllDlc),

                new (435881, 1207717296920736193, "Far Harbor", DepotTarget.AllDlc),
                new (435882, 8482181819175811242, "Far Harbor English", "english", DepotTarget.AllDlc),
                new (435887, 1696734609684135531, "Far Harbor Polish", "polish", DepotTarget.AllDlc),
                new (404093, 1398706778481280442, "Far Harbor Japanese", "japanese", DepotTarget.AllDlc),
                new (435885, 545294059663977441, "Far Harbor Italian", "italian", DepotTarget.AllDlc),
                new (435889, 3739447432423498108, "Far Harbor Portuguese", "portuguese", DepotTarget.AllDlc),
                new (435884, 1583726361179064237, "Far Harbor German", "german", DepotTarget.AllDlc),
                new (435886, 6337694505107499720, "Far Harbor Spanish", "spanish", DepotTarget.AllDlc),
                new (404092, 6806984433357643395, "Far Harbor Traditional Chinese", "traditional chinese", DepotTarget.AllDlc),
                new (435883, 8148702710057205377, "Far Harbor French", "french", DepotTarget.AllDlc),
                new (435888, 2814340383581262374, "Far Harbor Russian", "russian", DepotTarget.AllDlc),

                new (480631, 6588493486198824788, "Vault-Tec Workshop", DepotTarget.AllDlc),
                new (393885, 5000262035721758737, "Vault-Tec Workshop English", "english", DepotTarget.AllDlc),
                new (393890, 1765554658221186452, "Vault-Tec Workshop Polish", "polish", DepotTarget.AllDlc),
                new (393894, 284738489375199037, "Vault-Tec Workshop Japanese", "japanese", DepotTarget.AllDlc),
                new (393888, 4182964460983125860, "Vault-Tec Workshop Italian", "italian", DepotTarget.AllDlc),
                new (393892, 1388883862084490494, "Vault-Tec Workshop portuguese", "portuguese", DepotTarget.AllDlc),
                new (393887, 4458604458983717666, "Vault-Tec Workshop german", "german", DepotTarget.AllDlc),
                new (393889, 7553859846726526417, "Vault-Tec Workshop spanish", "spanish", DepotTarget.AllDlc),
                new (393893, 442593679549850747, "Vault-Tec Workshop Traditional Chinese", "traditional chinese", DepotTarget.AllDlc),
                new (393886, 4075502974578231964, "Vault-Tec Workshop French", "french", DepotTarget.AllDlc),
                new (393891, 631542034352937768, "Vault-Tec Workshop Russian", "russian", DepotTarget.AllDlc),

                new (490650, 4873048792354485093, "Nuka World", DepotTarget.AllDlc),
                new (393895, 7677765994120765493, "Nuka World English", "english", DepotTarget.AllDlc),
                new (404097, 3040241873036299160, "Nuka World Polish", "polish", DepotTarget.AllDlc),
                new (377169, 2112185424871906435, "Nuka-World Italian", "italian", DepotTarget.AllDlc),
                new (404096, 2951208112779014496, "Nuka-World Japanese", "japanese", DepotTarget.AllDlc),
                new (404094, 2888111047071072771, "Nuka-World Portuguese", "portuguese", DepotTarget.AllDlc),
                new (393897, 1357704281835463005, "Nuka-World German", "german", DepotTarget.AllDlc),
                new (393898, 8573679706590820412, "Nuka-World Spanish", "spanish", DepotTarget.AllDlc),
                new (404095, 3169890264626778200, "Nuka-World Traditional Chinese", "traditional chinese", DepotTarget.AllDlc),
                new (393896, 4271967849859961419, "Nuka-World French", "french", DepotTarget.AllDlc),
                new (393899, 4271967849859961419, "Nuka-World Russian", "russian", DepotTarget.AllDlc),

                new (540810, 1558929737289295473, "HD Texture Pack", DepotTarget.HDTextures),
                new (1946161, 6928748513006443409, DepotTarget.CreationKit),
                new (1946162, 3951536123944501689, DepotTarget.CreationKit),
            };
        }

        public static Depot GetHDTextures()
        {
            return depots.First(x => x.Target == DepotTarget.HDTextures);
        }

        public static IReadOnlyList<Depot> GetCreationKit()
        {
            return depots.Where(x => x.Target == DepotTarget.CreationKit).ToList();
        }


        public static IReadOnlyList<Depot> GetLanguageNeutral(DepotTarget target)
        {
            return depots.Where(x => string.IsNullOrEmpty(x.Language) && x.Target == target).ToList();
        }

        public static IReadOnlyList<Depot> Get(DepotTarget target, string? language)
        {
            return depots.Where(x =>
            !string.IsNullOrEmpty(x.Language) &&
            (string.IsNullOrEmpty(language) || x.Language.Equals(language, StringComparison.OrdinalIgnoreCase))
            && x.Target == target).ToList();
        }

    }

    public struct Depot
    {
        public uint Id;
        public DepotTarget Target;
        public ulong ManifestId;
        public string Language;
        public string Name;

        public Depot(
            uint id,
            ulong manifestId,
            string name,
            string language,
            DepotTarget type = DepotTarget.RequiredDlc)
        {
            this.Id = id;
            this.Target = type;
            this.ManifestId = manifestId;
            this.Language = language;
            this.Name = name;
        }

        public Depot(
            uint id,
            ulong manifestId,
            string name,
            DepotTarget type = DepotTarget.RequiredDlc)
        {
            this.Id = id;
            this.Target = type;
            this.ManifestId = manifestId;
            this.Language = "";
            this.Name = name;
        }
        public Depot(
            uint id,
            ulong manifestId,
            DepotTarget type = DepotTarget.Game)
        {
            this.Id = id;
            this.Target = type;
            this.ManifestId = manifestId;
            this.Language = "";
            this.Name = "";
        }


        public (uint, ulong) AsTuple()
        {
            return (Id, ManifestId);
        }

        public override string ToString()
        {
            var str = "";

            if (!string.IsNullOrEmpty(Name))
                str += Name + "\t";

            //if (!string.IsNullOrEmpty(Language))
            //    str += Language + "\t";

            str += Id + "\t" + ManifestId;

            return str;
        }

    }

    public enum DepotTarget
    {
        Game = 0,
        RequiredDlc = 1,
        AllDlc = 2,
        HDTextures = 3,
        CreationKit = 4,
    }
}
