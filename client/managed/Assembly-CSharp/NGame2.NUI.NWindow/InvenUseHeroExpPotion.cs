using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.ClassBuff;
using NShared;
using NShared.CancelFlaskItem;
using NShared.NLocalization;
using NShared.UseFlaskItem;
using NShared.UsePotionItem;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class InvenUseHeroExpPotion : BaseWindow, HeroGetExpComponent.Listener, HeroGetExpComponent.IDataProvider, ItemIconComponent.Listener, IUserObserver<NGame2.NAccount.UserHeroManager.ChangeHeroInfo>, IUserObserver, IUserObserver<ItemInfo>
{
	public const string PrefabPath = "Prefab2/Window/Inventory/InvenUseHeroExpPotion";

	public UIScrollGrid HeroExpPotionItemGrid;

	public UIScrollGrid HeroListGrid;

	private List<ItemIconComponent> _itemIconList = new List<ItemIconComponent>();

	private List<HeroGetExpComponent> _heroExpList = new List<HeroGetExpComponent>();

	private int _selectItemIndex;

	public UILabel TitleLabel;

	public UIToggle ExpPotionToggle;

	public UIToggle EmptyFlaskToggle;

	private HeroGetExpComponent.ExpStyle CurrentStyle;

	private IClassBuffListener _iClassBuffListener;

	public static void Open(HeroGetExpComponent.ExpStyle style, int selectedItemIndex, int selectedHeroIndex = 0)
	{
		WindowPrefab<InvenUseHeroExpPotion> windowPrefab = new WindowPrefab<InvenUseHeroExpPotion>("Prefab2/Window/Inventory/InvenUseHeroExpPotion", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal(style, selectedItemIndex, selectedHeroIndex);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	public void OpenInternal(HeroGetExpComponent.ExpStyle style, int selectedItemIndex, int selectedHeroIndex = 0)
	{
		base.Open();
		Init(style, selectedItemIndex, selectedHeroIndex);
	}

	public void Init(int selectedItemIndex)
	{
		HeroGetExpComponent.ExpStyle style = HeroGetExpComponent.ExpStyle.HeroExp;
		ItemData data = GameRoot.Instance.ItemDataContainer.GetData(selectedItemIndex);
		if (data != null && data.Type == ItemType.Flask)
		{
			style = HeroGetExpComponent.ExpStyle.FlaskExp;
		}
		Init(style, selectedItemIndex);
	}

	public void Init(HeroGetExpComponent.ExpStyle style, int selectedItemIndex, int selectedHeroIndex = 0)
	{
		SetupHeroList();
		ChangeStyle(style, selectedItemIndex);
		if (selectedHeroIndex != 0)
		{
			MoveHeroList(selectedHeroIndex);
		}
		_iClassBuffListener = SceneAutoBehaviour<ClassBuffManagement>.instance.ClassBuffWindowManager;
	}

	public override void Reset()
	{
		base.Reset();
		FactoryBase.ReleaseAndClearList(_itemIconList);
		FactoryBase.ReleaseAndClearList(_heroExpList);
		_selectItemIndex = 0;
		CurrentStyle = HeroGetExpComponent.ExpStyle.None;
	}

	private ItemIconComponent GetItemIconComponent(int itemIndex)
	{
		if (itemIndex == 0)
		{
			return null;
		}
		for (int i = 0; i < _itemIconList.Count; i++)
		{
			ItemIconComponent itemIconComponent = _itemIconList[i];
			if (itemIconComponent.ItemIndex == itemIndex)
			{
				return itemIconComponent;
			}
		}
		return null;
	}

	private ItemIconComponent GetSelectedItemIconComponent()
	{
		return GetItemIconComponent(_selectItemIndex);
	}

	private void ReleaseSelectedItemIconComponent()
	{
		if (_selectItemIndex == 0)
		{
			return;
		}
		for (int i = 0; i < _itemIconList.Count; i++)
		{
			ItemIconComponent itemIconComponent = _itemIconList[i];
			if (itemIconComponent.ItemIndex == _selectItemIndex)
			{
				_itemIconList.RemoveAt(i);
				FactoryBase.ReleaseComponent(itemIconComponent);
				UIHelper.RepositionGrid(HeroExpPotionItemGrid);
				_selectItemIndex = 0;
				break;
			}
		}
	}

	private void SetupExpPotionItemList(int expPotionItemIndex)
	{
		List<ItemInfo> heroExpPotionItemList = GetHeroExpPotionItemList();
		if (heroExpPotionItemList.Count == 0)
		{
			_selectItemIndex = 0;
			FactoryBase.ReleaseAndClearList(_itemIconList);
		}
		else
		{
			GenerateItemList(heroExpPotionItemList);
			SelectAndMoveItem(expPotionItemIndex);
		}
	}

	private void GenerateItemList(List<ItemInfo> itemInfoList)
	{
		FactoryBase.ReleaseAndClearList(_itemIconList);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(itemInfoList); i++)
		{
			ItemInfo itemInfo = itemInfoList[i];
			ItemIconComponent itemIconComponent = CreateItemIcon(itemInfo, HeroExpPotionItemGrid);
			if (!(itemIconComponent == null))
			{
				_itemIconList.Add(itemIconComponent);
			}
		}
		UIHelper.RepositionGrid(HeroExpPotionItemGrid);
	}

	private void SelectAndMoveItem(int selectedItemIndex)
	{
		int num = -1;
		if (_itemIconList.Count == 0)
		{
			_selectItemIndex = 0;
			return;
		}
		for (int i = 0; i < _itemIconList.Count; i++)
		{
			ItemData itemData = _itemIconList[i].ItemData;
			if (selectedItemIndex == 0 || itemData.Index == selectedItemIndex)
			{
				_selectItemIndex = itemData.Index;
				num = i;
				break;
			}
		}
		if (num == -1)
		{
			num = 0;
			_selectItemIndex = _itemIconList[0].ItemIndex;
		}
		RefreshSelectionMark();
		HeroExpPotionItemGrid.ScrollView.MoveRelative(new Vector2(0f, (float)num * HeroExpPotionItemGrid.Grid.cellHeight));
		HeroExpPotionItemGrid.ScrollView.RestrictWithinBounds(instant: true);
	}

	private void SetupHeroList()
	{
		List<CachedHeroInfo> list = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.ToList();
		if (list.Count != 0)
		{
			list.Sort(new HeroComparer());
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(list); i++)
			{
				CachedHeroInfo heroInfo = list[i];
				HeroGetExpComponent item = CreateHeroGetExpComponent(heroInfo);
				_heroExpList.Add(item);
			}
			UIHelper.RepositionGrid(HeroListGrid);
		}
	}

	private HeroGetExpComponent CreateHeroGetExpComponent(CachedHeroInfo heroInfo)
	{
		HeroGetExpComponent heroGetExpComponent = UIHelper.AddGridItem(HeroListGrid, HeroGetExpComponent.PrefabAuto);
		heroGetExpComponent.Open(heroInfo, this, this);
		return heroGetExpComponent;
	}

	private void MoveHeroList(int heroIndex)
	{
		HeroGetExpComponent heroExpComponent = GetHeroExpComponent(heroIndex);
		HeroListGrid.ScrollTo(heroExpComponent, 0f, 0f);
	}

	private List<ItemInfo> GetHeroExpPotionItemList()
	{
		List<ItemInfo> itemsByType = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.GetItemsByType(ItemType.Potion);
		List<ItemInfo> list = new List<ItemInfo>();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(itemsByType); i++)
		{
			ItemData<PotionItemData> data = GameRoot.Instance.ItemDataContainer.GetData<PotionItemData>(itemsByType[i].ItemIndex);
			if (data != null && data.SubData.ActionType == ItemActionType.GetHeroExp && data.SubData.ActionValue > 0)
			{
				list.Add(itemsByType[i]);
			}
		}
		list.Sort(delegate(ItemInfo lhs, ItemInfo rhs)
		{
			ItemData<PotionItemData> data2 = GameRoot.Instance.ItemDataContainer.GetData<PotionItemData>(lhs.ItemIndex);
			ItemData<PotionItemData> data3 = GameRoot.Instance.ItemDataContainer.GetData<PotionItemData>(rhs.ItemIndex);
			return -data2.SubData.ActionValue.CompareTo(data3.SubData.ActionValue);
		});
		return list;
	}

	private ItemIconComponent CreateItemIcon(ItemInfo itemInfo, IGrid grid)
	{
		ItemData data = GameRoot.Instance.ItemDataContainer.GetData(itemInfo.ItemIndex);
		if (data == null)
		{
			return null;
		}
		ItemIconComponent itemIconComponent = UIHelper.AddGridItem(grid, ItemIconComponent.PrefabAuto);
		itemIconComponent.Open(data, this);
		int count = itemInfo.Count;
		Color? color = null;
		int num = count;
		itemIconComponent.SetItemCount(num, color);
		if (data.Type == ItemType.Potion)
		{
			ItemData<PotionItemData> data2 = GameRoot.Instance.ItemDataContainer.GetData<PotionItemData>(data.Index);
			string text = ((data2 != null) ? ("EXP+" + data2.SubData.ActionValue) : string.Empty);
			itemIconComponent.SetUpperLabel(text, Color.green);
		}
		return itemIconComponent;
	}

	private void SetupFlaskItemList(int selectFlaskItemIndex)
	{
		List<ItemInfo> emptyFlaskItemList = GetEmptyFlaskItemList();
		if (emptyFlaskItemList.Count == 0)
		{
			_selectItemIndex = 0;
			FactoryBase.ReleaseAndClearList(_itemIconList);
		}
		else
		{
			GenerateItemList(emptyFlaskItemList);
			SelectAndMoveItem(selectFlaskItemIndex);
		}
	}

	private List<ItemInfo> GetEmptyFlaskItemList()
	{
		List<ItemInfo> itemsByType = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.GetItemsByType(ItemType.Flask);
		itemsByType.Sort(delegate(ItemInfo lhs, ItemInfo rhs)
		{
			ItemData<FlaskItemData> data = GameRoot.Instance.ItemDataContainer.GetData<FlaskItemData>(lhs.ItemIndex);
			ItemData<FlaskItemData> data2 = GameRoot.Instance.ItemDataContainer.GetData<FlaskItemData>(rhs.ItemIndex);
			int num = data.SubData.ShowIndex.CompareTo(data2.SubData.ShowIndex);
			return (num == 0) ? (-data.SubData.FlaskExp.CompareTo(data2.SubData.FlaskExp)) : num;
		});
		return itemsByType;
	}

	void HeroGetExpComponent.Listener.ApplyUseExpPotion(CachedHeroInfo heroInfo, int PotionItemIndex, int UseItemCount)
	{
		NShared.UsePotionItem.Request request = new NShared.UsePotionItem.Request();
		request.ItemIndex = PotionItemIndex;
		request.HeroIndex = heroInfo.HeroIndex;
		request.ItemCount = UseItemCount;
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.UsePotionItem.Response response)
		{
			if (response.Result != NShared.UsePotionItem.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.ItemResult);
			GlobalAutoBehaviour<AccountManager>.instance.HeroManager.ApplyHeroExp(response.HeroExpResult);
			if (_iClassBuffListener != null)
			{
				_iClassBuffListener.AddPoint(response.HeroExpResult);
			}
		});
	}

	void HeroGetExpComponent.Listener.UsePotion(int count)
	{
		ExecSelectedItemInListDecCount(count);
	}

	void HeroGetExpComponent.Listener.EquipFlask(CachedHeroInfo heroInfo, int itemIndex)
	{
		ItemInfo itemInfo = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Get(itemIndex);
		if (itemInfo == null || itemInfo.Count == 0)
		{
			CenterMessage.Open(LocalizationManager.GetString(HeroFlask.NO_HAVE_USABLE_FLASK));
			return;
		}
		ItemData<FlaskItemData> data = GameRoot.Instance.ItemDataContainer.GetData<FlaskItemData>(itemInfo.ItemIndex);
		string text = data.Name;
		string message = LocalizationManager.GetStringDirect(data.SubData.UseDescLocal);
		MessagePopup.Open(text, message, delegate
		{
			NShared.UseFlaskItem.Request request = new NShared.UseFlaskItem.Request
			{
				ItemIndex = itemIndex,
				HeroIndex = heroInfo.HeroIndex
			};
			SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.UseFlaskItem.Response response)
			{
				if (response != null)
				{
					if (response.BaseResult != BaseResultType.Success)
					{
						MessagePopup.OpenError(response.BaseResult);
					}
					else
					{
						GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.ItemResult);
						GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Apply(response.FlaskResult);
						ItemData<FlaskItemData> data2 = GameRoot.Instance.ItemDataContainer.GetData<FlaskItemData>(itemInfo.ItemIndex);
						HeroFlask heroFlask = HeroFlask.FLASK_EQUIP_COMPLETE;
						if (data2 != null && data2.SubData != null)
						{
							switch (data2.SubData.Type)
							{
							case FlaskType.ValentineChocolate:
								heroFlask = HeroFlask.EVENT_CHOCOLATE_EQUIP_COMPLETE;
								break;
							case FlaskType.WhiteCandy:
								heroFlask = HeroFlask.EVENT_CHOCOLATE_EQUIP_COMPLETE;
								break;
							}
						}
						message = LocalizationManager.GetString(heroFlask, heroInfo.CreatureData.Name);
						CenterMessage.Open(message);
					}
				}
			});
		}, null);
	}

	void HeroGetExpComponent.Listener.UnEquipFlask(CachedHeroInfo heroInfo)
	{
		if (heroInfo == null)
		{
			return;
		}
		GameRoot.Instance.FlaskItemDataContainer.GetAllData();
		int flaskItemIndex = heroInfo.FlaskItemIndex;
		ItemData<FlaskItemData> flaskItemData = GameRoot.Instance.ItemDataContainer.GetData<FlaskItemData>(flaskItemIndex);
		if (flaskItemData == null || flaskItemData.SubData == null)
		{
			return;
		}
		if (flaskItemData.SubData.IsUnEquip)
		{
			string text = LocalizationManager.GetString(HeroFlask.CANCEL_FLASK_TITLE);
			string message = string.Empty;
			if (flaskItemData.SubData.SourceType == FlaskSourceType.Chicken)
			{
				message = LocalizationManager.GetString(HeroFlask.CANCEL_CURSED_RUBY_FLASK_MESSAGE);
			}
			else
			{
				message = LocalizationManager.GetString(HeroFlask.CANCEL_FLASK_MESSAGE);
			}
			MessagePopup.Open(text, message, delegate
			{
				NShared.CancelFlaskItem.Request request = new NShared.CancelFlaskItem.Request
				{
					HeroIndex = heroInfo.HeroIndex
				};
				SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.CancelFlaskItem.Response response)
				{
					if (response != null)
					{
						if (response.BaseResult != BaseResultType.Success)
						{
							MessagePopup.OpenError(response.BaseResult);
						}
						else if (response.Result != NShared.CancelFlaskItem.ResultType.Success)
						{
							if (response.Result == NShared.CancelFlaskItem.ResultType.NoFlaskItem)
							{
								if (flaskItemData != null)
								{
									int itemCount = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.GetItemCount(heroInfo.FlaskItemIndex);
									ItemResultInfo itemResultInfo = new ItemResultInfo
									{
										ItemIndex = heroInfo.FlaskItemIndex,
										AddCount = 1,
										NewCount = itemCount + 1
									};
									GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(itemResultInfo);
								}
								FlaskResultInfo info = new FlaskResultInfo
								{
									HeroIndex = heroInfo.FlaskItemIndex,
									ItemIndex = 0,
									AddExp = 0,
									NewExp = 0
								};
								GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Apply(info);
							}
							MessagePopup.OpenError(response.Result);
						}
						else
						{
							GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.ItemResult);
							GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Apply(response.FlaskResult);
							message = LocalizationManager.GetString(HeroFlask.FLASK_UNEQUIP_COMPLETE);
							CenterMessage.Open(message);
						}
					}
				});
			}, null);
		}
		else
		{
			string text2 = LocalizationManager.GetString(HeroFlask.CANCEL_FLASK_TITLE);
			string message2 = LocalizationManager.GetString(NShared.CancelFlaskItem.ResultType.IsNotUnequipType);
			MessagePopup.Open(text2, message2);
		}
	}

	private void ExecSelectedItemInListDecCount(int decreaseCount)
	{
		ItemIconComponent selectedItemIconComponent = GetSelectedItemIconComponent();
		if (!(selectedItemIconComponent == null))
		{
			long num = checked(selectedItemIconComponent.ItemCount - decreaseCount);
			int num2 = 0;
			if (num > num2)
			{
				selectedItemIconComponent.SetItemCount(num);
			}
			else if (num == 0L)
			{
				ReleaseSelectedItemIconComponent();
			}
		}
	}

	public ItemInfo GetSelectedItemInfo()
	{
		return GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Get(_selectItemIndex);
	}

	public int GetAddHeroExp()
	{
		ItemInfo selectedItemInfo = GetSelectedItemInfo();
		if (selectedItemInfo == null)
		{
			return 0;
		}
		ItemData<PotionItemData> data = GameRoot.Instance.ItemDataContainer.GetData<PotionItemData>(selectedItemInfo.ItemIndex);
		if (data == null || data.SubData.ActionType != ItemActionType.GetHeroExp)
		{
			return 0;
		}
		return data.SubData.ActionValue;
	}

	void ItemIconComponent.Listener.OnClick(ItemIconComponent itemIcon)
	{
		if (!(itemIcon == null))
		{
			_selectItemIndex = itemIcon.ItemIndex;
			RefreshSelectionMark();
		}
	}

	private void RefreshSelectionMark()
	{
		for (int i = 0; i < _itemIconList.Count; i++)
		{
			ItemIconComponent itemIconComponent = _itemIconList[i];
			itemIconComponent.SetSelected(_selectItemIndex == itemIconComponent.ItemIndex);
		}
	}

	public void OnToggleChanged()
	{
		HeroGetExpComponent.ExpStyle heroExpComponentExpStyle = GetHeroExpComponentExpStyle();
		if (heroExpComponentExpStyle != HeroGetExpComponent.ExpStyle.None)
		{
			ChangeStyle(heroExpComponentExpStyle);
		}
	}

	private HeroGetExpComponent.ExpStyle GetHeroExpComponentExpStyle()
	{
		HeroGetExpComponent.ExpStyle result = HeroGetExpComponent.ExpStyle.None;
		if (ExpPotionToggle.value)
		{
			result = HeroGetExpComponent.ExpStyle.HeroExp;
		}
		else if (EmptyFlaskToggle.value)
		{
			result = HeroGetExpComponent.ExpStyle.FlaskExp;
		}
		return result;
	}

	private void ChangeStyle(HeroGetExpComponent.ExpStyle style, int selectedItemIndex = 0)
	{
		if (CurrentStyle == style)
		{
			return;
		}
		foreach (HeroGetExpComponent heroExp in _heroExpList)
		{
			heroExp.SwitchExpMode(style);
		}
		switch (style)
		{
		case HeroGetExpComponent.ExpStyle.HeroExp:
		{
			if (!ExpPotionToggle.value)
			{
				ExpPotionToggle.value = true;
			}
			SetupExpPotionItemList(selectedItemIndex);
			string text2 = LocalizationManager.GetString(Item.EXPPOTION_TITLE_MESSAGE);
			TitleLabel.text = text2;
			break;
		}
		case HeroGetExpComponent.ExpStyle.FlaskExp:
		{
			if (!EmptyFlaskToggle.value)
			{
				EmptyFlaskToggle.value = true;
			}
			SetupFlaskItemList(selectedItemIndex);
			string text = LocalizationManager.GetString(HeroFlask.FLASK_TITLE_MESSAGE);
			TitleLabel.text = text;
			break;
		}
		}
		CurrentStyle = style;
	}

	private HeroGetExpComponent GetHeroExpComponent(int heroIndex)
	{
		foreach (HeroGetExpComponent heroExp in _heroExpList)
		{
			if (heroExp.HeroInfo.HeroIndex == heroIndex)
			{
				return heroExp;
			}
		}
		return null;
	}

	void IUserObserver<NGame2.NAccount.UserHeroManager.ChangeHeroInfo>.OnChanged(NGame2.NAccount.UserHeroManager.ChangeHeroInfo changedInfo)
	{
		if (changedInfo.ChangeType == NGame2.NAccount.UserHeroManager.HeroChangeType.FlaskItemIndex)
		{
			GetHeroExpComponent(changedInfo.HeroIndex).SetupFlaskInfo();
		}
	}

	void IUserObserver<ItemInfo>.OnChanged(ItemInfo itemInfo)
	{
		ItemData data = GameRoot.Instance.ItemDataContainer.GetData(itemInfo.ItemIndex);
		if (data != null && data.Type == ItemType.Flask)
		{
			SetupFlaskItemList(_selectItemIndex);
		}
	}

	bool IUserObserver.IsGlobal()
	{
		return false;
	}
}
