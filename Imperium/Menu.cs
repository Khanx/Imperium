using System.Collections.Generic;
using NetworkUI;

namespace Imperium
{
    [ModLoader.ModManager]
    public static class EmpireMenu
    {
        public static void SendMenuEmpireList(Players.Player player)
        {
            bool belongEmpire = null != Empire.GetEmpire(player);

            NetworkMenu menu = new NetworkMenu();
            menu.LocalStorage.SetAs("header", "Empires");
            menu.Width = 550;


            if (!belongEmpire)
                menu.Items.Add(new NetworkUI.Items.ButtonCallback("Empire_NewEmpire_NOTHING", new LabelData("Found a new empire", UnityEngine.Color.white, UnityEngine.TextAnchor.MiddleCenter)));

            var table = new NetworkUI.Items.Table(550, 180);
            table.ExternalMarginHorizontal = 0f;
            {
                var headerRow = new NetworkUI.Items.HorizontalRow(new List<(IItem, int)>()
                {
                    (new NetworkUI.Items.Label("Empire"), 250),
                     (new NetworkUI.Items.Label("Action"), 150),
                     (new NetworkUI.Items.Label("Members"), 100)
                });
                var headerBG = new NetworkUI.Items.BackgroundColor(headerRow, height: -1, color: NetworkUI.Items.Table.HEADER_COLOR);
                table.Header = headerBG;
            }
            table.Rows = new List<IItem>();

            foreach (Empire empire in Empire.empires)
            {
                List<(IItem, int)> emp = new List<(IItem, int)>();
                emp.Add((new NetworkUI.Items.Label(empire.name), 250));

                if (!belongEmpire && !empire.joinRequest.Contains(player.ID))
                    emp.Add((new NetworkUI.Items.ButtonCallback("Empire_Apply_" + empire.name, new LabelData("Apply", UnityEngine.Color.white, UnityEngine.TextAnchor.MiddleCenter)), 150));
                else
                    emp.Add((new NetworkUI.Items.EmptySpace(), 150));

                emp.Add((new NetworkUI.Items.Label(empire.GetPlayers().Count.ToString()), 100));

                table.Rows.Add(new NetworkUI.Items.HorizontalRow(emp));
            }

            menu.Items.Add(table);

            NetworkMenuManager.SendServerPopup(player, menu);
        }

        public static void SendMenuFoundEmpire(Players.Player player)
        {
            bool belongEmpire = null != Empire.GetEmpire(player);

            NetworkMenu menu = new NetworkMenu();
            menu.LocalStorage.SetAs("header", "Empires");
            menu.Width = 500;

            menu.Items.Add(new NetworkUI.Items.Label("Empire Name"));
            menu.Items.Add(new NetworkUI.Items.InputField("EmpireName"));
            menu.Items.Add(new NetworkUI.Items.ButtonCallback("Empire_FoundEmpire_NOTHING", new LabelData("Create empire", UnityEngine.Color.white, UnityEngine.TextAnchor.MiddleCenter), onClickActions: NetworkUI.Items.ButtonCallback.EOnClickActions.ClosePopup));

            NetworkMenuManager.SendServerPopup(player, menu);
        }

        public static void SendMenuEmpireRequest(Players.Player player)
        {
            Empire empire = Empire.GetEmpire(player);

            if (null == empire)
            {
                Chatting.Chat.Send(player, "<color=orange>You do not belong to any empire.</color>");
                return;
            }

            if (!empire.CanPermission(player, Permissions.Invite))
                return;

            NetworkMenu menu = new NetworkMenu();
            menu.Width = 550;
            menu.LocalStorage.SetAs("header", "Join Request");

            var table = new NetworkUI.Items.Table(550, 180);
            table.ExternalMarginHorizontal = 0f;
            {
                var headerRow = new NetworkUI.Items.HorizontalRow(new List<(IItem, int)>()
                {
                    (new NetworkUI.Items.Label("Empire"), 250),
                    (new NetworkUI.Items.Label("Action"), 125),
                    (new NetworkUI.Items.EmptySpace(), 125)
                });
                var headerBG = new NetworkUI.Items.BackgroundColor(headerRow, height: -1, color: NetworkUI.Items.Table.HEADER_COLOR);
                table.Header = headerBG;
            }
            table.Rows = new List<IItem>();

            foreach (NetworkID nID in empire.joinRequest)
            {
                List<(IItem, int)> requests = new List<(IItem, int)>();

                Players.Player plr = Players.GetPlayer(nID);

                if (null != Empire.GetEmpire(plr))
                {
                    empire.joinRequest.Remove(nID);
                    continue;
                }

                requests.Add((new NetworkUI.Items.Label(plr.Name), 250));
                requests.Add((new NetworkUI.Items.ButtonCallback("Empire_AcceptRequest_" + plr.Name, new LabelData("Accept", UnityEngine.Color.white, UnityEngine.TextAnchor.MiddleCenter)), 125));
                requests.Add((new NetworkUI.Items.ButtonCallback("Empire_RejectRequest_" + plr.Name, new LabelData("Reject", UnityEngine.Color.white, UnityEngine.TextAnchor.MiddleCenter)), 125));

                table.Rows.Add(new NetworkUI.Items.HorizontalRow(requests));
            }

            menu.Items.Add(table);

            NetworkMenuManager.SendServerPopup(player, menu);
        }

        public static void SendMenuEmpire(Players.Player player)
        {
            Empire empire = Empire.GetEmpire(player);

            if (null == empire)
            {
                Chatting.Chat.Send(player, "<color=orange>You do not belong to any empire.</color>");
                return;
            }

            NetworkMenu menu = new NetworkMenu();
            menu.Width = 600;
            menu.LocalStorage.SetAs("header", empire.name);


            /* K: ANNOUNCEMENT SYSTEM
            if (empire.CanPermission(player, Permissions.Announcement))
                if (empire.joinRequest.Count > 0)
                {
                    NetworkUI.Items.ButtonCallback b_announcement = new NetworkUI.Items.ButtonCallback("Empire_SetAnnouncement_NOTHING", new LabelData("Set Announcement", UnityEngine.Color.green, UnityEngine.TextAnchor.MiddleCenter));
                    b_announcement.Enabled = false;
                    menu.Items.Add(b_announcement);
                }
            */

            menu.Items.Add(new NetworkUI.Items.ButtonCallback("Empire_HELP_NOTHING", new LabelData("HELP", UnityEngine.Color.yellow, UnityEngine.TextAnchor.MiddleCenter)));

            if (empire.CanPermission(player, Permissions.Invite))
                if (empire.joinRequest.Count > 0)
                    menu.Items.Add(new NetworkUI.Items.ButtonCallback("Empire_ApplyManage_NOTHING", new LabelData("Manage applications", UnityEngine.Color.green, UnityEngine.TextAnchor.MiddleCenter)));

            if (empire.GetRank(player) == Rank.Emperor)
            {
                menu.Items.Add(new NetworkUI.Items.ButtonCallback("Empire_BackPermission_NOTHING", new LabelData("Set Permissions", UnityEngine.Color.white, UnityEngine.TextAnchor.MiddleCenter)));
                menu.Items.Add(new NetworkUI.Items.ButtonCallback("Empire_SettingsMenu_NOTHING", new LabelData("Settings", UnityEngine.Color.white, UnityEngine.TextAnchor.MiddleCenter)));
            }


            var table = new NetworkUI.Items.Table(600, 800);
            table.ExternalMarginHorizontal = 0f;
            {
                var headerRow = new NetworkUI.Items.HorizontalRow(new List<(IItem, int)>()
                {
                    (new NetworkUI.Items.Label("Name"), 250),
                    (new NetworkUI.Items.Label("Rank"), 125),
                    (new NetworkUI.Items.EmptySpace(), 125),
                });
                var headerBG = new NetworkUI.Items.BackgroundColor(headerRow, height: -1, color: NetworkUI.Items.Table.HEADER_COLOR);
                table.Header = headerBG;
            }
            table.Rows = new List<IItem>();

            foreach (Players.Player plr in empire.GetPlayersOrderedByRank())
            {
                List<(IItem, int)> members = new List<(IItem, int)>();

                members.Add((new NetworkUI.Items.Label(plr.Name), 250));
                members.Add((new NetworkUI.Items.Label(empire.GetRank(plr).ToString()), 125));

                if ((empire.CanPermission(player, Permissions.Ranks) || empire.CanPermission(player, Permissions.Kick)) && (empire.GetRank(player) < empire.GetRank(plr) || empire.GetRank(player) == Rank.Emperor) && !player.ID.Equals(plr.ID))
                    members.Add((new NetworkUI.Items.ButtonCallback("Empire_Manage_" + plr.Name, new LabelData("Manage", UnityEngine.Color.white, UnityEngine.TextAnchor.MiddleCenter)), 125));
                else
                    members.Add((new NetworkUI.Items.EmptySpace(), 125));

                table.Rows.Add(new NetworkUI.Items.HorizontalRow(members));
            }
            table.Height = table.Rows.Count * 14 + 24; //Each row ~14 pixels + 24 for header & distance

            menu.Items.Add(table);

            NetworkMenuManager.SendServerPopup(player, menu);
        }

        public static void SendMenuHelp(Players.Player player)
        {
            NetworkMenu menu = new NetworkMenu();
            menu.Width = 600;
            menu.Height = 400;
            menu.LocalStorage.SetAs("header", "HELP");

            menu.Items.Add(new NetworkUI.Items.Label(new LabelData("Commands", UnityEngine.Color.white, UnityEngine.TextAnchor.MiddleCenter, 45)));

            menu.Items.Add(new NetworkUI.Items.EmptySpace(15));

            menu.Items.Add(new NetworkUI.Items.Label(new LabelData("/empire - Show information about your current empire", UnityEngine.Color.white, UnityEngine.TextAnchor.MiddleLeft, 20)));
            menu.Items.Add(new NetworkUI.Items.Label(new LabelData("/empires - Show information about ALL the empires", UnityEngine.Color.white, UnityEngine.TextAnchor.MiddleLeft, 20)));
            menu.Items.Add(new NetworkUI.Items.Label(new LabelData("/belong_empire - Show the affiliation of each connected player", UnityEngine.Color.white, UnityEngine.TextAnchor.MiddleLeft, 20)));

            menu.Items.Add(new NetworkUI.Items.EmptySpace(15));

            menu.Items.Add(new NetworkUI.Items.Label(new LabelData("/ce <message> - Sends the <message> only to the members of the empire", UnityEngine.Color.white, UnityEngine.TextAnchor.MiddleLeft, 20)));
            //There is a conflict with the ColonyCommands mod
            //menu.Items.Add(new NetworkUI.Items.Label(new LabelData("/chat_empire - Enable / Disable send all the messages only to empire's members", UnityEngine.Color.white, UnityEngine.TextAnchor.MiddleCenter, 20)));

            menu.Items.Add(new NetworkUI.Items.EmptySpace(15));

            menu.Items.Add(new NetworkUI.Items.Label(new LabelData("/leave_empire - Remove you from your current empire", UnityEngine.Color.white, UnityEngine.TextAnchor.MiddleLeft, 20)));

            menu.Items.Add(new NetworkUI.Items.Label(new LabelData("/disband_empire - Disand the current empire (Only EMPEROR)", UnityEngine.Color.white, UnityEngine.TextAnchor.MiddleLeft, 20)));

            NetworkMenuManager.SendServerPopup(player, menu);
        }

        public static void SendMenuEmpireManage(Players.Player player, Players.Player player2)
        {
            Empire empire = Empire.GetEmpire(player);

            if (null == empire)
            {
                Chatting.Chat.Send(player, "<color=orange>You do not belong to any empire.</color>");
                return;
            }

            Rank p1_rank = empire.GetRank(player);
            Rank p2_rank = empire.GetRank(player2);

            NetworkMenu menu = new NetworkMenu();
            menu.Width = 400;
            menu.LocalStorage.SetAs("header", empire.name);

            menu.Items.Add(new NetworkUI.Items.Label("Name: " + player2.Name));
            string rank = "Rank:";

            for (Rank i = Rank.Emperor; i < Rank.None; i++)
            {
                if (i == p2_rank)
                    rank = rank + " <color=green>" + i.ToString() + "</color>";
                else
                    rank = rank + " " + i.ToString();
            }

            menu.Items.Add(new NetworkUI.Items.Label(rank));
            menu.Items.Add(new NetworkUI.Items.EmptySpace());
            menu.Items.Add(new NetworkUI.Items.Line(UnityEngine.Color.white, 25, 4, 2, 10));

            NetworkUI.Items.ButtonCallback b_Promote, b_Demote, b_Kick;

            b_Promote = new NetworkUI.Items.ButtonCallback("Empire_Promote_" + player2.Name, new LabelData("Promote", UnityEngine.Color.white, UnityEngine.TextAnchor.MiddleCenter));
            b_Demote = new NetworkUI.Items.ButtonCallback("Empire_Demote_" + player2.Name, new LabelData("Demote", UnityEngine.Color.white, UnityEngine.TextAnchor.MiddleCenter));
            b_Kick = new NetworkUI.Items.ButtonCallback("Empire_Kick_" + player2.Name, new LabelData("Kick", UnityEngine.Color.white, UnityEngine.TextAnchor.MiddleCenter));

                //You can only Kick people with LOWER rank than you
                if (!empire.CanPermission(p1_rank, Permissions.Kick) || p1_rank >= p2_rank)
                {
                    b_Kick.Enabled = false;
                }

                //You can DEMOTE anyone with LOWER rank than you. Exception: Emperor can demote emperors
                if (((!empire.CanPermission(p1_rank, Permissions.Ranks) || p1_rank >= p2_rank) && (p1_rank != Rank.Emperor)) || p2_rank == Rank.Lord)
                {
                    b_Demote.Enabled = false;
                }

                //You can PROMOTE anyone with LOWER rank than you BUT you cannot promote to your rank
                if (!empire.CanPermission(p1_rank, Permissions.Ranks) || (p2_rank == Rank.Emperor) || (p1_rank!=Rank.Emperor && p1_rank >= p2_rank-1))
                {
                    b_Promote.Enabled = false;
                }

            menu.Items.Add(b_Promote);
            menu.Items.Add(b_Demote);
            menu.Items.Add(b_Kick);

            NetworkMenuManager.SendServerPopup(player, menu);
        }

        public static void SendMenuEmpireSettings(Players.Player player)
        {
            Empire empire = Empire.GetEmpire(player);

            if(empire == null)
            {
                Chatting.Chat.Send(player, "<color=orange>You do not belong to any empire.</color>");
                return;
            }

            if(empire.GetRank(player) != Rank.Emperor)
            {
                Chatting.Chat.Send(player, "<color=orange>Only the emperor can manage the settings of the empire.</color>");
                return;
            }

            NetworkMenu menu = new NetworkMenu();
            menu.LocalStorage.SetAs("header", "Empires");
            menu.Width = 500;

            menu.Items.Add(new NetworkUI.Items.Label("Empire Name"));

            menu.LocalStorage.SetAs("EmpireName", empire.name);
            menu.Items.Add(new NetworkUI.Items.InputField("EmpireName"));

            menu.LocalStorage.SetAs("AutomaticRequest", empire.automaticRequest);
            menu.Items.Add(new NetworkUI.Items.Toggle("Automatically accept joining request to the empire", "AutomaticRequest"));

            menu.Items.Add(new NetworkUI.Items.ButtonCallback("Empire_SetSettings_NOTHING", new LabelData("Save", UnityEngine.Color.white, UnityEngine.TextAnchor.MiddleCenter), onClickActions: NetworkUI.Items.ButtonCallback.EOnClickActions.ClosePopup));

            NetworkMenuManager.SendServerPopup(player, menu);
        }

        public static void SendMenuPermissions(Players.Player player)
        {
            Empire empire = Empire.GetEmpire(player);

            if (null == empire)
            {
                Chatting.Chat.Send(player, "<color=orange>You do not belong to any empire.</color>");
                return;
            }

            if (empire.GetRank(player) != Rank.Emperor)
            {
                Chatting.Chat.Send(player, "<color=orange>Only the emperor can change the permissions.</color>");
                return;
            }

            NetworkMenu menu = new NetworkMenu();
            menu.LocalStorage.SetAs("header", "Permissions");

            // 0 = emperor
            for (int i = 1; i < (int)Rank.None; i++)
            {
                menu.Items.Add(new NetworkUI.Items.ButtonCallback("Empire_Permission_" + i, new LabelData(((Rank)i).ToString(), UnityEngine.Color.white, UnityEngine.TextAnchor.MiddleCenter)));
            }

            NetworkMenuManager.SendServerPopup(player, menu);
        }

        public static void SendMenuPermissionsManagement(Players.Player player, int rank)
        {
            Empire empire = Empire.GetEmpire(player);

            if (null == empire)
            {
                Chatting.Chat.Send(player, "<color=orange>You do not belong to any empire.</color>");
                return;
            }

            if (empire.GetRank(player) != Rank.Emperor)
            {
                Chatting.Chat.Send(player, "<color=orange>Only the emperor can change the permissions.</color>");
                return;
            }

            NetworkMenu menu = new NetworkMenu();
            menu.LocalStorage.SetAs("header", ((Rank)rank).ToString());

            for (int i = (int)Permissions.Invite; i < (int)Permissions.Disband; i *= 2)
            {
                menu.Items.Add(new NetworkUI.Items.Toggle(((Permissions)i).ToString(), ((Permissions)i).ToString()));
                menu.LocalStorage.SetAs(((Permissions)i).ToString(), empire.CanPermission((Rank)rank, (Permissions)i));
            }

            List<IItem> buttons = new List<IItem>();

            menu.Items.Add(new NetworkUI.Items.ButtonCallback("Empire_SetPermission_" + rank, new LabelData("Save", UnityEngine.Color.white, UnityEngine.TextAnchor.MiddleCenter)));
            menu.Items.Add(new NetworkUI.Items.ButtonCallback("Empire_BackPermission_NOTHING", new LabelData("Back", UnityEngine.Color.white, UnityEngine.TextAnchor.MiddleCenter)));

            NetworkMenuManager.SendServerPopup(player, menu);
        }


        public static void SendMenuBelong(Players.Player player)
        {
            NetworkMenu menu = new NetworkMenu();
            menu.LocalStorage.SetAs("header", "Empire belong");
            menu.Width = 700;

            var table = new NetworkUI.Items.Table(700, 180);
            table.ExternalMarginHorizontal = 0f;
            {
                var headerRow = new NetworkUI.Items.HorizontalRow(new List<(IItem, int)>()
                {
                    (new NetworkUI.Items.Label("Player"), 250),
                     (new NetworkUI.Items.Label("Empire"), 250),
                     (new NetworkUI.Items.Label("Rank"), 125)
                });
                var headerBG = new NetworkUI.Items.BackgroundColor(headerRow, height: -1, color: NetworkUI.Items.Table.HEADER_COLOR);
                table.Header = headerBG;
            }
            table.Rows = new List<IItem>();

            for(int i = 0; i < Players.CountConnected;i++)
            {
                Players.Player plr = Players.GetConnectedByIndex(i);

                List<(IItem, int)> emp = new List<(IItem, int)>();

                emp.Add((new NetworkUI.Items.Label(plr.Name), 250));

                Empire empire = Empire.GetEmpire(plr);

                if (empire != null)
                {
                    emp.Add((new NetworkUI.Items.Label(empire.name), 250));
                    emp.Add((new NetworkUI.Items.Label(empire.GetRank(plr).ToString()), 250));
                }
                else
                {
                    emp.Add((new NetworkUI.Items.Label("-"), 250));
                    emp.Add((new NetworkUI.Items.Label("-"), 250));
                }

                table.Rows.Add(new NetworkUI.Items.HorizontalRow(emp));
            }

            menu.Items.Add(table);

            NetworkMenuManager.SendServerPopup(player, menu);
        }


        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerPushedNetworkUIButton, "Khanx.Imperium.PressButton")]
        public static void EmpireButtonManager(ButtonPressCallbackData data)
        {
            if (data.ButtonIdentifier.StartsWith("Empire"))
            {
                string[] args = data.ButtonIdentifier.Split('_');

                string action = args[1];
                string target = args[2];

                Empire empire = Empire.GetEmpire(data.Player);

                switch (action)
                {
                    case "Manage":
                    {
                        if (Players.TryMatchName(target, out Players.Player plr))
                            SendMenuEmpireManage(data.Player, plr);
                    }
                    break;

                    case "Promote":
                    {
                        if (Players.TryMatchName(target, out Players.Player plr))
                            if (null != empire)
                            {
                                empire.Promote(plr, data.Player);
                                SendMenuEmpireManage(data.Player, plr);
                            }
                    }
                    break;

                    case "Demote":
                    {
                        if (Players.TryMatchName(target, out Players.Player plr))
                            if (null != empire)
                            {
                                empire.Demote(plr, data.Player);
                                SendMenuEmpireManage(data.Player, plr);
                            }
                    }
                    break;

                    case "Kick":
                    {
                        if (Players.TryMatchName(target, out Players.Player plr))
                            if (null != empire)
                            {
                                empire.Kick(plr, data.Player);
                                SendMenuEmpireManage(data.Player, plr);
                            }
                    }
                    break;

                    case "Permission":
                    {
                        int rank = int.Parse(target);
                        SendMenuPermissionsManagement(data.Player, rank);
                    }
                    break;

                    case "BackPermission":
                    {
                        SendMenuPermissions(data.Player);
                    }
                    break;

                    case "SetPermission":
                    {
                        int rank = int.Parse(target);

                        Permissions newPermission = 0;

                        for (int i = (int)Permissions.Invite; i < (int)Permissions.Disband; i *= 2)
                            if (data.Storage.GetAs<bool>(((Permissions)i).ToString()))
                                newPermission |= (Permissions)i;

                        if (null != empire)
                            empire.SetPermissions(data.Player, (Rank)rank, newPermission);
                    }
                    break;

                    case "Apply":
                    {
                        empire = Empire.GetEmpire(target);
                        if (null != empire)
                            empire.ApplyFor(data.Player);
                        SendMenuEmpireList(data.Player);
                    }
                    break;

                    case "ApplyManage":
                    {
                        SendMenuEmpireRequest(data.Player);
                    }
                    break;

                    case "AcceptRequest":
                    {
                        if (Players.TryMatchName(target, out Players.Player plr))
                            if (null != empire)
                                empire.Invite(plr, data.Player);

                        if (empire.joinRequest.Count > 0)
                            SendMenuEmpireRequest(data.Player);
                        else
                            SendMenuEmpire(data.Player);
                    }
                    break;

                    case "RejectRequest":
                    {
                        if (Players.TryMatchName(target, out Players.Player plr))
                            if (null != empire)
                                if (empire.joinRequest.Contains(plr.ID))
                                {

                                    empire.joinRequest.Remove(plr.ID);
                                    Chatting.Chat.Send(plr, string.Format("<color=green>{0} has rejected your request of joining.</color>", empire.name));
                                }

                        if (empire.joinRequest.Count > 0)
                            SendMenuEmpireRequest(data.Player);
                        else
                            SendMenuEmpire(data.Player);
                    }
                    break;

                    case "FoundEmpire":
                    {
                            Empire.CreateEmpire(data.Storage.GetAs<string>("EmpireName"), data.Player);
                    }
                    break;

                    case "NewEmpire":
                    {
                            SendMenuFoundEmpire(data.Player);
                    }
                    break;

                    case "HELP":
                    {
                        SendMenuHelp(data.Player);
                    }
                    break;

                    case "SettingsMenu":
                    {
                        SendMenuEmpireSettings(data.Player);
                    }
                    break;

                    case "SetSettings":
                    {
                        string newName = data.Storage.GetAs<string>("EmpireName");
                        if(!empire.name.Equals(newName))
                            empire.SetEmpireName(newName, data.Player);

                        bool automaticRequest = data.Storage.GetAs<bool>("AutomaticRequest");
                        if (empire.automaticRequest != automaticRequest)
                            empire.SetAutomaticRequest(automaticRequest, data.Player);
                    }
                    break;
                }
            }
        }
    }
}
