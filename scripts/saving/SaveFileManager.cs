using System;
using System.Collections.Generic;
using Godot;
using Incremental.scripts.director;
using Incremental.scripts.director.data;
using Incremental.scripts.director.data.recipe;
using Incremental.scripts.planet.data;

namespace Incremental.scripts.saving;

public static class SaveFileManager
{
    static readonly string path = "user://savegame.save";
    public static bool TryLoad = false;

    public static void Save()
    {
        GD.Print("Saving game...");
        using FileAccess save = FileAccess.Open(path, FileAccess.ModeFlags.Write);

        save.StoreLine("# Penguin Inc.");
        save.StoreLine("# Save format version: 0.1");
        save.StoreLine("#");

        save.StoreLine("# Inventory:");
        foreach (KeyValuePair<Item, ItemData> itemData in Inventory.Items)
        {
            save.StoreLine($"eii item:{(int)itemData.Key}={itemData.Value.Amount}={(itemData.Value.Obtained ? 1 : 0)}");
        }

        save.StoreLine("# Recipe unlocks:");
        foreach (KeyValuePair<RecipeID, ItemRecipe> itemRecipe in Inventory.Recipes)
        {
            save.StoreLine($"ei recipe_unlock:{(int)itemRecipe.Key}={(itemRecipe.Value.Unlocked ? 1 : 0)}");

            for (int i = 0; i < itemRecipe.Value.Ingredients.Count; i++)
            {
                Ingredient ingredient = itemRecipe.Value.Ingredients[i];
                save.StoreLine($"eid recipe_ingredient_cost:{(int)itemRecipe.Key}={i}={ingredient.Cost}");
            }
        }

        save.StoreLine("# Research unlocks:");
        foreach (KeyValuePair<RecipeID, bool> researchData in Inventory.Research)
        {
            save.StoreLine($"ei research_unlock:{(int)researchData.Key}={(researchData.Value ? 1 : 0)}");
        }
        
        save.StoreLine("# Mined tiles:");
        for (int y = 0; y < Game.I._data.Layers.Count; y++)
        {
            int strip = 0;
            string line = $"is layer_data:{y}";
            
            PlanetTile[] layer = Game.I._data.Layers[y];
            for (int x = 0; x < layer.Length; x++)
            {
                if (layer[x].IsEmpty())
                {
                    if (strip <= 0)
                        strip--;
                    else
                    {
                        line += $"={strip}";
                        strip = -1;
                    }
                }
                else
                {
                    if (strip >= 0)
                        strip++;
                    else
                    {
                        line += $"={strip}";
                        strip = 1;
                    }
                }
            }
            
            line += $"={strip}";
            save.StoreLine(line);
        }

        
        save.StoreLine("##### <3 #####");
        save.Flush();
        save.Close();
        GD.Print("Game saved!");
    }

    public static void Load()
    {
        if (!TryLoad)
        {
            GD.Print("Started new game, removing save...");
            Save();
            return;
        }

        GD.Print("Loading savegame...");

        using FileAccess save = FileAccess.Open(path, FileAccess.ModeFlags.Read);

        if (FileAccess.GetOpenError() != Error.Ok)
        {
            GD.Print("No savegame found!");
            return;
        }


        while (save.GetPosition() < save.GetLength())
        {
            string line = save.GetLine();

            if (line.StartsWith("#") || line.Length == 0)
                continue;

            string[] parts = line.Split(":", 2);
            string[] typeParts = parts[0].Split(" ", 2);

            string type = typeParts[0];
            string key = typeParts[1];

            string[] data = parts[1].Split("=");


            if (type == "ei")
            {
                int id = int.Parse(data[0]);
                int amount = int.Parse(data[1]);

                if (key == "recipe_unlock")
                    Inventory.Recipes[(RecipeID)id].Unlocked = amount == 1;
                else if (key == "research_unlock")
                    Inventory.Research[(RecipeID)id] = amount == 1;
            }
            else if (type == "eid")
            {
                int id = int.Parse(data[0]);
                int index = int.Parse(data[1]);
                double cost = double.Parse(data[2]);

                if (key == "recipe_ingredient_cost")
                    Inventory.Recipes[(RecipeID)id].Ingredients[index].Cost = cost;
            }
            else if (type == "eii")
            {
                int id = int.Parse(data[0]);
                int amount = int.Parse(data[1]);
                bool obtained = int.Parse(data[2]) == 1;
                
                if (key == "item")
                {
                    Inventory.Items[(Item)id].Amount = amount;
                    Inventory.Items[(Item)id].Obtained = obtained;
                }
            }
            else if (type == "is")
            {
                int y = int.Parse(data[0]);
                int x = 0;

                for (int i = 1; i < data.Length; i++)
                {
                    int strip = int.Parse(data[i]);

                    for (int j = 0; j < Math.Abs(strip); j++)
                    {
                        if (strip < 0)
                        {
                            Game.I._data.Layers[y][x + j].Integrity = 0;
                        }
                    }

                    x += Math.Abs(strip);
                }
            }
        }

        save.Close();
        GD.Print("Save loaded!");
    }
}