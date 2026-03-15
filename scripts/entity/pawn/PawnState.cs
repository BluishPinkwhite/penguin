namespace Incremental.scripts.entity.pawn;

public enum PawnState
{
    // gravity ON
    Idle = 1,
    Move = 2,
    Action = 3,
    ReturnH = 4,
    ReturnV = 5,
    
    // gravity OFF
    DropOff = -1
}