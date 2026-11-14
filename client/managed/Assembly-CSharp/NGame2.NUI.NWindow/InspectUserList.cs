using System;
using System.Collections.Generic;
using NGame2.N3D;
using NGame2.NChatting;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NRender3D;
using NGameAsset.NCreature;
using NShared;
using NVespa;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class InspectUserList : BaseWindow, HeroPortraitComponent.Listener, ClassTagIconComponent.Listener, HeroStatPanel.Listener, HeroTranscendSkillListPanel.Listener, HeroRunehengeIcon.IDataProvider, HeroPortraitList.Listener
{
	public enum EToggle
	{
		None,
		Info,
		Skill,
		Attribute
	}

	public interface Listener
	{
		void OnClickEquipIcon(ItemIconComponent caller);

		void OnClickDetailView();

		void OnClickHeroPortrait(HeroPortraitComponent heroPortrait);

		void OnClickChatting();

		void OnClickRunehengeIcon();

		void OnClose();
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Hero/InspectUserList";

	public UIScrollGrid HeroListGrid;

	public UITexture HeroPreviewTexture;

	public UILabel NameLabel;

	public UILabel LevelLabel;

	public UILabel TranscendLabel;

	public GameObject StarParent;

	public List<UIWidget> StarList = new List<UIWidget>();

	public UISimpleGrid ClassTagGrid;

	public Hero3DViewEquipImpl EquipImpl;

	public UIWidget ChatAlertObject;

	public UIToggle ToggleInfo;

	public UIToggle ToggleSkill;

	public UIToggle ToggleAttr;

	public HeroRunehengeIcon RunehengeIcon;

	public HeroStatPanel HeroStatPanel;

	public HeroSkillListPanel HeroSkillListPanel;

	public HeroTranscendSkillListPanel HeroTranscendSkillListPanel;

	private Dictionary<int, HeroPortraitComponent> _dictHeroes = new Dictionary<int, HeroPortraitComponent>();

	private List<int> _ListHeroIndices = new List<int>();

	private int _SelectedHeroIndex;

	private CachedHeroInfo _CachedHeroInfo;

	private CachedHeroInfo[] _HeroInfos;

	private HeroPreview _heroPreview;

	private List<ClassTagIconComponent> _classTagIconComponentList = new List<ClassTagIconComponent>();

	private List<LabelFieldComponent> _baseStatList = new List<LabelFieldComponent>();

	private List<HeroSkillSubComponent> _skillList = new List<HeroSkillSubComponent>();

	private DateTime _lastAlertCheckTime = DateTime.UtcNow;

	private Listener _listener;

	private EToggle _toggle;

	private int[] _deckArray;

	[NonSerialized]
	private WindowPrefab<HeroPortraitList> _allHeroes;

	public void Open(Listener listener, CachedHeroInfo[] heroInfos, int heroIndex, int[] deckArray)
	{
		base.Open();
		Init(listener, heroInfos, heroIndex, deckArray);
	}

	public void Init(Listener listener, CachedHeroInfo[] heroInfos, int heroIndex, int[] deckArray)
	{
		if (heroInfos == null)
		{
			return;
		}
		_listener = listener;
		_HeroInfos = heroInfos;
		_deckArray = deckArray;
		CloseAllHeroes();
		SprkTicketSearch.EnsureInspectAllHeroes(this);
		if (heroIndex == 0 && __Extension_Method_ICollection__.COUNT(heroInfos) > 0)
		{
			heroIndex = heroInfos[0].HeroIndex;
		}
		CachedHeroInfo[] heroInfos2 = _HeroInfos;
		foreach (CachedHeroInfo cachedHeroInfo in heroInfos2)
		{
			if (cachedHeroInfo.HeroIndex == heroIndex)
			{
				_SelectedHeroIndex = heroIndex;
				Setup();
				SetupHeroInfo(cachedHeroInfo);
				MoveList(heroIndex);
				break;
			}
		}
		if (RunehengeIcon != null)
		{
			RunehengeIcon.Open(this);
		}
		SetToggle(EToggle.Info);
	}

	public bool SelectHero(int HeroIndex)
	{
		int selectedHeroIndex = _SelectedHeroIndex;
		if (selectedHeroIndex == HeroIndex)
		{
			return false;
		}
		_SelectedHeroIndex = HeroIndex;
		if (_dictHeroes.ContainsKey(selectedHeroIndex))
		{
			_dictHeroes[selectedHeroIndex].SetSelectSmall(select: false);
		}
		if (_dictHeroes.ContainsKey(_SelectedHeroIndex))
		{
			_dictHeroes[_SelectedHeroIndex].SetSelectSmall(select: true);
		}
		MoveList(HeroIndex);
		return true;
	}

	private void ClearHeroes()
	{
		FactoryBase.ReleaseAndClearList(_dictHeroes);
		_ListHeroIndices.Clear();
	}

	public override void Reset()
	{
		CloseAllHeroes();
		base.Reset();
		ClearHeroes();
		if (!base.gameObject.activeSelf)
		{
			ClosePreview();
		}
		ClearClassTagIconList();
		ClearEquipItemIconList();
		ClearAllPanel();
		if (RunehengeIcon != null)
		{
			RunehengeIcon.Close();
		}
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		UpdateAlert();
	}

	public override void OnClosing()
	{
		CloseAllHeroes();
		base.OnClosing();
		if (_listener != null)
		{
			_listener.OnClose();
		}
		ClearHeroes();
	}

	private void SetChatAlert(int count)
	{
		UIHelper.EnableGameObject(ChatAlertObject, count > 0);
	}

	private void UpdateAlert()
	{
		DateTime utcNow = DateTime.UtcNow;
		if (!((utcNow - _lastAlertCheckTime).TotalSeconds < 0.5))
		{
			_lastAlertCheckTime = utcNow;
			int totalAlertCount = GlobalAutoBehaviour<ChattingManager>.instance.GetTotalAlertCount(refresh: true);
			SetChatAlert(totalAlertCount);
		}
	}

	private void Setup()
	{
		ClearHeroes();
		CachedHeroInfo[] heroInfos = _HeroInfos;
		foreach (CachedHeroInfo heroInfo in heroInfos)
		{
			CreateHeroItem(heroInfo);
		}
		UIHelper.RepositionGrid(HeroListGrid);
	}

	private void CreateHeroItem(CachedHeroInfo heroInfo)
	{
		if (heroInfo == null)
		{
			return;
		}
		if (GameRoot.Instance.CreatureDataContainer.GetData(heroInfo.HeroIndex) == null)
		{
			VespaDebug.LogError("Cannot find creature data : " + heroInfo.HeroIndex);
			return;
		}
		HeroPortraitComponent heroPortraitComponent = UIHelper.AddGridItem(HeroListGrid, HeroPortraitComponent.SmallPrefabAuto);
		heroPortraitComponent.Open(heroInfo, this, null, HeroPortraitComponent.EPortraitMode.Icon);
		if (_SelectedHeroIndex == heroInfo.HeroIndex)
		{
			heroPortraitComponent.SetSelectSmall(select: true);
		}
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(_deckArray) && TypeIterator.Contains(_deckArray, heroInfo.HeroIndex))
		{
			heroPortraitComponent.ShowDeckComponent(show: true);
		}
		_dictHeroes.Add(heroInfo.HeroIndex, heroPortraitComponent);
		_ListHeroIndices.Add(heroInfo.HeroIndex);
	}

	private void InitPreview()
	{
		if (!(HeroPreviewTexture == null) && _CachedHeroInfo != null)
		{
			RenderFacadeBuilder renderFacadeBuilder = new RenderFacadeBuilder(null, TeamType.Left, _CachedHeroInfo);
			renderFacadeBuilder.SetPreload(AnimationKeyData.EmptyPreloadSet);
			UIHelper.InitHero3dPreview(ref _heroPreview, HeroPreviewTexture, _CachedHeroInfo.HeroIndex, _CachedHeroInfo.CostumeIndex, renderFacadeBuilder, "Normal", perspective: false);
		}
	}

	private void ClosePreview()
	{
		UIHelper.CloseHero3dPreview(ref _heroPreview, HeroPreviewTexture);
	}

	public void SetupHeroInfo(CachedHeroInfo cachedHeroInfo)
	{
		_CachedHeroInfo = cachedHeroInfo;
		InitPreview();
		SetupHeroInfo();
		SetupClassTagIconList();
		SetupEquipItemIcon();
		if (RunehengeIcon != null)
		{
			RunehengeIcon.Refresh();
		}
	}

	public void ActivatePreview(bool isActive)
	{
		if (!isActive)
		{
			ClosePreview();
		}
		else
		{
			InitPreview();
		}
	}

	private void SetupHeroInfo()
	{
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(_CachedHeroInfo.HeroIndex);
		if (data == null)
		{
			return;
		}
		EquipImpl.Init(_CachedHeroInfo, _listener.OnClickEquipIcon);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(StarList); i++)
		{
			if ((bool)StarList[i])
			{
				UIHelper.EnableGameObject(StarList[i], i + 1 == _CachedHeroInfo.Star);
			}
		}
		string text = data.Name;
		UIHelper.SetLabelText(NameLabel, text);
		UIHelper.SetLabelText(LevelLabel, _CachedHeroInfo.Level);
		UIHelper.EnableGameObject(TranscendLabel, _CachedHeroInfo.Transcended > 0);
		UIHelper.SetLabelText(TranscendLabel, "+" + _CachedHeroInfo.Transcended);
	}

	private void ClearClassTagIconList()
	{
		FactoryBase.ReleaseAndClearList(_classTagIconComponentList);
	}

	private void SetupClassTagIconList()
	{
		ClearClassTagIconList();
		if (ClassTagGrid == null)
		{
			return;
		}
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(_CachedHeroInfo.HeroIndex);
		if (data == null)
		{
			return;
		}
		CreatureTagType tagType = data.TagType;
		if (tagType != CreatureTagType.None)
		{
			ClassTagIconComponent classTagIconComponent = UIHelper.AddGridItem(ClassTagGrid, ClassTagIconComponent.PrefabAuto);
			if (classTagIconComponent != null)
			{
				_classTagIconComponentList.Add(classTagIconComponent);
				classTagIconComponent.Open(tagType, this);
			}
			UIHelper.RepositionGrid(ClassTagGrid);
		}
	}

	private void ClearEquipItemIconList()
	{
		EquipImpl.Clear();
	}

	public void SetupEquipItemIcon()
	{
		EquipImpl.SetupEquipItemIcon();
	}

	private void ClearAllPanel()
	{
		HeroStatPanel.Close();
		HeroSkillListPanel.Close();
		HeroTranscendSkillListPanel.Close();
	}

	private void SetupStat()
	{
		HeroStatPanel.Open(_CachedHeroInfo, this, isMine: false);
		HeroStatPanel.ResetScroll();
	}

	private void SetupSkill()
	{
		HeroSkillListPanel.Open(_CachedHeroInfo, null, isMine: false);
	}

	private void SetupTranscendSkill()
	{
		HeroTranscendSkillListPanel.Open(_CachedHeroInfo, this, isMine: false);
	}

	public void OnClickDetailView()
	{
		_listener.OnClickDetailView();
	}

	public void OnClickAllHero()
	{
		if (_HeroInfos == null || _HeroInfos.Length == 0)
		{
			return;
		}
		_allHeroes = _allHeroes ?? new WindowPrefab<HeroPortraitList>(HeroPortraitList.PrefabPath, GameUIRoot.ELayerType.UI);
		_allHeroes.instance.Open(this, _SelectedHeroIndex, _HeroInfos);
	}

	private void CloseAllHeroes()
	{
		if (_allHeroes != null && _allHeroes.IsOpen())
		{
			_allHeroes.instance.Close();
		}
	}

	bool HeroPortraitList.Listener.OnCloseRequest(HeroPortraitList caller)
	{
		return true;
	}

	void HeroPortraitList.Listener.OnClick(HeroPortraitComponent comp)
	{
		// Use the sidebar portrait so every inspection listener follows its existing selection path.
		if (comp != null && comp.HeroInfo != null && _dictHeroes.TryGetValue(comp.HeroInfo.HeroIndex, out HeroPortraitComponent portrait))
		{
			((HeroPortraitComponent.Listener)this).OnClick(portrait);
			CloseAllHeroes();
		}
	}

	public void OnClickChatting()
	{
		_listener.OnClickChatting();
	}

	public void OnClickRunehengeIcon()
	{
		_listener.OnClickRunehengeIcon();
	}

	void HeroPortraitComponent.Listener.OnClick(HeroPortraitComponent comp)
	{
		int heroIndex = comp.HeroInfo.HeroIndex;
		if (SelectHero(heroIndex))
		{
			_listener.OnClickHeroPortrait(comp);
		}
		SetToggle(_toggle);
	}

	void HeroPortraitComponent.Listener.OnViewInfo(HeroPortraitComponent comp, bool isMine)
	{
	}

	void HeroPortraitComponent.Listener.OnClickHeroNotify(HeroNotifyComponent.ENotifyInfo notifyInfo, CachedHeroInfo cachedHeroInfo)
	{
	}

	void HeroPortraitComponent.Listener.OnLongClick(HeroPortraitComponent heroPortrait, CachedHeroInfo heroInfo)
	{
	}

	void ClassTagIconComponent.Listener.OnClick(ClassTagIconComponent caller)
	{
	}

	bool HeroStatPanel.Listener.OnCloseRequest(HeroStatPanel closeThis)
	{
		return false;
	}

	void HeroStatPanel.Listener.OnClickShortCut(CachedHeroInfo heroInfo, HeroStatPanel.AwakeStateType awakeState)
	{
	}

	void HeroStatPanel.Listener.OnClickTranscend(CachedHeroInfo heroInfo, HeroStatPanel.AwakeStateType awakeState)
	{
	}

	void HeroStatPanel.Listener.OnClickLimitBreakLevelUp(CachedHeroInfo heroInfo)
	{
	}

	void HeroStatPanel.Listener.OnClickLimitBreakExpUp(CachedHeroInfo heroInfo)
	{
	}

	void HeroStatPanel.Listener.OnClickCraftItem(ItemIconComponent caller)
	{
	}

	void HeroTranscendSkillListPanel.Listener.SelectTranscendSkill(int page, int transcended, int skillSlotIndex, int skillIndex)
	{
	}

	void HeroTranscendSkillListPanel.Listener.SelectTranscendPreset(int heroIndex, int page)
	{
	}

	void HeroTranscendSkillListPanel.Listener.AddTranscendPreset(int page)
	{
	}

	void HeroTranscendSkillListPanel.Listener.ResetTranscendSkill(int page)
	{
	}

	void HeroTranscendSkillListPanel.Listener.RecommendTranscend(HeroInfo heroInfo, int page)
	{
	}

	public void MoveList(int HeroIndex)
	{
		if (_ListHeroIndices.Count <= 4)
		{
			return;
		}
		int num = _ListHeroIndices.FindIndex((int value) => value == HeroIndex);
		if (num != -1)
		{
			HeroPortraitComponent heroPortraitComponent = _dictHeroes[_ListHeroIndices[num]];
			Transform cachedTransform = HeroListGrid.ScrollView.panel.cachedTransform;
			Vector3[] worldCorners = HeroListGrid.ScrollView.panel.worldCorners;
			Vector3 vector2;
			if (num <= 1)
			{
				Vector3 vector = cachedTransform.InverseTransformPoint(worldCorners[2]);
				vector2 = cachedTransform.InverseTransformPoint(_dictHeroes[_ListHeroIndices[0]].transform.position) - vector;
				vector2.y += HeroListGrid.Grid.cellHeight / 2f;
			}
			else if (num >= _ListHeroIndices.Count - 2)
			{
				Vector3 vector3 = cachedTransform.InverseTransformPoint(worldCorners[0]);
				vector2 = cachedTransform.InverseTransformPoint(_dictHeroes[_ListHeroIndices[_ListHeroIndices.Count - 1]].transform.position) - vector3;
				vector2.y -= HeroListGrid.Grid.cellHeight / 2f;
			}
			else
			{
				Vector3 position = (worldCorners[2] + worldCorners[0]) * 0.5f;
				Vector3 vector4 = cachedTransform.InverseTransformPoint(heroPortraitComponent.transform.position);
				Vector3 vector5 = cachedTransform.InverseTransformPoint(position);
				vector2 = vector4 - vector5;
			}
			if (!HeroListGrid.ScrollView.canMoveHorizontally)
			{
				vector2.x = 0f;
			}
			if (!HeroListGrid.ScrollView.canMoveVertically)
			{
				vector2.y = 0f;
			}
			vector2.z = 0f;
			Vector3 pos = cachedTransform.localPosition - vector2;
			SpringPanel.Begin(HeroListGrid.ScrollView.panel.cachedGameObject, pos, 13f).onFinished = delegate
			{
				HeroListGrid.ScrollView.RestrictWithinBounds(instant: true);
			};
		}
	}

	public void SetToggle(EToggle toggle)
	{
		ToggleInfo.Set(toggle == EToggle.Info);
		ToggleSkill.Set(toggle == EToggle.Skill);
		ToggleAttr.Set(toggle == EToggle.Attribute);
		_toggle = toggle;
		ClearAllPanel();
		switch (_toggle)
		{
		case EToggle.Info:
			SetupStat();
			break;
		case EToggle.Skill:
			SetupSkill();
			break;
		case EToggle.Attribute:
			SetupTranscendSkill();
			break;
		}
	}

	public void OnToggleChanged()
	{
		EToggle eToggle = EToggle.None;
		if (ToggleInfo.value)
		{
			eToggle = EToggle.Info;
		}
		else if (ToggleSkill.value)
		{
			eToggle = EToggle.Skill;
		}
		else if (ToggleAttr.value)
		{
			eToggle = EToggle.Attribute;
		}
		if (eToggle != EToggle.None && _toggle != eToggle)
		{
			_toggle = eToggle;
			ClearAllPanel();
			switch (_toggle)
			{
			case EToggle.Info:
				SetupStat();
				break;
			case EToggle.Skill:
				SetupSkill();
				break;
			case EToggle.Attribute:
				SetupTranscendSkill();
				break;
			}
		}
	}

	public void OnClickClose()
	{
		Close();
	}

	public override void OnSystemBackButton()
	{
		OnClickClose();
	}

	public override void ReOpen()
	{
	}

	CachedHeroInfo HeroRunehengeIcon.IDataProvider.GetHeroInfo()
	{
		return _CachedHeroInfo;
	}
}
