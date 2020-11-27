using Chatting;
using System.Collections.Generic;


namespace Imperium.Commands
{
    [ChatCommandAutoLoader]
    public class Empires : IChatCommand
    {
        public bool TryDoCommand(Players.Player player, string chat, List<string> splits)
        {
            if (!chat.Trim().ToLower().Equals("/empires"))
                return false;

            EmpireMenu.SendMenuEmpireList(player);

            return true;
        }
    }
}
