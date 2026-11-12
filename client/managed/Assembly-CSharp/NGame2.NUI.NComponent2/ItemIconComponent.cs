using System;
using System.Collections;
using System.Linq;
using NGame2.NAccount;
using NGame2.NAccount.NExtension;
using NGame2.NContentsDefine;
using NGame2.NDropBonusEvent;
using NGame2.NGameSetting.NUi;
using NGame2.NUI.NComponent;
using NGame2.NUI.NComponent2.NHelper;
using NGame2.NUI.NComponent2.NItemIconSub;
using NGame2.NUI.NHelper;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NGame2.NUtil.NStringProxy;
using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class ItemIconComponent : UIScrollItemBehaviour, NVespa.NPrefab2.InitOnCreate, IInspectorGuiDrawer, IIconComponent
{
	public interface Listener
	{
		void OnClick(ItemIconComponent itemIconComponent);
	}

	public interface TooltipListener
	{
		void OnShowTooltip(ItemIconComponent itemIconComponent);
	}

	private class TooltipTextInfoProxy
	{
		private ItemTooltipTextInfo _tooltipTextInfo;

		public ItemTooltipTextInfo GetInfo(ItemData itemData, EquipItemInfo equipItemInfo, CachedHeroInfo cachedHeroInfo)
		{
			if (_tooltipTextInfo == null)
			{
				SetupTooltipText(itemData, equipItemInfo, cachedHeroInfo);
			}
			return _tooltipTextInfo;
		}

		private void SetupTooltipText(ItemData itemData, EquipItemInfo equipItemInfo, CachedHeroInfo cachedHeroInfo)
		{
			if (itemData != null)
			{
				if (_tooltipTextInfo == null)
				{
					_tooltipTextInfo = new ItemTooltipTextInfo();
				}
				_tooltipTextInfo.Reset();
				_tooltipTextInfo.Name.Value = itemData.Name;
				if (itemData.Type == ItemType.Equip)
				{
					SetupEquipItemTooltipText(itemData, equipItemInfo, cachedHeroInfo);
				}
				else
				{
					SetupItemTooltipText(itemData, equipItemInfo);
				}
			}
		}

		private void SetupEquipItemTooltipText(ItemData itemData, EquipItemInfo equipItemInfo, CachedHeroInfo cachedHeroInfo)
		{
			EquipItemData subData = itemData.GetSubData<EquipItemData>();
			if (subData == null)
			{
				return;
			}
			_tooltipTextInfo.Name.Assign(new ItemNameBuilder(itemData, equipItemInfo, appendLine: false));
			_tooltipTextInfo.Desc1.Assign(new EquipItemDataBuilder(itemData, subData, equipItemInfo, cachedHeroInfo, EquipItemDataBuilder.EMethod.Desc1, appendLine: false));
			_tooltipTextInfo.Desc2.Assign(new EquipItemDataBuilder(itemData, subData, equipItemInfo, cachedHeroInfo, EquipItemDataBuilder.EMethod.ReqLevelAndPartType, appendLine: false));
			string creatureLimitString = subData.GetCreatureLimitString();
			if (!string.IsNullOrEmpty(creatureLimitString))
			{
				if (subData.SubType == EquipItemSubType.SpecialTreasure_1 || subData.SubType == EquipItemSubType.SpecialTreasure_2 || subData.SubType == EquipItemSubType.SpecialTreasure_3 || subData.SubType == EquipItemSubType.SpecialTreasure_4)
				{
					_tooltipTextInfo.CreatureLimit = LocalizationManager.GetString(Equip.EQUIP_CREATURE_LIMIT_TREASURE, creatureLimitString);
				}
				else
				{
					_tooltipTextInfo.CreatureLimit = LocalizationManager.GetString(Equip.EQUIP_CREATURE_LIMIT_WEAPON, creatureLimitString);
				}
			}
			else
			{
				_tooltipTextInfo.CreatureLimit = string.Empty;
			}
			if (GameRoot.Instance.EquipItemDataContainer.ContainsAllClassTag(subData.EquipCode))
			{
				_tooltipTextInfo.EquipAllClassTag = true;
			}
			else
			{
				for (int i = 0; i < subData.TagType.Length; i++)
				{
					_tooltipTextInfo.EquipClassTags.Add(subData.TagType[i]);
				}
			}
			if (!String_Extension_Method.IsNullOrEmpty(itemData.Description))
			{
				_tooltipTextInfo.Desc.Value = itemData.Description;
			}
			SetEnhanceStoneDesc(itemData);
		}

		private void SetEnhanceStoneDesc(ItemData data)
		{
			if (data != null && GameRoot.Instance.EquipItemDataContainer.IsEnhanceStone(data.Code))
			{
				_tooltipTextInfo.Desc1.Value = data.Description2;
			}
		}

		private void SetupItemTooltipText(ItemData itemData, EquipItemInfo equipItemInfo)
		{
			_tooltipTextInfo.Name.Assign(new ItemNameBuilder(itemData, null, appendLine: false));
			RuneSlotType[] runeSlotTypes = RuneHelper.GetRuneSlotTypes(itemData);
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(runeSlotTypes))
			{
				_tooltipTextInfo.EquipRuneSlotTypes = runeSlotTypes.ToList();
			}
			switch (itemData.Type)
			{
			case ItemType.Recipe:
			case ItemType.Material:
				_tooltipTextInfo.TypeText.Assign(new EnumBuilder().Key(Item.ITEM_MATERIAL));
				break;
			case ItemType.UpgradeBook:
				_tooltipTextInfo.TypeText.Assign(new EnumBuilder().Key(Item.ITEM_UPGRADEBOOK));
				break;
			default:
				_tooltipTextInfo.TypeText.Reset();
				break;
			}
			if (!String_Extension_Method.IsNullOrEmpty(itemData.Description))
			{
				_tooltipTextInfo.Desc.Value = itemData.Description;
			}
			if (itemData.Type == ItemType.Potion)
			{
				_tooltipTextInfo.Desc1.Assign(new PotionItemDataBuilder(itemData, appendLine: false));
			}
			else if (itemData.Type == ItemType.Rune || itemData.Type == ItemType.PunishmentRune)
			{
				_tooltipTextInfo.Desc1.Assign(new RuneItemDataBuilder(itemData, equipItemInfo?.PunishmentRuneOption, RuneItemDataBuilder.EMethod.Desc1, appendLine: false));
			}
			else if (itemData.Type == ItemType.Flask)
			{
				_tooltipTextInfo.Desc1.Assign(new FlaskItemDataBuilder(itemData, appendLine: false));
			}
			else
			{
				_tooltipTextInfo.Desc1.Value = itemData.Description2;
			}
			SetSoulStoneHeroName(itemData);
		}

		private void SetSoulStoneHeroName(ItemData itemData)
		{
			if (_tooltipTextInfo == null || itemData == null || itemData.Type != ItemType.SoulStone)
			{
				return;
			}
			int result = 0;
			if (int.TryParse(itemData.GetValue(0), out result))
			{
				CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(result);
				if (data != null)
				{
					_tooltipTextInfo.CreatureLimit = data.Name + " " + LocalizationManager.GetStringDirect("INVENTORY_SOULSTONE");
				}
			}
		}
	}

	public const string PrefabPath = "Prefab2/Component/Icon/ItemIconComponent";

	public static PrefabAuto<ItemIconComponent> PrefabAuto = new PrefabAuto<ItemIconComponent>("Prefab2/Component/Icon/ItemIconComponent");

	public UIWidget WidgetThis;

	public UIWidget RootWidget;

	public PlaySoundComponent playSound;

	public UISprite RewardNoticeSprite;

	private int _originalWidgetDepth;

	[SerializeField]
	private UILabel LbInvenCount;

	private ProxyContainer _subProxyContainer = new ProxyContainer();

	private TooltipTextInfoProxy _tooltipTextProxy = new TooltipTextInfoProxy();

	private Listener _listener;

	private NGame2.NUI.NComponent2.NHelper.GetItemAnimationData _itemAnimation;

	public ItemData ItemData { get; private set; }

	public EquipItemInfo EquipItemInfo { get; private set; }

	public int ItemIndex
	{
		get
		{
			if (ItemData == null)
			{
				return 0;
			}
			return ItemData.Index;
		}
	}

	public EquipItemData EquipItemData => GetItemSubData<EquipItemData>();

	public RuneItemData RuneItemData => GetItemSubData<RuneItemData>();

	public CachedHeroInfo CachedHeroInfo { get; private set; }

	public EquipPartType EquipPartType { get; private set; }

	public bool IsSoulWeapon { get; private set; }

	public long ItemCount { get; private set; }

	public long ItemMaxCount { get; private set; }

	public int SubKey { get; private set; }

	private bool CheckIsSoulWeapon
	{
		get
		{
			if (EquipItemInfo == null)
			{
				return false;
			}
			return EquipItemInfo.IsSoulWeapon();
		}
	}

	private bool IsTechnoMagic
	{
		get
		{
			EquipItemData equipItemData = EquipItemData;
			if (equipItemData == null)
			{
				return false;
			}
			return equipItemData.EquipType == EquipItemEquipType.TechnoMagic;
		}
	}

	public void SetListener(Listener listener)
	{
		_listener = listener;
	}

	void NVespa.NPrefab2.InitOnCreate.OnCreate()
	{
		_subProxyContainer.Owner = this;
		if (WidgetThis != null)
		{
			_originalWidgetDepth = WidgetThis.depth;
		}
		SetRewardNotice(enable: false);
	}

	void NVespa.NPrefab2.InitOnCreate.OnRelease()
	{
		if (DragScrollView != null)
		{
			DragScrollView.OnReleasePrefab();
		}
		_subProxyContainer.OnRelease();
		if (_itemAnimation != null)
		{
			_itemAnimation.Close();
			_itemAnimation = null;
		}
		_tooltipTextProxy = null;
		RootWidget.alpha = 1f;
		RootWidget.transform.localScale = Vector3.one;
		UIHelper.SetWidgetGrayScale(RootWidget, grayScale: false);
		SetRewardNotice(enable: false);
		int num = 0;
		ItemCount = num;
		int num2 = 0;
		ItemMaxCount = num2;
		SubKey = 0;
		if (WidgetThis != null && _originalWidgetDepth != WidgetThis.depth)
		{
			WidgetThis.depth = _originalWidgetDepth;
		}
	}

	public bool IsSubComponentActive<T>() where T : ProxyBase
	{
		return _subProxyContainer.IsActive<T>();
	}

	public bool Open(EquipItemInfo equipItemInfo, Listener listener, CachedHeroInfo cachedHeroInfo = null)
	{
		if (equipItemInfo == null)
		{
			Init(null, null, listener, cachedHeroInfo);
			return true;
		}
		ItemData data = GameRoot.Instance.ItemDataContainer.GetData(equipItemInfo.ItemIndex);
		if (data == null)
		{
			return false;
		}
		Init(data, equipItemInfo, listener, cachedHeroInfo);
		return true;
	}

	public void Open(ItemData itemData, EquipItemInfo equipItemInfo, Listener listener, CachedHeroInfo cachedHeroInfo = null)
	{
		Init(itemData, equipItemInfo, listener, cachedHeroInfo);
	}

	public void Open(ItemData itemData, Listener listener = null, bool isShowInvenItemCount = false)
	{
		Init(itemData, null, listener, null, EquipPartType.None, isShowInvenItemCount);
	}

	public void OpenWithDelay(ItemData itemData, Listener listener = null, bool isShowInvenItemCount = false, float delay = 0.3f)
	{
		InitWithDelay(itemData, null, listener, null, EquipPartType.None, isShowInvenItemCount, delay);
	}

	public void Open(Listener listener, EquipPartType partType = EquipPartType.None, CachedHeroInfo cachedHeroInfo = null)
	{
		Init(null, null, listener, cachedHeroInfo, partType);
		SetupPartsTypeIcon(partType);
	}

	public void OpenEmpty(Listener listener)
	{
		_listener = listener;
		ItemData = null;
		EquipItemInfo = null;
		SetEmpty(isEmpty: true);
		Refresh();
	}

	private void OnDisable()
	{
		_subProxyContainer.OnDisable();
	}

	private void Update()
	{
		_subProxyContainer.ChainUpdate();
		if (_itemAnimation != null)
		{
			_itemAnimation.ChainUpdate();
		}
	}

	public NGame2.NUI.NComponent2.NHelper.GetItemAnimationData CreateGetAnimationSimple()
	{
		if (_itemAnimation != null)
		{
			_itemAnimation.Close();
			_itemAnimation = null;
		}
		NGame2.NGameSetting.NUi.GetItemAnimationData getUniqueItemAnimation = GameSetting.data.Ui.GetUniqueItemAnimation;
		_itemAnimation = new NGame2.NUI.NComponent2.NHelper.GetItemAnimationData();
		_itemAnimation.Open(ItemData, RootWidget, null, getUniqueItemAnimation);
		return _itemAnimation;
	}

	public NGame2.NUI.NComponent2.NHelper.GetItemAnimationData CreateGetAnimation()
	{
		if (ClientValueManager.IsIgnoreItemEffect)
		{
			return null;
		}
		EffectSubComponent.ComponentProxy componentProxy = _subProxyContainer.Get<EffectSubComponent.ComponentProxy>();
		if (componentProxy == null)
		{
			return null;
		}
		if (_itemAnimation != null)
		{
			_itemAnimation.Close();
			_itemAnimation = null;
		}
		if (ItemData == null)
		{
			return null;
		}
		int num = ((EquipItemInfo != null) ? EquipItemInfo.Star : 0);
		ItemEffectWidgetData data = GameRoot.Instance.ItemEffectWidgetDataContainer.GetData(ItemData.GetEffect, num);
		if (data == null)
		{
			return null;
		}
		UIGame3DWidget effect = componentProxy.EnableAndGet(data.EffectWidgetType);
		NGame2.NGameSetting.NUi.GetItemAnimationData itemAnimationData = GetItemAnimationData(data.EffectWidgetType);
		_itemAnimation = new NGame2.NUI.NComponent2.NHelper.GetItemAnimationData();
		_itemAnimation.Open(ItemData, RootWidget, effect, itemAnimationData, num);
		return _itemAnimation;
	}

	private void Init(ItemData itemData, EquipItemInfo equipItemInfo, Listener listener, CachedHeroInfo cachedHeroInfo = null, EquipPartType equipPartType = EquipPartType.None, bool isShowInvenItemCount = false)
	{
		_listener = listener;
		ItemData = itemData;
		EquipItemInfo = equipItemInfo;
		int num = 1;
		ItemCount = num;
		IsSoulWeapon = CheckIsSoulWeapon;
		SetDisabled(disabled: false);
		CachedHeroInfo = cachedHeroInfo;
		EquipPartType = equipPartType;
		SetInvenItemCount(isShowInvenItemCount);
		InitEquipPartType();
		Refresh();
	}

	private void InitWithDelay(ItemData itemData, EquipItemInfo equipItemInfo, Listener listener, CachedHeroInfo cachedHeroInfo = null, EquipPartType equipPartType = EquipPartType.None, bool isShowInvenItemCount = false, float delay = 0.3f)
	{
		_listener = listener;
		ItemData = itemData;
		EquipItemInfo = equipItemInfo;
		int num = 1;
		ItemCount = num;
		IsSoulWeapon = CheckIsSoulWeapon;
		SetDisabled(disabled: false);
		CachedHeroInfo = cachedHeroInfo;
		EquipPartType = equipPartType;
		SetInvenItemCount(isShowInvenItemCount);
		InitEquipPartType();
		VespaCoroutine.Start(RefreshWithDelay(delay));
	}

	private IEnumerator RefreshWithDelay(float delay)
	{
		yield return new WaitForSeconds(delay);
		Refresh();
	}

	private void InitEquipPartType()
	{
		if (EquipPartType == EquipPartType.None && ItemData != null)
		{
			EquipItemData subData = ItemData.GetSubData<EquipItemData>();
			if (subData != null)
			{
				EquipPartType = subData.PartType;
			}
		}
	}

	private void SetInvenItemCount(bool isShow)
	{
		UIHelper.EnableGameObject(LbInvenCount, isShow);
		if (ItemData != null)
		{
			int itemCount = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.GetItemCount(ItemData.Index);
			UIHelper.SetLabelText(LbInvenCount, LocalizationManager.GetString(Item.COMMON_GROWTH_REWARD_COUNT, Vespa_Int_Extension_Method.ToStringCache(itemCount)));
		}
	}

	private T GetItemSubData<T>() where T : class, IItemSubData
	{
		if (ItemData == null)
		{
			return null;
		}
		return ItemData.GetSubData<T>();
	}

	public void SetEmpty(bool isEmpty)
	{
		_subProxyContainer.Get<EmptySubComponent.ComponentProxy>()?.Set(isEmpty);
		SetPlaySound(!isEmpty);
	}

	private void SetPlaySound(bool play)
	{
		if (playSound != null)
		{
			playSound.enabled = play;
		}
	}

	public void SetRewardNotice(bool enable)
	{
		UIHelper.EnableGameObject(RewardNoticeSprite, enable);
	}

	public void SetCustomLabel(string text, Color color)
	{
		_subProxyContainer.Get<CustomLabelSubComponent.ComponentProxy>()?.Set(text, color, CheckIsSoulWeapon);
	}

	public void HideCustomLabel()
	{
		_subProxyContainer.Get<CustomLabelSubComponent.ComponentProxy>()?.Set(string.Empty, Color.white, CheckIsSoulWeapon);
	}

	public void SetUpperLabel(string text, Color color)
	{
		_subProxyContainer.Get<UpperLabelSubComponent.ComponentProxy>()?.Set(text, color);
	}

	public void SetupPartsTypeIcon(EquipPartType partType)
	{
		_subProxyContainer.Get<PartsTypeIconSubComponent.ComponentProxy>()?.Set(partType);
	}

	public void SetLocked(bool locked)
	{
		_subProxyContainer.Get<LockedSubComponent.ComponentProxy>()?.Set(locked);
	}

	public void SetLockSubComponent(bool locked)
	{
		_subProxyContainer.Get<ItemLockedSubComponent.ComponentProxy>()?.Set(locked);
	}

	public void SetPopSubCompnent(int count, PopSubComponent.Listener listener = null)
	{
		PopSubComponent.ComponentProxy componentProxy = _subProxyContainer.Get<PopSubComponent.ComponentProxy>();
		if (componentProxy != null)
		{
			if (count <= 0)
			{
				componentProxy.Set(string.Empty, listener);
			}
			else
			{
				componentProxy.Set(count.ToString(), listener);
			}
		}
	}

	public void SetClassTagError(bool classTagError)
	{
		string text = (classTagError ? LocalizationManager.GetString(Equip.EQUIP_CLASS_TAG_TYPE_ERROR) : string.Empty);
		Color color = (classTagError ? new Color(1f, 0.294f, 0.294f, 1f) : Color.white);
		SetCustomLabel(text, color);
	}

	public void SetBoosterInReward(bool displayBooster, string boosterCode, bool displayBoosterLabel)
	{
		_subProxyContainer.Get<RewardSubComponent.ComponentProxy>()?.SetBoosterInfo(displayBooster, boosterCode, displayBoosterLabel);
	}

	public void SetInvenCountInReward(bool displayInvenCount, ItemData itemData, bool isLightBackground)
	{
		_subProxyContainer.Get<RewardSubComponent.ComponentProxy>()?.SetInvenCount(displayInvenCount, itemData, isLightBackground);
	}

	public void SetUnderLabel(bool displayUnderLabel, string text, bool isLightBackground)
	{
		_subProxyContainer.Get<RewardSubComponent.ComponentProxy>()?.SetUnderLabel(displayUnderLabel, text, isLightBackground);
	}

	public void SetFIrstClearLabel(bool isFirstClear)
	{
		_subProxyContainer.Get<FirstClearComponent.ComponentProxy>()?.Set(isFirstClear);
	}

	public void SetBestTeamMark(bool isBestTeam)
	{
		_subProxyContainer.Get<BestTeamComponent.ComponentProxy>()?.Set(isBestTeam);
	}

	public void ChangeWidgetDepth(int depth)
	{
		if (WidgetThis != null)
		{
			WidgetThis.depth = depth;
		}
	}

	public void ResetPosition()
	{
		Vespa_Unity_Transform_Extension_Method.ResetLocalTM(base.transform);
	}

	public void Refresh()
	{
		InitIconTexture();
		InitStar();
		InitTreasureSlot();
		InitTier();
		InitSoulWeaponGrade();
		InitTechnoMagic();
		InitItemLocked();
		if (ClientValueManager.IsShowEquipItemSlotIndex)
		{
			InitEquipItemSlotIndex();
		}
		_tooltipTextProxy = new TooltipTextInfoProxy();
	}

	public void SetDisabled(bool disabled)
	{
		_subProxyContainer.Get<DisabledBlackSubComponent.ComponentProxy>()?.Set(disabled);
	}

	public void SetLowLevel(bool lowlevel, int reqLevel = 0)
	{
		string text = (lowlevel ? LocalizationManager.GetString(Equip.MAX_EQUIP_REQ_LEVEL, reqLevel) : string.Empty);
		Color color = (lowlevel ? new Color(1f, 0.294f, 0.294f, 1f) : Color.white);
		SetCustomLabel(text, color);
	}

	public bool IsDisabled()
	{
		if (!_subProxyContainer.IsActive<CustomLabelSubComponent.ComponentProxy>() && !_subProxyContainer.IsActive<SelectSubComponent.ComponentProxy>())
		{
			return _subProxyContainer.IsActive<DisabledBlackSubComponent.ComponentProxy>();
		}
		return true;
	}

	public bool IsDisabledClick()
	{
		return _subProxyContainer.IsActive<DisabledBlackSubComponent.ComponentProxy>();
	}

	public void SetSelected(bool selected, SelectSubComponent.ESelectType selectType = SelectSubComponent.ESelectType.Select_Checked)
	{
		_subProxyContainer.Get<SelectSubComponent.ComponentProxy>()?.Set(selected, selectType, CheckIsSoulWeapon);
	}

	public bool IsSelected()
	{
		return _subProxyContainer.Get<SelectSubComponent.ComponentProxy>()?.IsSelected ?? false;
	}

	public void SetNew(bool isNew)
	{
		_subProxyContainer.Get<NewMarkSubComponent.ComponentProxy>()?.Set(isNew);
	}

	public void SetGetMark(bool isGet)
	{
		_subProxyContainer.Get<GetMarkSubComponent.ComponentProxy>()?.Set(isGet);
	}

	public void SetPetSoulInsert(bool isInsert)
	{
		_subProxyContainer.Get<PetSoulInsertSubComponent.ComponentProxy>()?.Set(isInsert);
	}

	public void ShowStatDiff(bool isVisible, CompareEquipItem compareValue)
	{
		_subProxyContainer.Get<StatDiffSubComponent.ComponentProxy>()?.Set(isVisible, compareValue);
	}

	public void InitStar()
	{
		if (ItemData == null)
		{
			return;
		}
		int num = 0;
		int num2 = 0;
		if (ItemData.Type == ItemType.Equip)
		{
			if (EquipItemInfo != null)
			{
				num = EquipItemInfo.Star;
			}
		}
		else if (ItemData.Type == ItemType.AwakeMaterial)
		{
			num = ItemData.GetValueInt(0);
		}
		else if (ItemData.Type == ItemType.Hero)
		{
			int valueInt = ItemData.GetValueInt(0);
			CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(valueInt);
			if (data != null)
			{
				num = data.StartHeroStar;
			}
			if (__Extension_Method_ICollection__.COUNT(ItemData.Value) > 1)
			{
				num = ((ItemData.Value[1] != null) ? Vespa_String_Extension_Method.TOINT(ItemData.Value[1], num) : num);
			}
			if (__Extension_Method_ICollection__.COUNT(ItemData.Value) > 3)
			{
				num2 = ((ItemData.Value[3] != null) ? Vespa_String_Extension_Method.TOINT(ItemData.Value[3], num2) : num2);
			}
		}
		SetStar(num, num2);
	}

	public void SetStar(int star, int transcend = 0)
	{
		_subProxyContainer.Get<ItemStarSubComponent.ComponentProxy>()?.Set(star, transcend, EquipItemData != null && EquipItemData.EquipType == EquipItemEquipType.TechnoMagic);
	}

	public void InitSoulWeaponGrade()
	{
		int grade = -1;
		int level = 0;
		if (IsSoulWeapon)
		{
			grade = EquipItemInfo.SoulWeaponInfo.Grade;
			level = EquipItemInfo.SoulWeaponInfo.ReinforceLevel;
		}
		SetSoulWeaponGrade(grade, level);
	}

	public void SetSoulWeaponGrade(int grade, int level)
	{
		_subProxyContainer.Get<SoulWeaponGradeSubComponent.ComponentProxy>()?.Set(grade, level);
	}

	public void InitTechnoMagic()
	{
		if (EquipItemData != null && EquipItemData.WorkPlace == EquipItemWorkPlace.Valance)
		{
			if (EquipItemData.TagType.Count() > 0)
			{
				SetTechnoMagicClass(EquipItemData.TagType[0]);
			}
			else
			{
				SetTechnoMagicClass(CreatureTagType.None);
			}
			SetTechnoMagicIdentified(EquipItemInfo == null || EquipItemInfo.IsIdentified);
		}
	}

	public void SetTechnoMagicClass(CreatureTagType classType)
	{
		_subProxyContainer.Get<TechnoMagicClassSubComponent.ComponentProxy>()?.Set(classType);
	}

	public void SetTechnoMagicIdentified(bool identified)
	{
		_subProxyContainer.Get<TechnoMagicIdentifiedSubComponent.ComponentProxy>()?.Set(identified);
	}

	private void InitIconTexture()
	{
		if (ItemData == null)
		{
			_subProxyContainer.ReleaseProxy<ItemBaseSubComponent.ComponentProxy>();
			return;
		}
		ItemBaseSubComponent.ComponentProxy componentProxy = _subProxyContainer.Get<ItemBaseSubComponent.ComponentProxy>();
		if (componentProxy == null)
		{
			return;
		}
		if (!IsSoulWeapon)
		{
			if (!IsTechnoMagic)
			{
				componentProxy.Set(ItemData.Code, ItemData.Grade);
			}
			else
			{
				componentProxy.Set(ItemData.Code, 200);
			}
		}
		else if (EquipItemInfo != null && EquipItemInfo.SoulWeaponInfo != null)
		{
			SoulWeaponInfo soulWeaponInfo = EquipItemInfo.SoulWeaponInfo;
			componentProxy.Set(ItemData.Code, (byte)(100 + soulWeaponInfo.Grade));
		}
	}

	private void InitTreasureSlot()
	{
		_subProxyContainer.Get<TreasureSlotSubComponent.ComponentProxy>()?.Set(CachedHeroInfo);
	}

	public void SetItemRatio(long ratioPerMille)
	{
		_subProxyContainer.Get<ItemRatioSubComponent.ComponentProxy>()?.Set(ratioPerMille);
	}

	public void SetItemMinMax(long count, long maxCount, Color? color = null)
	{
		SetItemCount_complex(count, color ?? Color.white, 0, maxCount);
	}

	public void SetItemCount(long count, Color? color = null)
	{
		Color color2 = color ?? Color.white;
		int displayZero = 0;
		int displayOne = 0;
		int num = 0;
		SetItemCount_complex(count, color2, 0, num, (byte)displayOne != 0, (byte)displayZero != 0);
	}

	public void SetItemCount_with_MaxCap(long count)
	{
		int maxCapCount = ItemMaxCap.GetMaxCapCount(ItemIndex);
		Color color = Color.white;
		if (maxCapCount > 0)
		{
			int num = maxCapCount;
			if (count >= num)
			{
				color = Color.red;
			}
		}
		Color color2 = color;
		int displayZero = 0;
		int displayOne = 0;
		int num2 = 0;
		SetItemCount_complex(count, color2, 0, num2, (byte)displayOne != 0, (byte)displayZero != 0);
	}

	public void SetItemCount_notzero(int count, Color? color = null)
	{
		Color obj = color ?? Color.white;
		int displayZero = 0;
		int displayOne = 1;
		int num = 0;
		int reqCount = 0;
		Color color2 = obj;
		SetItemCount_complex(count, color2, reqCount, num, (byte)displayOne != 0, (byte)displayZero != 0);
	}

	public void SetItemCount_anycount(int count, Color? color = null)
	{
		Color obj = color ?? Color.white;
		int displayZero = 1;
		int displayOne = 1;
		int num = 0;
		int reqCount = 0;
		Color color2 = obj;
		SetItemCount_complex(count, color2, reqCount, num, (byte)displayOne != 0, (byte)displayZero != 0);
	}

	public void SetItemCount_req(int count, int reqCount)
	{
		Color white = Color.white;
		int displayZero = 0;
		int displayOne = 0;
		int num = 0;
		Color color = white;
		SetItemCount_complex(count, color, reqCount, num, (byte)displayOne != 0, (byte)displayZero != 0);
	}

	private void SetItemCount_complex(long count, Color color, int reqCount = 0, long maxCount = 0, bool displayOne = false, bool displayZero = false)
	{
		ItemCount = count;
		ItemMaxCount = maxCount;
		bool flag = displayOne;
		bool flag2 = displayZero;
		string text;
		if (reqCount > 0)
		{
			text = $"{ItemCount}/{reqCount}";
		}
		else
		{
			long itemMaxCount = ItemMaxCount;
			int num = 0;
			if (itemMaxCount > num && ItemMaxCount != ItemCount)
			{
				text = $"{ItemCount}-{ItemMaxCount}";
			}
			else
			{
				if (ItemCount != 0L || flag2)
				{
					long itemCount = ItemCount;
					int num2 = 1;
					if (itemCount != num2 || flag)
					{
						text = ItemCount.ToString();
						goto IL_00da;
					}
				}
				text = string.Empty;
			}
		}
		goto IL_00da;
		IL_00da:
		SetItemCountSubComponent(text, color);
	}

	private void SetItemCountSubComponent(string text, Color color)
	{
		_subProxyContainer.Get<ItemCountSubComponent.ComponentProxy>()?.Set(text, color);
	}

	public void LargeSetItemMinMax(int count, int maxCount = 0)
	{
		ItemCount = count;
		ItemMaxCount = maxCount;
		long itemMaxCount = ItemMaxCount;
		int num = 0;
		string countText;
		if (itemMaxCount > num && ItemMaxCount != ItemCount)
		{
			countText = $"{ItemCount}-{ItemMaxCount}";
		}
		else
		{
			if (ItemCount != 0L)
			{
				long itemCount = ItemCount;
				int num2 = 1;
				if (itemCount != num2)
				{
					countText = ItemCount.ToString();
					goto IL_00aa;
				}
			}
			countText = string.Empty;
		}
		goto IL_00aa;
		IL_00aa:
		_subProxyContainer.Get<ItemCountSubComponent2.ComponentProxy>()?.Set(countText, Color.white);
	}

	private void InitTier()
	{
		int tier = 0;
		bool isValance = false;
		if (EquipItemData != null)
		{
			tier = EquipItemData.Tier;
			isValance = EquipItemData.WorkPlace == EquipItemWorkPlace.Valance;
		}
		else if (ItemData != null)
		{
			tier = ItemData.Tier;
		}
		_subProxyContainer.Get<TierSubComponent.ComponentProxy>()?.Set(tier, isValance);
	}

	public void InitItemLocked()
	{
		bool isVisible = false;
		if (EquipItemInfo != null)
		{
			isVisible = EquipItemInfo.Locked != 0;
		}
		else if (ItemData != null)
		{
			ItemInfo itemInfo = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Get(ItemData.Index);
			isVisible = itemInfo != null && itemInfo.Locked != 0;
		}
		_subProxyContainer.Get<ItemLockedSubComponent.ComponentProxy>()?.Set(isVisible);
	}

	public void OnClickIcon()
	{
		if (_listener != null)
		{
			_listener.OnClick(this);
		}
	}

	public void OnShowTooltip()
	{
		if (_itemAnimation == null || _itemAnimation.CanNext)
		{
			if (_tooltipTextProxy == null)
			{
				_tooltipTextProxy = new TooltipTextInfoProxy();
			}
			ItemTooltipTextInfo info = _tooltipTextProxy.GetInfo(ItemData, EquipItemInfo, CachedHeroInfo);
			ItemIconTooltip.Open(WidgetThis, info);
			if (_listener is TooltipListener tooltipListener)
			{
				tooltipListener.OnShowTooltip(this);
			}
		}
	}

	public void OnHideTooltip()
	{
		ItemIconTooltip.Hide(WidgetThis);
	}

	public bool ApplyEquipItemLocked()
	{
		if (EquipItemInfo == null)
		{
			return false;
		}
		if (EquipItemInfo.Locked != 0)
		{
			string text = LocalizationManager.GetString(Item.NOT_LOCK);
			SetCustomLabel(text, Color.green);
			return true;
		}
		return false;
	}

	public void ApplyNotForSale(bool apply)
	{
		if (!apply)
		{
			HideCustomLabel();
		}
		else
		{
			if (ApplyEquipItemLocked())
			{
				return;
			}
			if (ItemData == null)
			{
				HideCustomLabel();
				return;
			}
			ItemInfo itemInfo = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Get(ItemData.Index);
			if (itemInfo != null && itemInfo.Locked != 0)
			{
				string text = LocalizationManager.GetString(Item.NOT_LOCK);
				SetCustomLabel(text, Color.green);
			}
			else if (ItemData.NotForSale || (EquipItemInfo != null && !EquipItemInfo.IsSellable))
			{
				string text2 = LocalizationManager.GetString(Item.NOT_FOR_SALE);
				SetCustomLabel(text2, Color.green);
			}
			else
			{
				HideCustomLabel();
			}
		}
	}

	public void ApplyBreakable(bool apply)
	{
		if (!apply)
		{
			HideCustomLabel();
		}
		else
		{
			if (ApplyEquipItemLocked())
			{
				return;
			}
			if (ItemData == null)
			{
				HideCustomLabel();
				return;
			}
			ItemInfo itemInfo = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Get(ItemData.Index);
			if (itemInfo != null && itemInfo.Locked != 0)
			{
				string text = LocalizationManager.GetString(Item.NOT_LOCK);
				SetCustomLabel(text, Color.green);
			}
			else if (!ItemData.Breakable || (EquipItemInfo != null && !EquipItemInfo.IsBreakable))
			{
				string text2 = LocalizationManager.GetString(Item.NOT_BREAKABLE);
				SetCustomLabel(text2, Color.green);
			}
			else
			{
				HideCustomLabel();
			}
		}
	}

	public void ApplyKeepable()
	{
		if (EquipItemInfo != null && !EquipItemInfo.IsKeepable)
		{
			string text = LocalizationManager.GetString(Item.NOT_CHEST);
			SetCustomLabel(text, Color.green);
		}
		else
		{
			HideCustomLabel();
		}
	}

	public void SetSellSelected(bool selected)
	{
		SellSelectSubComponent.ComponentProxy componentProxy = _subProxyContainer.Get<SellSelectSubComponent.ComponentProxy>();
		if (componentProxy != null)
		{
			int num = 0;
			if (selected)
			{
				num = GlobalAutoBehaviour<DropBonusEventManager>.instance.GetCheckEquipSellEventBonus(ItemData);
			}
			componentProxy.Set(selected, checked(num * ItemCount));
		}
		bool isShow = false;
		if (selected)
		{
			isShow = GlobalAutoBehaviour<DropBonusEventManager>.instance.IsEffectiveEvent(EventBonusType.BonusEquipSellReward);
		}
		ShowEventBonus(isShow);
	}

	public void SetBreakSelected(bool selected)
	{
		_subProxyContainer.Get<BreakSelectSubComponent.ComponentProxy>()?.Set(selected);
		if (EquipItemData == null || EquipItemInfo == null)
		{
			return;
		}
		EquipItemBreakData data = GameRoot.Instance.EquipItemBreakDataContainer.GetData(EquipItemData.Tier, EquipItemData.DetailIndex, ItemData.Code, EquipItemInfo.Star, EquipItemData.SetIndex);
		if (data != null)
		{
			bool isShow = false;
			if (selected)
			{
				isShow = GlobalAutoBehaviour<DropBonusEventManager>.instance.GetEquipBreakEventBonus(data.ItemCode1) != null;
			}
			ShowEventBonus(isShow);
		}
	}

	public void ShowEventBonus(bool isShow)
	{
		_subProxyContainer.Get<EventBonusSubComponent.ComponentProxy>()?.Set(isShow);
	}

	public void ApplyCustomOption(int itemCustomOptionIndex)
	{
		if (itemCustomOptionIndex != 0)
		{
			EquipItemInfo customEquipItem = GameRoot.Instance.CustomEquipItemDataContainer.GetCustomEquipItem(itemCustomOptionIndex, 0, ItemData.Index);
			if (customEquipItem != null)
			{
				EquipItemInfo = customEquipItem;
				Refresh();
			}
		}
	}

	public void UpdateStatDiffByHero(CachedHeroInfo cachedHeroInfo)
	{
		if (EquipItemInfo == null || EquipItemData == null || cachedHeroInfo == null)
		{
			ShowStatDiff(isVisible: false, CompareEquipItem.Worse);
			return;
		}
		EquipPartType partType = EquipItemData.PartType;
		EquipItemInfo equipItemInfoByPartType = cachedHeroInfo.GetEquipItemInfoByPartType(partType);
		if (equipItemInfoByPartType == null)
		{
			ShowStatDiff(isVisible: true, CompareEquipItem.Better);
			return;
		}
		if (equipItemInfoByPartType == EquipItemInfo)
		{
			ShowStatDiff(isVisible: false, CompareEquipItem.Worse);
			return;
		}
		ItemData<EquipItemData> data = GameRoot.Instance.ItemDataContainer.GetData<EquipItemData>(equipItemInfoByPartType.ItemIndex);
		if (data == null)
		{
			ShowStatDiff(isVisible: false, CompareEquipItem.Worse);
			return;
		}
		if (data.SubData == null || (data.SubData.PartType == EquipPartType.Accessory && data.SubData.SubType != EquipItemData.SubType))
		{
			ShowStatDiff(isVisible: false, CompareEquipItem.Worse);
			return;
		}
		long equipItemScore = equipItemInfoByPartType.GetEquipItemScore(cachedHeroInfo.CreatureData);
		long equipItemScore2 = EquipItemInfo.GetEquipItemScore(cachedHeroInfo.CreatureData);
		CompareEquipItem compareValue = ((equipItemScore2 <= equipItemScore) ? CompareEquipItem.Worse : CompareEquipItem.Better);
		ShowStatDiff(equipItemScore2 != equipItemScore, compareValue);
	}

	public void UpdateStateByHero(CachedHeroInfo cachedHeroInfo)
	{
		SetSelected(selected: false);
		SetDisabled(disabled: false);
		SetClassTagError(classTagError: false);
		SetLowLevel(lowlevel: false);
		ShowStatDiff(isVisible: false, CompareEquipItem.Worse);
		SetSellSelected(selected: false);
		SetBreakSelected(selected: false);
		if (EquipItemData != null && EquipItemInfo != null && cachedHeroInfo != null)
		{
			switch (cachedHeroInfo.CheckEquipResult(EquipItemData))
			{
			case CachedHeroInfo.EquipResult.Fail:
				SetDisabled(disabled: true);
				break;
			case CachedHeroInfo.EquipResult.TagMismatch:
			case CachedHeroInfo.EquipResult.CreatureMismatch:
				SetClassTagError(classTagError: true);
				break;
			case CachedHeroInfo.EquipResult.LowLevel:
			{
				short reqLevel = EquipItemData.ReqLevel;
				SetLowLevel(lowlevel: true, reqLevel);
				break;
			}
			default:
				UpdateStatDiffByHero(cachedHeroInfo);
				break;
			}
		}
	}

	public void SetStarBlink(bool isEnabled = true)
	{
		if (EquipItemInfo != null)
		{
			_subProxyContainer.Get<StarBlinkSubComponent.ComponentProxy>()?.Set(isEnabled);
		}
	}

	public void SetIconGrayScale(bool grayScale)
	{
		UIHelper.SetWidgetGrayScale(RootWidget, grayScale);
	}

	public void SetSubKey(int subKey)
	{
		SubKey = subKey;
	}

	public void PlayUseEffect(Action onEnd)
	{
		EffectSubComponent.ComponentProxy componentProxy = _subProxyContainer.Get<EffectSubComponent.ComponentProxy>();
		if (componentProxy != null)
		{
			UIGame3DWidget uIGame3DWidget = componentProxy.EnableAndGet(EffectWidgetType.UseItem);
			if (uIGame3DWidget != null)
			{
				uIGame3DWidget.Play(onEnd);
			}
		}
	}

	public void ActiveHeroItemEffect(bool isActive)
	{
		EffectSubComponent.ComponentProxy componentProxy = _subProxyContainer.Get<EffectSubComponent.ComponentProxy>();
		if (componentProxy == null)
		{
			return;
		}
		UIGame3DWidget uIGame3DWidget = componentProxy.EnableAndGet(EffectWidgetType.HeroItem);
		if (!(uIGame3DWidget == null))
		{
			if (isActive)
			{
				uIGame3DWidget.Play(null);
			}
			else
			{
				uIGame3DWidget.Stop(immediatelyClear: false);
			}
		}
	}

	public void ShowEquipItemBreakBonusIcon(bool isShow)
	{
		_subProxyContainer.Get<EventBonusSubComponent.ComponentProxy>()?.Set(isShow);
	}

	private void InitEquipItemSlotIndex()
	{
		if (EquipItemInfo != null)
		{
			_subProxyContainer.Get<ItemSlotIndexSubComponent.ComponentProxy>()?.Set(EquipItemInfo.SlotIndex, (EquipItemInfo.EquipItemData != null) ? EquipItemInfo.EquipItemData.SetGroupType : EquipItemSetGroupType.None);
		}
	}

	public void SetEquipped(bool isShow)
	{
		if (EquipItemInfo.IsEquipped)
		{
			ShowEquipped(isShow);
		}
	}

	public void ShowEquipped(bool isShow)
	{
		_subProxyContainer.Get<EquippedSubComponent.ComponentProxy>()?.Set(isShow);
	}

	private NGame2.NGameSetting.NUi.GetItemAnimationData GetItemAnimationData(EffectWidgetType effectType)
	{
		NGame2.NGameSetting.NUi.GetItemAnimationData result = GameSetting.data.Ui.GetItemAnimation;
		switch (effectType)
		{
		case EffectWidgetType.GetItem:
			result = GameSetting.data.Ui.GetItemAnimation;
			break;
		case EffectWidgetType.GetRareItem:
			result = GameSetting.data.Ui.GetRareItemAnimation;
			break;
		case EffectWidgetType.GetUniqueItem:
			result = GameSetting.data.Ui.GetUniqueItemAnimation;
			break;
		case EffectWidgetType.GetUniqueItemAwesome:
			result = GameSetting.data.Ui.GetUniqueItemAwesome;
			break;
		case EffectWidgetType.GetUniqueItemPerfect:
			result = GameSetting.data.Ui.GetUniqueItemPerfect;
			break;
		case EffectWidgetType.GetClassUniqueItem:
			result = GameSetting.data.Ui.GetClassUniqueItemAnimation;
			break;
		}
		return result;
	}
}
