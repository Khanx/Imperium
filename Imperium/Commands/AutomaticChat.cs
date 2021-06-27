using System;
using System.Collections.Generic;
using Chatting;

namespace Imperium.Commands
{
    [ChatCommandAutoLoader]
    public class AutomaticChat : IChatCommand
    {
        public static List<Players.Player> activeTeamChat { get; } = new List<Players.Player>();

        public bool TryDoCommand(Players.Player player, string chat, List<string> splits)
        {
            Empire empire = Empire.GetEmpire(player);

            if(chat.Equals("/chat_empire", StringComparison.OrdinalIgnoreCase))
            {
                if(empire == null)
                {
                    Chatting.Chat.Send(player, "<color=orange>You do not belong to any empire.</color>");

                    return true;
                }

                if(!activeTeamChat.Contains(player))
                {
                    Chatting.Chat.Send(player, "<color=green>Activated the automatic chat empire.</color>");
                    activeTeamChat.Add(player);

                    return true;
                }
                else
                {
                    Chatting.Chat.Send(player, "<color=green>Desactivated the automatic chat empire.</color>");
                    activeTeamChat.Remove(player);

                    return true;
                }
            }
            
            if (chat.StartsWith("/"))
                return false;
            
            if (activeTeamChat.Contains(player) && empire == null)
            {
                activeTeamChat.Remove(player);

                return false;
            }
            
            if (ColonyCommands.ColonyCommandsMod.ColonyCommands)
                return true;

            if(!activeTeamChat.Contains(player) && empire != null && !empire.tag.Equals(""))
            {
                Chatting.Chat.SendToConnected(string.Format("{0}[<color=green>{1}</color>]: {2}", player.Name, empire.tag, chat));

                return true;
            }
            
            foreach (Players.Player plr in empire.GetConnectedPlayers())
                Chatting.Chat.Send(plr, string.Format("<color=yellow>[{0}][{1}]: {2}</color>", empire.GetRank(player).ToString(), player.Name, chat));

            return true;
        }
    }
}
