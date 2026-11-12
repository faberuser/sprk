using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUtil;
using NShared;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NUI.NManager.NOpenNode;

public class SingleOpenNodeFinder : OpenNodeFinderBase
{
	public override void Find(List<ReserveEffectComp> reserveEffectList, int chapterIndex, int nodeIndex, int startDepth)
	{
		Stack<Tuple<int, int, int, int, bool>> stack = new Stack<Tuple<int, int, int, int, bool>>();
		stack.Push(new Tuple<int, int, int, int, bool>(chapterIndex, 0, nodeIndex, startDepth, i_item5: true));
		bool flag = false;
		while (stack.Count > 0)
		{
			Tuple<int, int, int, int, bool> checkNode = stack.Pop();
			CampaignNodeData data = GameRoot.Instance.CampaignNodeDataContainer.GetData(checkNode.Item1, checkNode.Item3);
			int num = __Extension_Method_ICollection__.COUNT(data.NextNodeIndex) - 1;
			while (num >= 0 && checkNode.Item5)
			{
				int num2 = data.NextNodeIndex[num];
				CampaignNodeData data2 = GameRoot.Instance.CampaignNodeDataContainer.GetData(checkNode.Item1, num2);
				if (SingletonAuto<ConditionChecker>.instance.CheckNodeOpen(data2))
				{
					bool num3 = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.CanBypass(checkNode.Item1, checkNode.Item3);
					bool flag2 = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.CanVisit(checkNode.Item1, num2);
					if (num3 && flag2)
					{
						VespaDebug.LogError($"skip node open effect from {checkNode.Item3} to {num2}");
						VespaDebug.LogError($"startNodeIndex {nodeIndex} depth{checkNode.Item4}");
					}
					else
					{
						flag = CheckNextNodeAlreadyOpened(data2);
						bool i_item = false;
						if (data2.Function == NodeFunctionType.None && !flag)
						{
							i_item = true;
						}
						else if (data2.Function == NodeFunctionType.MoveChapter && __Extension_Method_ICollection__.COUNT(data2.Value) >= 2)
						{
							Vespa_String_Extension_Method.TOINT(data2.Value[0], 0);
							CampaignChapterData data3 = GameRoot.Instance.CampaignChapterDataContainer.GetData(chapterIndex);
							if (data3 != null && data3.Type == ChapterType.WorldMap && data3.NodeOpenEffect)
							{
								i_item = true;
							}
						}
						stack.Push(new Tuple<int, int, int, int, bool>(checkNode.Item1, checkNode.Item3, num2, startDepth, i_item));
					}
				}
				num--;
			}
			if (checkNode.Item2 != 0)
			{
				flag = CheckNextNodeAlreadyOpened(data);
				ReserveEffectComp reserveEffectComp = reserveEffectList.Find((ReserveEffectComp e) => e.ChapterIndex == checkNode.Item1 && e.NodeIndex1 == checkNode.Item2 && e.NodeIndex2 == checkNode.Item3);
				if (reserveEffectComp != null)
				{
					reserveEffectList.Remove(reserveEffectComp);
				}
				startDepth++;
				reserveEffectList.Add(new ReserveEffectComp(checkNode.Item1, checkNode.Item2, checkNode.Item3, startDepth, flag));
				if (data.Function != NodeFunctionType.None)
				{
					GlobalAutoBehaviour<CampaignNodeNpcAvatarManager>.instance.AddToNPCInfoList(checkNode.Item1, checkNode.Item3, CampaignNodeNPCData.NPCAvatarState.Appear);
				}
			}
		}
	}
}
