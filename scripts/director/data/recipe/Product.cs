namespace Incremental.scripts.director.data.recipe;

public class Product
{
    public readonly Item Item;
    public readonly int Amount;
    public readonly double Chance;

    public Product(Item item, int amount, double chance = 1)
    {
        Amount = amount;
        Item = item;
        Chance = chance;
    }
}