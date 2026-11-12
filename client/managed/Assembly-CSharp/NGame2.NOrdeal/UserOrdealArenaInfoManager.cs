using System;
using System.Collections.Generic;
using System.Linq;
using NGame2.NAccount;
using NShared;
using NShared.EndOrdealArena;
using NShared.OrdealInfo;
using NShared.RefreshNode;
using NShared.ResurrectionHero;
using NShared.SelectBuff;
using NShared.SelectTier;
using NVespa.NSingleton;

namespace NGame2.NOrdeal;

public class UserOrdealArenaInfoManager
{
	private Dictionary<int, OrdealNodeInfo> _dicTotalNodeInfo = new Dictionary<int, OrdealNodeInfo>();

	private HashSet<int> _hashClearNodeIndex = new HashSet<int>();

	private int _completeFoor;

	private HashSet<int> _hashDeadHeroIndex = new HashSet<int>();

	private List<int> _listSelectableBuffIndex = new List<int>();

	private List<int> _listSelectedBuffIndex = new List<int>();

	private int _remainRefreshCount;

	private MatchTierType _selectedTier;

	private int _matchScore;

	private int _lastClearDungeonIndex;

	private bool _isCompleteOrdeal;

	public UserObserverManager<OrdealArenaInfoChanged> _observerManager = new UserObserverManager<OrdealArenaInfoChanged>();

	public void AddObserver(IUserObserver observer)
	{
		_observerManager.AddObserver(observer);
	}

	public void RemoveObserver(IUserObserver observer)
	{
		_observerManager.RemoveObserver(observer);
	}

	public void NoticeObserver(OrdealArenaInfoChanged changedInfo)
	{
		if (changedInfo != null && _observerManager != null)
		{
			_observerManager.AnnounceToObservers(changedInfo);
		}
	}

	public void Set(NShared.OrdealInfo.Response res)
	{
		SetSelectedMatchTierType(res.SelectedTier);
		SetTotalNodeInfo(res.OrdealNodeInfos);
		SetClearNodeIndex(res.ClearNodeIndices, isClear: true);
		SetDeadHeroIndices(res.DeadHeroIndices);
		SetSelectableBuffIndices(res.SelectableBuffIndices);
		SetSelectedBuffIndices(res.SelectedBuffIndices);
		SetRemainRefrshCount(res.RemainRefreshCount);
		SetMatchScore(res.MatchScore);
	}

	public void Apply(NShared.EndOrdealArena.Response res)
	{
		GlobalAutoBehaviour<AccountManager>.instance.ApplyRewardResult(res.AreaRewardInfo);
		GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(res.CurrencyResult);
		SetMatchScore(res.ResultMatchScore);
		SetDeadHeroIndices(res.DeadHeroIndices);
		SetSelectableBuffIndices(res.SelectableBuffIndices);
		SetClearNodeIndex(res.ClearNodeIndices);
	}

	public void Apply(NShared.RefreshNode.Response res)
	{
		ApplyTotalNodeInfo(res.OrdealNodeInfos);
		SetRemainRefrshCount(res.RemainRefreshCount);
		GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(res.CurrencyResults);
	}

	public void SetMatchScore(int score)
	{
		if (score != 0)
		{
			_matchScore = score;
		}
	}

	public void Apply(NShared.ResurrectionHero.Response res)
	{
		GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(res.CurrencyResults);
		SetDeadHeroIndices(res.DeadHeroIndices);
	}

	public void Apply(NShared.SelectBuff.Response res)
	{
		ResetSelectableBuffList();
		SetSelectedBuffIndices(res.SelectedBuffIndices);
	}

	public void Apply(NShared.SelectTier.Response res)
	{
		SetSelectedMatchTierType(res.SelectedTier);
		SetTotalNodeInfo(res.OrdealNodeInfos);
		SetClearNodeIndex(res.ClearNodeIndices, isClear: true);
		SetDeadHeroIndices(res.DeadHeroIndices);
		SetSelectableBuffIndices(res.SelectableBuffIndices);
		SetSelectedBuffIndices(res.SelectedBuffIndices);
		SetRemainRefrshCount(res.RemainRefreshCount);
		SetMatchScore(res.MatchScore);
	}

	private void SetTotalNodeInfo(OrdealNodeInfo[] nodeInfos)
	{
		SetNodeInfos(_dicTotalNodeInfo, nodeInfos);
	}

	private void ApplyTotalNodeInfo(OrdealNodeInfo nodeInfo)
	{
		ApplyNodeInfo(_dicTotalNodeInfo, nodeInfo);
	}

	public void SetClearNodeIndex(int[] nodeInidices, bool isClear = false)
	{
		if (isClear)
		{
			_hashClearNodeIndex.Clear();
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(nodeInidices); i++)
		{
			int clearNodeIndex = nodeInidices[i];
			SetClearNodeIndex(clearNodeIndex);
		}
	}

	private void SetClearNodeIndex(int clearNodeIndex)
	{
		if (!_hashClearNodeIndex.Contains(clearNodeIndex))
		{
			_hashClearNodeIndex.Add(clearNodeIndex);
			OrdealNodeInfo ordealArenaNodeInfo = GetOrdealArenaNodeInfo(clearNodeIndex);
			SetCompleteFloor(ordealArenaNodeInfo);
		}
	}

	public void SetCompleNodeInfo(OrdealNodeInfo nodeInfo)
	{
		if (nodeInfo != null)
		{
			SetClearNodeIndex(nodeInfo.Index);
		}
	}

	public void SetCompleteFloor(OrdealNodeInfo nodeInfo)
	{
		if (nodeInfo != null && nodeInfo.Floor > _completeFoor)
		{
			_completeFoor = nodeInfo.Floor;
			OrdealArenaNodeData dataByIndex = GameRoot.Instance.OrdealArenaNodeDataContainer.GetDataByIndex(nodeInfo.Index);
			if (dataByIndex != null)
			{
				_lastClearDungeonIndex = dataByIndex.DungeonIndex;
			}
			if (dataByIndex.NodeType == EOrdealArenaNodeType.End)
			{
				_isCompleteOrdeal = true;
			}
			NoticeObserver(new OrdealArenaInfoChanged
			{
				Type = EOrdealArenaInfoChangeType.CompleteFloor,
				ChangedNodeInfo = nodeInfo
			});
		}
	}

	private void SetDeadHeroIndices(int[] deadHeroIndices)
	{
		_hashDeadHeroIndex.Clear();
		int num = __Extension_Method_ICollection__.COUNT(deadHeroIndices);
		for (int i = 0; i < num; i++)
		{
			int item = deadHeroIndices[i];
			_hashDeadHeroIndex.Add(item);
		}
	}

	private void SetSelectableBuffIndices(int[] buffIndices)
	{
		SetList(_listSelectableBuffIndex, buffIndices);
	}

	private void SetSelectedBuffIndices(int[] buffIndices)
	{
		SetList(_listSelectedBuffIndex, buffIndices);
		_listSelectedBuffIndex.Sort(delegate(int left, int right)
		{
			if (left < right)
			{
				return 1;
			}
			return (left > right) ? (-1) : 0;
		});
	}

	private void SetRemainRefrshCount(int count)
	{
		_remainRefreshCount = count;
	}

	private void SetSelectedMatchTierType(MatchTierType matchTierType)
	{
		_selectedTier = matchTierType;
	}

	private void SetNodeInfos(IDictionary<int, OrdealNodeInfo> dic, OrdealNodeInfo[] nodeInfos, Action<OrdealNodeInfo> extraAction = null)
	{
		dic.Clear();
		int num = __Extension_Method_ICollection__.COUNT(nodeInfos);
		for (int i = 0; i < num; i++)
		{
			OrdealNodeInfo ordealNodeInfo = nodeInfos[i];
			if (ordealNodeInfo != null)
			{
				dic.Add(ordealNodeInfo.Index, ordealNodeInfo);
				Vespa_System_Action_Extension_Method.fire(extraAction, ordealNodeInfo);
			}
		}
	}

	private void ApplyNodeInfo(IDictionary<int, OrdealNodeInfo> dic, OrdealNodeInfo nodeInfo)
	{
		if (nodeInfo != null)
		{
			int index = nodeInfo.Index;
			if (dic.ContainsKey(index))
			{
				dic[index] = nodeInfo;
			}
			else
			{
				dic.Add(index, nodeInfo);
			}
		}
	}

	private void SetList<T>(List<T> list, T[] responseArray)
	{
		list.Clear();
		if (__Extension_Method_ICollection__.COUNT(responseArray) > 0)
		{
			list.AddRange(responseArray);
		}
	}

	public bool IsCompleteOrdeal()
	{
		return _isCompleteOrdeal;
	}

	public int GetCompleteFloor()
	{
		return _completeFoor;
	}

	public int[] GetDeadHeroIndices()
	{
		return _hashDeadHeroIndex.ToArray();
	}

	public int GetLastClearDungeonIndex(int chapterIndex)
	{
		return _lastClearDungeonIndex;
	}

	public int GetMatchScore()
	{
		return _matchScore;
	}

	public OrdealNodeInfo GetOrdealArenaNodeInfo(int chapterIndex, int dungeonIndex)
	{
		if (!GameRoot.isValidInstance)
		{
			return null;
		}
		OrdealArenaNodeData data = GameRoot.Instance.OrdealArenaNodeDataContainer.GetData(chapterIndex, dungeonIndex);
		if (data == null)
		{
			return null;
		}
		return GetOrdealArenaNodeInfo(data.Index);
	}

	public OrdealNodeInfo GetOrdealArenaNodeInfo(int nodeIndex)
	{
		_dicTotalNodeInfo.TryGetValue(nodeIndex, out var value);
		return value;
	}

	public bool IsCompleteNode(int chapterIndex, int dungeonIndex)
	{
		OrdealArenaNodeData data = GameRoot.Instance.OrdealArenaNodeDataContainer.GetData(chapterIndex, dungeonIndex);
		if (data == null)
		{
			return false;
		}
		return _hashClearNodeIndex.Contains(data.Index);
	}

	public bool IsCompleteNode(OrdealArenaNodeData nodeData)
	{
		if (nodeData == null)
		{
			return false;
		}
		return IsComplete(nodeData.Floor);
	}

	private bool IsComplete(int floor)
	{
		return floor <= _completeFoor;
	}

	public bool ExistDeadHeros()
	{
		return _hashDeadHeroIndex.Count > 0;
	}

	public bool IsDeadHero(int heroIndex)
	{
		return _hashDeadHeroIndex.Contains(heroIndex);
	}

	public bool HasSelectableBuff()
	{
		return __Extension_Method_ICollection__.COUNT(_listSelectableBuffIndex) > 0;
	}

	public List<int> GetSelectableBuffList()
	{
		return _listSelectableBuffIndex;
	}

	public List<int> GetSelectedBuffList()
	{
		return _listSelectedBuffIndex;
	}

	public int GetRemainRefreshCount()
	{
		return _remainRefreshCount;
	}

	public bool IsSelectedTier()
	{
		if (__Extension_Method_ICollection__.COUNT(_dicTotalNodeInfo) == 0)
		{
			return false;
		}
		return true;
	}

	public MatchTierType GetSelectedTier()
	{
		return _selectedTier;
	}

	public void Clear()
	{
		ResetAll();
		_observerManager.ClearObserver();
	}

	public void ResetSelectableBuffList()
	{
		_listSelectableBuffIndex.Clear();
	}

	public void ResetAll()
	{
		_dicTotalNodeInfo.Clear();
		_hashClearNodeIndex.Clear();
		_hashDeadHeroIndex.Clear();
		_listSelectableBuffIndex.Clear();
		_listSelectedBuffIndex.Clear();
		_isCompleteOrdeal = false;
		_lastClearDungeonIndex = 0;
		_completeFoor = 0;
		_matchScore = 0;
		_remainRefreshCount = 0;
		_selectedTier = MatchTierType.Bronze;
	}

	public void ResetDeadHeros()
	{
		_hashDeadHeroIndex.Clear();
		NoticeObserver(new OrdealArenaInfoChanged
		{
			Type = EOrdealArenaInfoChangeType.ResetDeadHeros
		});
	}
}
