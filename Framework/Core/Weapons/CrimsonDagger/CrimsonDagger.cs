using LegendaryWeapons.Framework.Core.Weapons.Forge;
using LegendaryWeapons.Framework.Util;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Tools;
using xTile.Dimensions;
using xTile.Tiles;

namespace LegendaryWeapons.Framework.Core.Weapons.CrimsonDagger;

[Registrable]
public static class CrimsonDagger
{

    private static bool _acquired;
    private static bool _ultimateVerAcquired;
    
    private const uint TicksPerSpawnConditionCheck = 60;
    private const uint TicksPerGrimReaperAnimationTimerUpdate = 30;

    private const int BoneItemAmountForUltimateVer = 6;
    
    public static readonly string CrimsonDaggerId = $"{IdCP}_CrimsonDagger";
    public static readonly string CrimsonDaggerQualifiedId = $"(W){CrimsonDaggerId}";
    
    public static readonly string CrimsonDaggerUltimateId = $"{IdCP}_CrimsonDaggerUltimate";
    public static readonly string CrimsonDaggerUltimateQualifiedId = $"(W){CrimsonDaggerUltimateId}";

    private static readonly string CrimsonStoneQualifiedId = $"(O){IdCP}_CrimsonStone";

    private static bool _addedForgeRecipe;

    private static readonly string GrimReaperTilesheetId = $"z_{Id}_GrimReaper";
    private static readonly string GrimReaperImageSource = $@"Mods\{Id}\Tiles\GrimReaper";
    private static int _grimReaperAnimationTimer = 1.5f.ToTicks();

    public static void Register()
    {
        Helper.Events.Player.Warped += OnWarped_TryStartCheckingConditions;
        Helper.Events.Content.AssetRequested += OnAssetRequested_AddGrimReaperTilesheet;
        Helper.Events.GameLoop.DayStarted += OnDayStarted_AddCrimsonDaggerUltimateForgeRecipe;
        Helper.Events.GameLoop.ReturnedToTitle += (_, _) => { _acquired = false; };
    }
    
    public static bool IsCrimsonDagger(this MeleeWeapon weapon)
    {
        return weapon.Name.Contains(CrimsonDaggerId);
    }

    #region Spawn
    private static void OnWarped_TryStartCheckingConditions(object? sender, WarpedEventArgs e)
    {
        if (!e.IsLocalPlayer) return;
        if (e.NewLocation.Name.Contains("UndergroundMine77377"))
        {
            var player = e.Player;
            if (!_acquired && !player.mailReceived.Contains(WeaponFlags.CrimsonDaggerAcquiredMailFlag))
                Helper.Events.GameLoop.UpdateTicked += OnUpdateTicked_CheckCrimsonDaggerSpawnConditions;
            else if (!_ultimateVerAcquired &&
                     !player.mailReceived.Contains(WeaponFlags.CrimsonDaggerUltimateAcquiredMailFlag))
                Helper.Events.GameLoop.UpdateTicked += OnUpdateTicked_CheckCrimsonDaggerUltimateSpawnConditions;
        }
        else
        {
            _grimReaperAnimationTimer = -1;
            Helper.Events.GameLoop.UpdateTicked -= OnUpdateTicked_CheckCrimsonDaggerSpawnConditions;
            Helper.Events.GameLoop.UpdateTicked -= OnUpdateTicked_CheckCrimsonDaggerUltimateSpawnConditions;
        }

    }

    private static void OnUpdateTicked_CheckCrimsonDaggerSpawnConditions(object? sender, UpdateTickedEventArgs e)
    {
        if (!e.IsMultipleOf(TicksPerSpawnConditionCheck) || !Context.IsPlayerFree) return;
        var player = Game1.player;
        var loc = player.currentLocation;
        if (loc.Objects.TryGetValue(new Vector2(28, 6), out var obj1) &&
            loc.Objects.TryGetValue(new Vector2(31, 6), out var obj2) &&
            obj1.QualifiedItemId == GameQualifiedIds.SkullBrazier &&
            obj2.QualifiedItemId == GameQualifiedIds.SkullBrazier &&
            obj1.IsOn && obj2.IsOn &&
            player.ActiveItem?.QualifiedItemId == GameQualifiedIds.PrismaticShard &&
            player.Tile is { Y: <= 8, X: >= 28 and <= 31 })
        {
            Log.Trace("Crimson Dagger spawn conditions are satisfied.");
            _acquired = true;
            player.addItemByMenuIfNecessaryElseHoldUp(ItemRegistry.Create(CrimsonDaggerQualifiedId));
            PlayGrimReaperAnimation(player, loc);
            player.mailReceived.Add(WeaponFlags.CrimsonDaggerAcquiredMailFlag);
        }
    }

    private static void OnUpdateTicked_CheckCrimsonDaggerUltimateSpawnConditions(object? sender, UpdateTickedEventArgs e)
    {
        if (!e.IsMultipleOf(TicksPerSpawnConditionCheck) || !Context.IsPlayerFree) return;
        var player = Game1.player;
        var boneItems = player.Items.Where(item => item.HasContextTag("bone_item"));
        if (player.mailReceived.Contains(WeaponFlags.CrimsonDaggerAcquiredMailFlag) &&
            boneItems.Count() >= BoneItemAmountForUltimateVer &&
            player.ActiveItem?.QualifiedItemId == GameQualifiedIds.GalaxySoul &&
            player.Tile is { Y: <= 8, X: >= 28 and <= 31 })
        {
            Log.Trace("Ultimate Crimson Dagger spawn conditions are satisfied.");
            _ultimateVerAcquired = true;
            Game1.player.reduceActiveItemByOne();
            player.addItemByMenuIfNecessaryElseHoldUp(ItemRegistry.Create(CrimsonDaggerQualifiedId));
            PlayGrimReaperAnimation(player, player.currentLocation);
        }
    }
    #endregion
    
    #region GrimReaperAnim
    private static void OnAssetRequested_AddGrimReaperTilesheet(object? sender, AssetRequestedEventArgs e)
    {
        if (e.Name.IsEquivalentTo("Maps/Mines/77377"))
        {
            e.Edit(asset =>
                {
                    var map = asset.AsMap().Data;
                    map.AddTileSheet(new TileSheet(
                        id: GrimReaperTilesheetId,
                        map: map,
                        imageSource: GrimReaperImageSource,
                        sheetSize: new Size(2, 1),
                        tileSize: new Size(16)
                    ));
                }
            );
        }
    }

    private static void PlayGrimReaperAnimation(Farmer player, GameLocation quarryMine)
    {
        var tileSheetIndex = player.mailReceived.Contains("gotGoldenScythe") ? 1 : 0;
        quarryMine.setMapTile(30, 4, tileSheetIndex, "Front", GrimReaperTilesheetId);
        Helper.Events.GameLoop.UpdateTicked += OnUpdateTicked_CheckGrimReaperAnimationTimer;
    }

    private static void OnUpdateTicked_CheckGrimReaperAnimationTimer(object? sender, UpdateTickedEventArgs e)
    {
        if (!e.IsMultipleOf(TicksPerGrimReaperAnimationTimerUpdate)) return;
        if (_grimReaperAnimationTimer > 0)
        {
            _grimReaperAnimationTimer -= (int)TicksPerGrimReaperAnimationTimerUpdate;
            return;
        }
        
        var loc = Game1.player.currentLocation.Name.Contains("UndergroundMine77377")
            ? Game1.player.currentLocation
            : Game1.getLocationFromName("UndergroundMine77377");
        if (loc != null && loc.Map.GetLayer("Front").Tiles[30, 4].TileSheet.ImageSource == GrimReaperImageSource)
        {
            var tileSheetIndex = Game1.player.mailReceived.Contains("gotGoldenScythe") ? 246 : 244;
            loc.setMapTile(30, 4, tileSheetIndex, "Front", "mine");
            Helper.Events.GameLoop.UpdateTicked -= OnUpdateTicked_CheckGrimReaperAnimationTimer;
        }
    }
    #endregion
    
    #region Forge

    private static void OnDayStarted_AddCrimsonDaggerUltimateForgeRecipe(object? sender, DayStartedEventArgs e)
    {
        if (_addedForgeRecipe) return;
        var recipe = LegendaryWeaponForgeRecipeFactory.Create(
            weaponQualifiedId: CrimsonDaggerQualifiedId,
            ingredientQualifiedId: CrimsonStoneQualifiedId,
            resultQualifiedId: CrimsonDaggerUltimateQualifiedId,
            flagOnResultReceived: WeaponFlags.CrimsonDaggerUltimateAcquiredMailFlag
        );
        LegendaryWeaponForgeRecipeFactory.RegisterRecipe(CrimsonDaggerUltimateId, recipe);
        _addedForgeRecipe = true;
    }
    #endregion
}