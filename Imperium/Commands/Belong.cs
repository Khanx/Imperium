using Chatting;
using System.Collections.Generic;

namespace Imperium.Commands
{
    [ChatCommandAutoLoader]
    public class Belong : IChatCommand
    {
        public bool TryDoCommand(Players.Player player, string chat, List<string> splits)
        {
            if (!chat.Trim().ToLower().Equals("/belong_empire"))
                return false;


            EmpireMenu.SendMenuBelong(player);

            return true;
        }
    }
}
