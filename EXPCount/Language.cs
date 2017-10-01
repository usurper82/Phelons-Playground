using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXPCount
{

    public static class LanguageUtil
    {
        public static Language Instance;

        public static Language GetInstance()
        {
            if (Instance == null)
            {
                if (System.Globalization.CultureInfo.InstalledUICulture.Name.ToLower().StartsWith("zh"))
                    Instance = new CN();
                else
                    Instance = new Language();
            }

            return Instance;
        }
    }

    public class Language
    {
        public string DefaultRunTime = "Run times: 0:0:0";
        public string DefaultTotalEXP = "Total EXP: 0";
        public string DefaultEXPPerHour = "EXP per hour: 0";
        public string DefaultDeath = "Death: 0";
        public string DefaultLevelUp = "Level Up: 0";
        public string DefaultCreateGames = "Create Games: 0";
        public string DefaultLeaveGames = "Leave Games: 0";
        public string DefaultRiftCount = "Rift Count: 0 Greater: 0 Nephalem: 0";
        public string DefaultBestGreaterTime = "Best Greater Time: 0:0:0";
        public string DefaultWorseGreaterTime = "Worse Greater Time: 0:0:0";
        public string DefaultBestGreaterEXP = "Best Greater EXP: 0";
        public string DefaultWorseGreaterEXP = "Worse Greater EXP: 0";
        public string DefaultBestNephalemTime = "Best Nephalem Time: 0:0:0";
        public string DefaultWorseNephalemTime = "Worse Nephalem Time: 0:0:0";
        public string DefaultBestNephalemEXP = "Best Nephalem EXP: 0";
        public string DefaultWorseNephalemEXP = "Worse Nephalem EXP: 0";

        public string TabHeader = "EXPCount";

        public string RunTime = "Run Time: ";
        public string TotalEXP = "Total Exp: ";
        public string EXPPerHour = "Exp Per Hour: ";
        public string Death = "Death: ";
        public string LevelUp = "Level Up: ";
        public string CreateGames = "Create Games: ";
        public string LeaveGames = "Leave Games: ";
        public string RiftCount = "Rift Count: ";
        public string Greater = " Greater: ";
        public string Nephalem = " Nephalem: ";
        public string BestGreaterTime = "Best Greater Time: ";
        public string WorseGreaterTime = "Worse Greater Time: ";
        public string BestGreaterEXP = "Best Greater EXP: ";
        public string WorseGreaterEXP = "Worse Greater EXP: ";
        public string BestNephalemTime = "Best Nephalem Time: ";
        public string WorseNephalemTime = "Worse Nephalem Time: ";
        public string BestNephalemEXP = "Best Nephalem EXP: ";
        public string WorseNephalemEXP = "Worse Nephalem EXP: ";

        public string PluginName = "EXPCount";
        public string PluginAuthor = "KillSB修复";
        public string PluginDescription = "Diablo plugin enthusiasts association QQ group 319496921";

        public string LogSystemLoaded = " - Was loaded successfully";
    }

    public class CN : Language
    {
        public CN()
        {
            DefaultRunTime = "运行时间: 0:0:0";
            DefaultTotalEXP = "总共获得经验: 0";
            DefaultEXPPerHour = "每小时经验: 0";
            DefaultDeath = "死亡次数: 0";
            DefaultLevelUp = "总计升级次数: 0";
            DefaultCreateGames = "创建房间次数: 0";
            DefaultLeaveGames = "离开房间次数: 0";
            DefaultRiftCount = "秘境次数: 0  大秘境: 0  小秘境: 0";
            DefaultBestGreaterTime = "单次大秘境最快时间: 0:0:0";
            DefaultWorseGreaterTime = "单次大秘境最慢时间: 0:0:0";
            DefaultBestGreaterEXP = "单次大秘境最多经验: 0";
            DefaultWorseGreaterEXP = "单次大秘境最少经验: 0";
            DefaultBestNephalemTime = "单次小秘境最快时间: 0:0:0";
            DefaultWorseNephalemTime = "单次小秘境最慢时间: 0:0:0";
            DefaultBestNephalemEXP = "单次小秘境最多经验: 0";
            DefaultWorseNephalemEXP = "单次小秘境最少经验: 0";

            TabHeader = "经验统计";

            RunTime = "运行时间: ";
            TotalEXP = "总共获得经验: ";
            EXPPerHour = "每小时经验: ";
            Death = "死亡次数: ";
            LevelUp = "总计升级次数: ";
            CreateGames = "创建房间次数: ";
            LeaveGames = "离开房间次数: ";
            RiftCount = "秘境次数: ";
            Greater = " 大秘境: ";
            Nephalem = " 小秘境: ";
            BestGreaterTime = "单次大秘境最快时间: ";
            WorseGreaterTime = "单次大秘境最慢时间: ";
            BestGreaterEXP = "单次大秘境最多经验: ";
            WorseGreaterEXP = "单次大秘境最少经验: ";
            BestNephalemTime = "单次小秘境最快时间: ";
            WorseNephalemTime = "单次小秘境最慢时间: ";
            BestNephalemEXP = "单次小秘境最多经验: ";
            WorseNephalemEXP = "单次小秘境最少经验: ";

            LogSystemLoaded = "【暗黑插件爱好者协会QQ群319496921】插件加载成功";
        }
    }
}
