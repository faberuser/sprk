using System.Collections.Generic;
using NGame2.NAccount;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NUI.NManager.NOpenNode;

public abstract class OpenNodeFinderBase : NGame2.NUI.NManager.NOpenNode.IOpenNodeFinder
{
	public abstract void Find(List<ReserveEffectComp> reserveEffectList, int chapterIndex, int nodeIndex, int startDepth);

	protected bool CheckNextNodeAlreadyOpened(CampaignNodeData nodeData)
	{
		if (GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.IsCompleted(nodeData.ChapterIndex, nodeData.DungeonIndex))
		{
			return true;
		}
		if (nodeData.Function == NodeFunctionType.None)
		{
			return GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.CanVisit(nodeData.ChapterIndex, nodeData.DungeonIndex);
		}
		return false;
	}
}
