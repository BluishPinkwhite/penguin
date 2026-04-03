using System;
using Godot;
using Incremental.scripts.director;
using Incremental.scripts.planet.data;

namespace Incremental.scripts.planet.rendering;

public partial class PlanetRenderer : Node2D
{
    [Export] public Texture2D AtlasTexture;
    [Export] public int AtlasColumns = 4;
    [Export] public Shader PlanetShader;

    private PlanetData _data;
    private Image _dataImage;
    private ImageTexture _dataTexture;
    private ShaderMaterial _mat;

    public override void _Ready() => Initialize(Game.I._data);

    public void Initialize(PlanetData data)
    {
        _data = data;

        int maxTiles = 0;
        foreach (var layer in _data.Layers)
            maxTiles = Math.Max(maxTiles, layer.Length);

        _dataImage = Image.CreateEmpty(maxTiles, _data.Layers.Count, false, Image.Format.Rgb8);
        _dataTexture = ImageTexture.CreateFromImage(_dataImage);

        _mat = new ShaderMaterial { Shader = PlanetShader };
        _mat.SetShaderParameter("data_texture", _dataTexture);
        _mat.SetShaderParameter("atlas_texture", AtlasTexture);
        _mat.SetShaderParameter("atlas_columns", AtlasColumns);
        _mat.SetShaderParameter("total_layers", _data.Layers.Count);

        BuildFullMesh(maxTiles);
        RefreshDataTexture();
    }

    private void BuildFullMesh(int maxTiles)
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
                for (int p = 0; p <= prevTileCount; p++) parentTops[p] = Polar(innerR, p * pStep);
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
                        if (i % 2 == 1) p00 = pMid;
                        else p01 = pMid;
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

                Vector2 dataUV = new Vector2((i + 0.5f) / maxTiles, (layer + 0.5f) / _data.Layers.Count);
                AddTileToSurface(st, p00, p01, p10, p11, dataUV);
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

    private void AddTileToSurface(SurfaceTool st, Vector2 p00, Vector2 p01, Vector2 p10, Vector2 p11, Vector2 dataUV)
    {
        void V(Vector2 pos, int type)
        {
            st.SetColor(new Color(type / 255f, 0, 0));
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

    private Vector2 Polar(float r, float angle) => new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * r;

    private void RefreshDataTexture()
    {
        for (int l = 0; l < _data.Layers.Count; l++)
        {
            for (int t = 0; t < _data.Layers[l].Length; t++)
            {
                var tile = _data.Layers[l][t];
                _dataImage.SetPixel(t, l, new Color((int)tile.Material / 255f, tile.Destroyed ? 0 : 1f, 0));
            }
        }

        _dataTexture.Update(_dataImage);
    }

    public override void _Process(double delta)
    {
        _data._innerGrowth += PlanetData.GrowthSpeed * (float)delta;
        if (_data._innerGrowth >= _data.TileSize)
        {
            _data._innerGrowth = 0;
            RefreshDataTexture();
        }

        _mat.SetShaderParameter("inner_growth", _data._innerGrowth);
    }
}