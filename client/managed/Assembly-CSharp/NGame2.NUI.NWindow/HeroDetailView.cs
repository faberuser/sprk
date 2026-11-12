using System;
using System.Collections.Generic;
using System.Linq;
using NGame2.NAccount;
using NGame2.NBattleContext;
using NGame2.NScene;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NComponent2.NCostume;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow.NPayShop;
using NGameAsset;
using NGameAsset.NCreature;
using NGameAsset.NSound;
using NNguiExtension;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using NVespa.NUtil;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class HeroDetailView : HeroCostumeSceneView, HeroViewAnimationComponent.Listener, HeroCostumeComponent.Listener
{
	public new interface Listener : HeroFullSceneViewBase.Listener
	{
		void OnTryBattle(HeroDetailView sender);
	}

	[Serializable]
	public class HeroBonusInfo
	{
		public Transform Root;

		public UILabel NameLabel;

		public UILabel DescLabel;

		public UITexture IconTexture;
	}

	[Serializable]
	public class CharacterVoiceUiData
	{
		public UILabelSelector VoiceLanguage;
	}

	public enum EToggle
	{
		None,
		Animation,
		Cinema,
		Costume,
		Skill
	}

	public const string PrefabPath = "Resources/Prefab2/Window/FullScene/HeroDetailView";

	public static int HeroIndex;

	public static int CostumeIndex;

	public UILabel HeroClassNameLabel;

	public UISimpleGrid HeroClassTagGrid;

	public UITable SkillIconTable;

	public HeroBonusInfo HeroBonus;

	public CharacterVoiceUiData CharacterVoiceUi;

	public LocalizeLinkData CharacterVoiceOnChangeMessage;

	public UIScrollGrid HeroViewGrid;

	public UIScrollGrid HeroCostumeGrid;

	public UIToggle ToggleAnimation;

	public UIToggle ToggleCinema;

	public UIToggle ToggleCostume;

	public UIToggle ToggleSkill;

	public UIButton ButtonSkillInfo;

	public Transform TrsTitleShowAnimation;

	public Transform TrsTabRoot;

	public UIWidget WidgetCostumeDragScrollView;

	public UIButton ButtonTryUse;

	public List<Transform> EnableList = new List<Transform>();

	public List<Transform> DisableList = new List<Transform>();

	public Transform ButtonModelView;

	private List<ClassTagIconComponent> _classTagIconList = new List<ClassTagIconComponent>();

	private List<Component> _skillIconComponentList = new List<Component>();

	private List<HeroViewAnimationComponent> _heroViewAnimationComponentList = new List<HeroViewAnimationComponent>();

	private List<HeroCostumeComponent> _costumeComponentList = new List<HeroCostumeComponent>();

	private Dictionary<int, List<GetNewHeroSceneBundleData>> _cacheHeroIndexGetNewHeroBundlePair = new Dictionary<int, List<GetNewHeroSceneBundleData>>();

	private bool isInitHeroViewGrid;

	private EToggle _toggle;

	public CostumeData CurrentCostumeData { get; private set; }

	public void Open(CachedHeroInfo heroInfo, Listener listener, Action onEnd = null, bool useSceneChanger = true)
	{
		CachedHeroInfo heroInfo2 = JsonMarshaler.Clone(heroInfo);
		Open(heroInfo2, (HeroFullSceneViewBase.Listener)listener, onEnd, useSceneChanger);
	}

	public void Open(CachedHeroInfo heroInfo, Listener listener, EToggle startToggle, Action onEnd = null, bool useSceneChanger = true)
	{
		Action onEnd2 = delegate
		{
			InitToggle(startToggle);
			InitModelViewButton();
			Vespa_System_Action_Extension_Method.fire(onEnd);
		};
		Open(heroInfo, (HeroFullSceneViewBase.Listener)listener, onEnd2, useSceneChanger);
	}

	public void OpenOnlyViewMotion(CachedHeroInfo heroInfo, Listener listener)
	{
		Open(heroInfo, (HeroFullSceneViewBase.Listener)listener, (Action)delegate
		{
			SetEnableOrDisable(enable: true);
			InitModelViewButton();
		}, useSceneChanger: true);
	}

	private void InitModelViewButton()
	{
		UIHelper.EnableGameObject(ButtonModelView, enable: false);
	}

	private void SetEnableOrDisable(bool enable)
	{
		foreach (Transform enable2 in EnableList)
		{
			UIHelper.EnableGameObject(enable2, enable);
		}
		foreach (Transform disable in DisableList)
		{
			UIHelper.EnableGameObject(disable, !enable);
		}
	}

	private void SetDisableButtonTryUse(CostumeData costumeData)
	{
		bool enable = true;
		if (!costumeData.IsShow && GlobalAutoBehaviour<AccountManager>.instance.CostumeManager.Get(costumeData.CostumeIndex) == null)
		{
			enable = false;
		}
		UIHelper.EnableGameObject(ButtonTryUse, enable);
	}

	public override void Reset()
	{
		base.Reset();
		FactoryBase.ReleaseAndClearList(_classTagIconList);
		FactoryBase.ReleaseAndClearList(_skillIconComponentList);
		FactoryBase.ReleaseAndClearList(_heroViewAnimationComponentList);
		isInitHeroViewGrid = false;
		_toggle = EToggle.None;
		CurrentCostumeData = null;
		SetEnableOrDisable(enable: false);
		_cacheHeroIndexGetNewHeroBundlePair.Clear();
	}

	protected override void InitInfo()
	{
		base.InitInfo();
		HeroIndex = ((base.HeroInfo != null) ? base.HeroInfo.HeroIndex : 0);
		CostumeIndex = ((base.HeroInfo != null) ? base.HeroInfo.CostumeIndex : 0);
		if (base.HeroInfo != null && base.CreatureData != null)
		{
			UIHelper.EnableGameObject(HeroClassNameLabel, base.CreatureData.TagType != CreatureTagType.None);
			if (base.CreatureData.TagType != CreatureTagType.None)
			{
				ClassTagIconComponent classTagIconComponent = UIHelper.AddGridItem(HeroClassTagGrid, ClassTagIconComponent.PrefabAuto);
				if (classTagIconComponent != null)
				{
					classTagIconComponent.Open(base.CreatureData.TagType, null, ShowDesc: true);
					_classTagIconList.Add(classTagIconComponent);
				}
				UIHelper.SetLabelText(HeroClassNameLabel, LocalizationManager.GetString(base.CreatureData.TagType));
			}
			UIHelper.RepositionGrid(HeroClassTagGrid);
		}
		InitToggle(EToggle.Animation);
		InitHeroBonus();
		InitCharacterVoice();
	}

	private void InitToggle(EToggle startToggle)
	{
		ToggleAnimation.Set(startToggle == EToggle.Animation);
		ToggleCinema.Set(startToggle == EToggle.Cinema);
		ToggleCostume.Set(startToggle == EToggle.Costume);
		ToggleSkill.Set(startToggle == EToggle.Skill);
		_toggle = startToggle;
	}

	private void InitHeroViewGrid()
	{
		if (!isInitHeroViewGrid && _motionSequence != null)
		{
			InitHeroAnimation();
			isInitHeroViewGrid = true;
		}
	}

	private void ResetHeroInfo()
	{
		FactoryBase.ReleaseAndClearList(_heroViewAnimationComponentList);
		FactoryBase.ReleaseAndClearList(_costumeComponentList);
		FactoryBase.ReleaseAndClearList(_skillIconComponentList);
	}

	private void InitHeroAnimation()
	{
		if (_motionSequence == null)
		{
			return;
		}
		ResetHeroInfo();
		HeroAnimationViewData[] datas = GameRoot.Instance.HeroAnimationViewDataContainer.GetDatas(HeroIndex);
		if (datas == null)
		{
			return;
		}
		Dictionary<string, Tuple<HeroAnimationViewData, int>> dictionary = new Dictionary<string, Tuple<HeroAnimationViewData, int>>();
		MotionSequenceData[] motionSequenceDatas = _motionSequence.GetMotionSequenceDatas();
		for (int i = 0; i < motionSequenceDatas.Length; i++)
		{
			MotionSequenceData motionSequenceData = motionSequenceDatas[i];
			string name = motionSequenceData.MotionList[0].Key.PlayName;
			HeroAnimationViewData heroAnimationViewData = Array.Find(datas, delegate(HeroAnimationViewData e)
			{
				if (e.SkipLegendCostume)
				{
					if (CurrentCostumeData == null)
					{
						CostumeData costumeData = GameRoot.Instance.CostumeDataContainer.GetData(base.HeroInfo.CostumeIndex);
						if (costumeData == null)
						{
							costumeData = GameRoot.Instance.CostumeDataContainer.GetDataByHeroStar(base.HeroInfo.HeroIndex, base.HeroInfo.Star);
						}
						if (costumeData != null && costumeData.CostumeType == EHeroCostumeType.Legend)
						{
							return false;
						}
					}
					else if (CurrentCostumeData.CostumeType == EHeroCostumeType.Legend)
					{
						return false;
					}
				}
				return string.Equals(e.Animation, name);
			});
			if (heroAnimationViewData != null)
			{
				Tuple<HeroAnimationViewData, int> value = new Tuple<HeroAnimationViewData, int>(heroAnimationViewData, i);
				if (!dictionary.ContainsKey(name))
				{
					dictionary.Add(name, value);
				}
			}
		}
		List<Tuple<HeroAnimationViewData, int>> list = dictionary.Values.ToList();
		list.Sort((Tuple<HeroAnimationViewData, int> l, Tuple<HeroAnimationViewData, int> r) => l.Item1.Index.CompareTo(r.Item1.Index));
		foreach (Tuple<HeroAnimationViewData, int> item in list)
		{
			HeroViewAnimationComponent heroViewAnimationComponent = UIHelper.AddGridItem(HeroViewGrid, HeroViewAnimationComponent.PrefabAuto);
			heroViewAnimationComponent.Open(this, item.Item2, item.Item1, base.HeroInfo);
			heroViewAnimationComponent.SetSelected(selected: false);
			_heroViewAnimationComponentList.Add(heroViewAnimationComponent);
		}
		UIHelper.RepositionGrid(HeroViewGrid);
		HeroViewAnimationComponent heroViewAnimationComponent2 = _heroViewAnimationComponentList[0];
		heroViewAnimationComponent2.SetSelected(selected: true);
		_motionSequence.Play(heroViewAnimationComponent2.SequenceIndex);
		_motionSequence.EnableMotionSound(needPlayAnimationSound: false);
	}

	private void InitHeroCinema()
	{
		ResetHeroInfo();
		CreateBundleNewHeroComponent();
		CreateAwakenComponent();
		CreateTranscendComponent();
		UIHelper.RepositionGrid(HeroViewGrid);
		_motionSequence.Play(0);
		_motionSequence.EnableMotionSound(needPlayAnimationSound: false);
	}

	private void CreateBundleNewHeroComponent()
	{
		List<GetNewHeroSceneBundleData> newHeroBundleList = GetNewHeroBundleList(HeroIndex);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(newHeroBundleList); i++)
		{
			HeroViewAnimationComponent heroViewAnimationComponent = UIHelper.AddGridItem(HeroViewGrid, HeroViewAnimationComponent.PrefabAuto);
			heroViewAnimationComponent.Open(this, GetNewHero2.EOpenMode.NewHero, newHeroBundleList[i]);
			heroViewAnimationComponent.SetSelected(selected: false);
			_heroViewAnimationComponentList.Add(heroViewAnimationComponent);
		}
	}

	private List<GetNewHeroSceneBundleData> GetNewHeroBundleList(int heroIndex)
	{
		if (!_cacheHeroIndexGetNewHeroBundlePair.ContainsKey(heroIndex))
		{
			List<GetNewHeroSceneBundleData> listOfIncludingHeroIndex = GameRoot.Instance.GetNewHeroSceneBundleDataContainer.GetListOfIncludingHeroIndex(heroIndex);
			_cacheHeroIndexGetNewHeroBundlePair[heroIndex] = listOfIncludingHeroIndex;
		}
		return _cacheHeroIndexGetNewHeroBundlePair[heroIndex];
	}

	private void CreateAwakenComponent()
	{
		HeroViewAnimationComponent heroViewAnimationComponent = UIHelper.AddGridItem(HeroViewGrid, HeroViewAnimationComponent.PrefabAuto);
		heroViewAnimationComponent.Open(this, GetNewHero2.EOpenMode.Awaken, null);
		heroViewAnimationComponent.SetSelected(selected: false);
		_heroViewAnimationComponentList.Add(heroViewAnimationComponent);
	}

	private void CreateTranscendComponent()
	{
		HeroViewAnimationComponent heroViewAnimationComponent = UIHelper.AddGridItem(HeroViewGrid, HeroViewAnimationComponent.PrefabAuto);
		heroViewAnimationComponent.Open(this, GetNewHero2.EOpenMode.Transcend, null);
		heroViewAnimationComponent.SetSelected(selected: false);
		_heroViewAnimationComponentList.Add(heroViewAnimationComponent);
	}

	private void InitHeroCostume()
	{
		ResetHeroInfo();
		CostumeData costumeData = CurrentCostumeData;
		if (costumeData == null)
		{
			costumeData = GameRoot.Instance.CostumeDataContainer.GetData(base.HeroInfo.CostumeIndex);
			if (costumeData == null)
			{
				costumeData = GameRoot.Instance.CostumeDataContainer.GetDataByHeroStar(base.HeroInfo.HeroIndex, base.HeroInfo.Star);
			}
		}
		SetupCostumeList(base.HeroInfo, costumeData, HeroCostumeGrid, _costumeComponentList, this);
		SprkMotionCostumes.Refresh(this, base.HeroInfo, _costumeComponentList);
		SelectCostume(costumeData);
		if ((float)_costumeComponentList.Count * HeroCostumeGrid.Grid.cellWidth > (float)WidgetCostumeDragScrollView.width)
		{
			ScrollToSelectedComponent();
		}
	}

	private void SetupCostumeList(HeroInfo heroInfo, CostumeData selectedBonusCostumeData, UIBaseGrid CostumeGrid, List<HeroCostumeComponent> costumeComponentList, HeroCostumeComponent.Listener listener)
	{
		SprkMotionCostumes.Setup(heroInfo, selectedBonusCostumeData, CostumeGrid, costumeComponentList, listener);
	}

	private void SelectCostume(CostumeData costumeData)
	{
		CostumeData currentCostumeData = CurrentCostumeData;
		CurrentCostumeData = costumeData;
		_motionSequence.Play(0);
		_motionSequence.EnableMotionSound(needPlayAnimationSound: false);
		base.HeroInfo.CostumeIndex = ((CurrentCostumeData != null) ? CurrentCostumeData.CostumeIndex : 0);
		if (costumeData.CostumeType == EHeroCostumeType.Legend)
		{
			base.HeroInfo.HairCostumeIndex = 0;
			base.HeroInfo.WeaponCostumeIndex = 0;
			base.HeroInfo.HideUniqueWeapon = 1;
		}
		SetDisableButtonTryUse(costumeData);
		ChangeRenderFacadeOnly(base.HeroInfo);
		UpdateCostumeListSelectDeselect(CurrentCostumeData, currentCostumeData, _costumeComponentList);
		SprkMotionCostumes.Refresh(this, base.HeroInfo, _costumeComponentList);
	}

	private void ChangeCostume(CostumeData costumeData)
	{
		CostumeData currentCostumeData = CurrentCostumeData;
		CurrentCostumeData = costumeData;
		base.HeroInfo.CostumeIndex = ((CurrentCostumeData != null) ? CurrentCostumeData.CostumeIndex : 0);
		ChangeRenderFacadeOnly(base.HeroInfo);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_costumeComponentList); i++)
		{
			if (Vespa_Common_Class_Extension_Method.EQUALS(_costumeComponentList[i].CostumeData, currentCostumeData))
			{
				_costumeComponentList[i].ChangeCostumeData(CurrentCostumeData);
			}
		}
		SprkMotionCostumes.Refresh(this, base.HeroInfo, _costumeComponentList);
	}

	private void ScrollToSelectedComponent()
	{
		foreach (HeroCostumeComponent costumeComponent in _costumeComponentList)
		{
			if (Vespa_Common_Class_Extension_Method.EQUALS(costumeComponent.CostumeData, CurrentCostumeData))
			{
				HeroCostumeGrid.ScrollTo(costumeComponent, 0f, 0f);
				break;
			}
		}
	}

	private void InitSkillIcon()
	{
		ResetHeroInfo();
		_skillIconComponentList.AddRange(HeroSkillIconComponent.CreateSkillIcon(base.HeroInfo, SkillIconTable.transform));
		if ((bool)SkillIconTable)
		{
			SkillIconTable.Reposition();
			SkillIconTable.repositionNow = true;
		}
	}

	private void InitHeroBonus()
	{
		UIHelper.EnableGameObject(HeroBonus.Root, enable: false);
		if (base.HeroInfo != null)
		{
			HeroBonusData data = GameRoot.Instance.HeroBonusDataContainer.GetData(base.HeroInfo.HeroIndex, base.HeroInfo.Star, base.HeroInfo.Transcended);
			if (data != null)
			{
				UIHelper.EnableGameObject(HeroBonus.Root, enable: true);
				UIHelper.SetLabelText(HeroBonus.NameLabel, LocalizationManager.GetStringDirect(data.BonusNameKey));
				UIHelper.SetLabelText(HeroBonus.DescLabel, LocalizationManager.GetStringDirect(data.BonusDescKey));
				UIHelper.ResetPlaneAssetTextureAsync(this, HeroBonus.IconTexture, UIHelper.GetAttrTypeIcon(data.Icon));
			}
		}
	}

	private void InitCharacterVoice()
	{
		if (base.CreatureData != null && CharacterVoiceUi != null && !(CharacterVoiceUi.VoiceLanguage == null))
		{
			SystemLanguage creatureLanguage = SingletonAuto<LanguageSetting>.instance.GetCreatureLanguage(base.CreatureData.CodeName);
			CharacterVoiceUi.VoiceLanguage.SelectEnum(creatureLanguage);
		}
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		if (_toggle == EToggle.Animation)
		{
			InitHeroViewGrid();
		}
	}

	public override void OnClickCloseNormal()
	{
		if (SceneAutoBehaviour<PayShopManagement>.isValidInstance)
		{
			PayShopHeroInfoPopup instance = SceneAutoBehaviour<PayShopManagement>.instance.GetInstance<PayShopHeroInfoPopup>(openedOnly: true);
			if (instance != null && instance.IsOpen())
			{
				instance.EnableCreatureWidget(enable: true);
			}
		}
		if (SceneAutoBehaviour<InventoryManagement>.isValidInstance)
		{
			PayShopHeroInfoPopup instance2 = SceneAutoBehaviour<InventoryManagement>.instance.GetInstance<PayShopHeroInfoPopup>(openedOnly: true);
			if (instance2 != null && instance2.IsOpen())
			{
				instance2.EnableCreatureWidget(enable: true);
			}
		}
		if (SceneAutoBehaviour<MyHeroManagement>.isValidInstance)
		{
			HeroList instance3 = SceneAutoBehaviour<MyHeroManagement>.instance.GetInstance<HeroList>(openedOnly: true);
			if (instance3 != null && instance3.IsOpen() && instance3.Hero3DViewPanel != null)
			{
				instance3.Hero3DViewPanel.SetupCameraZoom();
			}
		}
		base.OnClickCloseNormal();
	}

	public void OnClickVoiceDefault()
	{
		SetCharacterVoiceLanguage(SystemLanguage.Unknown);
	}

	public void OnClickVoiceKorean()
	{
		SetCharacterVoiceLanguage(SystemLanguage.Korean);
	}

	public void OnClickVoiceEnglish()
	{
		SetCharacterVoiceLanguage(SystemLanguage.English);
	}

	public void OnClickVoiceOfButtonName(GameObject buttonObject)
	{
		string text = buttonObject.name;
		string text2 = text;
		int num = text.LastIndexOf('_');
		if (num >= 0)
		{
			text2 = text.Substring(num + 1);
		}
		EnumLookup<SystemLanguage>.ValueData value = null;
		if (EnumLookup<SystemLanguage>.TryGetByName(text2, out value))
		{
			SystemLanguage characterVoiceLanguage = value?.EnumValue ?? SystemLanguage.Unknown;
			SetCharacterVoiceLanguage(characterVoiceLanguage);
		}
	}

	private void SetCharacterVoiceLanguage(SystemLanguage language)
	{
		if (base.CreatureData != null && SingletonAuto<LanguageSetting>.instance.GetCreatureLanguage(base.CreatureData.CodeName) != language)
		{
			SingletonAuto<LanguageSetting>.instance.SetCreatureLanguage(base.CreatureData.CodeName, language);
			CharacterVoiceUi.VoiceLanguage.SelectEnum(language);
			UILabel targetLabel = CharacterVoiceUi.VoiceLanguage.GetTargetLabel();
			if (targetLabel != null)
			{
				CenterMessage.Open(CharacterVoiceOnChangeMessage.GetLocalize(Vespa_String_Extension_Method.NguiColor(targetLabel.text, Color.yellow)));
			}
		}
	}

	public void OnClickTryBattle()
	{
		if (_listener is Listener listener)
		{
			listener.OnTryBattle(this);
		}
	}

	public void OnClickModelView()
	{
	}

	public static bool TryBattle(HeroInfo heroInfo)
	{
		CachedHeroInfo cachedHeroInfo = heroInfo as CachedHeroInfo;
		if (cachedHeroInfo == null)
		{
			cachedHeroInfo = new CachedHeroInfo(heroInfo);
		}
		cachedHeroInfo.MakeFullSkillLevel();
		BattlePlayerCreateInfo playerCreateInfo = new BattlePlayerCreateInfo
		{
			Id = GlobalAutoBehaviour<AccountManager>.instance.UserManager.Id,
			TeamType = TeamType.Left
		};
		BattleCreatureCreateInfo item = BattleCreatureCreateInfo.CreateMyCreatureCreateInfo(cachedHeroInfo, isLeader: false, isAi: false, null, null);
		List<BattleCreatureCreateInfo> list = new List<BattleCreatureCreateInfo>();
		list.Add(item);
		PlayerEnterBattleLog playerEnterBattleLog = new PlayerEnterBattleLog();
		playerEnterBattleLog.PlayerCreateInfo = playerCreateInfo;
		playerEnterBattleLog.CreatureCreateInfos = list.ToArray();
		int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("HeroTryUseChapterIndex");
		int valueInt2 = GameRoot.Instance.ConstantDataContainer.GetValueInt("HeroTryUseDungeonIndex");
		if (GameRoot.Instance.CampaignDungeonDataContainer.GetData(valueInt, valueInt2) == null)
		{
			VespaDebug.LogError("HeroTryUse dungeon not found : (" + valueInt + "," + valueInt2 + ")");
			return false;
		}
		SingletonAuto<BattleContext>.instance.SetCustomCampaign(valueInt, valueInt2, DungeonDifficultyType.Easy);
		SingletonAuto<CustomCampaignContext>.instance.SetEnterPlayerBattleLog(playerEnterBattleLog);
		SingletonAuto<CustomCampaignContext>.instance.SetBattleEndCallback(delegate
		{
			GlobalAutoBehaviour<LoadingManager>.instance.MoveScene(LoadingManager.ESceneType.Lobby);
		});
		SingletonAuto<BattleContext>.instance.MoveToBattleScene();
		return true;
	}

	void HeroCostumeComponent.Listener.OnClick(HeroCostumeComponent component)
	{
		CostumeData costumeData = component.CostumeData;
		if (costumeData != null)
		{
			SelectCostume(costumeData);
		}
	}

	void HeroCostumeComponent.Listener.OnClickChangeCos(int costumeIndex)
	{
		CostumeData data = GameRoot.Instance.CostumeDataContainer.GetData(costumeIndex);
		if (data != null)
		{
			ChangeCostume(data);
		}
	}

	void HeroViewAnimationComponent.Listener.PlayAnimation(HeroViewAnimationComponent component)
	{
		_motionSequence.Play(component.SequenceIndex);
		_motionSequence.EnableMotionSound(needPlayAnimationSound: false);
		PlayVoice(component.HeroAnimationViewData);
		foreach (HeroViewAnimationComponent heroViewAnimationComponent in _heroViewAnimationComponentList)
		{
			heroViewAnimationComponent.SetSelected(selected: false);
		}
		component.SetSelected(selected: true);
		UpdatePauseButtonState();
	}

	void HeroViewAnimationComponent.Listener.PlayCinema(HeroViewAnimationComponent components)
	{
		HeroInfo heroInfo = new HeroInfo();
		GetNewHero2.OpenData openData = null;
		bool hideInfo = true;
		if (components.OpenCinemaMode == GetNewHero2.EOpenMode.Awaken)
		{
			heroInfo.HeroIndex = HeroIndex;
			heroInfo.Star = base.CreatureData.StartHeroStar;
			heroInfo.Level = 1;
			openData = GetNewHero2.OpenData.CreateSingleFromHeroInfo(heroInfo, components.OpenCinemaMode, hideInfo);
		}
		else if (components.OpenCinemaMode == GetNewHero2.EOpenMode.Transcend)
		{
			heroInfo.HeroIndex = HeroIndex;
			heroInfo.Star = 5;
			heroInfo.Transcended = 5;
			heroInfo.Level = 1;
			openData = GetNewHero2.OpenData.CreateSingleFromHeroInfo(heroInfo, components.OpenCinemaMode, hideInfo);
		}
		else if (components.OpenCinemaMode == GetNewHero2.EOpenMode.NewHero)
		{
			GetNewHeroSceneBundleData getNewHeroSceneBundleData = components.GetNewHeroSceneBundleData;
			if (getNewHeroSceneBundleData == null)
			{
				VespaDebug.LogError("PlayCinema :: GetNewHeroBundleData is null");
				return;
			}
			openData = new GetNewHero2.OpenData();
			int[] sequenceIndices = getNewHeroSceneBundleData.GetSequenceIndices();
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(sequenceIndices); i++)
			{
				CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(sequenceIndices[i]);
				if (data != null)
				{
					heroInfo = new HeroInfo();
					heroInfo.Star = data.StartHeroStar;
					heroInfo.Level = 1;
					string packCode = getNewHeroSceneBundleData.PackCode;
					string treeName = getNewHeroSceneBundleData.TreeName[i];
					bool showInfoOnHeroLeft = getNewHeroSceneBundleData.GetShowInfoOnHeroLeft(i);
					openData.Enqueue(GetNewHero2.Stock.CreateCustomHideOpenningEffect(heroInfo, components.OpenCinemaMode, hideInfo, packCode, treeName, showInfoOnHeroLeft));
				}
			}
			if (openData.IsEmpty())
			{
				VespaDebug.LogError("PlayCinema :: openData is empty");
				return;
			}
		}
		SceneAutoBehaviour<LobbyManagement>.instance.GetInstance<GetNewHero2>().Open(openData, null, null);
	}

	private void PlayVoice(HeroAnimationViewData heroAnimationViewData)
	{
		if (String_Extension_Method.IsNullOrEmpty(heroAnimationViewData.VoiceCode))
		{
			return;
		}
		if (!SingletonAuto<SoundContainer>.instance.IsEmptySoundList())
		{
			_motionSequence.EnableMotionSound(needPlayAnimationSound: false);
		}
		else if (base.HeroInfo != null && base.HeroInfo.CreatureData != null)
		{
			string codeName = base.HeroInfo.CreatureData.CodeName;
			SystemLanguage creatureLanguage = SingletonAuto<LanguageSetting>.instance.GetCreatureLanguage(codeName);
			SingletonAuto<LanguageSetting>.instance.SetCreatureLanguage(codeName, NGame2.NUI.NManager.IllustratedBookManagement.SelectedLanguage);
			SoundStock stock = new SoundStock
			{
				LocalTimer = GlobalTimer.DefaultLocal
			};
			string voiceCode = heroAnimationViewData.VoiceCode;
			string costumeVoicePostfix = SoundHelper.GetCostumeVoicePostfix(base.HeroInfo);
			string soundCode = SoundHelper.GetSoundCode(voiceCode, costumeVoicePostfix);
			stock.Code = soundCode;
			if (SingletonAuto<SoundContainer>.instance.TryPlayOnce(stock, 10f))
			{
				_motionSequence.EnableMotionSound(needPlayAnimationSound: false);
			}
			SingletonAuto<LanguageSetting>.instance.SetCreatureLanguage(codeName, creatureLanguage);
		}
	}

	public void OnToggleChanged()
	{
		EToggle eToggle = EToggle.None;
		if (ToggleAnimation.value)
		{
			eToggle = EToggle.Animation;
		}
		else if (ToggleCinema.value)
		{
			eToggle = EToggle.Cinema;
		}
		else if (ToggleCostume.value)
		{
			eToggle = EToggle.Costume;
		}
		else if (ToggleSkill.value)
		{
			eToggle = EToggle.Skill;
		}
		if (eToggle != EToggle.None && _toggle != eToggle)
		{
			_toggle = eToggle;
			if (_toggle == EToggle.Animation)
			{
				InitHeroAnimation();
			}
			else if (_toggle == EToggle.Cinema)
			{
				InitHeroCinema();
			}
			else if (_toggle == EToggle.Costume)
			{
				InitHeroCostume();
			}
			else if (_toggle == EToggle.Skill)
			{
				InitSkillIcon();
			}
			UpdatePauseButtonState();
		}
	}

	public void OnTouchForRandomAnimation()
	{
	}
}
