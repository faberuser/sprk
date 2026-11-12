using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NWorldMap.NMapLayout.NEventManager;
using NGameAsset;
using NShared;
using NShared.ReceiveCompletedReward;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWorldMap.NMapLayout;

public class EventManager
{
	private Mediator _mediator;

	private IncreaseDic<Type, IEventManagerComponent> _componentDictionary = new IncreaseDic<Type, IEventManagerComponent>();

	private bool _mapLoaded;

	public Mediator Mediator => _mediator;

	public NpcAvatarManager NpcAvatarManager { get; private set; }

	public void Open(Mediator mediator)
	{
		_mediator = mediator;
		_mapLoaded = false;
		NpcAvatarManager = new NpcAvatarManager();
		NpcAvatarManager.Open(mediator);
		InitComponent();
		List<IAsyncLoad<AudioClip>> list = SingletonAuto<SoundContainer>.instance.ReserveSound("UI.WorldMap.Coin");
		SingletonAuto<AsyncSceneContainer>.instance.AddList(list);
	}

	public void ReleaseGameAsset()
	{
		ForEachComponent(delegate(IEventManagerComponent e)
		{
			e.Close();
		});
		if (NpcAvatarManager != null)
		{
			NpcAvatarManager.Close();
		}
	}

	public void OnUnloadMap()
	{
		ReleaseGameAsset();
	}

	public void OnLoadMap()
	{
		ForEachComponent(delegate(IEventManagerComponent e)
		{
			e.Open(this);
		});
		SetupWorldMapEvent();
		ProcessVisitNode();
		_mapLoaded = true;
	}

	public void ChainUpdate()
	{
		if (_mapLoaded)
		{
			ForEachComponent(delegate(IEventManagerComponent e)
			{
				e.ChainUpdate();
			});
		}
		if (NpcAvatarManager != null)
		{
			NpcAvatarManager.Update();
		}
	}

	public void OnNodeChanged(int dungeonIndex)
	{
		ForEachComponent(delegate(IEventManagerComponent e)
		{
			e.OnNodeChanged(dungeonIndex);
		});
	}

	public void OnChangeDropBonusEvent(int chapaterIndex, int dungeonIndex, bool isAdd)
	{
		ForEachComponent(delegate(IEventManagerComponent e)
		{
			e.OnChangeDropBonusEvent(chapaterIndex, dungeonIndex, isAdd);
		});
	}

	private void InitComponent()
	{
		ReleaseGameAsset();
		_componentDictionary.Clear();
		AddComponent(new QuestDungeonManager());
		AddComponent(new MapCoinManager());
		AddComponent(new ExcavationManager());
		AddComponent(new EventDungeonManager2());
		AddComponent(new GoblinShopManager());
		AddComponent(new HeroItemManager());
		AddComponent(new FirstRewardManager());
		AddComponent(new VisitRewardManager());
		AddComponent(new DungeonNpcManager());
		AddComponent(new TargetManager());
	}

	private void AddComponent<T>(T component) where T : class, IEventManagerComponent
	{
		_componentDictionary.RemoveSlow(typeof(T));
		_componentDictionary.Add(typeof(T), component);
	}

	public T GetComponent<T>() where T : class, IEventManagerComponent
	{
		IEventManagerComponent o_value = null;
		_componentDictionary.TryGetValue(typeof(T), out o_value);
		return o_value as T;
	}

	private void ForEachComponent(Action<IEventManagerComponent> action)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_componentDictionary); i++)
		{
			IEventManagerComponent valueByIndex = _componentDictionary.GetValueByIndex(i);
			Vespa_System_Action_Extension_Method.fire(action, valueByIndex);
		}
	}

	private void SetupWorldMapEvent()
	{
		foreach (WorldMapEventInfo item in GlobalAutoBehaviour<AccountManager>.instance.WorldMapEventManager)
		{
			CreateWorldMapEvent(item);
		}
	}

	private void CreateWorldMapEvent(WorldMapEventInfo worldMapEventInfo)
	{
		ForEachComponent(delegate(IEventManagerComponent e)
		{
			e.Setup(worldMapEventInfo);
		});
	}

	private void RemoveWorldMapEvent(WorldMapEventInfo worldMapEventInfo)
	{
		ForEachComponent(delegate(IEventManagerComponent e)
		{
			e.Remove(worldMapEventInfo, eatEffect: false);
		});
	}

	public void SortPanel(MonoBehaviour component)
	{
		if (component == null)
		{
			return;
		}
		Transform parent = component.transform.parent;
		if (parent == null)
		{
			VespaDebug.LogError("Parent not exists.");
			return;
		}
		UIPanel component2 = parent.GetComponent<UIPanel>();
		if (component2 == null)
		{
			VespaDebug.LogError("Parent has no panel.");
			return;
		}
		int num = component2.depth + 1;
		for (int i = 0; i < parent.childCount; i++)
		{
			Transform child = parent.GetChild(i);
			if (child == null)
			{
				continue;
			}
			if (child.GetComponent<UIPanel>() == null)
			{
				child.gameObject.AddComponent<UIPanel>().depth = -100;
			}
			UIPanel[] componentsInChildren = child.GetComponentsInChildren<UIPanel>(includeInactive: true);
			if (__Extension_Method_ICollection__.COUNT(componentsInChildren) != 0)
			{
				List<UIPanel> list = new List<UIPanel>();
				list.AddRange(componentsInChildren);
				list.Sort((UIPanel l, UIPanel r) => l.depth.CompareTo(r.depth));
				for (int num2 = 0; num2 < __Extension_Method_ICollection__.COUNT(list); num2++)
				{
					list[num2].depth = num++;
				}
			}
		}
	}

	public void ProcessVisitNode()
	{
		ForEachComponent(delegate(IEventManagerComponent e)
		{
			e.Visit();
		});
	}

	public void RequestReceiveCompletedReward(int chapterIndex, int[] dungeonIndices, int[] eventIndices, Action<NShared.ReceiveCompletedReward.Response> onResponse = null)
	{
		NShared.ReceiveCompletedReward.Request request = new NShared.ReceiveCompletedReward.Request();
		request.ChapterIndex = chapterIndex;
		request.DungeonIndices = dungeonIndices;
		request.EventIndices = eventIndices;
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, onResponse);
	}

	public void ApplyReceiveCompletedRewardResponse(NShared.ReceiveCompletedReward.Response response)
	{
		if (response != null && response.BaseResult == BaseResultType.Success && response.Result == ResultType.Success)
		{
			GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.CurrencyResults);
			GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.ExpResult);
			GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.StaminaResult);
			GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.ItemResults);
			GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Apply(response.EquipItemInfos);
			GlobalAutoBehaviour<AccountManager>.instance.WorldMapEventTimeManager.Set(response.WorldMapEventTimeInfos);
			ApplyChangedWorldMapEvent(response.WorldMapEventInfos, response.RemovedWorldMapEventInfos);
		}
	}

	public void ApplyChangedWorldMapEvent(WorldMapEventInfo[] changedWorldMapEventInfos, WorldMapEventInfo[] removedWorldMapEventInfos)
	{
		GlobalAutoBehaviour<AccountManager>.instance.WorldMapEventManager.Unset(removedWorldMapEventInfos);
		GlobalAutoBehaviour<AccountManager>.instance.WorldMapEventManager.Set(changedWorldMapEventInfos);
		if (changedWorldMapEventInfos != null)
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(changedWorldMapEventInfos); i++)
			{
				WorldMapEventInfo worldMapEventInfo = changedWorldMapEventInfos[i];
				RemoveWorldMapEvent(worldMapEventInfo);
				CreateWorldMapEvent(worldMapEventInfo);
			}
		}
		if (removedWorldMapEventInfos != null)
		{
			for (int j = 0; j < __Extension_Method_ICollection__.COUNT(removedWorldMapEventInfos); j++)
			{
				WorldMapEventInfo worldMapEventInfo2 = removedWorldMapEventInfos[j];
				RemoveWorldMapEvent(worldMapEventInfo2);
			}
		}
		ProcessVisitNode();
	}

	public void ShowReceivedReward(MapNode node, CurrencyResultInfo3[] currencyResultInfos, GoldResultInfo goldResult, GemResultInfo gemResult, ItemResultInfo[] itemResults, EquipItemInfo[] equipItemInfos)
	{
	}

	public void ApplyChangeQuestInfo(string NPCCode)
	{
		String_Extension_Method.IsNullOrEmpty(NPCCode);
	}
}
