using Unity.Entities;
using UnityEngine;

public struct MechMovementStatus : IComponentData {
    public BlittableBool IsOnGround;
    public MechMovementState State;
    public Vector3 Velocity;
    public float LegYaw;
}