using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Zeta.Game;

namespace EXPCount
{
    public class EXPCountUI
    {
        static TextBlock[] Texts;
        private static TabItem _tabItem;

        internal static void initTab()
        {


            Application.Current.Dispatcher.Invoke(
                () =>
                {
                    Texts = new TextBlock[16];


                    var mainWindow = Application.Current.MainWindow;

                    var ExperiencePanel1 = new StackPanel { Background = Brushes.DimGray, HorizontalAlignment = HorizontalAlignment.Stretch, Height = 176, Margin = new Thickness(2, 2, 0, 2) };

                    Texts[0] = CreateLabel(LanguageUtil.GetInstance().DefaultRunTime, HorizontalAlignment.Stretch);
                    ExperiencePanel1.Children.Add(Texts[0]);

                    Texts[1] = CreateLabel(LanguageUtil.GetInstance().DefaultTotalEXP, HorizontalAlignment.Stretch);
                    ExperiencePanel1.Children.Add(Texts[1]);

                    Texts[2] = CreateLabel(LanguageUtil.GetInstance().DefaultEXPPerHour, HorizontalAlignment.Stretch);
                    ExperiencePanel1.Children.Add(Texts[2]);

                    Texts[3] = CreateLabel(LanguageUtil.GetInstance().DefaultDeath, HorizontalAlignment.Stretch);
                    ExperiencePanel1.Children.Add(Texts[3]);

                    Texts[4] = CreateLabel(LanguageUtil.GetInstance().DefaultLevelUp, HorizontalAlignment.Stretch);
                    ExperiencePanel1.Children.Add(Texts[4]);

                    Texts[5] = CreateLabel(LanguageUtil.GetInstance().DefaultCreateGames, HorizontalAlignment.Stretch);
                    ExperiencePanel1.Children.Add(Texts[5]);

                    Texts[6] = CreateLabel(LanguageUtil.GetInstance().DefaultLeaveGames, HorizontalAlignment.Stretch);
                    ExperiencePanel1.Children.Add(Texts[6]);

                    Texts[7] = CreateLabel(LanguageUtil.GetInstance().DefaultRiftCount, HorizontalAlignment.Stretch);
                    ExperiencePanel1.Children.Add(Texts[7]);

                    var ExperiencePanel2 = new StackPanel { Background = Brushes.DimGray, HorizontalAlignment = HorizontalAlignment.Stretch, Height = 176, Margin = new Thickness(2, 2, 0, 2) };

                    Texts[8] = CreateLabel(LanguageUtil.GetInstance().DefaultBestGreaterTime, HorizontalAlignment.Stretch);
                    ExperiencePanel2.Children.Add(Texts[8]);

                    Texts[9] = CreateLabel(LanguageUtil.GetInstance().DefaultWorseGreaterTime, HorizontalAlignment.Stretch);
                    ExperiencePanel2.Children.Add(Texts[9]);

                    Texts[10] = CreateLabel(LanguageUtil.GetInstance().DefaultBestGreaterEXP, HorizontalAlignment.Stretch);
                    ExperiencePanel2.Children.Add(Texts[10]);
                    
                    Texts[11] = CreateLabel(LanguageUtil.GetInstance().DefaultWorseGreaterEXP, HorizontalAlignment.Stretch);
                    ExperiencePanel2.Children.Add(Texts[11]);

                    Texts[12] = CreateLabel(LanguageUtil.GetInstance().DefaultBestNephalemTime, HorizontalAlignment.Stretch);
                    ExperiencePanel2.Children.Add(Texts[12]);

                    Texts[13] = CreateLabel(LanguageUtil.GetInstance().DefaultWorseNephalemTime, HorizontalAlignment.Stretch);
                    ExperiencePanel2.Children.Add(Texts[13]);

                    Texts[14] = CreateLabel(LanguageUtil.GetInstance().DefaultBestNephalemEXP, HorizontalAlignment.Stretch);
                    ExperiencePanel2.Children.Add(Texts[14]);

                    Texts[15] = CreateLabel(LanguageUtil.GetInstance().DefaultWorseNephalemEXP, HorizontalAlignment.Stretch);
                    ExperiencePanel2.Children.Add(Texts[15]);


                    var uniformGrid = new UniformGrid
                    {
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Top,
                        MaxHeight = 180,
                        MinWidth = 500,
                        Columns = 2
                    };

                    uniformGrid.Children.Add(ExperiencePanel1);
                    uniformGrid.Children.Add(ExperiencePanel2);

                    _tabItem = new TabItem
                    {
                        Header = LanguageUtil.GetInstance().TabHeader,
                        Content = uniformGrid,
                    };

                    var tabs = mainWindow.FindName("tabControlMain") as TabControl;
                    if (tabs == null)
                    {
                        return;
                    }
                    
                    tabs.Items.Add(_tabItem);
                });
        }



        static TextBlock CreateLabel(string title, HorizontalAlignment haAlignment)
        {
            return new TextBlock
            {
                Text = title,
                Height = 18,
                //Padding = new Thickness(0, 2, 0, 0),
                Margin = new Thickness(2),
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = haAlignment,
                TextAlignment = TextAlignment.Left,
                Foreground = Brushes.White,
                Background = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0))
            };
        }

        internal static void destroyUI()
        {
            Application.Current.Dispatcher.Invoke(new System.Action(() =>
            {
                Window mainWindow = Application.Current.MainWindow;
                var tabs = mainWindow.FindName("tabControlMain") as TabControl;
                if (tabs == null)
                    return;
                tabs.Items.Remove(_tabItem);
            }));
        }

        internal static void updateUI()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                TimeSpan taken = DateTime.Now - EXPCountPugin.StartTime;
                Texts[0].Text = LanguageUtil.GetInstance().RunTime + " " + (taken.Hours + (taken.Days * 24)) +":" +taken.Minutes+ ":"+taken.Seconds;
                Texts[1].Text = LanguageUtil.GetInstance().TotalEXP + EXPCountPugin.GetSimpleEXP(EXPCountPugin.TotalEXP);
                Texts[2].Text = LanguageUtil.GetInstance().EXPPerHour + EXPCountPugin.GetSimpleEXP(GetEXPPerHour(taken, EXPCountPugin.TotalEXP));
                Texts[3].Text = LanguageUtil.GetInstance().Death + EXPCountPugin.DBDeath;
                Texts[4].Text = LanguageUtil.GetInstance().LevelUp + (EXPCountPugin.StartLevel == 0 ? 0 : (ZetaDia.Me.ParagonLevel - EXPCountPugin.StartLevel));
                Texts[5].Text = LanguageUtil.GetInstance().CreateGames + EXPCountPugin.Creates;
                Texts[6].Text = LanguageUtil.GetInstance().LeaveGames + EXPCountPugin.Leaves;
                Texts[7].Text = LanguageUtil.GetInstance().RiftCount + (EXPCountPugin.GreaterRiftCount + EXPCountPugin.NephalemRiftCount) + LanguageUtil.GetInstance().Greater + EXPCountPugin.GreaterRiftCount + LanguageUtil.GetInstance().Nephalem + EXPCountPugin.NephalemRiftCount;

                IEnumerable<EXPCountPugin.RiftEntry> results = EXPCountPugin.RiftList.Where(r => r.IsCompleted && r.IsStarted && r.RiftType == Zeta.Game.Internals.RiftType.Greater);
                if (results.FirstOrDefault() != null)
                {
                    EXPCountPugin.RiftEntry temp = results.FirstOrDefault();
                    foreach (EXPCountPugin.RiftEntry entry in results)
                    {
                        if (entry.GetTakenTime() < temp.GetTakenTime())
                            temp = entry;
                    }
                    Texts[8].Text = LanguageUtil.GetInstance().BestGreaterTime + temp.GetTakenTimeSimple();

                    temp = results.FirstOrDefault();
                    foreach (EXPCountPugin.RiftEntry entry in results)
                    {
                        if (entry.GetTakenTime() > temp.GetTakenTime())
                            temp = entry;
                    }
                    Texts[9].Text = LanguageUtil.GetInstance().WorseGreaterTime + temp.GetTakenTimeSimple();

                    temp = results.FirstOrDefault();
                    foreach (EXPCountPugin.RiftEntry entry in results)
                    {
                        if (entry.GetEXP() > temp.GetEXP())
                            temp = entry;
                    }
                    Texts[10].Text = LanguageUtil.GetInstance().BestGreaterEXP + EXPCountPugin.GetSimpleEXP(temp.GetEXP());
                    
                    temp = results.FirstOrDefault();
                    foreach (EXPCountPugin.RiftEntry entry in results)
                    {
                        if (entry.GetEXP() < temp.GetEXP())
                            temp = entry;
                    }
                    Texts[11].Text = LanguageUtil.GetInstance().WorseGreaterEXP + EXPCountPugin.GetSimpleEXP(temp.GetEXP());
                }
                else
                {
                    Texts[8].Text = LanguageUtil.GetInstance().DefaultBestGreaterTime;
                    Texts[9].Text = LanguageUtil.GetInstance().DefaultWorseGreaterTime;
                    Texts[10].Text = LanguageUtil.GetInstance().DefaultBestGreaterEXP;
                    Texts[11].Text = LanguageUtil.GetInstance().DefaultWorseGreaterEXP;
                }

                results = EXPCountPugin.RiftList.Where(r => r.IsCompleted && r.IsStarted && r.RiftType == Zeta.Game.Internals.RiftType.Nephalem);
                if (results.FirstOrDefault() != null)
                {
                    EXPCountPugin.RiftEntry temp = results.FirstOrDefault();
                    foreach (EXPCountPugin.RiftEntry entry in results)
                    {
                        if (entry.GetTakenTime() < temp.GetTakenTime())
                            temp = entry;
                    }
                    Texts[12].Text = LanguageUtil.GetInstance().BestNephalemTime + temp.GetTakenTimeSimple();

                    temp = results.FirstOrDefault();
                    foreach (EXPCountPugin.RiftEntry entry in results)
                    {
                        if (entry.GetTakenTime() > temp.GetTakenTime())
                            temp = entry;
                    }
                    Texts[13].Text = LanguageUtil.GetInstance().WorseNephalemTime + temp.GetTakenTimeSimple();

                    temp = results.FirstOrDefault();
                    foreach (EXPCountPugin.RiftEntry entry in results)
                    {
                        if (entry.GetEXP() > temp.GetEXP())
                            temp = entry;
                    }
                    Texts[14].Text = LanguageUtil.GetInstance().BestNephalemEXP + EXPCountPugin.GetSimpleEXP(temp.GetEXP());
                    
                    temp = results.FirstOrDefault();
                    foreach (EXPCountPugin.RiftEntry entry in results)
                    {
                        if (entry.GetEXP() < temp.GetEXP())
                            temp = entry;
                    }
                    Texts[15].Text = LanguageUtil.GetInstance().WorseNephalemEXP + EXPCountPugin.GetSimpleEXP(temp.GetEXP());
                }
                else
                {
                    Texts[12].Text = LanguageUtil.GetInstance().DefaultBestNephalemTime;
                    Texts[13].Text = LanguageUtil.GetInstance().DefaultWorseNephalemTime;
                    Texts[14].Text = LanguageUtil.GetInstance().DefaultBestNephalemEXP;
                    Texts[15].Text = LanguageUtil.GetInstance().DefaultWorseNephalemEXP;
                }
            });
        }

        internal static long GetEXPPerHour(TimeSpan taken, long totalEXP)
        {
            double seconds = taken.TotalSeconds;
            double perEXP = totalEXP / seconds;
            return (long)(perEXP * 3600);
        }
    }
}
