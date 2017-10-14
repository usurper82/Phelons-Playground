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

    public class ExitPortals
    {
        public static List<ExitPortal> Portals { get; set; }

        public static void Load()
        {
            Portals = new List<ExitPortal>();
            var myFileStream = Path.Combine(FileManager.SettingsPath, "ExitPortals.json");

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
                Portals = JsonSerializer.Deserialize<List<ExitPortal>>(fileContents).OrderBy(x => x.LevelAreaId).ToList();
                //Portals = JsonConvert.DeserializeObject<List<ExitPortal>>(fileContents);
            }
            catch (Exception ex)
            {
                Core.Logger.Error($"Failed to Load Exit Portals {ex}");
            }
            finally
            {
                reader?.Close();
            }


            //XmlSerializer serializer = new XmlSerializer(typeof(List<ExitPortal>));
            //try
            //{
            //    using (FileStream fileStream = new FileStream(myFileStream, FileMode.Open))
            //    {
            //        Portals = (List<ExitPortal>)serializer.Deserialize(fileStream);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Core.Logger.Error($"Failed to Load Exit Portals {ex}");
            //}
        }

        public static void Save()
        {

            var myFileStream = Path.Combine(FileManager.SettingsPath, "ExitPortals.json");
            TextWriter writer = null;
            try
            {
                var contentsToWriteToFile = JsonSerializer.Serialize(Portals);
                //var contentsToWriteToFile = JsonConvert.SerializeObject(Portals);
                writer = new StreamWriter(myFileStream, false);
                writer.Write(contentsToWriteToFile);
            }
            catch (Exception ex)
            {
                Core.Logger.Error($"Failed to Save Exit Portals {ex}");
            }
            finally
            {
                writer?.Close();
            }
            ////XmlSerializer serializer = new XmlSerializer(typeof(List<ExitPortal>));
            //try
            //{
            //    if (!Portals.Any())
            //        Portals = new List<ExitPortal>();
            //    using (FileStream fileStream = new FileStream(myFileStream, FileMode.Open))
            //    {
            //        serializer.Serialize(fileStream, Portals);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Core.Logger.Error($"Failed to Save Exit Portals {ex}");
            //}
        }
    }
    //[Serializable]
    public class ExitPortal
    {
        public ExitPortal()
        {
            WorldDynamicId = Core.Player.WorldDynamicId;
            LevelAreaId = Core.Player.LevelAreaId;
            SceneId = Core.Player.SceneId;
            //X = Core.Player.Position.X;
            //Y = Core.Player.Position.Y;
            //Z = Core.Player.Position.Z;
            Position = Core.Player.Position;
            //ZoneType = ZetaDia.Me.InTieredLootRunLevel > 0 ? "GreaterRift" : "NephalemRift";
        }
        public int WorldDynamicId { get; set; }
        public int LevelAreaId { get; set; }

        public int SceneId { get; set; }
        
        //public float X { get; set; }
        
        //public float Y { get; set; }
        
        //public float Z { get; set; }

        public Vector3 Position { get; set; }
        //public string ZoneType { get; set; }
        public override string ToString()
        {
            return $"World Dynamic Id: {WorldDynamicId} | Level Area Id: {LevelAreaId} | Scene Id: {SceneId} | Position: {Position}";
        }
    }
}
