using System;
using NGame2.NAccount;
using NGame2.NBattleContext;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow;

public class PurchaseDungeonRemainTimer : DungeonRemainTimerBase
{
	private PurchaseDungeonData _purchaseDungeonData;

	public PurchaseDungeonRemainTimer(PurchaseDungeonData purchaseDungeonData, IDungeonRemainTimer parent)
		: base(parent)
	{
		_purchaseDungeonData = purchaseDungeonData;
	}

	public override int GetRemainTimeDerived(EnterDungeonData enterDungeon)
	{
		int parentRemainTime = GetParentRemainTime(enterDungeon);
		if (parentRemainTime > 0)
		{
			return parentRemainTime;
		}
		return GetRemainedBoosterTime();
	}

	private int GetRemainedBoosterTime()
	{
		BoosterItemData data = GameRoot.Instance.BoosterItemDataContainer.GetData(_purchaseDungeonData.BoosterItemCode);
		if (data == null)
		{
			return 0;
		}
		PlayerItemTimeDurationInfo playerItemTimeDurationInfo = GlobalAutoBehaviour<AccountManager>.instance.ItemTimeDurationManager.Get(data.BoosterItemIndex);
		if (playerItemTimeDurationInfo == null)
		{
			return 0;
		}
		if (!Vespa_String_Extension_Method2.TryParseNetworkTime2(playerItemTimeDurationInfo.EndTime, out var o_dateTime))
		{
			return 0;
		}
		DateTime currentServerUTCTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime();
		return Math.Max((int)(o_dateTime - currentServerUTCTime).TotalSeconds, 0);
	}
}
