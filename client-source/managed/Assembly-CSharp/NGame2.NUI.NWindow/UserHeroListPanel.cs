using System;
using System.Collections.Generic;
using System.Linq;
using NGame2.NAccount;
using NGame2.NBattleContext;
using NGame2.NDispatchBattle;
using NGame2.NUI.NComponent2;
using NGameAsset;
using NShared;
using NVespa;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

[Serializable]
public class UserHeroListPanel
{
	[Serializable]
	public class PositionToggleGroup
	{
		public UIToggle Total;

		public UIToggle Forward;

		public UIToggle Middle;

		public UIToggle Rear;

		public bool IsValidToggle()
		{
			if (Total == null || Middle == null || Rear == null)
			{
				return false;
			}
			return true;
		}
	}

	public interface IListener
	{
		bool IsRestrictedHero(HeroInfo heroInfo);

		bool hasAcceptedHeroes();

		bool hasBannedHeroes();
	}

	public class HeroInfoKey : Tuple<long, int>
	{
		public HeroInfoKey(long AccountId, int HeroIndex)
			: base(AccountId, HeroIndex)
		{
		}
	}

	public HeroPortraitComponent.PrefabSelector PortraitSelector = new HeroPortraitComponent.PrefabSelector(HeroPortraitComponent.PrefabSelector.EPrefabType.Short);

	public PositionToggleGroup TogglePosition;

	public UIScrollGrid GridHeroList;

	private Dictionary<HeroInfoKey, HeroPortraitComponent> _dicUserHeroPortarit = new Dictionary<HeroInfoKey, HeroPortraitComponent>();

	private HashSet<string> _hashReservedHeroSelectVoice = new HashSet<string>();

	private HeroDeck2.EPositionTab _positionTab;

	private CreatureTagType _tagType;

	private HeroPortraitComponent.Listener _heroPortraitListener;

	private IListener _listener;

	private Action<HeroPortraitComponent> _selectionMarkAction;

	private EnterDungeonData _dungeonData;

	private HeroInfoKey _searchKey = new HeroInfoKey(0L, 0);

	public void InitHeroListPanel(EnterDungeonData dungeonData, HeroPortraitComponent.Listener listener, Action<HeroPortraitComponent> selectionMarkAction)
	{
		_heroPortraitListener = listener;
		_listener = listener as IListener;
		_selectionMarkAction = selectionMarkAction;
		_dungeonData = dungeonData;
		if (IsValidTogglePosition() && !TogglePosition.Total.value)
		{
			TogglePosition.Total.value = true;
		}
		else
		{
			SetUserHeroList();
		}
	}

	public void Release()
	{
		ReleaseHeroPortraitComponent();
		_selectionMarkAction = null;
		_heroPortraitListener = null;
	}

	private void ReleaseHeroPortraitComponent()
	{
		FactoryBase.ReleaseAndClearList(_dicUserHeroPortarit);
	}

	private void ReservedHeroSelectVoice(string heroCode)
	{
		if (!String_Extension_Method.IsNullOrEmpty(heroCode) && !_hashReservedHeroSelectVoice.Contains(heroCode))
		{
			_hashReservedHeroSelectVoice.Add(heroCode);
			string code = DefineSound.HeroSelectSound(heroCode);
			List<IAsyncLoad<AudioClip>> list = SingletonAuto<SoundContainer>.instance.ReserveSound(code);
			SingletonAuto<AsyncSceneContainer>.instance.AddList(list);
		}
	}

	public void SetBanUserHeroList(BanRuleData banRuleData, Dictionary<SkillAttrType, int> checkHeroCountByAttrDic = null)
	{
		if (banRuleData == null)
		{
			return;
		}
		RaidBanRuleData[] banRuleDataAry = banRuleData.GetBanRuleDataAry();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(banRuleDataAry); i++)
		{
			RaidBanRuleData raidBanRuleData = banRuleDataAry[i];
			if (raidBanRuleData == null)
			{
				continue;
			}
			switch (raidBanRuleData.BanType)
			{
			case RaidBanType.CreatureTagType:
			{
				if (BanRuleData.GetCreatureTagType(raidBanRuleData.BanValue, out var type2))
				{
					BanByType(type2);
				}
				else
				{
					VespaDebug.LogError("EnumParse Fail / GetCreatureTagType / banValue : " + raidBanRuleData.BanValue);
				}
				break;
			}
			case RaidBanType.SkillAttrType:
			{
				if (BanRuleData.GetSkillAttrType(raidBanRuleData.BanValue, out var type))
				{
					BanByType(type);
				}
				else
				{
					VespaDebug.LogError("EnumParse Fail / GetCreatureTagType / banValue : " + raidBanRuleData.BanValue);
				}
				break;
			}
			case RaidBanType.CodeName:
			{
				string[] array = raidBanRuleData.BanValue.Split(',');
				foreach (string codeName in array)
				{
					BanByType(codeName);
				}
				break;
			}
			case RaidBanType.AttributePermission:
				if (!__Extension_Method_ICollection__.IsNullOrEmpty(checkHeroCountByAttrDic))
				{
					SetBanHeroAttributePermission(raidBanRuleData, ref checkHeroCountByAttrDic);
				}
				break;
			}
		}
	}

	private void SetBanHeroAttributePermission(RaidBanRuleData data, ref Dictionary<SkillAttrType, int> dic)
	{
		int possibleHeroCount = GetPossibleHeroCount(data);
		if (possibleHeroCount <= 0)
		{
			return;
		}
		SkillAttrType banAttrType = GetBanAttrType(data);
		if (banAttrType == SkillAttrType.None)
		{
			return;
		}
		int value = 0;
		if (dic.TryGetValue(banAttrType, out value))
		{
			if (value < possibleHeroCount)
			{
				UnBanByType(banAttrType);
			}
			else
			{
				BanByType(banAttrType);
			}
		}
	}

	public void SetPossibleHeroAttribute(SkillAttrType possibleHeroAttrType)
	{
		AllBan();
		UnBanByType(possibleHeroAttrType);
	}

	private int GetPossibleHeroCount(RaidBanRuleData data)
	{
		int result = 0;
		if (data == null)
		{
			return result;
		}
		if (String_Extension_Method.IsNullOrEmpty(data.BanValue))
		{
			return result;
		}
		string[] array = data.BanValue.Split(',');
		if (__Extension_Method_ICollection__.IsNullOrEmpty(array) || array.Length != 2)
		{
			return result;
		}
		int.TryParse(array[0], out result);
		return result;
	}

	private SkillAttrType GetBanAttrType(RaidBanRuleData data)
	{
		SkillAttrType type = SkillAttrType.None;
		if (data == null)
		{
			return type;
		}
		if (String_Extension_Method.IsNullOrEmpty(data.BanValue))
		{
			return type;
		}
		string[] array = data.BanValue.Split(',');
		if (__Extension_Method_ICollection__.IsNullOrEmpty(array) || array.Length != 2)
		{
			return type;
		}
		BanRuleData.GetSkillAttrType(array[1], out type);
		return type;
	}

	public void BanByType(CreatureTagType type, List<int> enteredHeroIndices)
	{
		if (enteredHeroIndices == null || enteredHeroIndices.Count == 0)
		{
			BanByType(type, checkDispatched: true);
			return;
		}
		foreach (KeyValuePair<HeroInfoKey, HeroPortraitComponent> item in _dicUserHeroPortarit)
		{
			if (!enteredHeroIndices.Contains(item.Value.CreatureData.Index) && !item.Value.IsDispatched && item.Value.CreatureData.TagType == type)
			{
				item.Value.SetBan(ban: true);
			}
		}
	}

	public void UnBanByTag(CreatureTagType type)
	{
		foreach (KeyValuePair<HeroInfoKey, HeroPortraitComponent> item in _dicUserHeroPortarit)
		{
			if (item.Value.CreatureData.TagType == type)
			{
				item.Value.SetBan(ban: false);
			}
		}
	}

	private void BanByType(CreatureTagType type, bool checkDispatched = false)
	{
		foreach (KeyValuePair<HeroInfoKey, HeroPortraitComponent> item in _dicUserHeroPortarit)
		{
			if (item.Value.CreatureData.TagType != type)
			{
				continue;
			}
			if (checkDispatched)
			{
				if (!item.Value.IsDispatched)
				{
					item.Value.SetBan(ban: true);
				}
			}
			else
			{
				item.Value.SetBan(ban: true);
			}
		}
	}

	private void BanByType(SkillAttrType type)
	{
		foreach (KeyValuePair<HeroInfoKey, HeroPortraitComponent> item in _dicUserHeroPortarit)
		{
			if (item.Value.CreatureData.AttrType[0] == type)
			{
				item.Value.SetBan(ban: true);
			}
		}
	}

	private void UnBanByType(SkillAttrType type)
	{
		foreach (KeyValuePair<HeroInfoKey, HeroPortraitComponent> item in _dicUserHeroPortarit)
		{
			if (!(item.Value == null) && item.Value.CreatureData != null && item.Value.CreatureData.AttrType[0] == type)
			{
				item.Value.SetBan(ban: false);
			}
		}
	}

	private void BanByType(string codeName)
	{
		foreach (KeyValuePair<HeroInfoKey, HeroPortraitComponent> item in _dicUserHeroPortarit)
		{
			if (item.Value.CreatureData.CodeName.Equals(codeName))
			{
				item.Value.SetBan(ban: true);
			}
		}
	}

	public void SetBanByHeroIndices(List<int> banHeroIndices)
	{
		foreach (KeyValuePair<HeroInfoKey, HeroPortraitComponent> item in _dicUserHeroPortarit)
		{
			if (__Extension_Method_ICollection__.IsNullOrEmpty(banHeroIndices))
			{
				break;
			}
			int heroIndex = item.Value.HeroInfo.HeroIndex;
			for (int num = __Extension_Method_ICollection__.COUNT(banHeroIndices) - 1; num >= 0; num--)
			{
				int num2 = banHeroIndices[num];
				if (heroIndex == num2)
				{
					banHeroIndices.RemoveAt(num);
					item.Value.SetBan(ban: true);
					break;
				}
			}
		}
	}

	public void AllUnBan()
	{
		foreach (KeyValuePair<HeroInfoKey, HeroPortraitComponent> item in _dicUserHeroPortarit)
		{
			if (!(item.Value == null) && item.Value.CreatureData != null)
			{
				item.Value.SetBan(ban: false);
			}
		}
	}

	private void AllBan()
	{
		foreach (KeyValuePair<HeroInfoKey, HeroPortraitComponent> item in _dicUserHeroPortarit)
		{
			if (!(item.Value == null) && item.Value.CreatureData != null)
			{
				item.Value.SetBan(ban: true);
			}
		}
	}

	public void SetDispatch(bool isSet = true)
	{
		isSet = true;
		foreach (KeyValuePair<HeroInfoKey, HeroPortraitComponent> item in _dicUserHeroPortarit)
		{
			HeroPortraitComponent value = item.Value;
			if (!(value == null) && value.HeroInfo != null)
			{
				if (isSet)
				{
					value.SetDispatched(DispatchHelper.IsDispatchedHero(value.HeroInfo), value.PortraitMode == HeroPortraitComponent.EPortraitMode.Icon);
				}
				else
				{
					value.SetDispatched(isDispatched: false);
				}
			}
		}
		SprkDispatchSelection.FitList(this);
	}

	public void SetUserHeroList()
	{
		ReleaseHeroPortraitComponent();
		int chapter = _dungeonData.Chapter;
		int linkedDungeon = _dungeonData.LinkedDungeon;
		DungeonDifficultyType difficulty = _dungeonData.Difficulty;
		bool isScenario = _dungeonData.IsScenario;
		CreaturePositionType positionType = GetPositionType(_positionTab);
		bool flag = Helper.CheckFixedSetHero(chapter, linkedDungeon, difficulty, isScenario);
		List<CachedHeroInfo> heroInfoList = GetHeroInfoList(flag);
		if (__Extension_Method_ICollection__.IsNullOrEmpty(heroInfoList))
		{
			SprkDispatchSelection.RefreshList(this);
			return;
		}
		if (_listener != null && (_listener.hasAcceptedHeroes() || _listener.hasBannedHeroes()))
		{
			heroInfoList.Sort(delegate(CachedHeroInfo l, CachedHeroInfo r)
			{
				bool flag2 = _listener.IsRestrictedHero(l);
				bool flag3 = _listener.IsRestrictedHero(r);
				if ((flag2 && flag3) || (!flag2 && !flag3))
				{
					int num2 = -l.Transcended.CompareTo(r.Transcended);
					if (num2 != 0)
					{
						return num2;
					}
					num2 = -l.Level.CompareTo(r.Level);
					if (num2 != 0)
					{
						return num2;
					}
					num2 = -l.Star.CompareTo(r.Star);
					if (num2 != 0)
					{
						return num2;
					}
					return l.HeroIndex.CompareTo(r.HeroIndex);
				}
				if (!flag2)
				{
					return -1;
				}
				return (!flag3) ? 1 : 0;
			});
		}
		for (int num = 0; num < __Extension_Method_ICollection__.COUNT(heroInfoList); num++)
		{
			CachedHeroInfo cachedHeroInfo = heroInfoList[num];
			if (cachedHeroInfo == null)
			{
				continue;
			}
			if (flag)
			{
				HeroInfo dungeonHeroSetHeroInfo = Helper.GetDungeonHeroSetHeroInfo(chapter, linkedDungeon, difficulty, cachedHeroInfo.HeroIndex, isScenario);
				if (dungeonHeroSetHeroInfo != null)
				{
					cachedHeroInfo = dungeonHeroSetHeroInfo as CachedHeroInfo;
				}
			}
			bool isRestricted = _listener != null && _listener.IsRestrictedHero(cachedHeroInfo);
			if (_positionTab == HeroDeck2.EPositionTab.Total)
			{
				CreateHeroPortraitComponent(cachedHeroInfo, isRestricted, flag);
				continue;
			}
			CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(cachedHeroInfo.HeroIndex);
			if (data != null && data.GetPositionType() == positionType)
			{
				CreateHeroPortraitComponent(cachedHeroInfo, isRestricted, flag);
			}
		}
		UIHelper.RepositionGrid(GridHeroList);
		SelectionMark();
		SprkDispatchSelection.RefreshList(this);
	}

	public void OnHeroPositionToggleChange()
	{
		HeroDeck2.EPositionTab positionTab = _positionTab;
		_ = _tagType;
		if (IsValidTogglePosition())
		{
			if (TogglePosition.Total.value)
			{
				_positionTab = HeroDeck2.EPositionTab.Total;
			}
			else if (TogglePosition.Forward.value)
			{
				_positionTab = HeroDeck2.EPositionTab.Forward;
			}
			else if (TogglePosition.Middle.value)
			{
				_positionTab = HeroDeck2.EPositionTab.Middle;
			}
			else if (TogglePosition.Rear.value)
			{
				_positionTab = HeroDeck2.EPositionTab.Rear;
			}
			if (positionTab != _positionTab)
			{
				SetUserHeroList();
			}
		}
	}

	private bool IsValidTogglePosition()
	{
		if (TogglePosition == null || !TogglePosition.IsValidToggle())
		{
			return false;
		}
		return true;
	}

	public void SelectionMark()
	{
		List<HeroPortraitComponent> list = _dicUserHeroPortarit.Values.ToList();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(list); i++)
		{
			HeroPortraitComponent heroPortraitComponent = list[i];
			if (!(heroPortraitComponent == null))
			{
				Vespa_System_Action_Extension_Method.fire(_selectionMarkAction, heroPortraitComponent);
			}
		}
	}

	private List<CachedHeroInfo> GetHeroInfoList(bool isDungeonHeroSet)
	{
		List<CachedHeroInfo> list = null;
		int chapter = _dungeonData.Chapter;
		int linkedDungeon = _dungeonData.LinkedDungeon;
		DungeonDifficultyType difficulty = _dungeonData.Difficulty;
		if (isDungeonHeroSet)
		{
			int heroListSetDataIndex = GameRoot.Instance.DungeonHeroSetDataContainer.GetData(chapter, linkedDungeon, difficulty).HeroListSetDataIndex;
			if (heroListSetDataIndex != 0)
			{
				IList<DungeonHeroListSetData> datasByGroupIndex = GameRoot.Instance.DungeonHeroListSetDataContainer.GetDatasByGroupIndex(heroListSetDataIndex);
				if (!__Extension_Method_ICollection__.IsNullOrEmpty(datasByGroupIndex))
				{
					list = new List<CachedHeroInfo>();
					for (int i = 0; i < __Extension_Method_ICollection__.COUNT(datasByGroupIndex); i++)
					{
						DungeonHeroListSetData dungeonHeroListSetData = datasByGroupIndex[i];
						CachedHeroInfo cachedHeroInfo = new CachedHeroInfo();
						cachedHeroInfo.HeroIndex = dungeonHeroListSetData.HeroIndex;
						cachedHeroInfo.CostumeIndex = dungeonHeroListSetData.CostumeIndex;
						list.Add(cachedHeroInfo);
					}
				}
			}
		}
		if (__Extension_Method_ICollection__.IsNullOrEmpty(list))
		{
			list = GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfoList(sort: true);
		}
		return list;
	}

	public HeroPortraitComponent GetUserHeroPortraitComponent(long ownerAccountId, int heroIndex)
	{
		return Vespa_Generic_IDictionary_Extension_Method.GETVALUE(_dicUserHeroPortarit, new HeroInfoKey(ownerAccountId, heroIndex), null);
	}

	public void SetPositionType(HeroDeck2.EPositionTab positionTab)
	{
		_positionTab = positionTab;
	}

	private CreaturePositionType GetPositionType(HeroDeck2.EPositionTab tab)
	{
		return tab switch
		{
			HeroDeck2.EPositionTab.Forward => CreaturePositionType.Forward, 
			HeroDeck2.EPositionTab.Middle => CreaturePositionType.Middle, 
			HeroDeck2.EPositionTab.Rear => CreaturePositionType.Rear, 
			_ => CreaturePositionType.Forward, 
		};
	}

	private void CreateHeroPortraitComponent(CachedHeroInfo heroInfo, bool isRestricted = false, bool isDungeonHeroSet = false)
	{
		if (heroInfo != null)
		{
			HeroPortraitComponent.OwnerInfo owner = new HeroPortraitComponent.OwnerInfo
			{
				AccountId = GlobalAutoBehaviour<AccountManager>.instance.UserManager.Id,
				Nick = GlobalAutoBehaviour<AccountManager>.instance.UserManager.Nick,
				Level = GlobalAutoBehaviour<AccountManager>.instance.UserManager.TeamLevel
			};
			PrefabAuto<HeroPortraitComponent> prefabAuto = PortraitSelector.PrefabAuto;
			HeroPortraitComponent.EPortraitMode portraitMode = ((PortraitSelector.PrefabType == HeroPortraitComponent.PrefabSelector.EPrefabType.Small) ? HeroPortraitComponent.EPortraitMode.Icon : HeroPortraitComponent.EPortraitMode.Basic);
			long id = GlobalAutoBehaviour<AccountManager>.instance.UserManager.Id;
			_ = heroInfo.HeroIndex;
			HeroPortraitComponent heroPortraitComponent = UIHelper.AddGridItem(GridHeroList, prefabAuto);
			heroPortraitComponent.Open(heroInfo, _heroPortraitListener, owner, portraitMode);
			heroPortraitComponent.SetRestricted(isRestricted);
			heroPortraitComponent.EnableViewMyInfo(enable: true);
			if (isDungeonHeroSet)
			{
				heroPortraitComponent.ShowCreatureName();
			}
			_dicUserHeroPortarit.Add(new HeroInfoKey(id, heroInfo.HeroIndex), heroPortraitComponent);
			if (heroPortraitComponent.CreatureData != null)
			{
				ReservedHeroSelectVoice(heroPortraitComponent.CreatureData.CodeName);
			}
		}
	}

	public void ForeachAction(Action<HeroPortraitComponent> action)
	{
		foreach (KeyValuePair<HeroInfoKey, HeroPortraitComponent> item in _dicUserHeroPortarit)
		{
			Vespa_System_Action_Extension_Method.fire(action, item.Value);
		}
	}

	public void UpdateAction(long accountId, int heroIndex, Action<HeroPortraitComponent> action)
	{
		HeroPortraitComponent value = null;
		_searchKey.Item1 = accountId;
		_searchKey.Item2 = heroIndex;
		if (_dicUserHeroPortarit.TryGetValue(_searchKey, out value))
		{
			Vespa_System_Action_Extension_Method.fire(action, value);
		}
	}
}
