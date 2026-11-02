using CombatExtended.Framework.Core.Abilities;
using CombatExtended.Framework.Data;
using CombatExtended.Framework.UI.Animations;
using LegendaryWeapons.Framework.Util;
using Microsoft.Xna.Framework;
using StardewValley;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace LegendaryWeapons.Framework.Core.Abilities;

[Registrable]
public sealed class SummonRain : Ability
{
    public static readonly string SummonRainAnimationPath = $"Mods/{ModBindings.Id}/Animations/SummonRain";
    
    public static readonly string UniqueId = $"{IdCP}_MjolnirSummonRain";

    private SummonRain(
        string id,
        AbilityData data) : base(id, data) { }

    protected override void OnTrigger(Farmer player, GameLocation location, Vector2 cursorTile, Vector2 playerTile)
    {
        const float interval = 50f;
        var animation = new TextureTemporaryAnimatedSprite(
            texturePath: SummonRainAnimationPath,
            sourceRect: new Rectangle(0, 0, 64, 64),
            animationInterval: interval,
            animationLength: 6,
            numberOfLoops: 3,
            position: new Vector2((cursorTile.X - 1f) * Game1.tileSize, (cursorTile.Y - 2.5f) * Game1.tileSize),
            color: Color.White,
            layerDepth: 1f,
            scale: 3f
        );
        location.temporarySprites.Add(animation);
    }

    public static void Register()
    {
        RegisterCustom(UniqueId, data => new SummonRain(UniqueId, data));
    }
}