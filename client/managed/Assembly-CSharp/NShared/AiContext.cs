namespace NShared;

public class AiContext : SnapshotObject
{
	private readonly SnapshotObjectMember<BattleCreature> _creatureHolder;

	private readonly SnapshotObjectMember<BlackBoard> _blackBoardHolder;

	public BattleCreature Creature
	{
		get
		{
			return _creatureHolder.Get();
		}
		set
		{
			_creatureHolder.Set(value);
		}
	}

	public BlackBoard BlackBoard
	{
		get
		{
			return _blackBoardHolder.Get();
		}
		set
		{
			_blackBoardHolder.Set(value);
		}
	}

	public BattleInstance BattleInstance
	{
		get
		{
			if (Creature == null)
			{
				return null;
			}
			return Creature.BattleInstance;
		}
	}

	public AiContext()
	{
		_creatureHolder = new SnapshotObjectMember<BattleCreature>(this);
		_blackBoardHolder = new SnapshotObjectMember<BlackBoard>(this);
	}

	public void Init(BattleCreature owner)
	{
		_creatureHolder.Set(owner);
	}

	public long GetCurrentTimeMs()
	{
		return BattleInstance.MainTimer.GetCurrentTimeMs();
	}
}
