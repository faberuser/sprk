using NShared;
using NShared.NCampaign;

namespace NGame2.NBattleThread;

public interface IThreadAction
{
	bool IsWaiting { get; set; }

	void Action(BattleInstance battleInstance, CampaignUpdater campaignUpdater);
}
