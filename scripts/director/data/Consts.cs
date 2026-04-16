using System.Collections.Generic;
using Incremental.scripts.entity.pawn.roles;

namespace Incremental.scripts.director.data;

public static class Consts
{
    public const float MaxLayerWidth = 1024f;

    public static readonly Dictionary<Role, PawnConfig> Pawns = new()
    {
        { Role.Miner, new PawnConfig(50, 0.7f, 0.8f)},
        { Role.Hauler, new PawnConfig(250, 1.25f, 1.5f)},
        { Role.Archeologist, new PawnConfig(25, 0.5f, 0.5f)},
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