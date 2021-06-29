using Chatting;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Imperium.Commands
{
    [ChatCommandAutoLoader]
    public class Belong : IChatCommand
    {
        public bool TryDoCommand(Players.Player player, string chat, List<string> splits)
        {
            if (!splits[0].ToLower().Equals("/belong_empire"))
                return false;

            if (splits.Count == 1)
            {
                EmpireMenu.SendMenuBelong(player);
            }
            else
            {
                var m = Regex.Match(chat, @"/belong_empire( (?<targetplayername>['].+[']|[^ ]+))?");

                if (!m.Success)
                {
                    Chatting.Chat.Send(player, "Syntax: /belong_empire [targetplayername]");
                    return true;
                }

                string targetPlayerName = m.Groups["targetplayername"].Value;

                if (!PlayerHelper.TryGetPlayer(targetPlayerName, out Players.Player targetPlayer, out string error, true))
                {
                    Chatting.Chat.Send(player, $"Could not find '{targetPlayerName}'; {error}");
                    return true;
                }

                Empire empire = Empire.GetEmpire(targetPlayer);

                if (empire == null)
                {
                    Chatting.Chat.Send(player, $"'{targetPlayerName}' does not belong to any empire.");
                }
                else
                {
                    Chatting.Chat.Send(player, $"'{targetPlayerName}' belong to '{empire.Name}'.");
                }
            }

            return true;
        }
    }
}
