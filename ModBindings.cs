global using static LegendaryWeapons.ModBindings;
global using SUtility = StardewValley.Utility;
global using SObject = StardewValley.Object;
global using CEUtility = CombatExtended.Framework.Util.Utility;

using StardewModdingAPI;

namespace LegendaryWeapons;

public static class ModBindings
{
    public static IModHelper Helper => ModEntry.Instance.Helper;
    
    public static IManifest Manifest => ModEntry.Instance.ModManifest;
    
    public static ModLog Log => ModEntry.Instance.Log;
    
    public static ModConfig Config => ModEntry.Instance.Config;
    
    public static string Id => ModEntry.Instance.ModManifest.UniqueID;
    
    public static string IdCP => $"{Id}CP";
}
