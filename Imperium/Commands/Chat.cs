﻿using Chatting;
using System.Collections.Generic;

namespace Imperium.Commands
{
    [ChatCommandAutoLoader]
    public class Chat : IChatCommand
    {
        public bool TryDoCommand(Players.Player player, string chat, List<string> splits)
        {
            if (!chat.Trim().ToLower().StartsWith("/ce"))
                return false;

            Empire empire = Empire.GetEmpire(player);

            if(null == empire)
            {
                Chatting.Chat.Send(player, "<color=orange>You do not belong to any empire.</color>");

                return true;
            }

            string name = player.Name;

            foreach(Players.Player plr in empire.GetConnectedPlayers())
                Chatting.Chat.Send(plr, string.Format("<color=yellow>[{0}][{1}]: {2}</color>", empire.GetRank(player).ToString(), name, chat.Substring(4)));

            return true;
        }
    }
}
