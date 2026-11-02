using StardewValley;

namespace LegendaryWeapons.Framework.Core.Weapons;

public static class WeaponFlags
{
    public static readonly string MjolnirAcquiredMailFlag = $"{Id}_mjolnirAcquired";
    public static readonly string AwakenedMjolnirAcquiredMailFlag = $"{Id}_awakenedMjolnirAcquired";

    public static readonly string CrimsonDaggerAcquiredMailFlag = $"{Id}_crimsonDaggerAcquired";
    public static readonly string CrimsonDaggerUltimateAcquiredMailFlag = $"{Id}_crimsonDaggerUltimateAcquired";

    public static bool AcquiredAnyLegendaryUltimateWeapon(this Farmer farmer)
    {
        var acquiredInfinityWeapon = Game1.player.achievements.Contains(42);
        return acquiredInfinityWeapon || farmer.mailReceived.Contains(AwakenedMjolnirAcquiredMailFlag)
            || farmer.mailReceived.Contains(CrimsonDaggerUltimateAcquiredMailFlag);
    }
}
