using System;
using System.Collections.Generic;
using Chatting;

namespace Imperium.Commands
{
    [ChatCommandAutoLoader]
    public class AutomaticChat : IChatCommand
    {
        public static List<Players.Player> ActiveTeamChat { get; } = new List<Players.Player>();

        public bool TryDoCommand(Players.Player player, string chat, List<string> splits)
        {
            Empire empire = Empire.GetEmpire(player);

            if (chat.Equals("/chat_empire", StringComparison.OrdinalIgnoreCase))
            {
                if (empire == null)
                {
                    Chatting.Chat.Send(player, "<color=orange>You do not belong to any empire.</color>");

                    return true;
                }

                if (!ActiveTeamChat.Contains(player))
                {
                    Chatting.Chat.Send(player, "<color=green>Activated the automatic chat empire.</color>");
                    ActiveTeamChat.Add(player);

                    return true;
                }
                else
                {
                    Chatting.Chat.Send(player, "<color=green>Desactivated the automatic chat empire.</color>");
                    ActiveTeamChat.Remove(player);

                    return true;
                }
            }

            if (chat.StartsWith("/"))
                return false;

            if (ActiveTeamChat.Contains(player) && empire == null)
            {
                ActiveTeamChat.Remove(player);

                return false;
            }

            if (ColonyCommands.ColonyCommandsMod.ColonyCommands)
                return true;

            if (!ActiveTeamChat.Contains(player) && empire != null && !empire.Tag.Equals(""))
            {
                Chatting.Chat.SendToConnected(string.Format("{0}[<color=green>{1}</color>]: {2}", player.Name, empire.Tag, chat));

                return true;
            }

            if (!ActiveTeamChat.Contains(player))
            {
                Chatting.Chat.SendToConnected(string.Format("{0}: {2}", player.Name, chat));

                return true;
            }

            foreach (Players.Player plr in empire.GetConnectedPlayers())
            Chatting.Chat.Send(plr, string.Format("<color=yellow>[{0}][{1}]: {2}</color>", empire.GetRank(player).ToString(), player.Name, chat));

            return true;
        }
    }
}
