namespace NShared;

public class BattleCreatureRestingInfo : SnapshotObject
{
	private readonly SnapshotObjectMember<int> _wakeUpWaveIndexHolder;

	private readonly SnapshotObjectMember<int> _wakeUpWaveTimeMsHolder;

	private readonly SnapshotObjectMember<bool> _wakeUpFromAllyHolder;

	private readonly SnapshotObjectMember<WakeUpPivotType> _wakeUpPivotTypeHolder;

	private readonly SnapshotObjectMember<int> _wakeUpOffsetHolder;

	public int WakeUpWaveIndex => _wakeUpWaveIndexHolder.Get();

	public int WakeUpWaveTimeMs => _wakeUpWaveTimeMsHolder.Get();

	public bool WakeUpFromAlly => _wakeUpFromAllyHolder.Get();

	public WakeUpPivotType WakeUpPivotType => _wakeUpPivotTypeHolder.Get();

	public int WakeUpOffset => _wakeUpOffsetHolder.Get();

	public BattleCreatureRestingInfo()
	{
		_wakeUpWaveIndexHolder = new SnapshotObjectMember<int>(this, 0);
		_wakeUpWaveTimeMsHolder = new SnapshotObjectMember<int>(this, 0);
		_wakeUpFromAllyHolder = new SnapshotObjectMember<bool>(this, value: false);
		_wakeUpPivotTypeHolder = new SnapshotObjectMember<WakeUpPivotType>(this, WakeUpPivotType.Center);
		_wakeUpOffsetHolder = new SnapshotObjectMember<int>(this, 0);
	}

	public void Init(int wakeUpWaveIndex, int wakeUpWaveTimeMs, bool wakeUpFomAlly, WakeUpPivotType wakeUpPivotType, int wakeUpOffset)
	{
		_wakeUpWaveIndexHolder.Set(wakeUpWaveIndex);
		_wakeUpWaveTimeMsHolder.Set(wakeUpWaveTimeMs);
		_wakeUpFromAllyHolder.Set(wakeUpFomAlly);
		_wakeUpPivotTypeHolder.Set(wakeUpPivotType);
		_wakeUpOffsetHolder.Set(wakeUpOffset);
	}
}
