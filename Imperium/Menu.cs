using System.Collections.Generic;
using NetworkUI;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Imperium
{
    [ModLoader.ModManager]
    public static class EmpireMenu
    {
        public static readonly int maxMembers = 20;

        public static void SendMenuEmpireList(Players.Player player)
        {
            bool belongEmpire = null != Empire.GetEmpire(player);

            NetworkMenu menu = new NetworkMenu();
            menu.LocalStorage.SetAs("header", "Empires");
            menu.Width = 550;
            menu.Height = 600;

            if (!belongEmpire)
                menu.Items.Add(new NetworkUI.Items.ButtonCallback("Imperium_NewEmpire", new LabelData("Found a new empire", UnityEngine.Color.white, UnityEngine.TextAnchor.MiddleCenter)));

            NetworkUI.Items.Table table = new NetworkUI.Items.Table(550, 180)
            {
                ExternalMarginHorizontal = 0f
            };
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

            var orderedEmpire = from empire in Empire.empires orderby empire.GetPlayers().Count() descending select empire;

            foreach (Empire empire in orderedEmpire)
            {
                List<(IItem, int)> emp = new List<(IItem, int)>
                {
                    (new NetworkUI.Items.Label(empire.Name), 250)
                };

                if (!belongEmpire && !empire.joinRequest.Contains(player.ID) && empire.GetPlayers().Count <= maxMembers)
                    emp.Add((new NetworkUI.Items.ButtonCallback("Imperium_Apply", new LabelData("Apply", UnityEngine.Color.white, UnityEngine.TextAnchor.MiddleCenter), ButtonPayload: new JObject() { { "empire", empire.Name } }), 150));
                else
                    emp.Add((new NetworkUI.Items.ButtonCallback("Imperium_Apply", new LabelData("Apply", UnityEngine.Color.white, UnityEngine.TextAnchor.MiddleCenter), isInteractive: false), 150));

                emp.Add((new NetworkUI.Items.Label(empire.GetPlayers().Count.ToString() + "/" + maxMembers), 100));

                table.Rows.Add(new NetworkUI.Items.HorizontalRow(emp));
            }

            menu.Items.Add(table);

            NetworkMenuManager.SendServerPopup(player, menu);
        }

        public static void SendMenuFoundEmpire(Players.Player player)
        {
            NetworkMenu menu = new NetworkMenu();
            menu.LocalStorage.SetAs("header", "Empires");
            menu.Width = 500;

            menu.Items.Add(new NetworkUI.Items.Label("Empire Name"));
            menu.Items.Add(new NetworkUI.Items.InputField("EmpireName"));

            menu.Items.Add(new NetworkUI.Items.Label("Empire Tag"));
            menu.Items.Add(new NetworkUI.Items.InputField("EmpireTag"));

            menu.Items.Add(new NetworkUI.Items.ButtonCallback("Imperium_FoundEmpire", new LabelData("Create empire", UnityEngine.Color.white, UnityEngine.TextAnchor.MiddleCenter), onClickActions: NetworkUI.Items.ButtonCallback.EOnClickActions.ClosePopup));

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

            if (!empire.CanPermission(player.ID, Permissions.Invite))
                return;

            NetworkMenu menu = new NetworkMenu
            {
                Width = 550
            };
            menu.LocalStorage.SetAs("header", "Join Request");

            NetworkUI.Items.Table table = new NetworkUI.Items.Table(550, 180)
            {
                ExternalMarginHorizontal = 0f
            };
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
                if(empire.GetPlayers().Count < maxMembers)
                    requests.Add((new NetworkUI.Items.ButtonCallback("Imperium_Request", new LabelData("Accept", UnityEngine.Color.white, UnityEngine.TextAnchor.MiddleCenter), ButtonPayload: new JObject() { { "accept", true }, { "player", player.ID.ToString() } }), 125));
                else
                    requests.Add((new NetworkUI.Items.ButtonCallback("Imperium_Request", new LabelData("Accept", UnityEngine.Color.white, UnityEngine.TextAnchor.MiddleCenter), isInteractive: false), 125));
                requests.Add((new NetworkUI.Items.ButtonCallback("Imperium_Request", new LabelData("Reject", UnityEngine.Color.white, UnityEngine.TextAnchor.MiddleCenter), ButtonPayload: new JObject() { { "accept", false }, { "player", player.ID.ToString() } }), 125));

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

            NetworkMenu menu = new NetworkMenu
            {
                Width = 700,
                Height = 600
            };
            menu.LocalStorage.SetAs("header", empire.Name);


            /* K: ANNOUNCEMENT SYSTEM
            if (empire.CanPermission(player, Permissions.Announcement))
                if (empire.joinRequest.Count > 0)
                {
                    NetworkUI.Items.ButtonCallback b_announcement = new NetworkUI.Items.ButtonCallback("Empire_SetAnnouncement_NOTHING", new LabelData("Set Announcement", UnityEngine.Color.green, UnityEngine.TextAnchor.MiddleCenter));
                    b_announcement.Enabled = false;
                    menu.Items.Add(b_announcement);
                }
            */

            menu.Items.Add(new NetworkUI.Items.ButtonCallback("Imperium_HELP", new LabelData("HELP", UnityEngine.Color.yellow, UnityEngine.TextAnchor.MiddleCenter)));

            if (empire.CanPermission(player.ID, Permissions.Invite))
                if (empire.joinRequest.Count > 0)
                    menu.Items.Add(new NetworkUI.Items.ButtonCallback("Imperium_ApplyManage", new LabelData("Manage applications", UnityEngine.Color.green, UnityEngine.TextAnchor.MiddleCenter)));

            if (empire.GetRank(player) == Rank.Emperor)
            {
                menu.Items.Add(new NetworkUI.Items.ButtonCallback("Imperium_BackPermission", new LabelData("Set Permissions", UnityEngine.Color.white, UnityEngine.TextAnchor.MiddleCenter)));
                menu.Items.Add(new NetworkUI.Items.ButtonCallback("Imperium_SettingsMenu", new LabelData("Settings", UnityEngine.Color.white, UnityEngine.TextAnchor.MiddleCenter)));
            }


            NetworkUI.Items.Table table = new NetworkUI.Items.Table(700, 800)
            {
                ExternalMarginHorizontal = 0f
            };
            {
                NetworkUI.Items.HorizontalRow headerRow = new NetworkUI.Items.HorizontalRow
                {
                    Items = new List<(IItem, int)>
                {
                    (new NetworkUI.Items.Label("Name"), 250),
                    (new NetworkUI.Items.Label("Rank"), 125)
                }
                };

                if (ColonyCommands.ColonyCommandsMod.ColonyCommands)
                    headerRow.Items.Add((new NetworkUI.Items.Label("Last seen"), 125));

                headerRow.Items.Add((new NetworkUI.Items.EmptySpace(), 125));

                var headerBG = new NetworkUI.Items.BackgroundColor(headerRow, height: -1, color: NetworkUI.Items.Table.HEADER_COLOR);
                table.Header = headerBG;
            }
            table.Rows = new List<IItem>();

            foreach (Players.Player plr in empire.GetPlayersOrderedByRank())
            {
                List<(IItem, int)> members = new List<(IItem, int)>
                {
                    (new NetworkUI.Items.Label(plr.Name), 250),
                    (new NetworkUI.Items.Label(empire.GetRank(plr).ToString()), 125)
                };

                if (ColonyCommands.ColonyCommandsMod.ColonyCommands)
                {
                    var m = ColonyCommands.ColonyCommandsMod.GetMethodFromColonyCommandsMod("ActivityTracker", "GetLastSeen");

                    if (m != null)
                    {
                        string lastseen = (string)m.Invoke(null, new object[] { plr.ID.ToStringReadable() });

                        if (!lastseen.Equals("never"))
                            lastseen = lastseen.Substring(0, lastseen.IndexOf(" ")).Trim();

                        members.Add((new NetworkUI.Items.Label(lastseen), 125));
                    }
                    else
                        members.Add((new NetworkUI.Items.EmptySpace(), 125));
                }
                
                if ((empire.CanPermission(player.ID, Permissions.Ranks) || empire.CanPermission(player.ID, Permissions.Kick)) && (empire.GetRank(player) < empire.GetRank(plr) || empire.GetRank(player) == Rank.Emperor) && !player.ID.Equals(plr.ID))
                    members.Add((new NetworkUI.Items.ButtonCallback("Imperium_Manage", new LabelData("Manage", UnityEngine.Color.white, UnityEngine.TextAnchor.MiddleCenter), ButtonPayload: new JObject() { { "player", plr.ID.ToString() } }), 125));
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
            NetworkMenu menu = new NetworkMenu
            {
                Width = 600,
                Height = 400
            };
            menu.LocalStorage.SetAs("header", "HELP");

            menu.Items.Add(new NetworkUI.Items.Label(new LabelData("Commands", UnityEngine.Color.white, UnityEngine.TextAnchor.MiddleCenter, 45)));

            menu.Items.Add(new NetworkUI.Items.EmptySpace(15));

            menu.Items.Add(new NetworkUI.Items.Label(new LabelData("/empire - Show information about your current empire", UnityEngine.Color.white, UnityEngine.TextAnchor.MiddleLeft, 20)));
            menu.Items.Add(new NetworkUI.Items.Label(new LabelData("/empires - Show information about ALL the empires", UnityEngine.Color.white, UnityEngine.TextAnchor.MiddleLeft, 20)));
            menu.Items.Add(new NetworkUI.Items.Label(new LabelData("/belong_empire - Show the affiliation of each connected player", UnityEngine.Color.white, UnityEngine.TextAnchor.MiddleLeft, 20)));
            menu.Items.Add(new NetworkUI.Items.Label(new LabelData("/belong_empire <playername> - Show the affiliation of <playername>", UnityEngine.Color.white, UnityEngine.TextAnchor.MiddleLeft, 20)));

            menu.Items.Add(new NetworkUI.Items.EmptySpace(15));

            menu.Items.Add(new NetworkUI.Items.Label(new LabelData("/ce <message> - Sends the <message> only to the members of the empire", UnityEngine.Color.white, UnityEngine.TextAnchor.MiddleLeft, 20)));
            //The conflict of the ColonyCommandsMod has been resolved
            menu.Items.Add(new NetworkUI.Items.Label(new LabelData("/chat_empire - Enable / Disable send all the messages only to empire's members", UnityEngine.Color.white, UnityEngine.TextAnchor.MiddleLeft, 20)));

            menu.Items.Add(new NetworkUI.Items.EmptySpace(15));

            menu.Items.Add(new NetworkUI.Items.Label(new LabelData("/leave_empire - Remove you from your current empire", UnityEngine.Color.white, UnityEngine.TextAnchor.MiddleLeft, 20)));

            menu.Items.Add(new NetworkUI.Items.Label(new LabelData("/disband_empire - Disand the current empire (Only EMPEROR)", UnityEngine.Color.white, UnityEngine.TextAnchor.MiddleLeft, 20)));

            if(PermissionsManager.HasPermission(player, "khanx.imperium"))
            {
                menu.Items.Add(new NetworkUI.Items.EmptySpace(15));
                menu.Items.Add(new NetworkUI.Items.Label(new LabelData("Staff Commands", UnityEngine.Color.white, UnityEngine.TextAnchor.MiddleCenter, 45)));

                menu.Items.Add(new NetworkUI.Items.Label(new LabelData("/empire_asemperor - Shows an interface that allows you to be the emperor of ANY empire.", UnityEngine.Color.white, UnityEngine.TextAnchor.MiddleLeft, 20)));

                menu.Items.Add(new NetworkUI.Items.Label(new LabelData("/empire_setrank - Allows you to change your rank in the empire you are in.", UnityEngine.Color.white, UnityEngine.TextAnchor.MiddleLeft, 20)));
            }

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

            NetworkMenu menu = new NetworkMenu
            {
                Width = 400
            };
            menu.LocalStorage.SetAs("header", empire.Name);

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

            b_Promote = new NetworkUI.Items.ButtonCallback("Imperium_Promote", new LabelData("Promote", UnityEngine.Color.white, UnityEngine.TextAnchor.MiddleCenter), ButtonPayload: new JObject() { { "player", player2.ID.ToString() } });
            b_Demote = new NetworkUI.Items.ButtonCallback("Imperium_Demote", new LabelData("Demote", UnityEngine.Color.white, UnityEngine.TextAnchor.MiddleCenter), ButtonPayload: new JObject() { { "player", player2.ID.ToString() } });
            b_Kick = new NetworkUI.Items.ButtonCallback("Imperium_Kick", new LabelData("Kick", UnityEngine.Color.white, UnityEngine.TextAnchor.MiddleCenter), ButtonPayload: new JObject() { { "player", player2.ID.ToString() } });

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
            menu.LocalStorage.SetAs("EmpireName", empire.Name);
            menu.Items.Add(new NetworkUI.Items.InputField("EmpireName"));

            menu.Items.Add(new NetworkUI.Items.Label("Empire Tag"));
            menu.LocalStorage.SetAs("EmpireTag", empire.Tag);
            menu.Items.Add(new NetworkUI.Items.InputField("EmpireTag"));

            menu.LocalStorage.SetAs("AutomaticRequest", empire.automaticRequest);
            menu.Items.Add(new NetworkUI.Items.Toggle("Automatically accept joining request to the empire", "AutomaticRequest"));

            menu.Items.Add(new NetworkUI.Items.ButtonCallback("Imperium_SetSettings", new LabelData("Save", UnityEngine.Color.white, UnityEngine.TextAnchor.MiddleCenter), onClickActions: NetworkUI.Items.ButtonCallback.EOnClickActions.ClosePopup));

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
                menu.Items.Add(new NetworkUI.Items.ButtonCallback("Imperium_Permission", new LabelData(((Rank)i).ToString(), UnityEngine.Color.white, UnityEngine.TextAnchor.MiddleCenter), ButtonPayload: new JObject() { { "rank", i } }));
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

            menu.Items.Add(new NetworkUI.Items.ButtonCallback("Imperium_SetPermission", new LabelData("Save", UnityEngine.Color.white, UnityEngine.TextAnchor.MiddleCenter), ButtonPayload: new JObject() { { "rank", rank } }));
            menu.Items.Add(new NetworkUI.Items.ButtonCallback("Imperium_BackPermission", new LabelData("Back", UnityEngine.Color.white, UnityEngine.TextAnchor.MiddleCenter)));

            NetworkMenuManager.SendServerPopup(player, menu);
        }

        public static void SendMenuBelong(Players.Player player)
        {
            NetworkMenu menu = new NetworkMenu();
            menu.LocalStorage.SetAs("header", "Empire belong");
            menu.Width = 700;

            NetworkUI.Items.Table table = new NetworkUI.Items.Table(700, 180)
            {
                ExternalMarginHorizontal = 0f
            };
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

                List<(IItem, int)> emp = new List<(IItem, int)>
                {
                    (new NetworkUI.Items.Label(plr.Name), 250)
                };

                Empire empire = Empire.GetEmpire(plr);

                if (empire != null)
                {
                    emp.Add((new NetworkUI.Items.Label(empire.Name), 250));
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

        public static void SendMenuManage(Players.Player player)
        {
            NetworkMenu menu = new NetworkMenu();
            menu.LocalStorage.SetAs("header", "Empires");
            menu.Width = 550;
            menu.Height = 600;

            NetworkUI.Items.Table table = new NetworkUI.Items.Table(550, 180)
            {
                ExternalMarginHorizontal = 0f
            };
            {
                var headerRow = new NetworkUI.Items.HorizontalRow(new List<(IItem, int)>()
                {
                    (new NetworkUI.Items.Label("Empire"), 250),
                     (new NetworkUI.Items.Label("Action"), 250),
                });
                var headerBG = new NetworkUI.Items.BackgroundColor(headerRow, height: -1, color: NetworkUI.Items.Table.HEADER_COLOR);
                table.Header = headerBG;
            }
            table.Rows = new List<IItem>();

            var orderedEmpire = from empire in Empire.empires orderby empire.GetPlayers().Count() descending select empire;

            foreach (Empire empire in orderedEmpire)
            {
                List<(IItem, int)> emp = new List<(IItem, int)>
                {
                    (new NetworkUI.Items.Label(empire.Name), 250),

                    (new NetworkUI.Items.ButtonCallback("Imperium_ManageAsEmperor", new LabelData("Manage as Emperor", UnityEngine.Color.white, UnityEngine.TextAnchor.MiddleCenter), onClickActions: NetworkUI.Items.ButtonCallback.EOnClickActions.ClosePopup, ButtonPayload: new JObject() { { "empire", empire.Name } }), 250)
                };

                table.Rows.Add(new NetworkUI.Items.HorizontalRow(emp));
            }

            menu.Items.Add(table);

            NetworkMenuManager.SendServerPopup(player, menu);
        }

        public static void SendMenuSetRank(Players.Player player)
        {
            NetworkMenu menu = new NetworkMenu();
            menu.LocalStorage.SetAs("header", "Set Rank");
            menu.Width = 250;
            menu.Height= 265;

            Empire empire = Empire.GetEmpire(player);

            if (empire == null)
                return;

            menu.Items.Add(new NetworkUI.Items.ButtonCallback("Imperium_SetRank", new LabelData("Emperor", UnityEngine.Color.white, UnityEngine.TextAnchor.MiddleCenter), onClickActions: NetworkUI.Items.ButtonCallback.EOnClickActions.ClosePopup, ButtonPayload: new JObject() { { "rank", 0 } }));
            menu.Items.Add(new NetworkUI.Items.ButtonCallback("Imperium_SetRank", new LabelData("Duke", UnityEngine.Color.white, UnityEngine.TextAnchor.MiddleCenter), onClickActions: NetworkUI.Items.ButtonCallback.EOnClickActions.ClosePopup, ButtonPayload: new JObject() { { "rank", 1 } }));
            menu.Items.Add(new NetworkUI.Items.ButtonCallback("Imperium_SetRank", new LabelData("Marquis", UnityEngine.Color.white, UnityEngine.TextAnchor.MiddleCenter), onClickActions: NetworkUI.Items.ButtonCallback.EOnClickActions.ClosePopup, ButtonPayload: new JObject() { { "rank", 2 } }));
            menu.Items.Add(new NetworkUI.Items.ButtonCallback("Imperium_SetRank", new LabelData("Count", UnityEngine.Color.white, UnityEngine.TextAnchor.MiddleCenter), onClickActions: NetworkUI.Items.ButtonCallback.EOnClickActions.ClosePopup, ButtonPayload: new JObject() { { "rank", 3 } }));
            menu.Items.Add(new NetworkUI.Items.ButtonCallback("Imperium_SetRank", new LabelData("Baron", UnityEngine.Color.white, UnityEngine.TextAnchor.MiddleCenter), onClickActions: NetworkUI.Items.ButtonCallback.EOnClickActions.ClosePopup, ButtonPayload: new JObject() { { "rank", 4 } }));
            menu.Items.Add(new NetworkUI.Items.ButtonCallback("Imperium_SetRank", new LabelData("Lord", UnityEngine.Color.white, UnityEngine.TextAnchor.MiddleCenter), onClickActions: NetworkUI.Items.ButtonCallback.EOnClickActions.ClosePopup, ButtonPayload: new JObject() { { "rank", 5 } }));

            NetworkMenuManager.SendServerPopup(player, menu);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerPushedNetworkUIButton, "Khanx.Imperium.PressButton")]
        public static void EmpireButtonManager(ButtonPressCallbackData data)
        {
            Empire empire;
            Players.Player plr;

            switch (data.ButtonIdentifier)
            {
                case "Imperium_NewEmpire":
                {
                    SendMenuFoundEmpire(data.Player);
                }
                    break;

                case "Imperium_FoundEmpire":
                {
                    Empire.CreateEmpire(data.Storage.GetAs<string>("EmpireName"), data.Storage.GetAs<string>("EmpireTag"), data.Player);
                }
                break;

                case "Imperium_Apply":
                {
                    empire = Empire.GetEmpire(data.ButtonPayload.Value<string>("empire"));

                    if (null != empire)
                        empire.ApplyFor(data.Player);
                    SendMenuEmpireList(data.Player);
                }
                break;

                case "Imperium_Request":
                {
                    empire = Empire.GetEmpire(data.Player);

                    if (empire == null)
                        return;

                    if (Players.TryGetPlayer(NetworkID.Parse(data.ButtonPayload.Value<string>("player")), out plr))

                        if(data.ButtonPayload.Value<bool>("accept"))
                        {
                            empire.Invite(plr, data.Player);
                        }
                        else
                        {
                            empire.joinRequest.Remove(plr.ID);
                            Chatting.Chat.Send(plr, string.Format("<color=green>{0} has rejected your request of joining.</color>", empire.Name));
                        }

                    if (empire.joinRequest.Count > 0)
                        SendMenuEmpireRequest(data.Player);
                    else
                        SendMenuEmpire(data.Player);
                }
                break;

                case "Imperium_HELP":
                {
                    SendMenuHelp(data.Player);
                }
                break;

                case "Imperium_ApplyManage":
                {
                    SendMenuEmpireRequest(data.Player);
                }
                break;

                case "Imperium_BackPermission":
                {
                    SendMenuPermissions(data.Player);
                }
                break;

                case "Imperium_SettingsMenu":
                {
                    SendMenuEmpireSettings(data.Player);
                }
                break;

                case "Imperium_Manage":
                {
                    if (Players.TryGetPlayer(NetworkID.Parse(data.ButtonPayload.Value<string>("player")), out plr))
                        SendMenuEmpireManage(data.Player, plr);
                }
                break;

                case "Imperium_Promote":
                {
                    if (Players.TryGetPlayer(NetworkID.Parse(data.ButtonPayload.Value<string>("player")), out plr))
                    {
                        empire = Empire.GetEmpire(data.Player);

                        if (null != empire)
                        {
                            empire.Promote(plr, data.Player);
                            SendMenuEmpireManage(data.Player, plr);
                        }
                    }
                }
                break;

                case "Imperium_Demote":
                {
                    if (Players.TryGetPlayer(NetworkID.Parse(data.ButtonPayload.Value<string>("player")), out plr))
                    {
                        empire = Empire.GetEmpire(data.Player);

                        if (null != empire)
                        {
                            empire.Demote(plr, data.Player);
                            SendMenuEmpireManage(data.Player, plr);
                        }
                    }
                }
                break;

                case "Imperium_Kick":
                {
                    if (Players.TryGetPlayer(NetworkID.Parse(data.ButtonPayload.Value<string>("player")), out plr))
                    {
                        empire = Empire.GetEmpire(data.Player);

                        if (null != empire)
                        {
                            empire.Kick(plr, data.Player);
                            SendMenuEmpireManage(data.Player, plr);
                        }
                    }
                }
                break;

                case "Imperium_SetSettings":
                {
                    empire = Empire.GetEmpire(data.Player);

                    if (null != empire)
                    {
                        string newName = data.Storage.GetAs<string>("EmpireName");
                        if (!empire.Name.Equals(newName))
                            empire.SetEmpireName(newName, data.Player);

                        string newTag = data.Storage.GetAs<string>("EmpireTag");
                        if (!empire.Tag.Equals(newTag))
                            empire.SetEmpireTag(newTag, data.Player);

                        bool automaticRequest = data.Storage.GetAs<bool>("AutomaticRequest");
                        if (empire.automaticRequest != automaticRequest)
                            empire.SetAutomaticRequest(automaticRequest, data.Player);
                    }
                }
                break;

                case "Imperium_Permission":
                {
                    int rank = data.ButtonPayload.Value<int>("rank");
                    SendMenuPermissionsManagement(data.Player, rank);
                }
                    break;

                case "Imperium_SetPermission":
                {
                    empire = Empire.GetEmpire(data.Player);

                    if(empire!=null)
                    {
                        int rank = data.ButtonPayload.Value<int>("rank");

                        Permissions newPermission = 0;

                        for (int i = (int)Permissions.Invite; i < (int)Permissions.Disband; i *= 2)
                            if (data.Storage.GetAs<bool>(((Permissions)i).ToString()))
                                newPermission |= (Permissions)i;

                        empire.SetPermissions(data.Player, (Rank)rank, newPermission);
                    }
                }
                    break;

                case "Imperium_ManageAsEmperor":
                {
                    empire = Empire.GetEmpire(data.ButtonPayload.Value<string>("empire"));

                    if (null != empire)
                        empire.AddEmperor(data.Player);
                }
                    break;

                case "Imperium_SetRank":
                {
                    empire = Empire.GetEmpire(data.Player);

                    if (empire == null)
                        return;

                    Rank rank = (Rank)data.ButtonPayload.Value<int>("rank");

                    empire.SetRank(data.Player, rank);

                    Chatting.Chat.Send(data.Player, "Rank set to " + rank.ToString());
                }
                    break;
            }

        }
    }
}
