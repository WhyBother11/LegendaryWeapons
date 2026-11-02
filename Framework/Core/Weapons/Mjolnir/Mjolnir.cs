using HarmonyLib;
using LegendaryWeapons.Framework.Util;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.Menus;
using StardewValley.Tools;

namespace LegendaryWeapons.Framework.Core.Weapons.Mjolnir;

/// <summary>
/// Implements logic for base Mjölnir weapon.
/// </summary>
[Registrable]
[HarmonyPatch]
public static class Mjolnir
{

    #nullable disable

    private static bool _spawned;
    
    private static bool _acquired;
    #nullable restore
    
    private static readonly List<string> SpawnEligibleLocations = new() { "IslandSouth", "IslandSouthEast", "IslandEast", "IslandWest", "IslandNorth" };
    
    public static readonly string MjolnirId = $"{IdCP}_Mjolnir";
    public static readonly string MjolnirQualifiedId = $"(W){MjolnirId}";
    private static readonly string MjolnirObjectId = $"{IdCP}_MjolnirObject";

    public static void Register()
    {
        Helper.Events.Player.Warped += OnWarped_TrySpawningMjolnir;
        Helper.Events.GameLoop.DayEnding += OnDayEnding_RemoveSpawnedMjolnir;
        Helper.Events.Multiplayer.ModMessageReceived += OnMessageReceived_CheckMjolnirMessages;
    }

    #region Spawn
    private static void OnWarped_TrySpawningMjolnir(object? sender, WarpedEventArgs e)
    {
        if (_spawned || !e.IsLocalPlayer || !SpawnEligibleLocations.Contains(e.NewLocation.Name) ||
            !e.Player.mailReceived.Contains("galaxySword") || !e.NewLocation.GetWeather().IsRaining ||
            e.Player.mailReceived.Contains(WeaponFlags.MjolnirAcquiredMailFlag)) return;
        //if (Spawned || !SpawnEligibleLocations.Contains(e.NewLocation.Name)) return;
        Log.Trace("Mjolnir spawn conditions are satisfied.");
        var loc = Game1.getLocationFromName(Game1.random.ChooseFrom(SpawnEligibleLocations));
        const int maxTries = 25;
        Vector2? spawnPos = null;
        // try to find a tile to spawn
        for (int i = 0; i < maxTries; i++)
        {
            if (_spawned) break;
            var randomTile = loc.getRandomTile();
            if (loc.CanItemBePlacedHere(randomTile) &&
                !loc.IsNoSpawnTile(randomTile) &&
                !loc.isObjectAtTile((int)randomTile.X, (int)randomTile.Y) &&
                !loc.isWaterTile((int)randomTile.X, (int)randomTile.Y))
            {
                spawnPos = randomTile;
            }
        }
        if (spawnPos.HasValue)
        {
            var mjolnir = ItemRegistry.Create<SObject>("(O)" + MjolnirObjectId);
            if (!_spawned && loc.dropObject(mjolnir, spawnPos.Value * Game1.tileSize, Game1.viewport, true))
            {
                Helper.Multiplayer.SendMessage("MjolnirIsSpawned", $"{Id}_MjolnirSpawned", modIDs: new []{ Id });
                Log.Trace($"Spawned mjolnir on location {loc.Name} on tile X:{spawnPos.Value.X} Y:{spawnPos.Value.Y}");
                _spawned = true;
            }
            else
            {
                Log.Trace($"Failed to spawn mjolnir on location {loc.Name} on tile X:{spawnPos.Value.X} Y:{spawnPos.Value.Y}");
            }
        }
        else
        {
            Log.Trace($"Failed to spawn mjolnir on location {loc.Name} after trying {maxTries} times");
        }
    }
    
    /// <summary>
    /// If Mjölnir was spawn by another player, set spawned flag.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private static void OnMessageReceived_CheckMjolnirMessages(object? sender, ModMessageReceivedEventArgs e)
    {
        if (e.FromModID == Id)
        {
            if (e.Type == $"{Id}_MjolnirSpawned")
            {
                Log.Trace($"Received {e.Type} message");
                _spawned = true;
            }
            if (e.Type == $"{Id}_MjolnirAcquired")
            {
                Log.Trace($"Received {e.Type} message");
                _acquired = true;
            }
        }
    }

    /// <summary>
    /// Remove spawned Mjölnir objects, if any.
    /// </summary>
    /// <param name="sender">Event sender.</param>
    /// <param name="e">See <see cref="DayEndingEventArgs"/>.</param>
    private static void OnDayEnding_RemoveSpawnedMjolnir(object? sender, DayEndingEventArgs e)
    {
        if (!Context.IsMainPlayer) return;
        var locations = SpawnEligibleLocations.Select(Game1.getLocationFromName);
        foreach (var gameLocation in locations)
        {
            var positions = gameLocation.Objects.Values
                .Where(obj => obj is Item item && item.ItemId == MjolnirObjectId)
                .Select(obj => obj.getLocalPosition(Game1.viewport));
            foreach (var position in positions)
            {
                Log.Trace($"Removed spawned mjolnir on location {gameLocation.Name} at position X:{position.X} Y:{position.Y}");
                gameLocation.Objects.Remove(position);
            }
        }
        _spawned = false;
    }

    /// <summary>
    /// Replace picked up Mjölnir object with actual weapon.
    /// </summary>
    /// <param name="__instance">Player that picked up item.</param>
    /// <param name="item">Item to be picked up, see <see cref="Farmer.addItemToInventory(Item)"/></param>
    [HarmonyPatch(typeof(Farmer), nameof(Farmer.addItemToInventory), typeof(Item))]
    [HarmonyPrefix]
    private static void Farmer_addItemToInventory_MjolnirObjectPrefix(Farmer __instance, ref Item item)
    {
        if (item.ItemId == MjolnirObjectId)
        {
            var mjolnir = ItemRegistry.Create<MeleeWeapon>(MjolnirQualifiedId);
            Log.Trace($"Replacing {item.ItemId} with {mjolnir.ItemId}");
            item = mjolnir;
            var message = Helper.Translation.Get("message.mjolnir.lift-worthy");
            __instance.mailReceived.Add(WeaponFlags.MjolnirAcquiredMailFlag);
            Helper.Multiplayer.SendMessage("MjolnirIsAcquired", $"{Id}_MjolnirAcquired",
                modIDs: new []{ Id });
            Game1.activeClickableMenu = new DialogueBox(message);
        }
    }

    /// <summary>
    /// Check if player should pick up spawned Mjölnir object.
    /// </summary>
    /// <param name="item">Item to be picked up.</param>
    /// <param name="__result">If player inventory should accept picked up item.</param>
    /// <returns></returns>
    [HarmonyPatch(typeof(Farmer), nameof(Farmer.couldInventoryAcceptThisItem), typeof(Item))]
    [HarmonyPrefix]
    private static bool Farmer_couldInventoryAcceptThisItem_MjolnirObjectPrefix(
        Item item, ref bool __result)
    {
        if (item.ItemId == MjolnirObjectId)
        {
            var mastery = MasteryTrackerMenu.getCurrentMasteryLevel();
            if (mastery < Config.MasteryForMjolnirWorthiness)
            {
                __result = false;
                var message = Helper.Translation.Get("message.mjolnir.lift-unworthy");
                Game1.activeClickableMenu = new DialogueBox(message);
                return false;
            }
        }
        return true;
    }
    #endregion
}
