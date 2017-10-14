using System;
using System.Collections.Generic;
using System.Linq;

namespace Trinity.Components.Adventurer.Util
{
    using System.IO;
    using System.IO.Compression;
    using System.Reflection;
    using Framework;

    public class LogCleaner
    {
        private static string _logFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Logs");
        // Delete after 1 day
        private static readonly TimeSpan DeleteFileAfter = new TimeSpan(1, 0, 0, 0);
        private static DateTime _lastCheck = DateTime.MinValue;

        /// <summary>
        ///     Delete files older than <see cref="DeleteFileAfter" />
        /// </summary>
        public static void Delete()
        {
            if (_lastCheck > DateTime.Now)
                return;
            _lastCheck = DateTime.Now.AddMinutes(10);
            var di = new DirectoryInfo(_logFilePath);
            IEnumerable<FileInfo> files =
                di.EnumerateFiles()
                    .Where(
                        f =>
                            (f.LastWriteTimeUtc < DateTime.UtcNow.Subtract(DeleteFileAfter) || f.Length / 1024f / 1024f > 10) &&
                            (f.Name.EndsWith(".txt") || f.Name.EndsWith(".txt.gz") || f.Name.EndsWith(".txt.zip")));
            foreach (FileInfo f in files)
            {
                try
                {
                    Core.Logger.Log("[Log Cleaner] Deleting log file {0}, age {1}, length {2} MB", f.Name,
                        DateTime.UtcNow - f.LastWriteTimeUtc, f.Length / 1024f / 1024f);
                    f.Delete();
                }
                catch (Exception ex)
                {
                    Core.Logger.Debug(ex.ToString());
                }
            }
        }

        /// <summary>
        ///     Compress files older than <see cref="CompressDurationAfter" /> but before <see cref="DeleteFileAfter" />
        /// </summary>
        private static void Compress()
        {
            var di = new DirectoryInfo(_logFilePath);
            IEnumerable<FileInfo> files =
                di.EnumerateFiles()
                    .Where(
                        f =>
                            f.LastWriteTimeUtc < DateTime.UtcNow.Subtract(DeleteFileAfter) && f.Name.EndsWith(".txt"));
            foreach (FileInfo f in files)
            {
                try
                {
                    using (FileStream infile = File.OpenRead(f.FullName))
                    {
                        string outfileName = f.FullName + ".gz";

                        if (File.Exists(outfileName))
                        {
                            continue;
                        }
                        using (FileStream outfile = File.OpenWrite(outfileName))
                        {
                            using (var compressor = new GZipStream(outfile, CompressionMode.Compress))
                            {
                                Core.Logger.Log("[Log Cleaner] Compressing log file {0}, age {1}", f.Name,
                                    (DateTime.UtcNow - f.LastWriteTimeUtc));

                                try
                                {
                                    int inByte = infile.ReadByte();
                                    while (inByte != -1)
                                    {
                                        compressor.WriteByte((byte)inByte);
                                        inByte = infile.ReadByte();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Core.Logger.Debug(ex.ToString());
                                }
                            }
                        }
                    }

                    // Delete the file
                    f.Delete();
                }
                catch (Exception ex)
                {
                    Core.Logger.Debug(ex.ToString());
                }
            }
        }
    }
}
