public class AgentTest : NPC
{
	public override void OnStart() => base.OnStart();
	
	public override void OnUpdate() => base.OnUpdate();

	public override void OnFixedUpdate() => base.OnFixedUpdate();

    protected override void CheckForPlayer() => base.CheckForPlayer();

    protected override void HandleMovement() => base.HandleMovement();
    
	protected override void Wander(AILocationSelectorScript.LocationType locationType) => base.Wander(AILocationSelectorScript.LocationType.Default);
	
	protected override void TargetPlayer() => base.TargetPlayer();
}