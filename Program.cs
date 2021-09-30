using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using MelonLoader;
using Newtonsoft.Json;

using System.Threading.Tasks;

namespace FixedVRCUpdater
{
    public class VRCUpdater : MelonMod
    {
        public string api_base = "https://api.vrcmg.com/v0";

        public override void OnApplicationStart()
        {
            MelonLogger.Msg($"Thanks for using BetterUpdateDownloader!");
            MelonLogger.Msg("Fetching mods from api.vrmg.com");

            string res;

            var client = new WebClient { Headers = { ["User-Agent"] = "Better-Update-Downloader-UwU" } };

            Dictionary<string, ModVersion> modLookupTable = new Dictionary<string, ModVersion>();
            Dictionary<string, Updater> updatingMods = new Dictionary<string, Updater>();

            try {
                res = client.DownloadString($"{api_base}/mods.json");
            } catch (WebException e)
            {
                MelonLogger.Error($"Could not fetch mods: {e.Message}");
                return;
            }

            List<Mod> Mods = JsonConvert.DeserializeObject<List<Mod>>(res);

            if(Mods == null)
            {
                MelonLogger.Error("No mods were recived from the api, strange.");
                return;
            }

            foreach (var mod in Mods)
            {
                if (mod.versions.Count == 0) continue;

                foreach (var name in mod.aliases)
                {
                    if (mod.versions[0].ApprovalStatus == 1)
                    {
                        modLookupTable.Add(name, mod.versions[0]);
                    }
                }
            } 


            foreach (var mod in MelonHandler.Mods)
            {
                Version exsisting_version = new Version(mod.Info.Version);

                if (modLookupTable.ContainsKey(mod.Info.Name))
                {

                    if(updatingMods.ContainsKey(mod.Info.Name))
                    {
                        MelonLogger.Msg(ConsoleColor.Yellow, $"{mod.Info.Name} is already downloading, skipping.");
                        continue;
                    }

                    ModVersion fetched_mod = modLookupTable[mod.Info.Name];

                    Version updated_version = new Version(fetched_mod.modversion);

                    if(exsisting_version.CompareTo(updated_version) < 0)
                    {
                        MelonLogger.Msg(ConsoleColor.Cyan, $"{mod.Info.Name} is out of date. Auto updating. {exsisting_version} => {updated_version}.");

                        Updater mod_downloader = new Updater(fetched_mod);

                        updatingMods.Add(mod.Info.Name, mod_downloader);
                    }

                } else
                {
                    MelonLogger.Msg($"You have an unknown mod loaded: {mod.Info.Name}");
                }
            }

            MelonLogger.Msg("Hey! Just a quick reminder to restart your game <3");
        }
    }
}
