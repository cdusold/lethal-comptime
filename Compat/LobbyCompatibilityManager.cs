using BepInEx;
using LobbyCompatibility.Enums;
using LobbyCompatibility.Features;

namespace LethalCompTime.Compat
{
    [BepInDependency("BMX.LobbyCompatibility")]
    internal class LobbyCompatibilityManager
    {
        public static LobbyCompatibilityManager Instance { get; private set; }

        public static void Init()
        {
            if (Instance == null)
            {
                Instance = new LobbyCompatibilityManager();
            }
        }

        private LobbyCompatibilityManager()
        {
            PluginHelper.RegisterPlugin(PluginInfo.PLUGIN_GUID, new System.Version(PluginInfo.PLUGIN_VERSION), CompatibilityLevel.ClientOnly, VersionStrictness.None);
        }
    }
}