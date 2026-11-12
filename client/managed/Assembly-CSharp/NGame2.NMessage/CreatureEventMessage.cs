using System.Collections.Generic;
using NGame2.NNodeTree;
using NVespa;

namespace NGame2.NMessage;

public class CreatureEventMessage : SmallPooling<CreatureEventMessage>, ISmallPoolingItem, IMessage
{
	public enum EEventType
	{
		Spawn,
		BossIntro,
		BattleStart,
		BattleEnd,
		Dead,
		LastKill,
		BossNameTag,
		EliteNameTag,
		BossStatus_Show
	}

	private List<IRuntime> _runtimeList;

	public bool IsDummy { get; set; }

	public int CreatureUid { get; private set; }

	public EEventType EventType { get; private set; }

	public PackData NodeTreePack { get; private set; }

	public string Keyword { get; private set; }

	public ILocalTimer EventTimer { get; private set; }

	void ISmallPoolingItem.OnCreateNew()
	{
	}

	void ISmallPoolingItem.OnDestroy()
	{
	}

	void ISmallPoolingItem.OnCreate()
	{
	}

	void ISmallPoolingItem.OnRelease()
	{
		if (_runtimeList != null)
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_runtimeList); i++)
			{
				_runtimeList[i].ReleaseSelf();
			}
			_runtimeList.Clear();
		}
	}

	void IMessage.ReleaseSelf()
	{
		SmallPooling<CreatureEventMessage>.ReleaseInstance(this);
	}

	public void Open(int creatureUid, EEventType eventType, PackData pack, string keyword, ILocalTimer eventTimer = null)
	{
		CreatureUid = creatureUid;
		EventType = eventType;
		NodeTreePack = pack;
		Keyword = keyword;
		EventTimer = eventTimer;
	}

	public void AddRuntime(IRuntime runtime)
	{
		if (_runtimeList == null)
		{
			_runtimeList = new List<IRuntime>();
		}
		_runtimeList.Add(runtime);
	}

	public List<IRuntime> PopRuntimeList()
	{
		List<IRuntime> runtimeList = _runtimeList;
		_runtimeList = null;
		return runtimeList;
	}
}
