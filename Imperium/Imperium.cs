using Imperium.Commands;
using Pipliz;

namespace Imperium
{
    [ModLoader.ModManager]
    public static class Imperium
    {

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
                    Chatting.Chat.Send(plr, string.Format("<color=yellow>[{0}]</color>{1}> <color=yellow>{2}</color>", empire.GetRank(causedBy).ToString(), Name, Text));

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
