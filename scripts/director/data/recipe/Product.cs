namespace Incremental.scripts.director.data.recipe;

public class Product
{
    public Item Item;
    public int Amount;
    public double Chance;

    public Product(Item item, int amount, double chance = 1)
    {
        Amount = amount;
        Item = item;
        Chance = chance;
    }
}