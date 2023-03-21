using Pipliz;
using ModLoaderInterfaces;
using Imperium.Commands;

namespace Imperium
{
    public class PlayerCD : IOnPlayerConnectedLate, IOnPlayerDisconnected
    {
        public void OnPlayerConnectedLate(Players.Player player)
        {
            Empire empire = Empire.GetEmpire(player);

            if (empire == null)
                return;

            if(empire.joinRequest.Count > 0 && empire.CanPermission(player.ID.ID.ID, Permissions.Invite))
            {
                Chatting.Chat.Send(player, "<color=green> Someone has requested to join your empire, you can manage requests in /empire -> Manage applications </color>");
            }

        }

        public void OnPlayerDisconnected(Players.Player player)
        {
            AutomaticChat.ActiveTeamChat.Remove(player);
        }
    }
}
