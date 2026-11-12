using System;
using System.Collections;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NScene;
using NGame2.NUI.NManager.NOpenNode;
using NGame2.NUI.NWindow.NChatting;
using NGame2.NUI.NWorldMap;
using NGame2.NUI.NWorldMap.NMapLayout;
using NShared;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NManager;

public class CampaignNodeOpenEffectManager : GlobalAutoBehaviour<CampaignNodeOpenEffectManager>
{
	private List<ReserveEffectComp> ReserveEffectList = new List<ReserveEffectComp>();

	private bool _focusReleaseReserved;

	private MapFocusHolder _mapFocusHolder;

	public bool IsPlaying { get; private set; }

	public bool IsFocus { get; private set; }

	public int NewClearNodeChapterIndex { get; private set; }

	public int NewClearNodeDungeonIndex { get; private set; }

	public bool IsFocusing()
	{
		if (IsPlaying)
		{
			return IsFocus;
		}
		return false;
	}

	public void ReleaseFocus()
	{
		_focusReleaseReserved = _mapFocusHolder == null;
		if (_mapFocusHolder != null)
		{
			IsFocus = false;
			_mapFocusHolder.Dispose();
		}
	}

	private void Update()
	{
		if (AchievementClearedPopupQueue.IsProgressing())
		{
			ReleaseFocus();
		}
	}

	public void CheckNewClearNode(int ChapterIdx, int DungeonIdx)
	{
		_focusReleaseReserved = false;
		IsPlaying = false;
		IsFocus = false;
		NewClearNodeChapterIndex = 0;
		NewClearNodeDungeonIndex = 0;
		CampaignChapterData data = GameRoot.Instance.CampaignChapterDataContainer.GetData(ChapterIdx);
		if (data == null || data.Type != ChapterType.WorldMap || !data.NodeOpenEffect || GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.IsCompleted(ChapterIdx, DungeonIdx))
		{
			return;
		}
		List<ReserveEffectComp> list = new List<ReserveEffectComp>();
		foreach (ReserveEffectComp reserveEffect in ReserveEffectList)
		{
			if (reserveEffect.ChapterIndex <= ChapterIdx)
			{
				list.Add(reserveEffect);
			}
		}
		foreach (ReserveEffectComp item in list)
		{
			ReserveEffectList.Remove(item);
		}
		list.Clear();
		CampaignNodeData data2 = GameRoot.Instance.CampaignNodeDataContainer.GetData(ChapterIdx, DungeonIdx);
		if (data2 != null)
		{
			NewClearNodeChapterIndex = ChapterIdx;
			NewClearNodeDungeonIndex = DungeonIdx;
			NGame2.NUI.NManager.NOpenNode.IOpenNodeFinder nodeFinder = GetNodeFinder(data2);
			nodeFinder.Find(ReserveEffectList, ChapterIdx, DungeonIdx, 0);
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(data2.AddDirectionNode); i++)
			{
				int nodeIndex = data2.AddDirectionNode[i];
				nodeFinder.Find(ReserveEffectList, ChapterIdx, nodeIndex, GetReserveMaxDepth() + 1);
			}
		}
	}

	private NGame2.NUI.NManager.NOpenNode.IOpenNodeFinder GetNodeFinder(CampaignNodeData nodeData)
	{
		if (nodeData.NodeOpenStyle != CampaignNodeData.ENodeOpenStyle.Multi)
		{
			return new SingleOpenNodeFinder();
		}
		return new MultiOpenNodeFinder();
	}

	private int GetReserveMaxDepth()
	{
		int maxDepth = 0;
		ReserveEffectList.ForEach(delegate(ReserveEffectComp comp)
		{
			if (maxDepth < comp.Depth)
			{
				maxDepth = comp.Depth;
			}
		});
		return maxDepth;
	}

	public bool CheckNodeReserved(int chapterIndex, int dungeonIndex)
	{
		foreach (ReserveEffectComp reserveEffect in ReserveEffectList)
		{
			if (reserveEffect.ChapterIndex == chapterIndex && reserveEffect.NodeIndex2 == dungeonIndex && !reserveEffect.LastNodeAlreadyOpened)
			{
				return true;
			}
		}
		return false;
	}

	public bool CheckEdgeReserved(int chapterIndex, int dungeonIndex1, int dungeonIndex2)
	{
		foreach (ReserveEffectComp reserveEffect in ReserveEffectList)
		{
			if (reserveEffect.ChapterIndex == chapterIndex)
			{
				if (reserveEffect.NodeIndex1 == dungeonIndex1 && reserveEffect.NodeIndex2 == dungeonIndex2)
				{
					return true;
				}
				if (reserveEffect.NodeIndex1 == dungeonIndex2 && reserveEffect.NodeIndex2 == dungeonIndex1)
				{
					return true;
				}
			}
		}
		return false;
	}

	private NodeGraph GetNodeGraph()
	{
		if (!SceneAutoBehaviour<WorldMapManagement>.isValidInstance)
		{
			return null;
		}
		MapLayout mapLayout = SceneAutoBehaviour<WorldMapManagement>.instance.GetInstance<MapLayout>();
		if (mapLayout == null)
		{
			return null;
		}
		return mapLayout.Mediator.NodeGraph;
	}

	private List<Tuple<int, int, bool>> GetTargetList(int depth)
	{
		List<Tuple<int, int, bool>> list = new List<Tuple<int, int, bool>>();
		List<ReserveEffectComp> list2 = new List<ReserveEffectComp>();
		foreach (ReserveEffectComp reserveEffect in ReserveEffectList)
		{
			if (reserveEffect.ChapterIndex == NGame2.NAccount.UserCampaignManager.ChapterIndex && reserveEffect.Depth == depth)
			{
				list2.Add(reserveEffect);
				list.Add(new Tuple<int, int, bool>(reserveEffect.NodeIndex1, reserveEffect.NodeIndex2, reserveEffect.LastNodeAlreadyOpened));
			}
		}
		foreach (ReserveEffectComp item in list2)
		{
			ReserveEffectList.Remove(item);
		}
		list2.Clear();
		return list;
	}

	private IEnumerator coPlayEffectOneDepth(List<Tuple<int, int, bool>> targetList, MapFocusHolder mapFocusHolder, Action onComplete)
	{
		if (targetList.Count == 0)
		{
			Vespa_System_Action_Extension_Method.fire(onComplete);
			yield break;
		}
		bool executeEnd = false;
		mapFocusHolder.Focus(targetList, delegate
		{
			executeEnd = true;
		});
		while (!executeEnd)
		{
			yield return null;
		}
		NodeGraph nodeGraph = GetNodeGraph();
		if (nodeGraph == null)
		{
			yield break;
		}
		List<MapEdge2> list = new List<MapEdge2>();
		float num = 0f;
		foreach (Tuple<int, int, bool> target in targetList)
		{
			MapEdge2 edgeByDungeonIndex = nodeGraph.GetEdgeByDungeonIndex(target.Item1, target.Item2);
			if (edgeByDungeonIndex == null)
			{
				Debug.LogError("Edge not found by DungeonIndex: (" + target.Item1 + ", " + target.Item2 + ")");
				yield break;
			}
			if (edgeByDungeonIndex.GetNextEffectStartTime() > num)
			{
				num = edgeByDungeonIndex.GetNextEffectStartTime();
			}
			list.Add(edgeByDungeonIndex);
			VespaCoroutine.Start(edgeByDungeonIndex.coPlayEffect());
		}
		yield return new WaitForSeconds(num);
		List<MapNode> list2 = new List<MapNode>();
		num = 0f;
		foreach (Tuple<int, int, bool> target2 in targetList)
		{
			if (!target2.Item3)
			{
				MapNode nodeByDungeonIndex = nodeGraph.GetNodeByDungeonIndex(target2.Item2);
				if (nodeByDungeonIndex == null)
				{
					Debug.LogError("Node not found by DungeonIndex: " + target2.Item2);
					yield break;
				}
				if (nodeByDungeonIndex.GetNextEffectStartTime() > num)
				{
					num = nodeByDungeonIndex.GetNextEffectStartTime();
				}
				list2.Add(nodeByDungeonIndex);
				VespaCoroutine.Start(nodeByDungeonIndex.coPlayEffect());
			}
		}
		yield return new WaitForSeconds(num);
		Vespa_System_Action_Extension_Method.fire(onComplete);
	}

	public IEnumerator coPlayEffect(Action onStart, Action onEnd)
	{
		while (GlobalAutoBehaviour<LoadingManager>.instance.IsSceneLoading)
		{
			yield return null;
		}
		ChattingWindow chattingWindow = SceneAutoBehaviour<ChattingManagement>.instance.GetInstance<ChattingWindow>();
		if (chattingWindow != null && chattingWindow.IsOpen())
		{
			while (chattingWindow.IsOpen())
			{
				yield return null;
			}
		}
		int maxDepth = 0;
		int chapterIndex = NGame2.NAccount.UserCampaignManager.ChapterIndex;
		foreach (ReserveEffectComp reserveEffect in ReserveEffectList)
		{
			if (reserveEffect.ChapterIndex == chapterIndex && reserveEffect.Depth > maxDepth)
			{
				maxDepth = reserveEffect.Depth;
			}
		}
		Vespa_System_Action_Extension_Method.fire(onStart);
		if (IsFocus && _focusReleaseReserved)
		{
			IsFocus = false;
		}
		using (_mapFocusHolder = new MapFocusHolder(_focusReleaseReserved))
		{
			int curDepth = 1;
			while (curDepth <= maxDepth)
			{
				List<Tuple<int, int, bool>> targetList = GetTargetList(curDepth);
				yield return VespaCoroutine.Start(coPlayEffectOneDepth(targetList, _mapFocusHolder, null));
				int num = curDepth + 1;
				curDepth = num;
			}
		}
		_mapFocusHolder = null;
		Vespa_System_Action_Extension_Method.fire(onEnd);
	}

	public void StartEffect()
	{
		if (ReserveEffectList.Count == 0)
		{
			IsPlaying = false;
			IsFocus = false;
		}
		else
		{
			if (IsPlaying)
			{
				return;
			}
			IsPlaying = true;
			IsFocus = ReserveEffectList.Exists(delegate(ReserveEffectComp comp)
			{
				if (comp.ChapterIndex != NGame2.NAccount.UserCampaignManager.ChapterIndex)
				{
					return false;
				}
				CampaignNodeData data = GameRoot.Instance.CampaignNodeDataContainer.GetData(comp.ChapterIndex, comp.NodeIndex2);
				return data != null && !__Extension_Method_ICollection__.IsNullOrEmpty(data.NodeOpenValues);
			});
			VespaCoroutine.Start(coPlayEffect(delegate
			{
				GlobalAutoBehaviour<CampaignNodeNpcAvatarManager>.instance.UpdateNpcState(CampaignNodeNPCData.NPCAvatarState.Dead);
			}, delegate
			{
				IsPlaying = false;
				IsFocus = false;
				GlobalAutoBehaviour<CampaignNodeNpcAvatarManager>.instance.UpdateNpcState(CampaignNodeNPCData.NPCAvatarState.Appear, clearList: true);
			}));
		}
	}

	public bool IsNewClearNode(int chapterIndex, int dungeonIndex)
	{
		if (NewClearNodeChapterIndex == 0 || NewClearNodeDungeonIndex == 0)
		{
			return false;
		}
		if (NewClearNodeChapterIndex == chapterIndex)
		{
			return NewClearNodeDungeonIndex == dungeonIndex;
		}
		return false;
	}
}
