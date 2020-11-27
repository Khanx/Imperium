using System;
using System.Collections.Generic;
using Chatting;

namespace Imperium.Commands
{
    [ChatCommandAutoLoader]
    public class AutomaticChat : IChatCommand
    {
        public List<Players.Player> activeTeamChat { get; } = new List<Players.Player>();

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

            if(activeTeamChat.Contains(player) && empire == null)
            {
                activeTeamChat.Remove(player);

                return false;
            }

            if(chat.StartsWith("/") || !activeTeamChat.Contains(player))
                return false;

            string name = player.Name;

            foreach(Players.Player plr in empire.GetConnectedPlayers())
                Chatting.Chat.Send(plr, string.Format("<color=yellow>[{0}][{1}]: {2}</color>", empire.GetRank(player).ToString(), name, chat));

            return true;
        }
    }
}
