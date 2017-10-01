using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Zeta.Bot;
using Zeta.Common.Plugins;
using Zeta.Game;
using Zeta.Game.Internals;

namespace EXPCount
{
    public class EXPCountPugin : IPlugin
    {
        public static readonly string NAME = LanguageUtil.GetInstance().PluginName;
        public static readonly string AUTHOR = LanguageUtil.GetInstance().PluginAuthor;
        public static readonly Version VERSION = new Version(1, 0, 4);
        public static readonly string DESCRIPTION = LanguageUtil.GetInstance().PluginDescription;

        public string Author
        {
            get { return AUTHOR; }
        }

        public Version Version
        {
            get { return VERSION; }
        }

        public string Name
        {
            get { return NAME; }
        }

        public string Description
        {
            get { return DESCRIPTION; }
        }

        public Window DisplayWindow
        {
            get { return null; }
        }

        public bool Equals(IPlugin other)
        {
            return Name.Equals(other.Name) && Author.Equals(other.Author) && Version.Equals(other.Version);
        }

        public static long DBDeath;
        public static long LastEXP;
        public static long LastLevelEXP;
        public static long TotalEXP;
        public static DateTime StartTime;
        public static long LastLevel;
        public static long StartLevel;
        public static long Creates;
        public static long Leaves;

        public static Thread ExcuteThread;
        public static int ThreadUpdateInterval = 1000;
        public static bool IsExcuteThreadRunning;

        public void OnDisabled()
        {
            Zeta.Bot.GameEvents.OnGameJoined -= OnGameJoined;
            Zeta.Bot.GameEvents.OnGameLeft -= OnGameLeft;
            Zeta.Bot.GameEvents.OnPlayerDied -= OnPlayerDied;
            Zeta.Bot.BotMain.OnStart -= OnStart;
            Zeta.Bot.BotMain.OnStop -= OnStop;
            EXPCountUI.destroyUI();

        }

        public void OnEnabled()
        {
            Zeta.Bot.GameEvents.OnGameJoined += OnGameJoined;
            Zeta.Bot.GameEvents.OnGameLeft += OnGameLeft;
            Zeta.Bot.GameEvents.OnPlayerDied += OnPlayerDied;
            Zeta.Bot.BotMain.OnStart += OnStart;
            Zeta.Bot.BotMain.OnStop += OnStop;
            EXPCountUI.initTab();

        }

        public void OnInitialize()
        {
            Logger.system(LanguageUtil.GetInstance().LogSystemLoaded);
        }

        public void OnPulse()
        {

        }

        public void excute()
        {
            while (IsExcuteThreadRunning)
            {
                try
                {
                    if (ZetaDia.Me == null || !ZetaDia.Me.IsValid || !ZetaDia.IsInGame || ZetaDia.Globals.IsLoadingWorld)
                    {
                        Thread.Sleep(ThreadUpdateInterval);
                        continue;
                    }

                    long ParagonCurrentExperience = ZetaDia.Me.ParagonCurrentExperience;
                    long ParagonExperienceNextLevel = ZetaDia.Me.ParagonExperienceNextLevel;
                    long ParagonLevel = ZetaDia.Me.ParagonLevel;

                    if (ParagonCurrentExperience == 0 || ParagonExperienceNextLevel == 0 || ParagonLevel == 0)
                    {
                        Thread.Sleep(ThreadUpdateInterval);
                        continue;
                    }


                    if (LastEXP == 0)
                    {
                        DBDeath = 0;
                        StartTime = DateTime.Now;
                        LastEXP = ParagonCurrentExperience;
                        LastLevelEXP = ParagonExperienceNextLevel;
                        TotalEXP = 0;
                        StartLevel = ParagonLevel;
                        Creates = 0;
                    }

                    var currentEXP = ParagonCurrentExperience;
                    if (LastLevel != 0 && ParagonLevel > LastLevel)
                    {
                        TotalEXP += LastLevelEXP + currentEXP;
                    }

                    if (LastEXP < currentEXP)
                    {
                        TotalEXP += (currentEXP - LastEXP);
                    }

                    LastEXP = currentEXP;
                    LastLevelEXP = ParagonExperienceNextLevel;
                    LastLevel = ParagonLevel;
                    UpdateRift();
                    EXPCountUI.updateUI();
                }
                catch (Exception e)
                {
                }

                Thread.Sleep(ThreadUpdateInterval);
            }
        }

        public void OnShutdown()
        {

        }

        public void UpdateRift()
        {
            if (ZetaDia.Storage.CurrentRiftType != RiftType.None)
            {
                if (CurrentRift == null)
                {
                    if (IsRiftInvalid())
                    {
                        return;
                    }

                    CreateNewRift();
                }
                else
                {
                    // 如果秘境已经启动， 并且未完成
                    if (IsRiftUnderway() && IsNewRift())
                    {
                        UpdateRiftMissionEXP();
                        CreateNewRift();
                    }

                    if (!IsNewRift() && IsRiftCompleted())
                    {
                        CurrentRift.EndTime = DateTime.Now;
                        CurrentRift.EndEXP = TotalEXP;
                        CurrentRift.IsCompleted = true;
                        RiftSequenceNumber++;
                    }
                }
            }
        }

        public bool IsRiftInvalid()
        {
            if (ZetaDia.Storage.CurrentRiftType == RiftType.Nephalem || ZetaDia.Storage.CurrentRiftType == RiftType.Greater)
            {
                if (ZetaDia.Storage.RiftStarted || ZetaDia.Storage.RiftCompleted)
                    return false;

                return true;
            }
            return false;
        }

        public void UpdateRiftMissionEXP()
        {
            if (CurrentRift.EndEXP != TotalEXP)
                CurrentRift.EndEXP = TotalEXP;
        }

        public bool IsNewRift()
        {
            return CurrentRift.RiftSequenceNumber != RiftSequenceNumber;
        }

        public bool IsRiftCompleted()
        {
            return ZetaDia.Storage.RiftStarted && ZetaDia.Storage.RiftCompleted;
        }

        public bool IsRiftUnderway()
        {
            return ZetaDia.Storage.RiftStarted && !ZetaDia.Storage.RiftCompleted;
        }

        public void CreateNewRift()
        {
            CurrentRift = new RiftEntry(RiftSequenceNumber);
            RiftList.Add(CurrentRift);
            RiftCountUp();
        }

        public void RiftCountUp()
        {
            if (CurrentRift.RiftType == RiftType.Greater)
                GreaterRiftCount++;

            if (CurrentRift.RiftType == RiftType.Nephalem)
                NephalemRiftCount++;
        }

        public void OnGameJoined(object sender, EventArgs eventArgs)
        {
            Creates++;
        }

        public void OnGameLeft(object sender, EventArgs eventArgs)
        {
            Leaves++;
        }

        public void OnPlayerDied(object sender, EventArgs eventArgs)
        {
            DBDeath++;
        }

        public void OnStart(IBot bot)
        {
            IsExcuteThreadRunning = false;
            ExcuteThread = null;
            IsExcuteThreadRunning = true;
            ExcuteThread = new Thread(excute);
            ExcuteThread.Start();
        }

        public void OnStop(IBot bot)
        {
            DBDeath = 0;
            StartTime = DateTime.Now;
            LastEXP = 0;
            TotalEXP = 0;
            LastLevelEXP = 0;
            LastLevel = 0;
            StartLevel = 0;
            Creates = 0;
            Leaves = 0;
            CurrentRift = null;
            GreaterRiftCount = 0;
            NephalemRiftCount = 0;
            RiftSequenceNumber = 0;
            RiftList.Clear();

            IsExcuteThreadRunning = false;
            ExcuteThread = null;
        }

        public static string GetSimpleEXP(long exp)
        {
            if (System.Globalization.CultureInfo.InstalledUICulture.Name.ToLower().StartsWith("zh"))
                return GetCNSimpleEXP(exp);

            return GetENSimpleEXP(exp);
        }

        public static string GetCNSimpleEXP(long exp)
        {
            string result = "" + (exp % 10000);
            if (exp > 10000)
            {
                result = ((exp % 100000000) / 10000) + "万" + result;


            }
            if (exp > 100000000)
            {
                result = (exp / 100000000) + "亿" + result;
            }

            return result;
        }

        public static string GetENSimpleEXP(long exp)
        {
            return exp.ToString("N0");
        }

        public static RiftEntry CurrentRift;
        public static int GreaterRiftCount;
        public static int NephalemRiftCount;
        public static int RiftSequenceNumber;
        public static List<RiftEntry> RiftList = new List<RiftEntry>();

        public class RiftEntry
        {
            public RiftType RiftType;
            public bool IsStarted;
            public bool IsCompleted;
            public bool HasGuardianSpawned;
            public int RiftLevel;
            public DateTime StartTime;
            public DateTime EndTime;
            public long StartEXP;
            public long EndEXP;
            public int RiftSequenceNumber;

            public RiftEntry(int number)
            {
                RiftType = ZetaDia.Storage.CurrentRiftType;
                IsStarted = ZetaDia.Storage.RiftStarted;
                IsCompleted = ZetaDia.Storage.RiftCompleted;
                HasGuardianSpawned = ZetaDia.Storage.RiftGuardianSpawned;
                RiftLevel = ZetaDia.Storage.CurrentRiftLevel;
                StartTime = DateTime.Now;
                EndTime = DateTime.Now;
                StartEXP = TotalEXP;
                RiftSequenceNumber = number;
            }

            public double GetTakenTime()
            {
                return (EndTime - StartTime).TotalSeconds;
            }

            public string GetTakenTimeSimple()
            {
                TimeSpan taken = (EndTime - StartTime);
                return taken.Hours + ":" + taken.Minutes + ":" + taken.Seconds;
            }

            public long GetEXP()
            {
                return EndEXP - StartEXP;
            }

            public override string ToString()
            {
                return "[RiftType=" + RiftType.ToString() + " IsStarted=" + Convert.ToSingle(IsStarted) + " IsCompleted=" + Convert.ToString(IsCompleted) + " HasGuardianSpawned=" + Convert.ToString(HasGuardianSpawned) + " RiftLevel=" + Convert.ToString(RiftLevel) +
                    " StartTime=" + StartTime.ToShortDateString() + " EndTime=" + EndTime.ToShortDateString() + " StartEXP=" + Convert.ToString(StartEXP) + " EndEXP=" + Convert.ToString(EndEXP) + "]";
            }
        }
    }
}
