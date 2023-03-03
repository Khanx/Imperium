using Pipliz;
using System;
using System.Reflection;

namespace Imperium.ColonyCommands
{
    
    [ModLoader.ModManager]
    public static class ColonyCommandsMod
    {
        public static bool ColonyCommands = false;
        public static Assembly colonyCommandsMod = null;

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterModsLoaded, "Khanx.Imperium.AfterModsLoaded")]
        public static void ChatMarkerAfterModsLoaded(System.Collections.Generic.List<ModLoader.ModDescription> mods)
        {
            for (int i = 0; i < mods.Count; i++)
            {
                if (mods[i].name == "Colony Commands")
                {
                    ColonyCommands = true;
                    Log.Write("Imperium: found ColonyCommands mod, disabling chat");

                    colonyCommandsMod = mods[i].LoadedAssembly;
                }
            }
        }

        public static MethodInfo GetMethodFromColonyCommandsMod(string _class, string _method)
        {
            if (colonyCommandsMod != null)
            {
                foreach (Type t in colonyCommandsMod.GetTypes())
                {
                    if (t.FullName == "ColonyCommands." + _class)
                    {
                        MethodInfo m = t.GetMethod(_method);
                        if (m != null)
                        {
                            return m;
                        }
                    }
                }
            }

            return null;
        }
    }
}
