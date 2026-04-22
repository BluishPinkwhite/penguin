using System;

namespace Incremental.scripts.planet.data;

public static class PlanetGenerator
{
    private static Random r = new();
    private static readonly float[] percentages = [1f, 0.99f, 0.86f, 0.54f, 0.24f, 0.12f, 0.03f, 0];

    public static float LayerSpan(int layerCount) => 3f / layerCount;

    public static PlanetTile[] GenerateLayerTiles(int tileCount, int layer, int layerCount)
    {
        PlanetTile[] tiles = new PlanetTile[tileCount];
        float layerPercent = (float)layer / layerCount;
        float layerSpan = LayerSpan(layerCount);

        for (int i = 0; i < tileCount; i++)
        {
            tiles[i] = new PlanetTile
            {
                Material = LayerMaterial(layerPercent, layerSpan),
                Integrity = 1f,
                Light = layer < layerCount - PlanetTile.LightReach ? 0 : 1 - (layerCount - layer) / (float)PlanetTile.LightReach
            };
        }

        return tiles;
    }

    private static TileMaterial RandomMaterialIndex()
    {
        return (TileMaterial)(r.Next(0, 8));
    }

    public static TileMaterial LayerMaterial(float layerPercent, float layerSpan)
    {
        float value = r.NextSingle();

        for (int i = 0; i < percentages.Length; i++)
        {
            if (layerPercent >= percentages[i])
            {
                // layer edge - random between the two layer materials
                if (i > 1 && i < percentages.Length - 1 && 
                    layerPercent <= percentages[i] + layerSpan && value > ((layerPercent - percentages[i]) / layerSpan))
                {
                    return (TileMaterial)(i);
                }

                // return material of this layer
                return (TileMaterial)(i - 1);
            }
        }

        return TileMaterial.Unknown;
    }
}