using System;
using System.Collections.Generic;
using System.Linq;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NHelper;
using NGame2.NUI.NManager;
using NNguiExtension;
using NShared;
using NShared.NLocalization;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class HeroRunehengeWindow : BaseWindow, HeroRuneListPanel.IListener, HeroRuneListPanel.IDataProvider, HeroRuneStatPanel.IDataProvider, HeroRuneItemInfo.IListener
{
	public interface IListener
	{
		void RequestEquipRune(HeroRunehengeWindow caller, int heroIndex, int itemIndex, int equipItemSlotIndex, int preserveEquipItemSlotIndex, RuneSlotType slot, int runePage, bool isPreserve, Action onCompelete);

		void RequestUnequipRune(HeroRunehengeWindow caller, int heroIndex, int itemIndex, int equipItemSlotIndex, RuneSlotType slot, int runePage, bool isPreserve);

		void RequestApplyRunePage(HeroRunehengeWindow caller, int heroIndex, int runePageIndex);

		void RequestExtendRunePage(HeroRunehengeWindow caller, int heroIndex, int runePageIndex);
	}

	private enum EBehaviorMode
	{
		None,
		Equip,
		EquipInEmptySlot
	}

	public class BehaviorData
	{
		public ItemData ItemData;

		public EquipItemInfo EquipItemInfo;

		public BehaviorData()
		{
		}

		public BehaviorData(ItemData itemData, EquipItemInfo equipItemInfo)
		{
			ItemData = itemData;
			EquipItemInfo = equipItemInfo;
		}
	}

	public enum EToggle
	{
		None,
		RuneList,
		RuneStat
	}

	[Serializable]
	public class Category
	{
		public EToggle Type;

		public GameObject Root;

		public UIToggle Toggle;
	}

	[Serializable]
	public class RuneSlotData
	{
		public RuneSlotType Slot;

		public UIButton Button_Slot;

		public UITexture Texture_Icon;

		public UISprite Sprite_Active;

		public UIGame3DWidget Effect_EquipStart;

		public UIGame3DWidget Effect_EquipLoop;

		public ItemData ItemData;

		public EquipItemInfo EquipItemInfo;
	}

	[Serializable]
	public class RunePageData
	{
		public enum RunePageState
		{
			Select,
			NotSelect,
			Unavailable
		}

		public int RunePageIndex;

		public GameObject PageRoot;

		public UIButton Button_RunPage;

		public UIButton Button_Add;

		public UISpriteSelector SpriteSelector;

		private RunePageState State;

		public void ChangeRunePageState(RunePageState state)
		{
			switch (state)
			{
			case RunePageState.Unavailable:
				UIHelper.EnableGameObject(Button_Add, enable: true);
				UIHelper.EnableGameObject(Button_RunPage, enable: false);
				break;
			case RunePageState.Select:
			case RunePageState.NotSelect:
				UIHelper.EnableGameObject(Button_Add, enable: false);
				UIHelper.EnableGameObject(Button_RunPage, enable: true);
				SpriteSelector.SelectByIndex((int)state);
				break;
			}
			State = state;
		}
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Hero/HeroRunehenge";

	public HeroRuneListPanel RuneListPanel;

	public HeroRuneStatPanel RuneStatPanel;

	public List<RunePageData> RunePageDatas;

	public List<RuneSlotData> RuneSlotDatas;

	public Category[] Categories;

	public GameObject EquipHeroRuneItemInfoRoot;

	public GameObject UnequipHeroRuneItemInfoRoot;

	public GameObject ToggleRoot;

	public UILabel Label_HeroName;

	public UIGrid Grid_RunePage;

	private CachedHeroInfo _heroInfo;

	private int _currentMaxRunePage;

	private EToggle _toggle;

	private EBehaviorMode _behaviorMode;

	private BehaviorData _behaviorData;

	private IListener _listener;

	private List<HeroRuneItemInfo> _runeItemInfos = new List<HeroRuneItemInfo>();

	private Dictionary<RuneSlotType, RuneSlotData> _runeSlotDataDic = new Dictionary<RuneSlotType, RuneSlotData>();

	private Dictionary<int, RunePageData> _runePageDataDic = new Dictionary<int, RunePageData>();

	public int HeroIndex
	{
		get
		{
			if (_heroInfo == null)
			{
				return 0;
			}
			return _heroInfo.HeroIndex;
		}
	}

	public int ApplyRunePage
	{
		get
		{
			if (_heroInfo == null)
			{
				return RuneHelper.GetDefaultRunePage();
			}
			if (_heroInfo.ApplyRunePage != 0)
			{
				return _heroInfo.ApplyRunePage;
			}
			return RuneHelper.GetDefaultRunePage();
		}
	}

	public void Open(IListener listener, CachedHeroInfo heroInfo)
	{
		base.Open(delegate
		{
			_heroInfo = heroInfo;
			_listener = listener;
			Init();
		});
	}

	private void Init()
	{
		if (RuneListPanel != null)
		{
			RuneListPanel.Open(this, this);
		}
		if (RuneStatPanel != null)
		{
			RuneStatPanel.Open(this);
		}
		_runeSlotDataDic.Clear();
		_runePageDataDic.Clear();
		foreach (RuneSlotData runeSlotData in RuneSlotDatas)
		{
			_runeSlotDataDic[runeSlotData.Slot] = runeSlotData;
		}
		foreach (RunePageData runePageData in RunePageDatas)
		{
			_runePageDataDic[runePageData.RunePageIndex] = runePageData;
		}
		SetHeroName();
		SetCategory(EToggle.RuneStat);
		Refresh();
		RightPanelRefresh();
		CloseRuneItemInfos();
	}

	public void Refresh()
	{
		SetBehaviorMode(EBehaviorMode.None, null);
		ActiveRuneSlot(null);
		SetupRunePage();
		SetupRuneLoopEffect();
	}

	public void RightPanelRefresh()
	{
		if (RuneListPanel != null)
		{
			RuneListPanel.Refresh();
		}
		if (RuneStatPanel != null)
		{
			RuneStatPanel.Refresh();
		}
	}

	private void SetupRunePage()
	{
		if (_heroInfo == null)
		{
			return;
		}
		Dictionary<int, List<HeroRunePageInfo>> dictionary = new Dictionary<int, List<HeroRunePageInfo>>();
		int maxRunePage = RuneHelper.GetMaxRunePage();
		_currentMaxRunePage = 0;
		if (RunePageDatas != null)
		{
			foreach (RunePageData runePageData2 in RunePageDatas)
			{
				UIHelper.EnableGameObject(runePageData2.PageRoot, runePageData2.RunePageIndex <= maxRunePage);
			}
		}
		if (Grid_RunePage != null)
		{
			Grid_RunePage.Reposition();
			Grid_RunePage.repositionNow = true;
		}
		for (int i = 1; i <= maxRunePage; i++)
		{
			List<HeroRunePageInfo> runePageInfos = _heroInfo.GetRunePageInfos(i);
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(runePageInfos))
			{
				dictionary.Add(i, runePageInfos);
				_currentMaxRunePage = Math.Max(i, _currentMaxRunePage);
			}
		}
		for (int j = 1; j <= maxRunePage; j++)
		{
			RunePageData runePageData = GetRunePageData(j);
			if (runePageData != null)
			{
				if (!dictionary.ContainsKey(j))
				{
					runePageData.ChangeRunePageState(RunePageData.RunePageState.Unavailable);
					UIHelper.EnableButton(runePageData.Button_Add, j <= _currentMaxRunePage + 1);
				}
				else if (j == ApplyRunePage)
				{
					runePageData.ChangeRunePageState(RunePageData.RunePageState.Select);
				}
				else
				{
					runePageData.ChangeRunePageState(RunePageData.RunePageState.NotSelect);
				}
			}
		}
		if (dictionary.ContainsKey(ApplyRunePage))
		{
			SetupRuneSlot(dictionary[ApplyRunePage]);
		}
	}

	private void SetupRuneSlot(List<HeroRunePageInfo> infos)
	{
		foreach (HeroRunePageInfo info in infos)
		{
			RuneSlotData runeSlotData = GetRuneSlotData((RuneSlotType)info.SlotNum);
			if (runeSlotData == null)
			{
				continue;
			}
			ItemData data = GameRoot.Instance.ItemDataContainer.GetData(info.ItemIndex);
			if (data == null)
			{
				UIHelper.SetTexture(runeSlotData.Texture_Icon, null);
			}
			else if (data.Type == ItemType.Rune || data.Type == ItemType.PunishmentRune)
			{
				runeSlotData.ItemData = data;
				runeSlotData.EquipItemInfo = null;
				UIHelper.ResetPlaneAssetTextureAsync(this, runeSlotData.Texture_Icon, UIHelper.GetItemIcon2(data.Code));
				if (data.Type == ItemType.PunishmentRune)
				{
					runeSlotData.EquipItemInfo = GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Get(info.EquipItemSlotIndex);
				}
			}
			else
			{
				runeSlotData.ItemData = null;
				runeSlotData.EquipItemInfo = null;
				UIHelper.SetTexture(runeSlotData.Texture_Icon, null);
			}
		}
	}

	private RuneSlotData GetRuneSlotData(RuneSlotType slot)
	{
		if (_runeSlotDataDic.ContainsKey(slot))
		{
			return _runeSlotDataDic[slot];
		}
		return null;
	}

	private RunePageData GetRunePageData(int pageIndex)
	{
		if (_runePageDataDic.ContainsKey(pageIndex))
		{
			return _runePageDataDic[pageIndex];
		}
		return null;
	}

	public override void Reset()
	{
		base.Reset();
	}

	public override void Close()
	{
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(_runeItemInfos))
		{
			FactoryBase.ReleaseAndClearList(_runeItemInfos);
			_runeItemInfos.Clear();
			return;
		}
		if (RuneListPanel != null)
		{
			RuneListPanel.Close();
		}
		if (RuneStatPanel != null)
		{
			RuneStatPanel.Close();
		}
		if (_heroInfo != null)
		{
			_heroInfo.RefreshRuneInfo();
		}
		base.Close();
	}

	private void SetCategory(EToggle toggle)
	{
		if (toggle == EToggle.None)
		{
			return;
		}
		Category[] categories = Categories;
		foreach (Category category in categories)
		{
			if (category != null && !(category.Toggle == null))
			{
				category.Toggle.value = category.Type == toggle;
				UIHelper.EnableGameObject(category.Root, category.Toggle.value);
			}
		}
		if (_toggle != toggle)
		{
			_toggle = toggle;
			switch (_toggle)
			{
			case EToggle.RuneList:
				if (RuneListPanel != null)
				{
					RuneListPanel.Refresh();
					RuneListPanel.ResetScroll();
				}
				break;
			case EToggle.RuneStat:
				if (RuneStatPanel != null)
				{
					RuneStatPanel.Refresh();
				}
				break;
			}
		}
		SetBehaviorMode(EBehaviorMode.None, null);
		ActiveRuneSlot(null);
	}

	private void SetHeroName()
	{
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(_heroInfo.HeroIndex);
		if (data != null)
		{
			HeroProfileData data2 = GameRoot.Instance.HeroProfileDataContainer.GetData(_heroInfo.HeroIndex);
			string empty = string.Empty;
			empty = ((data2 == null || String_Extension_Method.IsNullOrEmpty(data2.HeroName)) ? data.Name : LocalizationManager.GetStringDirect(data2.HeroName));
			UIHelper.SetLabelText(Label_HeroName, LocalizationManager.GetStringDirect("RUNE_PAGE_TITLE", empty));
		}
	}

	private void SetBehaviorMode(EBehaviorMode mode, BehaviorData data)
	{
		_behaviorMode = mode;
		_behaviorData = data;
	}

	private RuneSlotType GetActivedEmptyRuneSlot()
	{
		foreach (RuneSlotData runeSlotData in RuneSlotDatas)
		{
			if (runeSlotData.ItemData == null && runeSlotData.Sprite_Active.isActiveAndEnabled)
			{
				return runeSlotData.Slot;
			}
		}
		return RuneSlotType.None;
	}

	private List<RuneSlotType> GetActivedRuneSlots()
	{
		List<RuneSlotType> list = new List<RuneSlotType>();
		foreach (RuneSlotData runeSlotData in RuneSlotDatas)
		{
			if (runeSlotData.Sprite_Active.isActiveAndEnabled)
			{
				list.Add(runeSlotData.Slot);
			}
		}
		return list;
	}

	private void ActiveRuneSlot(RuneSlotType[] types)
	{
		foreach (RuneSlotData runeSlotData2 in RuneSlotDatas)
		{
			UIHelper.EnableGameObject(runeSlotData2.Sprite_Active, enable: false);
		}
		if (_toggle != EToggle.RuneList || __Extension_Method_ICollection__.IsNullOrEmpty(types))
		{
			return;
		}
		foreach (RuneSlotType runeSlotType in types)
		{
			RuneSlotData runeSlotData = GetRuneSlotData(runeSlotType);
			if (runeSlotData != null)
			{
				UIHelper.EnableGameObject(runeSlotData.Sprite_Active, runeSlotData.Slot == runeSlotType);
			}
		}
	}

	private void OpenUnequipRuneItemInfo(RuneSlotType slotType, ItemData itemData, EquipItemInfo equipItemInfo)
	{
		if (!(UnequipHeroRuneItemInfoRoot == null))
		{
			HeroRuneItemInfo heroRuneItemInfo = HeroRuneItemInfo.PrefabAuto.Factory.Create(UnequipHeroRuneItemInfoRoot.transform);
			int num = 0;
			heroRuneItemInfo.Open(this, slotType, HeroRuneItemInfo.EMode.Unequip, itemData, num, equipItemInfo);
			_runeItemInfos.Add(heroRuneItemInfo);
		}
	}

	private void OpenEquipRuneItemInfo(ItemData itemData, long itemCount, EquipItemInfo equipItemInfo)
	{
		if (!(EquipHeroRuneItemInfoRoot == null))
		{
			HeroRuneItemInfo heroRuneItemInfo = HeroRuneItemInfo.PrefabAuto.Factory.Create(EquipHeroRuneItemInfoRoot.transform);
			heroRuneItemInfo.Open(this, RuneSlotType.None, HeroRuneItemInfo.EMode.Equip, itemData, itemCount, equipItemInfo);
			_runeItemInfos.Add(heroRuneItemInfo);
		}
	}

	public void CloseRuneItemInfos()
	{
		CloseRuneItemInfo(HeroRuneItemInfo.EMode.Equip);
		CloseRuneItemInfo(HeroRuneItemInfo.EMode.Unequip);
	}

	private void CloseRuneItemInfo(HeroRuneItemInfo.EMode mode)
	{
		HeroRuneItemInfo heroRuneItemInfo = _runeItemInfos.Find((HeroRuneItemInfo x) => mode == x.Mode);
		if (!(heroRuneItemInfo == null))
		{
			heroRuneItemInfo.Close();
		}
	}

	private HeroRuneItemInfo GetEquipRuneItemInfo(HeroRuneItemInfo.EMode mode)
	{
		return _runeItemInfos.Find((HeroRuneItemInfo x) => mode == x.Mode);
	}

	public void OnClickClose()
	{
		Close();
	}

	public void OnChangeToggle()
	{
		EToggle eToggle = EToggle.None;
		Category[] categories = Categories;
		foreach (Category category in categories)
		{
			if (category != null && !(category.Toggle == null) && category.Toggle.value)
			{
				eToggle = category.Type;
			}
		}
		if (_toggle != eToggle)
		{
			SetCategory(eToggle);
		}
	}

	public void SetupRuneLoopEffect()
	{
		foreach (KeyValuePair<RuneSlotType, RuneSlotData> item in _runeSlotDataDic)
		{
			UIHelper.EnableGameObject(item.Value.Effect_EquipLoop, item.Value.ItemData != null && item.Value.ItemData.Type == ItemType.PunishmentRune);
		}
	}

	public void StartRuneEquipEffect(RuneSlotType slotType)
	{
		if (_runeSlotDataDic.ContainsKey(slotType))
		{
			RuneSlotData runeSlotData = _runeSlotDataDic[slotType];
			if (!(runeSlotData.Effect_EquipStart == null))
			{
				runeSlotData.Effect_EquipStart.Play(null);
			}
		}
	}

	public void OnClickRuneSlot(UIButton button)
	{
		RuneSlotData runeSlotData = RuneSlotDatas.Find((RuneSlotData x) => x.Button_Slot == button);
		switch (_behaviorMode)
		{
		case EBehaviorMode.None:
		case EBehaviorMode.EquipInEmptySlot:
			if (runeSlotData.ItemData == null && _toggle == EToggle.RuneList)
			{
				SetBehaviorMode(EBehaviorMode.EquipInEmptySlot, null);
				ActiveRuneSlot(new RuneSlotType[1] { runeSlotData.Slot });
			}
			else if (runeSlotData.ItemData != null)
			{
				SetBehaviorMode(EBehaviorMode.None, null);
				OpenUnequipRuneItemInfo(runeSlotData.Slot, runeSlotData.ItemData, runeSlotData.EquipItemInfo);
				ActiveRuneSlot(new RuneSlotType[1] { runeSlotData.Slot });
			}
			if (RuneListPanel != null && _toggle == EToggle.RuneList)
			{
				new List<RuneSlotType>().Add(runeSlotData.Slot);
				RuneListPanel.Refresh(immediate: false);
			}
			break;
		case EBehaviorMode.Equip:
		{
			if (_behaviorData == null)
			{
				break;
			}
			List<RuneSlotType> activedRuneSlots = GetActivedRuneSlots();
			if (activedRuneSlots.Contains(runeSlotData.Slot) && activedRuneSlots.Contains(runeSlotData.Slot) && _behaviorData.ItemData != null)
			{
				if (runeSlotData.ItemData == null)
				{
					ProcessEquipRune(runeSlotData.Slot, _behaviorData.ItemData, _behaviorData.EquipItemInfo, null);
				}
				else
				{
					ProcessUnequipRune(runeSlotData.ItemData, runeSlotData.EquipItemInfo, _behaviorData.ItemData, _behaviorData.EquipItemInfo, runeSlotData.Slot, isExchange: true);
				}
			}
			break;
		}
		}
	}

	private void ProcessEquipRune(RuneSlotType slot, ItemData itemData, EquipItemInfo eqiupItemInfo, EquipItemInfo preserveEquipItemInfo)
	{
		if (_listener == null || !RuneHelper.IsCanEquipSlotTypes(itemData, slot))
		{
			return;
		}
		int heroIndex = HeroIndex;
		int index = itemData.Index;
		int equipItemSlotIndex = eqiupItemInfo?.SlotIndex ?? 0;
		int preserveEquipItemSlotIndex = preserveEquipItemInfo?.SlotIndex ?? 0;
		_listener.RequestEquipRune(this, heroIndex, index, equipItemSlotIndex, preserveEquipItemSlotIndex, slot, ApplyRunePage, isPreserve: false, delegate
		{
			if (itemData.Type == ItemType.PunishmentRune)
			{
				StartRuneEquipEffect(slot);
			}
		});
	}

	private void ProcessUnequipRune(ItemData dest, EquipItemInfo destEquipItemInfo, ItemData src, EquipItemInfo srcEqiupItemInfo, RuneSlotType slot, bool isExchange)
	{
		if (dest == null)
		{
			return;
		}
		RunePriceInfo priceInfo = RuneHelper.GetRunePriceInfo(dest);
		if (priceInfo == null)
		{
			return;
		}
		if (priceInfo.IsFreeKeepRune)
		{
			ProcessUnequipRune2(dest, destEquipItemInfo, src, srcEqiupItemInfo, slot, priceInfo.Price, preserve: true, isExchange);
			return;
		}
		string content = LocalizationManager.GetString(isExchange ? Rune.RUNE_REPLACE_ASK_MESSAGE : Rune.RUNE_UNEQUIP_ASK_MESSAGE);
		string stringDirect = LocalizationManager.GetStringDirect("RUNE_UNEQUIP_DELETE");
		PreserveMessagePopup.Flags flags = PreserveMessagePopup.Flags.None;
		if (priceInfo.IsDiscountKeepRuneEvent)
		{
			flags |= PreserveMessagePopup.Flags.AttachEventSprite;
		}
		if (destEquipItemInfo == null || (destEquipItemInfo != null && destEquipItemInfo.Locked == 0))
		{
			flags |= PreserveMessagePopup.Flags.EnableRuneRemoveButton;
		}
		bool flag = dest != null && dest.Type == ItemType.PunishmentRune;
		bool flag2 = src != null && src.Type == ItemType.PunishmentRune;
		if (GlobalAutoBehaviour<AccountManager>.instance.IsMaxPunishmentRuneItemCount())
		{
			if (!flag)
			{
				flags |= PreserveMessagePopup.Flags.EnableRunePreserveButton;
			}
			else if (flag && flag2 && isExchange)
			{
				flags |= PreserveMessagePopup.Flags.EnableRunePreserveButton;
			}
		}
		else
		{
			flags |= PreserveMessagePopup.Flags.EnableRunePreserveButton;
		}
		PreserveMessagePopup.Open(content, stringDirect, delegate(bool preserve)
		{
			ProcessUnequipRune2(dest, destEquipItemInfo, src, srcEqiupItemInfo, slot, priceInfo.Price, preserve, isExchange);
		}, null, flags);
	}

	private void ProcessUnequipRune2(ItemData dest, EquipItemInfo destEquipItemInfo, ItemData src, EquipItemInfo srcEqiupItemInfo, RuneSlotType slot, int price, bool preserve, bool isExchange)
	{
		if (preserve && price > 0)
		{
			if (!IsEnoughCurrency(price))
			{
				string text = LocalizationManager.GetString(NShared.NLocalization.Common.ERROR);
				string message = LocalizationManager.GetString(NShared.NLocalization.System.MOVE_GEM_SHOP_CONFIRM);
				MessagePopup.Open(text, message, delegate
				{
					SceneAutoBehaviour<PayShopManagement>.instance.OpenNewPayShop(NewPayShopCategoryType.Ruby.ToString());
				}, null);
				return;
			}
			string text2 = LocalizationManager.GetString(Rune.RUNE_UNEQUIP_ASK_TITLE);
			string content = LocalizationManager.GetString(Rune.RUNE_PRESERVE_ASK_MESSAGE, price);
			PreserveMessagePopup2.Open(text2, content, PreserveMessagePopup2.EPriceType.Gem, price, delegate
			{
				ProcessUnequipRune3(dest, destEquipItemInfo, src, srcEqiupItemInfo, slot, price, preserve, isExchange);
			});
		}
		else
		{
			ProcessUnequipRune3(dest, destEquipItemInfo, src, srcEqiupItemInfo, slot, price, preserve, isExchange);
		}
	}

	private void ProcessUnequipRune3(ItemData dest, EquipItemInfo destEquipItemInfo, ItemData src, EquipItemInfo srcEqiupItemInfo, RuneSlotType slot, int price, bool preserve, bool isExchange)
	{
		if (!isExchange)
		{
			if (dest != null && _listener != null)
			{
				int equipItemSlotIndex = destEquipItemInfo?.SlotIndex ?? 0;
				_listener.RequestUnequipRune(this, _heroInfo.HeroIndex, dest.Index, equipItemSlotIndex, slot, _heroInfo.ApplyRunePage, preserve);
			}
		}
		else
		{
			if (dest == null || src == null || _listener == null)
			{
				return;
			}
			int equipItemSlotIndex2 = srcEqiupItemInfo?.SlotIndex ?? 0;
			int preserveEquipItemSlotIndex = destEquipItemInfo?.SlotIndex ?? 0;
			_listener.RequestEquipRune(this, _heroInfo.HeroIndex, src.Index, equipItemSlotIndex2, preserveEquipItemSlotIndex, slot, _heroInfo.ApplyRunePage, preserve, delegate
			{
				if (src.Type == ItemType.PunishmentRune)
				{
					StartRuneEquipEffect(slot);
				}
			});
		}
	}

	private bool IsEnoughCurrency(int price)
	{
		return GlobalAutoBehaviour<AccountManager>.instance.UserManager.TotalGem >= price;
	}

	public void OnClickSelectRunePage(UIButton button)
	{
		RunePageData runePageData = RunePageDatas.Find((RunePageData x) => x.Button_RunPage == button);
		if (runePageData != null && _heroInfo != null && _heroInfo.ApplyRunePage != runePageData.RunePageIndex && _listener != null)
		{
			_listener.RequestApplyRunePage(this, _heroInfo.HeroIndex, runePageData.RunePageIndex);
		}
	}

	public void OnClickAddRunePage(UIButton button)
	{
		if (RunePageDatas.Find((RunePageData x) => x.Button_Add == button) != null && _heroInfo != null && _listener != null)
		{
			int runePageIndex = _currentMaxRunePage + 1;
			_listener.RequestExtendRunePage(this, _heroInfo.HeroIndex, runePageIndex);
		}
	}

	void HeroRuneListPanel.IListener.OnClick(ItemIconComponent component)
	{
		if (!(component == null) && (component.ItemData.Type == ItemType.Rune || component.ItemData.Type == ItemType.PunishmentRune))
		{
			switch (component.ItemData.Type)
			{
			case ItemType.Rune:
				OpenEquipRuneItemInfo(component.ItemData, component.ItemCount, component.EquipItemInfo);
				break;
			case ItemType.PunishmentRune:
				OpenEquipRuneItemInfo(component.ItemData, component.ItemCount, component.EquipItemInfo);
				break;
			}
		}
	}

	void HeroRuneListPanel.IListener.UpdateItemIcon(ItemIconComponent component)
	{
	}

	void HeroRuneItemInfo.IListener.OnClose(HeroRuneItemInfo caller)
	{
		if (caller.Mode == HeroRuneItemInfo.EMode.Equip)
		{
			ActiveRuneSlot(null);
			SetBehaviorMode(EBehaviorMode.None, null);
		}
		HeroRuneItemInfo heroRuneItemInfo = _runeItemInfos.Find((HeroRuneItemInfo x) => x == caller);
		_runeItemInfos.Remove(heroRuneItemInfo);
		FactoryBase.ReleaseComponent(heroRuneItemInfo);
	}

	void HeroRuneItemInfo.IListener.OnEquip(HeroRuneItemInfo caller)
	{
		if (caller == null)
		{
			return;
		}
		CloseRuneItemInfo(HeroRuneItemInfo.EMode.Unequip);
		ItemData itemData = caller.ItemData;
		if (itemData == null)
		{
			return;
		}
		RuneSlotType[] runeSlotTypes = RuneHelper.GetRuneSlotTypes(itemData);
		switch (_behaviorMode)
		{
		case EBehaviorMode.EquipInEmptySlot:
		{
			RuneSlotType activedEmptyRuneSlot = GetActivedEmptyRuneSlot();
			if (activedEmptyRuneSlot != RuneSlotType.None)
			{
				if (RuneHelper.IsCanEquipSlotTypes(itemData, activedEmptyRuneSlot))
				{
					ProcessEquipRune(activedEmptyRuneSlot, itemData, caller.EquipItemInfo, null);
					break;
				}
				SetBehaviorMode(EBehaviorMode.Equip, new BehaviorData(itemData, caller.EquipItemInfo));
				ActiveRuneSlot(runeSlotTypes);
			}
			else
			{
				SetBehaviorMode(EBehaviorMode.None, null);
			}
			break;
		}
		case EBehaviorMode.None:
		case EBehaviorMode.Equip:
			SetBehaviorMode(EBehaviorMode.Equip, new BehaviorData(itemData, caller.EquipItemInfo));
			ActiveRuneSlot(runeSlotTypes);
			break;
		}
	}

	void HeroRuneItemInfo.IListener.OnUnequip(HeroRuneItemInfo caller)
	{
		if (!(caller == null) && caller.SlotType != RuneSlotType.None && caller.ItemData != null)
		{
			ProcessUnequipRune(caller.ItemData, caller.EquipItemInfo, null, null, caller.SlotType, isExchange: false);
		}
	}

	CachedHeroInfo HeroRuneStatPanel.IDataProvider.GetHeroInfo()
	{
		return _heroInfo;
	}

	int HeroRuneStatPanel.IDataProvider.GetRunePage()
	{
		return _heroInfo.ApplyRunePage;
	}

	List<RuneSlotType> HeroRuneListPanel.IDataProvider.GetActivedRuneSlotTypes()
	{
		return GetActivedRuneSlots().ToList();
	}
}
