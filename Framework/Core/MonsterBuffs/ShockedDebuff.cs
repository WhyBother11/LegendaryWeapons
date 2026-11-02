using CombatExtended.Framework.Core.Monsters;
using CombatExtended.Framework.Core.Monsters.Buffs;
using CombatExtended.Framework.Data;
using CombatExtended.Framework.UI.Animations;
using LegendaryWeapons.Framework.Util;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Monsters;
using Utility = LegendaryWeapons.Framework.Util.Utility;

namespace LegendaryWeapons.Framework.Core.MonsterBuffs;

[Registrable]
public sealed class ShockedDebuff : MonsterBuff
{
    
    private static readonly string StunAnimationPath = $"Mods/{IdCP}/Animations/Stun";

    public static readonly string UniqueId = $"{IdCP}_Shocked";

    private TemporaryAnimatedSprite? _stunAnimation;

    private ShockedDebuff(string id, MonsterBuffData data) : base(id, data) { }

    protected override void OnApplied(Monster monster, GameLocation location)
    {
        base.OnApplied(monster, location);
        
        const float interval = 100f;
        const int animLength = 3;
        var loops = Data!.Duration.ToMilliseconds() / (interval * animLength) + 1;
        var animation = new TextureTemporaryAnimatedSprite(
            texturePath: StunAnimationPath,
            sourceRect: new Rectangle(0, 0, 64, 64),
            animationInterval: interval,
            animationLength: 3,
            numberOfLoops: (int)loops,
            position: monster.GetApproximatePositionOnTop(),
            color: Color.White,
            layerDepth: 1f
        );
        _stunAnimation = animation;
        location.temporarySprites.Add(animation);
    }

    public override void StopAnimating(Monster monster, GameLocation location)
    {
        if (_stunAnimation != null) 
            location.temporarySprites.Remove(_stunAnimation);
    }

    public static void Register()
    {
        RegisterCustom(UniqueId, data => new ShockedDebuff(UniqueId, data));
    }
}
