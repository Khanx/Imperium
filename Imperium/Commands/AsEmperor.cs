using Chatting;
using System.Collections.Generic;

namespace Imperium.Commands
{
    [ChatCommandAutoLoader]
    public class MAsEmperor : IChatCommand
    {
        public bool TryDoCommand(Players.Player player, string chat, List<string> splits)
        {
            if (!chat.Trim().ToLower().Equals("/empire_asemperor"))
                return false;

            if (!PermissionsManager.CheckAndWarnPermission(player, "khanx.imperium"))
            {
                return true;
            }

            EmpireMenu.SendMenuManage(player);

            return true;
        }
    }
}
