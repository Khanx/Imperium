using Chatting;
using System.Collections.Generic;

namespace Imperium.Commands
{
    [ChatCommandAutoLoader]
    public class MEmpire : IChatCommand
    {
        public bool TryDoCommand(Players.Player player, string chat, List<string> splits)
        {
            if (!chat.Trim().ToLower().Equals("/empire"))
                return false;

            Empire empire = Empire.GetEmpire(player);

            if (empire == null)
                EmpireMenu.SendMenuEmpireList(player);
            else
                EmpireMenu.SendMenuEmpire(player);

            return true;
        }
    }
}
