using Godot;
using Incremental.scripts.director;
using Incremental.scripts.entity.station;
using Incremental.scripts.pawn;
using Incremental.scripts.planet.data;

namespace Incremental.scripts.entity.pawn;

public partial class Pawn : SurfaceEntity
{
    protected PawnState _state = PawnState.Idle;
    protected Role _role = Role.Unemployed;
    protected bool _flying = false;

    protected float _cooldown = 0;
    protected int _ID;

    [Export] protected Label _debugText;


    const float PawnAngularWidth = 1.45f;
    const float PawnHeight = PawnAngularWidth;

    private static int _nextID = 0;


    public override void _Ready()
    {
        _ID = _nextID++;

        _polar_pos = new Vector2(
            Game.RandomTo(Game.I._data.GetLayerSize(Game.I._data.Layers.Count)),
            Game.I._data.Layers.Count + 2);

        base._Ready();
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
        _debugText.Text = _cooldown > 0 ? $"<{_cooldown:F1}>" : $"[{_state}]";


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
        else if (_state is PawnState.Move or PawnState.ReturnH)
        {
            int targetLayer = Mathf.FloorToInt(_target.Y);
            int targetSize = Game.I._data.GetLayerSize(targetLayer);
            float targetX = _target.X * _currSize / targetSize;
            targetX = Mathf.PosMod(targetX, _currSize);

            float dx = CircularDelta(_polar_pos.X, targetX, _currSize);
            float dy = _target.Y - _polar_pos.Y;
            
            float stepX = Mathf.Clamp(dx, -1f, 1f) * d * 10f;
            float newX = _polar_pos.X + stepX;

            
            bool reachedTarget = false;
            if (!_flying)
            {
                // horizontal movement
                if (dx * dx > 0.05f || Mathf.FloorToInt(_target.X) != Mathf.FloorToInt(_polar_pos.X + 0.45f))
                {
                    if (!CheckCollision(newX, _polar_pos.Y))
                        _polar_pos.X = newX;
                    else
                        _flying = true;
                }
                // else if (dy * dy > 0.1f || Mathf.FloorToInt(_target.Y) != Mathf.FloorToInt(_polar_pos.Y))
                // {
                    // _flying = true;
                // }
                else
                {
                    reachedTarget = true;
                }
            }
            else
            {
                {
                    // vertical movement
                    float stepY = 12 * d;//Mathf.Clamp(dy, -1f, 1f) * d * 12f;
                    float newY = _polar_pos.Y + stepY;

                    if (!CheckCollision(_polar_pos.X, newY))
                        _polar_pos.Y = newY;
                }
                // else 
                if (!CheckCollision(newX, _polar_pos.Y))
                {
                    _polar_pos.X = newX;
                    _flying = false;
                }
            }
            
            _polar_pos.X = Mathf.PosMod(_polar_pos.X, _currSize);
            
            _debugText.Text +=
                $" {{{dx * dx}; | {_target.X};{_target.Y} | {_polar_pos.X};{_polar_pos.Y}}}";

            if (reachedTarget)
            {
                if (_state == PawnState.Move)
                {
                    _state = PawnState.Mine;
                    SetCooldown(1);
                }
                else if (_state == PawnState.ReturnH)
                {
                    _state = PawnState.ReturnV;
                    _target = ResourceStation.I.PolarPos;
                    SetCooldown(0.35f);
                }
            }
        }
        else if (_state == PawnState.ReturnV)
        {
            float dy = _target.Y - _polar_pos.Y;
            if (dy * dy > 0.1f || Mathf.FloorToInt(_target.Y) != Mathf.FloorToInt(_polar_pos.Y))
            {
                // vertical movement
                float stepY = Mathf.Clamp(dy * 5, -1f, 1f) * d * 12f;
                float newY = _polar_pos.Y + stepY;

                if (!CheckCollision(_polar_pos.X, newY))
                    _polar_pos.Y = newY;
            }
            else
            {
                _state = PawnState.DropOff;
                SetCooldown(2.5f);
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

                        // shift "below station" down
                        if (Mathf.FloorToInt(_polar_pos.X) == Mathf.FloorToInt(ResourceStation.I.Below.X))
                        {
                            ResourceStation.I.Below =
                                new Vector2(ResourceStation.I.Below.X, ResourceStation.I.Below.Y - 1);
                        }

                        _state = PawnState.ReturnH;
                        _target = new Vector2(ResourceStation.I.Below.X, ResourceStation.I.Below.Y);
                        SetCooldown(1);
                    }
                }
            }
        }
        else if (_state == PawnState.DropOff)
        {
            _state = PawnState.Idle;
            SetCooldown(1);
        }
    }

    private void GetNewMiningTarget()
    {
        if (Game.I._data.NextMiningTarget(_ID, out Vector2 target))
        {
            _target = new Vector2(target.X + 0.5f, target.Y + 1.5f);
            _state = PawnState.Move;
            SetCooldown(1);
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

    protected void SetCooldown(float value)
    {
        _cooldown = Game.RandomAround(value, value * 0.25f);
    }

    public void SetRole(Role role)
    {
        if (_role == role)
            return;

        _role = role;
        GetNode<Sprite2D>("Profession").SetRegionRect(
            new Rect2(32 * ((int)role % 8), 32 * (int)((int)role / 8), 32, 32));
    }
}