using System;
using NGame2.NAccount;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NGuild;

public class GuildWithdraw : IGuildPenalty
{
	public GuildPenaltyType GetPenaltyType()
	{
		return GuildPenaltyType.WithdrawPenalty;
	}

	public bool OnPenalty()
	{
		string guildWithdrawTime = GlobalAutoBehaviour<AccountManager>.instance.UserManager.MiscInfo.GuildWithdrawTime;
		if (String_Extension_Method.IsNullOrEmpty(guildWithdrawTime))
		{
			return false;
		}
		if (!Vespa_String_Extension_Method2.TryParseNetworkTime2(guildWithdrawTime, out var o_dateTime))
		{
			return false;
		}
		if ((int)(GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime() - o_dateTime).TotalMinutes < GetDurationMin())
		{
			return true;
		}
		return false;
	}

	public DateTime EndTime()
	{
		if (!Vespa_String_Extension_Method2.TryParseNetworkTime2(GlobalAutoBehaviour<AccountManager>.instance.UserManager.MiscInfo.GuildWithdrawTime, out var o_dateTime))
		{
			return DateTime.MinValue;
		}
		return o_dateTime.AddMinutes(GetDurationMin());
	}

	private int GetDurationMin()
	{
		int guildWithdrawCount = GlobalAutoBehaviour<AccountManager>.instance.UserManager.MiscInfo.GuildWithdrawCount;
		return GameRoot.Instance.GuildPenaltyDataContainer.GetPanaltyData(guildWithdrawCount)?.RestrictTimeMin ?? 0;
	}
}
