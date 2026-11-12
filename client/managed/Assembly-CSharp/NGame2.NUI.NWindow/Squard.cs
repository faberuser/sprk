using System;
using System.Collections.Generic;
using NGame2.NBattleContext;
using NGame2.NUI.NComponent2;
using NShared;
using UnityEngine;

namespace NGame2.NUI.NWindow;

[Serializable]
public class Squard : Squard.IDataProvider
{
	public interface IDataProvider
	{
		EnterDungeonData EnterDungeonData { get; }
	}

	public enum SquardViewType
	{
		Switching,
		ShowAll
	}

	public SquardViewType ViewType;

	public SquardPanel MainSquard;

	public SquardPanel SubSquard;

	public MiniPortraitSquardPanel MiniPortraitSquard;

	public GameObject SquardToggleRoot;

	public List<UILabel> TitleLabelList = new List<UILabel>();

	private MiniPortraitSquardPanel _miniPortraitSquard;

	private EnterDungeonData _enterDungeonData;

	private bool _isSelectMainSquard = true;

	private Action<List<Common.HeroPortraitAnim>, List<PartySlotComponent>, Common.HeroPortraitAnim> _animateSquardPosition;

	EnterDungeonData IDataProvider.EnterDungeonData => _enterDungeonData;

	public void InitSquard(int mainSquardCount, int subSquardCount, HeroPortraitComponent.Listener listener)
	{
		InitSquardPanel(MainSquard, mainSquardCount, listener);
		InitSquardPanel(SubSquard, subSquardCount, listener);
	}

	public void SetMiniPortraitPanel(MiniPortraitSquardPanel miniPortraitSquard)
	{
		_miniPortraitSquard = miniPortraitSquard;
	}

	public void InitMiniPortrait(EnterDungeonData enterDungeonData)
	{
		_enterDungeonData = enterDungeonData;
		SetOnOffSquard(isOnMainSquard: true);
		SetOnOffSquardToggle();
		SwichingSquard(isOnMainSquard: true);
	}

	private void InitSquardPanel(SquardPanel squardPanel, int count, HeroPortraitComponent.Listener listener)
	{
		if (squardPanel != null && count > 0)
		{
			squardPanel.SetSquardPanel(count);
			squardPanel.SetHeroPortraitComponentListener(listener);
		}
	}

	public void InitAnimationSquardPostionAction(Action<List<Common.HeroPortraitAnim>, List<PartySlotComponent>, Common.HeroPortraitAnim> animateSquardPosition)
	{
		_animateSquardPosition = animateSquardPosition;
	}

	private bool IsValidSquardPanel(SquardPanel squardPanel)
	{
		return squardPanel != null;
	}

	public Common.HeroPortraitAnim CreateHeroPortraitAnim(bool isMainSquard, CachedHeroInfo heroInfo, Vector3 position, HeroPortraitComponent.OwnerInfo ownerInfo, bool isControllerable, Action<Common.HeroPortraitAnim> setAction)
	{
		if (isMainSquard)
		{
			return CreateMainHeroPortraitAnim(heroInfo, position, ownerInfo, isControllerable, setAction);
		}
		return CreateSubHeroPortraitAnim(heroInfo, position, ownerInfo, isControllerable, setAction);
	}

	public Common.HeroPortraitAnim CreateMainHeroPortraitAnim(CachedHeroInfo heroInfo, Vector3 position, HeroPortraitComponent.OwnerInfo ownerInfo, bool isControllerable, Action<Common.HeroPortraitAnim> setAction)
	{
		if (!IsValidSquardPanel(MainSquard))
		{
			return null;
		}
		return MainSquard.CreateHeroPortraitAnim(heroInfo, position, ownerInfo, isControllerable, setAction);
	}

	public Common.HeroPortraitAnim CreateSubHeroPortraitAnim(CachedHeroInfo heroInfo, Vector3 position, HeroPortraitComponent.OwnerInfo ownerInfo, bool isControllerable, Action<Common.HeroPortraitAnim> setAction)
	{
		if (!IsValidSquardPanel(SubSquard))
		{
			return null;
		}
		return SubSquard.CreateHeroPortraitAnim(heroInfo, position, ownerInfo, isControllerable, setAction);
	}

	public void RemoveHeroPortraitAnim(HeroPortraitComponent heroDeckPortrait, int heroIndex, long ownerAccontId = 0L)
	{
		if (IsContainSquardHero(heroIndex, ownerAccontId))
		{
			RemoveMainHeroPortraitAnim(heroDeckPortrait, heroIndex, ownerAccontId);
			RemoveSubHeroPortraitAnim(heroDeckPortrait, heroIndex, ownerAccontId);
		}
	}

	private void RemoveMainHeroPortraitAnim(HeroPortraitComponent heroDeckPortrait, int index, long ownerAccontId = 0L)
	{
		if (IsValidSquardPanel(MainSquard))
		{
			MainSquard.RemoveHeroPortraitAnim(heroDeckPortrait, index, ownerAccontId);
		}
	}

	private void RemoveSubHeroPortraitAnim(HeroPortraitComponent heroDeckPortrait, int index, long ownerAccontId = 0L)
	{
		if (IsValidSquardPanel(SubSquard))
		{
			SubSquard.RemoveHeroPortraitAnim(heroDeckPortrait, index, ownerAccontId);
		}
	}

	public void SetMainEmptySlotClassTag(int index, CreatureTagType tagType)
	{
		if (IsValidSquardPanel(MainSquard))
		{
			MainSquard.SetEmptySlotClassTag(index, tagType);
		}
	}

	public List<CreatureTagType> GetLeftClassTagList(bool isMain, List<CreatureTagType> classTagList)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(classTagList))
		{
			return null;
		}
		List<CreatureTagType> list = null;
		if (isMain)
		{
			return MainSquard.GetLeftClassTagList(classTagList);
		}
		return SubSquard.GetLeftClassTagList(classTagList);
	}

	public Vector3 GetMainEmptySlotPositionByIndex(int index)
	{
		return GetEmptySlotPositionByIndex(MainSquard, index);
	}

	public Vector3 GetSubEmptySlotPositionByIndex(int index)
	{
		return GetEmptySlotPositionByIndex(SubSquard, index);
	}

	private Vector3 GetEmptySlotPositionByIndex(SquardPanel squardPanel, int index)
	{
		if (!IsValidSquardPanel(squardPanel))
		{
			return Vector3.zero;
		}
		PartySlotComponent emptySlotByIndex = squardPanel.GetEmptySlotByIndex(index);
		if (emptySlotByIndex == null)
		{
			return Vector3.zero;
		}
		return emptySlotByIndex.transform.position;
	}

	public PartySlotComponent GetMainEmptySlotByIndex(int index)
	{
		return GetEmptySlotByIndex(MainSquard, index);
	}

	public PartySlotComponent GetSubEmptySlotByIndex(int index)
	{
		return GetEmptySlotByIndex(SubSquard, index);
	}

	private PartySlotComponent GetEmptySlotByIndex(SquardPanel squardPanel, int index)
	{
		if (!IsValidSquardPanel(squardPanel))
		{
			return null;
		}
		return squardPanel.GetEmptySlotByIndex(index);
	}

	public UIGrid GetMainEmptySlotGrid()
	{
		return GetEmptySlotGrid(MainSquard);
	}

	public UIGrid GetSubEmptySlotGrid()
	{
		return GetEmptySlotGrid(SubSquard);
	}

	public UIGrid GetEmptySlotGrid(SquardPanel squardPanel)
	{
		if (!IsValidSquardPanel(squardPanel))
		{
			return null;
		}
		return squardPanel.GetEmptySlotGrid();
	}

	public void AnimateSqaurdPosition(Common.HeroPortraitAnim heroPortraitAnim)
	{
		AnimateSquardPosition(MainSquard, heroPortraitAnim);
		AnimateSquardPosition(SubSquard, heroPortraitAnim);
	}

	private void AnimateSquardPosition(SquardPanel squardPanel, Common.HeroPortraitAnim heroPortraitAnim)
	{
		if (IsValidSquardPanel(squardPanel))
		{
			squardPanel.AnimateSqaurdPosition(heroPortraitAnim, _animateSquardPosition);
		}
	}

	public void UpdateHeroPortraitAnim()
	{
		if (IsValidSquardPanel(MainSquard))
		{
			MainSquard.UpdateHeroPortraitAnim();
		}
		if (IsValidSquardPanel(SubSquard))
		{
			SubSquard.UpdateHeroPortraitAnim();
		}
	}

	public int GetSquardSlotCount(bool isMainSquard = true)
	{
		if (!isMainSquard)
		{
			return GetSubSquardSlotCount();
		}
		return GetMainSquardSlotCount();
	}

	public int GetMainSquardSlotCount()
	{
		if (!IsValidSquardPanel(MainSquard))
		{
			return 0;
		}
		return MainSquard.GetSquardSlotCount();
	}

	public int GetSubSquardSlotCount()
	{
		if (!IsValidSquardPanel(SubSquard))
		{
			return 0;
		}
		return SubSquard.GetSquardSlotCount();
	}

	public int GetTotalSquardSlotCount()
	{
		return GetMainSquardSlotCount() + GetSubSquardSlotCount();
	}

	public List<long> GetMainSquardHeroOwnerAccountIdList()
	{
		if (!IsValidSquardPanel(MainSquard))
		{
			return null;
		}
		return MainSquard.GetTotalSquardHeroOwnerAccountIdList();
	}

	public List<long> GetSubSquardHeroOwnerAccountIdList()
	{
		if (!IsValidSquardPanel(SubSquard))
		{
			return null;
		}
		return SubSquard.GetTotalSquardHeroOwnerAccountIdList();
	}

	public List<int> GetMainSquardTotalHeroIndexList()
	{
		if (!IsValidSquardPanel(MainSquard))
		{
			return null;
		}
		return MainSquard.GetTotalSquardHeroIndexList();
	}

	public List<int> GetSubSquardTotalHeroIndexList()
	{
		if (!IsValidSquardPanel(SubSquard))
		{
			return null;
		}
		return SubSquard.GetTotalSquardHeroIndexList();
	}

	public List<int> GetTotalHeroIndexList()
	{
		List<int> list = new List<int>();
		if (IsValidSquardPanel(MainSquard))
		{
			MainSquard.AddTotalSquardHeroIndexList(list);
		}
		if (IsValidSquardPanel(SubSquard))
		{
			SubSquard.AddTotalSquardHeroIndexList(list);
		}
		return list;
	}

	public List<int> GetMainSquardMyHeroIndexList()
	{
		if (!IsValidSquardPanel(MainSquard))
		{
			return null;
		}
		return MainSquard.GetSquardMyHeroIndexList();
	}

	public List<int> GetSubSquardMyHeroIndexList()
	{
		if (!IsValidSquardPanel(SubSquard))
		{
			return null;
		}
		return SubSquard.GetSquardMyHeroIndexList();
	}

	public bool IsSelectedMainSquard()
	{
		return _isSelectMainSquard;
	}

	public List<Common.HeroPortraitAnim> GetSquardHeroPortraitAnimListByAccountId(long accountId)
	{
		List<Common.HeroPortraitAnim> list = new List<Common.HeroPortraitAnim>();
		if (IsValidSquardPanel(MainSquard))
		{
			List<Common.HeroPortraitAnim> heroPortraitAnimListByAccountId = MainSquard.GetHeroPortraitAnimListByAccountId(accountId);
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(heroPortraitAnimListByAccountId))
			{
				list.AddRange(heroPortraitAnimListByAccountId);
			}
		}
		if (IsValidSquardPanel(SubSquard))
		{
			List<Common.HeroPortraitAnim> heroPortraitAnimListByAccountId2 = SubSquard.GetHeroPortraitAnimListByAccountId(accountId);
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(heroPortraitAnimListByAccountId2))
			{
				list.AddRange(heroPortraitAnimListByAccountId2);
			}
		}
		return list;
	}

	public BattlePartySetting.BattlePartyInfo GetBattlePartyInfo()
	{
		BattlePartySetting.BattlePartyInfo battlePartyInfo = new BattlePartySetting.BattlePartyInfo();
		List<int> mainSquardTotalHeroIndexList = GetMainSquardTotalHeroIndexList();
		List<int> subSquardTotalHeroIndexList = GetSubSquardTotalHeroIndexList();
		List<long> mainSquardHeroOwnerAccountIdList = GetMainSquardHeroOwnerAccountIdList();
		List<long> subSquardHeroOwnerAccountIdList = GetSubSquardHeroOwnerAccountIdList();
		int num = __Extension_Method_ICollection__.COUNT(mainSquardTotalHeroIndexList) + __Extension_Method_ICollection__.COUNT(subSquardTotalHeroIndexList);
		battlePartyInfo.heroIndices = new int[num];
		battlePartyInfo.accountIds = new long[num];
		for (int i = 0; i < num; i++)
		{
			int num2 = 0;
			long num3 = 0L;
			if (__Extension_Method_ICollection__.COUNT(mainSquardTotalHeroIndexList) > i)
			{
				num2 = mainSquardTotalHeroIndexList[i];
				num3 = mainSquardHeroOwnerAccountIdList[i];
			}
			else
			{
				int index = i - __Extension_Method_ICollection__.COUNT(mainSquardTotalHeroIndexList);
				num2 = subSquardTotalHeroIndexList[index];
				num3 = subSquardHeroOwnerAccountIdList[index];
			}
			battlePartyInfo.accountIds[i] = num3;
			battlePartyInfo.heroIndices[i] = num2;
		}
		battlePartyInfo.subHeroIndices = subSquardTotalHeroIndexList.ToArray();
		return battlePartyInfo;
	}

	public Common.HeroPortraitAnim GetSquardHeroProtraitAnim(int heroIndex, long ownerAccountId = 0L, bool ignoreSelection = false)
	{
		Common.HeroPortraitAnim heroPortraitAnim = null;
		if (ignoreSelection)
		{
			if (IsValidSquardPanel(MainSquard))
			{
				heroPortraitAnim = MainSquard.GetHeroPortraitAnim(heroIndex, ownerAccountId);
			}
			if (heroPortraitAnim == null && IsValidSquardPanel(SubSquard))
			{
				heroPortraitAnim = SubSquard.GetHeroPortraitAnim(heroIndex, ownerAccountId);
			}
		}
		else
		{
			if (IsValidSquardPanel(MainSquard) && _isSelectMainSquard)
			{
				heroPortraitAnim = MainSquard.GetHeroPortraitAnim(heroIndex, ownerAccountId);
			}
			if (heroPortraitAnim == null && IsValidSquardPanel(SubSquard) && !_isSelectMainSquard)
			{
				heroPortraitAnim = SubSquard.GetHeroPortraitAnim(heroIndex, ownerAccountId);
			}
		}
		return heroPortraitAnim;
	}

	public List<Common.HeroPortraitAnim> GetAllHeroPortraitAnimList()
	{
		List<Common.HeroPortraitAnim> list = new List<Common.HeroPortraitAnim>();
		if (IsValidSquardPanel(MainSquard))
		{
			List<Common.HeroPortraitAnim> allHeroPortraitAnim = MainSquard.GetAllHeroPortraitAnim();
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(allHeroPortraitAnim))
			{
				list.AddRange(allHeroPortraitAnim);
			}
		}
		if (IsValidSquardPanel(SubSquard))
		{
			List<Common.HeroPortraitAnim> allHeroPortraitAnim2 = SubSquard.GetAllHeroPortraitAnim();
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(allHeroPortraitAnim2))
			{
				list.AddRange(allHeroPortraitAnim2);
			}
		}
		return list;
	}

	public Vector3 GetSquardPanelPosition()
	{
		UIPanel uIPanel = null;
		if (_isSelectMainSquard && IsValidSquardPanel(MainSquard))
		{
			uIPanel = MainSquard.GetPanelSquard();
		}
		if (!_isSelectMainSquard && IsValidSquardPanel(SubSquard))
		{
			uIPanel = SubSquard.GetPanelSquard();
		}
		if (!(uIPanel == null))
		{
			return uIPanel.transform.position;
		}
		return Vector3.zero;
	}

	public bool IsContainSquardHero(int heroIndex, long ownerAccountId = 0L)
	{
		bool flag = IsContainSquardHero(MainSquard, heroIndex, ownerAccountId);
		if (!flag)
		{
			flag = IsContainSquardHero(SubSquard, heroIndex, ownerAccountId);
		}
		return flag;
	}

	private bool IsContainSquardHero(SquardPanel squardPanel, int heroIndex, long ownerAccountId = 0L)
	{
		if (IsValidSquardPanel(squardPanel) && squardPanel.GetHeroPortraitAnim(heroIndex, ownerAccountId) != null)
		{
			return true;
		}
		return false;
	}

	private void SetOnOffSquardToggle()
	{
		bool enable = MainSquard != null && SubSquard != null && SquardToggleRoot != null;
		UIHelper.EnableGameObject(SquardToggleRoot, enable);
	}

	private void SetOnOffSquard(bool isOnMainSquard)
	{
		_isSelectMainSquard = isOnMainSquard;
		switch (ViewType)
		{
		case SquardViewType.ShowAll:
			if (IsValidSquardPanel(MainSquard))
			{
				UIHelper.EnableGameObject(MainSquard.Root, enable: true);
			}
			if (IsValidSquardPanel(SubSquard))
			{
				UIHelper.EnableGameObject(SubSquard.Root, enable: true);
			}
			break;
		case SquardViewType.Switching:
			if (IsValidSquardPanel(MainSquard))
			{
				UIHelper.EnableGameObject(MainSquard.Root, isOnMainSquard);
			}
			if (IsValidSquardPanel(SubSquard))
			{
				UIHelper.EnableGameObject(SubSquard.Root, !isOnMainSquard);
			}
			break;
		}
	}

	public void CloseSquardTab()
	{
		UIHelper.EnableGameObject(SquardToggleRoot, enable: false);
	}

	public void Release()
	{
		if (IsValidSquardPanel(MainSquard))
		{
			MainSquard.Release();
		}
		if (IsValidSquardPanel(SubSquard))
		{
			SubSquard.Release();
		}
		if (_miniPortraitSquard != null)
		{
			_miniPortraitSquard.Release();
			_miniPortraitSquard.CloseRoot();
			_miniPortraitSquard = null;
		}
		CloseSquardTab();
	}

	public void SwichingSquard(bool isOnMainSquard)
	{
		if (IsValidSquardPanel(MainSquard) && IsValidSquardPanel(SubSquard))
		{
			SetOnOffSquard(isOnMainSquard);
			SetMiniPortrait(isOnMainSquard);
			SetFocusingObject(isOnMainSquard);
		}
	}

	private void SetFocusingObject(bool isOnMainSquard)
	{
		if (IsValidSquardPanel(MainSquard))
		{
			MainSquard.SetFocusingObject(isOnMainSquard);
		}
		if (IsValidSquardPanel(SubSquard))
		{
			SubSquard.SetFocusingObject(!isOnMainSquard);
		}
	}

	private void SetMiniPortrait(bool isOnMainSquard)
	{
		if (_miniPortraitSquard != null)
		{
			_miniPortraitSquard.SetOnOffRoot(isOnMainSquard);
			_miniPortraitSquard.SetToggleValue(isOnMainSquard);
			_miniPortraitSquard.SetMiniPortrait(isOnMainSquard ? SubSquard : MainSquard, this);
		}
	}

	public bool GetMiniPortraitMainToggleValue()
	{
		if (_miniPortraitSquard == null)
		{
			return false;
		}
		return _miniPortraitSquard.GetMainToggleValue();
	}

	public bool GetMiniPortraitSubToggleValue()
	{
		if (_miniPortraitSquard == null)
		{
			return false;
		}
		return _miniPortraitSquard.GetSubToggleValue();
	}

	public HeroPortraitComponent GetMiniSquardHeroProtrait(int heroIndex)
	{
		if (_miniPortraitSquard == null)
		{
			return null;
		}
		return _miniPortraitSquard.GetMiniPortrait(heroIndex);
	}

	public bool IsSquardSwitchButtonOn()
	{
		if (ViewType == SquardViewType.ShowAll)
		{
			return false;
		}
		if (_isSelectMainSquard)
		{
			return MainSquard.ButtonSquardChange != null;
		}
		return SubSquard.ButtonSquardChange != null;
	}

	public void UpdateSquardPortraitAnimDepth()
	{
		if (IsValidSquardPanel(MainSquard))
		{
			MainSquard.UpdateSquardPortraitAnimDepth();
		}
		if (IsValidSquardPanel(SubSquard))
		{
			SubSquard.UpdateSquardPortraitAnimDepth();
		}
	}
}
