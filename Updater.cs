using System;
using System.IO;
using System.Security.Cryptography;
using System.Net;
using MelonLoader;

namespace FixedVRCUpdater
{
    public class Updater
    {
        private Uri download_url;
        private string version;
        private string hash;
        private string name;
        public Boolean isFinished;

        public Updater(ModVersion mod)
        {
            this.download_url = new Uri(mod.downloadlink);
            this.name = mod.name;
            this.version = mod.modversion;
            this.isFinished = false;
            this.hash = mod.hash;
                
            UpdateMod();
        }


        public void UpdateMod()
        {
            MelonLogger.Msg($"Starting Downlaod of mod {this.name}!");

            using(var client = new WebClient())
            {
                client.DownloadDataCompleted += (s, e) =>
                {

                    if(e.Error != null)
                    {
                        MelonLogger.Error($"Failed while trying to download {this.name}!");
                        return;
                    }

                    byte[] rawData = e.Result;

                    MelonLogger.Msg(ConsoleColor.Green, $"{this.name} has downloaded successfully! Verifing hash, then saving.");

                    this.isFinished = true;

                    string file_hash = GetFileHash(rawData);

                    if(file_hash == this.hash)
                    {
                        MelonLogger.Msg(ConsoleColor.White, $"{this.name}'s Checksum has been verified.");

                        File.WriteAllBytes($"Mods/{this.name}.dll", rawData);   
                    }
                    else
                    {
                        MelonLogger.Msg(ConsoleColor.Red, $"{this.name}'s Downloaded files checksum does not match the one provided by the server! Not saving mod.");
                    }
                };

                client.DownloadDataAsync(download_url, $"Mods/{this.name}.dll");
            }


        }

        public string GetFileHash(byte[] data)
        {
            using (SHA256 sha_hash = SHA256.Create())
            {
                byte[] filebytes = sha_hash.ComputeHash(data);
                return Convert.ToBase64String(filebytes);
            }
        }
    }
}
