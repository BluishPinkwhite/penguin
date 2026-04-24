using System.Collections.Generic;
using Incremental.ui;

namespace Incremental.scripts.director.data.recipe;

public class ResearchRecipe : ItemRecipe
{
    public ResearchRecipe(RecipeID id, List<Ingredient> ingredients, bool unlocked = false) 
        : base(id, ingredients, null, unlocked)
    {
    }
    
    protected override void Apply()
    {
        Inventory.Research[ID] = true;

        Resources.I.UpdateVisuals();
        Game.I.Pawns.UpdatePawnCounts();
    }
}
