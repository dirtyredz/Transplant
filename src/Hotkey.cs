using BepInEx.Configuration;
using UnityEngine;

namespace Transplant
{
    /// <summary>
    /// Key checks for a hold-to-arm binding.
    ///
    /// Deliberately not KeyboardShortcut.IsPressed()/IsDown(). Those end in:
    ///
    ///     _modifierBlockKeyCodes.All(c =&gt; !Input.GetKey(c) || allKeys.Contains(c))
    ///
    /// where _modifierBlockKeyCodes is every supported key except the mouse buttons - so they
    /// report false whenever *any* other key is held. Wrong for this mod: the player is holding
    /// a movement key while walking the row of crops they want to rearrange, and the binding
    /// would simply never fire.
    ///
    /// Same reasoning and same code as Plant Peek's Hotkey.cs. Kept as a copy rather than a
    /// shared package because these mods ship as independent DLLs with no common dependency.
    /// </summary>
    internal static class Hotkey
    {
        internal static bool IsHeld(KeyboardShortcut shortcut)
        {
            var main = shortcut.MainKey;
            if (main == KeyCode.None || !UnityEngine.Input.GetKey(main))
            {
                return false;
            }

            return ModifiersHeld(shortcut);
        }

        internal static bool WasPressed(KeyboardShortcut shortcut)
        {
            var main = shortcut.MainKey;
            if (main == KeyCode.None || !UnityEngine.Input.GetKeyDown(main))
            {
                return false;
            }

            return ModifiersHeld(shortcut);
        }

        private static bool ModifiersHeld(KeyboardShortcut shortcut)
        {
            foreach (var modifier in shortcut.Modifiers)
            {
                if (!UnityEngine.Input.GetKey(modifier))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
