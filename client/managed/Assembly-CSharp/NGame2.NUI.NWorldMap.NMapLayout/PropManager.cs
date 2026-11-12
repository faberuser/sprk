using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NGame2.NAccount;
using NGame2.NDropBonusEvent;
using NGame2.NGameSetting;
using NGame2.NGuild;
using NGame2.NOrdeal;
using NGame2.NScene;
using NGame2.NTutorial;
using NGame2.NUtil;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using NVespa.NTask;
using NVespa.NTask.NRuntime;
using UnityEngine;

namespace NGame2.NUI.NWorldMap.NMapLayout;

public class PropManager : ITutorialListener, IUserObserver<DropBonusEventManager.DropBonusEventChangeInfo>, IUserObserver, IUserObserver<MonthlyHeroInfo>, IUserObserver<OrdealArenaInfoChanged>
{
	public enum PropActionType
	{
		None,
		SetText,
		SetGuildText,
		SetLayer,
		SetRenderFacade,
		SetHonorRenderFacade,
		SetOrdealArena
	}

	public class PropInfo<T> where T : class
	{
		public WorldMapPropData Data;

		public T Object;

		public EventBonusType EventBonusType;
	}

	public const string WorldMapPropPrefabRoot = "Prefab2/WorldMap/Prop/";

	private Mediator _myMediator;

	private List<PropInfo<DummyNodeIndicator>> _propIndicatorList = new List<PropInfo<DummyNodeIndicator>>();

	private List<PropInfo<GameObject>> _propPrefabList = new List<PropInfo<GameObject>>();

	private List<PropComponent> _propComponentList = new List<PropComponent>();

	public void Open(Mediator mediator)
	{
		_myMediator = mediator;
	}

	public void OnUnloadMap()
	{
		CloseInternal();
	}

	public void OnLoadMap()
	{
		SingletonUpdater<TutorialManager>.instance.AddListener(this);
		GlobalAutoBehaviour<DropBonusEventManager>.instance.ObserverManager.AddObserver(this);
		GlobalAutoBehaviour<OrdealArenaManager>.instance.AddObserver(this);
		InitProp();
	}

	public void Close()
	{
		SingletonUpdater<TutorialManager>.instance.RemoveListener(this);
		GlobalAutoBehaviour<DropBonusEventManager>.instance.ObserverManager.RemoveObserver(this);
		GlobalAutoBehaviour<OrdealArenaManager>.instance.RemoveObserver(this);
		CloseInternal();
	}

	private void CloseInternal()
	{
		_propComponentList.ForEach(ReleasePropComponent);
		_propComponentList.Clear();
		_propIndicatorList.ForEach(DestroyIndicatorProp);
		_propIndicatorList.Clear();
		_propPrefabList.ForEach(DestroyPrefabProp);
		_propPrefabList.Clear();
	}

	private void ReleasePropComponent(PropComponent propComponent)
	{
		propComponent.Release();
	}

	private void DestroyPrefabProp(PropInfo<GameObject> info)
	{
		if (info != null && info.Object != null)
		{
			VespaObject.DestroyGo(info.Object);
			info.Object = null;
		}
	}

	private void DestroyIndicatorProp(PropInfo<DummyNodeIndicator> info)
	{
		if (info != null && info.Object != null)
		{
			info.Object.CloseImmediately();
			info.Object = null;
		}
	}

	public void ChainUpdate()
	{
		_propComponentList.ForEach(delegate(PropComponent prop)
		{
			prop.ChainUpdate();
		});
	}

	private void InitProp()
	{
		CloseInternal();
		RefreshProp();
	}

	private void RefreshProp()
	{
		List<PropInfo<GameObject>> list = new List<PropInfo<GameObject>>();
		List<PropInfo<DummyNodeIndicator>> list2 = new List<PropInfo<DummyNodeIndicator>>();
		int chapterIndex = NGame2.NAccount.UserCampaignManager.ChapterIndex;
		IList<WorldMapPropData> propDataOfChapter = GameRoot.Instance.WorldMapPropDataContainer.GetPropDataOfChapter(chapterIndex);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(propDataOfChapter); i++)
		{
			WorldMapPropData propData = SprkOrvelVisuals.FilterProp(propDataOfChapter[i]);
			if (propData == null)
			{
				continue;
			}
			EventBonusType eventBonusType = EventBonusType.None;
			if (propData.ConditionType == WorldMapPropConditionType.CompleteTutorial)
			{
				int tutorialIndex = Vespa_String_Extension_Method.TOINT(Helper.GetArg(propData.ConditionArgs, 0), 0);
				bool num = Vespa_String_Extension_Method.TOBOOL(Helper.GetArg(propData.ConditionArgs, 1), i_default: false);
				bool flag = GlobalAutoBehaviour<AccountManager>.instance.TutorialManager.IsComplete(tutorialIndex);
				if (num != flag)
				{
					continue;
				}
			}
			else if (propData.ConditionType == WorldMapPropConditionType.ClearDungeon)
			{
				int chapterIndex2 = Vespa_String_Extension_Method.TOINT(Helper.GetArg(propData.ConditionArgs, 0), 0);
				int dungeonIndex = Vespa_String_Extension_Method.TOINT(Helper.GetArg(propData.ConditionArgs, 1), 0);
				bool num2 = Vespa_String_Extension_Method.TOBOOL(Helper.GetArg(propData.ConditionArgs, 2), i_default: false);
				bool flag2 = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.IsCompleted(chapterIndex2, dungeonIndex);
				if (num2 != flag2)
				{
					continue;
				}
			}
			else if (propData.ConditionType == WorldMapPropConditionType.BonusEvent)
			{
				eventBonusType = ArgParser.GetValueEnum(propData.ConditionArgs, 0, EventBonusType.None);
				string valueString = ArgParser.GetValueString(propData.ConditionArgs, 1);
				if (GlobalAutoBehaviour<DropBonusEventManager>.instance.GetEffectiveEvent(eventBonusType, valueString) == null)
				{
					continue;
				}
			}
			else if (propData.ConditionType == WorldMapPropConditionType.BattleEvent)
			{
				BattleType valueEnum = ArgParser.GetValueEnum(propData.ConditionArgs, 0, BattleType.None);
				eventBonusType = ArgParser.GetValueEnum(propData.ConditionArgs, 1, EventBonusType.None);
				if (GlobalAutoBehaviour<DropBonusEventManager>.instance.GetEffectiveEvent(valueEnum, eventBonusType) == null)
				{
					continue;
				}
			}
			else if (propData.ConditionType == WorldMapPropConditionType.ShopRestockEffect)
			{
				eventBonusType = ArgParser.GetValueEnum(propData.ConditionArgs, 2, EventBonusType.None);
				string valueString2 = ArgParser.GetValueString(propData.ConditionArgs, 3);
				DropBonusEvent effectiveEvent = GlobalAutoBehaviour<DropBonusEventManager>.instance.GetEffectiveEvent(eventBonusType, valueString2);
				if (eventBonusType != EventBonusType.None && effectiveEvent == null)
				{
					continue;
				}
			}
			else if (propData.ConditionType == WorldMapPropConditionType.MonthlyHeroRewardable)
			{
				if (GlobalAutoBehaviour<AccountManager>.instance.MonthlyHeroManager == null)
				{
					continue;
				}
			}
			else if (propData.ConditionType == WorldMapPropConditionType.GuildAttendanceRewardable)
			{
				if (!GlobalAutoBehaviour<GuildManager>.instance.CheckGuildAttendanceRewardable)
				{
					continue;
				}
			}
			else if (propData.ConditionType == WorldMapPropConditionType.EventWorldTree && !SingletonAuto<ConditionChecker>.instance.Check(propData.ConditionArgs))
			{
				continue;
			}
			PropInfo<GameObject> propInfo = _propPrefabList.FirstOrDefault((PropInfo<GameObject> e) => e.Data == propData);
			if (propInfo != null)
			{
				list.Add(propInfo);
				continue;
			}
			PropInfo<DummyNodeIndicator> propInfo2 = _propIndicatorList.FirstOrDefault((PropInfo<DummyNodeIndicator> e) => e.Data == propData);
			if (propInfo2 != null)
			{
				list2.Add(propInfo2);
				continue;
			}
			GameObject createdObject = null;
			switch (propData.PropType)
			{
			case WorldMapPropType.Prefab:
			{
				string text = "Prefab2/WorldMap/Prop/" + propData.PropName;
				GameObject gameObject = VespaResource.Load<GameObject>(text);
				if (gameObject == null)
				{
					VespaDebug.LogError("Prop prefab not found : " + text);
					continue;
				}
				GameObject gameObject2 = UnityEngine.Object.Instantiate(gameObject);
				if (gameObject2 == null)
				{
					VespaDebug.LogError("Instantiated propObject is null : " + text);
					continue;
				}
				Vespa_Unity_Transform_Extension_Method.ParentResetTM(gameObject2.transform, _myMediator.Layout.PrefabPropRoot);
				gameObject2.transform.localScale = Vector3.one * propData.Scale / 100f;
				gameObject2.transform.localPosition = new Vector3(propData.PositionX, propData.PositionY, 0f);
				gameObject2.transform.rotation = Quaternion.AngleAxis(propData.Angle, Vector3.back);
				list.Add(new PropInfo<GameObject>
				{
					Data = propData,
					Object = gameObject2,
					EventBonusType = eventBonusType
				});
				createdObject = gameObject2;
				break;
			}
			case WorldMapPropType.Effect:
			case WorldMapPropType.WorldEffect:
			{
				DummyNodeIndicator dummyNodeIndicator2 = new DummyNodeIndicator(chapterIndex, 0);
				dummyNodeIndicator2.OpenAssetEffect(_myMediator, propData.PropName, propData.PositionX, propData.PositionY, propData.PropType == WorldMapPropType.WorldEffect);
				dummyNodeIndicator2.SetScale(propData.Scale);
				dummyNodeIndicator2.Rotate(propData.Angle);
				dummyNodeIndicator2.Show(propData.ConditionType != WorldMapPropConditionType.ShopRestockEffect);
				list2.Add(new PropInfo<DummyNodeIndicator>
				{
					Data = propData,
					Object = dummyNodeIndicator2,
					EventBonusType = eventBonusType
				});
				createdObject = dummyNodeIndicator2.EffectGo;
				break;
			}
			case WorldMapPropType.CustomEffect:
			{
				string[] conditionArgs = propData.ConditionArgs;
				int valueInt = ArgParser.GetValueInt(conditionArgs, 0);
				bool valueBool = ArgParser.GetValueBool(conditionArgs, 1);
				if (valueInt == 0)
				{
					VespaDebug.LogError("Can't Parse string or Index zero!!! string = " + propData.ConditionArgs[0]);
					continue;
				}
				DummyNodeIndicator dummyNodeIndicator = new DummyNodeIndicator(chapterIndex, valueInt);
				dummyNodeIndicator.OpenAssetEffect(_myMediator, propData.PropName, propData.PositionX, propData.PositionY, propData.PropType == WorldMapPropType.CustomEffect);
				dummyNodeIndicator.SetScale(propData.Scale);
				dummyNodeIndicator.Rotate(propData.Angle);
				dummyNodeIndicator.Show(valueBool);
				list2.Add(new PropInfo<DummyNodeIndicator>
				{
					Data = propData,
					Object = dummyNodeIndicator,
					EventBonusType = eventBonusType
				});
				createdObject = dummyNodeIndicator.EffectGo;
				break;
			}
			}
			ProcessPropAction(createdObject, propData.PropAction, propData.LayerName, _myMediator.Layout.PrefabPropRoot.gameObject.layer);
		}
		for (int num3 = 0; num3 < __Extension_Method_ICollection__.COUNT(_propPrefabList); num3++)
		{
			PropInfo<GameObject> propInfo3 = _propPrefabList[num3];
			WorldMapPropData propData2 = propInfo3.Data;
			if (!list.Exists((PropInfo<GameObject> e) => e.Data == propData2))
			{
				DestroyPrefabProp(propInfo3);
			}
		}
		for (int num4 = 0; num4 < __Extension_Method_ICollection__.COUNT(_propIndicatorList); num4++)
		{
			PropInfo<DummyNodeIndicator> propInfo4 = _propIndicatorList[num4];
			WorldMapPropData propData3 = propInfo4.Data;
			if (!list2.Exists((PropInfo<DummyNodeIndicator> e) => e.Data == propData3))
			{
				DestroyIndicatorProp(propInfo4);
			}
		}
		_propPrefabList = list;
		_propIndicatorList = list2;
		_propComponentList.Clear();
		_propPrefabList.ForEach(delegate(PropInfo<GameObject> info)
		{
			PropComponent component = info.Object.GetComponent<PropComponent>();
			if (component != null)
			{
				if (!component.IsValid())
				{
					component.Setup(info.Data.PropAction, _myMediator);
				}
				_propComponentList.Add(component);
			}
		});
	}

	public void ChangedPropPrefabList()
	{
		_propPrefabList.ForEach(delegate(PropInfo<GameObject> info)
		{
			ProcessPropAction(info.Object, info.Data.PropAction, info.Data.LayerName, _myMediator.Layout.PrefabPropRoot.gameObject.layer);
		});
	}

	public void ForceEnableProbComponent(int chapterIndex, int dungeonIndex, bool isEnable, float delay = 0f)
	{
		VespaCoroutine.Start(coForceEnableProbComponent(chapterIndex, dungeonIndex, isEnable, delay), null, null);
	}

	private IEnumerator coForceEnableProbComponent(int chapterIndex, int dungeonIndex, bool isEnable, float delay)
	{
		yield return null;
		while (GlobalAutoBehaviour<LoadingManager>.instance.IsSceneLoading)
		{
			yield return null;
		}
		yield return new WaitForSeconds(delay);
		PropInfo<DummyNodeIndicator> propInfo = _propIndicatorList.Find((PropInfo<DummyNodeIndicator> propInfo2) => propInfo2.Object.ChapterIndex == chapterIndex && propInfo2.Object.DungeonIndex == dungeonIndex);
		if (propInfo == null)
		{
			VespaDebug.LogError($"Can't Find Match Prob Indicator!!! List Count = {__Extension_Method_ICollection__.COUNT(_propIndicatorList)} / ChapterIndex : {chapterIndex} / DungeonIndex : {dungeonIndex}");
		}
		else
		{
			propInfo.Object.Show(isEnable);
		}
	}

	private void ProcessPropAction(GameObject createdObject, string[] actions, string layerType, int originLayer)
	{
		PropActionType valueEnum = ArgParser.GetValueEnum<string, PropActionType>((IList<string>)actions, 0, PropActionType.None);
		switch (valueEnum)
		{
		case PropActionType.SetLayer:
			if (!String_Extension_Method.IsNullOrEmpty(layerType))
			{
				GameUIRoot.ELayerType layerType3 = (GameUIRoot.ELayerType)Enum.Parse(typeof(GameUIRoot.ELayerType), layerType);
				Vespa_Unity_GameObject_Extension_Method.ChangeLayerRecursive(createdObject, GameUIRoot.instance.GetLayerInfo(layerType3).Layer);
			}
			break;
		case PropActionType.SetText:
		case PropActionType.SetGuildText:
		{
			string valueString = ArgParser.GetValueString(actions, 1);
			int valueInt = ArgParser.GetValueInt(actions, 2);
			string valueString2 = ArgParser.GetValueString(actions, 3);
			int valueInt2 = ArgParser.GetValueInt(actions, 4);
			string valueString3 = ArgParser.GetValueString(actions, 5);
			Transform transform = Vespa_Unity_Transform_Extension_Method.FindFlexible(createdObject.transform, valueString, valueInt);
			if (transform == null)
			{
				break;
			}
			UILabel component = transform.GetComponent<UILabel>();
			if (!(component == null))
			{
				if (valueEnum == PropActionType.SetGuildText)
				{
					int valueInt3 = ArgParser.GetValueInt(actions, 6);
					GuildBuildingInfo guildBuildingInfo = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info.GetGuildBuildingInfo(valueInt3);
					string text = "";
					text = ((guildBuildingInfo != null) ? LocalizationManager.GetStringDirect(valueString2, guildBuildingInfo.BuildingLevel) : LocalizationManager.GetStringDirect(valueString2));
					UIHelper.SetLabelText(component, text);
				}
				else
				{
					UIHelper.SetLabelText(component, LocalizationManager.GetStringDirect(valueString2));
				}
				if (valueInt2 > 0)
				{
					component.fontSize = valueInt2;
				}
				if (!string.IsNullOrEmpty(valueString3))
				{
					Color color = NGUIText.ParseColor(valueString3, 0);
					UIHelper.SetLabelColor(component, color);
				}
				if (!string.IsNullOrEmpty(layerType))
				{
					GameUIRoot.ELayerType layerType2 = (GameUIRoot.ELayerType)Enum.Parse(typeof(GameUIRoot.ELayerType), layerType);
					Vespa_Unity_GameObject_Extension_Method.ChangeLayerRecursive(createdObject, GameUIRoot.instance.GetLayerInfo(layerType2).Layer);
				}
				else
				{
					Vespa_Unity_GameObject_Extension_Method.ChangeLayerRecursive(createdObject, originLayer);
				}
				createdObject.name = valueString2;
			}
			break;
		}
		}
	}

	void ITutorialListener.OnTutorialBegin(NShared.TutorialData tutorialData)
	{
	}

	void ITutorialListener.OnTutorialEnd(int tutorialIndex, Action onEnd)
	{
		RefreshProp();
		Vespa_System_Action_Extension_Method.fire(onEnd);
	}

	bool IUserObserver.IsGlobal()
	{
		return false;
	}

	void IUserObserver<DropBonusEventManager.DropBonusEventChangeInfo>.OnChanged(DropBonusEventManager.DropBonusEventChangeInfo changeInfo)
	{
		RefreshProp();
	}

	void IUserObserver<OrdealArenaInfoChanged>.OnChanged(OrdealArenaInfoChanged changedInfo)
	{
		if (changedInfo != null && changedInfo.Type == EOrdealArenaInfoChangeType.CompleteFloor)
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_propComponentList); i++)
			{
				_propComponentList[i].Refresh();
			}
		}
	}

	void IUserObserver<MonthlyHeroInfo>.OnChanged(MonthlyHeroInfo changedInfo)
	{
		SetMonthlyHeroRewardableNew();
	}

	public void SetMonthlyHeroRewardableNew()
	{
		if (GlobalAutoBehaviour<AccountManager>.instance.MonthlyHeroManager == null)
		{
			return;
		}
		PropInfo<DummyNodeIndicator> propInfo = _propIndicatorList.Find((PropInfo<DummyNodeIndicator> item) => item.Data.ConditionType == WorldMapPropConditionType.MonthlyHeroRewardable);
		if (propInfo != null)
		{
			bool num = UIHelper.IsEnabledGameObjet(propInfo.Object.EffectGo);
			bool num2 = GlobalAutoBehaviour<AccountManager>.instance.MonthlyHeroManager.ExistReceiableReward();
			bool mothlyHeroWindowVisited = GlobalAutoBehaviour<AccountManager>.instance.MonthlyHeroManager.MothlyHeroWindowVisited;
			bool flag = num2 && !mothlyHeroWindowVisited;
			if (num != flag)
			{
				propInfo.Object.Show(flag);
			}
		}
	}

	private void SetShopRestockEffectPlay(int shopIndex, bool play)
	{
		PropInfo<DummyNodeIndicator> propInfo = _propIndicatorList.Find(delegate(PropInfo<DummyNodeIndicator> item)
		{
			if (item.Data.ConditionType != WorldMapPropConditionType.ShopRestockEffect)
			{
				return false;
			}
			int valueInt3 = ArgParser.GetValueInt(item.Data.ConditionArgs, 1);
			bool flag = UIHelper.IsEnabledGameObjet(item.Object.EffectGo);
			return valueInt3 == shopIndex && flag != play;
		});
		if (propInfo == null)
		{
			return;
		}
		if (play)
		{
			int valueInt = ArgParser.GetValueInt(propInfo.Data.ConditionArgs, 0);
			foreach (PropInfo<DummyNodeIndicator> propIndicator in _propIndicatorList)
			{
				if (propIndicator.Data.ConditionType == WorldMapPropConditionType.ShopRestockEffect)
				{
					int valueInt2 = ArgParser.GetValueInt(propIndicator.Data.ConditionArgs, 0);
					if (valueInt == valueInt2)
					{
						propIndicator.Object.Show(show: false);
					}
				}
			}
		}
		propInfo.Object.Show(play);
	}

	public void ShopRestockEffectPlay(Dictionary<int, bool> playList)
	{
		foreach (KeyValuePair<int, bool> play in playList)
		{
			SetShopRestockEffectPlay(play.Key, play.Value);
		}
	}

	public void SetEventDungeonCurrencyChecknew()
	{
		PropInfo<DummyNodeIndicator> propInfo = _propIndicatorList.Find(delegate(PropInfo<DummyNodeIndicator> item)
		{
			EventBonusType valueEnum2 = ArgParser.GetValueEnum(item.Data.ConditionArgs, 0, EventBonusType.None);
			return (item.Data.ConditionType == WorldMapPropConditionType.BonusEvent && valueEnum2 == EventBonusType.EventShop && __Extension_Method_ICollection__.COUNT(item.Data.ConditionArgs) > 2) ? true : false;
		});
		if (propInfo != null)
		{
			CurrencyType valueEnum = ArgParser.GetValueEnum(propInfo.Data.ConditionArgs, 2, CurrencyType.None);
			int num = 0;
			long num2 = num;
			switch (valueEnum)
			{
			case CurrencyType.EventDungeonTicket:
				num2 = GlobalAutoBehaviour<AccountManager>.instance.UserManager.EventDungeonTicket;
				break;
			case CurrencyType.EventDungeonPoint:
				num2 = GlobalAutoBehaviour<AccountManager>.instance.UserManager.EventDungeonPoint;
				break;
			case CurrencyType.EventDungeonPoint2:
				num2 = GlobalAutoBehaviour<AccountManager>.instance.UserManager.EventDungeonPoint2;
				break;
			case CurrencyType.EventHeroDungeonTicket:
				num2 = GlobalAutoBehaviour<AccountManager>.instance.UserManager.EventHeroDungeonTicket;
				break;
			}
			DummyNodeIndicator dummyNodeIndicator = propInfo.Object;
			long num3 = num2;
			int num4 = 0;
			dummyNodeIndicator.Show(num3 > num4);
		}
	}

	public void SetGuildAttendanceNewIndicator()
	{
		PropInfo<DummyNodeIndicator> propInfo = _propIndicatorList.Find(delegate(PropInfo<DummyNodeIndicator> item)
		{
			if (item == null || item.Data == null)
			{
				return false;
			}
			return item.Data.ConditionType == WorldMapPropConditionType.GuildAttendanceRewardable;
		});
		if (propInfo != null && propInfo.Object != null)
		{
			propInfo.Object.Show(GlobalAutoBehaviour<GuildManager>.instance.CheckGuildAttendanceRewardable);
		}
	}

	public void SetEventWorldTreeIndicator()
	{
		RefreshProp();
	}

	public ITask PropShowTask()
	{
		EntryMenuData entryMenu = GameSetting.data.EntryMenu;
		SequenceTask sequenceTask = new SequenceTask();
		sequenceTask.AddTask(coPropShowOrHide(isShow: true, entryMenu.BlendTimeLength));
		return sequenceTask;
	}

	public ITask PropHideTask(EntryMenuSettingData menuAppearance)
	{
		SequenceTask sequenceTask = new SequenceTask();
		sequenceTask.AddTask(coPropShowOrHide(isShow: false, menuAppearance.NpcSlideTimeLength));
		return sequenceTask;
	}

	private IEnumerator coPropShowOrHide(bool isShow, float duration)
	{
		if (duration == 0f)
		{
			float result = (isShow ? 1f : 0f);
			_propComponentList.ForEach(delegate(PropComponent prop)
			{
				prop.SetExtraAlpha(result);
			});
			yield break;
		}
		float elapsedTime = 0f;
		do
		{
			yield return null;
			elapsedTime += Time.deltaTime;
			float num = Mathf.Clamp01(elapsedTime / duration);
			float offset = (isShow ? num : (1f - num));
			_propComponentList.ForEach(delegate(PropComponent prop)
			{
				prop.SetExtraAlpha(offset);
			});
		}
		while (!(elapsedTime >= duration));
	}
}
