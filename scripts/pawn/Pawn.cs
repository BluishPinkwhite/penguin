using Godot;
using Incremental.scripts.director;
using Incremental.scripts.planet.data;

namespace Incremental.scripts.pawn;

public partial class Pawn : Node2D
{
    private Vector2 _polar_pos; // X = tile, Y = layer
    private int _prevLayer;

    private Vector2 _target;
    private bool _targetReached;

    private PawnState _state = PawnState.Idle;
    private float _cooldown = 0;

    const float PawnAngularWidth = 0.9f;
    const float PawnHeight = PawnAngularWidth;

    public override void _Ready()
    {
        _polar_pos = new Vector2(0, 120);
        _prevLayer = (int)_polar_pos.Y;
    }

    public override void _PhysicsProcess(double delta)
    {
        // if (_target != PlanetRenderer.target)
        // {
        //     _target = PlanetRenderer.target;
        //     _targetReached = false;
        // }

        float d = (float)delta;

        int layer = Mathf.FloorToInt(_polar_pos.Y);
        int prevSize = Game.I._data.GetLayerSize(_prevLayer);
        int currSize = Game.I._data.GetLayerSize(layer);

        if (layer != _prevLayer)
            _polar_pos.X = (_polar_pos.X * prevSize / (float)currSize);
        _polar_pos.X = Mathf.PosMod(_polar_pos.X, currSize);

        // gravity
        float gravityY = _polar_pos.Y - d * 10f;
        if (!CheckCollision(_polar_pos.X, gravityY))
            _polar_pos.Y = gravityY;
        gravityY = _polar_pos.Y - d * 10f;


        GD.Print(_cooldown > 0 ? "Cooldown: " + _cooldown : _state);

        if (_cooldown > 0)
        {
            _cooldown -= d;
        }
        else if (_state == PawnState.Idle)
        {
            PlanetTile below = Game.I._data.GetTileAtPolarCoords(
                Mathf.FloorToInt(_polar_pos.X), Mathf.FloorToInt(gravityY));
            if (below != null && !below.Destroyed)
            {
                bool found = false;
                for (int i = Game.I._data.Layers.Count - 1; i >= 0; i--)
                {
                    if (found)
                        break;

                    PlanetTile[] layerData = Game.I._data.Layers[i];
                    for (int j = 0; j < layerData.Length; j++)
                    {
                        PlanetTile tileData = layerData[j];
                        if (!tileData.Destroyed && tileData.Material != TileMaterial.Unknown)
                        {
                            _target = new Vector2(j + 0.5f, i + 1.5f);
                            _state = PawnState.Move;
                            _targetReached = false;
                            _cooldown = 1f;
                            found = true;
                            break;
                        }
                    }
                }
            }
        }
        else if (_state is PawnState.Move or PawnState.Return)
        {
            int targetLayer = Mathf.FloorToInt(_target.Y);
            int targetSize = Game.I._data.GetLayerSize(targetLayer);
            float targetX = _target.X * currSize / targetSize;
            targetX = Mathf.PosMod(targetX, currSize);


            float dx = CircularDelta(_polar_pos.X, targetX, currSize);
            float dy = _target.Y - _polar_pos.Y;

            // if (dx * dx + dy * dy > 0.05f)
            GD.Print($"[{Mathf.FloorToInt(_target.X)}, {Mathf.FloorToInt(_polar_pos.X)}], [{Mathf.FloorToInt(_target.Y)}, {Mathf.FloorToInt(_polar_pos.Y)}]");
            if (dx * dx + dy * dy > 0.05f ||
                Mathf.FloorToInt(_target.X) != Mathf.FloorToInt(_polar_pos.X) ||
                Mathf.FloorToInt(_target.Y) != Mathf.FloorToInt(_polar_pos.Y))
            {
                // horizontal movement
                float stepX = Mathf.Clamp(dx * 5, -1f, 1f) * d * 10f;
                float newX = _polar_pos.X + stepX;

                if (!CheckCollision(newX, _polar_pos.Y))
                    _polar_pos.X = newX;

                _polar_pos.X = Mathf.PosMod(_polar_pos.X, currSize);


                // vertical movement
                float stepY = Mathf.Clamp(dy * 5, -1f, 1f) * d * 12f;
                float newY = _polar_pos.Y + stepY;

                if (!CheckCollision(_polar_pos.X, newY))
                    _polar_pos.Y = newY;
            }
            else
            {
                if (_state == PawnState.Move)
                {
                    _state = PawnState.Mine;
                    _cooldown = 1f;
                }
                else if (_state == PawnState.Return)
                {
                    _state = PawnState.Idle;
                    _cooldown = 1f;
                }
            }
        }
        else if (_state == PawnState.Mine)
        {
            // find new tile when this tile was broken by someone else
            if (Mathf.FloorToInt(_target.X) != Mathf.FloorToInt(_polar_pos.X) ||
                Mathf.FloorToInt(_target.Y) != Mathf.FloorToInt(_polar_pos.Y))
            {
                _state = PawnState.Idle;
            }
            else
            {
                PlanetTile below = Game.I._data.GetTileAtPolarCoords(
                    Mathf.FloorToInt(_polar_pos.X), Mathf.FloorToInt(gravityY));

                GD.Print(below == null ? "null" : below.Integrity + " " + below.Material);

                if (below != null && !below.Destroyed)
                {
                    below.Integrity -= d * 0.2f;

                    if (below.Integrity < 0)
                    {
                        below.Integrity = 0;
                        below.Destroyed = true;
                        below.Material = TileMaterial.Unknown;
                        PlanetRenderer.SetChunkDirty(Mathf.FloorToInt(gravityY));

                        _state = PawnState.Return;
                        _target = new Vector2(10.5f, Game.I._data.Layers.Count + 10.5f);
                        _targetReached = false;
                        _cooldown = 1f;
                    }
                }
            }
        }


        if (_polar_pos.Y < 0f)
            _polar_pos.Y = 0f;


        // apply and display
        Position = Game.I._data.PolarToWorld(_polar_pos.X, _polar_pos.Y);
        Rotation = Mathf.Atan2(Position.Y, Position.X) + Mathf.Pi / 2f;

        _prevLayer = layer;
    }

    private float CircularDelta(float from, float to, float size)
    {
        float diff = Mathf.PosMod(to - from, size);

        if (diff > size / 2f)
            diff -= size;

        return diff;
    }

    private float ScaleTileBetweenLayers(float tile, int fromLayer, int toLayer)
    {
        int fromSize = Game.I._data.GetLayerSize(fromLayer);
        int toSize = Game.I._data.GetLayerSize(toLayer);

        return tile * toSize / (float)fromSize;
    }

    private bool IsAir(int tile, int layer)
    {
        if (layer < 0)
            return false;

        int size = Game.I._data.GetLayerSize(layer);
        tile = Mathf.PosMod(tile, size);

        PlanetTile t = Game.I._data.GetTileAtPolarCoords(tile, layer);

        return t == null || t.Destroyed || t.Material == TileMaterial.Unknown;
    }

    bool IsSolid(int tile, int layer)
    {
        PlanetTile t = Game.I._data.GetTileAtPolarCoords(tile, layer);

        return t != null &&
               !t.Destroyed &&
               t.Material != TileMaterial.Unknown;
    }


    float GetHalfWidthTiles(int layer)
    {
        int size = Game.I._data.GetLayerSize(layer);
        return PawnAngularWidth / size;
    }

    bool CheckCollision(float x, float y)
    {
        int layerBottom = Mathf.FloorToInt(y);
        int layerTop = Mathf.FloorToInt(y + PawnHeight);

        float halfW = GetHalfWidthTiles(layerBottom);

        int sizeBottom = Game.I._data.GetLayerSize(layerBottom);
        int sizeTop = Game.I._data.GetLayerSize(layerTop);

        int leftBottom = Mathf.FloorToInt(Mathf.PosMod(x - halfW, sizeBottom));
        int rightBottom = Mathf.FloorToInt(Mathf.PosMod(x + halfW, sizeBottom));

        int leftTop = Mathf.FloorToInt(Mathf.PosMod(x - halfW, sizeTop));
        int rightTop = Mathf.FloorToInt(Mathf.PosMod(x + halfW, sizeTop));

        if (IsSolid(leftBottom, layerBottom)) return true;
        if (IsSolid(rightBottom, layerBottom)) return true;
        if (IsSolid(leftTop, layerTop)) return true;
        if (IsSolid(rightTop, layerTop)) return true;

        return false;
    }
}