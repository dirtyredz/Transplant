using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace Transplant
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInProcess("Moonlight Peaks.exe")]
    public sealed class TransplantPlugin : BaseUnityPlugin
    {
        public const string PluginGuid = "com.dirtyredz.moonlightpeaks.transplant";
        public const string PluginName = "Transplant";
        // Single-sourced from <Version> in the csproj: Directory.Build.props generates
        // ModBuildInfo from it at build time, so this can never drift by hand. pack.ps1 names the
        // archive from that same <Version>, and BepInEx reports this. See 12-versioning-and-release.md.
        public const string PluginVersion = ModBuildInfo.Version;

        private Harmony harmony;

        private void Awake()
        {
            Plugin.Bind(Config, Logger);

            harmony = new Harmony(PluginGuid);
            harmony.PatchAll(typeof(DecorateStatePatches));
            harmony.PatchAll(typeof(DecorateStateProbe));
            harmony.PatchAll(typeof(SelectStatePatches));
            harmony.PatchAll(typeof(CanMoveGridViewPatch));
            harmony.PatchAll(typeof(CancelPatch));
            harmony.PatchAll(typeof(PlacementPatch));
            harmony.PatchAll(typeof(ShoutPatch));

            Plugin.Log.LogInfo(
                $"{PluginName} {PluginVersion} loaded. Nothing new is written to your save.");
        }

        private void OnDestroy()
        {
            harmony?.UnpatchSelf();
        }
    }

    /// <summary>
    /// Config and logging, kept off the plugin type so the patches can reach them without
    /// carrying a reference to the BaseUnityPlugin around.
    /// </summary>
    internal static class Plugin
    {
        internal static ManualLogSource Log;

        /// <summary>
        /// Diagnostic logging, silent unless the player asks for it.
        ///
        /// These lines ran unconditionally through 0.1.x because they were the only way to tell
        /// three identical-looking failures apart, and the run that needed them was always the
        /// run where nobody had thought to turn them on. That trade is right while debugging and
        /// wrong once shipped - a working mod has no business writing a line per cursor cell into
        /// everyone's log.
        /// </summary>
        internal static void Debug(string message)
        {
            if (VerboseLogging != null && VerboseLogging.Value)
            {
                Log.LogInfo(message);
            }
        }

        internal static ConfigEntry<bool> Enabled;
        internal static ConfigEntry<bool> RequireModifier;
        internal static ConfigEntry<KeyboardShortcut> Modifier;
        internal static ConfigEntry<bool> PressToToggle;
        internal static ConfigEntry<bool> RequireSoil;
        internal static ConfigEntry<bool> IncludeWildPlants;
        internal static ConfigEntry<string> NeedsSoilMessage;
        internal static ConfigEntry<bool> VerboseLogging;

        // Mod Menu reads these out of ConfigDescription.Tags to title its sections. Display
        // names only - the .cfg section keys are untouched, since renaming one there would
        // orphan every saved value.
        private const string MovingSection = "ModMenu.Section=Moving plants";
        private const string SafetySection = "ModMenu.Section=Safety";
        private const string DiagnosticsSection = "ModMenu.Section=Diagnostics";

        internal static void Bind(ConfigFile config, ManualLogSource log)
        {
            Log = log;

            // Descriptions are kept to one short line: Mod Menu renders them in a settings row
            // and overflows on anything longer. The reasoning lives in the README.

            Enabled = config.Bind(
                "Moving", "Enabled", true,
                new ConfigDescription(
                    "Let planted crops be picked up and moved in decorate mode.",
                    null,
                    MovingSection, "ModMenu.Label=Enable moving plants"));

            RequireModifier = config.Bind(
                "Moving", "RequireModifier", false,
                new ConfigDescription(
                    "Require the key below. Off makes plants always movable in decorate mode.",
                    null,
                    MovingSection, "ModMenu.Label=Hold a key to arm"));

            Modifier = config.Bind(
                "Moving", "Modifier", new KeyboardShortcut(KeyCode.X),
                new ConfigDescription(
                    "Press this in decorate mode to make plants selectable.",
                    null,
                    MovingSection, "ModMenu.Label=Arming key"));

            PressToToggle = config.Bind(
                "Moving", "PressToToggle", true,
                new ConfigDescription(
                    "Press the key to turn plant-moving on and off. Off means hold it instead.",
                    null,
                    MovingSection, "ModMenu.Label=Press to toggle"));

            IncludeWildPlants = config.Bind(
                "Moving", "IncludeWildPlants", false,
                new ConfigDescription(
                    "Also move wild plants - trees, bushes, weeds - not just crops you planted.",
                    null,
                    MovingSection, "ModMenu.Label=Include wild plants"));

            RequireSoil = config.Bind(
                "Safety", "RequireSoil", true,
                new ConfigDescription(
                    "Refuse to place a plant where it cannot be watered. Turning this off will freeze plants forever.",
                    null,
                    SafetySection, "ModMenu.Label=Only place where it can be watered"));

            NeedsSoilMessage = config.Bind(
                "Safety", "NeedsSoilMessage", "It needs watered ground.",
                new ConfigDescription(
                    "What your character says when a spot would leave the plant unable to grow.",
                    null,
                    SafetySection, "ModMenu.Label=Refusal message"));

            VerboseLogging = config.Bind(
                "Diagnostics", "VerboseLogging", false,
                new ConfigDescription(
                    "Log each pickup, placement and cancel.",
                    null,
                    DiagnosticsSection, "ModMenu.Label=Verbose logging"));
        }
    }
}
