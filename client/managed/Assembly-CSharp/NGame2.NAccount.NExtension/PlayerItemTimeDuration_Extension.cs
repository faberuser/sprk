using System;
using NGame2.NUI.NWindow;
using NShared;

namespace NGame2.NAccount.NExtension;

public static class PlayerItemTimeDuration_Extension
{
	public static TimeSpan GetRemainTime(this PlayerItemTimeDurationInfo This, ItemData<BoosterItemData> itemData = null)
	{
		if (This == null)
		{
			return TimeSpan.Zero;
		}
		Vespa_String_Extension_Method2.TryParseNetworkTime2(This.EndTime, out var o_dateTime);
		return o_dateTime - Common.GetUTCNow();
	}
}
