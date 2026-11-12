using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NScene.NPreloader;
using NGame2.NUI.NComponent2.NHeroPortraitSub;
using NGame2.NUI.NWindow;
using NGameAsset;
using NNguiExtension;
using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

[UIEvent(AllowDuringPatchDownloading = true)]
public sealed class HeroPortraitComponent : UIScrollItemBehaviour, InitOnCreate, HeroNotifyComponent.Listener, IInspectorGuiDrawer
{
	[Serializable]
	public class PrefabSelector
	{
		public enum EPrefabType
		{
			Default,
			Short,
			Small,
			BattleSmall
		}

		public EPrefabType PrefabType;

		public PrefabAuto<HeroPortraitComponent> PrefabAuto
		{
			get
			{
				if (PrefabType == EPrefabType.Default)
				{
					return HeroPortraitComponent.PrefabAuto;
				}
				if (PrefabType == EPrefabType.Short)
				{
					return ShortPrefabAuto;
				}
				if (PrefabType == EPrefabType.Small)
				{
					return SmallPrefabAuto;
				}
				if (PrefabType == EPrefabType.BattleSmall)
				{
					return BattleSmallPrefabAuto;
				}
				return HeroPortraitComponent.PrefabAuto;
			}
		}

		public PrefabSelector(EPrefabType prefabType)
		{
			PrefabType = prefabType;
		}
	}

	public enum ESubComponent
	{
		Select,
		SelectSmall,
		NewMark,
		ClassTag,
		HeroStat,
		PurifyGauge,
		StarBlink,
		PurifyPercent,
		FragmentState,
		TrialSelect,
		HeroStar,
		HeroName,
		EquipableState,
		Owned,
		Locked,
		Deck,
		HeroSmallStar,
		NotOwnedSmall,
		CostumeNewTagSmall,
		AchievementCleared,
		AchievementCompleted,
		Ban,
		Disable,
		DeckSlot,
		Blind,
		CustomLabel,
		Bookmark,
		Revive
	}

	public enum EPortraitMode
	{
		Basic,
		Icon,
		Battle,
		BanPick
	}

	public interface Listener
	{
		void OnClick(HeroPortraitComponent heroPortrait);

		void OnViewInfo(HeroPortraitComponent heroPortrait, bool isMine);

		void OnLongClick(HeroPortraitComponent heroPortrait, CachedHeroInfo heroInfo);

		void OnClickHeroNotify(HeroNotifyComponent.ENotifyInfo notifyInfo, CachedHeroInfo heroInfo);
	}

	public interface IDataProvider
	{
		bool AchievementCompleted(int HeroIndex);

		int GetRewardableCount(int HeroIndex);
	}

	public class OwnerInfo
	{
		public long AccountId;

		public string Nick;

		public int Level;
	}

	[Serializable]
	public class ClassTagInfo
	{
		public CreatureTagType tagType;

		public UISprite Sprite_ClassTag;
	}

	[Serializable]
	public class RootWidgetInfo
	{
		public ESubComponent SubComponent;

		public UIWidget RootWidget;
	}

	public static string PrefabPath = "Prefab2/Component/Portrait/HeroPortrait";

	public static string ShortPrefabPath = "Prefab2/Component/Portrait/HeroPortraitShort";

	public static string SmallPrefabPath = "Prefab2/Component/Portrait/HeroPortraitSmall";

	public static string BattleSmallPrefabPath = "Prefab3/Component/Portrait/HeroBattlePortraitSmall";

	public static PrefabAuto<HeroPortraitComponent> PrefabAuto = new PrefabAuto<HeroPortraitComponent>(PrefabPath);

	public static PrefabAuto<HeroPortraitComponent> ShortPrefabAuto = new PrefabAuto<HeroPortraitComponent>(ShortPrefabPath);

	public static PrefabAuto<HeroPortraitComponent> SmallPrefabAuto = new PrefabAuto<HeroPortraitComponent>(SmallPrefabPath);

	public static PrefabAuto<HeroPortraitComponent> BattleSmallPrefabAuto = new PrefabAuto<HeroPortraitComponent>(BattleSmallPrefabPath);

	private static Color _dispatchDefaultColorTop = new Color(1f, 0.73f, 0.38f, 1f);

	private static Color _dispatchDefaultColorBottom = new Color(1f, 0.96f, 0.74f, 1f);

	private static Color _restrictedDefaultColorTop = new Color(1f, 0.73f, 0.38f, 1f);

	private static Color _restrictedDefaultColorBottom = new Color(1f, 0.96f, 0.74f, 1f);

	public UIWidget WidgetThis;

	public UIBundleLoader HeroBundleTex;

	public GameObject HeroLevelRoot;

	public GameObject recommandedIcon;

	public UILabel HeroLevel;

	public UISpriteSelector HeroDamageType;

	public float BookmarkAlpha = 0.6f;

	public List<UIWidget> HeroFrameList;

	public UIWidget FrameWidget;

	public RootWidgetInfo[] RootWidgetInfos;

	public UISimpleGrid NotifyGrid;

	public Transform RewardEffectRoot;

	public UIGame3DWidget LevelUpEffect;

	public Color LimitBreakLabelColor = Color.red;

	public Color LimitBreakFrameColor = Color.red;

	private HeroTooltipTextInfo _TooltipTextInfo;

	private Listener _listener;

	private IDataProvider _dataProvider;

	private bool _selected;

	private bool _banned;

	private bool _dead;

	private bool _disabled;

	private bool _isDispatched;

	private bool _isRestricted;

	private bool _isRecommanded;

	private bool _pressed;

	private DateTime _pressedTime;

	private bool _Mine;

	private AssetTexture.TextureInfo _portraitTexInfo;

	private EPortraitMode _portraitMode;

	private List<HeroNotifyComponent> _ListNotifyComp = new List<HeroNotifyComponent>();

	public SubComponentContainer<HeroPortraitComponent> SubCompContainer = new SubComponentContainer<HeroPortraitComponent>();

	public CachedHeroInfo HeroInfo { get; set; }

	public CreatureData CreatureData { get; private set; }

	public int VisualLevel { get; private set; }

	public int VisualStar { get; private set; }

	public int VisualTranscend { get; private set; }

	public OwnerInfo Owner { get; set; }

	public long OwnerId
	{
		get
		{
			if (Owner == null)
			{
				return 0L;
			}
			return Owner.AccountId;
		}
	}

	public string OwnerNick
	{
		get
		{
			if (Owner == null)
			{
				return null;
			}
			return Owner.Nick;
		}
	}

	public int OwnerLevel
	{
		get
		{
			if (Owner == null)
			{
				return 0;
			}
			return Owner.Level;
		}
	}

	public bool IsControllerable { get; set; }

	public EPortraitMode PortraitMode => _portraitMode;

	public int HeroCostumeIndex { get; private set; }

	public bool IsSelected => _selected;

	public bool IsBanned => _banned;

	public bool IsDisabled => _disabled;

	public bool IsDispatched => _isDispatched;

	public bool IsRestricted => _isRestricted;

	public bool IsDead => _dead;

	public bool IsRecommanded => _isRecommanded;

	void InitOnCreate.OnCreate()
	{
		HeroCostumeIndex = 0;
	}

	void InitOnCreate.OnRelease()
	{
		Close();
	}

	public static void ReserveSubComponent(BasePreloader preloader)
	{
		preloader.ReserveComponent(SelectSubComponent.PrefabAuto);
		preloader.ReserveComponent(SelectSmallSubComponent.PrefabAuto);
		preloader.ReserveComponent(BookmarkSmallSubComponent.PrefabAuto);
		preloader.ReserveComponent(NewMarkComponent.PrefabAuto);
		preloader.ReserveComponent(ClassTagComponent.PrefabAuto);
		preloader.ReserveComponent(PurifyComponent.PrefabAuto);
		preloader.ReserveComponent(StarSubComponent.PrefabAuto);
		preloader.ReserveComponent(SmallStarSubComponent.PrefabAuto);
		preloader.ReserveComponent(TrialSelectSubComponent.PrefabAuto);
		preloader.ReserveComponent(FragmentStateComponent.PrefabAuto);
		preloader.ReserveComponent(HeroNameComponent.PrefabAuto);
		preloader.ReserveComponent(EquipableStateComponent.PrefabAuto);
		preloader.ReserveComponent(OwnedSubComponent.PrefabAuto);
		preloader.ReserveComponent(LockedSubComponent.PrefabAuto);
		preloader.ReserveComponent(DeckComponent.PrefabAuto);
		preloader.ReserveComponent(NotOwnedSmallSubComponent.PrefabAuto);
		preloader.ReserveComponent(CostumeNewTagSmallSubComponent.PrefabAuto);
		preloader.ReserveComponent(AchievementClearedSubComponent.PrefabAuto);
		preloader.ReserveComponent(BanSubComponent.PrefabAuto);
		preloader.ReserveComponent(CustomLabelSubComponent.PrefabAuto);
		preloader.ReserveComponent(RestrictedSubComponent.PrefabAuto);
		preloader.ReserveComponent(DisableSubComponent.PrefabAuto);
		preloader.ReserveComponent(DeckSlotSmallSubComponent.PrefabAuto);
		preloader.ReserveComponent(BlindSubComponent.PrefabAuto);
		preloader.ReserveComponent(DummySubComponent.PrefabAuto);
	}

	public T GetSubComponent<T>() where T : BaseSubComponent<HeroPortraitComponent>
	{
		T subComponent = SubCompContainer.GetSubComponent<T>();
		if (subComponent != null)
		{
			return subComponent;
		}
		subComponent = SubCompContainer.CreateSubComponent<T>(base.gameObject.transform);
		if (subComponent == null)
		{
			return null;
		}
		subComponent.Open(this);
		return subComponent;
	}

	public void ForEachSubComponent<T>(Action<BaseSubComponent<HeroPortraitComponent>> action) where T : BaseSubComponent<HeroPortraitComponent>
	{
		SubCompContainer.ForEach<T>(action);
	}

	public bool ContainSubComponent<T>() where T : BaseSubComponent<HeroPortraitComponent>
	{
		return SubCompContainer.GetSubComponent<T>() != null;
	}

	public bool ActiveSubComponent<T>() where T : BaseSubComponent<HeroPortraitComponent>
	{
		return false;
	}

	public GameObject GetRootGO(Enum e)
	{
		if (e.GetType() != typeof(ESubComponent))
		{
			return null;
		}
		ESubComponent eSubComponent = (ESubComponent)(object)e;
		for (int i = 0; i < RootWidgetInfos.Length; i++)
		{
			if (RootWidgetInfos[i].SubComponent == eSubComponent && RootWidgetInfos[i].RootWidget != null)
			{
				return RootWidgetInfos[i].RootWidget.gameObject;
			}
		}
		return WidgetThis.gameObject;
	}

	public bool Open(CachedHeroInfo heroInfo, Listener listener, OwnerInfo owner = null, EPortraitMode PortraitMode = EPortraitMode.Basic)
	{
		HeroInfo = heroInfo;
		Owner = owner;
		_portraitMode = PortraitMode;
		SetupInfo(listener);
		UIHelper.EnableGameObject(RewardEffectRoot, enable: false);
		UIHelper.EnableGameObject(LevelUpEffect, enable: false);
		return true;
	}

	public bool Open(CreatureData creatureData, Listener listener, EPortraitMode PortraitMode = EPortraitMode.Basic, int visualStar = 0, int visualTranscend = 0, int costumeIndex = 0, int visualLevel = 0, bool isRecommanded = false)
	{
		if (creatureData == null)
		{
			return false;
		}
		CreatureData = creatureData;
		HeroInfo = null;
		_portraitMode = PortraitMode;
		VisualLevel = visualLevel;
		VisualStar = visualStar;
		VisualTranscend = visualTranscend;
		HeroCostumeIndex = costumeIndex;
		_isRecommanded = isRecommanded;
		SetupInfo(listener);
		UIHelper.EnableGameObject(RewardEffectRoot, enable: false);
		UIHelper.EnableGameObject(LevelUpEffect, enable: false);
		return true;
	}

	public void ResetPortrait(CachedHeroInfo heroInfo, CreatureData creatureData)
	{
		HeroInfo = heroInfo;
		CreatureData = creatureData;
		UIHelper.EnableGameObject(HeroBundleTex, enable: false);
		SetupInfo(_listener);
	}

	private void SetupInfo(Listener listener)
	{
		_listener = listener;
		IsControllerable = true;
		WidgetThis.alpha = 1f;
		if (HeroInfo != null || CreatureData != null)
		{
			if (HeroInfo != null)
			{
				CreatureData = GameRoot.Instance.CreatureDataContainer.GetData(HeroInfo.HeroIndex);
			}
			SetHeroFrame();
			SetClassTag();
			SetDamageType(CreatureData.PlayableCharacter, CreatureData.AttrType);
			ShowStar(show: true);
			SetLevel(show: true);
			EnableViewMyInfo(enable: false);
			EnableViewOtherInfo(enable: false);
			SetupTooltipText();
			SetRecommandedIcon(_isRecommanded);
		}
		else
		{
			CreatureData = null;
			SetShowClassTag(enable: false);
			SetDamageType(enable: false);
			ShowStar(show: false);
			SetLevel(show: false);
			SetEmptySlot();
			SetRecommandedIcon(isRecommanded: false);
		}
		SetHeroImage();
	}

	public void SetupFrameColor()
	{
		if (HeroInfo != null && HeroInfo.LimitBreakLevel > 0)
		{
			SetFrameColor(LimitBreakFrameColor);
		}
		else
		{
			SetFrameColor(Color.white);
		}
	}

	public void Close()
	{
		HeroInfo = null;
		CreatureData = null;
		IsControllerable = true;
		_pressed = false;
		_selected = false;
		VisualLevel = 0;
		VisualStar = 0;
		VisualTranscend = 0;
		_dataProvider = null;
		SetGrayScale(grayScale: false);
		SetFrameColor(Color.white);
		SubCompContainer.ReleaseSubComponent();
		ClearNotify();
		_banned = false;
		_selected = false;
		_disabled = false;
		_isDispatched = false;
		_isRestricted = false;
		_dead = false;
		_isRecommanded = false;
	}

	public void SetupAchievement(IDataProvider dataProvider)
	{
		_dataProvider = dataProvider;
		int index = CreatureData.Index;
		if (_dataProvider.GetRewardableCount(index) > 0)
		{
			AchievementClearedSubComponent subComponent = GetSubComponent<AchievementClearedSubComponent>();
			if (subComponent != null)
			{
				subComponent.Show(show: true);
			}
		}
	}

	public void ShowStar(bool show)
	{
		if (_portraitMode == EPortraitMode.Basic)
		{
			StarSubComponent subComponent = GetSubComponent<StarSubComponent>();
			if (subComponent != null)
			{
				subComponent.Show(show);
			}
		}
		else
		{
			SmallStarSubComponent subComponent2 = GetSubComponent<SmallStarSubComponent>();
			if (subComponent2 != null)
			{
				subComponent2.Show(show);
			}
		}
	}

	public void SetLevel(bool show)
	{
		UIHelper.EnableGameObject(HeroLevelRoot, show);
		UIHelper.EnableGameObject(HeroLevel, show);
		if (show)
		{
			if (HeroInfo != null)
			{
				UIHelper.EnableGameObject(HeroLevel, enable: true);
				UIHelper.SetLabelText(HeroLevel, HeroInfo.Level);
				int maxLevel = GameRoot.Instance.CreatureLevelDataContainer.MaxLevel;
				UIHelper.SetLabelColor(HeroLevel, (HeroInfo.Level > maxLevel) ? LimitBreakLabelColor : Color.white);
			}
			else if (VisualLevel > 0)
			{
				UIHelper.EnableGameObject(HeroLevel, enable: true);
				UIHelper.SetLabelText(HeroLevel, VisualLevel);
				int maxLevel2 = GameRoot.Instance.CreatureLevelDataContainer.MaxLevel;
				UIHelper.SetLabelColor(HeroLevel, (VisualLevel > maxLevel2) ? LimitBreakLabelColor : Color.white);
			}
			else
			{
				UIHelper.EnableGameObject(HeroLevel, enable: false);
			}
		}
	}

	public void ShowRecommandIcon(bool show)
	{
		UIHelper.EnableGameObject(recommandedIcon, show);
	}

	public void ShowEquipable(bool show)
	{
		EquipableStateComponent subComponent = GetSubComponent<EquipableStateComponent>();
		if (subComponent != null)
		{
			subComponent.Show(!_isDispatched && show);
		}
	}

	public void SetPurifyGauge(float purified)
	{
		PurifyComponent subComponent = GetSubComponent<PurifyComponent>();
		if (subComponent != null)
		{
			subComponent.SetPurified(purified);
		}
	}

	public void ShowShortOfFragment(bool show, HeroAwakeType toggle)
	{
		FragmentStateComponent subComponent = GetSubComponent<FragmentStateComponent>();
		if (subComponent != null)
		{
			subComponent.Show(show, toggle);
		}
	}

	public void ShowDeckComponent(bool show)
	{
		DeckComponent subComponent = GetSubComponent<DeckComponent>();
		if (subComponent != null)
		{
			subComponent.Show(show);
		}
	}

	public void SetGrayScale(bool grayScale)
	{
		UIHelper.SetWidgetGrayScale(WidgetThis, grayScale);
	}

	public void SetFrameColor(Color color)
	{
		UIHelper.SetWidgetColor(FrameWidget, color);
	}

	public void SetPortraitState(EHeroState state)
	{
		SetSelection(state == EHeroState.Party);
	}

	public void SetDead(bool isDead)
	{
		if (_dead != isDead)
		{
			_dead = isDead;
			ReviveSubComponent subComponent = GetSubComponent<ReviveSubComponent>();
			if (subComponent != null)
			{
				subComponent.SetShow(isDead);
			}
		}
	}

	public void SetReviveGemCount(int reqGemCount)
	{
		if (_dead)
		{
			ReviveSubComponent subComponent = GetSubComponent<ReviveSubComponent>();
			if (subComponent != null)
			{
				subComponent.SetReqGemLabel(reqGemCount);
			}
		}
	}

	public void SetResetRemainTime(string remainText)
	{
		if (_dead)
		{
			ReviveSubComponent subComponent = GetSubComponent<ReviveSubComponent>();
			if (subComponent != null)
			{
				subComponent.SetRemainTime(remainText);
			}
		}
	}

	public void SetBan(bool ban)
	{
		if (_banned != ban)
		{
			_banned = ban;
			BanSubComponent subComponent = GetSubComponent<BanSubComponent>();
			if (subComponent != null)
			{
				subComponent.SetShow(ban);
			}
		}
	}

	public void SetDisable(bool disable)
	{
		if (_disabled != disable)
		{
			_disabled = disable;
			DisableSubComponent subComponent = GetSubComponent<DisableSubComponent>();
			if (subComponent != null)
			{
				subComponent.SetShow(disable);
			}
		}
	}

	public void SetSelection(bool select)
	{
		if (_selected != select)
		{
			_selected = select;
			GetSubComponent<SelectSubComponent>().SetShow(select);
		}
	}

	public void SetSelectSmall(bool select)
	{
		SelectSmallSubComponent subComponent = GetSubComponent<SelectSmallSubComponent>();
		if (subComponent != null)
		{
			subComponent.SetShow(select);
		}
	}

	public void SetBookmarkedSmall(bool select)
	{
		BookmarkSmallSubComponent subComponent = GetSubComponent<BookmarkSmallSubComponent>();
		if (subComponent != null)
		{
			subComponent.SetShow(select);
		}
	}

	public void SetDeckSlot(bool enable, int slotIndex = 0)
	{
		DeckSlotSmallSubComponent subComponent = GetSubComponent<DeckSlotSmallSubComponent>();
		if (subComponent != null)
		{
			subComponent.SetShow(enable);
			subComponent.SetSlotIndex(slotIndex);
		}
	}

	public void SetBlind(bool enable)
	{
		BlindSubComponent subComponent = GetSubComponent<BlindSubComponent>();
		if (subComponent != null)
		{
			subComponent.SetShow(enable);
		}
		if (enable)
		{
			ShowStar(show: false);
			SetLevel(show: false);
			SetDamageType(enable: false);
			SetShowClassTag(enable: false);
		}
	}

	public void SetDummy(bool enable)
	{
		DummySubComponent subComponent = GetSubComponent<DummySubComponent>();
		if (subComponent != null)
		{
			subComponent.SetShow(enable);
		}
		if (enable)
		{
			ShowStar(show: false);
			SetLevel(show: false);
			SetDamageType(enable: false);
			SetShowClassTag(enable: false);
		}
	}

	public void SetNotOwnedSmall()
	{
		NotOwnedSmallSubComponent subComponent = GetSubComponent<NotOwnedSmallSubComponent>();
		if (subComponent != null)
		{
			subComponent.SetShow(show: true);
		}
	}

	public void SetTrialHeroSelect(bool select)
	{
		TrialSelectSubComponent subComponent = GetSubComponent<TrialSelectSubComponent>();
		if (subComponent != null)
		{
			subComponent.SetShow(select);
		}
	}

	public void SetNew(bool show)
	{
		if (show || ContainSubComponent<NewMarkComponent>())
		{
			NewMarkComponent subComponent = GetSubComponent<NewMarkComponent>();
			if (subComponent != null)
			{
				subComponent.SetShow(show);
			}
		}
	}

	public void SetOwned(bool owned)
	{
		OwnedSubComponent subComponent = GetSubComponent<OwnedSubComponent>();
		if (subComponent != null)
		{
			subComponent.SetShow(owned);
		}
	}

	public void SetLocked(bool locked)
	{
		LockedSubComponent subComponent = GetSubComponent<LockedSubComponent>();
		if (subComponent != null)
		{
			subComponent.SetShow(locked);
		}
	}

	public void SetCostumeNewSmallTag()
	{
		CostumeNewTagSmallSubComponent subComponent = GetSubComponent<CostumeNewTagSmallSubComponent>();
		if (subComponent != null)
		{
			subComponent.SetShow(show: true);
		}
	}

	public void SetRecommandedIcon(bool isRecommanded)
	{
		UIHelper.EnableGameObject(recommandedIcon, isRecommanded);
	}

	public void EnableViewMyInfo(bool enable)
	{
		_Mine = enable;
	}

	public void EnableViewOtherInfo(bool enable)
	{
		_Mine = !enable;
	}

	public void OnClickPortrait()
	{
		if (SprkDispatchSelection.CanClick(this))
		{
			_pressed = false;
			if (_listener != null)
			{
				_listener.OnClick(this);
			}
		}
	}

	public void OnLongClickPortrait()
	{
		_pressed = false;
		if (_listener != null)
		{
			_listener.OnLongClick(this, HeroInfo);
		}
	}

	public void OnPressPortrait()
	{
		if (!_selected)
		{
			_pressed = true;
			_pressedTime = DateTime.UtcNow;
		}
	}

	public void OnReleasePortrait()
	{
		_pressed = false;
	}

	public void ShowCreatureName(bool showShadow = true)
	{
		HeroNameComponent subComponent = GetSubComponent<HeroNameComponent>();
		if (subComponent != null)
		{
			subComponent.SetShowName(ShowName: true, showShadow);
		}
	}

	public void ShowName(string Name, bool showShadow = true)
	{
		HeroNameComponent subComponent = GetSubComponent<HeroNameComponent>();
		if (subComponent != null)
		{
			subComponent.SetShowName(Name, showShadow);
		}
	}

	public void SetHeroImage(int star)
	{
		if (CreatureData != null)
		{
			int costumeIndex = ((HeroInfo == null) ? HeroCostumeIndex : HeroInfo.CostumeIndex);
			string assetPath = string.Empty;
			UIHelper.EAssetTextureAssign assetTexAssign = UIHelper.EAssetTextureAssign.None;
			if (_portraitMode == EPortraitMode.Basic || _portraitMode == EPortraitMode.BanPick)
			{
				assetPath = UIHelper.GetHeroPortraitPath(CreatureData.CodeName, star, costumeIndex);
				assetTexAssign = UIHelper.EAssetTextureAssign.ClipBottom;
			}
			else if (_portraitMode == EPortraitMode.Icon)
			{
				assetPath = UIHelper.GetHeroIconPath(CreatureData.Index, star, "", costumeIndex);
			}
			else if (_portraitMode == EPortraitMode.Battle)
			{
				assetPath = UIHelper.GetBattleHeroPortraitPath(CreatureData.CodeName, star, costumeIndex);
			}
			UIHelper.SetBundleTexture(HeroBundleTex, assetPath, assetTexAssign);
		}
	}

	public void SetHeroImage()
	{
		if (CreatureData == null)
		{
			UIHelper.EnableGameObject(HeroBundleTex, enable: false);
			return;
		}
		int num = VisualStar;
		if (num == 0)
		{
			num = ((HeroInfo == null) ? CreatureData.StartHeroStar : HeroInfo.Star);
		}
		int costumeIndex = ((HeroInfo == null) ? HeroCostumeIndex : HeroInfo.CostumeIndex);
		string assetPath = string.Empty;
		UIHelper.EAssetTextureAssign assetTexAssign = UIHelper.EAssetTextureAssign.None;
		if (_portraitMode == EPortraitMode.Basic || _portraitMode == EPortraitMode.BanPick)
		{
			assetPath = UIHelper.GetHeroPortraitPath(CreatureData.CodeName, num, costumeIndex);
			assetTexAssign = UIHelper.EAssetTextureAssign.ClipBottom;
		}
		else if (_portraitMode == EPortraitMode.Icon)
		{
			assetPath = UIHelper.GetHeroIconPath(CreatureData.Index, num, "", costumeIndex);
		}
		else if (_portraitMode == EPortraitMode.Battle)
		{
			assetPath = UIHelper.GetBattleHeroPortraitPath(CreatureData.CodeName, num, costumeIndex);
		}
		UIHelper.SetBundleTexture(HeroBundleTex, assetPath, assetTexAssign);
		UIHelper.EnableGameObject(HeroBundleTex, enable: true);
	}

	public void SetEmptySlot()
	{
	}

	public void SetHeroFrame()
	{
		if (CreatureData == null)
		{
			return;
		}
		int num = 0;
		if (HeroInfo != null)
		{
			num = HeroInfo.Transcended;
		}
		for (int i = 0; i < HeroFrameList.Count; i++)
		{
			UIWidget component = HeroFrameList[i];
			if (num == i)
			{
				UIHelper.EnableGameObject(component, enable: true);
			}
			else
			{
				UIHelper.EnableGameObject(component, enable: false);
			}
		}
		SetupFrameColor();
	}

	public void SetCustomLabelSmall(string text)
	{
		CustomLabelSubComponent subComponent = GetSubComponent<CustomLabelSubComponent>();
		if (!(subComponent == null))
		{
			subComponent.SetShow(show: true);
			subComponent.SetSmall();
			subComponent.SetDiabledSpriteSmall(active: false);
			subComponent.SetLabelText(text);
			subComponent.SetGradientActivation(isActive: true);
			subComponent.SetGradientColor(_dispatchDefaultColorTop, _dispatchDefaultColorBottom);
			subComponent.SetFontSize(28);
		}
	}

	public void SetDispatched(bool isDispatched, bool isSmall = false, string text = "")
	{
		_isDispatched = isDispatched;
		CustomLabelSubComponent subComponent = GetSubComponent<CustomLabelSubComponent>();
		if (!(subComponent != null))
		{
			return;
		}
		subComponent.SetShow(_isDispatched);
		if (_isDispatched)
		{
			if (!isSmall)
			{
				subComponent.SetLarge();
			}
			else
			{
				subComponent.SetSmall();
			}
			if (String_Extension_Method.IsNullOrEmpty(text))
			{
				text = LocalizationManager.GetStringDirect("STATUS_HERO_DISPATCHED");
			}
			subComponent.SetLabelText(text);
			subComponent.SetGradientActivation(isActive: true);
			subComponent.SetGradientColor(_dispatchDefaultColorTop, _dispatchDefaultColorBottom);
			subComponent.SetFontSize((!isSmall) ? 30 : 25);
		}
	}

	public void SetRestricted(bool isRestricted, bool isSmall = false, string text = "")
	{
		_isRestricted = isRestricted;
		RestrictedSubComponent subComponent = GetSubComponent<RestrictedSubComponent>();
		if (!(subComponent != null))
		{
			return;
		}
		subComponent.SetShow(_isRestricted);
		if (_isRestricted)
		{
			if (!isSmall)
			{
				subComponent.SetLarge();
			}
			else
			{
				subComponent.SetSmall();
			}
			if (String_Extension_Method.IsNullOrEmpty(text))
			{
				text = LocalizationManager.GetStringDirect("STATUS_HERO_RESTRICTED");
			}
			subComponent.SetLabelText(text);
			subComponent.SetGradientActivation(isActive: true);
			subComponent.SetGradientColor(_restrictedDefaultColorTop, _restrictedDefaultColorBottom);
			subComponent.SetFontSize((!isSmall) ? 30 : 25);
		}
	}

	public bool IsHeroImageLoaded()
	{
		return true;
	}

	private void Update()
	{
		UpdatePressed();
	}

	private void UpdatePressed()
	{
		if (_pressed && (DateTime.UtcNow - _pressedTime).TotalSeconds >= 0.25)
		{
			_pressed = false;
			if (_listener != null)
			{
				_listener.OnViewInfo(this, _Mine);
			}
		}
	}

	public bool HasBetterEquip()
	{
		foreach (HeroNotifyComponent item in _ListNotifyComp)
		{
			if (item.DisplayNotifyInfo == HeroNotifyComponent.ENotifyInfo.CanEquipChange)
			{
				return true;
			}
		}
		return false;
	}

	private void ClearNotify()
	{
		FactoryBase.ReleaseAndClearList(_ListNotifyComp);
	}

	private bool CheckUpdateNeeded(IList<HeroNotifyComponent.ENotifyInfo> ListNotifyInfo)
	{
		if (_ListNotifyComp.Count == ListNotifyInfo.Count)
		{
			for (int i = 0; i < _ListNotifyComp.Count; i++)
			{
				if (_ListNotifyComp[i].DisplayNotifyInfo != ListNotifyInfo[i])
				{
					return true;
				}
			}
			return false;
		}
		return true;
	}

	public void AddHeroNotify(IList<HeroNotifyComponent.ENotifyInfo> ListNotifyInfo, bool portraitSmall = false)
	{
		if (!(NotifyGrid == null) && ListNotifyInfo != null && CheckUpdateNeeded(ListNotifyInfo))
		{
			ClearNotify();
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(ListNotifyInfo); i++)
			{
				HeroNotifyComponent heroNotifyComponent = ((!portraitSmall) ? UIHelper.AddGridItem(NotifyGrid, HeroNotifyComponent.PrefabAuto) : UIHelper.AddGridItem(NotifyGrid, HeroNotifyComponent.PrefabSmallAuto));
				heroNotifyComponent.Open(this, ListNotifyInfo[i]);
				_ListNotifyComp.Add(heroNotifyComponent);
			}
			UIHelper.RepositionGrid(NotifyGrid);
		}
	}

	public void OnShowTooltip()
	{
		HeroIconTooltip.Open(WidgetThis, _TooltipTextInfo);
	}

	public void OnHideTooltip()
	{
		HeroIconTooltip.Hide(WidgetThis);
	}

	private void SetupTooltipText()
	{
		if (CreatureData != null)
		{
			if (_TooltipTextInfo == null)
			{
				_TooltipTextInfo = new HeroTooltipTextInfo();
			}
			_TooltipTextInfo.Reset();
			_TooltipTextInfo.Name.Value = CreatureData.Name;
			_TooltipTextInfo.CreatureTagType = CreatureData.TagType;
			if (!String_Extension_Method.IsNullOrEmpty(CreatureData.Description))
			{
				_TooltipTextInfo.Desc.Value = CreatureData.Description;
			}
			if (HeroInfo != null)
			{
				_TooltipTextInfo.TypeText.Value = LocalizationManager.GetString(HeroStat.STAT_LEVEL) + " " + HeroInfo.Level;
			}
		}
	}

	public void SetClassTag()
	{
		if (CreatureData != null)
		{
			ClassTagComponent subComponent = GetSubComponent<ClassTagComponent>();
			if (subComponent != null)
			{
				subComponent.SetShow(show: true);
				subComponent.SetTag(CreatureData.TagType);
			}
		}
	}

	public void SetShowClassTag(bool enable)
	{
		ClassTagComponent subComponent = GetSubComponent<ClassTagComponent>();
		if (subComponent != null)
		{
			subComponent.SetShow(enable);
		}
	}

	public void SetDamageType(bool enable, SkillAttrType[] damageType = null)
	{
		if (HeroDamageType == null)
		{
			return;
		}
		if (damageType == null)
		{
			UIHelper.EnableGameObject(HeroDamageType, enable: false);
			return;
		}
		UIHelper.EnableGameObject(HeroDamageType, enable);
		if (damageType.Contains(SkillAttrType.Physical))
		{
			HeroDamageType.Select("Physical");
		}
		else if (damageType.Contains(SkillAttrType.Magical))
		{
			HeroDamageType.Select("Magical");
		}
	}

	public bool IsEmptySlot()
	{
		if (HeroInfo == null)
		{
			return true;
		}
		return false;
	}

	public void EnableRewardEffectRoot(bool isActive)
	{
		UIHelper.EnableGameObject(RewardEffectRoot, isActive);
	}

	public void PlayLevelUpEffect(bool play)
	{
		if (LevelUpEffect == null)
		{
			return;
		}
		if (play)
		{
			UIHelper.EnableGameObject(LevelUpEffect, enable: true);
			if (!LevelUpEffect.IsPlaying)
			{
				LevelUpEffect.Play(null);
			}
		}
		else
		{
			LevelUpEffect.Stop(immediatelyClear: true);
		}
	}

	public static void SetClassTag(CreatureData creatureData, List<ClassTagInfo> ListClassTag)
	{
		if (creatureData != null)
		{
			ListClassTag?.ForEach(delegate(ClassTagInfo info)
			{
				UIHelper.EnableGameObject(info.Sprite_ClassTag, info.tagType == creatureData.TagType);
			});
		}
	}

	public static string GetRaceName(CreatureRaceType raceType)
	{
		if (raceType == CreatureRaceType.None)
		{
			return string.Empty;
		}
		return LocalizationManager.GetString(raceType);
	}

	void HeroNotifyComponent.Listener.OnClickComponent(HeroNotifyComponent.ENotifyInfo notifyInfo)
	{
		if (_listener != null)
		{
			_listener.OnClickHeroNotify(notifyInfo, HeroInfo);
		}
	}
}
