using System.Globalization;
using Godot;
using Incremental.scripts.director;
using Incremental.scripts.entity.station;
using Incremental.scripts.pawn;
using Incremental.scripts.planet.data;

namespace Incremental.scripts.entity.pawn;

public partial class Pawn : SurfaceEntity
{
    protected PawnState _state = PawnState.Idle;
    protected float _cooldown = 0;

    [Export] protected Label _debugText;
    

    const float PawnAngularWidth = 0.9f;
    const float PawnHeight = PawnAngularWidth;

    
    public override void _Ready()
    {
        base._Ready();
        
        _polar_pos = new Vector2(0, 120);
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        
        float d = (float)delta;
        
        // gravity
        float gravityY = _polar_pos.Y - d * 10f;
        if (_state != PawnState.DropOff)
        {
            if (!CheckCollision(_polar_pos.X, gravityY))
                _polar_pos.Y = gravityY;
            gravityY = _polar_pos.Y - d * 10f;
        }

        // debug
        _debugText.Text = _cooldown > 0 ? 
            $"<{_cooldown:F1}>" : 
            $"[{_state}]";

        
        // behaviour
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
                GetNewMiningTarget();
            }
        }
        else if (_state is PawnState.Move or PawnState.ReturnH or PawnState.ReturnV)
        {
            int targetLayer = Mathf.FloorToInt(_target.Y);
            int targetSize = Game.I._data.GetLayerSize(targetLayer);
            float targetX = _target.X * _currSize / targetSize;
            targetX = Mathf.PosMod(targetX, _currSize);

            float dx = CircularDelta(_polar_pos.X, targetX, _currSize);
            float dy = _target.Y - _polar_pos.Y;

            if (dx * dx + dy * dy > 0.05f ||
                Mathf.FloorToInt(_target.X) != Mathf.FloorToInt(_polar_pos.X) ||
                Mathf.FloorToInt(_target.Y) != Mathf.FloorToInt(_polar_pos.Y))
            {
                // horizontal movement
                float stepX = Mathf.Clamp(dx * 5, -1f, 1f) * d * 10f;
                float newX = _polar_pos.X + stepX;

                if (!CheckCollision(newX, _polar_pos.Y))
                    _polar_pos.X = newX;

                _polar_pos.X = Mathf.PosMod(_polar_pos.X, _currSize);


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
                else if (_state == PawnState.ReturnH)
                {
                    _state = PawnState.ReturnV;
                    _target = ResourceStation.I.PolarPos;
                    _cooldown = 0.35f;
                }
                else if (_state == PawnState.ReturnV)
                {
                    _state = PawnState.DropOff;
                    _cooldown = 2.5f;
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

                if (below != null && !below.Destroyed)
                {
                    below.Integrity -= d * 0.2f;

                    if (below.Integrity < 0)
                    {
                        below.Integrity = 0;
                        below.Destroyed = true;
                        below.Material = TileMaterial.Unknown;
                        PlanetRenderer.SetChunkDirty(Mathf.FloorToInt(gravityY));

                        _state = PawnState.ReturnH;
                        _target = new Vector2(ResourceStation.I.PolarPos.X, ResourceStation.I.PolarPos.Y - 10);
                        _cooldown = 1f;
                    }
                }
            }
        }
        else if (_state == PawnState.DropOff)
        {
            _state = PawnState.Idle;
            _cooldown = 1f;
        }
    }

    private void GetNewMiningTarget()
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
                    _cooldown = 1f;
                    found = true;
                    break;
                }
            }
        }
    }
    
    protected float GetHalfWidthTiles(int layer)
    {
        int size = Game.I._data.GetLayerSize(layer);
        return PawnAngularWidth / size;
    }

    protected bool CheckCollision(float x, float y)
    {
        int layerBottom = Mathf.FloorToInt(y);
        float halfW = GetHalfWidthTiles(layerBottom);
        int sizeBottom = Game.I._data.GetLayerSize(layerBottom);
        
        int leftBottom = Mathf.FloorToInt(Mathf.PosMod(x - halfW, sizeBottom));
        if (IsSolid(leftBottom, layerBottom)) return true;
        
        int rightBottom = Mathf.FloorToInt(Mathf.PosMod(x + halfW, sizeBottom));
        if (IsSolid(rightBottom, layerBottom)) return true;
        
        int layerTop = Mathf.FloorToInt(y + PawnHeight);
        int sizeTop = Game.I._data.GetLayerSize(layerTop);
        int leftTop = Mathf.FloorToInt(Mathf.PosMod(x - halfW, sizeTop));
        if (IsSolid(leftTop, layerTop)) return true;
        
        int rightTop = Mathf.FloorToInt(Mathf.PosMod(x + halfW, sizeTop));
        if (IsSolid(rightTop, layerTop)) return true;

        return false;
    }
}