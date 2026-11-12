using System.Collections.Generic;

namespace NShared;

public class BlackBoard : SnapshotObject
{
	private readonly SnapshotObjectMember<BattleCreature> _ownerHolder;

	private readonly SnapshotObjectMember<IndexableSnapshotDictionary<string, BlackBoardValueInfo>> _dictionaryHolder;

	public BlackBoard()
	{
		_ownerHolder = new SnapshotObjectMember<BattleCreature>(this);
		_dictionaryHolder = new SnapshotObjectMember<IndexableSnapshotDictionary<string, BlackBoardValueInfo>>(this);
	}

	public BlackBoard(BattleCreature creature)
		: this()
	{
		_ownerHolder.Set(creature);
		_dictionaryHolder.Set(new IndexableSnapshotDictionary<string, BlackBoardValueInfo>());
	}

	public void Update()
	{
		long currentTimeMs = _ownerHolder.Get().BattleInstance.MainTimer.GetCurrentTimeMs();
		List<string> list = new List<string>();
		for (int i = 0; i < _dictionaryHolder.Get().Count; i++)
		{
			string keyByIndex = _dictionaryHolder.Get().GetKeyByIndex(i);
			BlackBoardValueInfo valueByIndex = _dictionaryHolder.Get().GetValueByIndex(i);
			if (valueByIndex != null && valueByIndex.ExpireTime > 0 && currentTimeMs - valueByIndex.AddedTime > valueByIndex.ExpireTime)
			{
				list.Add(keyByIndex);
			}
		}
		foreach (string item in list)
		{
			_dictionaryHolder.Get().TryRemove(item, out var _);
		}
	}

	public void SetValue(string key, string value, int expireTime = 0)
	{
		long currentTimeMs = _ownerHolder.Get().BattleInstance.MainTimer.GetCurrentTimeMs();
		BlackBoardValueInfo value2 = new BlackBoardValueInfo
		{
			Value = value,
			AddedTime = currentTimeMs,
			ExpireTime = expireTime
		};
		_dictionaryHolder.Get().Remove(key);
		_dictionaryHolder.Get().Add(key, value2);
	}

	public BlackBoardValueInfo GetValueInfo(string key)
	{
		BlackBoardValueInfo value = null;
		if (_dictionaryHolder.Get().TryGetValue(key, out value))
		{
			return value;
		}
		return null;
	}

	public string GetValue(string key)
	{
		return GetValueInfo(key)?.Value;
	}
}
