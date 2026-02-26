using System;
using Godot;
using Incremental.scripts.director;
using Incremental.scripts.pawn.rendering;
using Incremental.scripts.planet;
using Incremental.scripts.planet.data;

public partial class PlanetRenderer : Node2D
{
    [Export] public int LayersPerChunk = 4;
    [Export] public Texture2D AtlasTexture;
    [Export] public int AtlasColumns = 4;

    private PlanetData _data;
    private FastNoiseLite _noise;

    private Random r = new Random(100);
    private int _vertex_count = 0;

    public override void _Ready()
    {
        Initialize(Game.I._data);
        
        AddChild(new Pawn());
    }


    public void Initialize(PlanetData data)
    {
        _data = data;

        _noise = new FastNoiseLite();
        _noise.NoiseType = FastNoiseLite.NoiseTypeEnum.Simplex;
        _noise.Frequency = 0.05f;

        BuildAllChunks();
    }

    private void BuildAllChunks()
    {
        foreach (Node child in GetChildren())
            child.QueueFree();

        _data._chunks.Clear();
        
        int chunkCount = Mathf.CeilToInt((float)_data.Layers.Count / LayersPerChunk);

        for (int i = 0; i < chunkCount; i++)
        {
            PlanetChunk chunk = BuildChunk(i);
            _data._chunks.Add(chunk);
            AddChild(chunk.Instance);
        }
    }

    private PlanetChunk BuildChunk(int chunkIndex)
    {
        int layerStart = chunkIndex * LayersPerChunk;
        int layerEnd = Mathf.Min(layerStart + LayersPerChunk, _data.Layers.Count);

        PlanetChunk chunk = new PlanetChunk
        {
            LayerStart = layerStart,
            LayerCount = layerEnd - layerStart
        };

        var st = new SurfaceTool();
        st.Begin(Mesh.PrimitiveType.Triangles);

        for (int layer = layerStart; layer < layerEnd; layer++)
        {
            BuildLayer(st, chunk, layer);
        }

        chunk.Mesh = st.Commit();
        chunk.Instance = new MeshInstance2D
        {
            Mesh = chunk.Mesh,
            Texture = AtlasTexture
        };

        return chunk;
    }
    
    private void BuildLayer(SurfaceTool st, PlanetChunk chunk, int layer)
    {
        PlanetTile[] tiles = _data.Layers[layer];
        int tileCount = tiles.Length;

        float innerR = (layer == 0) ? 0f : layer * _data.TileSize - (_data.TileSize - _data._innerGrowth);
        float outerR = (layer == 0) ? _data._innerGrowth : (layer + 1) * _data.TileSize - (_data.TileSize - _data._innerGrowth);

        // Only needed if current layer has more tiles than previous
        int prevTileCount = (layer == 0) ? _data.Layers[0].Length : _data.Layers[layer - 1].Length;
        bool isSplitLayer = tileCount > prevTileCount;

        float angleStepPrev = Mathf.Tau / prevTileCount;

        // Precompute parent top vertices if splitting
        Vector2[] parentTopLeft = null;
        Vector2[] parentTopRight = null;

        if (isSplitLayer)
        {
            parentTopLeft = new Vector2[prevTileCount];
            parentTopRight = new Vector2[prevTileCount];

            for (int p = 0; p < prevTileCount; p++)
            {
                float a0 = p * angleStepPrev;
                float a1 = (p + 1) * angleStepPrev;

                Vector2 tl = Polar(outerR - _data.TileSize, a0); // top-left
                Vector2 tr = Polar(outerR - _data.TileSize, a1); // top-right

                ApplyNoise(ref tl);
                ApplyNoise(ref tr);

                parentTopLeft[p] = tl;
                parentTopRight[p] = tr;
            }
        }


        float angleStepCurrent = Mathf.Tau / tileCount;

        for (int i = 0; i < tileCount; i++)
        {
            PlanetTile tile = tiles[i];
            if (tile.Destroyed)
                continue;

            float a0 = i * angleStepCurrent;
            float a1 = (i + 1) * angleStepCurrent;

            Vector2 snapBottomRight = Vector2.Zero;
            Vector2 snapBottomLeft = Vector2.Zero;

            if (isSplitLayer)
            {
                int parentIndex = i * prevTileCount / tileCount;

                Vector2 parentMid = (parentTopLeft[parentIndex] + parentTopRight[parentIndex]) / 2f;

                int tilesPerParent = tileCount / prevTileCount;
                int indexInParent = i % tilesPerParent;

                if (indexInParent == 0)
                    snapBottomLeft = Vector2.Zero; // left child
                else
                    snapBottomLeft = parentMid; // right child

                if (indexInParent == 0)
                    snapBottomRight = parentMid; // left child
                else
                    snapBottomRight = Vector2.Zero; // right child
            }

            int vertexStart = _vertex_count;
            chunk.Tiles[(layer, i)] = new TileInfo
            {
                VertexStart = vertexStart,
                Layer = layer,
                Tile = i
            };

            AddTile(st, tile, innerR, outerR, a0, a1, snapBottomLeft, snapBottomRight);
        }
    }

    private void AddTile(
        SurfaceTool st,
        PlanetTile tile,
        float innerR,
        float outerR,
        float a0,
        float a1,
        Vector2 snapBottomLeft,
        Vector2 snapBottomRight)
    {
        Vector2 p00 = (snapBottomLeft != Vector2.Zero) ? snapBottomLeft : Polar(innerR, a0);
        Vector2 p01 = (snapBottomRight != Vector2.Zero) ? snapBottomRight : Polar(innerR, a1);

        Vector2 p10 = Polar(outerR, a0);
        Vector2 p11 = Polar(outerR, a1);

        if ((snapBottomLeft == Vector2.Zero))
            ApplyNoise(ref p00);
        if ((snapBottomRight == Vector2.Zero))
            ApplyNoise(ref p01);
        ApplyNoise(ref p10);
        ApplyNoise(ref p11);

        Vector2 uv0, uv1, uv2, uv3;
        GetAtlasUV(tile.Material, out uv0, out uv1, out uv2, out uv3);

        // Triangle 1
        st.SetUV(uv0);
        st.AddVertex(new Vector3(p00.X, p00.Y, 0));
        st.SetUV(uv2);
        st.AddVertex(new Vector3(p10.X, p10.Y, 0));
        st.SetUV(uv3);
        st.AddVertex(new Vector3(p11.X, p11.Y, 0));

        // Triangle 2
        st.SetUV(uv0);
        st.AddVertex(new Vector3(p00.X, p00.Y, 0));
        st.SetUV(uv3);
        st.AddVertex(new Vector3(p11.X, p11.Y, 0));
        st.SetUV(uv1);
        st.AddVertex(new Vector3(p01.X, p01.Y, 0));
    }


    private Vector2 Polar(float baseRadius, float angle)
    {
        Vector2 dir = new(Mathf.Cos(angle), Mathf.Sin(angle));

        return dir * baseRadius;
    }


    private void ApplyNoise(ref Vector2 p)
    {
        float n = _noise.GetNoise2D(p.X, p.Y);
        p += p.Normalized() * n * 2.0f;
    }


    private void GetAtlasUV(
        TileMaterial material,
        out Vector2 uv0,
        out Vector2 uv1,
        out Vector2 uv2,
        out Vector2 uv3)
    {
        int index = (int)material;
        float size = 1f / AtlasColumns;

        int x = index % AtlasColumns;
        int y = index / AtlasColumns;

        Vector2 baseUV = new Vector2(x * size, y * size);

        uv0 = baseUV;
        uv1 = baseUV + new Vector2(size, 0);
        uv2 = baseUV + new Vector2(0, size);
        uv3 = baseUV + new Vector2(size, size);
    }
    
    
    public override void _Input(InputEvent e)
    {
        if (e is InputEventMouseButton mb &&
            mb.Pressed &&
            mb.ButtonIndex == MouseButton.Left)
        {
            Vector2 local = ToLocal(GetGlobalMousePosition());

            if (_data.LocalPositionToPolarCoords(local, out int layer, out int tile))
            {
                ModifyTile(layer, tile);
            }
        }
    }
    
    private void ModifyTile(int layer, int tile)
    {
        PlanetTile t = _data.Layers[layer][tile];
        t.Material = TileMaterial.Rock; // example
        t.Integrity = 0.5f;
        _data.Layers[layer][tile] = t;

        int chunkIndex = layer / LayersPerChunk;
        RebuildChunk(chunkIndex);
    }

    private void RebuildChunk(int chunkIndex)
    {
        PlanetChunk oldChunk = _data._chunks[chunkIndex];
        oldChunk.Instance.QueueFree();

        PlanetChunk newChunk = BuildChunk(chunkIndex);
        _data._chunks[chunkIndex] = newChunk;

        AddChild(newChunk.Instance);
        MoveChild(newChunk.Instance, chunkIndex);
    }




    public override void _Process(double delta)
    {
        base._Process(delta);
        
        // Rotate((float)delta * 0.002f);

        // if (true)
        // {
        //     _innerGrowth += GrowthSpeed * (float)delta; // delta = frame time
        //
        //     // If inner layer fully grown
        //     if (_innerGrowth >= TileSize)
        //     {
        //         _innerGrowth = TileSize;
        //
        //         // Insert a new growing layer at index 0
        //         PlanetTile[] newLayer = _data.GenerateLayerTiles(_data.BaseTileCount);
        //         _data.Layers.Insert(0, newLayer);
        //
        //         // Reset inner growth
        //         _innerGrowth = 0f;
        //     }
        //
        //     // Rebuild the mesh
        //     BuildAllChunks();
        // }
    }
}