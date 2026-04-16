namespace Incremental.scripts.entity.pawn;

public enum PawnState
{
    // gravity ON
    Idle = 1,
    Move = 2,
    Action = 3,
    ReturnH = 4,
    ReturnV = 5,
    
    RetireH = 10,
    RetireV = 11,
    GiveUp = 12,
    
    // gravity OFF
    DropOff = -1,
}