using System.Collections;
using System.Collections.Generic;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class GuildArenaDeckComponent : LinkFactoryComponent, InitOnCreate, HeroPortraitComponent.Listener
{
	public interface IListener
	{
		void OnClickStartBattle(GuildArenaDeckInfo deckInfo, CachedHeroInfo[] heroInfos);

		void OnClickDeck(GuildArenaDeckComponent component);

		void OnClickDeckDetailInfo(CachedHeroInfo[] heroInfos, string titleName);

		void OnClickDeckHeroPortrait(HeroPortraitComponent component);
	}

	public enum ComponentType
	{
		DeckSetting,
		OnAttack
	}

	public const string PrefabPath = "Prefab2/Component/GuildArena/GuildArenaDeckComponent";

	public const string PrefabPath2 = "Prefab2/Component/GuildArena/GuildArenaDeckComponentOnAttack";

	public static PrefabAuto<GuildArenaDeckComponent> PrefabAuto = new PrefabAuto<GuildArenaDeckComponent>("Prefab2/Component/GuildArena/GuildArenaDeckComponent");

	public static PrefabAuto<GuildArenaDeckComponent> PrefabAuto2 = new PrefabAuto<GuildArenaDeckComponent>("Prefab2/Component/GuildArena/GuildArenaDeckComponentOnAttack");

	public UISimpleGrid Grid_HeroIcon;

	public UISimpleGrid Grid_BuffIcon;

	public GameObject FocusRoot;

	private IListener _listener;

	public List<CachedHeroInfo> CachedHeroInfos = new List<CachedHeroInfo>();

	private List<HeroPortraitComponent> PortraitComponents = new List<HeroPortraitComponent>();

	private List<SkillIconComponent> BuffComponents = new List<SkillIconComponent>();

	public UILabel Label_TeamName;

	public UIButton Button_Attack;

	public UIButton Button_DetailInfoSmall;

	public UIButton Button_DetailInfoLong;

	public GameObject InBattleRoot;

	public GameObject ForecAttackRoot;

	public GameObject FocusEffectRoot;

	public GameObject BG_Long;

	public GameObject BG_Long2;

	public GameObject BGDisable_Short;

	public UILabel Label_DisableReason;

	public GameObject BGDisable_Long;

	private ComponentType _componentType;

	[SerializeField]
	private int _deckIndex;

	private int _skillIndex;

	private int[] _skillIndices = new int[5] { 60000001, 60000002, 60000003, 60000004, 60000005 };

	private GuildArenaDeckInfo _deckInfo;

	private TeamType _teamType;

	[HideInInspector]
	public int DeckIndex
	{
		get
		{
			return _deckIndex;
		}
		set
		{
			_deckIndex = value;
		}
	}

	public void Init(int deckIndex, int slotCount, IListener listener, TeamType teamType, ComponentType componentSize = ComponentType.DeckSetting)
	{
		UIHelper.SetWidgetGrayScale(base.gameObject.GetComponent<UIWidget>(), grayScale: false, setColliders: true);
		_listener = listener;
		_componentType = componentSize;
		_deckIndex = deckIndex;
		_teamType = teamType;
		SetFocus(focus: false);
		InitBuffIcon();
		CreateHeroIcons(slotCount);
		InitBG();
		InitLabels();
		InitAttackButton();
		InitInfoButton();
	}

	public void SetDeckInfo(GuildArenaDeckInfo deckInfo)
	{
		_deckInfo = null;
		_deckInfo = deckInfo;
		UIHelper.SetWidgetGrayScale(base.gameObject.GetComponent<UIWidget>(), grayScale: false, setColliders: true);
		SetDefeatDeck();
		SetHeros();
		SetDummyHeros();
		SetBlindDeck();
		SetBattleButtons();
	}

	private void SetBattleButtons()
	{
		if (ForecAttackRoot != null)
		{
			UIHelper.EnableCollidersInChildren(ForecAttackRoot.GetComponent<UIWidget>(), _deckInfo.IsForceAttack == 1);
		}
		UIHelper.EnableGameObject(InBattleRoot, _deckInfo.IsBattle == 1 && _teamType == TeamType.Right);
		UIHelper.EnableGameObject(ForecAttackRoot, _deckInfo.IsBattle != 1 && _deckInfo.IsForceAttack == 1 && _teamType == TeamType.Right);
		UIHelper.EnableGameObject(Button_Attack.gameObject, _deckInfo.IsBattle != 1 && _deckInfo.IsForceAttack != 1 && _teamType == TeamType.Right);
	}

	private void SetHeros()
	{
		if (_deckInfo.CachedHeroInfos != null)
		{
			CachedHeroInfo[] cachedHeroInfos = _deckInfo.CachedHeroInfos;
			foreach (CachedHeroInfo heroInfo in cachedHeroInfos)
			{
				GetEmptySlot().ResetPortrait(heroInfo, null);
			}
		}
	}

	private void SetDummyHeros()
	{
		if (_deckInfo.IsNPC != 1)
		{
			return;
		}
		foreach (HeroPortraitComponent portraitComponent in PortraitComponents)
		{
			portraitComponent.SetDummy(enable: true);
		}
	}

	private bool SetDefeatDeck()
	{
		if (_deckInfo.IsLose == 1)
		{
			SetHeroPortraitDefeat();
			UIHelper.EnableCollidersInChildren(this, isEnable: false);
			SetDisableBG();
			VespaCoroutine.Start(RefreshRoot());
			return true;
		}
		return false;
	}

	private void SetBlindDeck()
	{
		if (_teamType != TeamType.Left && _deckInfo.IsBlind == 1)
		{
			SetHeroPortraitBlind();
		}
	}

	private IEnumerator RefreshRoot()
	{
		for (int i = 0; i < 5; i++)
		{
			UIHelper.EnableGameObject(base.gameObject, enable: false);
			UIHelper.EnableGameObject(base.gameObject, enable: true);
			yield return null;
		}
	}

	private void SetHeroPortraitDefeat()
	{
		foreach (HeroPortraitComponent portraitComponent in PortraitComponents)
		{
			portraitComponent.SetBan(ban: true);
		}
	}

	private void SetHeroPortraitBlind()
	{
		foreach (HeroPortraitComponent portraitComponent in PortraitComponents)
		{
			portraitComponent.SetBlind(enable: true);
		}
	}

	public void SetDisableBG()
	{
		UIHelper.EnableGameObject(BGDisable_Short, _componentType == ComponentType.DeckSetting);
		UIHelper.EnableGameObject(BGDisable_Long, _componentType == ComponentType.OnAttack);
		switch (_deckIndex)
		{
		default:
			UIHelper.SetLabelText(Label_DisableReason, "");
			break;
		case 4:
			UIHelper.SetLabelText(Label_DisableReason, LocalizationManager.GetStringDirect("GUILD_ARENA_DEFENCE_SETTING_LIMIT_1"));
			break;
		case 5:
			UIHelper.SetLabelText(Label_DisableReason, LocalizationManager.GetStringDirect("GUILD_ARENA_DEFENCE_SETTING_LIMIT_2"));
			break;
		}
	}

	public void InitBG()
	{
		UIHelper.EnableGameObject(BG_Long, _componentType == ComponentType.OnAttack && _teamType == TeamType.Left);
		UIHelper.EnableGameObject(BG_Long2, _componentType == ComponentType.OnAttack && _teamType == TeamType.Right);
		UIHelper.EnableGameObject(BGDisable_Short, enable: false);
		UIHelper.EnableGameObject(BGDisable_Long, enable: false);
	}

	public void InitLabels()
	{
		UIHelper.SetLabelText(Label_TeamName, LocalizationManager.GetStringDirect(string.Format("{0}{1}", "DEFENCE_TEAM_NUM", _deckIndex)));
	}

	public void InitBuffIcon()
	{
		ClearBuffIcon();
		int[] array = GameRoot.Instance.ConstantDataContainer.GetValueArray<int>("GuildArenaDefenceSkillIndices");
		if (array == null)
		{
			array = _skillIndices;
		}
		if (array.Length > _deckIndex - 1)
		{
			SkillIconComponent skillIconComponent = UIHelper.AddGridItem(Grid_BuffIcon, SkillIconComponent.PrefabAuto);
			skillIconComponent.Open(array[_deckIndex - 1]);
			BuffComponents.Add(skillIconComponent);
		}
	}

	private void CreateHeroIcons(int slotCount)
	{
		ClearHeorIcons();
		for (int i = 0; i < slotCount; i++)
		{
			if (GameRoot.Instance.CreatureDataContainer.GetData(i + 1) == null)
			{
				VespaDebug.LogError("CretureData is null : heroIndex=" + i);
				return;
			}
			CachedHeroInfo cachedHeroInfo = null;
			HeroPortraitComponent heroPortraitComponent = UIHelper.AddGridItem(Grid_HeroIcon, HeroPortraitComponent.SmallPrefabAuto);
			heroPortraitComponent.Open(cachedHeroInfo, this, null, HeroPortraitComponent.EPortraitMode.Icon);
			heroPortraitComponent.gameObject.SetActive(value: true);
			PortraitComponents.Add(heroPortraitComponent);
			CachedHeroInfos.Add(cachedHeroInfo);
		}
		UIHelper.RepositionGrid(Grid_HeroIcon);
	}

	private void ClearHeorIcons()
	{
		FactoryBase.ReleaseAndClearList(PortraitComponents);
		CachedHeroInfos.Clear();
	}

	private void ClearBuffIcon()
	{
		FactoryBase.ReleaseAndClearList(BuffComponents);
	}

	public void InitAttackButton()
	{
		UIHelper.EnableGameObject(Button_Attack, _teamType == TeamType.Right);
		UIHelper.EnableGameObject(InBattleRoot, enable: false);
	}

	public void InitInfoButton()
	{
		UIHelper.EnableGameObject(Button_DetailInfoSmall, _teamType == TeamType.Left && _componentType == ComponentType.DeckSetting);
		UIHelper.EnableGameObject(Button_DetailInfoLong, _teamType == TeamType.Left && _componentType == ComponentType.OnAttack);
	}

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		UIHelper.SetWidgetGrayScale(base.gameObject.GetComponent<UIWidget>(), grayScale: false, setColliders: true);
		ClearHeorIcons();
		_deckInfo = null;
	}

	public int[] GetHeroIndices()
	{
		List<int> list = new List<int>();
		foreach (HeroPortraitComponent portraitComponent in PortraitComponents)
		{
			if (!(portraitComponent == null) && portraitComponent.HeroInfo != null)
			{
				list.Add(portraitComponent.HeroInfo.HeroIndex);
			}
		}
		return list.ToArray();
	}

	public void SetFocus(bool focus)
	{
		UIHelper.EnableGameObject(FocusRoot, focus);
	}

	public void SetFocusEffect(bool enable)
	{
		if (!enable || HasEmptySlot())
		{
			UIHelper.EnableGameObject(FocusEffectRoot, enable);
		}
	}

	public bool HasEmptySlot()
	{
		foreach (HeroPortraitComponent portraitComponent in PortraitComponents)
		{
			if (portraitComponent.HeroInfo == null)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasComponent(HeroPortraitComponent heroPortrait)
	{
		foreach (HeroPortraitComponent portraitComponent in PortraitComponents)
		{
			if (portraitComponent == heroPortrait)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsSetHero(HeroPortraitComponent heroPortrait)
	{
		foreach (HeroPortraitComponent portraitComponent in PortraitComponents)
		{
			if (portraitComponent.HeroInfo != null && heroPortrait.HeroInfo != null && portraitComponent.HeroInfo.HeroIndex == heroPortrait.HeroInfo.HeroIndex)
			{
				return true;
			}
		}
		return false;
	}

	public void OnClickDeck()
	{
		if (_listener != null)
		{
			_listener.OnClickDeck(this);
		}
	}

	public void OnClickDeckDetailInfo()
	{
		if (_listener == null)
		{
			return;
		}
		List<CachedHeroInfo> list = new List<CachedHeroInfo>();
		foreach (HeroPortraitComponent portraitComponent in PortraitComponents)
		{
			if (portraitComponent.HeroInfo != null)
			{
				list.Add(portraitComponent.HeroInfo);
			}
		}
		_listener.OnClickDeckDetailInfo(list.ToArray(), Label_TeamName.text);
	}

	public void OnClickStartBattle()
	{
		if (_listener == null)
		{
			return;
		}
		List<CachedHeroInfo> list = new List<CachedHeroInfo>();
		foreach (HeroPortraitComponent portraitComponent in PortraitComponents)
		{
			if (portraitComponent.HeroInfo != null)
			{
				list.Add(portraitComponent.HeroInfo);
			}
		}
		_listener.OnClickStartBattle(_deckInfo, list.ToArray());
	}

	public HeroPortraitComponent GetEmptySlot()
	{
		foreach (HeroPortraitComponent portraitComponent in PortraitComponents)
		{
			if (portraitComponent.HeroInfo == null)
			{
				return portraitComponent;
			}
		}
		return null;
	}

	void HeroPortraitComponent.Listener.OnClick(HeroPortraitComponent heroPortrait)
	{
		if (_listener != null)
		{
			_listener.OnClickDeckHeroPortrait(heroPortrait);
		}
	}

	void HeroPortraitComponent.Listener.OnViewInfo(HeroPortraitComponent heroPortrait, bool isMine)
	{
	}

	void HeroPortraitComponent.Listener.OnLongClick(HeroPortraitComponent heroPortrait, CachedHeroInfo heroInfo)
	{
	}

	void HeroPortraitComponent.Listener.OnClickHeroNotify(HeroNotifyComponent.ENotifyInfo notifyInfo, CachedHeroInfo heroInfo)
	{
	}
}
