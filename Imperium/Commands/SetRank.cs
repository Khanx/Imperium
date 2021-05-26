using Chatting;
using System.Collections.Generic;

namespace Imperium.Commands
{
    [ChatCommandAutoLoader]
    public class MSetRank : IChatCommand
    {
        public bool TryDoCommand(Players.Player player, string chat, List<string> splits)
        {
            if (!chat.Trim().ToLower().Equals("/empire_setrank"))
                return false;

            if (!PermissionsManager.CheckAndWarnPermission(player, "khanx.imperium"))
            {
                return true;
            }

            if (Empire.GetEmpire(player) == null)
            {
                Chatting.Chat.Send(player, "<color=orange>You do not belong to any empire.</color>");
                return true;
            }

            EmpireMenu.SendMenuSetRank(player);

            return true;
        }
    }
}
