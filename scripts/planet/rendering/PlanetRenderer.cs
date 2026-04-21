using System;
using Godot;
using Incremental.scripts.director;
using Incremental.scripts.director.data;
using Incremental.scripts.director.data.recipe;
using Incremental.scripts.entity.item;
using Incremental.scripts.entity.pawn;
using Incremental.scripts.planet.data;

namespace Incremental.scripts.planet.rendering;

public partial class PlanetRenderer : Node2D
{
    [Export] public Texture2D AtlasTexture;
    [Export] public int AtlasColumns = 4;
    [Export] public Shader PlanetShader;

    public static bool isDirty = false;
    public static bool isLightDirty = false;

    private PlanetData _data;
    private Image _dataImage;
    private ImageTexture _dataTexture;
    private ShaderMaterial _mat;

    public override void _Ready()
    {
        Initialize(Game.I._data);
    }

    public void Initialize(PlanetData data)
    {
        _data = data;

        _dataImage = Image.CreateEmpty(_data.Layers[^1].Length, _data.Layers.Count, false, Image.Format.Rgb8);
        _dataTexture = ImageTexture.CreateFromImage(_dataImage);

        _mat = new ShaderMaterial { Shader = PlanetShader };
        _mat.SetShaderParameter("data_texture", _dataTexture);
        _mat.SetShaderParameter("atlas_texture", AtlasTexture);
        _mat.SetShaderParameter("atlas_columns", AtlasColumns);
        _mat.SetShaderParameter("total_layers", _data.Layers.Count);
        _mat.SetShaderParameter("max_layer_width", Consts.MaxLayerWidth);

        BuildFullMesh();
        RefreshDataTexture();
    }

    private void BuildFullMesh()
    {
        var st = new SurfaceTool();
        st.Begin(Mesh.PrimitiveType.Triangles);

        for (int layer = 0; layer < _data.Layers.Count; layer++)
        {
            PlanetTile[] tiles = _data.Layers[layer];
            int tileCount = tiles.Length;
            int prevTileCount = (layer == 0) ? tileCount : _data.Layers[layer - 1].Length;
            bool isSplitLayer = tileCount > prevTileCount;

            float innerR = layer * _data.TileSize;
            float outerR = (layer + 1) * _data.TileSize;
            float angleStep = Mathf.Tau / tileCount;

            Vector2[] parentTops = null;
            if (isSplitLayer)
            {
                parentTops = new Vector2[prevTileCount + 1];
                float pStep = Mathf.Tau / prevTileCount;

                for (int p = 0; p <= prevTileCount; p++)
                    parentTops[p] = Polar(innerR, p * pStep);
            }

            for (int i = 0; i < tileCount; i++)
            {
                float a0 = i * angleStep;
                float a1 = (i + 1) * angleStep;

                Vector2 p00, p01;
                if (layer == 0)
                {
                    p00 = Vector2.Zero;
                    p01 = Vector2.Zero;
                }
                else
                {
                    p00 = Polar(innerR, a0);
                    p01 = Polar(innerR, a1);

                    if (isSplitLayer)
                    {
                        int parentIdx = i / (tileCount / prevTileCount);
                        Vector2 pMid = (parentTops[parentIdx] + parentTops[parentIdx + 1]) * 0.5f;
                        if (i % 2 == 1)
                            p00 = pMid;
                        else
                            p01 = pMid;
                    }
                }

                Vector2 p10 = Polar(outerR, a0);
                Vector2 p11 = Polar(outerR, a1);

                if (isSplitLayer)
                {
                    int parentIdx = i / (tileCount / prevTileCount);
                    Vector2 pMid = (parentTops[parentIdx] + parentTops[parentIdx + 1]) * 0.5f;
                    if (i % 2 == 1) p00 = pMid;
                    else p01 = pMid;
                }

                Vector2 dataUV = new Vector2((i + 0.5f) / tileCount, (layer + 0.5f) / _data.Layers.Count);
                AddTileToSurface(st, tileCount, p00, p01, p10, p11, dataUV);
            }
        }

        var meshInstance = new MeshInstance2D
        {
            Mesh = st.Commit(),
            Material = _mat,
            Texture = AtlasTexture,
            TextureFilter = TextureFilterEnum.Nearest,
            TextureRepeat = TextureRepeatEnum.Disabled
        };
        AddChild(meshInstance);
    }

    private void AddTileToSurface(SurfaceTool st, int tileCount, Vector2 p00, Vector2 p01, Vector2 p10,
        Vector2 p11, Vector2 dataUV)
    {
        void V(Vector2 pos, int type)
        {
            // R: Corner Type (0-3)
            // G: Tile Index (0 to MAX_DIM)
            st.SetColor(new Color(
                type / 255f,
                tileCount / Consts.MaxLayerWidth,
                0,
                0
            ));
            st.SetUV(dataUV);
            st.SetUV2(new Vector2(0, dataUV.Y));
            st.AddVertex(new Vector3(pos.X, pos.Y, 0));
        }

        // 0:BL, 1:BR, 2:TL, 3:TR
        V(p00, 0);
        V(p10, 2);
        V(p11, 3);
        V(p00, 0);
        V(p11, 3);
        V(p01, 1);
    }

    private Vector2 Polar(float r, float angle)
    {
        return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * r;
    }

    private void RefreshDataTexture()
    {
        int maxTiles = _data.Layers[^1].Length;

        for (int layer = 0; layer < _data.Layers.Count; layer++)
        {
            PlanetTile[] layerTiles = _data.Layers[layer];
            int tileCount = layerTiles.Length;
            // How many pixels wide is one tile in this layer?
            int multiplier = maxTiles / tileCount;

            for (int tile = 0; tile < tileCount; tile++)
            {
                PlanetTile tileData = layerTiles[tile];
                Color pixelColor = new Color(
                    (int)tileData.Material / 255f,
                    tileData.Regrowing ? 1f : Math.Max(0, tileData.Integrity / 2f),
                    tileData.Light
                );

                // "Stretch" the tile by filling all pixels in its block
                for (int m = 0; m < multiplier; m++)
                {
                    int pixelX = (tile * multiplier) + m;
                    _dataImage.SetPixel(pixelX, layer, pixelColor);
                }
            }
        }

        _dataTexture.Update(_dataImage);
    }

    private double _lastUpdate = 0;

    public override void _PhysicsProcess(double delta)
    {
        _lastUpdate += delta;

        if (_lastUpdate > 0.1f)
        {
            _lastUpdate = 0;
            isDirty = true;
        }
    }

    public override void _Process(double delta)
    {
        // if (Input.IsActionPressed("press"))
        // {
        //     Vector2 local = ToLocal(GetGlobalMousePosition());
        //     float dist = local.Length() - _data._innerGrowth;
        //     Vector2 virtualPos = local.Normalized() * dist;
        //
        //     if (_data.LocalPositionToPolarCoords(virtualPos, out int layer, out int tile))
        //     {
        //         PlanetTile t = _data.Layers[layer][tile];
        //
        //         RecipeID recipe = t.Destroy();
        //
        //         _data.PropagateLight(layer, tile, PlanetTile.LightMax);
        //                 
        //         if (recipe != RecipeID.None)
        //         {
        //             foreach ((Item item, int amount) tuple in ItemRecipe.TryGetOutput(recipe))
        //             {
        //                 for (int i = 0; i < tuple.amount; i++)
        //                 {
        //                     Pickup.Instantiate(new Vector2(tile + 0.5f, layer + 0.5f), tuple.item);
        //                 }
        //             }
        //         }
        //     }
        // }


        if (!Inventory.IsResearchUnlocked(RecipeID.Research_OrbitalCoreExtractor))
        {
            float newGrowth = _data._innerGrowth + PlanetData.GrowthSpeed * (float)delta;

            if (_data._innerGrowth < 0 && newGrowth >= 0)
            {
                _data.RegrowLayers();
                isDirty = true;
            }

            _data._innerGrowth = newGrowth;

            // growth finished, regrow a layer
            if (_data._innerGrowth >= 1)
            {
                _data._innerGrowth = -Game.RandomAround(8, 2);
                isLightDirty = true;
                isDirty = true;

                // reset Growing state on tiles
                foreach (PlanetTile[] layerData in _data.Layers)
                {
                    for (int tile = 0; tile < layerData.Length; tile++)
                    {
                        PlanetTile tileData = layerData[tile];
                        tileData.Regrowing = false;
                    }
                }

                foreach (Node node in Game.I.Pawns.GetChildren())
                {
                    if (node is Pawn pawn)
                    {
                        PlanetTile tile = _data.GetTileAtPolarCoords(pawn.PolarPos.X, pawn.PolarPos.Y);
                        if (tile != null && !tile.IsEmpty())
                            pawn.PolarPos.Y += 1;
                    }
                }

                foreach (Node node in Game.I.Pickups.GetChildren())
                {
                    if (node is Pickup pickup)
                    {
                        PlanetTile tile = _data.GetTileAtPolarCoords(pickup.PolarPos.X, pickup.PolarPos.Y);
                        if (tile != null && !tile.IsEmpty())
                            pickup.PolarPos.Y += 1;
                    }
                }
            }
        }

        if (isLightDirty)
        {
            _data.RecalculateGlobalLight();
            isLightDirty = false;
        }

        if (isDirty)
        {
            _lastUpdate = 0;
            RefreshDataTexture();
            isDirty = false;
        }

        float renderGrowth = Mathf.Clamp(_data._innerGrowth, 0, 1);
        _mat.SetShaderParameter("inner_growth", renderGrowth);
    }
}