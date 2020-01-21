using Unity.Entities;
using UnityEngine;

class WalkActionSystem : ComponentSystem {
    protected override void OnUpdate() {
        this.Entities.ForEach<MechAction, WalkActionConfigData>(UpdateEntity);
    }

    private void UpdateEntity(ref MechAction mechAction, ref WalkActionConfigData config) {
        if (!mechAction.IsActive) {
            mechAction.State = ActionState.RequestingActivation;
            return;
        }
        var locoStatus = this.EntityManager.GetComponentData<MechLocoStatus>(mechAction.Owner);
        //if (!locoStatus.IsOnGround) {
        //    return;
        //}
        var velocity = new Vector3(locoStatus.Velocity.x, 0f, locoStatus.Velocity.z);
        velocity.x += InputSystem.GetHorizontalMotion() * config.Accel * Time.deltaTime;
        velocity.z += InputSystem.GetVerticalMotion() * config.Accel * Time.deltaTime;
        if (InputSystem.GetHorizontalMotion() == 0) {
            var xAbs = Mathf.Max(Mathf.Abs(velocity.x) - config.Decel * Time.deltaTime, 0f);
            velocity.x = velocity.x > 0 ? xAbs : -xAbs;
        }
        if (InputSystem.GetVerticalMotion() == 0) {
            var zAbs = Mathf.Max(Mathf.Abs(velocity.z) - config.Decel * Time.deltaTime, 0f);
            velocity.z = velocity.z > 0 ? zAbs : -zAbs;
        }
        velocity = velocity.magnitude > config.MaxSpeed ? velocity.normalized * config.MaxSpeed : velocity;
        var command = new MechLocoCommand {
            Motion = velocity * Time.deltaTime,
            NextState = velocity.magnitude == 0 ? MechLocoState.Idle : MechLocoState.Walking
        };
        this.EntityManager.SetComponentData(mechAction.Owner, command);
        //this.animator.SetFloat("WalkSpeed", this.MoveDirection.magnitude);
    }
}