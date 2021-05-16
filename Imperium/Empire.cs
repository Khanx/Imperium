using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Pipliz;
using Pipliz.JSON;


namespace Imperium
{
    public enum Rank
    {
        Emperor,
        Duke,
        Marquis,
        Count,
        Baron,
        Lord,
        None
    }

    [Flags]
    public enum Permissions
    {
        Chat = 1 << 0,   //Use the chat
        //OfficerChat = 1 << 1,    //Use the officer chat
        Invite = 1 << 1,    //Add players
        Kick = 1 << 2,   //Kick players
        Ranks = 1 << 3,   //Promote & demote ranks
        //Announcement = 1 << 4, //Change the current announcement
        Disband = 1 << 4    //Destroy the empire D':
        //Teleport to other players of the guild
        //War ?
    }

    /*
    public struct Member
    {
        public NetworkID networkID;
        public Rank rank;

        public Member(NetworkID networkID, Rank rank)
        {
            this.networkID = networkID;
            this.rank = rank;
        }

        public override bool Equals(object obj)
        {
            if(!( obj is Member ))
                return false;

            return ( (Member)obj ).networkID == networkID;
        }
    }
    */
    [ModLoader.ModManager]
    public class Empire
    {
        public static List<Empire> empires = new List<Empire>();

        public string name { get; internal set; } = "";
        public string tag { get; internal set; } = "";
        public static readonly string[] NoTag = { "ANAL", "ANUS", "ARSE", "ASS", "BOOB", "BUM", "BUTT", "CAWK", "CIPA", "CLIT", "CNUT", "COCK", "COK", "COON", "COX", "CRAP", "CUM", "CUMS", "CUNT", "DAMN", "DICK", "DINK", "DLCK", "DYKE", "FAG", "FAGS", "FCUK", "FECK", "FOOK", "FUCK", "FUK", "FUKS", "FUX", "HELL", "HOAR", "HOER", "HOMO", "HORE", "JAP", "JISM", "JIZ", "JIZM", "JIZZ", "KAWK", "KNOB", "KOCK", "KUM", "KUMS", "LUST", "MOFO", "MUFF", "NAZI", "NOB", "PAWN", "PHUK", "PHUQ", "PISS", "POOP", "PORN", "PRON", "PUBE", "SEX", "SHAG", "SHIT", "SLUT", "SMUT", "SPAC", "TEEZ", "TIT", "TITS", "TITT", "TURD", "TWAT", "WANG", "WANK", "XXX", "DAGO", "DIKE", "GAY", "GOOK", "HEEB", "HO", "HOE", "KIKE", "KUNT", "KYKE", "MICK", "PAKI", "POON", "PUTO", "SHIZ", "SMEG", "SPIC", "TARD", "VAG", "WOP", "FOAH", "PUST", "SEKS", "SLAG", "ZUBB", "BBW", "BDSM", "DVDA", "GURO", "MILF", "NUDE", "ORGY", "POOF", "PTHC", "QUIM", "RAPE", "SCAT", "SEXO", "SEXY", "SUCK", "SMUT", "YURI", "YAOI", "XX", "XXX", "XXXX" };
        //public string announcement { get; internal set; } = "Test Announcement";

        public readonly List<NetworkID> joinRequest = new List<NetworkID>();         //People who has requested to join the empire but have not accepted / rejected
        public bool automaticRequest;
        
        private Dictionary<NetworkID, Rank> members = new Dictionary<NetworkID, Rank>();
        private Permissions[] permissions =
        {
            //Emperor
            Permissions.Chat /*| Permissions.OfficerChat*/ | Permissions.Invite | Permissions.Kick | Permissions.Ranks /* | Permissions.Announcement */ | Permissions.Disband,
            //Duke
            Permissions.Chat /*| Permissions.OfficerChat*/ | Permissions.Invite | Permissions.Kick | Permissions.Ranks /* | Permissions.Announcement */,
            //Marquis
            Permissions.Chat /*| Permissions.OfficerChat*/ | Permissions.Invite | Permissions.Ranks,
            //Count
            Permissions.Chat |  Permissions.Invite,
            //Baron
            Permissions.Chat,
            //Lord
            Permissions.Chat,
        };

        public static Empire GetEmpire(string name)
        {
            foreach(Empire empire in empires)
                if(name.Equals(empire.name))
                    return empire;

            return null;
        }

        public static Empire GetEmpire(Players.Player player)
        {
            foreach(Empire empire in empires)
                if(empire.members.ContainsKey(player.ID))
                    return empire;

            return null;
        }

        public static bool NameUsed(string name)
        {
            return GetEmpire(name) != null;
        }

        public static bool TagUsed(string tag)
        {
            tag = tag.ToUpper().Trim();

            foreach (Empire empire in empires)
                if (empire.tag.Equals(tag))
                    return true;
            return false;
        }

        public static bool AllowedTag(string tag)
        {
            tag = tag.ToUpper().Trim();

            foreach (string nt in NoTag)
                if (nt.Equals(tag))
                    return false;

            return true;
        }

        public static bool CreateEmpire(string name, string tag, Players.Player emperor)
        {
            if(null != GetEmpire(emperor))
            {
                Chatting.Chat.Send(emperor, "<color=orange>You have to leave your current empire before creating one.</color>");
                return false;
            }

            name = name.Trim();
            
            if (name.Length < 4 || name.Length > 50)
            {
                Chatting.Chat.Send(emperor, "<color=orange>The name of your empire needs to have between 4 and 50 characters.</color>");
                return false;
            }

            name = char.ToUpper(name[0]) + name.Substring(1);

            if(NameUsed(name))
            {
                Chatting.Chat.Send(emperor, "<color=orange>There is already an empire with that name.</color>");
                return true;
            }

            new Empire(name, emperor);

            Chatting.Chat.Send(emperor, "<color=yellow>You have founded an empire.</color>");

            Empire empire = GetEmpire(emperor);

            if (!empire.SetEmpireTag(tag, emperor))
                Chatting.Chat.Send(emperor, "<color=orange>You can set the tag of your empire in the settings of your empire.</color>");

            return true;
        }

        private Empire(string name, Players.Player emperor)
        {
            this.name = name;
            members.Add(emperor.ID, Rank.Emperor);
            empires.Add(this);
        }

        public void Disband(Players.Player player)
        {
            if(!members.ContainsKey(player.ID))
            {
                Chatting.Chat.Send(player, "<color=orange>You do not belong to any empire.</color>");
                return;
            }

            if(Rank.Emperor != GetRank(player))
            {
                Chatting.Chat.Send(player, "<color=orange>Only the emperor can disband the empire.</color>");
                return;
            }

            string message = "<color=yellow>The empire has been disbanded.</color>";

            foreach(Players.Player plr in GetPlayers())
            {
                Chatting.Chat.Send(player, message);
                members.Remove(plr.ID);
            }

            empires.Remove(this);
        }

        public List<Players.Player> GetPlayers()
        {
            List<Players.Player> players = new List<Players.Player>();

            foreach(NetworkID networkID in members.Keys)
            {
                Players.Player player = Players.GetPlayer(networkID);
                if(null != player)
                    players.Add(player);
            }

            return players;
        }

        public List<Players.Player> GetConnectedPlayers()
        {
            List<Players.Player> players = new List<Players.Player>();

            foreach(NetworkID networkID in members.Keys)
            {
                Players.Player player = Players.GetPlayer(networkID);
                if(null != player && player.ConnectionState == Players.EConnectionState.Connected)
                    players.Add(player);
            }

            return players;
        }

        public List<Players.Player> GetPlayersOrderedByRank()
        {
            List<Players.Player>[] help = new List<Players.Player>[(int)Rank.None];

            for(int i = 0; i < help.Length; i++)
                help[i] = new List<Players.Player>();

            foreach(NetworkID networkID in members.Keys)
            {
                Players.Player player = Players.GetPlayer(networkID);
                if(null != player)
                    help[(int)members[networkID]].Add(player);
            }

            List<Players.Player> players = new List<Players.Player>();

            for(int i = 0; i < help.Length; i++)
            {
                players.AddRange(help[i]);
                help[i].Clear();
            }

            return players;
        }

        public Rank GetRank(Players.Player player)
        {
            if(members.ContainsKey(player.ID))
                return members[player.ID];

            return Rank.None;
        }

        public bool CanPermission(Rank rank, Permissions permission)
        {
            return ( permissions[(int)rank] & permission ) == permission;
        }

        public bool CanPermission(Players.Player player, Permissions permission)
        {
            return CanPermission(GetRank(player), permission);
        }

        public bool SetEmpireName(string name, Players.Player player)
        {
            if(!members.ContainsKey(player.ID))
            {
                Chatting.Chat.Send(player, "<color=orange>You do not belong to any empire.</color>");
                return false;
            }

            if(Rank.Emperor != GetRank(player))
            {
                Chatting.Chat.Send(player, "<color=orange>Only the emperor can change the name of the empire.</color>");
                return false;
            }

            name = name.Trim();

            if(name.Length < 4 || name.Length > 50)
            {
                Chatting.Chat.Send(player, "<color=orange>The name of your empire needs to have between 4 and 50 characters.</color>");
                return false;
            }

            this.name = char.ToUpper(name[0]) + name.Substring(1);

            string message = string.Format("<color=yellow>{0} is the new name of the empire.</color>", this.name);
            foreach(Players.Player plr in GetConnectedPlayers())
                Chatting.Chat.Send(plr, message);

            return true;
        }

        public bool SetEmpireTag(string tag, Players.Player player)
        {
            if (tag.Equals(""))
                return false;

            if (!members.ContainsKey(player.ID))
            {
                Chatting.Chat.Send(player, "<color=orange>You do not belong to any empire.</color>");
                return false;
            }

            if (Rank.Emperor != GetRank(player))
            {
                Chatting.Chat.Send(player, "<color=orange>Only the emperor can change the name of the empire.</color>");
                return false;
            }

            tag = tag.ToUpper().Trim();

            if (tag.Length > 4)
            {
                Chatting.Chat.Send(player, "<color=orange>The tag of your empire must be less than 5 letters.</color>");
                return false;
            }

            if(!tag.All(Char.IsLetter))
            {
                Chatting.Chat.Send(player, "<color=orange>The tag can only contain letters.</color>");
                return false;
            }

            if (TagUsed(tag))
            {
                Chatting.Chat.Send(player, "<color=orange>There is already an empire with that tag.</color>");
                return false;
            }

            if(!AllowedTag(tag))
            {
                Chatting.Chat.Send(player, "<color=orange>It is not allowed to use that tag.</color>");
                return false;
            }

            this.tag = tag;

            string message = string.Format("<color=yellow>{0} is the new tag of the empire.</color>", this.tag);
            foreach (Players.Player plr in GetConnectedPlayers())
                Chatting.Chat.Send(plr, message);

            return true;
        }

        /* K: ANNOUNCEMENT SYSTEM
        public void SetAnnouncement(string announcement, Players.Player player)
        {
            if(!members.ContainsKey(player.ID))
            {
                Chatting.Chat.Send(player, "<color=orange>You do not belong to any empire.</color>");
                return;
            }


            if(!CanPermission(player, Permissions.Announcement))
            {
                Chatting.Chat.Send(player, "<color=orange>You do not have permission to change the announcement.</color>");
                return;
            }

            this.announcement = announcement;

            foreach(Players.Player plr in GetConnectedPlayers())
                Chatting.Chat.Send(plr, announcement);
        }
        */

        public void SetAutomaticRequest(bool automaticRequest, Players.Player player)
        {
            if (Rank.Emperor != GetRank(player))
            {
                Chatting.Chat.Send(player, "<color=orange>Only the emperor can change the name of the empire.</color>");
                return;
            }

            this.automaticRequest = automaticRequest;

            if(this.automaticRequest)
            { 
                foreach(var request in joinRequest)
                {
                    if (Players.TryGetPlayer(request, out Players.Player requester))
                    {
                        Invite(requester);
                        joinRequest.Remove(request);
                    }
                }
            }
        }

        public void SetPermissions(Players.Player player, Rank rank, Permissions permission)
        {
            if(!members.ContainsKey(player.ID))
            {
                Chatting.Chat.Send(player, "<color=orange>You do not belong to any empire.</color>");
                return;
            }

            if(Rank.Emperor != GetRank(player))
            {
                Chatting.Chat.Send(player, "<color=orange>Only the emperor can change the permissions.</color>");
                return;
            }

            permissions[(int)rank] = permission;
            Chatting.Chat.Send(player, "<color=orange>Permissions changed.</color>");
        }

        public void ApplyFor(Players.Player player)
        {
            if(null != Empire.GetEmpire(player))
            {
                Chatting.Chat.Send(player, "<color=orange>You can not apply because you already belong to an Empire.</color>");
                return;
            }

            if(joinRequest.Contains(player.ID))
            {
                Chatting.Chat.Send(player, "<color=orange>You have already requested to join this empire. Wait until they accept / reject your request to request joining again.</color>");
                return;
            }

            if(automaticRequest)
            {
                Invite(player);
                return;
            }

            joinRequest.Add(player.ID);
            Chatting.Chat.Send(player, string.Format("<color=green>You have requested to join to {0}.</color>", name));

            foreach(Players.Player plr in GetConnectedPlayers())
                Chatting.Chat.Send(plr, string.Format("<color=green>{0} has requested to join your empire. Only someone of sufficient rank can accept his request.</color>", player.Name));
        }

        //Change the behaviour of this method
        public void Invite(Players.Player player, Players.Player causedBy)
        {
            Rank rank = GetRank(causedBy);

            if(!CanPermission(causedBy, Permissions.Invite))
            {
                Chatting.Chat.Send(causedBy, "<color=orange>You do not have permission to invite.</color>");
                return;
            }

            if(null != Empire.GetEmpire(player))
            {
                Chatting.Chat.Send(causedBy, string.Format("<color=orange>{0} already belongs to an Empire.</color>", player.Name));

                if(joinRequest.Contains(player.ID))
                    joinRequest.Remove(player.ID);

                return;
            }

            string message = string.Format("<color=orange>{0} has invited {1} to join the Empire</color>", causedBy.Name, player.Name);
            foreach(Players.Player plr in GetConnectedPlayers())
                Chatting.Chat.Send(plr, message);


            members.Add(player.ID, Rank.Lord);
            Chatting.Chat.Send(player, string.Format("<color=green>{0} has accepted your request of joining.</color>", name));

            if (joinRequest.Contains(player.ID))
                joinRequest.Remove(player.ID);
        }

        public void Invite(Players.Player player)
        {
            if (null != Empire.GetEmpire(player))
            {
                Chatting.Chat.Send(player,"<color=orange>You already belongs to an Empire.</color>");

                if (joinRequest.Contains(player.ID))
                    joinRequest.Remove(player.ID);

                return;
            }

            if (joinRequest.Contains(player.ID))
                joinRequest.Remove(player.ID);

            members.Add(player.ID, Rank.Lord);

            string message = string.Format("<color=orange>{0} has joined join the Empire</color>", player.Name);
            foreach (Players.Player plr in GetConnectedPlayers())
                Chatting.Chat.Send(plr, message);
        }

        public void Quit(Players.Player player)
        {
            if(!members.ContainsKey(player.ID))
            {
                Chatting.Chat.Send(player, "<color=orange>You do not belong to any empire.</color>");
                return;
            }

            if(Rank.Emperor == GetRank(player))
            {
                Chatting.Chat.Send(player, "<color=orange>The emperor can not leave the empire without designating a new emperor.</color>");
                return;
            }

            members.Remove(player.ID);
            Chatting.Chat.Send(player, "<color=orange>You has left the empire.</color>");

            string message = string.Format("<color=orange>{0} has defected.</color>", player.Name);
            foreach(Players.Player plr in GetConnectedPlayers())
                Chatting.Chat.Send(plr, message);
        }

        public void Kick(Players.Player player, Players.Player causedBy)
        {
            if(!members.ContainsKey(causedBy.ID))
            {
                Chatting.Chat.Send(causedBy, "<color=orange>You do not belong to any empire.</color>");
                return;
            }

            if(!CanPermission(causedBy, Permissions.Kick))
            {
                Chatting.Chat.Send(causedBy, "<color=orange>You do not have permission to kick.</color>");
                return;
            }

            if(!members.ContainsKey(player.ID))
            {
                Chatting.Chat.Send(causedBy, string.Format("<color=orange>{0} does not belongs to your empire.</color>", player.Name));
                return;
            }

            Rank rankCausedBy = GetRank(causedBy);
            Rank rankPlayer = GetRank(player);

            if(rankPlayer <= rankCausedBy)
            {
                Chatting.Chat.Send(causedBy, "<color=orange>You can not kick someone of greater or equal rank to yours.</color>");
                return;
            }

            Chatting.Chat.Send(player, "<color=orange>You have been kicked of the empire.</color>");
            members.Remove(player.ID);

            //Notify ALL the players of the empire
            string message = string.Format("<color=orange>{0} has been kicked by {1}</color>", player.Name, causedBy.Name);
            foreach(Players.Player plr in GetConnectedPlayers())
                Chatting.Chat.Send(plr, message);

        }

        public void Promote(Players.Player player, Players.Player causedBy)
        {
            if(!members.ContainsKey(causedBy.ID))
            {
                Chatting.Chat.Send(causedBy, "<color=orange>You do not belong to any empire.</color>");
                return;
            }

            if(!CanPermission(causedBy, Permissions.Ranks))
            {
                Chatting.Chat.Send(causedBy, "<color=orange>You do not have permission to promote / demote.</color>");
                return;
            }

            if(!members.ContainsKey(player.ID))
            {
                Chatting.Chat.Send(causedBy, string.Format("<color=orange>{0} does not belongs to your empire.</color>", player.Name));
                return;
            }

            Rank rankPlayer = GetRank(player);
            Rank rankCausedBy = GetRank(causedBy);

            if(rankPlayer <= rankCausedBy)
            {
                Chatting.Chat.Send(causedBy, "<color=orange>You can only promote people with a lower rank than yours.</color>");
                return;
            }

            /* With this ENABLED there is only ONE emperor
            if(rankCausedBy == Rank.Emperor && rankPlayer == Rank.Duke)
            {
                members[player.ID] = Rank.Emperor;
                members[causedBy.ID] = Rank.Duke;

                string message = string.Format("<color=yellow>{0} is the new emperor of the empire.</color>", player.Name);
                foreach(Players.Player plr in GetConnectedPlayers())
                    Chatting.Chat.Send(plr, message);
                return;
            }*/

            members[player.ID] = rankPlayer - 1;
            string message2 = string.Format("<color=orange>{0} has been promoted to {1}.</color>", player.Name, members[player.ID].ToString());
            foreach(Players.Player plr in GetConnectedPlayers())
                Chatting.Chat.Send(plr, message2);
        }

        public void Demote(Players.Player player, Players.Player causedBy)
        {
            if(!members.ContainsKey(causedBy.ID))
            {
                Chatting.Chat.Send(causedBy, "<color=orange>You do not belong to any empire.</color>");
                return;
            }

            Rank rankCausedBy = GetRank(causedBy);

            if(!CanPermission(causedBy, Permissions.Ranks))
            {
                Chatting.Chat.Send(causedBy, "<color=orange>You do not have permission to promote / demote.</color>");
                return;
            }

            if(!members.ContainsKey(player.ID))
            {
                Chatting.Chat.Send(causedBy, string.Format("<color=orange>{0} does not belongs to your empire.</color>", player.Name));
                return;
            }

            Rank rankPlayer = GetRank(player);

            if(rankPlayer <= rankCausedBy && rankCausedBy != Rank.Emperor) //An emperor can DEMOTE another emperor
            {
                Chatting.Chat.Send(causedBy, "<color=orange>You can only demote people with a lower rank than yours.</color>");
                return;
            }

            if(rankPlayer == Rank.Lord)
            {
                Chatting.Chat.Send(causedBy, string.Format("<color=orange>{0} is the lowest rank.</color>", Rank.Lord.ToString()));
                return;
            }

            members[player.ID] = rankPlayer + 1;
            string message2 = string.Format("<color=orange>{0} has been demoted to {1}.</color>", player.Name, members[player.ID].ToString());
            foreach(Players.Player plr in GetConnectedPlayers())
                Chatting.Chat.Send(plr, message2);
        }

        public Empire(JSONNode json)
        {
            name = json.GetAs<string>("Name");
            tag = json.GetAsOrDefault<string>("Tag", "");

            //announcement = json.GetAs<string>("Announcement");
            automaticRequest = json.GetAs<bool>("automaticRequest");

            //Load Permissions
            int i = (int)Rank.Emperor;
            foreach(var permission in json.GetAs<JSONNode>("Permissions").LoopArray())
                permissions[i++] = (Permissions)Enum.Parse(typeof(Permissions), permission.GetAs<string>());

            //Load members
            foreach(var member in json.GetAs<JSONNode>("Members").LoopArray())
            {
                NetworkID nID = NetworkID.Parse(member.GetAs<string>("ID"));
                Rank rank = (Rank)Enum.Parse(typeof(Rank), member.GetAs<string>("Rank"));

                members.Add(nID, rank);
            }

            //Load Request
            foreach (var requests in json.GetAs<JSONNode>("Requests").LoopArray())
            {
                NetworkID nID = NetworkID.Parse(requests.GetAs<string>());
                joinRequest.Add(nID);
            }

            empires.Add(this);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterWorldLoad, "Khanx.Imperium.LoadEmpires")]
        public static void LoadEmpires()
        {
            Log.Write("<color=green>Loading empires</color>");
            string jsonFilePath = "./gamedata/savegames/" + ServerManager.WorldName + "/Imperium.json";

            if(!File.Exists(jsonFilePath))
                return;

            JSONNode json = JSON.Deserialize(jsonFilePath);

            foreach(JSONNode empire in json.LoopArray())
                new Empire(empire);
        }

        public JSONNode SaveEmpire()
        {
            JSONNode json = new JSONNode();
            json.SetAs<string>("Name", name);
            json.SetAs<string>("Tag", tag);
            //json.SetAs<string>("Announcement", announcement);
            json.SetAs<bool>("automaticRequest", automaticRequest);

            JSONNode permissionsjson = new JSONNode(NodeType.Array);

            foreach(var permission in permissions)
                permissionsjson.AddToArray(new JSONNode(permission.ToString()));

            json.SetAs("Permissions", permissionsjson);

            JSONNode membersjson = new JSONNode(NodeType.Array);
            foreach(var member in members.Keys)
            {
                JSONNode memberjson = new JSONNode();
                memberjson.SetAs<string>("ID", member.ToString());
                memberjson.SetAs<string>("Rank", members[member].ToString());

                membersjson.AddToArray(memberjson);
            }

            json.SetAs("Members", membersjson);

            JSONNode requestjson = new JSONNode(NodeType.Array);

            foreach (var request in joinRequest)
                requestjson.AddToArray(new JSONNode(request.ToString()));

            json.SetAs("Requests", requestjson);

            return json;
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnAutoSaveWorld, "Khanx.Imperium.SaveOnAutoSave")]
        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnQuit, "Khanx.Imperium.SaveOnQuit")]
        public static void SaveEmpires()
        {
            Log.Write("<color=green>Saving empires</color>");
            string jsonFilePath = "./gamedata/savegames/" + ServerManager.WorldName + "/Imperium.json";

            if(File.Exists(jsonFilePath))
                File.Delete(jsonFilePath);

            if(empires.Count == 0)
                return;

            JSONNode json = new JSONNode(NodeType.Array);

            foreach(Empire emp in empires)
                json.AddToArray(emp.SaveEmpire());

            JSON.Serialize(jsonFilePath, json);
        }
    }
}
