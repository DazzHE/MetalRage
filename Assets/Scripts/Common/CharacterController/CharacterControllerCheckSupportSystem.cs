using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;

[UpdateAfter(typeof(CharacterControllerStepSystem))]
[AlwaysSynchronizeSystem]
[AlwaysUpdateSystem]
public class CharacterControllerCheckSupportSystem : JobComponentSystem {
    BuildPhysicsWorld m_BuildPhysicsWorldSystem;

    protected override void OnCreate() {
        m_BuildPhysicsWorldSystem = World.GetOrCreateSystem<BuildPhysicsWorld>();
    }

    protected override unsafe JobHandle OnUpdate(JobHandle inputDeps) {
        var physicsWorld = m_BuildPhysicsWorldSystem.PhysicsWorld;

        var constraints = new NativeList<SurfaceConstraintInfo>(Allocator.Temp);
        var castHits = new NativeList<ColliderCastHit>(Allocator.Temp);

        var elapsedTime = this.Time.ElapsedTime;
        var deltaTime = this.Time.DeltaTime;

        Entities
            .WithName("CheckSupportJob")
            .ForEach((
                    ref CharacterRigidbody ccData,
                    ref CharacterPhysicsInput ccQuery,
                    ref CharacterPhysicsOutput resultPosition,
                    ref CharacterPhysicsVelocity velocity,
                    ref CharacterControllerCollider ccCollider,
                    ref GroundContactStatus ccGroundData) => {
                        if (!ccQuery.CheckSupport) {
                            ccGroundData.SupportedState = CharacterControllerUtilities.CharacterSupportState.Unsupported;
                            ccGroundData.SurfaceVelocity = float3.zero;
                            ccGroundData.SurfaceNormal = float3.zero;
                            return;
                        }

                        constraints.Clear();
                        castHits.Clear();

                        var collider = (Collider*)ccCollider.Collider.GetUnsafePtr();

                        var stepInput = new CharacterControllerUtilities.CharacterControllerStepInput {
                            World = physicsWorld,
                            DeltaTime = deltaTime,
                            Up = math.up(),
                            Gravity = new float3(0.0f, -9.8f, 0.0f),
                            MaxIterations = ccData.MaxIterations,
                            Tau = CharacterControllerUtilities.k_DefaultTau,
                            Damping = CharacterControllerUtilities.k_DefaultDamping,
                            SkinWidth = ccData.SkinWidth,
                            ContactTolerance = ccData.ContactTolerance * 2.0f,
                            MaxSlope = ccData.MaxSlope,
                            RigidBodyIndex = -1,
                            CurrentVelocity = velocity.Velocity,
                            MaxMovementSpeed = ccData.MaxMovementSpeed
                        };

                        var transform = new RigidTransform {
                            pos = resultPosition.MoveResult,
                            rot = quaternion.identity
                        };

                        // FollowGround can cause the collider to lift further above ground
                        // before entering upwards slopes or exiting downwards slopes.
                        // Halfpipes show the issue the most.
                        // Lengthen the ground probe vector to remove undesired unsupporteds.
                        float probeFactor = ccQuery.FollowGround ? 2 : 1;

                        // Check support
                        CharacterControllerUtilities.CheckSupport(
                            ref physicsWorld,
                            collider,
                            stepInput,
                            ccData.GroundProbeVector * probeFactor,
                            transform,
                            ccData.MaxSlope,
                            ref constraints,
                            ref castHits,
                            out ccGroundData.SupportedState,
                            out ccGroundData.SurfaceNormal,
                            out ccGroundData.SurfaceVelocity);
                    }).Run();

        constraints.Dispose();
        castHits.Dispose();

        return inputDeps;
    }
}
