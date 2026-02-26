using System.Collections.Generic;
using Godot;

namespace Incremental.scripts.planet;

public class PlanetChunk
{
    public int LayerStart;
    public int LayerCount;

    public ArrayMesh Mesh;
    public MeshInstance2D Instance;

    public Dictionary<(int layer, int tile), TileInfo> Tiles = new();
}
