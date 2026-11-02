using LegendaryWeapons;

namespace LegendaryWeapons.Integrations.GMCM;

public static class EnableGmcmIntegration
{
    public static void AddGmcmOptions(
        this ModEntry modEntry,
        IGenericModConfigMenu configMenu)
    {
        // todo: add mjolnir reqquired mastery
        /*configMenu.Add(
            mod: modEntry.ModManifest,
            name: () => modEntry.Helper.Translation.Get("config.option.no-immunities"),
            tooltip: () => modEntry.Helper.Translation.Get("config.option.no-immunities.tooltip"),
            getValue: () => modEntry.Config.NoImmunities,
            setValue: value => modEntry.Config.NoImmunities = value
        );*/
    }
}