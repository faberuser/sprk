using NShared.NUtil;

namespace NShared;

public class MoveHandle : SnapshotObject
{
	public static Vector3[] MoveTargetOffsets = new Vector3[7]
	{
		new Vector3(0L, 0L, 0L),
		new Vector3(0L, 0L, 1000L),
		new Vector3(0L, 0L, -1000L),
		new Vector3(-1000L, 0L, 1000L).normalized,
		new Vector3(-1000L, 0L, -1000L).normalized,
		new Vector3(1000L, 0L, 1000L).normalized,
		new Vector3(1000L, 0L, -1000L).normalized
	};

	private readonly SnapshotObjectMember<MoveTargetType> _moveTargetTypeHolder;

	private readonly SnapshotObjectMember<long> _startedTimeMsHolder;

	private readonly SnapshotObjectMember<Vector3> _startPositionHolder;

	private readonly SnapshotObjectMember<Vector3> _targetPositionHolder;

	private readonly SnapshotObjectMember<BattleCreature> _targetCreatureHolder;

	private readonly SnapshotObjectMember<int> _approachDistanceMmHolder;

	private readonly SnapshotObjectMember<int> _speedMmsHolder;

	private readonly SnapshotObjectMember<int> _moveTargetOffsetIndexHolder;

	private readonly SnapshotObjectMember<bool> _isManualMove;

	public MoveTargetType MoveTargetType => _moveTargetTypeHolder.Get();

	public long StartedTimeMs => _startedTimeMsHolder.Get();

	public Vector3 StartPosition => _startPositionHolder.Get();

	public Vector3 TargetPosition => _targetPositionHolder.Get();

	public BattleCreature TargetCreature => _targetCreatureHolder.Get();

	public int ApproachDistanceMm => _approachDistanceMmHolder.Get();

	public int SpeedMms => _speedMmsHolder.Get();

	public int MoveTargetOffsetIndex => _moveTargetOffsetIndexHolder.Get();

	public bool IsManualMove => _isManualMove.Get();

	public MoveHandle()
	{
		_moveTargetTypeHolder = new SnapshotObjectMember<MoveTargetType>(this, MoveTargetType.Position);
		_startedTimeMsHolder = new SnapshotObjectMember<long>(this, 0L);
		_startPositionHolder = new SnapshotObjectMember<Vector3>(this);
		_targetPositionHolder = new SnapshotObjectMember<Vector3>(this);
		_targetCreatureHolder = new SnapshotObjectMember<BattleCreature>(this);
		_approachDistanceMmHolder = new SnapshotObjectMember<int>(this, 0);
		_speedMmsHolder = new SnapshotObjectMember<int>(this, 0);
		_moveTargetOffsetIndexHolder = new SnapshotObjectMember<int>(this, 0);
		_isManualMove = new SnapshotObjectMember<bool>(this, value: false);
	}

	public void Init(long startedTimeMs, Vector3 startPosition, Vector3 targetPosition, int approachDistanceMm, bool isManualMove)
	{
		_moveTargetTypeHolder.Set(MoveTargetType.Position);
		_targetPositionHolder.Set(targetPosition);
		_targetCreatureHolder.Set(null);
		InitDefault(startedTimeMs, startPosition, approachDistanceMm, isManualMove);
	}

	public void Init(long startedTimeMs, Vector3 startPosition, BattleCreature targetCreature, int approachDistanceMm, bool isManualMove)
	{
		_moveTargetTypeHolder.Set(MoveTargetType.Creature);
		_targetPositionHolder.Set(Vector3.zero);
		_targetCreatureHolder.Set(targetCreature);
		InitDefault(startedTimeMs, startPosition, approachDistanceMm, isManualMove);
	}

	private void InitDefault(long startedTimeMs, Vector3 startPosition, int approachDistanceMm, bool isManualMove)
	{
		_startedTimeMsHolder.Set(startedTimeMs);
		_startPositionHolder.Set(startPosition);
		_approachDistanceMmHolder.Set(approachDistanceMm);
		_isManualMove.Set(isManualMove);
		_speedMmsHolder.Set(0);
		_moveTargetOffsetIndexHolder.Set(0);
	}

	public void SetMoveSpeed(int speedMms)
	{
		_speedMmsHolder.Set(speedMms);
	}

	public void SetAvailableMoveTargetOffset(BattleCreature creature, BattleCreature targetCreature)
	{
		if (creature == null)
		{
		}
	}
}
