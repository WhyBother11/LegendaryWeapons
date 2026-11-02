using System.Reflection;
using HarmonyLib;
using LegendaryWeapons.Framework.Util;
using LegendaryWeapons.Integrations.GMCM;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace LegendaryWeapons;

public class ModEntry : Mod
{
    #nullable disable
    public static ModEntry Instance { get; private set; }
    
    public ModConfig Config { get; private set; }
    
    public ModLog Log { get; private set; }
    #nullable restore

    public override void Entry(IModHelper helper)
    {
        Instance = this;
        Config = helper.ReadConfig<ModConfig>();
        Log = new ModLog(Monitor);
        Harmony harmony = new(ModManifest.UniqueID);
        harmony.PatchAll();
        
        var registrableClasses = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => t.IsDefined(typeof(Registrable)));
        Register(registrableClasses);
        
        helper.Events.GameLoop.GameLaunched += OnGameLaunched_AddGmcmIntegration;
    }

    private static void Register(IEnumerable<Type> types)
    {
        foreach (var type in types)
        {
            var registerMethod = type.GetMethods(BindingFlags.Public | BindingFlags.Static)
                .First(m => m.Name == "Register");
            registerMethod.Invoke(null, Array.Empty<object>());
        }
    }
    
    private void OnGameLaunched_AddGmcmIntegration(object? sender, GameLaunchedEventArgs e)
    {
        var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenu>("spacechase0.GenericModConfigMenu");
        if (configMenu is null)
            return;

        configMenu.Register(
            mod: ModManifest,
            reset: () => Config = new ModConfig(),
            save: () => Helper.WriteConfig(Config)
        );

        this.AddGmcmOptions(
            configMenu: configMenu
        );
    }
}
