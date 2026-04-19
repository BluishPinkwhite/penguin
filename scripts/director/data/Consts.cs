using System.Collections.Generic;
using Incremental.scripts.entity.pawn.roles;

namespace Incremental.scripts.director.data;

public static class Consts
{
    public const float MaxLayerWidth = 1024f;

    public static readonly Dictionary<Role, PawnConfig> Pawns = new()
    {
        { Role.Miner, new PawnConfig(50, 3.5f, 1.2f)},
        { Role.Hauler, new PawnConfig(250, 5.5f, 1.4f)},
        { Role.Archeologist, new PawnConfig(25, 2f, 1.1f)},
    }; 

    public class PawnConfig
    {
        public readonly int RetirementCycles;
        public readonly float WalkSpeed;
        public readonly float FlySpeed;

        public PawnConfig(int retirementCycles, float walkSpeed, float flySpeed)
        {
            RetirementCycles = retirementCycles;
            WalkSpeed = walkSpeed;
            FlySpeed = flySpeed;
        }
    }
}