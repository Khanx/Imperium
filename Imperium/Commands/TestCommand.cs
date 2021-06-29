using Chatting;
using System.Collections.Generic;

namespace Imperium.Commands
{
    [ChatCommandAutoLoader]
    public class TestCommand : IChatCommand
    {
        public bool TryDoCommand(Players.Player player, string chat, List<string> splits)
        {
            if (!chat.Trim().ToLower().Equals("/empire_test"))
                return false;

            Chatting.Chat.SendToConnected("/empire_test detected");

            var m = ColonyCommands.ColonyCommandsMod.GetMethodFromColonyCommandsMod("ActivityTracker", "GetLastSeen");

            if (m != null)
                Chatting.Chat.SendToConnected("Method found");
            else
                Chatting.Chat.SendToConnected("Method not found");

            string lastseen = (string)m.Invoke(null, new object[] { player.ID.ToStringReadable() });

            if (!lastseen.Equals("never"))
                lastseen = lastseen.Substring(0, lastseen.IndexOf(" ")).Trim();

            Chatting.Chat.SendToConnected("Last seen: " + lastseen);

            return true;
        }
    }
}
