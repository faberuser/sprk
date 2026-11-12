using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NBattleContext;
using NGame2.NDropBonusEvent;
using NShared;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class EventDungeonRemainTimer : DungeonRemainTimerBase
{
	private bool _showTimer = true;

	public EventDungeonRemainTimer(IDungeonRemainTimer parent)
		: base(parent)
	{
	}

	public void SetShowTimer(bool showTimer)
	{
		_showTimer = showTimer;
	}

	public override int GetRemainTimeDerived(EnterDungeonData enterDungeon)
	{
		if (!_showTimer)
		{
			return -1;
		}
		int parentRemainTime = GetParentRemainTime(enterDungeon);
		if (parentRemainTime > 0)
		{
			return parentRemainTime;
		}
		return GetRemainTime(enterDungeon);
	}

	private int GetRemainTime(EnterDungeonData enterDungeon)
	{
		DropBonusEvent dropBonusEvent = GetDropBonusEvent(enterDungeon);
		if (dropBonusEvent == null)
		{
			return 0;
		}
		if (String_Extension_Method.IsNullOrEmpty(dropBonusEvent.Condition))
		{
			return 0;
		}
		if (!int.TryParse(dropBonusEvent.Condition, out var result))
		{
			return 0;
		}
		if (result != enterDungeon.Chapter)
		{
			return 0;
		}
		if (Vespa_String_Extension_Method2.TryParseNetworkTime2(dropBonusEvent.EndTime, out var o_dateTime))
		{
			return Mathf.Max((int)(o_dateTime - GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime()).TotalSeconds, 0);
		}
		return 0;
	}

	private DropBonusEvent GetDropBonusEvent(EnterDungeonData enterDungeon)
	{
		List<DropBonusEvent> effectiveEvents = GlobalAutoBehaviour<DropBonusEventManager>.instance.GetEffectiveEvents(EventBonusType.EventDungeon);
		if (__Extension_Method_ICollection__.IsNullOrEmpty(effectiveEvents))
		{
			return null;
		}
		int result = 0;
		for (int i = 0; i < effectiveEvents.Count; i++)
		{
			DropBonusEvent dropBonusEvent = effectiveEvents[i];
			if (!String_Extension_Method.IsNullOrEmpty(dropBonusEvent.Condition) && int.TryParse(dropBonusEvent.Condition, out result) && result == enterDungeon.Chapter)
			{
				return dropBonusEvent;
			}
		}
		return null;
	}
}
