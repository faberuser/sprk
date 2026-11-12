using NShared.NUtil;

namespace NShared;

public class SkillProjectileInfo : SnapshotObject
{
	private readonly SnapshotObjectMember<long> _uidHolder;

	private readonly SnapshotObjectMember<BattleCreature> _firedCreatureHolder;

	private readonly SnapshotObjectMember<SkillData> _skillDataHolder;

	private readonly SnapshotObjectMember<SkillProjectileData> _projectileDataHolder;

	private readonly SnapshotObjectMember<int> _skillLevelHolder;

	private readonly SnapshotObjectMember<Vector3> _positionHolder;

	private readonly SnapshotObjectMember<Vector3> _directionHolder;

	private readonly SnapshotObjectMember<BattleCreature> _followTargetHolder;

	private readonly SnapshotObjectMember<SnapshotList<BattleCreature>> _touchedCreatureListHolder;

	private readonly SnapshotObjectMember<long> _elapsedTimeMsHolder;

	private readonly SnapshotObjectMember<long> _nextTickTimeMsHolder;

	private readonly SnapshotObjectMember<int> _radiusMmHolder;

	public long Uid
	{
		get
		{
			return _uidHolder.Get();
		}
		set
		{
			_uidHolder.Set(value);
		}
	}

	public BattleCreature FiredCreature
	{
		get
		{
			return _firedCreatureHolder.Get();
		}
		set
		{
			_firedCreatureHolder.Set(value);
		}
	}

	public SkillData SkillData
	{
		get
		{
			return _skillDataHolder.Get();
		}
		set
		{
			_skillDataHolder.Set(value);
		}
	}

	public SkillProjectileData ProjectileData
	{
		get
		{
			return _projectileDataHolder.Get();
		}
		set
		{
			_projectileDataHolder.Set(value);
		}
	}

	public int SkillLevel
	{
		get
		{
			return _skillLevelHolder.Get();
		}
		set
		{
			_skillLevelHolder.Set(value);
		}
	}

	public Vector3 Position
	{
		get
		{
			return _positionHolder.Get();
		}
		set
		{
			_positionHolder.Set(value);
		}
	}

	public Vector3 Direction
	{
		get
		{
			return _directionHolder.Get();
		}
		set
		{
			_directionHolder.Set(value);
		}
	}

	public BattleCreature FollowTarget
	{
		get
		{
			return _followTargetHolder.Get();
		}
		set
		{
			_followTargetHolder.Set(value);
		}
	}

	public SnapshotList<BattleCreature> TouchedCreatureList
	{
		get
		{
			return _touchedCreatureListHolder.Get();
		}
		set
		{
			_touchedCreatureListHolder.Set(value);
		}
	}

	public long ElapsedTimeMs
	{
		get
		{
			return _elapsedTimeMsHolder.Get();
		}
		set
		{
			_elapsedTimeMsHolder.Set(value);
		}
	}

	public long NextTickTimeMs
	{
		get
		{
			return _nextTickTimeMsHolder.Get();
		}
		set
		{
			_nextTickTimeMsHolder.Set(value);
		}
	}

	public int SetRadiusMm
	{
		get
		{
			return _radiusMmHolder.Get();
		}
		set
		{
			_radiusMmHolder.Set(value);
		}
	}

	public SkillProjectileInfo()
	{
		_uidHolder = new SnapshotObjectMember<long>(this, 0L);
		_firedCreatureHolder = new SnapshotObjectMember<BattleCreature>(this);
		_skillDataHolder = new SnapshotObjectMember<SkillData>(this, null, GameRoot.Instance.SkillDataContainer);
		_projectileDataHolder = new SnapshotObjectMember<SkillProjectileData>(this, null, GameRoot.Instance.SkillProjectileDataContainer);
		_skillLevelHolder = new SnapshotObjectMember<int>(this, 1);
		_positionHolder = new SnapshotObjectMember<Vector3>(this);
		_directionHolder = new SnapshotObjectMember<Vector3>(this);
		_followTargetHolder = new SnapshotObjectMember<BattleCreature>(this);
		_touchedCreatureListHolder = new SnapshotObjectMember<SnapshotList<BattleCreature>>(this);
		_elapsedTimeMsHolder = new SnapshotObjectMember<long>(this, 0L);
		_nextTickTimeMsHolder = new SnapshotObjectMember<long>(this, 0L);
		_radiusMmHolder = new SnapshotObjectMember<int>(this, 0);
	}

	public void Init()
	{
		_touchedCreatureListHolder.Alloc();
	}
}
