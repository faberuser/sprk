using NShared;

namespace NGame2.NBattleThread.NEventService;

public class EventRouter : BattleEventRouter
{
	public EventRouter()
	{
		Register(new GenericEventService<SpawnProjectile>());
		Register(new GenericEventService<SpawnCreature>());
		Register(new GenericEventService<DestroyCreature>());
		Register(new GenericEventService<SetPosition>());
		Register(new GenericEventService<BeginMove>());
		Register(new GenericEventService<WaveReady>());
		Register(new GenericEventService<CastSkill>());
		Register(new GenericEventService<CancelCastingSkill>());
		Register(new GenericEventService<BeginFireSkill>());
		Register(new GenericEventService<FireSkill>());
		Register(new GenericEventService<EndFireSkill>());
		Register(new GenericEventService<BeginOperation>());
		Register(new GenericEventService<EndOperation>());
		Register(new GenericEventService<CancelOperation>());
		Register(new GenericEventService<ReserveOperation>());
		Register(new GenericEventService<FlyingOperation>());
		Register(new GenericEventService<ResistOperation>());
		Register(new GenericEventService<Damage>());
		Register(new GenericEventService<Heal>());
		Register(new GenericEventService<DamageModified>());
		Register(new GenericEventService<ChangeMp>());
		Register(new GenericEventService<ChangeState>());
		Register(new GenericEventService<ShieldBegin>());
		Register(new GenericEventService<ShieldEnd>());
		Register(new GenericEventService<ShieldModified>());
		Register(new GenericEventService<EventTrigger>());
		Register(new GenericEventService<KarmaCardSuffle>());
	}
}
