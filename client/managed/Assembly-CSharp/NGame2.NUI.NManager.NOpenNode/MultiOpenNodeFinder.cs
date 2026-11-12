using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUtil;
using NShared;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NUI.NManager.NOpenNode;

public class MultiOpenNodeFinder : OpenNodeFinderBase
{
	public override void Find(List<ReserveEffectComp> reserveEffectList, int chapterIndex, int nodeIndex, int startDepth)
	{
		Queue<Tuple<int, int, int>> queue = new Queue<Tuple<int, int, int>>();
		queue.Enqueue(new Tuple<int, int, int>(chapterIndex, nodeIndex, startDepth));
		while (queue.Count > 0)
		{
			Tuple<int, int, int> checkNode = queue.Dequeue();
			CampaignNodeData data = GameRoot.Instance.CampaignNodeDataContainer.GetData(checkNode.Item1, checkNode.Item2);
			if (data == null || __Extension_Method_ICollection__.COUNT(data.NextNodeIndex) == 0)
			{
				continue;
			}
			if (data.Function != NodeFunctionType.None)
			{
				GlobalAutoBehaviour<CampaignNodeNpcAvatarManager>.instance.AddToNPCInfoList(checkNode.Item1, checkNode.Item2, CampaignNodeNPCData.NPCAvatarState.Dead);
			}
			int[] nextNodeIndex = data.NextNodeIndex;
			foreach (int nextNodeIndex2 in nextNodeIndex)
			{
				CampaignNodeData data2 = GameRoot.Instance.CampaignNodeDataContainer.GetData(checkNode.Item1, nextNodeIndex2);
				if (!SingletonAuto<ConditionChecker>.instance.CheckNodeOpen(data2))
				{
					continue;
				}
				bool num = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.CanBypass(checkNode.Item1, checkNode.Item2);
				bool flag = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.CanVisit(checkNode.Item1, nextNodeIndex2);
				if (num && flag)
				{
					VespaDebug.LogError($"skip node open effect from {checkNode.Item2} to {nextNodeIndex2}");
					VespaDebug.LogError($"startNodeIndex {nodeIndex} depth{checkNode.Item3}");
					continue;
				}
				bool flag2 = CheckNextNodeAlreadyOpened(data2);
				ReserveEffectComp reserveEffectComp = reserveEffectList.Find((ReserveEffectComp e) => e.ChapterIndex == checkNode.Item1 && e.NodeIndex1 == checkNode.Item2 && e.NodeIndex2 == nextNodeIndex2);
				if (reserveEffectComp != null)
				{
					reserveEffectList.Remove(reserveEffectComp);
				}
				reserveEffectList.Add(new ReserveEffectComp(checkNode.Item1, checkNode.Item2, nextNodeIndex2, checkNode.Item3 + 1, flag2));
				if (data2.Function != NodeFunctionType.None)
				{
					GlobalAutoBehaviour<CampaignNodeNpcAvatarManager>.instance.AddToNPCInfoList(checkNode.Item1, nextNodeIndex2, CampaignNodeNPCData.NPCAvatarState.Appear);
				}
				if (data2.Function == NodeFunctionType.None && !flag2)
				{
					queue.Enqueue(new Tuple<int, int, int>(checkNode.Item1, nextNodeIndex2, checkNode.Item3 + 1));
				}
				else if (data2.Function == NodeFunctionType.MoveChapter && __Extension_Method_ICollection__.COUNT(data2.Value) >= 2)
				{
					Vespa_String_Extension_Method.TOINT(data2.Value[0], 0);
					CampaignChapterData data3 = GameRoot.Instance.CampaignChapterDataContainer.GetData(chapterIndex);
					if (data3 != null && data3.Type == ChapterType.WorldMap && data3.NodeOpenEffect)
					{
						queue.Enqueue(new Tuple<int, int, int>(Vespa_String_Extension_Method.TOINT(data2.Value[0], 0), Vespa_String_Extension_Method.TOINT(data2.Value[1], 0), 0));
					}
				}
			}
		}
	}
}
