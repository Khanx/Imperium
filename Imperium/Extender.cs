using static Players;

namespace ExtensionMethods
{
    static class Extender
    {
        public static Player GetPlayerByID(this PlayerIDShort playerIDShort)
        {
            TryGetPlayer(playerIDShort, out Player plr);

            return plr;
        }
    }
}
