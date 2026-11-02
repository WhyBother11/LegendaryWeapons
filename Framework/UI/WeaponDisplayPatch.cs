using HarmonyLib;
using LegendaryWeapons.Framework.Core.Weapons.Mjolnir;
using StardewValley;
using StardewValley.Tools;

namespace LegendaryWeapons.Framework.UI;

[HarmonyPatch]
public static class WeaponDisplayPatch
{
    [HarmonyPatch(typeof(MeleeWeapon), nameof(MeleeWeapon.getCategoryName))]
    [HarmonyPrefix]
    private static bool MeleeWeapon_getCategoryName_Prefix(
        MeleeWeapon __instance,
        ref string __result)
    {
        if (__instance.QualifiedItemId == Mjolnir.MjolnirQualifiedId)
        {
            __result = GetCategoryName(__instance, GetLegendaryTemplate(__instance.type.Value));
            return false;
        }

        if (__instance.QualifiedItemId == MjolnirAwakened.MjolnirAwakenedQualifiedId)
        {
            __result = GetCategoryName(__instance, GetUltimateTemplate(__instance.type.Value));
            return false;
        }
        return true;
    }

    /// <summary>
    /// Get special category name for mod's weapons.
    /// </summary>
    /// <param name="weapon">Weapon instance.</param>
    /// <param name="templateKey">i18n key for special category name for given <paramref name="weapon"/>.</param>
    /// <returns></returns>
    private static string GetCategoryName(MeleeWeapon weapon, string templateKey)
    {
        GetLegendaryTemplate(weapon.type.Value);
        var path = weapon.type.Value switch
        {
            1 => "Strings\\StringsFromCSFiles:Tool.cs.14304",
            2 => "Strings\\StringsFromCSFiles:Tool.cs.14305",
            _ => "Strings\\StringsFromCSFiles:Tool.cs.14306"
        };
        var type = Game1.content.LoadString(path);
        var template = Helper.Translation.Get(templateKey);
        return string.Format(template, type);
    }
    
    /// <summary>
    /// Get i18n key for legendary weapon category name based on its type.
    /// </summary>
    /// <param name="weaponType">Weapon type, see <see cref="MeleeWeapon.type"/>.</param>
    /// <returns>i18n key for given <paramref name="weaponType"/>.</returns>
    private static string GetLegendaryTemplate(int weaponType)
    {
        return weaponType switch
        {
            1 => "weapon-display.legendary-weapon.category-name.dagger",
            2 => "weapon-display.legendary-weapon.category-name.club",
            _ => "weapon-display.legendary-weapon.category-name.sword"
        };
    }
    
    /// <summary>
    /// Get i18n key for ultimate weapon category name based on its type.
    /// </summary>
    /// <param name="weaponType">Weapon type, see <see cref="MeleeWeapon.type"/>.</param>
    /// <returns>i18n key for given <paramref name="weaponType"/>.</returns>
    private static string GetUltimateTemplate(int weaponType)
    {
        return weaponType switch
        {
            1 => "weapon-display.ultimate-weapon.category-name.dagger",
            2 => "weapon-display.ultimate-weapon.category-name.club",
            _ => "weapon-display.ultimate-weapon.category-name.sword"
        };
    }
}
