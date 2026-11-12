using System;
using System.Collections.Generic;
using System.Linq;
using NGame2.NAccount;
using NGame2.NContentsInfo;
using NGame2.NEventTrigger;
using NGame2.NScene;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NBattle;
using NShared;
using NShared.BeginOrdealArena;
using NShared.EndOrdealArena;
using NShared.NEvent;
using NShared.OrdealInfo;
using NShared.RefreshNode;
using NShared.ResurrectionHero;
using NShared.SelectBuff;
using NShared.SelectTier;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using NVespa.NWeb;

namespace NGame2.NOrdeal;

public class OrdealArenaManager : GlobalAutoBehaviour<OrdealArenaManager>, InitOnCreate
{
	public interface IListener
	{
		void OnOrdealInfo(NShared.OrdealInfo.Response res);

		void OnSelectTier(NShared.SelectTier.Response res);

		void OnSelectBuff(NShared.SelectBuff.Response res);

		void OnBeginOrdealArena(int chapterIndex, int dungeonIndex, int[] heroIndices, OrdealNodeInfo nodeInfo);

		void OnEndOrdealArena(OrdealArenaNodeData nodeData, NShared.EndOrdealArena.Response res);

		void OnRefreshNode(NShared.RefreshNode.Response res);

		void OnResurrectionHero(int reserrectionHeroIndex, NShared.ResurrectionHero.Response res);
	}

	private UserOrdealArenaInfoManager _infoManager;

	private List<IListener> _listListener;

	private DateTime _nextMidnightTime;

	private bool _initNextMidnightTime;

	public MatchSeasonData MatchSeasonData { get; private set; }

	void InitOnCreate.InitializeSingleton()
	{
	}

	public OrdealArenaManager()
	{
		_infoManager = new UserOrdealArenaInfoManager();
		_listListener = new List<IListener>();
		InitNextMidnightTime();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		_listListener.Clear();
		_infoManager = null;
		_initNextMidnightTime = false;
	}

	public bool IsCompleteOrdeal()
	{
		if (_infoManager == null)
		{
			return false;
		}
		return _infoManager.IsCompleteOrdeal();
	}

	public int GetCompleteFloor()
	{
		if (_infoManager == null)
		{
			return 0;
		}
		return _infoManager.GetCompleteFloor();
	}

	public int GetRemainRefreshCount()
	{
		if (_infoManager == null)
		{
			return 0;
		}
		return _infoManager.GetRemainRefreshCount();
	}

	public int[] GetDeadHeroInidices()
	{
		if (_infoManager == null)
		{
			return null;
		}
		return _infoManager.GetDeadHeroIndices();
	}

	public bool IsDeadHero(int heroIndex)
	{
		if (_infoManager == null)
		{
			return false;
		}
		return _infoManager.IsDeadHero(heroIndex);
	}

	public List<CachedHeroInfo> GetEnemyList(int chapterIndex, int dungeonIndex)
	{
		if (_infoManager == null)
		{
			return null;
		}
		OrdealNodeInfo ordealArenaNodeInfo = _infoManager.GetOrdealArenaNodeInfo(chapterIndex, dungeonIndex);
		if (ordealArenaNodeInfo.AccountInfo == null || __Extension_Method_ICollection__.IsNullOrEmpty(ordealArenaNodeInfo.AccountInfo.HeroInfos))
		{
			return null;
		}
		return ordealArenaNodeInfo.AccountInfo.HeroInfos.Values.ToList();
	}

	public int GetMatchScore()
	{
		if (_infoManager == null)
		{
			return 0;
		}
		if (!IsSelctedTier())
		{
			return 0;
		}
		return _infoManager.GetMatchScore();
	}

	public List<int> GetSelectedBuffList()
	{
		if (_infoManager == null)
		{
			return null;
		}
		return _infoManager.GetSelectedBuffList();
	}

	public List<OrdealArenaBuffData> GetSelectedBuffDataList()
	{
		List<int> selectedBuffList = GetSelectedBuffList();
		if (__Extension_Method_ICollection__.IsNullOrEmpty(selectedBuffList))
		{
			return null;
		}
		List<OrdealArenaBuffData> list = new List<OrdealArenaBuffData>();
		foreach (int item in selectedBuffList)
		{
			OrdealArenaBuffData data = GameRoot.Instance.OrdealArenaBuffDataContainer.GetData(item);
			if (data != null)
			{
				list.Add(data);
			}
		}
		return list;
	}

	public OrdealArenaSkillDataBase[] GetOrdealArenaSkillDataBases()
	{
		List<int> selectedBuffList = GetSelectedBuffList();
		if (__Extension_Method_ICollection__.COUNT(selectedBuffList) == 0)
		{
			return null;
		}
		List<OrdealArenaSkillDataBase> list = null;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(selectedBuffList); i++)
		{
			int key = selectedBuffList[i];
			OrdealArenaBuffData data = GameRoot.Instance.OrdealArenaBuffDataContainer.GetData(key);
			if (data != null && data.OptionType == OperationType.UseSkill)
			{
				if (list == null)
				{
					list = new List<OrdealArenaSkillDataBase>();
				}
				OrdealArenaSkillDataBase item = new OrdealArenaSkillDataBase
				{
					Index = data.Index,
					SkillIndex = data.OptionValue
				};
				list.Add(item);
			}
		}
		if (__Extension_Method_ICollection__.COUNT(list) <= 0)
		{
			return null;
		}
		return list.ToArray();
	}

	public OrdealArenaStatDataBase[] GetOrdealArenaStatDataBases()
	{
		List<int> selectedBuffList = GetSelectedBuffList();
		if (__Extension_Method_ICollection__.COUNT(selectedBuffList) == 0)
		{
			return null;
		}
		List<OrdealArenaStatDataBase> list = null;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(selectedBuffList); i++)
		{
			int key = selectedBuffList[i];
			OrdealArenaBuffData data = GameRoot.Instance.OrdealArenaBuffDataContainer.GetData(key);
			if (data != null && data.OptionType != OperationType.UseSkill)
			{
				if (list == null)
				{
					list = new List<OrdealArenaStatDataBase>();
				}
				OrdealArenaStatDataBase item = new OrdealArenaStatDataBase
				{
					Index = data.Index,
					OptionType = data.OptionType,
					OptionValue = data.OptionValue
				};
				list.Add(item);
			}
		}
		if (__Extension_Method_ICollection__.COUNT(list) <= 0)
		{
			return null;
		}
		return list.ToArray();
	}

	public List<int> GetSelectableBuffList()
	{
		if (_infoManager == null)
		{
			return null;
		}
		return _infoManager.GetSelectableBuffList();
	}

	public bool HasSelectableBuff()
	{
		if (_infoManager == null)
		{
			return false;
		}
		return _infoManager.HasSelectableBuff();
	}

	public bool IsSelctedTier()
	{
		if (_infoManager == null)
		{
			return false;
		}
		return _infoManager.IsSelectedTier();
	}

	public MatchTierType GetSelectedTier()
	{
		if (_infoManager == null)
		{
			return MatchTierType.Bronze;
		}
		return _infoManager.GetSelectedTier();
	}

	public OrdealNodeInfo GetOrdealNodeInfo(int index)
	{
		if (_infoManager == null)
		{
			return null;
		}
		return _infoManager.GetOrdealArenaNodeInfo(index);
	}

	public int GetLastClearDungeonIndex(int chapterIndex)
	{
		if (_infoManager == null)
		{
			return 0;
		}
		return _infoManager.GetLastClearDungeonIndex(chapterIndex);
	}

	public bool IsCompleteNode(int chapterIndex, int dungeonIndex)
	{
		if (_infoManager == null)
		{
			return false;
		}
		return _infoManager.IsCompleteNode(chapterIndex, dungeonIndex);
	}

	public void AddListener(IListener listener)
	{
		_listListener.Remove(listener);
		_listListener.Add(listener);
	}

	public void RemoveListener(IListener listener)
	{
		_listListener.Remove(listener);
	}

	public void ForeachListener(Action<IListener> action)
	{
		int num = __Extension_Method_ICollection__.COUNT(_listListener);
		for (int i = 0; i < num; i++)
		{
			IListener listener = _listListener[i];
			if (listener == null)
			{
				_listListener.RemoveAt(i);
				i--;
			}
			else
			{
				Vespa_System_Action_Extension_Method.fire(action, listener);
			}
		}
	}

	private void RequestStartNode()
	{
		OrdealArenaNodeData dataByIndex = GameRoot.Instance.OrdealArenaNodeDataContainer.GetDataByIndex(1);
		NShared.EndOrdealArena.Request request = new NShared.EndOrdealArena.Request
		{
			NodeIndex = 1,
			ChapterIndex = dataByIndex.ChapterIndex,
			DungeonIndex = dataByIndex.DungeonIndex,
			Completed = true
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, WebRequestFlag.None, delegate(NShared.EndOrdealArena.Response res)
		{
			_infoManager.SetClearNodeIndex(res.ClearNodeIndices);
		});
	}

	public void RequestOrdealInfo()
	{
		NShared.OrdealInfo.Request request = new NShared.OrdealInfo.Request();
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.OrdealInfo.Response res)
		{
			if (res.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(res.BaseResult);
			}
			else if (res.Result != NShared.OrdealInfo.ResultType.Success)
			{
				MessagePopup.OpenError(res.Result);
			}
			else
			{
				_infoManager.Set(res);
				if (!__Extension_Method_ICollection__.IsNullOrEmpty(res.OrdealNodeInfos) && __Extension_Method_ICollection__.IsNullOrEmpty(res.ClearNodeIndices))
				{
					RequestStartNode();
				}
				ForeachListener(delegate(IListener e)
				{
					e.OnOrdealInfo(res);
				});
			}
		});
	}

	public void RequestSelectTier(MatchTierType tierType)
	{
		NShared.SelectTier.Request request = new NShared.SelectTier.Request
		{
			Tier = tierType
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.SelectTier.Response res)
		{
			if (res.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(res.BaseResult);
			}
			else if (res.Result != NShared.SelectTier.ResultType.Success)
			{
				MessagePopup.OpenError(res.Result);
			}
			else
			{
				_infoManager.Apply(res);
				if (__Extension_Method_ICollection__.IsNullOrEmpty(res.ClearNodeIndices))
				{
					RequestStartNode();
				}
				ForeachListener(delegate(IListener e)
				{
					e.OnSelectTier(res);
				});
			}
		});
	}

	public void RequestSelectBuff(int buffIndex)
	{
		NShared.SelectBuff.Request request = new NShared.SelectBuff.Request
		{
			BuffIndex = buffIndex
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.SelectBuff.Response res)
		{
			if (res.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(res.BaseResult);
			}
			else if (res.Result != NShared.SelectBuff.ResultType.Success)
			{
				MessagePopup.OpenError(res.Result);
			}
			else
			{
				_infoManager.Apply(res);
				ForeachListener(delegate(IListener e)
				{
					e.OnSelectBuff(res);
				});
			}
		});
	}

	public void RequestBeginOrdealArena(int chapterIndex, int dungeonIndex, int[] heroIndices)
	{
		OrdealNodeInfo nodeInfo = _infoManager.GetOrdealArenaNodeInfo(chapterIndex, dungeonIndex);
		NShared.BeginOrdealArena.Request request = new NShared.BeginOrdealArena.Request
		{
			NodeIndex = nodeInfo.Index,
			HeroIndices = heroIndices
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.BeginOrdealArena.Response res)
		{
			if (res.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(res.BaseResult);
			}
			else if (res.Result != NShared.BeginOrdealArena.ResultType.Success)
			{
				MessagePopup.OpenError(res.Result);
			}
			else
			{
				ForeachListener(delegate(IListener e)
				{
					e.OnBeginOrdealArena(chapterIndex, dungeonIndex, heroIndices, nodeInfo);
				});
			}
		});
	}

	public void RequestEndOrdeaArena(int chapterIndex, int dungeonIndex)
	{
		OrdealArenaNodeData nodeData = GameRoot.Instance.OrdealArenaNodeDataContainer.GetData(chapterIndex, dungeonIndex);
		if (nodeData == null)
		{
			return;
		}
		NShared.EndOrdealArena.Request request = new NShared.EndOrdealArena.Request
		{
			NodeIndex = nodeData.Index,
			ChapterIndex = chapterIndex,
			DungeonIndex = dungeonIndex,
			Completed = true
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.EndOrdealArena.Response res)
		{
			if (res.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(res.BaseResult);
			}
			else if (res.Result != NShared.EndOrdealArena.ResultType.Success)
			{
				MessagePopup.OpenError(res.Result);
			}
			else
			{
				_infoManager.Apply(res);
				ForeachListener(delegate(IListener e)
				{
					e.OnEndOrdealArena(nodeData, res);
				});
			}
		});
	}

	public void RequestEndOrdeaArena(int chapterIndex, int dungeonIndex, int[] heroIndices, bool isWin, string battleLogString, string creatureInfoString, int pureBattleTime, long elapsedBattleTime)
	{
		OrdealArenaNodeData ordealAreanNodeData = GameRoot.Instance.OrdealArenaNodeDataContainer.GetData(chapterIndex, dungeonIndex);
		if (ordealAreanNodeData == null)
		{
			ReturnToLobby(LoadingManager.ELoadingType.Default);
			return;
		}
		NShared.EndOrdealArena.Request request = new NShared.EndOrdealArena.Request
		{
			NodeIndex = ordealAreanNodeData.Index,
			ChapterIndex = chapterIndex,
			DungeonIndex = dungeonIndex,
			Completed = isWin,
			HeroIndices = heroIndices,
			BattleLogString = battleLogString,
			CreatureInfoString = creatureInfoString,
			PureBattleTime = pureBattleTime,
			ElapsedBattleTime = elapsedBattleTime
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.EndOrdealArena.Response res)
		{
			if (res.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(res.BaseResult, delegate
				{
					ReturnToLobby(LoadingManager.ELoadingType.Default);
				});
			}
			else if (res.Result != NShared.EndOrdealArena.ResultType.Success)
			{
				MessagePopup.OpenError(res.Result, delegate
				{
					ReturnToLobby(LoadingManager.ELoadingType.Default);
				});
			}
			else
			{
				_infoManager.Apply(res);
				ForeachListener(delegate(IListener e)
				{
					e.OnEndOrdealArena(ordealAreanNodeData, res);
				});
			}
		});
	}

	public void ReturnToLobby(LoadingManager.ELoadingType loadingType)
	{
		if (PvpLoading.isValidInstance)
		{
			PvpLoading.ReleaseInstance();
		}
		SingletonAuto<EventTriggerManager>.instance.RaiseEvent(new OnClickReturnToMap());
		GlobalAutoBehaviour<LoadingManager>.instance.MoveScene(LoadingManager.ESceneType.Lobby, loadingType, delegate
		{
			SceneAutoBehaviour<OrdealArenaManagement>.instance.AddOrdealArenaSeasonChecker();
		});
	}

	public void RequestRefreshNode(int nodeIndex, bool useGem)
	{
		NShared.RefreshNode.Request request = new NShared.RefreshNode.Request
		{
			NodeIndex = nodeIndex,
			UseGem = useGem
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.RefreshNode.Response res)
		{
			if (res.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(res.BaseResult);
			}
			else if (res.Result != NShared.RefreshNode.ResultType.Success)
			{
				MessagePopup.OpenError(res.Result);
			}
			else
			{
				_infoManager.Apply(res);
				ForeachListener(delegate(IListener e)
				{
					e.OnRefreshNode(res);
				});
			}
		});
	}

	public void RequestResurrectionHero(int heroIndex)
	{
		NShared.ResurrectionHero.Request request = new NShared.ResurrectionHero.Request
		{
			HeroIndex = heroIndex
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.ResurrectionHero.Response res)
		{
			if (res.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(res.BaseResult);
			}
			else if (res.Result != NShared.ResurrectionHero.ResultType.Success)
			{
				MessagePopup.OpenError(res.Result);
			}
			else
			{
				_infoManager.Apply(res);
				ForeachListener(delegate(IListener e)
				{
					e.OnResurrectionHero(heroIndex, res);
				});
			}
		});
	}

	public void AddObserver(IUserObserver observer)
	{
		if (_infoManager != null)
		{
			_infoManager.AddObserver(observer);
		}
	}

	public void RemoveObserver(IUserObserver observer)
	{
		if (_infoManager != null)
		{
			_infoManager.RemoveObserver(observer);
		}
	}

	public TimeSpan GetSeasonRemainTime()
	{
		MatchArena.MatchSeasonInfo ordealSeasonInfo = SingletonAuto<MatchArena>.instance.GetOrdealSeasonInfo();
		if (ordealSeasonInfo == null || !GlobalAutoBehaviour<AccountManager>.isValidInstance)
		{
			return new TimeSpan(-1L);
		}
		DateTime currentServerUTCTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime();
		return ordealSeasonInfo.EndUtc - currentServerUTCTime;
	}

	private bool InitNextMidnightTime()
	{
		if (GlobalAutoBehaviour<AccountManager>.isValidInstance)
		{
			_nextMidnightTime = (_nextMidnightTime = GlobalAutoBehaviour<AccountManager>.instance.GetUTCOffsetNextMidnightTime());
			_initNextMidnightTime = true;
		}
		else
		{
			_initNextMidnightTime = false;
		}
		return _initNextMidnightTime;
	}

	public TimeSpan GetNextMidnightRemainTime()
	{
		if (!GlobalAutoBehaviour<AccountManager>.isValidInstance)
		{
			return new TimeSpan(-1L);
		}
		if (_infoManager != null && !_infoManager.ExistDeadHeros())
		{
			return new TimeSpan(-1L);
		}
		if (!_initNextMidnightTime && !InitNextMidnightTime())
		{
			return new TimeSpan(-1L);
		}
		DateTime currentServerUTCTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime();
		if (_nextMidnightTime.CompareTo(currentServerUTCTime) < 0)
		{
			ResetDeadHeros();
			InitNextMidnightTime();
		}
		return _nextMidnightTime - currentServerUTCTime;
	}

	public void ResetDeadHeros()
	{
		if (_infoManager != null)
		{
			_infoManager.ResetDeadHeros();
		}
	}

	public void ResetAll()
	{
		MatchSeasonData = null;
		if (_infoManager != null)
		{
			_infoManager.ResetAll();
		}
	}

	public void TempCompleteNode(int chapterIndex, int dungeonIndex)
	{
		if (_infoManager != null)
		{
			OrdealNodeInfo ordealArenaNodeInfo = _infoManager.GetOrdealArenaNodeInfo(chapterIndex, dungeonIndex);
			_infoManager.SetCompleNodeInfo(ordealArenaNodeInfo);
		}
	}

	public void TempClearMatchSeasonData()
	{
		MatchSeasonData = null;
	}
}
