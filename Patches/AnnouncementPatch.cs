using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.Data;
using AmongUs.Data.Player;
using Assets.InnerNet;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace TOHE;

// ##https://github.com/Yumenopai/TownOfHost_Y
public class ModNews
{
    public int Number;
    public int BeforeNumber;
    public string Title;
    public string SubTitle;
    public string ShortTitle;
    public string Text;
    public string Date;

    public Announcement ToAnnouncement()
    {
        var result = new Announcement
        {
            Number = Number,
            Title = Title,
            SubTitle = SubTitle,
            ShortTitle = ShortTitle,
            Text = Text,
            Language = (uint)DataManager.Settings.Language.CurrentLanguage,
            Date = Date,
            Id = "ModNews"
        };

        return result;
    }
}
[HarmonyPatch]
public class ModNewsHistory
{
    public static List<ModNews> AllModNews = new();

    // When creating new news, you can not delete old news 
    public static void Init()
    {
        if (TranslationController.Instance.currentLanguage.languageID == SupportedLangs.English)
        {
            {
                // TOHRRE v4.0.6
                var news = new ModNews
                {
                    Number = 100006,
                    Title = "TownOfHost Extreme v4.0.6",
                    SubTitle = "\r★★ Fixes! ★★",
                    ShortTitle = "TOHX v4.0.6",
                    Text = "<size=150%>Welcome to TownOfHost Extreme v4.0.6.</size>\n\n<size=125%>Support for Among Us v2023.7.11 and v2023.7.12</size>\n"
                        + "\n【Base】\n - Base on TOHRE v3.0.0\r\n"
                        + "\n【Changes】\n - Rebrand to TownOfHost Extreme"
                        + "\n【Added】\n - Added back Speed Booster\n\r - Added back Flash\n\r"
                        + "\n【Fixes】\n - Fixed Manager Hide Votes being casted to an integer\n\r",
                    Date = "2023-10-20T00:00:00Z"
                };
                AllModNews.Add(news);
            }
            {
                // TOHRRE v4.0.5
                var news = new ModNews
                {
                    Number = 100005,
                    Title = "TownOfHostReReEdited v4.0.5",
                    SubTitle = "\r★★ Public lobbies! ★★",
                    ShortTitle = "TOHRRE v4.0.5",
                    Text = "<size=150%>Welcome to TOH ReReEdited v4.0.5.</size>\n\n<size=125%>Support for Among Us v2023.7.11 and v2023.7.12</size>\n"
                        + "\n【Base】\n - Base on TOHRE v3.0.0\r\n"
                        + "\n【Added】\n - Added an option for manager to hide votes\r\n"
                        + "\n【Fixes】\n - Public lobbies are now supported\n\r - Fixed Manager votes not showing\n\r",
                    Date = "2023-10-20T00:00:00Z"
                };
                AllModNews.Add(news);
            }
            {
                // TOHRRE v4.0.4
                var news = new ModNews
                {
                    Number = 100004,
                    Title = "TownOfHostReReEdited v4.0.4",
                    SubTitle = "\r★★ Changes and Fixes! ★★",
                    ShortTitle = "TOHRRE v4.0.4",
                    Text = "<size=150%>Welcome to TOH ReReEdited v4.0.4.</size>\n\n<size=125%>Support for Among Us v2023.7.11 and v2023.7.12</size>\n"
                        + "\n【Base】\n - Base on TOHRE v3.0.0\r\n"
                        + "\n【Changes】\n - New image on the main menu\n\r - All buttons on main menu are color matched to the background now\n\r"
                        + "\n【Removed】\n - Removed the website and patreon buttons\n\r",
                    Date = "2023-10-19T00:00:00Z"
                };
                AllModNews.Add(news);
            }
            {
                // TOHRRE v4.0.3
                var news = new ModNews
                {
                    Number = 100003,
                    Title = "TownOfHostReReEdited v4.0.3",
                    SubTitle = "\r★★ More Fixes! ★★",
                    ShortTitle = "TOHRRE v4.0.3",
                    Text = "<size=150%>Welcome to TOH ReReEdited v4.0.3.</size>\n\n<size=125%>Support for Among Us v2023.7.11 and v2023.7.12</size>\n"
                        + "\n【Base】\n - Base on TOHRE v3.0.0\r\n"
                        + "\n【Fixes】\n - Fixed Manager having no colour\n\r - Fixed Karen not existing\n\r - Fixed Manager being a madmate\n\r"
                        + "\n【Changes】\n - Changed github and discord links\n\r",
                    Date = "2023-10-18T00:00:00Z"
                };
                AllModNews.Add(news);
            }
            {
                // TOHRRE v4.0.2
                var news = new ModNews
                {
                    Number = 100002,
                    Title = "TownOfHostReReEdited v4.0.2",
                    SubTitle = "\r★★ Fixes! ★★",
                    ShortTitle = "TOHRRE v4.0.2",
                    Text = "<size=150%>Welcome to TOH ReReEdited v4.0.2.</size>\n\n<size=125%>Support for Among Us v2023.7.11 and v2023.7.12</size>\n"
                        + "\n【Base】\n - Base on TOHRE v3.0.0\r\n"
                        + "\n【Fixes】\n - Fixed captain having no colour\n\r - Fixed missing strings\n\r"
                        + "\n【Changes】\n - Changed the edited text\n\r - Changed the mod colour\n\r",
                    Date = "2023-10-16T00:00:00Z"
                };
                AllModNews.Add(news);
            }
            {
                // TOHRRE v4.0.1
                var news = new ModNews
                {
                    Number = 100001,
                    Title = "TownOfHostReReEdited v4.0.1",
                    SubTitle = "\r★★ Aye Aye Captain! ★★",
                    ShortTitle = "TOHRRE v4.0.1",
                    Text = "<size=150%>Welcome to TOH ReReEdited v4.0.1.</size>\n\n<size=125%>Support for Among Us v2023.7.11 and v2023.7.12</size>\n"
                        + "\n【Base】\n - Base on TOHRE v3.0.0\r\n"
                        + "\n【Added】\n - New role: Captain (From Dark Roles)\n\r",
                    Date = "2023-9-15T00:00:00Z"
                };
                AllModNews.Add(news);
            }
            {
                // TOHRRE v4.0.0
                var news = new ModNews
                {
                    Number = 100000,
                    Title = "TownOfHostReReEdited v4.0.0",
                    SubTitle = "\r★★ The Start! ★★",
                    ShortTitle = "TOHRRE v4.0.0",
                    Text = "<size=150%>Welcome to TOH ReReEdited v4.0.0.</size>\n\n<size=125%>Support for Among Us v2023.7.11 and v2023.7.12</size>\n"
                        + "\n【Base】\n - Base on TOHRE v3.0.0/TOH 4.1.2\r\n"
                        + "\n【Added】\n - New role: Karen (From Dark Roles)\n\r - New role: Manager (From DarkRoles)\n\r",
                    Date = "2023-9-9T00:00:00Z"
                };
                AllModNews.Add(news);
            }
        }
    }

    [HarmonyPatch(typeof(PlayerAnnouncementData), nameof(PlayerAnnouncementData.SetAnnouncements)), HarmonyPrefix]
    public static bool SetModAnnouncements(PlayerAnnouncementData __instance, [HarmonyArgument(0)] ref Il2CppReferenceArray<Announcement> aRange)
    {
        if (!AllModNews.Any())
        {
            Init();
            AllModNews.Sort((a1, a2) => { return DateTime.Compare(DateTime.Parse(a2.Date), DateTime.Parse(a1.Date)); });
        }

        List<Announcement> FinalAllNews = new();
        AllModNews.Do(n => FinalAllNews.Add(n.ToAnnouncement()));
        foreach (var news in aRange)
        {
            if (!AllModNews.Any(x => x.Number == news.Number))
                FinalAllNews.Add(news);
        }
        FinalAllNews.Sort((a1, a2) => { return DateTime.Compare(DateTime.Parse(a2.Date), DateTime.Parse(a1.Date)); });

        aRange = new(FinalAllNews.Count);
        for (int i = 0; i < FinalAllNews.Count; i++)
            aRange[i] = FinalAllNews[i];

        return true;
    }
}