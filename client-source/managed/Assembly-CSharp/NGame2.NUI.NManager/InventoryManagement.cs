using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NGame2.NAccount;
using NGame2.NAccount.NExtension;
using NGame2.NContents;
using NGame2.NContentsDefine;
using NGame2.NDispatchBattle;
using NGame2.NDropBonusEvent;
using NGame2.NGuild;
using NGame2.NLobby;
using NGame2.NNotification;
using NGame2.NParty;
using NGame2.NScene;
using NGame2.NScene.NLobby;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NManager.NPet;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NBattle;
using NGame2.NUI.NWindow.NGlobalTopmost;
using NGame2.NUI.NWindow.NInventory;
using NGame2.NUI.NWindow.NPayShop;
using NGame2.NUtil;
using NGame2.NUtil.NCostume;
using NGame2.NUtil.NStringProxy;
using NShared;
using NShared.AddCraftSlot;
using NShared.AwakenTransition;
using NShared.BeginCampaign;
using NShared.BreakEquip;
using NShared.BreakItem;
using NShared.BreakRune;
using NShared.CancelCraftItem;
using NShared.CheckGuildName;
using NShared.CraftItem;
using NShared.ExecEquipGacha;
using NShared.ExtendChest;
using NShared.ExtendEquip;
using NShared.ExtendPunishmentRuneStorageSlot;
using NShared.InstantCraftItem;
using NShared.NLocalization;
using NShared.SellEquip;
using NShared.SellItem;
using NShared.SetChest;
using NShared.SetLockEquipItem;
using NShared.SetLockRuneItem;
using NShared.SoulWeaponAbilityTicket;
using NShared.SoulWeaponTransitionTicket;
using NShared.TakeCraftItem;
using NShared.UnsetChest;
using NShared.UseAccessorySelectItem;
using NShared.UseBoosterItem;
using NShared.UseCostumeSelectItem;
using NShared.UseEquipOptionSelectItem;
using NShared.UseGuildNameChangeItem;
using NShared.UseHeroGrowthItem;
using NShared.UseHeroSelectItem;
using NShared.UseMultipleHeroSelectItem;
using NShared.UsePackageItem;
using NShared.UsePackageSelectItem;
using NShared.UsePetSelectItem;
using NShared.UsePotionItem;
using NShared.UseWeaponUniqueSelectItem;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using NVespa.NTask;
using NVespa.NTask.NRuntime;
using UnityEngine;

namespace NGame2.NUI.NManager;

public class InventoryManagement : BaseManagement<InventoryManagement>, InvenList.Listener, InvenItemDesc.Listener, InvenRandomBoxDesc.Listener, InvenEquipInfo.Listener, HeroEquipInfo.Listener, CommonInvenUseHeroSelect.Listener, OptionSellConfirmPopup.Listener, OptionKeepConfirmPopup.Listener, WeaponUniqueSelect.Listener, EquipOptionSelect.Listener, EquipOptionSelectPopup.Listener, EquipOptionSelectConfirmPopup.Listener, InvenUseHeroCostumeSelect.Listener, BatchRuneBreakPopup.Listener, GuildNameChangeMessagePopup.Listener, CinemaItemScene.Listener, SkillBookHeroSelectPopup.Listener, ITouchDisabler, PayShopHeroInfoPopup.IListener, SelectionItemUsePopup.IListener, AccessoryHeroSelect.Listener, SoulStoneSelect.IListener, RuneSelectItemUsePopup.IListener, RuneSelectItemListWindow.IListener, RuneSelectItemFilterOptionList.IListener, EquipTransitionTicket.IListener, EquipTransitionTicketSelectConfirmPopup.IListener, InvenUsePetSelect.IListener, SoulWeaponAbilityTicket.IListener, SoulWeaponAbilityTicketConfirmPopup.IListener, InvenUseHeroGrowth.IListener, InvenUseMultipleHeroSelect.Listener, HeroInfoPopup.IListener
{
	bool ITouchDisabler.IsValid => true;

	protected override void Awake()
	{
		base.Awake();
		UseBackgroundBlack(use: true);
		AddInstance<InvenList>("Resources/Prefab2/Window/Inventory/InvenList");
		AddInstance<InvenItemDesc>("Resources/Prefab2/Window/Inventory/InvenItemDesc2");
		AddInstance<InvenMultipleItemDesc>("Resources/Prefab2/Window/Inventory/InvenMultipleItemDesc");
		AddInstance<HeroEquipBreakResultPopup>("Resources/Prefab2/Window/Hero/HeroEquipBreakResultPopup");
		AddInstance<InvenEquipInfo>("Resources/Prefab2/Window/Inventory/InvenEquipInfo");
		AddInstance<InvenEquipInfoUIContainer>("Resources/Prefab2/Window/Inventory/invenEquipInfouiContainer");
		AddInstance<InvenUseHeroSelect>("Prefab2/Window/Inventory/InvenUseHeroSelect");
		AddInstance<OptionSellConfirmPopup>("Resources/Prefab2/Window/MessagePopup/OptionSellConfirmPopup");
		AddInstance<OptionKeepConfirmPopup>("Resources/Prefab2/Window/MessagePopup/OptionKeepConfirmPopup");
		AddInstance<DismantleConfirmPopup>("Resources/Prefab2/Window/MessagePopup/DismantleConfirmPopup");
		AddInstance<BatchRuneBreakPopup>("Resources/Prefab2/Window/MessagePopup/BatchRuneBreakPopup");
		AddInstance<InvenRandomBoxDesc>("Resources/Prefab2/Window/Inventory/InvenRandomBoxDesc");
		AddInstance<WeaponUniqueSelect>("Prefab2/Window/Inventory/WeaponUniqueSelect");
		AddInstance<EquipOptionSelect>("Prefab2/Window/Inventory/EquipOptionSelect");
		AddInstance<EquipOptionSelectPopup>("Prefab2/Window/Inventory/EquipOptionSelectPopup");
		AddInstance<EquipOptionSelectConfirmPopup>("Prefab2/Window/Inventory/EquipOptionSelectConfirmPopup");
		AddInstance<InvenUseHeroCostumeSelect>("Prefab2/Window/Inventory/InvenUseHeroCostumeSelect");
		AddInstance<GuildNameChangeMessagePopup>("Prefab2/Window/MessagePopup/GuildNameChangeMessagePopup");
		AddInstance<CinemaItemScene>("Resources/Prefab2/Window/FullScene/CinemaItemScene");
		AddInstance<SkillBookHeroSelectPopup>("Resources/Prefab2/Window/MultipleDungeon/SkillBookHeroSelectPopup");
		AddInstance<PayShopHeroInfoPopup>("Resources/Prefab2/Window/PayShop/PayShopHeroInfoPopup");
		AddInstance<AccessoryHeroSelect>("Resources/Prefab2/Window/Inventory/AccessoryHeroSelect");
		AddInstance<SoulStoneSelect>("Prefab2/Window/Inventory/SoulStoneSelect");
		AddInstance<RuneSelectItemUsePopup>("Resources/Prefab2/Window/Inventory/RuneSelectItemUsePopup");
		AddInstance<RuneSelectItemListWindow>("Resources/Prefab2/Window/Inventory/RuneSelectItemListWindow");
		AddInstance<RuneSelectItemFilterOptionList>("Resources/Prefab2/Window/Inventory/RuneSelectItemFilterOptionList");
		AddInstance<InvenUseAttrTypeHeroSelect>("Prefab2/Window/Inventory/InvenUseAttrTypeHeroSelect");
		AddInstance<EquipTransitionTicket>("Resources/Prefab2/Window/Inventory/EquipTransitionTicket");
		AddInstance<EquipTransitionTicketSelectConfirmPopup>("Resources/Prefab2/Window/Inventory/EquipTransitionTicketSelectConfirmPopup");
		AddInstance<InvenUsePetSelect>("Prefab2/Window/Inventory/InvenUsePetSelect");
		AddInstance<SoulWeaponAbilityTicket>("Resources/Prefab2/Window/Inventory/SoulWeaponAbilityTicket");
		AddInstance<SoulWeaponAbilityTicketConfirmPopup>("Resources/Prefab2/Window/Inventory/SoulWeaponAbilityTicketConfirmPopup");
		AddInstance<InvenUseHeroGrowth>("Prefab2/Window/Inventory/InvenUseHeroGrowth");
		AddInstance<InvenUseMultipleHeroSelect>("Prefab2/Window/Inventory/MultipleHeroSelect/InvenUseMultipleHeroSelect");
		AddInstance<HeroInfoPopup>("Resources/Prefab2/Window/Inventory/MultipleHeroSelect/HeroInfoPopup");
		AddInstance<HeroSelectGuidePopup>("Prefab2/Window/Inventory/MultipleHeroSelect/HeroSelectGuidePopup");
	}

	public void OpenInventory(InvenList.EToggle toggle = InvenList.EToggle.Equip, string craftItemCode = null)
	{
		AddTask(CreateOpenInventoryTask(toggle, craftItemCode));
	}

	public void OpenInventory(InvenList.EToggle toggle, Action onClosing)
	{
		AddTask(base.TaskFactory.Open(delegate(InvenList window)
		{
			window.Init(this, toggle);
			window.SetOnClosing(onClosing);
		}));
	}

	public ITask CreateOpenInventoryTask(InvenList.EToggle toggle = InvenList.EToggle.Equip, string craftItemCode = null)
	{
		return base.TaskFactory.Open(delegate(InvenList window)
		{
			window.Init(this, toggle);
		});
	}

	private void MoveToHeroInventory(ItemType itemType, EquipPartType partType = EquipPartType.None)
	{
		AddTask(base.TaskFactory.Run(delegate(InvenList window)
		{
			window.Refresh(immediate: false);
		}), base.TaskFactory.Run(delegate
		{
			SceneAutoBehaviour<MyHeroManagement>.instance.OpenInventory(itemType, partType);
		}));
	}

	private void MoveToHeroInventory(EquipItemInfo equipItemInfo)
	{
		if (equipItemInfo != null)
		{
			AddTask(base.TaskFactory.Run(delegate(InvenList window)
			{
				window.Refresh(immediate: false);
			}), base.TaskFactory.Run(delegate
			{
				SceneAutoBehaviour<MyHeroManagement>.instance.OpenInventory(equipItemInfo);
			}));
		}
	}

	public void OpenHeroExpPotionWindow(HeroGetExpComponent.ExpStyle style, int selectItemIndex = 0, int selectHeroIndex = 0)
	{
		InvenUseHeroExpPotion.Open(style, selectItemIndex, selectHeroIndex);
	}

	private void OnUsePotionItem(ItemData itemData, InvenItemDesc caller)
	{
		if (itemData == null)
		{
			return;
		}
		ItemData<PotionItemData> data = GameRoot.Instance.ItemDataContainer.GetData<PotionItemData>(itemData.Code);
		if (data == null)
		{
			return;
		}
		if (data.SubData.ActionType == ItemActionType.RecoverStamina || data.SubData.ActionType == ItemActionType.RecoverStaminaPercent)
		{
			if (!EnumTools.TryParse<StaminaType>(data.SubData.ActionSubValue, out var o_value))
			{
				o_value = StaminaType.Chicken;
			}
			UIHelper.ResolveRecoverStamina(isNotEnough: false, new int[1] { itemData.Index }, o_value);
		}
		else if (data.SubData.ActionType == ItemActionType.GetHeroExp)
		{
			InvenUseHeroExpPotion.Open(HeroGetExpComponent.ExpStyle.HeroExp, itemData.Index);
		}
		else if (data.SubData.ActionType == ItemActionType.RecoverGem)
		{
			UIHelper.ResoveRecoverGem(new int[1] { itemData.Index });
		}
		else if (data.SubData.ActionType == ItemActionType.RecoverHideoutKey)
		{
			if (!EnumTools.TryParse<StaminaType>(data.SubData.ActionSubValue, out var o_value2))
			{
				o_value2 = StaminaType.HideoutKey;
			}
			UIHelper.ResolveRecoverStamina(isNotEnough: false, new int[1] { itemData.Index }, o_value2);
		}
		else if (data.SubData.ActionType == ItemActionType.RecoverConquestKey)
		{
			if (!EnumTools.TryParse<StaminaType>(data.SubData.ActionSubValue, out var o_value3))
			{
				o_value3 = StaminaType.ConquestKey;
			}
			UIHelper.ResolveRecoverStamina(isNotEnough: false, new int[1] { itemData.Index }, o_value3);
		}
		else if (data.SubData.ActionType == ItemActionType.ExtendEquip)
		{
			OnUseExtendEquip(data);
			caller.Close();
		}
		else if (data.SubData.ActionType == ItemActionType.ExtendChest)
		{
			OnUseExtendChest(data);
			caller.Close();
		}
	}

	private void OnUseExtendEquip(ItemData<PotionItemData> potionData)
	{
		if (SprkInventoryTickets.TryUse(potionData))
		{
			return;
		}
		ItemData<PotionItemData> potionData2 = potionData;
		int maxEquipItemCount = GlobalAutoBehaviour<AccountManager>.instance.GetMaxEquipItemCount();
		int inventoryExtendSize = Helper.GetInventoryExtendSize(GameRoot.Instance.InventoryExtendDataContainer.GetExtendMaxCount());
		int inventoryExtendSize2 = Helper.GetInventoryExtendSize(potionData2.SubData.ActionValue, addBaseCount: false);
		if (maxEquipItemCount >= inventoryExtendSize)
		{
			string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
			string message = LocalizationManager.GetString(ItemUse.EQUIP_EXTEND_IS_FULL);
			MessagePopup.Open(title, message);
		}
		else if (maxEquipItemCount + inventoryExtendSize2 > inventoryExtendSize)
		{
			int num = inventoryExtendSize - maxEquipItemCount;
			string title2 = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
			string message2 = LocalizationManager.GetString(ItemUse.EQUIP_EXTEND_DESC1, num, potionData2.Name);
			string message3 = LocalizationManager.GetString(ItemUse.EQUIP_EXTEND_DESC2);
			IconMessagePopup iconMessagePopup = IconMessagePopup.Open(title2, message2, message3, delegate
			{
				SendExtendEquip(potionData2);
			});
			if (iconMessagePopup != null)
			{
				iconMessagePopup.AddItem(potionData2.Index, 1);
				iconMessagePopup.SetOKButtonLabel(LocalizationManager.GetString(NShared.NLocalization.Common.USE));
			}
		}
		else
		{
			SendExtendEquip(potionData2);
		}
	}

	private void OnUseExtendChest(ItemData<PotionItemData> potionData)
	{
		int maxChestItemCount = GlobalAutoBehaviour<AccountManager>.instance.GetMaxChestItemCount();
		int chestExtendSize = Helper.GetChestExtendSize(GameRoot.Instance.InventoryExtendDataContainer.GetExtendMaxCount(InventoryType.Chest));
		int chestExtendSize2 = Helper.GetChestExtendSize(potionData.SubData.ActionValue, addBaseCount: false);
		if (maxChestItemCount >= chestExtendSize)
		{
			string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
			string message = LocalizationManager.GetString(ItemUse.CHEST_EXTEND_IS_FULL);
			MessagePopup.Open(title, message);
		}
		else if (maxChestItemCount + chestExtendSize2 > chestExtendSize)
		{
			int num = chestExtendSize - maxChestItemCount;
			string title2 = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
			string message2 = LocalizationManager.GetString(ItemUse.CHEST_EXTEND_DESC1, num, potionData.Name);
			string message3 = LocalizationManager.GetString(ItemUse.CHEST_EXTEND_DESC2);
			IconMessagePopup iconMessagePopup = IconMessagePopup.Open(title2, message2, message3, delegate
			{
				SendExtendChest(potionData);
			});
			if (iconMessagePopup != null)
			{
				iconMessagePopup.AddItem(potionData.Index, 1);
				iconMessagePopup.SetOKButtonLabel(LocalizationManager.GetString(NShared.NLocalization.Common.USE));
			}
		}
		else
		{
			SendExtendChest(potionData);
		}
	}

	public static void SendExtendEquip(ItemData itemData, Action callback = null)
	{
		NShared.UsePotionItem.Request request = new NShared.UsePotionItem.Request();
		request.ItemIndex = itemData.Index;
		request.ItemCount = 1;
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.UsePotionItem.Response response)
		{
			if (response.Result != NShared.UsePotionItem.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
				if (callback != null)
				{
					callback();
				}
			}
			else
			{
				int maxEquipItemCount = GlobalAutoBehaviour<AccountManager>.instance.GetMaxEquipItemCount();
				GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.ItemResult);
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.SetInventoryExtend(response.NewInventoryExtend);
				PopupExtendEquip(GlobalAutoBehaviour<AccountManager>.instance.GetMaxEquipItemCount() - maxEquipItemCount);
				if (callback != null)
				{
					callback();
				}
			}
		});
	}

	public static void SendExtendChest(ItemData itemData, Action callback = null)
	{
		NShared.UsePotionItem.Request request = new NShared.UsePotionItem.Request();
		request.ItemIndex = itemData.Index;
		request.ItemCount = 1;
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.UsePotionItem.Response response)
		{
			if (response.Result != NShared.UsePotionItem.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
				if (callback != null)
				{
					callback();
				}
			}
			else
			{
				int maxChestItemCount = GlobalAutoBehaviour<AccountManager>.instance.GetMaxChestItemCount();
				GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.ItemResult);
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.SetChestExtend(response.NewChestExtend);
				PopupExtendChest(GlobalAutoBehaviour<AccountManager>.instance.GetMaxChestItemCount() - maxChestItemCount);
				if (callback != null)
				{
					callback();
				}
			}
		});
	}

	public static void SendExtendPunishmentRune(ItemData itemData, Action callback = null)
	{
		new NShared.UsePotionItem.Request
		{
			ItemIndex = itemData.Index,
			ItemCount = 1
		};
	}

	public void OpenMaxExtendMessagePopup(string message)
	{
		MessagePopup.Open(LocalizationManager.GetString(NShared.NLocalization.Common.INFORM), message);
	}

	public void OpenQuestionExtendMessagePopup(string message1, string message2, int itemIndex, Action action)
	{
		IconMessagePopup iconMessagePopup = IconMessagePopup.Open(LocalizationManager.GetString(NShared.NLocalization.Common.INFORM), message1, message2, delegate
		{
			Vespa_System_Action_Extension_Method.fire(action);
		});
		if (iconMessagePopup != null)
		{
			iconMessagePopup.AddItem(itemIndex, 1);
			iconMessagePopup.SetOKButtonLabel(LocalizationManager.GetString(NShared.NLocalization.Common.USE));
		}
	}

	public static void OpenQuestionSellMessagePopup(long sellGold, Action action)
	{
		string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
		string message = LocalizationManager.GetString(Inventory.INVENTORY_ASK_SELL_MESSAGE);
		string action2 = LocalizationManager.GetString(Inventory.INVENTORY_ITEM_ACTION_SELL);
		PricedMessagePopup.Open(title, message, action2, PricedMessagePopup.EPriceType.Gold, sellGold, delegate
		{
			Vespa_System_Action_Extension_Method.fire(action);
		});
	}

	public void OpenQuestionBreakMessagePopup(Action action)
	{
		string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
		string message = LocalizationManager.GetString(Inventory.INVENTORY_ASK_BREAK_MESSAGE);
		MessagePopup.Open(title, message, delegate
		{
			Vespa_System_Action_Extension_Method.fire(action);
		});
	}

	public static void NotEnoughGemPopup()
	{
		string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
		string message = LocalizationManager.GetString(NShared.NLocalization.System.MOVE_GEM_SHOP_CONFIRM);
		MessagePopup.Open(title, message, delegate
		{
			SceneAutoBehaviour<PayShopManagement>.instance.OpenNewPayShop(NewPayShopCategoryType.Ruby.ToString());
		}, null);
	}

	private void OnUseFlaskItem(ItemData itemData)
	{
		InvenUseHeroExpPotion.Open(HeroGetExpComponent.ExpStyle.FlaskExp, itemData.Index);
	}

	private void OnUseWeaponUniqueSelectItem(ItemData itemData)
	{
		if (itemData != null)
		{
			GetInstance<WeaponUniqueSelect>().Open(itemData, this);
		}
	}

	private void OnUseSoulWeaponSelectItem(ItemData itemData)
	{
		if (itemData != null)
		{
			GetInstance<SoulStoneSelect>().Open(itemData, this);
		}
	}

	private void OnUseOptionSelectItem(ItemData itemData)
	{
		if (itemData != null)
		{
			GetInstance<EquipOptionSelect>().Open(itemData, this);
		}
	}

	private void ProcessItemUseEffect(Action onEnd)
	{
		bool effect_ended = false;
		new GroupTask().ADDTASK(base.TaskFactory.Run(delegate(InvenItemDesc window)
		{
			window.ExecUseItemEffect(delegate
			{
				effect_ended = true;
			});
		})).ADDTASK(new WaitTask(() => !effect_ended)).ADDTASK(base.TaskFactory.Run(delegate
		{
			Vespa_System_Action_Extension_Method.fire(onEnd);
		}))
			.WrapperTouchControl()
			.DisableTouch()
			.Commit();
	}

	private void OnUseBoosterItem(ItemData itemData)
	{
		string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
		string content = LocalizationManager.GetString(ItemUse.INVEN_USE_BOOSTER, itemData.Name);
		ItemInfo itemInfo = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Get(itemData.Index);
		if (itemInfo == null)
		{
			return;
		}
		List<Tuple<ItemData, int>> list = new List<Tuple<ItemData, int>>();
		list.Add(new Tuple<ItemData, int>(itemData, itemInfo.Count));
		SelectItemMessagePopup.Open(title, content, list, delegate(SelectItemMessagePopup selectItemPopup, int idx, int count)
		{
			SelectBoosterItem(itemData, count, delegate
			{
				selectItemPopup.Close();
			});
		}, null, closeOnYes: false);
	}

	private void UseBoosterItem(ItemData itemData, int itemCount)
	{
		if (itemData == null)
		{
			return;
		}
		NShared.UseBoosterItem.Request request = new NShared.UseBoosterItem.Request
		{
			ItemIndex = itemData.Index,
			ItemCount = itemCount
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.UseBoosterItem.Response response)
		{
			if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else if (response.Result != NShared.UseBoosterItem.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.ItemTimeDurationManager.Apply(response.ItemTimeDurationInfo);
				GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.ItemResult);
				PopupUseBoosterMsg(response.ItemTimeDurationInfo);
			}
		});
	}

	private void PopupUseBoosterMsg(PlayerItemTimeDurationInfo info)
	{
		if (info != null)
		{
			ItemData<BoosterItemData> data = GameRoot.Instance.ItemDataContainer.GetData<BoosterItemData>(info.ItemIndex);
			if (data != null)
			{
				TimeSpan remainTime = info.GetRemainTime(data);
				CenterMessage.Open(LocalizationManager.GetString(Item.USE_BOOSTER_MSG, ItemNameBuilder.GetItemName(data), (int)remainTime.TotalDays, remainTime.Hours, remainTime.Minutes));
			}
		}
	}

	private void OnUseHeroSelectItem(ItemData itemData)
	{
		if (itemData == null)
		{
			return;
		}
		HeroSelectItemData data = GameRoot.Instance.HeroSelectItemDataContainer.GetData(itemData.Code);
		if (data != null)
		{
			if (data.HeroSelectItemType == HeroSelectItemType.Attr)
			{
				OpenPopupInvenUseAttrTypeHeroSelect(itemData, null);
			}
			else
			{
				OpenPopupInvenUseHeroSelect(itemData, null);
			}
		}
	}

	private void OpenPopupInvenUseAttrTypeHeroSelect(ItemData itemData, CreatureData selectHeroData)
	{
		if (itemData != null)
		{
			InvenUseAttrTypeHeroSelect invenUseAttrTypeHeroSelect = GetInstance<InvenUseAttrTypeHeroSelect>();
			if (!(invenUseAttrTypeHeroSelect == null))
			{
				invenUseAttrTypeHeroSelect.Open(this, itemData.Index, selectHeroData);
			}
		}
	}

	private void OpenPopupInvenUseHeroSelect(ItemData itemData, CreatureData selectHeroData)
	{
		if (itemData != null)
		{
			AddTask(base.TaskFactory.Open(delegate(InvenUseHeroSelect window)
			{
				window.Init(this, itemData.Index, selectHeroData);
			}));
		}
	}

	private void OnUseMultipleHeroSelectItem(ItemData itemData)
	{
		if (itemData != null && GameRoot.Instance.MultipleHeroSelectItemDataContainer.GetData(itemData.Code) != null)
		{
			OpenPopupInvenUseMultipleHeroSelect(itemData, null, null);
		}
	}

	private void OpenPopupInvenUseMultipleHeroSelect(ItemData itemData, CreatureData selectHeroData1, CreatureData selectHeroData2, CreatureData lastSelectedHero = null, int lastSelectedSlot = 0)
	{
		if (itemData == null)
		{
			return;
		}
		Action inItAction = delegate
		{
			AddTask(base.TaskFactory.Open(delegate(InvenUseMultipleHeroSelect window)
			{
				window.Init(this, itemData.Index, selectHeroData1, selectHeroData2, lastSelectedHero, lastSelectedSlot);
			}));
		};
		VespaCoroutine.Start(CoOpenMultipleHeroSelect(inItAction));
	}

	private IEnumerator CoOpenMultipleHeroSelect(Action inItAction = null)
	{
		SceneChanger sceneChanger = GlobalTopmostWindow.instance.CreateSceneChanger(7f, GlobalTopmostWindow.ESceneChangeType.BlackScreen);
		while (!sceneChanger.IsCompleteAnimation())
		{
			yield return null;
		}
		Vespa_System_Action_Extension_Method.fire(inItAction);
		InvenUseMultipleHeroSelect instance = GetInvenUseMultipleSelectHero();
		float elapsedTime = 0f;
		while (!instance.IsOpen() && elapsedTime < 7f)
		{
			elapsedTime += Time.deltaTime;
			yield return null;
		}
		yield return null;
		yield return null;
		yield return null;
		yield return null;
		yield return null;
		sceneChanger.Close();
	}

	private void OnUseFreeEquipGacha(ItemData itemData)
	{
		if (itemData == null)
		{
			return;
		}
		GachaSelectItemData data = GameRoot.Instance.GachaSelectItemDataContainer.GetData(itemData.Index);
		if (data == null)
		{
			return;
		}
		if (data.GachaType != EquipGachaType.Pet && GlobalAutoBehaviour<AccountManager>.instance.IsMaxEquipItemCount())
		{
			string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
			string message = LocalizationManager.GetString(NShared.BeginCampaign.ResultType.EquipItemFull);
			MessagePopup.Open(title, message);
		}
		else if (__Extension_Method_ICollection__.COUNT(data.GachaIndices) > 1)
		{
			if (!UIHelper.IsEnableGachas(data.GachaIndices))
			{
				if (data.GachaType == EquipGachaType.StepUp)
				{
					string stringDirect = LocalizationManager.GetStringDirect("COMMON_NOTIFICATION");
					string stringDirect2 = LocalizationManager.GetStringDirect("STEPUP_GACHA_CANNOT_USE");
					MessagePopup.Open(stringDirect, stringDirect2);
				}
				else
				{
					MessagePopup.OpenError(NShared.ExecEquipGacha.ResultType.ClosedEquipGacha);
				}
			}
			else
			{
				if (IsOpenedAny())
				{
					CloseAllActiveWindow();
				}
				NewPayShopCategoryType newPayShopCategoryType = ((data.GachaCategoryType != NewPayShopCategoryType.None) ? data.GachaCategoryType : NewPayShopCategoryType.SpecialGacha);
				SceneAutoBehaviour<PayShopManagement>.instance.OpenNewPayShop(newPayShopCategoryType.ToString());
			}
		}
		else if (__Extension_Method_ICollection__.COUNT(data.GachaIndices) == 1)
		{
			int gachaIndex = data.GachaIndices[0];
			if (!UIHelper.IsEnableGachaTicket(itemData.Index))
			{
				MessagePopup.OpenError(NShared.ExecEquipGacha.ResultType.ClosedEquipGacha);
			}
			SceneAutoBehaviour<PayShopManagement>.instance.OpenEquipGacha(gachaIndex);
		}
	}

	private void OnUseSelectCostumeItem(ItemData itemData)
	{
		if (itemData != null)
		{
			InvenUseHeroCostumeSelect invenUseHeroCostumeSelect = GetInstance<InvenUseHeroCostumeSelect>();
			if (invenUseHeroCostumeSelect != null)
			{
				invenUseHeroCostumeSelect.Open(this, itemData.Index);
			}
		}
	}

	private void OnUseEquipOptionRenewCoupon(ItemData itemData)
	{
		SceneAutoBehaviour<BlacksmithManagement>.instance.OpenEquipOptionRenew();
	}

	private void OnUseExtraSkillRenewCoupon(ItemData itemData)
	{
		SceneAutoBehaviour<ValanceManagement>.instance.OpenRenewInven(null);
	}

	public void OnUseNickChangeItem(ItemData itemData)
	{
		if (itemData != null)
		{
			if (GlobalAutoBehaviour<PartyManager>.instance.IsValidParty())
			{
				MessagePopup.OpenError(LocalizationManager.GetString(Nick.NICK_CANNOT_CHANGE_RAID));
			}
			else
			{
				NickChangeMessagePopup.Open(itemData);
			}
		}
	}

	private void OnUseGuildNameChangeItem(ItemData itemData)
	{
		if (itemData == null)
		{
			return;
		}
		if (GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GetGuildId() == 0L)
		{
			MessagePopup.OpenError(LocalizationManager.GetString(Guild.GUILD_MEMBER_INVALID));
			return;
		}
		GuildMemberInfo myMemberInfo = GlobalAutoBehaviour<GuildManager>.instance.MemberManager.GetMyMemberInfo();
		if (myMemberInfo == null || myMemberInfo.Rank > 1)
		{
			MessagePopup.OpenError(LocalizationManager.GetString(Guild.GUILD_NAME_CHANGE_MASTER));
		}
		else
		{
			GetInstance<GuildNameChangeMessagePopup>().Open(this, itemData);
		}
	}

	private void OnUsePetSelectItem(ItemData itemData)
	{
		if (itemData != null && GameRoot.Instance.PetSelectItemDataContainer.GetData(itemData.Code) != null)
		{
			OpenPopupInvenUsePetSelect(itemData, null);
		}
	}

	private void OpenPopupInvenUsePetSelect(ItemData itemData, CreatureData selectPetData)
	{
		GetInstance<InvenUsePetSelect>().Open(this, itemData);
	}

	private void OnUseHeroGrowthItem(ItemData itemData)
	{
		if (itemData != null && GameRoot.Instance.HeroGrowthItemDataContainer.GetData(itemData.Code) != null)
		{
			GetInstance<InvenUseHeroGrowth>().Open(this, itemData);
		}
	}

	public static void BreakEquipItems<T, K>(T taskOwner, IEnumerable<EquipItemInfo> equipItemInfos, Action<bool> onBreak = null, Action<K> OnComplete = null) where T : BaseManagement<T> where K : BaseWindow
	{
		IEnumerable<EquipItemInfo> equipItemInfos2 = equipItemInfos;
		if (equipItemInfos2 == null || equipItemInfos2.Count() == 0)
		{
			return;
		}
		string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
		string message = SprkBatchGrind.Summary(LocalizationManager.GetString(Inventory.INVENTORY_ASK_BREAK_MESSAGE), equipItemInfos);
		MessagePopup.Open(title, message, delegate
		{
			taskOwner.AddTask(taskOwner.TaskFactory.Run(delegate(InvenList window)
			{
				window.CloseSubMenu();
			}), new WebServiceTask<NShared.BreakEquip.Request, NShared.BreakEquip.Response>(new NShared.BreakEquip.Request
			{
				EquipItemSlotIndices = equipItemInfos2.Select((EquipItemInfo e) => e.SlotIndex).ToArray()
			}, delegate(NShared.BreakEquip.Request rewquest, NShared.BreakEquip.Response response)
			{
				if (response.Result != NShared.BreakEquip.ResultType.Success)
				{
					Vespa_System_Action_Extension_Method.fire(onBreak, i_argA: false);
					return taskOwner.TaskFactory.Run(delegate
					{
						MessagePopup.OpenError(response.Result);
					});
				}
				Vespa_System_Action_Extension_Method.fire(onBreak, i_argA: true);
				return new GroupTask().ADDTASK(taskOwner.TaskFactory.Run(delegate
				{
					foreach (EquipItemInfo item in equipItemInfos2)
					{
						GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Apply(item.SlotIndex, null);
					}
					GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.ItemResults);
				})).ADDTASK(OpenHeroEquipBreakResultPopup(taskOwner, response.ItemResults)).ADDTASK(taskOwner.TaskFactory.Run(delegate(K window)
				{
					OnComplete(window);
				}));
			}));
		}, null);
	}

	void InvenList.Listener.Close()
	{
		CloseAllActiveWindow();
	}

	void InvenList.Listener.ReOpen()
	{
	}

	void InvenList.Listener.OnClickSellEquip(IEnumerable<EquipItemInfo> equipItemInfos)
	{
		ProcessSellEquip(equipItemInfos, delegate(bool success)
		{
			if (success)
			{
				GetInstance<InvenList>().CloseSubMenu();
				GetInstance<InvenList>().InitEquipmentFilter();
			}
		});
	}

	void InvenList.Listener.OnClickSellItem(IEnumerable<int> itemIndices, IEnumerable<int> itemCounts)
	{
		if (itemIndices == null || itemCounts == null || itemIndices.Count() == 0 || itemCounts.Count() == 0)
		{
			return;
		}
		List<int> list = itemIndices.ToList();
		List<int> list2 = itemCounts.ToList();
		long num = 0L;
		string ItemName = string.Empty;
		for (int i = 0; i < list.Count; i++)
		{
			int index = list[i];
			int num2 = list2[i];
			ItemData data = GameRoot.Instance.ItemDataContainer.GetData(index);
			if (data != null)
			{
				num += (long)GlobalAutoBehaviour<DropBonusEventManager>.instance.GetCheckEquipSellEventBonus(data) * (long)num2;
			}
		}
		if (list.Count() == 1)
		{
			ItemData data2 = GameRoot.Instance.ItemDataContainer.GetData(list[0]);
			if (data2 != null)
			{
				ItemName = ItemNameBuilder.GetItemName(data2);
			}
		}
		string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
		string message = LocalizationManager.GetString(Inventory.INVENTORY_ASK_SELL_MESSAGE);
		string action = LocalizationManager.GetString(Inventory.INVENTORY_ITEM_ACTION_SELL);
		PricedMessagePopup.Open(title, message, action, PricedMessagePopup.EPriceType.Gold, num, delegate
		{
			AddTask(base.TaskFactory.Run(delegate(InvenList window)
			{
				window.CloseSubMenu();
			}), new WebServiceTask<NShared.SellItem.Request, NShared.SellItem.Response>(new NShared.SellItem.Request
			{
				ItemIndices = itemIndices.ToArray(),
				ItemCount = itemCounts.ToArray()
			}, (NShared.SellItem.Request request, NShared.SellItem.Response response) => (response.Result != NShared.SellItem.ResultType.Success) ? base.TaskFactory.Run(delegate
			{
				MessagePopup.OpenError(response.Result);
			}) : new GroupTask().ADDTASK(base.TaskFactory.Run(delegate
			{
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.CurrencyResult);
				GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.ItemResults);
			})).ADDTASK(base.TaskFactory.Run(delegate
			{
				string text = string.Empty;
				if (response.CurrencyResult != null && response.CurrencyResult.CurrencyType == CurrencyType.Gold)
				{
					text = Vespa_Long_Extension_Method.ToCommaString(response.CurrencyResult.AddValue);
				}
				if (String_Extension_Method.IsNullOrEmpty(ItemName))
				{
					CenterMessage.Open(LocalizationManager.GetString(Inventory.ITEM_SOLD_MSG, text));
				}
				else
				{
					CenterMessage.Open(LocalizationManager.GetString(Inventory.NOTIFY_ITEM_SELL, ItemName, text));
				}
			}))));
		});
	}

	void InvenList.Listener.OnClickBreakEquip(IEnumerable<EquipItemInfo> equipItemInfos)
	{
		BreakEquipItems(this, equipItemInfos, null, delegate(InvenList window)
		{
			window.InitEquipmentFilter();
		});
	}

	public static ITask OpenHeroEquipBreakResultPopup<T>(T taskOwner, ItemResultInfo[] itemResults) where T : BaseManagement<T>
	{
		new List<int>();
		new List<int>();
		Dictionary<int, int> itemCountDictionary = new Dictionary<int, int>();
		foreach (ItemResultInfo itemResultInfo in itemResults)
		{
			if (itemCountDictionary.ContainsKey(itemResultInfo.ItemIndex))
			{
				itemCountDictionary[itemResultInfo.ItemIndex] += itemResultInfo.AddCount;
			}
			else
			{
				itemCountDictionary[itemResultInfo.ItemIndex] = itemResultInfo.AddCount;
			}
		}
		return taskOwner.TaskFactory.Open(delegate(HeroEquipBreakResultPopup window)
		{
			window.Init(itemCountDictionary);
		});
	}

	void InvenList.Listener.OnClickItemIcon(ItemIconComponent component)
	{
		if (component == null || component.ItemData == null)
		{
			return;
		}
		if (GlobalAutoBehaviour<AccountManager>.instance.ItemManager.IsNew(component.ItemIndex))
		{
			component.SetNew(isNew: false);
			GlobalAutoBehaviour<AccountManager>.instance.ItemManager.RemoveAddedItemIndex(component.ItemIndex);
			InvenList invenList = GetInstance<InvenList>();
			if (invenList != null)
			{
				invenList.UpdateTabNewState();
			}
		}
		if (component.ItemData.Type == ItemType.Equip)
		{
			if (GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.IsNew(component.EquipItemInfo.Uid))
			{
				component.SetNew(isNew: false);
				GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.NewIconRemove(component.EquipItemInfo);
				InvenList invenList2 = GetInstance<InvenList>();
				if (invenList2 != null)
				{
					invenList2.UpdateTabNewState();
				}
			}
			EquipItemInfo equipItemInfo = component.EquipItemInfo;
			GetInstance<InvenEquipInfoUIContainer>().OpenInventoryMode(equipItemInfo, this);
		}
		else if (component.ItemData.Type == ItemType.Package || component.ItemData.Type == ItemType.CinemaPackage || component.ItemData.Type == ItemType.SoulStonePackage)
		{
			GetInstance<InvenRandomBoxDesc>().Open(component.ItemData, component.ItemCount, this);
		}
		else if (component.ItemData.Type == ItemType.PunishmentRune)
		{
			if (GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.IsNew(component.EquipItemInfo.Uid))
			{
				component.SetNew(isNew: false);
				GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.NewIconRemove(component.EquipItemInfo);
				InvenList invenList3 = GetInstance<InvenList>();
				if (invenList3 != null)
				{
					invenList3.UpdateTabNewState();
				}
			}
			base.TaskFactory.Open(delegate(InvenItemDesc window)
			{
				window.Init(component.EquipItemInfo, this);
			}).Commit();
		}
		else
		{
			base.TaskFactory.Open(delegate(InvenItemDesc window)
			{
				window.Init(component.ItemData, component.ItemCount, this);
			}).Commit();
		}
	}

	void InvenList.Listener.OnClickExtend(InvenList caller)
	{
		SendExtendEquip();
	}

	private void SendExtendEquip()
	{
		int inventoryExtend = GlobalAutoBehaviour<AccountManager>.instance.UserManager.MiscInfo.InventoryExtend;
		GameRoot.Instance.InventoryExtendDataContainer.GetNextDataByExtendCount(inventoryExtend);
		ExtendInvenMessagePopup.Open(delegate(int size, int price)
		{
			AddTask(new WebServiceTask<NShared.ExtendEquip.Request, NShared.ExtendEquip.Response>(new NShared.ExtendEquip.Request
			{
				ReqGem = price
			}, delegate(NShared.ExtendEquip.Request request, NShared.ExtendEquip.Response response)
			{
				if (response.Result == NShared.ExtendEquip.ResultType.NotEnoughGem)
				{
					string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
					string message = LocalizationManager.GetString(NShared.NLocalization.System.MOVE_GEM_SHOP_CONFIRM);
					return base.TaskFactory.Run(delegate
					{
						MessagePopup.Open(title, message, delegate
						{
							SceneAutoBehaviour<PayShopManagement>.instance.OpenNewPayShop(NewPayShopCategoryType.Ruby.ToString());
						}, null);
					});
				}
				return (response.Result != NShared.ExtendEquip.ResultType.Success) ? base.TaskFactory.Run(delegate
				{
					MessagePopup.OpenError(response.Result);
				}) : new GroupTask().ADDTASK(base.TaskFactory.Run(delegate
				{
					GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.CurrencyResult);
					GlobalAutoBehaviour<AccountManager>.instance.UserManager.SetInventoryExtend(response.NewInventoryExtend);
				})).ADDTASK(base.TaskFactory.Run(delegate(InvenList window)
				{
					window.Refresh(immediate: false);
				})).ADDTASK(base.TaskFactory.Run(delegate
				{
					PopupExtendEquip(size);
				}));
			}));
		});
	}

	void InvenList.Listener.OnClickExtendChest(InvenList caller)
	{
		SendExtendChest();
	}

	private void SendExtendChest()
	{
		int chestExtend = GlobalAutoBehaviour<AccountManager>.instance.UserManager.MiscInfo.ChestExtend;
		GameRoot.Instance.InventoryExtendDataContainer.GetNextDataByExtendCount(chestExtend, InventoryType.Chest);
		ExtendInvenMessagePopup.Open(delegate(int size, int price)
		{
			AddTask(new WebServiceTask<NShared.ExtendChest.Request, NShared.ExtendChest.Response>(new NShared.ExtendChest.Request
			{
				ReqGem = price
			}, delegate(NShared.ExtendChest.Request request, NShared.ExtendChest.Response response)
			{
				if (response.Result == NShared.ExtendChest.ResultType.NotEnoughGem)
				{
					string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
					string message = LocalizationManager.GetString(NShared.NLocalization.System.MOVE_GEM_SHOP_CONFIRM);
					return base.TaskFactory.Run(delegate
					{
						MessagePopup.Open(title, message, delegate
						{
							SceneAutoBehaviour<PayShopManagement>.instance.OpenNewPayShop(NewPayShopCategoryType.Ruby.ToString());
						}, null);
					});
				}
				return (response.Result != NShared.ExtendChest.ResultType.Success) ? base.TaskFactory.Run(delegate
				{
					MessagePopup.OpenError(response.Result);
				}) : new GroupTask().ADDTASK(base.TaskFactory.Run(delegate
				{
					GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.CurrencyResult);
					GlobalAutoBehaviour<AccountManager>.instance.UserManager.SetChestExtend(response.NewChestExtend);
				})).ADDTASK(base.TaskFactory.Run(delegate(InvenList window)
				{
					window.Refresh(immediate: false);
				})).ADDTASK(base.TaskFactory.Run(delegate
				{
					PopupExtendChest(size);
				}));
			}));
		}, null, InventoryType.Chest);
	}

	void InvenList.Listener.OnClickExtendPunishmentRune(InvenList caller)
	{
		SendExtendPunishmentRune();
	}

	private void SendExtendPunishmentRune()
	{
		int punishmentRuneStorageSlotExtend = GlobalAutoBehaviour<AccountManager>.instance.UserManager.MiscInfo.PunishmentRuneStorageSlotExtend;
		GameRoot.Instance.InventoryExtendDataContainer.GetNextDataByExtendCount(punishmentRuneStorageSlotExtend, InventoryType.PunishmentRune);
		ExtendInvenMessagePopup.Open2(delegate(int size, int price)
		{
			AddTask(new WebServiceTask<NShared.ExtendPunishmentRuneStorageSlot.Request, NShared.ExtendPunishmentRuneStorageSlot.Response>(new NShared.ExtendPunishmentRuneStorageSlot.Request
			{
				ReqGem = price
			}, delegate(NShared.ExtendPunishmentRuneStorageSlot.Request request, NShared.ExtendPunishmentRuneStorageSlot.Response response)
			{
				if (response.Result == NShared.ExtendPunishmentRuneStorageSlot.ResultType.NotEnoughGem)
				{
					string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
					string message = LocalizationManager.GetString(NShared.NLocalization.System.MOVE_GEM_SHOP_CONFIRM);
					return base.TaskFactory.Run(delegate
					{
						MessagePopup.Open(title, message, delegate
						{
							SceneAutoBehaviour<PayShopManagement>.instance.OpenNewPayShop(NewPayShopCategoryType.Ruby.ToString());
						}, null);
					});
				}
				return (response.Result != NShared.ExtendPunishmentRuneStorageSlot.ResultType.Success) ? base.TaskFactory.Run(delegate
				{
					MessagePopup.OpenError(response.Result);
				}) : new GroupTask().ADDTASK(base.TaskFactory.Run(delegate
				{
					GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.CurrencyResult);
					GlobalAutoBehaviour<AccountManager>.instance.UserManager.SetPunishmentRuneExtend(response.PunishmentRuneStorageExtend);
				})).ADDTASK(base.TaskFactory.Run(delegate(InvenList window)
				{
					window.Refresh(immediate: false);
				})).ADDTASK(base.TaskFactory.Run(delegate
				{
					PopupExtendPunishmentRune(size);
				}));
			}));
		}, null, InventoryType.PunishmentRune);
	}

	private static void PopupExtendEquip(int ExtendedSlot)
	{
		CenterMessage.Open(LocalizationManager.GetString(Inventory.INVENTORY_ADD_SLOT, ExtendedSlot));
	}

	private static void PopupExtendChest(int ExtendedSlot)
	{
		CenterMessage.Open(LocalizationManager.GetString(Inventory.INVENTORY_ADD_CHEST_SLOT, ExtendedSlot));
	}

	private static void PopupExtendPunishmentRune(int ExtendedSlot)
	{
		CenterMessage.Open(LocalizationManager.GetString(Inventory.PUNISHMENT_ADD_SLOT, ExtendedSlot));
	}

	bool InvenList.Listener.IsNewItem(ItemType[] ItemTypes, EquipPartType[] partTypes, bool isInChestBox)
	{
		foreach (ItemType itemType in ItemTypes)
		{
			switch (itemType)
			{
			case ItemType.Equip:
			{
				bool flag = false;
				if (partTypes == null)
				{
					flag = true;
				}
				else if (partTypes.Length == 0)
				{
					flag = true;
				}
				if (flag)
				{
					if (GlobalAutoBehaviour<AccountManager>.instance.ItemManager.AddedItemExists(itemType))
					{
						return true;
					}
					break;
				}
				for (int j = 0; j < partTypes.Length; j++)
				{
					if (GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.IsNewEquipByParTypeWithInventoryMode(partTypes[j], isInChestBox))
					{
						return true;
					}
				}
				break;
			}
			case ItemType.PunishmentRune:
				if (GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.IsNewPunishmentRune())
				{
					return true;
				}
				break;
			default:
				if (GlobalAutoBehaviour<AccountManager>.instance.ItemManager.AddedItemExists(itemType))
				{
					return true;
				}
				break;
			}
		}
		return false;
	}

	void InvenList.Listener.OnClickBatchBreak()
	{
		AddTask(base.TaskFactory.Open(delegate(DismantleConfirmPopup window)
		{
			window.Init(this, isSell: false);
		}));
	}

	void InvenList.Listener.OnClickBatchSell()
	{
		AddTask(base.TaskFactory.Open(delegate(OptionSellConfirmPopup window)
		{
			window.Init(this, isSell: true);
		}));
	}

	void InvenList.Listener.OnClickBatchKeep()
	{
		AddTask(base.TaskFactory.Open(delegate(OptionKeepConfirmPopup window)
		{
			window.Init(this, isKeep: true);
		}));
	}

	void InvenList.Listener.OnClickBatchBring()
	{
		AddTask(base.TaskFactory.Open(delegate(OptionKeepConfirmPopup window)
		{
			window.Init(this, isKeep: false);
		}));
	}

	void InvenList.Listener.OnAddSlot(int index)
	{
		NShared.AddCraftSlot.Request request = new NShared.AddCraftSlot.Request
		{
			SlotIndex = index
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.AddCraftSlot.Response response)
		{
			if (response != null)
			{
				if (response.BaseResult != BaseResultType.Success)
				{
					MessagePopup.OpenError(response.BaseResult);
				}
				else if (response.Result == NShared.AddCraftSlot.ResultType.NotEnoughGold)
				{
					string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
					string message = LocalizationManager.GetString(NShared.NLocalization.System.MOVE_GOLD_SHOP_CONFIRM);
					MessagePopup.Open(title, message, delegate
					{
						SceneAutoBehaviour<PayShopManagement>.instance.OpenNewPayShop(NewPayShopCategoryType.Gold.ToString());
					}, null);
				}
				else if (response.Result != NShared.AddCraftSlot.ResultType.Success)
				{
					MessagePopup.OpenError(response.Result);
				}
				else
				{
					GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.CurrencyResult);
					GlobalAutoBehaviour<AccountManager>.instance.CraftSlotManager.Apply(response.CraftSlotResult);
				}
			}
		});
	}

	void InvenList.Listener.OnCraftItem(int index)
	{
		OpenCraftItemSelectMessagePopup(index);
	}

	void InvenList.Listener.OnCancelCraft(int index)
	{
		NShared.CancelCraftItem.Request request = new NShared.CancelCraftItem.Request
		{
			SlotIndex = index
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.CancelCraftItem.Response response)
		{
			if (response != null)
			{
				if (response.BaseResult != BaseResultType.Success)
				{
					MessagePopup.OpenError(response.BaseResult);
				}
				else if (response.Result != NShared.CancelCraftItem.ResultType.Success)
				{
					MessagePopup.OpenError(response.Result);
				}
				else
				{
					GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.CurrencyResult);
					GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.ItemResults);
					GlobalAutoBehaviour<AccountManager>.instance.CraftSlotManager.Apply(response.CraftSlotResult);
					SingletonAuto<NotificationManager>.instance.UnRegisteItemCraftCompletelNotification(response.CraftSlotResult.SlotIndex);
				}
			}
		});
	}

	void InvenList.Listener.OnFinishCraft(CraftItemComponent comp)
	{
		NShared.InstantCraftItem.Request request = new NShared.InstantCraftItem.Request
		{
			SlotIndex = comp._index,
			RemainTime = comp._remainTime
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.InstantCraftItem.Response response)
		{
			if (response != null)
			{
				if (response.BaseResult != BaseResultType.Success)
				{
					MessagePopup.OpenError(response.BaseResult);
				}
				else if (response.Result == NShared.InstantCraftItem.ResultType.NotEnoughGem)
				{
					string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
					string message = LocalizationManager.GetString(NShared.NLocalization.System.MOVE_GEM_SHOP_CONFIRM);
					MessagePopup.Open(title, message, delegate
					{
						SceneAutoBehaviour<PayShopManagement>.instance.OpenNewPayShop(NewPayShopCategoryType.Ruby.ToString());
					}, null);
				}
				else if (response.Result != NShared.InstantCraftItem.ResultType.Success)
				{
					MessagePopup.OpenError(response.Result);
				}
				else
				{
					comp.ShowFillAnimation();
					GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.CurrencyResult);
					GlobalAutoBehaviour<AccountManager>.instance.CraftSlotManager.Apply(response.CraftSlotResult);
					SingletonAuto<NotificationManager>.instance.UnRegisteItemCraftCompletelNotification(response.CraftSlotResult.SlotIndex);
				}
			}
		});
	}

	void InvenList.Listener.OnGetItem(int index)
	{
		NShared.TakeCraftItem.Request request = new NShared.TakeCraftItem.Request
		{
			SlotIndex = index
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.TakeCraftItem.Response response)
		{
			if (response != null)
			{
				if (response.BaseResult != BaseResultType.Success)
				{
					MessagePopup.OpenError(response.BaseResult);
				}
				else if (response.Result != NShared.TakeCraftItem.ResultType.Success)
				{
					MessagePopup.OpenError(response.Result);
				}
				else
				{
					GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.ItemResult);
					GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Apply(response.EquipItemResults);
					GlobalAutoBehaviour<AccountManager>.instance.CraftSlotManager.Apply(response.CraftSlotResult);
					ItemResultInfo[] array = ((response.ItemResult == null) ? null : new ItemResultInfo[1] { response.ItemResult });
					if (__Extension_Method_ICollection__.COUNT(array) > 0)
					{
						Array.Sort(array, (ItemResultInfo a, ItemResultInfo b) => b.ItemIndex.CompareTo(a.ItemIndex));
					}
					BaseRewardPopup.OpenPopup(new BaseRewardPopup.PopupOpenData().SetTitle(LocalizationManager.GetString(Inventory.INVENTORY_GET_CRAFT_ITEM)).SetItemResultInfos(array).SetEquipResultInfos(response.EquipItemResults), isGlobal: false);
				}
			}
		});
	}

	void InvenList.Listener.OnClickBreakRune(IEnumerable<ItemInfo> runeItemInfos, IEnumerable<EquipItemInfo> eqiupItemInfo)
	{
		if ((runeItemInfos == null || runeItemInfos.Count() == 0) && (eqiupItemInfo == null || eqiupItemInfo.Count() == 0))
		{
			return;
		}
		if (eqiupItemInfo != null)
		{
			string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
			string message = LocalizationManager.GetString(Inventory.INVENTORY_ASK_BREAK_MESSAGE);
			MessagePopup.Open(title, message, delegate
			{
				ProcessBreakRune(runeItemInfos, eqiupItemInfo);
			}, null);
		}
		else
		{
			ProcessBreakRune(runeItemInfos, eqiupItemInfo);
		}
	}

	void InvenList.Listener.OnClickBatchBreakRune()
	{
		AddTask(base.TaskFactory.Open(delegate(BatchRuneBreakPopup window)
		{
			window.Init(this);
		}));
	}

	void InvenList.Listener.OnClickKeep(IEnumerable<EquipItemInfo> equipItemInfos)
	{
		ProcessSetChest(equipItemInfos, delegate(bool success)
		{
			if (success)
			{
				GetInstance<InvenList>().CloseSubMenu();
				GetInstance<InvenList>().InitEquipmentFilter();
			}
		});
	}

	void InvenList.Listener.OnClickBring(IEnumerable<EquipItemInfo> equipItemInfos)
	{
		ProcessUnSetChest(equipItemInfos, delegate(bool success)
		{
			if (success)
			{
				GetInstance<InvenList>().CloseSubMenu();
				GetInstance<InvenList>().InitChestFilter();
			}
		});
	}

	public static void ProcessSellItem(ItemData itemData, long itemCount, Action<bool> onSell = null)
	{
		if (itemData == null)
		{
			return;
		}
		int num = 0;
		if (itemCount <= num)
		{
			return;
		}
		int itemIndex = itemData.Index;
		string text = LocalizationManager.GetString(Inventory.INVENTORY_ITEM_ACTION_SELL);
		string message = LocalizationManager.GetString(Inventory.INVENTORY_ASK_SELL_COUNT_MESSAGE);
		int min = 1;
		long max = itemCount;
		CountMessagePopup.Open(text, message, text, min, max, min, delegate(int number)
		{
			if (number >= min)
			{
				long num2 = max;
				if (number <= num2)
				{
					NShared.SellItem.Request request = new NShared.SellItem.Request
					{
						ItemIndices = new int[1] { itemIndex },
						ItemCount = new int[1] { number }
					};
					SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.SellItem.Response response)
					{
						if (response == null)
						{
							Vespa_System_Action_Extension_Method.fire(onSell, i_argA: false);
						}
						else if (response.BaseResult != BaseResultType.Success)
						{
							MessagePopup.OpenError(response.BaseResult);
							Vespa_System_Action_Extension_Method.fire(onSell, i_argA: false);
						}
						else if (response.Result != NShared.SellItem.ResultType.Success)
						{
							MessagePopup.OpenError(response.Result);
							Vespa_System_Action_Extension_Method.fire(onSell, i_argA: false);
						}
						else
						{
							string text2 = string.Empty;
							GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.CurrencyResult);
							if (response.CurrencyResult != null && response.CurrencyResult.CurrencyType == CurrencyType.Gold)
							{
								text2 = Vespa_Long_Extension_Method.ToCommaString(response.CurrencyResult.AddValue);
							}
							GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.ItemResults);
							CenterMessage.Open(LocalizationManager.GetString(Inventory.NOTIFY_ITEM_SELL, ItemNameBuilder.GetItemName(itemData), text2));
							Vespa_System_Action_Extension_Method.fire(onSell, i_argA: true);
						}
					});
				}
			}
		}).SetSinglePrice(ShopCostType.Gold, GlobalAutoBehaviour<DropBonusEventManager>.instance.GetCheckEquipSellEventBonus(itemData));
	}

	public static void ProcessSellEquip(EquipItemInfo equipItemInfo, Action<bool> onSell = null)
	{
		if (equipItemInfo == null)
		{
			Vespa_System_Action_Extension_Method.fire(onSell, i_argA: false);
			return;
		}
		ItemData itemData = GameRoot.Instance.ItemDataContainer.GetData(equipItemInfo.ItemIndex);
		if (itemData == null)
		{
			Vespa_System_Action_Extension_Method.fire(onSell, i_argA: false);
			return;
		}
		string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
		string empty = string.Empty;
		LocalizationManager.GetString(Inventory.INVENTORY_ITEM_ACTION_SELL);
		LocalizationManager.GetString(NShared.NLocalization.Common.COMMON_CANCEL);
		string action = LocalizationManager.GetString(Inventory.INVENTORY_ITEM_ACTION_SELL);
		empty = LocalizationManager.GetString(Inventory.INVENTORY_ASK_SELL_MESSAGE);
		PricedMessagePopup.Open(title, empty, action, PricedMessagePopup.EPriceType.Gold, GlobalAutoBehaviour<DropBonusEventManager>.instance.GetCheckEquipSellEventBonus(itemData), delegate
		{
			SellEquip(equipItemInfo, itemData, onSell);
		});
	}

	private static void SellEquip(EquipItemInfo equipItemInfo, ItemData itemData, Action<bool> onSell = null)
	{
		NShared.SellEquip.Request request = new NShared.SellEquip.Request
		{
			EquipItemSlotIndices = new int[1] { equipItemInfo.SlotIndex }
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.SellEquip.Response response)
		{
			if (response.Result != NShared.SellEquip.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
				Vespa_System_Action_Extension_Method.fire(onSell, i_argA: false);
			}
			else
			{
				string text = string.Empty;
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.CurrencyResult);
				if (response.CurrencyResult != null && response.CurrencyResult.CurrencyType == CurrencyType.Gold)
				{
					text = Vespa_Long_Extension_Method.ToCommaString(response.CurrencyResult.AddValue);
				}
				int[] equipItemSlotIndices = request.EquipItemSlotIndices;
				foreach (int slotIndex in equipItemSlotIndices)
				{
					GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Apply(slotIndex, null);
				}
				string itemName = ItemNameBuilder.GetItemName(itemData);
				if (!String_Extension_Method.IsNullOrEmpty(itemName))
				{
					CenterMessage.Open(LocalizationManager.GetString(Inventory.NOTIFY_ITEM_SELL, itemName, text));
				}
				else
				{
					CenterMessage.Open(LocalizationManager.GetString(Inventory.ITEM_SOLD_MSG, text));
				}
				Vespa_System_Action_Extension_Method.fire(onSell, i_argA: true);
			}
		});
	}

	public static void ProcessSellEquip(IEnumerable<EquipItemInfo> equipItemInfos, Action<bool> onSell = null)
	{
		if (equipItemInfos == null || equipItemInfos.Count() == 0)
		{
			Vespa_System_Action_Extension_Method.fire(onSell, i_argA: false);
			return;
		}
		long num = 0L;
		string itemName = string.Empty;
		foreach (EquipItemInfo equipItemInfo in equipItemInfos)
		{
			ItemData data = GameRoot.Instance.ItemDataContainer.GetData(equipItemInfo.ItemIndex);
			if (data != null)
			{
				num += GlobalAutoBehaviour<DropBonusEventManager>.instance.GetCheckEquipSellEventBonus(data);
			}
		}
		if (equipItemInfos.Count() == 1)
		{
			EquipItemInfo[] array = equipItemInfos.ToArray();
			ItemData data2 = GameRoot.Instance.ItemDataContainer.GetData(array[0].ItemIndex);
			if (data2 != null)
			{
				itemName = ItemNameBuilder.GetItemName(data2);
			}
		}
		string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
		string message = LocalizationManager.GetString(Inventory.INVENTORY_ASK_SELL_MESSAGE);
		string action = LocalizationManager.GetString(Inventory.INVENTORY_ITEM_ACTION_SELL);
		PricedMessagePopup.Open(title, message, action, PricedMessagePopup.EPriceType.Gold, num, delegate
		{
			NShared.SellEquip.Request request = new NShared.SellEquip.Request
			{
				EquipItemSlotIndices = equipItemInfos.Select((EquipItemInfo e) => e.SlotIndex).ToArray()
			};
			SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.SellEquip.Response response)
			{
				if (response.Result != NShared.SellEquip.ResultType.Success)
				{
					MessagePopup.OpenError(response.Result);
					Vespa_System_Action_Extension_Method.fire(onSell, i_argA: false);
				}
				else
				{
					GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.CurrencyResult);
					string text = string.Empty;
					if (response.CurrencyResult != null && response.CurrencyResult.CurrencyType == CurrencyType.Gold)
					{
						text = Vespa_Long_Extension_Method.ToCommaString(response.CurrencyResult.AddValue);
					}
					int[] equipItemSlotIndices = request.EquipItemSlotIndices;
					foreach (int slotIndex in equipItemSlotIndices)
					{
						GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Apply(slotIndex, null);
					}
					if (!String_Extension_Method.IsNullOrEmpty(itemName))
					{
						CenterMessage.Open(LocalizationManager.GetString(Inventory.NOTIFY_ITEM_SELL, itemName, text));
					}
					else
					{
						CenterMessage.Open(LocalizationManager.GetString(Inventory.ITEM_SOLD_MSG, text));
					}
					Vespa_System_Action_Extension_Method.fire(onSell, i_argA: true);
				}
			});
		});
	}

	public static void ProcessEquipLock(EquipItemInfo equipItemInfo, byte locked)
	{
		if (equipItemInfo == null)
		{
			VespaDebug.LogError("equipItemInfo is null");
			return;
		}
		if (equipItemInfo.Locked == locked)
		{
			VespaDebug.LogError("Locked is same. buggy situation");
			return;
		}
		int equipItemSlotIndex = equipItemInfo.SlotIndex;
		NShared.SetLockEquipItem.Request request = new NShared.SetLockEquipItem.Request
		{
			EquipItemSlotIndex = equipItemSlotIndex,
			Locked = locked
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.SetLockEquipItem.Response response)
		{
			if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
			}
			else if (response.Result == NShared.SetLockEquipItem.ResultType.AlreadyLocked || response.Result == NShared.SetLockEquipItem.ResultType.AlreadyUnLocked)
			{
				locked = ((response.Result == NShared.SetLockEquipItem.ResultType.AlreadyLocked) ? ((byte)1) : ((byte)0));
				EquipItemInfo equipItemInfo2 = JsonMarshaler.Clone(equipItemInfo);
				equipItemInfo2.Locked = locked;
				GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Apply(equipItemSlotIndex, equipItemInfo2);
			}
			else if (response.Result != NShared.SetLockEquipItem.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				EquipItemInfo equipItemInfo3 = JsonMarshaler.Clone(equipItemInfo);
				equipItemInfo3.Locked = locked;
				GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Apply(equipItemSlotIndex, equipItemInfo3);
				CenterMessage.Open(LocalizationManager.GetString((locked != 0) ? Item.LOCK_MESSAGE_1 : Item.LOCK_RELEASE_MESSAGE));
			}
		});
	}

	void InvenItemDesc.Listener.OpenRestoreSoulStone()
	{
		SceneAutoBehaviour<SoulWeaponManagement>.instance.OpenSoulStoneRestoreWindow();
	}

	void InvenItemDesc.Listener.MoveDirect(MoveDirectRegistInspector moveDirect)
	{
		switch (moveDirect.Type)
		{
		case MoveDirectRegistInspector.MoveDirectType.MoveNode:
			SceneAutoBehaviour<LobbyManagement>.instance.MoveNode(moveDirect.ChapterIndex, moveDirect.DungeonIndex);
			break;
		case MoveDirectRegistInspector.MoveDirectType.MoveMap:
			SceneAutoBehaviour<LobbyManagement>.instance.MoveMap(moveDirect.ChapterIndex, moveDirect.DungeonIndex);
			break;
		}
		CloseAllActiveWindow(immediately: true);
		if (SceneAutoBehaviour<MultiPartyManagement>.isValidInstance)
		{
			SceneAutoBehaviour<MultiPartyManagement>.instance.CloseAllActiveWindow(immediately: true);
		}
		if (SceneAutoBehaviour<RaidManagement>.isValidInstance)
		{
			SceneAutoBehaviour<RaidManagement>.instance.CloseAllActiveWindow(immediately: true);
		}
	}

	void InvenItemDesc.Listener.OnSell(InvenItemDesc caller)
	{
		if (caller == null || caller.ItemData == null)
		{
			return;
		}
		if (caller.EquipItemInfo != null)
		{
			ProcessSellEquip(caller.EquipItemInfo, delegate(bool success)
			{
				if (success)
				{
					caller.Close();
				}
			});
		}
		else
		{
			ProcessSellItem(caller.ItemData, caller.ItemCount);
		}
	}

	void InvenItemDesc.Listener.OnShortcut(InvenItemDesc caller)
	{
		if (caller == null || caller.ItemData == null)
		{
			return;
		}
		ClientActivityType moveTaskType = caller.ItemData.MoveTaskType;
		string[] moveTaskValue = caller.ItemData.MoveTaskValue;
		List<IWindow> list = null;
		switch (moveTaskType)
		{
		case ClientActivityType.OpenInventory:
		{
			if (GlobalAutoBehaviour<AccountManager>.instance.CraftSlotManager.HasEmptyCraftSlot())
			{
				SceneAutoBehaviour<ActivityManager>.instance.Run(moveTaskType.ToString(), moveTaskValue);
				InvenList invenList = GetInstance<InvenList>(openedOnly: true);
				if (invenList != null)
				{
					list = list ?? new List<IWindow>();
					list.Add(invenList);
				}
				break;
			}
			string title2 = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
			string stringDirect2 = LocalizationManager.GetStringDirect("ITEMSHORTCUT_CRAFTSLOTFULL_POPUP");
			MessagePopup.Open(title2, stringDirect2);
			return;
		}
		case ClientActivityType.MoveToNode:
		{
			int valueInt = ArgParser.GetValueInt(moveTaskValue, 0);
			int valueInt2 = ArgParser.GetValueInt(moveTaskValue, 1);
			if (GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.CanGoToChapter(valueInt) && GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.CanVisit(valueInt, valueInt2))
			{
				SceneAutoBehaviour<ActivityManager>.instance.Run(moveTaskType.ToString(), moveTaskValue);
				break;
			}
			string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
			string stringDirect = LocalizationManager.GetStringDirect("ITEMSHORTCUT_NOTUSEDPRESENT_POPUP");
			MessagePopup.Open(title, stringDirect);
			return;
		}
		}
		if (SceneAutoBehaviour<MyHeroManagement>.isValidInstance)
		{
			SceneAutoBehaviour<MyHeroManagement>.instance.CloseAllActiveWindow(immediately: true);
		}
		CloseAllActiveWindow(immediately: false, list);
	}

	void InvenItemDesc.Listener.OnUseItem(InvenItemDesc caller)
	{
		ItemData itemData = caller.ItemData;
		if (itemData == null)
		{
			return;
		}
		switch (itemData.Type)
		{
		case ItemType.Potion:
			OnUsePotionItem(itemData, caller);
			break;
		case ItemType.Booster:
			OnUseBoosterItem(itemData);
			break;
		case ItemType.HeroSelect:
			OnUseHeroSelectItem(itemData);
			break;
		case ItemType.FreeEquipGacha:
			OnUseFreeEquipGacha(itemData);
			break;
		case ItemType.Flask:
			OnUseFlaskItem(itemData);
			break;
		case ItemType.WeaponUniqueSelectItem:
			OnUseWeaponUniqueSelectItem(itemData);
			break;
		case ItemType.SoulStonePackageSelectItem:
			OnUseSoulWeaponSelectItem(itemData);
			break;
		case ItemType.EquipOptionSelectItem:
			OnUseOptionSelectItem(itemData);
			break;
		case ItemType.CostumeSelectItem:
			OnUseSelectCostumeItem(itemData);
			break;
		case ItemType.EquipOptionRenewCoupon:
			caller.Close();
			OnUseEquipOptionRenewCoupon(itemData);
			break;
		case ItemType.NickChange:
			OnUseNickChangeItem(itemData);
			break;
		case ItemType.GuildNameChange:
			OnUseGuildNameChangeItem(itemData);
			break;
		case ItemType.UpgradeBook:
			caller.Close();
			OnUseSkillBook(itemData);
			break;
		case ItemType.PackageSelectItem:
			caller.Close();
			OnUsePackageSelectItem(itemData);
			break;
		case ItemType.TranscendPoint:
			caller.Close();
			OnUseTranscendPointItem(itemData);
			break;
		case ItemType.AccessorySelectItem:
			OnUseAccessorySelectItem(itemData);
			break;
		case ItemType.TransitionTicket:
			OnUseEquipTransitionTicket(itemData);
			break;
		case ItemType.SoulWeaponOptionRenewCoupon:
			if (SingletonAuto<ContentsOpenChecker>.instance.CheckContentsOpen(ContentsType.GodkingTrial))
			{
				if (__Extension_Method_ICollection__.IsNullOrEmpty(GlobalAutoBehaviour<AccountManager>.instance.GetOwnedEquipItemList(delegate(EquipItemInfo e)
				{
					if (DispatchHelper.IsDispatchedHero(e.EquippedHeroInfo))
					{
						return false;
					}
					return e.IsSoulWeapon() ? true : false;
				})))
				{
					CenterMessage.Open(LocalizationManager.GetStringDirect("SOULWEAPON_CHANGE_NOTGET"));
					break;
				}
				caller.Close();
				OnUseSoulWeaponOptionRenewCoupon();
			}
			else
			{
				CenterMessage.Open(LocalizationManager.GetStringDirect("SOULWEAPON_CHANGE_CANTENTER"));
			}
			break;
		case ItemType.PetSelect:
			OnUsePetSelectItem(itemData);
			break;
		case ItemType.SoulWeaponAbilityTicket:
			if (SingletonAuto<ContentsOpenChecker>.instance.CheckContentsOpen(ContentsType.GodkingTrial))
			{
				OnUseSoulWeaponAbilityTicket(itemData);
			}
			else
			{
				CenterMessage.Open(LocalizationManager.GetStringDirect("ITEM_USE_CONDITION_GODKING"));
			}
			break;
		case ItemType.ExtraSkillRenewCoupon:
			if (!SingletonAuto<ContentsOpenChecker>.instance.CheckContentsOpen(ContentsType.Valance))
			{
				CenterMessage.Open(LocalizationManager.GetStringDirect("TECHNOMAGIC_ITEM_NOTICE_QUICKBUTTON"));
				break;
			}
			caller.Close();
			OnUseExtraSkillRenewCoupon(itemData);
			break;
		case ItemType.HeroGrowth:
			OnUseHeroGrowthItem(itemData);
			break;
		case ItemType.MultipleHeroSelect:
			OnUseMultipleHeroSelectItem(itemData);
			break;
		}
	}

	private void OnUseSoulWeaponOptionRenewCoupon()
	{
		SceneAutoBehaviour<SoulWeaponManagement>.instance.OpenSoulWeaponManageWindow(null, isSelectFirst: true);
	}

	public void OnUseAccessorySelectItem(ItemData itemData, bool disableConfime = false)
	{
		if (itemData.GetSubData<AccessorySelectItemData>() == null)
		{
			VespaDebug.LogError("OnUseItem::accessoryCostumeItemData == null");
		}
		else
		{
			GetInstance<AccessoryHeroSelect>().Open(itemData, this, disableConfime);
		}
	}

	private void OnUseSkillBook(ItemData itemData)
	{
		if (itemData != null)
		{
			UpgradeBookItemData data = GameRoot.Instance.UpgradeBookItemDataContainer.GetData(itemData.Code);
			if (data != null)
			{
				GetInstance<SkillBookHeroSelectPopup>().Open(data, this);
			}
		}
	}

	private void OpenCinemaPackage(ItemData itemData)
	{
		if (itemData != null)
		{
			GetInstance<CinemaItemScene>().Open(itemData, this);
		}
	}

	private void OnUseTranscendPointItem(ItemData itemData)
	{
		if (itemData != null)
		{
			UseTpItemHeroListPopup.OpenWindow();
		}
	}

	private void OnUsePackageSelectItem(ItemData usingItemData)
	{
		if (usingItemData == null)
		{
			return;
		}
		SelectItemData data = GameRoot.Instance.SelectItemDataContainer.GetData(usingItemData.Code);
		if (data == null)
		{
			return;
		}
		ItemInfo itemInfo = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Get(usingItemData.Index);
		if (itemInfo == null)
		{
			return;
		}
		if (data.SelectType == SelectItemPopupType.RuneSelectItem)
		{
			RuneSelectItemUsePopup runeSelectItemUsePopup = GetInstance<RuneSelectItemUsePopup>();
			if (runeSelectItemUsePopup != null)
			{
				runeSelectItemUsePopup.Open(usingItemData, itemInfo.Count, this);
			}
			return;
		}
		SelectionPopupItemInfo usingItem = new SelectionPopupItemInfo(usingItemData, itemInfo.Count);
		int num = __Extension_Method_ICollection__.COUNT(data.ItemIndices);
		List<SelectionPopupItemInfo> list = new List<SelectionPopupItemInfo>();
		for (int i = 0; i < num; i++)
		{
			int index = data.ItemIndices[i];
			ItemData data2 = GameRoot.Instance.ItemDataContainer.GetData(index);
			if (data2 != null)
			{
				int count = 0;
				ItemInfo itemInfo2 = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Get(data2.Index);
				if (itemInfo2 != null)
				{
					count = itemInfo2.Count;
				}
				SelectionPopupItemInfo item = new SelectionPopupItemInfo(data2, count);
				list.Add(item);
			}
		}
		SelectionItemUsePopup.Open(usingItem, list, this);
	}

	private void OnUseEquipTransitionTicket(ItemData itemData)
	{
		if (itemData == null)
		{
			return;
		}
		EquipTransitionTicketData data = GameRoot.Instance.EquipTransitionTicketDataContainer.GetData(itemData.Index);
		if (data != null)
		{
			EquipTransitionType type = data.Type;
			if ((uint)type <= 1u)
			{
				GetInstance<EquipTransitionTicket>().Open(this, itemData);
			}
		}
	}

	private void OnUseSoulWeaponAbilityTicket(ItemData itemData)
	{
		if (itemData != null && GameRoot.Instance.SoulWeaponAbilityTicketDataContainer.GetData(itemData.Code) != null)
		{
			GetInstance<SoulWeaponAbilityTicket>().Open(this, itemData);
		}
	}

	void SelectionItemUsePopup.IListener.OnClickOk(SelectionItemUsePopup popup, int usingItemIndex, int selecteditemIndex, int itemCount)
	{
		NShared.UsePackageSelectItem.Request request = new NShared.UsePackageSelectItem.Request
		{
			ItemIndex = usingItemIndex,
			ItemCount = itemCount,
			SelectItemIndex = selecteditemIndex
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.UsePackageSelectItem.Response response)
		{
			if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else if (response.Result != NShared.UsePackageSelectItem.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.ItemResult);
				GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.RewardItemResult);
				BaseRewardPopup.PopupOpenData popupOpenData = new BaseRewardPopup.PopupOpenData();
				popupOpenData.SetItemResultInfos(new ItemResultInfo[1] { response.RewardItemResult });
				BaseRewardPopup.OpenPopup(popupOpenData, isGlobal: false);
				popup.Close();
			}
		});
	}

	void RuneSelectItemUsePopup.IListener.OnClickConfirm(ItemData usingItemData, int usingItemCount, Action onComplete)
	{
		RuneSelectItemListWindow runeSelectItemListWindow = GetInstance<RuneSelectItemListWindow>();
		if (runeSelectItemListWindow != null)
		{
			Vespa_System_Action_Extension_Method.fire(onComplete);
			runeSelectItemListWindow.Open(usingItemData, usingItemCount, this);
		}
	}

	void RuneSelectItemListWindow.IListener.OnOpenItem(int usingItemIndex, int selectedItemIndex, int selectedItemCount)
	{
		NShared.UsePackageSelectItem.Request request = new NShared.UsePackageSelectItem.Request
		{
			ItemIndex = usingItemIndex,
			ItemCount = selectedItemCount,
			SelectItemIndex = selectedItemIndex
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.UsePackageSelectItem.Response response)
		{
			if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else if (response.Result != NShared.UsePackageSelectItem.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.ItemResult);
				GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.RewardItemResult);
				BaseRewardPopup.PopupOpenData popupOpenData = new BaseRewardPopup.PopupOpenData();
				popupOpenData.SetItemResultInfos(new ItemResultInfo[1] { response.RewardItemResult });
				BaseRewardPopup.OpenPopup(popupOpenData, isGlobal: false);
			}
		});
	}

	void RuneSelectItemListWindow.IListener.OnOpenOptionFilterWindow(List<Enum> alreadySelectFilterOption, int selectAbleCount)
	{
		RuneSelectItemFilterOptionList runeSelectItemFilterOptionList = GetInstance<RuneSelectItemFilterOptionList>();
		if (runeSelectItemFilterOptionList != null)
		{
			runeSelectItemFilterOptionList.Open(this, alreadySelectFilterOption, selectAbleCount);
		}
	}

	void RuneSelectItemFilterOptionList.IListener.OnClickConfirm(List<Enum> selectFilterOptionList, Action onComplete)
	{
		RuneSelectItemListWindow runeSelectItemListWindow = GetInstance<RuneSelectItemListWindow>();
		if (runeSelectItemListWindow != null)
		{
			runeSelectItemListWindow.RefreshRuneItemListApplyFilter(selectFilterOptionList);
			Vespa_System_Action_Extension_Method.fire(onComplete);
		}
	}

	void CinemaItemScene.Listener.OnClickUse(CinemaItemScene caller, Action<NShared.UsePackageItem.Response> OnResponse)
	{
		NShared.UsePackageItem.Request request = new NShared.UsePackageItem.Request
		{
			ItemIndex = caller.ItemData.Index,
			ItemCount = 1
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.UsePackageItem.Response response)
		{
			if (response.Result != NShared.UsePackageItem.ResultType.Success)
			{
				Vespa_System_Action_Extension_Method.fire(OnResponse, null);
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.ItemResult);
				for (int i = 0; i < response.RewardResults.Length; i++)
				{
					RewardResultInfo result = response.RewardResults[i];
					GlobalAutoBehaviour<AccountManager>.instance.ApplyRewardResult(result);
				}
				Vespa_System_Action_Extension_Method.fire(OnResponse, response);
			}
		});
	}

	void InvenItemDesc.Listener.OnToHero(InvenItemDesc caller)
	{
		MoveToHeroInventory(ItemType.Rune);
	}

	void InvenItemDesc.Listener.OnOpenItem(InvenItemDesc caller, long itemCount)
	{
		OpenUsePackageConfirmPopup(caller.ItemData, checked((int)itemCount));
	}

	void InvenRandomBoxDesc.Listener.OnOpenItem(InvenRandomBoxDesc caller, long itemCount)
	{
		if (caller.ItemData.Type == ItemType.CinemaPackage)
		{
			OpenCinemaPackage(caller.ItemData);
		}
		else
		{
			OpenUsePackageConfirmPopup(caller.ItemData, checked((int)itemCount));
		}
	}

	private void OpenUsePackageConfirmPopup(ItemData itemData, int itemCount)
	{
		if (itemData == null)
		{
			return;
		}
		PackageItemData packageData = GameRoot.Instance.PackageItemDataContainer.GetData(itemData.Code);
		if (packageData == null)
		{
			return;
		}
		Helper.GetItemCount(itemData.Index);
		OpenPackagePopup.Open(itemData, itemCount, delegate
		{
			if (packageData.HasEquip)
			{
				int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("AllowEquipOverflow", 2000);
				if (__Extension_Method_ICollection__.COUNT(GlobalAutoBehaviour<AccountManager>.instance.GetUnequippedEquipItemInfoList(includeChest: false)) >= valueInt)
				{
					string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
					string stringDirect = LocalizationManager.GetStringDirect("EQUIP_ITEM_FULL_DESC");
					MessagePopup.OpenEnabled(title, stringDirect, delegate
					{
						GetInstance<InvenRandomBoxDesc>().Close();
						GetInstance<InvenList>().Init(this);
					}, null, "MOVE_EQUIP_BAG", bYesEnabled: true);
					return;
				}
			}
			ExecUsePackageItem(itemData, itemCount);
		}, null, (packageData.OpenWithdrawal > 0) ? ((Action)delegate
		{
			if (GlobalAutoBehaviour<AccountManager>.instance.CountryServerName.Equals("Korea"))
			{
				PurchasePolicyPopup.OpenPopup();
			}
			else
			{
				string title = LocalizationManager.GetString(NShared.NLocalization.PayShop.CANCEL_DETAIL_TITLE);
				string empty = string.Empty;
				string withdrawTextKey = packageData.WithdrawTextKey;
				empty = ((!String_Extension_Method.IsNullOrEmpty(withdrawTextKey) && LocalizationManager.IsValidKey(withdrawTextKey)) ? LocalizationManager.GetStringDirect(withdrawTextKey) : LocalizationManager.GetString(NShared.NLocalization.PayShop.CANCEL_DETAIL_MSG));
				MessagePopup.Open(title, empty);
			}
		}) : null);
	}

	private HashSet<CurrencyType> GetCurrencyTypesFromPackageItem(ItemData itemData)
	{
		PackageItemData data = GameRoot.Instance.PackageItemDataContainer.GetData(itemData.Code);
		if (data == null)
		{
			return null;
		}
		int rewardIndex = data.RewardIndex1;
		RewardData data2 = GameRoot.Instance.RewardDataContainer.GetData(rewardIndex);
		if (data2 == null)
		{
			return null;
		}
		HashSet<CurrencyType> hashSet = null;
		int GroupCount;
		foreach (RewardElementData item in data2.GetRewardItemListWithGroupCount(out GroupCount, ignoreRate: true))
		{
			CurrencyType currencyType = item.GetCurrencyType();
			if (currencyType != CurrencyType.None)
			{
				hashSet = hashSet ?? new HashSet<CurrencyType>();
				hashSet.Add(currencyType);
			}
		}
		return hashSet;
	}

	private void ExecUsePackageItem(ItemData itemData, int itemCount)
	{
		CurrencyMaxCap.WarningOrExecute(GetCurrencyTypesFromPackageItem(itemData), delegate
		{
			ExecUsePackageItem_Real(itemData, itemCount);
		});
	}

	private void ExecUsePackageItem_Real(ItemData itemData, int itemCount)
	{
		AddTask(new WebServiceTask<NShared.UsePackageItem.Request, NShared.UsePackageItem.Response>(new NShared.UsePackageItem.Request
		{
			ItemIndex = itemData.Index,
			ItemCount = itemCount
		}, (NShared.UsePackageItem.Request request, NShared.UsePackageItem.Response response) => (response.Result != NShared.UsePackageItem.ResultType.Success) ? base.TaskFactory.Run(delegate
		{
			MessagePopup.OpenError(response.Result);
		}) : base.TaskFactory.Run(delegate
		{
			OnResponseUsePackageItem(response, itemData.Name);
		})));
	}

	private void OnResponseUsePackageItem(NShared.UsePackageItem.Response response, string usedItemName)
	{
		SequenceTask sequenceTask = new SequenceTask();
		bool effect_ended = false;
		sequenceTask.AddTask(base.TaskFactory.Run(delegate
		{
			GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.ItemResult);
			for (int i = 0; i < response.RewardResults.Length; i++)
			{
				RewardResultInfo result = response.RewardResults[i];
				GlobalAutoBehaviour<AccountManager>.instance.ApplyRewardResult(result);
			}
		}));
		if (ClientValueManager.IsIgnoreItemEffect)
		{
			effect_ended = true;
		}
		else
		{
			sequenceTask.AddTask(base.TaskFactory.Run(delegate
			{
				EmptyEffectPopup.Open(delegate
				{
					effect_ended = true;
				});
			}));
		}
		sequenceTask.AddTask(new WaitTask(() => !effect_ended));
		BaseRewardPopup.PopupOpenData popupOpen = new BaseRewardPopup.PopupOpenData(response);
		string text = "";
		text = ((response.RewardResults.Length != 1) ? LocalizationManager.GetString(Item.PACKAGE_ITEM_TITLE, __Extension_Method_ICollection__.COUNT(response.RewardResults)) : usedItemName);
		popupOpen.SetTitle(text);
		sequenceTask.AddTask(base.TaskFactory.Run(delegate
		{
			BaseRewardPopup.OpenPopup(popupOpen, isGlobal: false);
		}));
		sequenceTask.Commit();
	}

	bool InvenItemDesc.Listener.OnCloseRequest(InvenItemDesc CloseThis)
	{
		return true;
	}

	void InvenItemDesc.Listener.OnBreakRuneItem(InvenItemDesc caller)
	{
		if (caller.ItemData.Type == ItemType.PunishmentRune)
		{
			string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
			string message = LocalizationManager.GetString(Inventory.INVENTORY_ASK_BREAK_MESSAGE);
			MessagePopup.Open(title, message, delegate
			{
				List<EquipItemInfo> runeEquipItemInfos = new List<EquipItemInfo> { caller.EquipItemInfo };
				ProcessBreakRune(null, runeEquipItemInfos, delegate
				{
					caller.Close();
				});
			}, null);
			return;
		}
		int min = 1;
		long max = caller.ItemCount;
		RuneBreakCountMessagePopup.Open(caller.ItemData, min, max, min, delegate(int number)
		{
			if (number >= min)
			{
				long num = max;
				if (number <= num)
				{
					ItemInfo itemInfo = JsonMarshaler.Clone(GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Get(caller.ItemData.Index));
					itemInfo.Count = number;
					List<ItemInfo> runeItemInfos = new List<ItemInfo> { itemInfo };
					ProcessBreakRune(runeItemInfos, null);
				}
			}
		});
	}

	void InvenItemDesc.Listener.OnLockRuneItem(InvenItemDesc caller)
	{
		if (caller.ItemData != null)
		{
			if (caller.ItemData.Type == ItemType.PunishmentRune && caller.EquipItemInfo != null)
			{
				ProcessEquipLock(caller.EquipItemInfo, 1);
			}
			else
			{
				ProcessSetLockRuneItem(caller.ItemData.Index, 1);
			}
		}
	}

	void InvenItemDesc.Listener.OnUnlockRuneItem(InvenItemDesc caller)
	{
		if (caller.ItemData != null)
		{
			if (caller.ItemData.Type == ItemType.PunishmentRune && caller.EquipItemInfo != null)
			{
				ProcessEquipLock(caller.EquipItemInfo, 0);
			}
			else
			{
				ProcessSetLockRuneItem(caller.ItemData.Index, 0);
			}
		}
	}

	public static void ProcessSetLockRuneItem(int runeItemIndex, byte locked)
	{
		ItemInfo itemInfo = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Get(runeItemIndex);
		if (itemInfo == null)
		{
			VespaDebug.LogError("item info is not found, itemIndex = " + runeItemIndex);
			return;
		}
		if (itemInfo.Locked == locked)
		{
			VespaDebug.LogError("Locked is same. buggy situation");
			return;
		}
		NShared.SetLockRuneItem.Request request = new NShared.SetLockRuneItem.Request
		{
			RuneItemIndex = runeItemIndex,
			Locked = locked
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.SetLockRuneItem.Response response)
		{
			if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
			}
			else if (response.Result == NShared.SetLockRuneItem.ResultType.AlreadyLocked || response.Result == NShared.SetLockRuneItem.ResultType.AlreadyUnLocked)
			{
				locked = ((response.Result == NShared.SetLockRuneItem.ResultType.AlreadyLocked) ? ((byte)1) : ((byte)0));
				GlobalAutoBehaviour<AccountManager>.instance.ItemManager.SetLocked(runeItemIndex, locked);
			}
			else if (response.Result != NShared.SetLockRuneItem.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.ItemManager.SetLocked(runeItemIndex, locked);
				CenterMessage.Open(LocalizationManager.GetString((locked != 0) ? Item.LOCK_MESSAGE_1 : Item.LOCK_RELEASE_MESSAGE));
			}
		});
	}

	void InvenItemDesc.Listener.OnBreakItem(InvenItemDesc caller, bool isSoulStone)
	{
		int min = 1;
		long max = caller.ItemCount;
		ItemBreakCountMessagePopup.Open(caller.ItemData, min, max, min, delegate(int number)
		{
			if (number >= min)
			{
				long num = max;
				if (number <= num)
				{
					List<int> itemIndices = new List<int>();
					itemIndices.Add(caller.ItemData.Index);
					List<int> countList = new List<int>();
					countList.Add(number);
					BreakConfirmPopup.Open(caller.ItemData, number, delegate
					{
						ProcessBreakItem(itemIndices, countList, isSoulStone);
					}, null);
				}
			}
		});
	}

	private void ProcessBreakItem(List<int> itemIndices, List<int> countList, bool isSoulStone)
	{
		NShared.BreakItem.Request request = new NShared.BreakItem.Request
		{
			ItemIndices = itemIndices.ToArray(),
			Counts = countList.ToArray()
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.BreakItem.Response response)
		{
			if (response != null)
			{
				if (response.BaseResult != BaseResultType.Success)
				{
					MessagePopup.OpenError(response.BaseResult);
				}
				else if (response.Result != NShared.BreakItem.ResultType.Success)
				{
					MessagePopup.OpenError(response.Result);
				}
				else
				{
					InvenList invenList = GetInstance<InvenList>();
					if (invenList != null)
					{
						invenList.CloseSubMenu();
					}
					GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.DecItemResults);
					GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.ItemResults);
					GlobalAutoBehaviour<AccountManager>.instance.PlayerCurrencyManager.Apply(response.CurrencyResults);
					string title = (isSoulStone ? LocalizationManager.GetStringDirect("EQUIP_BREAK_SUCCESS") : LocalizationManager.GetString(ItemUse.HEROCHOICE_FRAGMENT_TITLE));
					BaseRewardPopup.OpenPopup(new BaseRewardPopup.PopupOpenData().SetTitle(title).SetItemResultInfos(response.ItemResults).SetCurrencyResultInfos3(response.CurrencyResults), isGlobal: false);
				}
			}
		});
	}

	bool HeroEquipInfo.Listener.OnCloseRequest(HeroEquipInfo closeThis)
	{
		SceneAutoBehaviour<MyHeroManagement>.instance.CloseEquipUpgrade();
		return true;
	}

	void HeroEquipInfo.Listener.OnAwaken(HeroEquipInfo closeThis)
	{
	}

	void HeroEquipInfo.Listener.OnEquip(HeroEquipInfo closeThis)
	{
	}

	void HeroEquipInfo.Listener.OnUnequip(HeroEquipInfo closeThis)
	{
	}

	void HeroEquipInfo.Listener.OnSell(HeroEquipInfo closeThis)
	{
	}

	void HeroEquipInfo.Listener.OnLock(HeroEquipInfo closeThis, byte Locked)
	{
		ProcessEquipLock(closeThis.EquipItemInfo, Locked);
	}

	void InvenEquipInfo.Listener.OnMove(InvenEquipInfo component)
	{
		EquipItemInfo equipItemInfo = component.EquipItemInfo;
		if (equipItemInfo != null)
		{
			component.Close();
			MoveToHeroInventory(equipItemInfo);
		}
	}

	void InvenEquipInfo.Listener.OnSell(InvenEquipInfo component)
	{
		EquipItemInfo equipItemInfo = component.EquipItemInfo;
		if (equipItemInfo == null)
		{
			return;
		}
		ProcessSellEquip(equipItemInfo, delegate(bool success)
		{
			if (success)
			{
				component.Close();
			}
		});
	}

	void InvenEquipInfo.Listener.OnBreak(InvenEquipInfo component)
	{
		EquipItemInfo equipItemInfo = component.EquipItemInfo;
		if (equipItemInfo == null)
		{
			return;
		}
		Action OnAccepted = delegate
		{
			List<EquipItemInfo> equipItemInfos = new List<EquipItemInfo> { equipItemInfo };
			BreakEquipItems(this, equipItemInfos, delegate(bool success)
			{
				if (success)
				{
					component.Close();
				}
			}, delegate(InvenList window)
			{
				window.InitEquipmentFilter();
			});
		};
		if ((equipItemInfo.IsUniqueWeapon || equipItemInfo.IsUniqueTreasure) && equipItemInfo.IsBreakable)
		{
			BreakConfirmPopup.Open(equipItemInfo, delegate
			{
				OnAccepted();
			}, null);
		}
		else
		{
			OnAccepted();
		}
	}

	void InvenEquipInfo.Listener.OnKeep(InvenEquipInfo component)
	{
		ProcessSetChest(new List<EquipItemInfo> { component.EquipItemInfo }, delegate(bool success)
		{
			if (success)
			{
				GetInstance<InvenList>().Refresh();
				GetInstance<InvenList>().CloseSubMenu();
				GetInstance<InvenEquipInfoUIContainer>().Close();
			}
		});
	}

	void InvenEquipInfo.Listener.OnBring(InvenEquipInfo component)
	{
		ProcessUnSetChest(new List<EquipItemInfo> { component.EquipItemInfo }, delegate(bool success)
		{
			if (success)
			{
				GetInstance<InvenList>().Refresh();
				GetInstance<InvenList>().CloseSubMenu();
				GetInstance<InvenEquipInfoUIContainer>().Close();
			}
		});
	}

	void InvenEquipInfo.Listener.OpenSoulWeaponLibearationWindow(EquipItemInfo equipItemInfo)
	{
		SceneAutoBehaviour<SoulWeaponManagement>.instance.OpenSoulWeaponLiberationWindow(equipItemInfo, isSelectFirst: false);
	}

	void InvenEquipInfo.Listener.OpenSoulWeaponManageWindow(EquipItemInfo equipItemInfo)
	{
		SceneAutoBehaviour<SoulWeaponManagement>.instance.OpenSoulWeaponManageWindow(equipItemInfo);
	}

	void InvenEquipInfo.Listener.CancelSoulWeapon(EquipItemInfo equipItemInfo)
	{
		SceneAutoBehaviour<SoulWeaponManagement>.instance.OpenBreakSoulWeaponPopup(equipItemInfo);
	}

	void InvenEquipInfo.Listener.ClickHeroSpecialEquipInfo(InvenEquipInfoUIContainer.WindowPosType windowPosType, EquipItemInfo equipItemInfo)
	{
		GetInstance<InvenEquipInfoUIContainer>().Open(windowPosType, equipItemInfo);
	}

	void InvenEquipInfo.Listener.TechnoMagicIdentified(EquipItemInfo equipItemInfo)
	{
		SceneAutoBehaviour<ValanceManagement>.instance.OpenIdentifiedInven(equipItemInfo);
	}

	void CommonInvenUseHeroSelect.Listener.OnSelectHero(CommonInvenUseHeroSelect caller, int heroIndex, int heroLevel, int heroStar, int heroTranscend)
	{
		if (caller == null || caller.HeroSelectItemData == null)
		{
			return;
		}
		if (GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(heroIndex) == null)
		{
			PayShopHeroInfoPopup payShopHeroInfoPopup = GetInstance<PayShopHeroInfoPopup>();
			if (!(payShopHeroInfoPopup != null) || payShopHeroInfoPopup.IsOpen())
			{
				return;
			}
			List<CreatureData> list = new List<CreatureData>();
			CreatureData creatureData = null;
			int[] heroIndices = caller.HeroSelectItemData.HeroIndices;
			foreach (int num in heroIndices)
			{
				CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(num);
				if (num == heroIndex)
				{
					creatureData = data;
				}
				if (GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(data.Index) == null)
				{
					list.Add(data);
				}
			}
			payShopHeroInfoPopup.OpenHeroSelect(creatureData, list, this, heroLevel, heroStar, heroTranscend);
		}
		else
		{
			UIHelper.OpenHeroAwakeChallengeItemPopup(heroIndex, heroStar, heroTranscend, delegate
			{
				RequestUseHeroSelectItem(caller, heroIndex);
			});
		}
	}

	private void RequestUseHeroSelectItem(CommonInvenUseHeroSelect caller, int heroIndex, bool getOtherReward = false)
	{
		if (caller == null || caller.ItemData == null)
		{
			return;
		}
		int index = caller.ItemData.Index;
		NShared.UseHeroSelectItem.Request request = new NShared.UseHeroSelectItem.Request
		{
			ItemIndex = index,
			HeroIndex = heroIndex,
			GetOtherReward = getOtherReward
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.UseHeroSelectItem.Response response)
		{
			if (response.Result != NShared.UseHeroSelectItem.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				caller.Close();
				GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.ItemResults);
				GlobalAutoBehaviour<AccountManager>.instance.ItemUseManager.Apply(response.ItemUseResults);
				GlobalAutoBehaviour<AccountManager>.instance.ApplyAddHero(response.HeroResult);
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.CurrencyResults);
				GlobalAutoBehaviour<AccountManager>.instance.PlayerCurrencyManager.Apply(response.CurrencyResults);
				GlobalAutoBehaviour<AccountManager>.instance.NPCFriendlyManager.Apply(response.FriendlyInfo);
				BaseRewardPopup.OpenPopup(new BaseRewardPopup.PopupOpenData().SetTitle(LocalizationManager.GetString(ItemUse.HEROCHOICE_FRAGMENT_TITLE)).SetItemResultInfos(response.ItemResults).SetCurrencyResultInfos3(response.CurrencyResults), isGlobal: false);
			}
		});
	}

	void CommonInvenUseHeroSelect.Listener.OpenHeroDetailView(ItemData itemData, HeroSelectItemData heroSelectItemData, int selectHeroIndex)
	{
		if (itemData != null && heroSelectItemData != null)
		{
			CachedHeroInfo cachedHeroInfo = new CachedHeroInfo();
			cachedHeroInfo.HeroIndex = selectHeroIndex;
			cachedHeroInfo.Level = (short)heroSelectItemData.HeroLevel;
			cachedHeroInfo.Star = (sbyte)heroSelectItemData.HeroStar;
			cachedHeroInfo.Transcended = (sbyte)heroSelectItemData.HeroTranscend;
			for (int i = 1; i <= 5; i++)
			{
				cachedHeroInfo.SetSkillLevel(i, 1);
				cachedHeroInfo.SetSkillExtend(i, 0);
			}
			cachedHeroInfo.Validate();
			SceneAutoBehaviour<LobbyCampaignDungeon>.instance.OpenHeroDetailView(cachedHeroInfo, new InventoryReturnState(itemData, null, heroSelectItemData.HeroSelectItemType));
		}
	}

	public void RefreshHeroEquipList()
	{
		GetInstance<InvenList>().InitEquipmentFilter();
	}

	public void RestoreHeroSelect(ItemData itemData, CreatureData selectedHeroData, HeroSelectItemType heroSelectItemType)
	{
		if (itemData == null)
		{
			return;
		}
		int index = itemData.Index;
		int itemCount = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.GetItemCount(index);
		AddTask(Vespa_Task_ExtensionMethod.ADDTASK(new GroupTask().ADDTASK(base.TaskFactory.Open(delegate(InvenList window)
		{
			window.Init(this, InvenList.EToggle.Material);
		})).ADDTASK(base.TaskFactory.Open(delegate(InvenItemDesc window)
		{
			ItemData itemData2 = itemData;
			int num = itemCount;
			InventoryManagement listener = this;
			int num2 = num;
			window.Init(itemData2, num2, listener);
		})), delegate
		{
			if (heroSelectItemType == HeroSelectItemType.Attr)
			{
				OpenPopupInvenUseAttrTypeHeroSelect(itemData, selectedHeroData);
			}
			else
			{
				OpenPopupInvenUseHeroSelect(itemData, selectedHeroData);
			}
		}));
	}

	void CommonInvenUseHeroSelect.Listener.OnSelectGetOtherReward(CommonInvenUseHeroSelect caller)
	{
		if (!(caller == null) && caller.ItemData != null && caller.HeroSelectItemData != null)
		{
			UIHelper.OpenHeroAwakeChallengeItemPopup(caller.HeroSelectItemData.AllOwnedRewardIndex, delegate
			{
				RequestUseHeroSelectItem(caller, 0, getOtherReward: true);
			});
		}
	}

	void InvenUseMultipleHeroSelect.Listener.OnGetReward(InvenUseMultipleHeroSelect caller, int selectedHero1, int selectedHero2)
	{
		if (caller == null || caller.MultipleHeroSelectItemData == null)
		{
			MessagePopup.OpenError("Find Smith. He is the Key.");
		}
		else
		{
			RequestUseMultipleHeroSelectItem(caller, selectedHero1, selectedHero2);
		}
	}

	private void RequestUseMultipleHeroSelectItem(InvenUseMultipleHeroSelect caller, int heroIndex1, int heroIndex2)
	{
		if (caller == null || caller.ItemData == null)
		{
			MessagePopup.OpenError("Can't find Item Data");
			return;
		}
		int index = caller.ItemData.Index;
		NShared.UseMultipleHeroSelectItem.Request request = new NShared.UseMultipleHeroSelectItem.Request();
		request.ItemIndex = index;
		request.LeftHeroIndices = ((heroIndex1 == 0) ? null : new int[1] { heroIndex1 });
		request.RightHeroIndices = ((heroIndex2 == 0) ? null : new int[1] { heroIndex2 });
		request.LeftAllOwnedReward = heroIndex1 == 0;
		request.RightAllOwnedReward = heroIndex2 == 0;
		NShared.UseMultipleHeroSelectItem.Request request2 = request;
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request2, delegate(NShared.UseMultipleHeroSelectItem.Response response)
		{
			if (response.Result != NShared.UseMultipleHeroSelectItem.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				caller.Close();
				GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.ItemResults);
				GlobalAutoBehaviour<AccountManager>.instance.ItemUseManager.Apply(response.ItemUseResults);
				GlobalAutoBehaviour<AccountManager>.instance.ApplyAddHero(response.HeroResults);
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.CurrencyResults);
				GlobalAutoBehaviour<AccountManager>.instance.PlayerCurrencyManager.Apply(response.CurrencyResults);
				BaseRewardPopup.OpenPopup(new BaseRewardPopup.PopupOpenData().SetTitle(LocalizationManager.GetString(ItemUse.HEROCHOICE_FRAGMENT_TITLE)).SetItemResultInfos(response.ItemResults).SetCurrencyResultInfos3(response.CurrencyResults), isGlobal: false);
			}
		});
	}

	void InvenUseMultipleHeroSelect.Listener.OnOpenHeroInfoPopup(InvenUseMultipleHeroSelect caller, CreatureData firstShowHero, List<CreatureData> heroGroupList, SkillAttrType recommandedSkillAttrType, HeroUsageType heroUsageType, bool showTotalList)
	{
		if (caller == null || caller.MultipleHeroSelectItemData == null || heroGroupList == null || heroGroupList.Count() < 1)
		{
			return;
		}
		HeroInfoPopup heroInfoPopup = GetInstance<HeroInfoPopup>();
		if (!(heroInfoPopup == null) && !heroInfoPopup.IsOpen())
		{
			CreatureData selectedHero = firstShowHero;
			if (firstShowHero == null)
			{
				int index = heroGroupList.FirstOrDefault().Index;
				selectedHero = GameRoot.Instance.CreatureDataContainer.GetData(index);
			}
			int heroLevel = caller.MultipleHeroSelectItemData.HeroLevel;
			int heroStar = caller.MultipleHeroSelectItemData.HeroStar;
			int heroTranscend = caller.MultipleHeroSelectItemData.HeroTranscend;
			heroInfoPopup.Open(selectedHero, heroGroupList, this, heroLevel, heroStar, heroTranscend, recommandedSkillAttrType, heroUsageType, showTotalList);
		}
	}

	public void RestoreMultipleHeroSelect(ItemData itemData, CreatureData selectedHeroData1, CreatureData selectedHeroData2, CreatureData lastSelectedHero = null, int lastSelectedSlot = 0)
	{
		if (itemData != null)
		{
			OpenPopupInvenUseMultipleHeroSelect(itemData, selectedHeroData1, selectedHeroData2, lastSelectedHero, lastSelectedSlot);
		}
	}

	void InvenUsePetSelect.IListener.OnConfirmPet(int itemIndex, int petIndex, Action onComplete)
	{
		NShared.UsePetSelectItem.Request request = new NShared.UsePetSelectItem.Request
		{
			ItemIndex = itemIndex,
			PetIndex = petIndex
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.UsePetSelectItem.Response response)
		{
			if (response.Result != NShared.UsePetSelectItem.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				Vespa_System_Action_Extension_Method.fire(onComplete);
				GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.ItemResults);
				GlobalAutoBehaviour<AccountManager>.instance.ItemUseManager.Apply(response.ItemUseResults);
				GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.PetAddResultInfo);
				GlobalAutoBehaviour<AccountManager>.instance.PetManager.Apply(response.PetAddResultInfo);
				BaseRewardPopup.OpenPopup(new BaseRewardPopup.PopupOpenData().SetTitle(LocalizationManager.GetStringDirect("PET_GET_REWARD_TEXT")).SetItemResultInfos(response.ItemResults).AddPetAddResultInfo(response.PetAddResultInfo), isGlobal: false);
			}
		});
	}

	void InvenUsePetSelect.IListener.OpenPetDetailView(int petIndex)
	{
		SceneAutoBehaviour<PetManagement>.instance.OpenPetDetailView(petIndex, isShowPetInfo: true);
	}

	void InvenUseHeroGrowth.IListener.OnConfirmHeroGrowth(int itemIndex, int heroIndex, bool allGrowth, int visualLevel, int visualStar, int visualTranscended, Action onComplete)
	{
		string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
		string stringDirect = LocalizationManager.GetStringDirect("CONFIRM_HERO_GROWTH_CHECK", visualStar, visualTranscended, visualLevel);
		MessagePopup3Button.Open(title, stringDirect, null, delegate
		{
			NShared.UseHeroGrowthItem.Request request = new NShared.UseHeroGrowthItem.Request
			{
				ItemIndex = itemIndex,
				HeroIndex = heroIndex,
				GetOtherReward = allGrowth
			};
			SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.UseHeroGrowthItem.Response response)
			{
				Vespa_System_Action_Extension_Method.fire(onComplete);
				if (response.Result != NShared.UseHeroGrowthItem.ResultType.Success)
				{
					MessagePopup.OpenError(response.Result);
				}
				else
				{
					GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.ItemResults);
					GlobalAutoBehaviour<AccountManager>.instance.ItemUseManager.Apply(response.ItemUseResults);
					GlobalAutoBehaviour<AccountManager>.instance.HeroFriendlyManager.Apply(response.HeroFriendlyInfo);
					GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.StaminaResult);
					GlobalAutoBehaviour<AccountManager>.instance.HeroManager.ApplyGrowthHeroInfo(response.HeroResult);
					GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.TeamExpResult);
				}
			});
		}, NShared.NLocalization.Common.COMMON_NO, NShared.NLocalization.Common.COMMON_YES);
	}

	void InvenUseHeroGrowth.IListener.OpenHeroDetailView(int heroIndex, int visualLevel, int visualStar, int isualTranscended)
	{
	}

	private CommonInvenUseHeroSelect GetCommonInvenUseInstance()
	{
		CommonInvenUseHeroSelect result = null;
		if (GetInstance<InvenUseHeroSelect>().IsOpen())
		{
			result = GetInstance<InvenUseHeroSelect>();
		}
		else if (GetInstance<InvenUseAttrTypeHeroSelect>().IsOpen())
		{
			result = GetInstance<InvenUseAttrTypeHeroSelect>();
		}
		return result;
	}

	void PayShopHeroInfoPopup.IListener.OnClickBuy(CreatureData creatureData)
	{
		CommonInvenUseHeroSelect useSelectPopup = GetCommonInvenUseInstance();
		if (useSelectPopup == null)
		{
			return;
		}
		HeroSelectItemData heroSelectItemData = useSelectPopup.HeroSelectItemData;
		if (heroSelectItemData != null)
		{
			string message = LocalizationManager.GetString(ItemUse.BUY_HERO_CONFIRM);
			string confirmLabel = LocalizationManager.GetString(NShared.NLocalization.Common.COMMON_YES);
			string cancelLabel = LocalizationManager.GetString(NShared.NLocalization.Common.COMMON_NO);
			IconConfirmPopup.NewOpen(message, confirmLabel, cancelLabel, delegate
			{
				RequestUseHeroSelectItem(useSelectPopup, creatureData.Index);
				GetInstance<PayShopHeroInfoPopup>().Close();
			}).AddHero(creatureData.Index, heroSelectItemData.HeroStar, heroSelectItemData.HeroTranscend);
		}
	}

	void PayShopHeroInfoPopup.IListener.OnClickViewHeroDetail(CachedHeroInfo heroInfo)
	{
		CommonInvenUseHeroSelect commonInvenUseInstance = GetCommonInvenUseInstance();
		if (!(commonInvenUseInstance == null))
		{
			SceneAutoBehaviour<LobbyCampaignDungeon>.instance.OpenHeroDetailView(heroInfo, new InventoryReturnState(commonInvenUseInstance.ItemData, null, commonInvenUseInstance.HeroSelectItemData.HeroSelectItemType));
		}
	}

	void PayShopHeroInfoPopup.IListener.OnTryBattle(HeroInfo heroInfo)
	{
		CommonInvenUseHeroSelect useSelectPopup = GetCommonInvenUseInstance();
		if (!(useSelectPopup == null))
		{
			AddTask(base.TaskFactory.Branch(base.TaskFactory.Run(() => HeroDetailView.TryBattle(heroInfo)), base.TaskFactory.Run(delegate
			{
				LobbyScene2.ListReturnState.Add(new InventoryReturnState(useSelectPopup.ItemData, heroInfo.CreatureData, useSelectPopup.HeroSelectItemData.HeroSelectItemType));
			})));
		}
	}

	void PayShopHeroInfoPopup.IListener.OnChangedHero(int heroIndex)
	{
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(heroIndex);
		if (data != null)
		{
			CommonInvenUseHeroSelect commonInvenUseInstance = GetCommonInvenUseInstance();
			if (commonInvenUseInstance != null)
			{
				commonInvenUseInstance.SelectHero(data);
			}
		}
	}

	private InvenUseMultipleHeroSelect GetInvenUseMultipleSelectHero()
	{
		InvenUseMultipleHeroSelect result = null;
		if (GetInstance<InvenUseMultipleHeroSelect>().IsOpen())
		{
			result = GetInstance<InvenUseMultipleHeroSelect>();
		}
		return result;
	}

	void HeroInfoPopup.IListener.OnClickBuy(CreatureData heroData)
	{
		InvenUseMultipleHeroSelect invenUseMultipleSelectHero = GetInvenUseMultipleSelectHero();
		if (!(invenUseMultipleSelectHero == null))
		{
			invenUseMultipleSelectHero.SetHeroPortrait(heroData);
		}
	}

	void HeroInfoPopup.IListener.OnClickViewHeroDetail(CachedHeroInfo heroData)
	{
		InvenUseMultipleHeroSelect invenUseMultipleSelectHero = GetInvenUseMultipleSelectHero();
		if (!(invenUseMultipleSelectHero == null) && invenUseMultipleSelectHero.ItemData != null && invenUseMultipleSelectHero.MultipleHeroSelectItemData != null)
		{
			_ = invenUseMultipleSelectHero.MultipleHeroSelectItemData;
			for (int i = 1; i <= 5; i++)
			{
				heroData.SetSkillLevel(i, 1);
				heroData.SetSkillExtend(i, 0);
			}
			heroData.Validate();
			SceneAutoBehaviour<LobbyCampaignDungeon>.instance.OpenHeroDetailView(heroData, new InventoryReturnStateMultipleSelectHero(invenUseMultipleSelectHero.ItemData, invenUseMultipleSelectHero.SelectedHero1, invenUseMultipleSelectHero.SelectedHero2, heroData.CreatureData, invenUseMultipleSelectHero.LastSelectedSlot));
		}
	}

	void HeroInfoPopup.IListener.OnTryBattle(HeroInfo heroInfo)
	{
		InvenUseMultipleHeroSelect useMultipleHeroSelect = GetInvenUseMultipleSelectHero();
		if (!(useMultipleHeroSelect == null))
		{
			AddTask(base.TaskFactory.Branch(base.TaskFactory.Run(() => HeroDetailView.TryBattle(heroInfo)), base.TaskFactory.Run(delegate
			{
				LobbyScene2.ListReturnState.Add(new InventoryReturnStateMultipleSelectHero(useMultipleHeroSelect.ItemData, useMultipleHeroSelect.SelectedHero1, useMultipleHeroSelect.SelectedHero2, heroInfo.CreatureData, useMultipleHeroSelect.LastSelectedSlot));
			})));
		}
	}

	void OptionSellConfirmPopup.Listener.OnClickBreakEquip(IEnumerable<EquipItemInfo> equipItemInfos)
	{
		BreakEquipItems(this, equipItemInfos, null, delegate(InvenList window)
		{
			window.InitEquipmentFilter();
		});
		GetInstance<DismantleConfirmPopup>().Close();
	}

	void OptionSellConfirmPopup.Listener.OnClickSellEquip(IEnumerable<EquipItemInfo> equipItemInfos)
	{
		ProcessSellEquip(equipItemInfos, delegate(bool success)
		{
			if (success)
			{
				GetInstance<InvenList>().CloseSubMenu();
				GetInstance<OptionSellConfirmPopup>().Close();
			}
		});
	}

	bool OptionSellConfirmPopup.Listener.IsEquipItemExist(int option, bool isGrade)
	{
		foreach (EquipItemInfo unequippedEquipItemInfo in GlobalAutoBehaviour<AccountManager>.instance.GetUnequippedEquipItemInfoList())
		{
			if (isGrade)
			{
				if (GameRoot.Instance.ItemDataContainer.GetData(unequippedEquipItemInfo.ItemIndex).Grade == option)
				{
					return true;
				}
			}
			else if (unequippedEquipItemInfo.Star == option)
			{
				return true;
			}
		}
		return false;
	}

	void OptionSellConfirmPopup.Listener.RequestItemInfos(OptionSellConfirmPopup.LocalSettingData data, List<int> gradeList, int minTier, int minStar, ref List<EquipItemInfo> equipItemInfos, bool isSell)
	{
		equipItemInfos.Clear();
		if (data == null)
		{
			return;
		}
		foreach (EquipItemInfo unequippedEquipItemInfo in GlobalAutoBehaviour<AccountManager>.instance.GetUnequippedEquipItemInfoList())
		{
			if (unequippedEquipItemInfo.Locked != 0 || unequippedEquipItemInfo.GetInventoryType() == InventoryType.Chest || unequippedEquipItemInfo.Star > minStar)
			{
				continue;
			}
			ItemData data2 = GameRoot.Instance.ItemDataContainer.GetData(unequippedEquipItemInfo.ItemIndex);
			if (data2 != null && gradeList.Contains(data2.Grade) && (isSell || data2.Breakable) && (!isSell || !data2.NotForSale))
			{
				EquipItemData subData = data2.GetSubData<EquipItemData>();
				if (subData != null && subData.Tier <= minTier && (data.EnhanceStone || subData.PartType != EquipPartType.EnhanceStone) && (data.EnchantedItem || !unequippedEquipItemInfo.EnchantOptionExists()))
				{
					equipItemInfos.Add(unequippedEquipItemInfo);
				}
			}
		}
	}

	void BatchRuneBreakPopup.Listener.RequestItemInfos(List<int> gradeList, List<int> partTypeList, List<ItemInfo> ItemInfos)
	{
		ItemInfos.Clear();
		List<ItemInfo> itemsByType = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.GetItemsByType(ItemType.Rune);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(itemsByType); i++)
		{
			ItemInfo itemInfo = itemsByType[i];
			if (itemInfo.Locked != 0)
			{
				continue;
			}
			ItemData data = GameRoot.Instance.ItemDataContainer.GetData(itemInfo.ItemIndex);
			if (data == null || !data.Breakable || !gradeList.Contains(data.Grade))
			{
				continue;
			}
			RuneItemData data2 = GameRoot.Instance.RuneItemDataContainer.GetData(data.Code);
			if (data2 != null)
			{
				int equipPartTypeByFlag = (int)data2.GetEquipPartTypeByFlag();
				if (partTypeList.Contains(equipPartTypeByFlag))
				{
					ItemInfos.Add(itemInfo);
				}
			}
		}
	}

	void BatchRuneBreakPopup.Listener.OnClickBreakRune(IEnumerable<ItemInfo> runeItemInfos)
	{
		ProcessBreakRune(runeItemInfos, null);
		BatchRuneBreakPopup batchRuneBreakPopup = GetInstance<BatchRuneBreakPopup>();
		if (batchRuneBreakPopup != null)
		{
			batchRuneBreakPopup.Close();
		}
	}

	private void ProcessBreakRune(IEnumerable<ItemInfo> runeItemInfos, IEnumerable<EquipItemInfo> runeEquipItemInfos, Action onBreak = null)
	{
		NShared.BreakRune.Request request = new NShared.BreakRune.Request
		{
			ItemIndices = runeItemInfos?.Select((ItemInfo e) => e.ItemIndex).ToArray(),
			EquipItemSlotIndices = runeEquipItemInfos?.Select((EquipItemInfo e) => e.SlotIndex).ToArray(),
			Counts = runeItemInfos?.Select((ItemInfo e) => e.Count).ToArray()
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.BreakRune.Response response)
		{
			if (response != null)
			{
				if (response.BaseResult != BaseResultType.Success)
				{
					MessagePopup.OpenError(response.BaseResult);
				}
				else if (response.Result != NShared.BreakRune.ResultType.Success)
				{
					MessagePopup.OpenError(response.Result);
				}
				else
				{
					InvenList invenList = GetInstance<InvenList>();
					if (invenList != null)
					{
						invenList.CloseSubMenu();
					}
					if (!__Extension_Method_ICollection__.IsNullOrEmpty(response.RemoveEquipItemSlotIndices))
					{
						int[] removeEquipItemSlotIndices = response.RemoveEquipItemSlotIndices;
						foreach (int slotIndex in removeEquipItemSlotIndices)
						{
							GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Apply(slotIndex, null);
						}
					}
					GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.DecItemResults);
					ResponseBreakItemResult(response.ItemResults);
					if (onBreak != null)
					{
						onBreak();
					}
				}
			}
		});
	}

	public void ResponseBreakItemResult(ItemResultInfo[] itemResultInfos)
	{
		GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(itemResultInfos);
		AddTask(OpenHeroEquipBreakResultPopup(this, itemResultInfos));
	}

	void WeaponUniqueSelect.Listener.OnSelectWeaponUnique(int useItemIndex, int buyItemIndex, Action onSelect)
	{
		ShowItemCountSelectPopup(useItemIndex, GameRoot.Instance.ConstantDataContainer.GetValueInt("EquipSelectItemMaxOpenCount"), delegate(int itemIndex, int itemCount)
		{
			ShowItemCountConfirmPopup(useItemIndex, buyItemIndex, itemCount, delegate
			{
				Vespa_System_Action_Extension_Method.fire(onSelect);
				RequestSelectUniqueWeapon(useItemIndex, buyItemIndex, itemCount);
			});
		});
	}

	private void ShowItemCountSelectPopup(int useItemIndex, int itemCountMax, Action<int, int> onItemCountConfirm)
	{
		string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
		string stringDirect = LocalizationManager.GetStringDirect("EQUIP_SELECT_ITEM_COUNT_MESSAGE");
		List<ItemInfo> list = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.SelectItemList((IItemInfo iteminfo) => iteminfo.ItemIndex == useItemIndex);
		List<Tuple<ItemData, int>> list2 = new List<Tuple<ItemData, int>>();
		foreach (ItemInfo item in list)
		{
			ItemData data = GameRoot.Instance.ItemDataContainer.GetData(item.ItemIndex);
			if (data != null)
			{
				list2.Add(new Tuple<ItemData, int>(data, item.Count));
			}
		}
		SelectItemMessagePopup.OpenWithDisableMaxButton(title, stringDirect, list2, delegate(SelectItemMessagePopup selectItemPopup, int idx, int count)
		{
			Vespa_System_Action_Extension_Method.fire(onItemCountConfirm, idx, count);
		}, itemCountMax);
	}

	private void ShowItemCountConfirmPopup(int useItemIndex, int getItemIndex, int itemCount, Action onConfirm)
	{
		string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
		string stringDirect = LocalizationManager.GetStringDirect("EQUIP_SELECT_ITEM_CONFIRM_MESSAGE", GetItemName(useItemIndex), itemCount, GetItemName(getItemIndex));
		MessagePopup.Open(title, stringDirect, delegate
		{
			Vespa_System_Action_Extension_Method.fire(onConfirm);
		}, null);
	}

	private string GetItemName(int itemIndex)
	{
		ItemData data = GameRoot.Instance.ItemDataContainer.GetData(itemIndex);
		if (data == null)
		{
			return string.Empty;
		}
		return data.Name;
	}

	private void RequestSelectUniqueWeapon(int useItemIndex, int buyItemIndex, int itemCount)
	{
		NShared.UseWeaponUniqueSelectItem.Request request = new NShared.UseWeaponUniqueSelectItem.Request();
		request.ItemIndex = useItemIndex;
		request.WeaponUniqueItemIndex = buyItemIndex;
		request.ItemCount = itemCount;
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.UseWeaponUniqueSelectItem.Response response)
		{
			if (response == null)
			{
				MessagePopup.OpenError(BaseResultType.Fail);
			}
			else if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
			}
			else if (response.Result != NShared.UseWeaponUniqueSelectItem.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Apply(response.EquipItemResults);
				GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.ItemResult);
				ItemData data = GameRoot.Instance.ItemDataContainer.GetData(useItemIndex);
				string empty = string.Empty;
				if (data != null)
				{
					empty = data.Name;
				}
				BaseRewardPopup.OpenPopup(new BaseRewardPopup.PopupOpenData().SetTitle(empty).SetEquipResultInfos(response.EquipItemResults), isGlobal: false);
			}
		});
	}

	void EquipOptionSelect.Listener.OnUseOptionSelectItem(int useItemIndex, int equipItemIndex, int[] optionIndices, int[] extraOptionIndices)
	{
		ShowItemCountSelectPopup(useItemIndex, GameRoot.Instance.ConstantDataContainer.GetValueInt("EquipSelectItemMaxOpenCount"), delegate(int itemIndex, int itemCount)
		{
			ShowItemCountConfirmPopup(useItemIndex, equipItemIndex, itemCount, delegate
			{
				RequestUseOptionSelectItem(useItemIndex, equipItemIndex, optionIndices, itemCount, extraOptionIndices);
			});
		});
	}

	void EquipOptionSelect.Listener.OnClickSelectOption(ItemData itemData, int selectMaxCount, bool isExtraOption)
	{
		EquipOptionSelectPopup equipOptionSelectPopup = GetInstance<EquipOptionSelectPopup>();
		if (equipOptionSelectPopup != null)
		{
			equipOptionSelectPopup.Open(itemData, selectMaxCount, isExtraOption, this);
		}
	}

	void EquipOptionSelectConfirmPopup.Listener.OnUseOptionSelectItem(int useItemIndex, int equipItemIndex, int[] optionIndices)
	{
		RequestUseOptionSelectItem(useItemIndex, equipItemIndex, optionIndices, 1);
	}

	private void RequestUseOptionSelectItem(int useItemIndex, int equipItemIndex, int[] optionIndices, int itemCount, int[] extraOptionIndices = null)
	{
		EquipOptionSelect equipOptionSelect = GetInstance<EquipOptionSelect>();
		if (equipOptionSelect != null)
		{
			equipOptionSelect.Close();
		}
		NShared.UseEquipOptionSelectItem.Request request = new NShared.UseEquipOptionSelectItem.Request
		{
			ItemIndex = useItemIndex,
			ItemCount = itemCount,
			EquipOptionItemIndex = equipItemIndex,
			EquipOptionIndices = optionIndices,
			EquipExtraOptionIndices = extraOptionIndices
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.UseEquipOptionSelectItem.Response response)
		{
			if (response != null)
			{
				if (response.BaseResult != BaseResultType.Success)
				{
					MessagePopup.OpenError(response.BaseResult);
				}
				else if (response.Result != NShared.UseEquipOptionSelectItem.ResultType.Success)
				{
					MessagePopup.OpenError(response.Result);
				}
				else
				{
					GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Apply(response.EquipItemResults);
					GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.ItemResult);
					ItemData data = GameRoot.Instance.ItemDataContainer.GetData(useItemIndex);
					string empty = string.Empty;
					if (data != null)
					{
						empty = data.Name;
					}
					BaseRewardPopup.OpenPopup(new BaseRewardPopup.PopupOpenData().SetTitle(empty).SetEquipResultInfos(response.EquipItemResults), isGlobal: false);
				}
			}
		});
	}

	void EquipOptionSelectPopup.Listener.OnOptionChange(Dictionary<int, int> slotOptionDic, bool isExtraOption)
	{
		if (GetInstance<EquipOptionSelect>().IsOpen())
		{
			if (!isExtraOption)
			{
				GetInstance<EquipOptionSelect>().SetOptionDictionary(new Dictionary<int, int>(slotOptionDic));
				return;
			}
			int[] array = new int[slotOptionDic.Values.Count];
			slotOptionDic.Values.CopyTo(array, 0);
			GetInstance<EquipOptionSelect>().SetExtraOptionArray(array);
		}
	}

	void InvenUseHeroCostumeSelect.Listener.OnSelectCostume(int useItemIndex, int costumeIndex)
	{
		CostumeData CostumeData = GameRoot.Instance.CostumeDataContainer.GetData(costumeIndex);
		if (CostumeData == null)
		{
			VespaDebug.LogError("costume data not found, costumeIndex = " + costumeIndex);
			return;
		}
		NShared.UseCostumeSelectItem.Request request = new NShared.UseCostumeSelectItem.Request
		{
			ItemIndex = useItemIndex,
			CostumeIndex = costumeIndex
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.UseCostumeSelectItem.Response response)
		{
			if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
			}
			else if (response.Result != NShared.UseCostumeSelectItem.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.CostumeManager.Apply(response.CostumeResults);
				GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.ItemResult);
				_ = (object)null;
				_ = 18;
				_ = 1;
				_ = 0;
				Array.Empty<object>();
				GlobalAutoBehaviour<AccountManager>.instance.HeroManager.SetCostumeIndex(CostumeData.HeroIndex, costumeIndex);
				CustomizingHelper.SetHeroCostume(response.HeroCostumeResult);
				string costumeName = CostumeData.CostumeName;
				GetInstance<InvenUseHeroCostumeSelect>().Close();
				CenterMessage.Open(LocalizationManager.GetString(Costume.COSTUME_GET_MSG, costumeName));
			}
		});
	}

	void InvenUseHeroCostumeSelect.Listener.OpenHeroCostumeView(int heroIndex, int costumeIndex)
	{
		SceneAutoBehaviour<CostumeManagement>.instance.OpenCostume(heroIndex, costumeIndex);
	}

	void InvenUseHeroCostumeSelect.Listener.OnSelectGetOtherReward(int useItemIndex)
	{
		NShared.UseCostumeSelectItem.Request request = new NShared.UseCostumeSelectItem.Request
		{
			ItemIndex = useItemIndex,
			GetOtherReward = true
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.UseCostumeSelectItem.Response response)
		{
			if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
			}
			else if (response.Result != NShared.UseCostumeSelectItem.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.ItemResult);
				GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.ItemResults);
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.CurrencyResults);
				GlobalAutoBehaviour<AccountManager>.instance.PlayerCurrencyManager.Apply(response.CurrencyResults);
				GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Apply(response.EquipItemResults);
				GetInstance<InvenUseHeroCostumeSelect>().Close();
				BaseRewardPopup.OpenPopup(new BaseRewardPopup.PopupOpenData().SetTitle(LocalizationManager.GetString(Costume.COSTUME_GEM_MSG_TITLE)).SetItemResultInfos(response.ItemResults).SetEquipResultInfos(response.EquipItemResults)
					.SetCurrencyResultInfos3(response.CurrencyResults), isGlobal: false);
			}
		});
	}

	private void SelectBoosterItem(ItemData itemData, int count, Action onComplete = null)
	{
		if (SprkHonorBuff.TrySelect(itemData, count, onComplete))
		{
			return;
		}
		ItemData itemData2 = itemData;
		int count2 = count;
		Action onComplete2 = onComplete;
		if (itemData2 == null)
		{
			return;
		}
		ItemInfo itemInfo = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Get(itemData2.Index);
		if (itemInfo == null || itemInfo.Count < count2)
		{
			return;
		}
		string remainTimeTextAllDelZero = UIHelper.GetRemainTimeTextAllDelZero(GameRoot.Instance.BoosterItemDataContainer.GetData(itemData2.Code).Duration * count2);
		string message = LocalizationManager.GetString(ItemUse.BOOSTER_ITEM_COMFIRM, itemData2.Name, remainTimeTextAllDelZero);
		string confirmLabel = LocalizationManager.GetString(NShared.NLocalization.Common.COMMON_YES);
		string cancelLabel = LocalizationManager.GetString(NShared.NLocalization.Common.COMMON_NO);
		IconConfirmPopup iconConfirmPopup = IconConfirmPopup.Open(message, confirmLabel, cancelLabel, delegate
		{
			UseBoosterItem(itemData2, count2);
			if (onComplete2 != null)
			{
				onComplete2();
			}
		});
		if (itemData2 != null)
		{
			iconConfirmPopup.AddItem(itemData2.Index, count2);
		}
	}

	void GuildNameChangeMessagePopup.Listener.IsValidGuildName(ItemData itemData, string name, Action<NShared.CheckGuildName.ResultType> onCheck)
	{
		NShared.CheckGuildName.ResultType resultType = SingletonAuto<GuildNameChecker>.instance.Check(name);
		if (resultType != NShared.CheckGuildName.ResultType.Success)
		{
			Vespa_System_Action_Extension_Method.fire(onCheck, resultType);
			return;
		}
		NShared.CheckGuildName.Request request = new NShared.CheckGuildName.Request
		{
			name = name
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.CheckGuildName.Response response)
		{
			if (response == null || response.BaseResult != BaseResultType.Success)
			{
				Vespa_System_Action_Extension_Method.fire(onCheck, NShared.CheckGuildName.ResultType.Fail);
			}
			else
			{
				Vespa_System_Action_Extension_Method.fire(onCheck, response.Result);
			}
		});
	}

	void GuildNameChangeMessagePopup.Listener.UseGuildName(ItemData itemData, string name, Action<NShared.UseGuildNameChangeItem.ResultType> onUse)
	{
		if (itemData == null)
		{
			Vespa_System_Action_Extension_Method.fire(onUse, NShared.UseGuildNameChangeItem.ResultType.Fail);
			return;
		}
		NShared.UseGuildNameChangeItem.Request request = new NShared.UseGuildNameChangeItem.Request
		{
			Name = name,
			ItemIndex = itemData.Index
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.UseGuildNameChangeItem.Response response)
		{
			if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else if (response.Result != NShared.UseGuildNameChangeItem.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				GlobalAutoBehaviour<GuildManager>.instance.SetMyGuildName(name);
				GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.ItemResult);
				Vespa_System_Action_Extension_Method.fire(onUse, response.Result);
				CenterMessage.Open(LocalizationManager.GetString(Item.USE_GUILD_NAME_CHANGE_MSG, name));
			}
		});
	}

	void GuildNameChangeMessagePopup.Listener.OpenConfirmPopup(string title, string content, Action onYes, Action onNo)
	{
		MessagePopup.Open(title, content, onYes, onNo);
	}

	void SkillBookHeroSelectPopup.Listener.SelectHero(CachedHeroInfo heroInfo, int grade)
	{
		if (heroInfo != null)
		{
			CloseInstance<SkillBookHeroSelectPopup>();
			CloseInstance<InvenList>();
			SceneAutoBehaviour<MyHeroManagement>.instance.OpenMyHero(heroInfo, Hero3DViewPanel.EToggle.Skill);
			SceneAutoBehaviour<MyHeroManagement>.instance.OpenHeroSkillDesc(heroInfo, grade);
		}
	}

	public void OpenShortCutCraftWindow(ItemData selectedItemData, int reqSelectedItemCount)
	{
		int emptyCraftSlot = GlobalAutoBehaviour<AccountManager>.instance.CraftSlotManager.GetEmptyCraftSlot();
		if (emptyCraftSlot <= 0)
		{
			CenterMessage.Open(LocalizationManager.GetStringDirect("CRAFT_ITEMCOMPONENT_UNAVAILABLE"));
		}
		else
		{
			OpenCraftItemSelectMessagePopup(emptyCraftSlot, selectedItemData, reqSelectedItemCount);
		}
	}

	private void OpenCraftItemSelectMessagePopup(int index, ItemData selectedItemData = null, int reqSelectedItemCount = 0)
	{
		CraftItemSelectMessagePopup.Open(delegate(int craftIndex, int itemIndex, int itemCount)
		{
			NShared.CraftItem.Request request = new NShared.CraftItem.Request
			{
				SlotIndex = index,
				CraftIndex = craftIndex,
				ItemIndex = itemIndex,
				ItemCount = itemCount
			};
			SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.CraftItem.Response response)
			{
				if (response != null)
				{
					if (response.BaseResult != BaseResultType.Success)
					{
						MessagePopup.OpenError(response.BaseResult);
					}
					else if (response.Result != NShared.CraftItem.ResultType.Success)
					{
						MessagePopup.OpenError(response.Result);
					}
					else
					{
						GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.CurrencyResult);
						GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.ItemResults);
						GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Apply(response.EquipItemResults);
						GlobalAutoBehaviour<AccountManager>.instance.CraftSlotManager.Apply(response.CraftSlotResult);
						if (__Extension_Method_ICollection__.COUNT(response.ItemResults) > 0)
						{
							Array.Sort(response.ItemResults, (ItemResultInfo a, ItemResultInfo b) => b.ItemIndex.CompareTo(a.ItemIndex));
						}
						BaseRewardPopup.OpenPopup(new BaseRewardPopup.PopupOpenData().SetTitle(LocalizationManager.GetString(Inventory.INVENTORY_GET_CRAFT_ITEM)).SetItemResultInfos(response.ItemResults).SetEquipResultInfos(response.EquipItemResults), isGlobal: false);
					}
				}
			});
		}, selectedItemData, reqSelectedItemCount);
	}

	public static void ProcessSetChest(IEnumerable<EquipItemInfo> equipItemInfos, Action<bool> onSet = null)
	{
		if (equipItemInfos == null || equipItemInfos.Count() == 0)
		{
			Vespa_System_Action_Extension_Method.fire(onSet, i_argA: false);
			return;
		}
		string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
		string stringDirect = LocalizationManager.GetStringDirect("RECOLLECT_KEEP_CHEST");
		MessagePopup.Open(title, stringDirect, delegate
		{
			NShared.SetChest.Request request = new NShared.SetChest.Request
			{
				EquipItemSlotIndex = equipItemInfos.Select((EquipItemInfo e) => e.SlotIndex).ToArray()
			};
			SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.SetChest.Response response)
			{
				if (response.Result != NShared.SetChest.ResultType.Success)
				{
					MessagePopup.OpenError(response.Result);
					Vespa_System_Action_Extension_Method.fire(onSet, i_argA: false);
				}
				else
				{
					int[] equipItemSlotIndex = request.EquipItemSlotIndex;
					foreach (int slotIndex in equipItemSlotIndex)
					{
						GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.ApplyInventoryType(slotIndex, InventoryType.Chest);
					}
					CenterMessage.Open(LocalizationManager.GetString(Inventory.INVENTORY_SET_CHEST));
					Vespa_System_Action_Extension_Method.fire(onSet, i_argA: true);
				}
			});
		}, null);
	}

	public static void ProcessUnSetChest(IEnumerable<EquipItemInfo> equipItemInfos, Action<bool> onSet = null)
	{
		if (equipItemInfos == null || equipItemInfos.Count() == 0)
		{
			Vespa_System_Action_Extension_Method.fire(onSet, i_argA: false);
			return;
		}
		string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
		string stringDirect = LocalizationManager.GetStringDirect("RECOLLECT_BRING_ITEM");
		MessagePopup.Open(title, stringDirect, delegate
		{
			NShared.UnsetChest.Request request = new NShared.UnsetChest.Request
			{
				EquipItemSlotIndex = equipItemInfos.Select((EquipItemInfo e) => e.SlotIndex).ToArray()
			};
			SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.UnsetChest.Response response)
			{
				if (response.Result != NShared.UnsetChest.ResultType.Success)
				{
					MessagePopup.OpenError(response.Result);
					Vespa_System_Action_Extension_Method.fire(onSet, i_argA: false);
				}
				else
				{
					int[] equipItemSlotIndex = request.EquipItemSlotIndex;
					foreach (int slotIndex in equipItemSlotIndex)
					{
						GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.ApplyInventoryType(slotIndex, InventoryType.Inventory);
					}
					CenterMessage.Open(LocalizationManager.GetString(Inventory.INVENTORY_UNSET_CHEST));
					Vespa_System_Action_Extension_Method.fire(onSet, i_argA: true);
				}
			});
		}, null);
	}

	void OptionKeepConfirmPopup.Listener.RequestItemInfos(OptionKeepConfirmPopup.LocalSettingData data, List<int> gradeList, int minTier, int minStar, List<EquipItemInfo> equipItemInfos, bool isKeep)
	{
		equipItemInfos.Clear();
		if (data == null)
		{
			return;
		}
		foreach (EquipItemInfo unequippedEquipItemInfo in GlobalAutoBehaviour<AccountManager>.instance.GetUnequippedEquipItemInfoList())
		{
			if (isKeep)
			{
				if (unequippedEquipItemInfo.GetInventoryType() == InventoryType.Chest)
				{
					continue;
				}
			}
			else if (unequippedEquipItemInfo.GetInventoryType() == InventoryType.Inventory)
			{
				continue;
			}
			if ((!data.EnhancedEquip && unequippedEquipItemInfo.Level > 0) || unequippedEquipItemInfo.Star > minStar)
			{
				continue;
			}
			ItemData data2 = GameRoot.Instance.ItemDataContainer.GetData(unequippedEquipItemInfo.ItemIndex);
			if (data2 == null || !gradeList.Contains(data2.Grade))
			{
				continue;
			}
			EquipItemData subData = data2.GetSubData<EquipItemData>();
			if (subData == null || subData.Tier > minTier || (!data.EnhancedEquip && unequippedEquipItemInfo.Level > 0) || (!data.EnhanceStone && subData.PartType == EquipPartType.EnhanceStone))
			{
				continue;
			}
			if (!data.EquipRuneItem)
			{
				int runeSlotCount = unequippedEquipItemInfo.RuneSlotCount;
				bool flag = false;
				for (int i = 1; i <= runeSlotCount; i++)
				{
					if (unequippedEquipItemInfo.GetRuneItemIndex(i) > 0)
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					continue;
				}
			}
			if (data.EnchantedItem || !unequippedEquipItemInfo.EnchantOptionExists())
			{
				equipItemInfos.Add(unequippedEquipItemInfo);
			}
		}
	}

	bool OptionKeepConfirmPopup.Listener.IsEquipItemExist(int option, bool isGrade)
	{
		throw new NotImplementedException();
	}

	void OptionKeepConfirmPopup.Listener.OnClickKeep(IEnumerable<EquipItemInfo> equipItemInfos)
	{
		ProcessSetChest(equipItemInfos, delegate(bool success)
		{
			if (success)
			{
				GetInstance<InvenList>().Refresh();
				GetInstance<InvenList>().CloseSubMenu();
				GetInstance<OptionKeepConfirmPopup>().Close();
			}
		});
	}

	void OptionKeepConfirmPopup.Listener.OnClickBring(IEnumerable<EquipItemInfo> equipItemInfos)
	{
		ProcessUnSetChest(equipItemInfos, delegate(bool success)
		{
			if (success)
			{
				GetInstance<InvenList>().Refresh();
				GetInstance<InvenList>().CloseSubMenu();
				GetInstance<OptionKeepConfirmPopup>().Close();
			}
		});
	}

	void AccessoryHeroSelect.Listener.ShowPreview(int heroIndex, AccessoryCostumeData accessoryCostumeData)
	{
		SceneAutoBehaviour<CostumeManagement>.instance.OpenAccessoryCostume(heroIndex, accessoryCostumeData);
	}

	void AccessoryHeroSelect.Listener.OnConfirm(int itemIndex, int accessoryIndex, int heroIndex, bool isAlternativeReward, Action onComplete)
	{
		NShared.UseAccessorySelectItem.Request request = new NShared.UseAccessorySelectItem.Request();
		request.ItemIndex = itemIndex;
		request.AccessoryCostumeIndex = accessoryIndex;
		request.HeroIndex = heroIndex;
		request.GetOtherReward = isAlternativeReward;
		request.PositionInfo = JsonMarshaler.EncodeJsonString(PositionInfo.Create());
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.UseAccessorySelectItem.Response response)
		{
			if (response.Result != NShared.UseAccessorySelectItem.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
				Vespa_System_Action_Extension_Method.fire(onComplete);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.ItemResult);
				GlobalAutoBehaviour<AccountManager>.instance.AccessoryCostumeManager.Apply(response.PlayerAccessoryCostumeInfo);
				CustomizingHelper.SetHeroCostume(response.HeroCostumeResultInfo);
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.CurrencyResults);
				GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.ItemResults);
				GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Apply(response.EquipItemResults);
				if (!isAlternativeReward)
				{
					AccessoryCostumeData data = GameRoot.Instance.AccessoryCostumeDataContainer.GetData(accessoryIndex);
					if (data != null)
					{
						CenterMessage.Open(LocalizationManager.GetString(Costume.COSTUME_GET_MSG, data.Name));
					}
				}
				else
				{
					BaseRewardPopup.OpenPopup(new BaseRewardPopup.PopupOpenData().SetTitle(LocalizationManager.GetString(Costume.COSTUME_GEM_MSG_TITLE)).SetCurrencyResultInfos3(response.CurrencyResults).SetItemResultInfos(response.ItemResults)
						.SetEquipResultInfos(response.EquipItemResults), isGlobal: false);
				}
				Vespa_System_Action_Extension_Method.fire(onComplete);
			}
		});
	}

	void HeroEquipInfo.Listener.OnClose(EquipItemInfo equipItemInfo)
	{
	}

	public void CloseInvenEquipInfoUIContainer()
	{
		InvenEquipInfoUIContainer invenEquipInfoUIContainer = GetInstance<InvenEquipInfoUIContainer>();
		if (invenEquipInfoUIContainer != null)
		{
			invenEquipInfoUIContainer.Close();
		}
	}

	void SoulStoneSelect.IListener.OnSelectSoulStone(int useItemIndex, int selectItemIndex, Action onSelect)
	{
		ShowItemCountSelectPopup(useItemIndex, 0, delegate(int itemIndex, int itemCount)
		{
			ShowItemCountConfirmPopup(useItemIndex, selectItemIndex, itemCount, delegate
			{
				Vespa_System_Action_Extension_Method.fire(onSelect);
				RequestSelectSoulStone(useItemIndex, selectItemIndex, itemCount);
			});
		});
	}

	private void RequestSelectSoulStone(int useItemIndex, int selectItemIndex, int itemCount)
	{
		NShared.UsePackageSelectItem.Request request = new NShared.UsePackageSelectItem.Request
		{
			ItemIndex = useItemIndex,
			ItemCount = itemCount,
			SelectItemIndex = selectItemIndex
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.UsePackageSelectItem.Response response)
		{
			if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else if (response.Result != NShared.UsePackageSelectItem.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.ItemResult);
				GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.RewardItemResult);
				ItemData data = GameRoot.Instance.ItemDataContainer.GetData(response.ItemResult.ItemIndex);
				if (data != null)
				{
					BaseRewardPopup.PopupOpenData popupOpenData = new BaseRewardPopup.PopupOpenData();
					popupOpenData.SetItemResultInfos(new ItemResultInfo[1] { response.RewardItemResult }).SetTitle(data.Name);
					BaseRewardPopup.OpenPopup(popupOpenData, isGlobal: false);
				}
			}
		});
	}

	internal void OpenCraftWindow(string itemCode)
	{
		ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(itemCode);
		OpenCraftItemSelectMessagePopup(GlobalAutoBehaviour<AccountManager>.instance.CraftSlotManager.GetEmptyCraftSlot(), dataByCode);
	}

	void EquipTransitionTicket.IListener.OpenTransitionConfirmPopup(EquipTransitionTicket.TransitionItemDataInfo transItemDataInfo, EquipTransitionType transitionType)
	{
		if (transItemDataInfo != null)
		{
			GetInstance<EquipTransitionTicketSelectConfirmPopup>().Open(this, transItemDataInfo, transitionType);
		}
	}

	void EquipTransitionTicketSelectConfirmPopup.IListener.OnClickTransition(int itemIndex, int giveEquipItemSlotIndex, int getEquipItemSlotIndex, EquipTransitionType transitionType)
	{
		switch (transitionType)
		{
		case EquipTransitionType.Awaken:
			RequestAwakenTransition(itemIndex, giveEquipItemSlotIndex, getEquipItemSlotIndex);
			break;
		case EquipTransitionType.SoulWeapon:
			RequestSoulWeaponTransition(itemIndex, giveEquipItemSlotIndex, getEquipItemSlotIndex);
			break;
		}
	}

	private void RequestAwakenTransition(int itemIndex, int giveEquipItemSlotIndex, int getEquipItemSlotIndex)
	{
		NShared.AwakenTransition.Request request = new NShared.AwakenTransition.Request
		{
			ItemIndex = itemIndex,
			SourceEquipItemSlotIndex = giveEquipItemSlotIndex,
			TargetEquipItemSlotIndex = getEquipItemSlotIndex
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.AwakenTransition.Response response)
		{
			bool flag = true;
			if (response == null)
			{
				flag = false;
			}
			else if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
				flag = false;
			}
			else if (response.Result != NShared.AwakenTransition.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
				flag = false;
			}
			if (!flag)
			{
				GetInstance<EquipTransitionTicketSelectConfirmPopup>().SetColliderEnable(isEnable: true);
			}
			else
			{
				GetInstance<EquipTransitionTicketSelectConfirmPopup>().SetTransitionEffectPlay(delegate
				{
					GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Apply(response.SourceEquipItemResult);
					GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Apply(response.TargetEquipItemResult);
					GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.ItemResult);
					GetInstance<EquipTransitionTicket>().Close();
					GetInstance<EquipTransitionTicketSelectConfirmPopup>().SetTransitionComplete(response.SourceEquipItemResult.SlotIndex, response.TargetEquipItemResult.SlotIndex);
				});
			}
		});
	}

	private void RequestSoulWeaponTransition(int itemIndex, int giveEquipItemSlotIndex, int getEquipItemSlotIndex)
	{
		NShared.SoulWeaponTransitionTicket.Request request = new NShared.SoulWeaponTransitionTicket.Request
		{
			ItemIndex = itemIndex,
			SourceEquipItemSlotIndex = giveEquipItemSlotIndex,
			TargetEquipItemSlotIndex = getEquipItemSlotIndex
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.SoulWeaponTransitionTicket.Response response)
		{
			bool flag = true;
			if (response == null)
			{
				flag = false;
			}
			else if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
				flag = false;
			}
			else if (response.Result != NShared.SoulWeaponTransitionTicket.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
				flag = false;
			}
			if (!flag)
			{
				GetInstance<EquipTransitionTicketSelectConfirmPopup>().SetColliderEnable(isEnable: true);
			}
			else
			{
				EquipTransitionTicketSelectConfirmPopup equipTransitionTicketSelectConfirmPopup = GetInstance<EquipTransitionTicketSelectConfirmPopup>();
				if (!(equipTransitionTicketSelectConfirmPopup == null) && equipTransitionTicketSelectConfirmPopup.IsOpen())
				{
					GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.ItemResult);
					GlobalAutoBehaviour<AccountManager>.instance.SoulWeaponManager.Apply(giveEquipItemSlotIndex, response.SourceSoulWeaponInfo);
					GlobalAutoBehaviour<AccountManager>.instance.SoulWeaponManager.Apply(getEquipItemSlotIndex, response.TargetSoulWeaponInfo);
					equipTransitionTicketSelectConfirmPopup.SetTransitionEffectPlay(delegate
					{
						GetInstance<EquipTransitionTicket>().Close();
						GetInstance<EquipTransitionTicketSelectConfirmPopup>().SetTransitionComplete(giveEquipItemSlotIndex, getEquipItemSlotIndex);
					});
				}
			}
		});
	}

	void SoulWeaponAbilityTicket.IListener.OpenConfirmPopup(SoulWeaponAbilityTicket.SoulWeaponAbilityItemDataInfo soulWeaponAbilityItemDataInfo, SoulWeaponAbilityTicketData soulWeaponAbilityTicketData)
	{
		if (soulWeaponAbilityItemDataInfo != null)
		{
			GetInstance<SoulWeaponAbilityTicketConfirmPopup>().Open(this, soulWeaponAbilityItemDataInfo, soulWeaponAbilityTicketData);
		}
	}

	void SoulWeaponAbilityTicketConfirmPopup.IListener.OnClickConfirmPopup(int itemIndex, int targetEquipItemSlotIndex)
	{
		NShared.SoulWeaponAbilityTicket.Request request = new NShared.SoulWeaponAbilityTicket.Request
		{
			ItemIndex = itemIndex,
			TargetEquipItemSlotIndex = targetEquipItemSlotIndex
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.SoulWeaponAbilityTicket.Response response)
		{
			bool flag = true;
			if (response == null)
			{
				flag = false;
			}
			else if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
				flag = false;
			}
			else if (response.Result != NShared.SoulWeaponAbilityTicket.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
				flag = false;
			}
			if (!flag)
			{
				GetInstance<SoulWeaponAbilityTicketConfirmPopup>().SetColliderEnable(isEnable: true);
			}
			else
			{
				SoulWeaponAbilityTicketConfirmPopup soulWeaponAbilityTicketConfirmPopup = GetInstance<SoulWeaponAbilityTicketConfirmPopup>();
				if (!(soulWeaponAbilityTicketConfirmPopup == null) && soulWeaponAbilityTicketConfirmPopup.IsOpen())
				{
					GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.ItemResult);
					GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.ItemRefundsResults);
					GlobalAutoBehaviour<AccountManager>.instance.SoulWeaponManager.Apply(targetEquipItemSlotIndex, response.TargetSoulWeaponInfo);
					soulWeaponAbilityTicketConfirmPopup.SetSoulWeaponAbilityEffectPlay(delegate
					{
						GetInstance<SoulWeaponAbilityTicket>().Close();
						GetInstance<SoulWeaponAbilityTicketConfirmPopup>().SetUseItemComplete(targetEquipItemSlotIndex);
					}, delegate
					{
						if (response.ItemRefundsResults != null)
						{
							BaseRewardPopup.PopupOpenData popupOpenData = new BaseRewardPopup.PopupOpenData();
							popupOpenData.SetTitle(LocalizationManager.GetStringDirect("COMMON_REWARD"));
							popupOpenData.SetItemResultInfos(response.ItemRefundsResults);
							BaseRewardPopup.OpenPopup(popupOpenData, isGlobal: false);
						}
					});
				}
			}
		});
	}
}
