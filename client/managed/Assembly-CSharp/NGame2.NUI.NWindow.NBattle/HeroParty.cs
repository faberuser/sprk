using System.Collections.Generic;
using NGame2.NBattleField;
using NGame2.NCutScene;
using NGame2.NEventTrigger;
using NGame2.NUI.NComponent2.NBattle;
using NShared;
using NShared.NEvent;
using NVespa;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow.NBattle;

public class HeroParty : BaseWindowUpdater
{
	public interface Listener : PartySlotComponent2.Listener
	{
		void InitSlot();
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Battle/HeroParty";

	public UISimpleGrid PartyGrid;

	public UISimpleGrid SubPartyGrid;

	public UITweener Tween_Party;

	public UITweener Tween_SubParty;

	public UIPanel Panel_Party;

	public UIPanel Panel_SubParty;

	public UIGame3DWidget Game3DWidget;

	private BaseSkillComponent.ClickIconCallback _onClickSkill;

	private long _playerId;

	private Listener _listener;

	private bool _autoPlayerable;

	private int _chapterIndex;

	private int _dungeonIndex;

	private int _mainPanelDepth;

	private int _subPanelDepth;

	private bool _enabledSwapParty;

	private bool _isMain = true;

	private bool _useFocusingEffect;

	private bool _enableManaAddictMode;

	private IncreaseDic<int, PartySlotComponent2> _partySlotDic = new IncreaseDic<int, PartySlotComponent2>();

	private IncreaseDic<int, PartySlotComponent2> _subPartySlotDic = new IncreaseDic<int, PartySlotComponent2>();

	private List<int> _removeUidList = new List<int>();

	private List<int> _tempRemoveSubUidList = new List<int>();

	public bool EnabledSwapParty => _enabledSwapParty;

	public bool IsSelectedMain => _isMain;

	public IncreaseDic<int, PartySlotComponent2> PartySlotDic => _partySlotDic;

	public IncreaseDic<int, PartySlotComponent2> SubPartySlotDic => _subPartySlotDic;

	public void Open(long playerId, BaseSkillComponent.ClickIconCallback onClickSkill, bool autoPlayerable, Listener listener, int chapterIndex, int dungeonIndex, bool enabledSwapParty)
	{
		SetUpdatePeriod(0.1f);
		Open(delegate
		{
			Init(playerId, onClickSkill, autoPlayerable, listener, chapterIndex, dungeonIndex, enabledSwapParty);
		}, null);
	}

	public void Init(long playerId, BaseSkillComponent.ClickIconCallback onClickSkill, bool autoPlayerable, Listener listener, int chapterIndex, int dungeonIndex, bool enabledSwapParty)
	{
		_playerId = playerId;
		_onClickSkill = onClickSkill;
		_listener = listener;
		_autoPlayerable = autoPlayerable;
		_chapterIndex = chapterIndex;
		_dungeonIndex = dungeonIndex;
		_enabledSwapParty = enabledSwapParty;
		_isMain = true;
		_mainPanelDepth = Panel_Party.depth;
		_subPanelDepth = Panel_SubParty.depth;
		FocusingHeroParty triggerEvent = new FocusingHeroParty
		{
			ChapterIndex = chapterIndex,
			DungeonIndex = dungeonIndex
		};
		_useFocusingEffect = SingletonAuto<EventTriggerManager>.instance.CanRaiseEvent(triggerEvent, ignoreCurrent: true);
		if (Game3DWidget != null)
		{
			UIHelper.EnableGameObject(Game3DWidget, _useFocusingEffect);
			Game3DWidget.Play(null);
		}
		else
		{
			_useFocusingEffect = false;
		}
		InitHeroParty();
	}

	public void InitHeroParty()
	{
		InitHeroSwapParty();
	}

	public override void Reset()
	{
		base.Reset();
		ClearAll();
		ResetSwapTween();
		_playerId = 0L;
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		UpdateSlot();
	}

	private void UpdateSlot()
	{
		if (!IsValidBattle())
		{
			return;
		}
		if (_isMain)
		{
			for (int i = 0; i < _partySlotDic.Count; i++)
			{
				_partySlotDic.GetValueByIndex(i).ChainUpdate();
			}
		}
		else
		{
			for (int j = 0; j < _subPartySlotDic.Count; j++)
			{
				_subPartySlotDic.GetValueByIndex(j).ChainUpdate();
			}
		}
		if (_useFocusingEffect && SceneAutoBehaviour<CutSceneManager>.isValidInstance)
		{
			if (SceneAutoBehaviour<CutSceneManager>.instance.IsPlaying && Game3DWidget.IsPlaying)
			{
				Game3DWidget.Stop(immediatelyClear: true);
			}
			else if (!SceneAutoBehaviour<CutSceneManager>.instance.IsPlaying && !Game3DWidget.IsPlaying)
			{
				Game3DWidget.Play(null);
			}
		}
	}

	private bool IsValidBattle()
	{
		if (!SingletonAuto<BattleFacade>.instance.IsValidBattle())
		{
			return false;
		}
		if (_playerId == 0L)
		{
			return false;
		}
		return true;
	}

	public override void UpdatePeriodically()
	{
		base.UpdatePeriodically();
		UpdateParty();
		UpdateSkillIcon();
	}

	private void UpdateParty()
	{
		if (IsValidBattle())
		{
			_removeUidList.Clear();
			if (__Extension_Method_ICollection__.COUNT(_partySlotDic) != 0)
			{
				_removeUidList.AddRange(_partySlotDic._dic.Keys);
			}
			_tempRemoveSubUidList.Clear();
			if (__Extension_Method_ICollection__.COUNT(_subPartySlotDic) != 0)
			{
				_tempRemoveSubUidList.AddRange(_subPartySlotDic._dic.Keys);
			}
			bool flag = false;
			bool flag2 = false;
			if (CreatePartyComponent())
			{
				flag2 = true;
				flag = true;
			}
			if (RemovePartyComponent())
			{
				flag2 = true;
			}
			if (flag2)
			{
				Reposition();
			}
			if (flag)
			{
				_listener.InitSlot();
			}
		}
	}

	private bool CreatePartyComponent()
	{
		for (int i = 0; i < SingletonAuto<BattleFacade>.instance.GetCreatureCount(); i++)
		{
			BattleFacade.CreatureCacheData creatureCacheByIndex = SingletonAuto<BattleFacade>.instance.GetCreatureCacheByIndex(i);
			int uid = creatureCacheByIndex.Uid;
			if (!IsDisplayCreature(creatureCacheByIndex))
			{
				continue;
			}
			if (_partySlotDic.ContainsKey(uid))
			{
				_removeUidList.Remove(uid);
				continue;
			}
			if (_subPartySlotDic.ContainsKey(uid))
			{
				_tempRemoveSubUidList.Remove(uid);
				continue;
			}
			int groupIndex = creatureCacheByIndex.CreateInfo.GroupIndex;
			if (groupIndex == 0)
			{
				PartySlotComponent2 partySlotComponent = UIHelper.AddGridItem(PartyGrid, PartySlotComponent2.PrefabAuto);
				if (!partySlotComponent.Open(uid, _onClickSkill, _listener, _autoPlayerable, groupIndex, _enableManaAddictMode))
				{
					FactoryBase.ReleaseComponent(partySlotComponent);
					continue;
				}
				_partySlotDic.Add(uid, partySlotComponent);
			}
			else
			{
				PartySlotComponent2 partySlotComponent = UIHelper.AddGridItem(SubPartyGrid, PartySlotComponent2.PrefabAuto);
				if (!partySlotComponent.Open(uid, _onClickSkill, _listener, _autoPlayerable, groupIndex, _enableManaAddictMode))
				{
					FactoryBase.ReleaseComponent(partySlotComponent);
					continue;
				}
				_subPartySlotDic.Add(uid, partySlotComponent);
			}
			return true;
		}
		return false;
	}

	private bool RemovePartyComponent()
	{
		bool result = false;
		if (_isMain)
		{
			for (int i = 0; i < _partySlotDic.Count; i++)
			{
				PartySlotComponent2 valueByIndex = _partySlotDic.GetValueByIndex(i);
				if (!IsDisplayCreature(valueByIndex.CreatureCache) || _removeUidList.Contains(valueByIndex.CreatureCache.Uid))
				{
					_partySlotDic.RemoveByIndex(i);
					i--;
					valueByIndex.Close();
					FactoryBase.ReleaseComponent(valueByIndex);
					result = true;
				}
			}
		}
		else
		{
			for (int j = 0; j < _subPartySlotDic.Count; j++)
			{
				PartySlotComponent2 valueByIndex2 = _subPartySlotDic.GetValueByIndex(j);
				if (!IsDisplayCreature(valueByIndex2.CreatureCache) || _tempRemoveSubUidList.Contains(valueByIndex2.CreatureCache.Uid))
				{
					_subPartySlotDic.RemoveByIndex(j);
					j--;
					valueByIndex2.Close();
					FactoryBase.ReleaseComponent(valueByIndex2);
					result = true;
				}
			}
		}
		return result;
	}

	private void ClearAll()
	{
		ClearParty(ref _partySlotDic, PartyGrid);
		ClearParty(ref _subPartySlotDic, SubPartyGrid);
	}

	private void Reposition()
	{
		Reposition(ref _partySlotDic, PartyGrid);
		Reposition(ref _subPartySlotDic, SubPartyGrid);
	}

	private void UpdateSkillIcon()
	{
		if (!_enabledSwapParty)
		{
			UpdateSkillIcon(ref _partySlotDic);
		}
		else if (_isMain)
		{
			UpdateSkillIcon(ref _partySlotDic);
		}
		else
		{
			UpdateSkillIcon(ref _subPartySlotDic);
		}
	}

	private int ComparePartSlotPosition(PartySlotComponent2 leftSlot, PartySlotComponent2 rightSlot)
	{
		if (leftSlot == null || rightSlot == null)
		{
			return 0;
		}
		if (!SingletonAuto<BattleFacade>.instance.IsValidBattle())
		{
			return 0;
		}
		long value = 0L;
		long num = 0L;
		BattleFacade.CreatureCacheData creatureCache = SingletonAuto<BattleFacade>.instance.GetCreatureCache(leftSlot.CreatureUid);
		if (creatureCache.Uid != 0)
		{
			value = creatureCache.CreateInfo.GetWavePositionScore();
		}
		BattleFacade.CreatureCacheData creatureCache2 = SingletonAuto<BattleFacade>.instance.GetCreatureCache(rightSlot.CreatureUid);
		if (creatureCache2.Uid != 0)
		{
			num = creatureCache2.CreateInfo.GetWavePositionScore();
		}
		return num.CompareTo(value);
	}

	private bool IsDisplayCreature(BattleFacade.CreatureCacheData creatureCache)
	{
		if (creatureCache.Uid == 0)
		{
			return false;
		}
		if (creatureCache.Player.Id != _playerId)
		{
			return false;
		}
		if (creatureCache.CreateInfo.IsAi)
		{
			return false;
		}
		return true;
	}

	public bool IsShowingTooltip()
	{
		if (IsShowingTooltip(ref _partySlotDic) || IsShowingTooltip(ref _subPartySlotDic))
		{
			return true;
		}
		return false;
	}

	public void SetHighestTpsEffect(int uId)
	{
		if (_isMain)
		{
			for (int i = 0; i < _partySlotDic.Count; i++)
			{
				PartySlotComponent2 valueByIndex = _partySlotDic.GetValueByIndex(i);
				valueByIndex.SetTpsAnimation(valueByIndex.CreatureUid == uId);
			}
		}
		else
		{
			for (int j = 0; j < _subPartySlotDic.Count; j++)
			{
				PartySlotComponent2 valueByIndex2 = _subPartySlotDic.GetValueByIndex(j);
				valueByIndex2.SetTpsAnimation(valueByIndex2.CreatureUid == uId);
			}
		}
	}

	private void ClearParty(ref IncreaseDic<int, PartySlotComponent2> partySlotDic, UISimpleGrid partyGrid)
	{
		if (partySlotDic != null && !(partyGrid == null))
		{
			for (int i = 0; i < partySlotDic.Count; i++)
			{
				PartySlotComponent2 valueByIndex = partySlotDic.GetValueByIndex(i);
				valueByIndex.Close();
				FactoryBase.ReleaseComponent(valueByIndex);
			}
			partySlotDic.Clear();
			if (partyGrid != null)
			{
				partyGrid.ClearGridItem<PartySlotComponent2>();
			}
		}
	}

	private void Reposition(ref IncreaseDic<int, PartySlotComponent2> partySlotDic, UISimpleGrid partyGrid)
	{
		if (partySlotDic.IsNullOrEmpty())
		{
			return;
		}
		partySlotDic.SortByValue(ComparePartSlotPosition);
		CampaignDungeonData data = GameRoot.Instance.CampaignDungeonDataContainer.GetData(_chapterIndex, _dungeonIndex);
		for (int i = 0; i < partySlotDic.Count; i++)
		{
			PartySlotComponent2 valueByIndex = partySlotDic.GetValueByIndex(i);
			valueByIndex.name = $"SlotOrder.{partySlotDic.Count - i}";
			if (data != null)
			{
				valueByIndex.SetSlotIndex(i, data);
			}
		}
		UIHelper.RepositionGrid(partyGrid);
	}

	private void UpdateSkillIcon(ref IncreaseDic<int, PartySlotComponent2> partySlotDic)
	{
		if (!partySlotDic.IsNullOrEmpty())
		{
			for (int i = 0; i < partySlotDic.Count; i++)
			{
				partySlotDic.GetValueByIndex(i).UpdateSkillIcon();
			}
		}
	}

	private bool IsShowingTooltip(ref IncreaseDic<int, PartySlotComponent2> partySlotDic)
	{
		for (int i = 0; i < partySlotDic.Count; i++)
		{
			if (partySlotDic.GetValueByIndex(i).IsShowingTooltip())
			{
				return true;
			}
		}
		return false;
	}

	public void InitHeroSwapParty()
	{
		UIHelper.EnableGameObject(SubPartyGrid, _enabledSwapParty);
		SwapHeroParty(isMain: true, isInit: true);
	}

	public void OnClickSwapHeroParty()
	{
		SwapHeroParty(!_isMain);
	}

	public void SwapHeroParty(bool isMain, bool isInit = false)
	{
		if (!_enabledSwapParty || (!isInit && _isMain == isMain))
		{
			return;
		}
		_isMain = isMain;
		if (isMain)
		{
			Panel_Party.depth = _mainPanelDepth;
			Panel_SubParty.depth = _subPanelDepth;
			if (Tween_Party != null)
			{
				Tween_Party.PlayForward();
			}
			if (Tween_SubParty != null)
			{
				Tween_SubParty.PlayReverse();
			}
		}
		else
		{
			Panel_SubParty.depth = _mainPanelDepth;
			Panel_Party.depth = _subPanelDepth;
			if (Tween_SubParty != null)
			{
				Tween_SubParty.PlayForward();
			}
			if (Tween_Party != null)
			{
				Tween_Party.PlayReverse();
			}
		}
	}

	private void ResetSwapTween()
	{
		if (Tween_Party != null)
		{
			Tween_Party.SampleAndStop(1f);
		}
		if (Tween_SubParty != null)
		{
			Tween_SubParty.SampleAndStop(0f);
		}
	}

	public void SetManaAddict(bool value)
	{
		_enableManaAddictMode = value;
	}
}
