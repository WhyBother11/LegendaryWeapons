using HarmonyLib;
using LegendaryWeapons.Framework.Core.Weapons.Forge;
using LegendaryWeapons.Framework.Util;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace LegendaryWeapons.Framework.Core.Weapons.Mjolnir;

/// <summary>
/// Contains code exclusive to awakened version of Mjölnir.
/// </summary>
[Registrable]
[HarmonyPatch]
public static class MjolnirAwakened
{
    
    public static readonly string MjolnirAwakenedId = $"{IdCP}_MjolnirAwakened";
    public static readonly string MjolnirAwakenedQualifiedId = $"(W){MjolnirAwakenedId}";
    private static readonly string ThunderSoulQualifiedId = $"(O){IdCP}_ThunderSoul";

    private const uint TicksPerSpawnConditionCheck = 60;

    private const double SpawnChance = 0.025;
    
    private static bool _addedForgeRecipe;

    public static void Register()
    {
        Helper.Events.Player.Warped += OnWarped_HandleMjolnirAwakenedSpawnConditionChecker;
        Helper.Events.GameLoop.DayStarted += OnDayStarted_AddMjolnirAwakenedForgeRecipe;
    }

    #region Spawn
    private static void OnWarped_HandleMjolnirAwakenedSpawnConditionChecker(
        object? sender, WarpedEventArgs e)
    {
        if (!e.IsLocalPlayer) return;
        if (e.NewLocation.IsLightningHere() && e.Player.mailReceived.Contains(WeaponFlags.MjolnirAcquiredMailFlag))
            Helper.Events.GameLoop.UpdateTicked += OnUpdateTicked_CheckForMjolnirAwakenedSpawnConditions;
        else 
            Helper.Events.GameLoop.UpdateTicked -= OnUpdateTicked_CheckForMjolnirAwakenedSpawnConditions;
    }

    private static void OnUpdateTicked_CheckForMjolnirAwakenedSpawnConditions(
        object? sender, UpdateTickedEventArgs e)
    {
        if (!e.IsMultipleOf(TicksPerSpawnConditionCheck) || !Context.IsPlayerFree) return;
        var player = Game1.player;
        var hasLightningRod = player.Items.ContainsId(GameQualifiedIds.LightningRod);
        var holdingGalaxySoul = player.ActiveItem?.QualifiedItemId == GameQualifiedIds.GalaxySoul;
        if (hasLightningRod && holdingGalaxySoul && Game1.random.NextDouble() < SpawnChance)
        {
            Game1.flashAlpha = (float)(0.5 + Game1.random.NextDouble());
            var strikePosition = new Vector2(player.Position.X, player.Position.Y - Game1.tileSize);
            SUtility.drawLightningBolt(strikePosition, player.currentLocation);
            Game1.playSound("thunder");
            var thunderSoul = ItemRegistry.Create(ThunderSoulQualifiedId);
            player.holdUpItemThenMessage(thunderSoul);
            player.reduceActiveItemByOne();
            if (!player.addItemToInventoryBool(thunderSoul))
                Game1.createItemDebris(thunderSoul, player.getStandingPosition(), 1);
        }
    }

    [HarmonyPatch(typeof(Item), nameof(Item.checkForSpecialItemHoldUpMeessage))]
    [HarmonyPrefix]
    // TODO: check if it works now
    private static bool Item_checkForSpecialItemHoldUpMeessage_ThunderSoulMessagePrefix(
        Item __instance, ref string __result)
    {
        if (__instance.QualifiedItemId == ThunderSoulQualifiedId)
        {
            __result = Helper.Translation.Get("message.thunder-soul.acquired");
            return false;
        }

        return true;
    }
    #endregion

    #region Forge
    private static void OnDayStarted_AddMjolnirAwakenedForgeRecipe(
        object? sender, DayStartedEventArgs e)
    {
        if (_addedForgeRecipe) return;
        var recipe = LegendaryWeaponForgeRecipeFactory.Create(
            weaponQualifiedId: Mjolnir.MjolnirQualifiedId,
            ingredientQualifiedId: ThunderSoulQualifiedId,
            resultQualifiedId: MjolnirAwakenedQualifiedId,
            flagOnResultReceived: WeaponFlags.AwakenedMjolnirAcquiredMailFlag
        );
        LegendaryWeaponForgeRecipeFactory.RegisterRecipe(MjolnirAwakenedId, recipe);
        _addedForgeRecipe = true;
    }
    #endregion
}
