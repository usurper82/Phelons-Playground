namespace Trinity.Framework.Objects
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml.Serialization;
    using Helpers;
    using IronPython.Modules;
    using Zeta.Common;
    using Zeta.Game;

    public class GemUpgrades
    {
        public static List<GemUpgrade> Upgrades { get; set; }

        public static void Load()
        {
            Upgrades = new List<GemUpgrade>();
            var myFileStream = Path.Combine(FileManager.SettingsPath, "GemUpgrades.json");

            if (string.IsNullOrEmpty(myFileStream) || !File.Exists(myFileStream))
            {
                Core.Logger.Error("Unable to find Exit Portals file.");
                Save();
            }
            TextReader reader = null;
            try
            {
                reader = new StreamReader(myFileStream);
                var fileContents = reader.ReadToEnd();
                Upgrades = JsonSerializer.Deserialize<List<GemUpgrade>>(fileContents).OrderBy(x => x.Chance).ToList();
                //Portals = JsonConvert.DeserializeObject<List<ExitPortal>>(fileContents);
            }
            catch (Exception ex)
            {
                Core.Logger.Error($"Failed to Load Gem Upgrades {ex}");
            }
            finally
            {
                reader?.Close();
            }
        }

        public static void Save()
        {

            var myFileStream = Path.Combine(FileManager.SettingsPath, "GemUpgrades.json");
            TextWriter writer = null;
            try
            {
                var contentsToWriteToFile = JsonSerializer.Serialize(Upgrades);
                //var contentsToWriteToFile = JsonConvert.SerializeObject(Portals);
                writer = new StreamWriter(myFileStream, false);
                writer.Write(contentsToWriteToFile);
            }
            catch (Exception ex)
            {
                Core.Logger.Error($"Failed to Save Gem Upgrades {ex}");
            }
            finally
            {
                writer?.Close();
            }
        }
    }
    //[Serializable]
    public class GemUpgrade
    {
        //public string Name { get; set; }
        public int Chance { get; set; }
        public int Success { get; set; }
        public int Failure { get; set; }
        public override string ToString()
        {
            return $"[Gem Upgrade] Chance: {Chance} | Success: {Success} | Failures: {Failure}";
        }
    }
}
