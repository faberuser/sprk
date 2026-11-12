using System.Collections.Generic;
using NVespa;

namespace NShared;

public class BattleSnapshotManager
{
	public const int DefaultSnapshotIntervalMs = 1000;

	public const int DefaultMaxSnapshotCount = 180;

	private readonly BattleInstance _battleInstance;

	private readonly List<BattleSnapshotInfo> _snapshotList;

	private int _snapshotIntervalMs;

	private int _maxSnapshotCount;

	private long _snapshotTimeMs;

	private bool _snapshotReserved;

	public BattleSnapshotManager(BattleInstance battleInstance)
	{
		_battleInstance = battleInstance;
		_snapshotList = new List<BattleSnapshotInfo>();
		_snapshotIntervalMs = 1000;
		_maxSnapshotCount = 180;
		_snapshotTimeMs = 0L;
		_snapshotReserved = false;
	}

	public void SetSnapshotInterval(int intervalMs)
	{
		_snapshotIntervalMs = intervalMs;
	}

	public void SetMaxSnapshotCount(int maxCount)
	{
		_maxSnapshotCount = maxCount;
	}

	public void Update(int deltaTimeMs)
	{
		_snapshotTimeMs += deltaTimeMs;
		if (_snapshotTimeMs >= _snapshotIntervalMs || _snapshotList.Count == 0 || _snapshotReserved)
		{
			AddSnapshot();
		}
		while (_snapshotList.Count > _maxSnapshotCount)
		{
			_snapshotList.RemoveAt(0);
		}
	}

	public void ReserveAddSnapshot()
	{
		_snapshotReserved = true;
	}

	public void AddSnapshot()
	{
		long currentTimeMs = _battleInstance.GlobalTimer.GetCurrentTimeMs();
		long currentTimeMs2 = _battleInstance.MainTimer.GetCurrentTimeMs();
		_ = _battleInstance.CreatureContainer;
		SerializedSnapshotContainer serializedSnapshotContainer = new SerializedSnapshotContainer();
		_battleInstance.CreatureContainer.Serialize(serializedSnapshotContainer);
		_battleInstance.Snapshot.Serialize(serializedSnapshotContainer);
		_snapshotList.Add(new BattleSnapshotInfo
		{
			GlobalTimeMs = currentTimeMs,
			MainTimeMs = currentTimeMs2,
			SnapshotContainer = serializedSnapshotContainer
		});
		_snapshotTimeMs = 0L;
		_snapshotReserved = false;
	}

	public BattleSnapshotInfo CreateSnapshotForTest()
	{
		AddSnapshot();
		if (_snapshotList.Count <= 0)
		{
			return null;
		}
		return _snapshotList[_snapshotList.Count - 1];
	}

	public BattleSnapshotInfo GetSnapShot(long targetTimeMS)
	{
		for (int i = 0; i < _snapshotList.Count; i++)
		{
			BattleSnapshotInfo battleSnapshotInfo = _snapshotList[i];
			BattleSnapshotInfo battleSnapshotInfo2 = ((i + 1 < _snapshotList.Count) ? _snapshotList[i + 1] : null);
			if (targetTimeMS >= battleSnapshotInfo.GlobalTimeMs && (battleSnapshotInfo2 == null || targetTimeMS < battleSnapshotInfo2.GlobalTimeMs))
			{
				return battleSnapshotInfo;
			}
		}
		VespaDebug.LogError("Failed to restore Snapshot : restoreTimeMs=" + targetTimeMS);
		return null;
	}

	public BattleSnapshotInfo GetLatestSnapShot()
	{
		if (_snapshotList == null || _snapshotList.Count == 0)
		{
			return null;
		}
		return _snapshotList[_snapshotList.Count - 1];
	}

	public BattleSnapshotInfo RestoreSnapshot(long restoreTimeMs)
	{
		BattleSnapshotInfo battleSnapshotInfo = null;
		int num = 0;
		for (int i = 0; i < _snapshotList.Count; i++)
		{
			BattleSnapshotInfo battleSnapshotInfo2 = _snapshotList[i];
			BattleSnapshotInfo battleSnapshotInfo3 = ((i + 1 < _snapshotList.Count) ? _snapshotList[i + 1] : null);
			if (restoreTimeMs >= battleSnapshotInfo2.GlobalTimeMs && (battleSnapshotInfo3 == null || restoreTimeMs < battleSnapshotInfo3.GlobalTimeMs))
			{
				battleSnapshotInfo = battleSnapshotInfo2;
				num = i;
				break;
			}
		}
		if (battleSnapshotInfo == null)
		{
			VespaDebug.LogError("Failed to restore Snapshot : restoreTimeMs=" + restoreTimeMs);
			return null;
		}
		_battleInstance.TraceBattleEvent(false, "ROLLBACK_BEGIN", "TIME=" + battleSnapshotInfo.GlobalTimeMs);
		_snapshotList.RemoveRange(num + 1, _snapshotList.Count - num - 1);
		SnapshotObjectPoolContainer.Reset();
		battleSnapshotInfo.SnapshotContainer.Deserialize();
		_battleInstance.GlobalTimer.Set(battleSnapshotInfo.GlobalTimeMs);
		_battleInstance.MainTimer.Set(battleSnapshotInfo.MainTimeMs);
		int num2 = 1000;
		while (true)
		{
			if (--num2 <= 0)
			{
				VespaDebug.LogError("TOO MANY UPDATE LOOP TO RESTORE!");
				break;
			}
			if (_battleInstance.GlobalTimer.GetCurrentTimeMs() >= restoreTimeMs)
			{
				break;
			}
			_battleInstance.UpdateTick(snapshotEnabled: false);
		}
		_battleInstance.TraceBattleEvent(false, "ROLLBACK_END", "TIME=" + _battleInstance.GlobalTimer.GetCurrentTimeMs());
		_snapshotTimeMs = 0L;
		if (_snapshotList.Count == 0)
		{
			return null;
		}
		return _snapshotList[_snapshotList.Count - 1];
	}
}
