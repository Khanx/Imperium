using Chatting;
using System.Collections.Generic;

namespace Imperium.Commands
{
    [ChatCommandAutoLoader]
    public class Disband : IChatCommand
    {
        public bool TryDoCommand(Players.Player player, string chat, List<string> splits)
        {
            if (!chat.Trim().ToLower().Equals("/disband_empire"))
                return false;

            Empire empire = Empire.GetEmpire(player);

            if (null == empire)
            {
                Chatting.Chat.Send(player, "<color=orange>You do not belong to any empire.</color>");
                return true;
            }

            empire.Disband(player);

            return true;
        }
    }
}
