using Imperium.Commands;
using Pipliz;

namespace Imperium
{
    [ModLoader.ModManager]
    public static class Imperium
    {
        public static bool ColonyCommandsMod = false;

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterModsLoaded, "Khanx.Imperium.AfterModsLoaded")]
        public static void AfterModsLoaded(System.Collections.Generic.List<ModLoader.ModDescription> mods)
        {
            for (int i = 0; i < mods.Count; i++)
            {
                if (mods[i].name == "Colony Commands")
                {
                    ColonyCommandsMod = true;
                    Log.Write("Imperium: found ColonyCommands mod, disabling chat");
                }
            }
        }

        /// <summary>
        /// This method implements the AutomaticChat for the Colony Commands Mod in order to avoid conflicts between mods
        /// </summary>
        /// 
        /// <param name="causedBy">Player</param>
        /// <param name="Name"> Player name</param>
        /// <param name="Prefix"> Prefix to add</param>
        /// <param name="Text"> Text to send</param>
        /// <returns></returns>
        public static bool ChatMarker(Players.Player causedBy, string Name, string Prefix, string Text)
        {
            Empire empire = Empire.GetEmpire(causedBy);

            if (AutomaticChat.activeTeamChat.Contains(causedBy) && empire != null)
            {
                foreach (Players.Player plr in empire.GetConnectedPlayers())
                    Chatting.Chat.Send(plr, string.Format("<color=yellow>[{0}][{1}]: {2}</color>", empire.GetRank(causedBy).ToString(), Name, Text));

                return true;
            }

            if(empire != null && !empire.tag.Equals(""))
            {
                Prefix += "[<color=green>" + empire.tag + "</color>]";

                Chatting.Chat.SendToConnected($"{Name}{Prefix}> {Text}");

                return true;
            }

            return false;
        }
    }
}
