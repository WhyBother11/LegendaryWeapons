using System.Reflection.Emit;
using CombatExtended.Framework.Util;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Monsters;
using StardewValley.Tools;

namespace LegendaryWeapons.Framework.Core.Misc.Tools;

[HarmonyPatch]
public static class ApplyingWetWateringCanPatch
{
    
    /// <summary>
    /// Enables <see cref="WateringCan"/> to apply wet debuff when it is being used.
    /// </summary>
    /// <param name="instructions">IL instructions.</param>
    /// <returns>Modified IL instructions.</returns>
    [HarmonyPatch(typeof(WateringCan), nameof(WateringCan.DoFunction))]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction>? WateringCan_DoFunction_ApplyWetTranspiler(
        IEnumerable<CodeInstruction> instructions)
    {
        try
        {
            var matcher = new CodeMatcher(instructions);
            matcher.MatchEndForward(
                new CodeMatch(OpCodes.Callvirt,
                    AccessTools.Method(typeof(GameLocation), nameof(GameLocation.performToolAction))),
                new CodeMatch(OpCodes.Pop));
            // insert ApplyingWetWateringCanPatch.ApplyWetDebuff(GameLocation location, int tileX, int tileY)
            matcher.Insert(
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Ldloc_S, (byte) 5),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Vector2), nameof(Vector2.X))),
                new CodeInstruction(OpCodes.Conv_I4),
                new CodeInstruction(OpCodes.Ldloc_S, (byte) 5),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Vector2), nameof(Vector2.Y))),
                new CodeInstruction(OpCodes.Conv_I4),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ApplyingWetWateringCanPatch), nameof(ApplyWetDebuff))));
            return matcher.InstructionEnumeration();
        } catch(Exception e)
        {
            Log.Error("Failed injecting patch for applying wet debuff when using watering can.", e);
            return instructions;
        }
    }

    /// <summary>
    /// Apply wet debuff to any monster on <paramref name="location"/> on <paramref name="tileX"/>, <paramref name="tileY"/>.>
    /// </summary>
    /// <param name="location">Current player location.</param>
    /// <param name="tileX">Tile X coordinates.</param>
    /// <param name="tileY">Tile Y coordinates.</param>
    private static void ApplyWetDebuff(GameLocation location, int tileX, int tileY)
    {
        foreach (var monster in location.characters.OfType<Monster>())
        {
            if ((int)monster.Tile.X == tileX && (int)monster.Tile.Y == tileY)
            {
                CombatExtended.ModBindings.MonsterBuffManager.ApplyBuff(
                    monster, location, $"{IdCP}_Wet",
                    Game1.player.GetCharacterType(), Game1.player.GetCharacterId()
                );
            }
        }
    }
}
