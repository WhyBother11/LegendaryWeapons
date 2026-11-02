using SpaceCore;
using StardewValley;

namespace LegendaryWeapons.Framework.Core.Weapons.Forge;

/// <summary>
/// Class to create and register custom forge recipes via Spacecore
/// </summary>
public static class LegendaryWeaponForgeRecipeFactory
{
    public static LegendaryWeaponForgeRecipe Create(
        string weaponQualifiedId,
        string ingredientQualifiedId,
        string resultQualifiedId,
        string flagOnResultReceived,
        int cinderShardCost = 20
    )
    {
        return new LegendaryWeaponForgeRecipe(
            weaponQualifiedId, ingredientQualifiedId, resultQualifiedId, flagOnResultReceived, cinderShardCost
        );
    }

    public static void RegisterRecipe(string resultId, LegendaryWeaponForgeRecipe recipe)
    {
        CustomForgeRecipe.Recipes.Add(recipe);
        Log.Trace($"Registered forge recipe for {resultId} via SpaceCore");
    }

    public sealed class LegendaryWeaponForgeRecipe : CustomForgeRecipe
    {
        public override IngredientMatcher BaseItem => new BaseWeaponIngredientMatcher(_weaponQualifiedId);

        public override IngredientMatcher IngredientItem => new BasicIngredientMatcher(_ingredientQualifiedId);

        public override int CinderShardCost => _cinderShardCost;

        private readonly string _weaponQualifiedId;
        private readonly string _ingredientQualifiedId;
        private readonly string _resultQualifiedId;
        private readonly string _flagOnResultReceived;
        private readonly int _cinderShardCost;

        public LegendaryWeaponForgeRecipe(
            string weaponQualifiedId,
            string ingredientQualifiedId,
            string resultQualifiedId,
            string flagOnResultReceived,
            int cinderShardCost
        )
        {
            _weaponQualifiedId = weaponQualifiedId;
            _ingredientQualifiedId = ingredientQualifiedId;
            _resultQualifiedId = resultQualifiedId;
            _flagOnResultReceived = flagOnResultReceived;
            _cinderShardCost = cinderShardCost;
        }

        public override Item CreateResult(Item baseItem, Item ingredItem)
        {
            var player = Game1.player;
            baseItem.modData.TryAdd($"{Id}_ForgingState", "0");
            var forgingState = baseItem.modData[$"{Id}_ForgingState"];
            
            if (player.AcquiredAnyLegendaryUltimateWeapon() || forgingState == "2")
            {
                player.mailReceived.Add(_flagOnResultReceived);
                return ItemRegistry.Create(_resultQualifiedId);
            }
            baseItem.modData[$"{Id}_ForgingState"] = (int.Parse(forgingState!) + 1).ToString();
            return baseItem;
        }
    }
    
    private class BaseWeaponIngredientMatcher : CustomForgeRecipe.IngredientMatcher
    {
        private readonly string _qualifiedItemId;

        public BaseWeaponIngredientMatcher(string qualifiedItemId)
        {
            _qualifiedItemId = qualifiedItemId;
        }

        public override bool HasEnoughFor(Item item)
        {
            return item.QualifiedItemId == _qualifiedItemId;
        }

        public override void Consume(ref Item? item)
        {
            item = null;
        }
    }
    
    private class BasicIngredientMatcher : CustomForgeRecipe.IngredientMatcher
    {
        private readonly string _qualifiedItemId;

        public BasicIngredientMatcher(string qualifiedItemId)
        {
            _qualifiedItemId = qualifiedItemId;
        }

        public override bool HasEnoughFor(Item item)
        {
            return item.QualifiedItemId == _qualifiedItemId;
        }

        public override void Consume(ref Item item)
        {
            item.Stack -= 1;
        }
    }
}