using NShared;
using NShared.NCampaign;

namespace NGame2.NBattleThread.NThreadAction;

public class SetServerTime : IThreadAction
{
	public long ServerTimeMs;

	public bool IsWaiting { get; set; }

	public SetServerTime(long timeMs)
	{
		ServerTimeMs = timeMs;
	}

	void IThreadAction.Action(BattleInstance battleInstance, CampaignUpdater campaignUpdater)
	{
		battleInstance.SetServerTimeMs(ServerTimeMs);
	}
}
