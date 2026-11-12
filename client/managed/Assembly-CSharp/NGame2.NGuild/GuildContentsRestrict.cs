using System;
using NGame2.NAccount;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NGuild;

public class GuildContentsRestrict : IGuildPenalty
{
	public GuildPenaltyType GetPenaltyType()
	{
		return GuildPenaltyType.ContentsRestricted;
	}

	public bool OnPenalty()
	{
		DateTime guildContentsRestrictedTime = GlobalAutoBehaviour<AccountManager>.instance.GuildContentsRestrictedTime;
		if (guildContentsRestrictedTime.CompareTo(DateTime.MinValue) != 0 && (int)(GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime() - guildContentsRestrictedTime).TotalMinutes < GetDurationMin())
		{
			return true;
		}
		return false;
	}

	public DateTime EndTime()
	{
		DateTime guildContentsRestrictedTime = GlobalAutoBehaviour<AccountManager>.instance.GuildContentsRestrictedTime;
		if (guildContentsRestrictedTime.CompareTo(DateTime.MinValue) == 0)
		{
			return DateTime.MinValue;
		}
		return guildContentsRestrictedTime.AddMinutes(GetDurationMin());
	}

	private int GetDurationMin()
	{
		return GameRoot.Instance.ConstantDataContainer.GetValueInt("GuildContentsRestrictMin", 10080);
	}
}
