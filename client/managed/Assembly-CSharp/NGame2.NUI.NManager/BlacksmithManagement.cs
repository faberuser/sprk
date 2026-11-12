using NGame2.NAccount;
using NGame2.NDropBonusEvent;
using NGame2.NEventTrigger;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NBattle;
using NGame2.NUI3.NEquipLimitBreak;
using NGame2.NUtil;
using NShared;
using NShared.AwakenEquip;
using NShared.ConfirmEquipEnchantOption;
using NShared.EnchantEquip;
using NShared.MaxUpgradeEquipOption;
using NShared.NEvent;
using NShared.NLocalization;
using NShared.RenewConfirmEquipOption;
using NShared.RenewEquipOption;
using NShared.UpgradeEquipOption;
using NShared.UpgradeEquipTier;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using NVespa.NTask;
using NVespa.NTask.NRuntime;
using UnityEngine;

namespace NGame2.NUI.NManager;

public class BlacksmithManagement : BaseManagement<BlacksmithManagement>, HeroEquipAwaken.Listener, NGame2.NUI.NWindow.EquipOptionUpgrade.Listener, NGame2.NUI.NWindow.EquipOptionRenew.Listener, EquipOptionRenewScene.Listener, NGame2.NUI.NWindow.EquipTierUpgrade.Listener, EquipEnchantOption.Listener, EquipEnchantOptionScene.Listener, UniqueWeaponLimitBreakWindow.Listener
{
	protected override void Awake()
	{
		base.Awake();
		AddInstance<HeroEquipAwaken>("Resources/Prefab2/Window/Equip/HeroEquipAwaken");
		AddInstance<HeroEquipUpgradeResultPopup>("Resources/Prefab2/Window/Hero/HeroEquipUpgradeResultPopup");
		AddInstance<NGame2.NUI.NWindow.EquipOptionUpgrade>("Resources/Prefab2/Window/Equip/EquipOptionUpgrade");
		AddInstance<NGame2.NUI.NWindow.EquipOptionRenew>("Resources/Prefab2/Window/Equip/EquipOptionRenew");
		AddInstance<EquipAwakenScene>("Resources/Prefab2/Window/FullScene/EquipAwakenScene");
		AddInstance<EquipOptionRenewScene>("Resources/Prefab2/Window/FullScene/EquipOptionRenewScene");
		AddInstance<NGame2.NUI.NWindow.EquipTierUpgrade>("Resources/Prefab2/Window/Equip/EquipTierUpgrade");
		AddInstance<EquipEnchantOption>("Resources/Prefab2/Window/Equip/EquipEnchantOption");
		AddInstance<EquipEnchantOptionScene>("Resources/Prefab2/Window/FullScene/EquipEnchantOptionScene");
		AddInstance<UniqueWeaponLimitBreakWindow>("Resources/Prefab3/EquipLimitBreak/UniqueWeaponLimitBreakWindow");
		AddInstance<LimitBreakResultScene>("Resources/Prefab3/EquipLimitBreak/LimitBreakResultScene");
	}

	public void OpenEquipAwaken(EquipItemInfo itemInfo = null)
	{
		AddTask(OpenEquipAwakenAsync());
		if (itemInfo != null)
		{
			AddTask(base.TaskFactory.Run(delegate(HeroEquipAwaken window)
			{
				window.SelectAwakenItem(itemInfo);
			}));
		}
	}

	private ITask OpenEquipAwakenAsync()
	{
		return base.TaskFactory.Open(delegate(HeroEquipAwaken window)
		{
			window.Open(this);
		});
	}

	public void OpenEquipLimitBreak(EquipItemInfo itemInfo = null)
	{
		AddTask(base.TaskFactory.Open(delegate(UniqueWeaponLimitBreakWindow window)
		{
			window.InitWindow(this, itemInfo);
		}));
	}

	public void OpenEquipOptionUpgrade(EquipItemInfo itemInfo = null)
	{
		AddTask(OpenEquipOptionUpgradeAsync());
		if (itemInfo != null)
		{
			AddTask(base.TaskFactory.Run(delegate(NGame2.NUI.NWindow.EquipOptionUpgrade window)
			{
				window.SelectItemIconComponent(itemInfo);
			}));
		}
	}

	private ITask OpenEquipOptionUpgradeAsync()
	{
		return base.TaskFactory.Open(delegate(NGame2.NUI.NWindow.EquipOptionUpgrade window)
		{
			window.Init(this);
		});
	}

	public void OpenEquipOptionRenew(EquipItemInfo itemInfo = null)
	{
		OpenRenewEquip triggerEvent = new OpenRenewEquip();
		if (SingletonAuto<EventTriggerManager>.instance.RaiseEvent(triggerEvent))
		{
			SceneAutoBehaviour<BlacksmithManagement>.instance.GetInstance<NGame2.NUI.NWindow.EquipOptionRenew>().Tutorial_InitUi();
		}
		AddTask(OpenEquipOptionRenewAsync());
		if (itemInfo != null)
		{
			AddTask(base.TaskFactory.Run(delegate(NGame2.NUI.NWindow.EquipOptionRenew window)
			{
				window.SelectItemIconComponent(itemInfo);
			}));
		}
	}

	private ITask OpenEquipOptionRenewAsync()
	{
		return base.TaskFactory.Open(delegate(NGame2.NUI.NWindow.EquipOptionRenew window)
		{
			window.Init(this);
		});
	}

	public void OpenEquipTierUpgrade()
	{
		AddTask(OpenEquipTierUpgradeAsync());
	}

	private ITask OpenEquipTierUpgradeAsync()
	{
		return base.TaskFactory.Open(delegate(NGame2.NUI.NWindow.EquipTierUpgrade window)
		{
			window.Init(this);
		});
	}

	public void OpenEquipEnchantOption(EquipItemInfo itemInfo = null)
	{
		AddTask(OpenEquipEnchantOptionAsync());
		if (itemInfo != null)
		{
			AddTask(base.TaskFactory.Run(delegate(EquipEnchantOption window)
			{
				window.SelectItemIconComponent(itemInfo);
			}));
		}
	}

	private ITask OpenEquipEnchantOptionAsync()
	{
		return base.TaskFactory.Open(delegate(EquipEnchantOption window)
		{
			window.Init(this);
		});
	}

	bool HeroEquipAwaken.Listener.OnCloseRequest(HeroEquipAwaken caller)
	{
		return false;
	}

	void HeroEquipAwaken.Listener.OnAwaken(HeroEquipAwaken caller)
	{
		int[] materialEquipSlotIndexArray = caller.GetMaterialEquipSlotIndexArray();
		int[] materialPublicAwakenItemIndexArray = caller.GetMaterialPublicAwakenItemIndexArray();
		if ((__Extension_Method_ICollection__.IsNullOrEmpty(materialEquipSlotIndexArray) && __Extension_Method_ICollection__.IsNullOrEmpty(materialPublicAwakenItemIndexArray)) || caller.TargetItem.EquipItemInfo == null)
		{
			return;
		}
		int equipItemSlotIndex = caller.TargetItem.EquipItemInfo.SlotIndex;
		EquipItemInfo equipItemInfo = GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Get(equipItemSlotIndex);
		if (equipItemInfo == null)
		{
			VespaDebug.LogError("EquipItemInfo not found : slotIndex=" + equipItemSlotIndex);
			return;
		}
		_ = equipItemInfo.Level;
		_ = equipItemInfo.Star;
		new WebServiceTask<NShared.AwakenEquip.Request, NShared.AwakenEquip.Response>(new NShared.AwakenEquip.Request
		{
			EquipItemSlotIndex = equipItemSlotIndex,
			MaterialSlotIndices = materialEquipSlotIndexArray,
			MaterialItemInfors = materialPublicAwakenItemIndexArray
		}, delegate(NShared.AwakenEquip.Request request, NShared.AwakenEquip.Response response)
		{
			if (response.Result != NShared.AwakenEquip.ResultType.Success)
			{
				return base.TaskFactory.Run(delegate
				{
					MessagePopup.OpenError(response.Result);
				});
			}
			GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.CurrencyResult);
			GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.ItemResults);
			if (response.RemovedEquipItemSlotIndices != null)
			{
				int[] removedEquipItemSlotIndices = response.RemovedEquipItemSlotIndices;
				foreach (int slotIndex in removedEquipItemSlotIndices)
				{
					GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Apply(slotIndex, null);
				}
			}
			GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.RemovedItemResultInfos);
			GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Apply(equipItemSlotIndex, response.ResultEquipItem);
			IsOpened<HeroEquipAwaken>();
			HeroEquipAwaken heroEquipAwaken = GetInstance<HeroEquipAwaken>();
			if (heroEquipAwaken != null)
			{
				GetInstance<EquipAwakenScene>().Open(response.ResultEquipItem, heroEquipAwaken, EquipAwakenScene.CinemaType.Equip_Awaken, response.Success, null);
			}
			ItemData data = GameRoot.Instance.ItemDataContainer.GetData(response.ResultEquipItem.ItemIndex);
			if (response.Success && data != null)
			{
				EquipItemData subData = data.GetSubData<EquipItemData>();
				if (subData != null)
				{
					if (subData.SubType == EquipItemSubType.SpecialWeapon)
					{
						_ = (object)null;
						_ = 22;
						_ = 1;
						_ = 0;
						(new object[1])[0] = response.ResultEquipItem.Star;
					}
					if (subData.SubType.ToString().Contains("SpecialTreasure"))
					{
						_ = (object)null;
						_ = 23;
						_ = 1;
						_ = 0;
						(new object[1])[0] = response.ResultEquipItem.Star;
					}
					if (subData.PartType == EquipPartType.Artifact)
					{
						_ = (object)null;
						_ = 24;
						_ = 1;
						_ = 0;
						(new object[1])[0] = response.ResultEquipItem.Star;
					}
				}
			}
			return (ITask)null;
		}).Commit();
	}

	void HeroEquipAwaken.Listener.OnClickMaterial(HeroEquipAwaken caller, ItemIconComponent itemIconComponent)
	{
	}

	void NGame2.NUI.NWindow.EquipOptionUpgrade.Listener.OnUpgrade(NGame2.NUI.NWindow.EquipOptionUpgrade equipOptionUpgrade)
	{
		if (equipOptionUpgrade == null)
		{
			return;
		}
		EquipItemInfo equipItemInfo = equipOptionUpgrade.SelectedEquipItemInfo;
		if (equipItemInfo == null)
		{
			return;
		}
		int materialItemIndex = equipOptionUpgrade.GetMaterialItemIndex();
		long materialItemReqCount = equipOptionUpgrade.GetMaterialItemReqCount();
		int itemCount = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.GetItemCount(materialItemIndex);
		int num = itemCount;
		long num2 = materialItemReqCount;
		int num3 = num;
		if (num3 < num2)
		{
			VespaDebug.LogError("Not enough material. req=" + materialItemReqCount + " owned=" + itemCount);
			return;
		}
		NShared.UpgradeEquipOption.Request request = new NShared.UpgradeEquipOption.Request();
		request.EquipItemSlotIndex = equipItemInfo.SlotIndex;
		request.MaterialItemIndex = materialItemIndex;
		request.MaterialItemCount = materialItemReqCount;
		AddTask(new WebServiceTask<NShared.UpgradeEquipOption.Request, NShared.UpgradeEquipOption.Response>(request, delegate(NShared.UpgradeEquipOption.Request req, NShared.UpgradeEquipOption.Response res)
		{
			if (res.Result != NShared.UpgradeEquipOption.ResultType.Success)
			{
				return base.TaskFactory.Run(delegate
				{
					MessagePopup.OpenError(res.Result);
				});
			}
			EquipItemInfo resultEquipItemInfo = res.ResultEquipItem;
			return (resultEquipItemInfo == null) ? base.TaskFactory.Run(delegate
			{
				MessagePopup.OpenError(NShared.UpgradeEquipOption.ResultType.Fail);
			}) : new SequenceTask().ADDTASK(base.TaskFactory.Run(delegate
			{
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(res.CurrencyResult);
			})).ADDTASK(base.TaskFactory.Run(delegate
			{
				GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(res.ItemResult);
			})).ADDTASK(base.TaskFactory.Run(delegate
			{
				GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Apply(equipItemInfo.SlotIndex, resultEquipItemInfo);
			}))
				.ADDTASK(base.TaskFactory.Run((NGame2.NUI.NWindow.EquipOptionUpgrade window) => window.CreateChangedOptionStatAnimation()))
				.ADDTASK(base.TaskFactory.Branch(base.TaskFactory.Run(() => resultEquipItemInfo.IsMaxOptionStep()), base.TaskFactory.Sequence(base.TaskFactory.Open(delegate(HeroEquipUpgradeResultPopup window)
				{
					window.Init(resultEquipItemInfo, equipOptionUpgrade.GetUpgradeText());
				}), base.TaskFactory.Run(delegate(NGame2.NUI.NWindow.EquipOptionUpgrade window)
				{
					window.DeselectTargetEquipItem();
				}), base.TaskFactory.Run(delegate(NGame2.NUI.NWindow.EquipOptionUpgrade window)
				{
					window.InventoryEquipItemGrid.RepositionGrid();
				}))))
				.WrapperTouchControl()
				.DisableTouch();
		}));
	}

	void NGame2.NUI.NWindow.EquipOptionUpgrade.Listener.OnMaxUpgrade(NGame2.NUI.NWindow.EquipOptionUpgrade equipOptionUpgrade)
	{
		if (equipOptionUpgrade == null)
		{
			return;
		}
		EquipItemInfo equipItemInfo = equipOptionUpgrade.SelectedEquipItemInfo;
		if (equipItemInfo == null)
		{
			return;
		}
		int materialItemIndex = equipOptionUpgrade.GetMaterialItemIndex();
		long materialItemReqCount = equipOptionUpgrade.GetMaterialItemReqCount();
		NShared.MaxUpgradeEquipOption.Request request = new NShared.MaxUpgradeEquipOption.Request();
		request.EquipItemSlotIndex = equipItemInfo.SlotIndex;
		request.MaterialItemIndex = materialItemIndex;
		request.MaterialItemCount = materialItemReqCount;
		AddTask(new WebServiceTask<NShared.MaxUpgradeEquipOption.Request, NShared.MaxUpgradeEquipOption.Response>(request, delegate(NShared.MaxUpgradeEquipOption.Request req, NShared.MaxUpgradeEquipOption.Response res)
		{
			if (res.Result != NShared.MaxUpgradeEquipOption.ResultType.Success)
			{
				return base.TaskFactory.Run(delegate
				{
					MessagePopup.OpenError(res.Result);
				});
			}
			EquipItemInfo resultEquipItemInfo = res.ResultEquipItem;
			return (resultEquipItemInfo == null) ? base.TaskFactory.Run(delegate
			{
				MessagePopup.OpenError(NShared.MaxUpgradeEquipOption.ResultType.Fail);
			}) : new SequenceTask().ADDTASK(base.TaskFactory.Run(delegate
			{
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(res.CurrencyResult);
			})).ADDTASK(base.TaskFactory.Run(delegate
			{
				GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(res.ItemResult);
			})).ADDTASK(base.TaskFactory.Run(delegate
			{
				GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Apply(equipItemInfo.SlotIndex, resultEquipItemInfo);
			}))
				.ADDTASK(base.TaskFactory.Run((NGame2.NUI.NWindow.EquipOptionUpgrade window) => window.CreateChangedOptionStatAnimation()))
				.ADDTASK(base.TaskFactory.Branch(base.TaskFactory.Run(() => resultEquipItemInfo.IsMaxOptionStep()), base.TaskFactory.Sequence(base.TaskFactory.Open(delegate(HeroEquipUpgradeResultPopup window)
				{
					window.Init(resultEquipItemInfo, equipOptionUpgrade.GetUpgradeText());
				}), base.TaskFactory.Run(delegate(NGame2.NUI.NWindow.EquipOptionUpgrade window)
				{
					window.DeselectTargetEquipItem();
				}), base.TaskFactory.Run(delegate(NGame2.NUI.NWindow.EquipOptionUpgrade window)
				{
					window.InventoryEquipItemGrid.RepositionGrid();
				}))))
				.WrapperTouchControl()
				.DisableTouch();
		}));
	}

	void NGame2.NUI.NWindow.EquipOptionRenew.Listener.RenewEquipOption(NGame2.NUI.NWindow.EquipOptionRenew window, int equipItemSlotIndex, int optionSlotIndex)
	{
		EquipItemInfo equipItemInfo = GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Get(equipItemSlotIndex);
		if (equipItemInfo == null)
		{
			VespaDebug.LogError("EquipItem not found : slotIndex=" + equipItemSlotIndex);
			return;
		}
		ItemData<EquipItemData> data = GameRoot.Instance.ItemDataContainer.GetData<EquipItemData>(equipItemInfo.ItemIndex);
		if (data == null)
		{
			return;
		}
		EquipItemDetailData data2 = GameRoot.Instance.EquipItemDetailDataContainer.GetData(data.SubData.DetailIndex);
		if (data2 == null)
		{
			return;
		}
		if (equipItemInfo.GetRenewedOptionCount() == 0 && data2.RenewableOptionCount == 1)
		{
			string title = LocalizationManager.GetString(NShared.NLocalization.EquipOptionRenew.CONFIRM_FIRST_RENEW_OPTION_TITLE);
			string message = LocalizationManager.GetString(NShared.NLocalization.EquipOptionRenew.CONFIRM_FIRST_RENEW_OPTION_MESSAGE);
			MessagePopup.Open(title, message, delegate
			{
				RenewEquipOptionInternal(window, equipItemSlotIndex, optionSlotIndex);
			}, null);
		}
		else
		{
			RenewEquipOptionInternal(window, equipItemSlotIndex, optionSlotIndex);
		}
	}

	private void RenewEquipOptionInternal(NGame2.NUI.NWindow.EquipOptionRenew window, int equipItemSlotIndex, int optionSlotIndex)
	{
		EquipItemInfo equipItemInfo = GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Get(equipItemSlotIndex);
		if (equipItemInfo == null)
		{
			VespaDebug.LogError("EquipItem not exist = slotIndex : " + equipItemSlotIndex);
			return;
		}
		ItemData<EquipItemData> data = GameRoot.Instance.ItemDataContainer.GetData<EquipItemData>(equipItemInfo.ItemIndex);
		if (data == null)
		{
			VespaDebug.LogError("equipItemData not exist = itemIdex : " + equipItemInfo.ItemIndex);
			return;
		}
		EquipItemDetailData data2 = GameRoot.Instance.EquipItemDetailDataContainer.GetData(data.SubData.DetailIndex);
		if (data2 == null)
		{
			VespaDebug.LogError("equipItemData not exist = itemIdex : " + equipItemInfo.ItemIndex);
			return;
		}
		if (data2.RenewTicketItemIndex == 0)
		{
			RequestRenewEquipOptionWithCurrency(equipItemSlotIndex, optionSlotIndex);
			return;
		}
		ItemInfo itemInfo = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Get(data2.RenewTicketItemIndex);
		if (itemInfo == null || (itemInfo != null && itemInfo.Count == 0))
		{
			RequestRenewEquipOptionWithCurrency(equipItemSlotIndex, optionSlotIndex);
			return;
		}
		int couponItemIndex = itemInfo.ItemIndex;
		LocalizationManager.GetString(NShared.NLocalization.EquipOptionRenew.ASK_RENEW_OPTION_TITLE);
		string message = LocalizationManager.GetString(NShared.NLocalization.EquipOptionRenew.ASK_RENEW_OPTION_WITH_COUPON_MESSAGE);
		string confirmLabel = LocalizationManager.GetString(NShared.NLocalization.Common.USE);
		string cancelLabel = LocalizationManager.GetString(NShared.NLocalization.Common.COMMON_CANCEL);
		IconConfirmPopup.Open(message, confirmLabel, cancelLabel, delegate
		{
			RequestRenewEquipOptionInternal(equipItemSlotIndex, optionSlotIndex, couponItemIndex);
		}, delegate
		{
			RequestRenewEquipOptionWithCurrency(equipItemSlotIndex, optionSlotIndex);
		}).AddItem(couponItemIndex, 1);
	}

	private void RequestRenewEquipOptionWithCurrency(int equipItemSlotIndex, int optionSlotIndex)
	{
		EquipItemInfo equipItemInfo = GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Get(equipItemSlotIndex);
		if (equipItemInfo == null)
		{
			VespaDebug.LogError("EquipItem not found : slotIndex=" + equipItemSlotIndex);
			return;
		}
		ItemData data = GameRoot.Instance.ItemDataContainer.GetData(equipItemInfo.ItemIndex);
		if (data == null)
		{
			VespaDebug.LogError("equipItemData not found : itemIndex=" + equipItemInfo.ItemIndex);
			return;
		}
		EquipItemDetailData data2 = GameRoot.Instance.EquipItemDetailDataContainer.GetData(data.GetSubData<EquipItemData>().DetailIndex);
		if (data2 == null)
		{
			VespaDebug.LogError("detailData not found : detailData =" + data.GetSubData<EquipItemData>().DetailIndex);
			return;
		}
		int[] renewPrice = data2.RenewPrice;
		if (__Extension_Method_ICollection__.COUNT(renewPrice) == 0)
		{
			VespaDebug.LogError("EquipOptionRenewPrice is empty");
			return;
		}
		PricedMessagePopup.EPriceType priceType = PricedMessagePopup.EPriceType.None;
		if (data2.RenewCurrencyType == CurrencyType.Gold)
		{
			priceType = PricedMessagePopup.EPriceType.Gold;
		}
		else if (data2.RenewCurrencyType == CurrencyType.Gem)
		{
			priceType = PricedMessagePopup.EPriceType.Gem;
		}
		int optionRenewCount = equipItemInfo.GetOptionRenewCount(optionSlotIndex);
		int num = ((optionRenewCount < __Extension_Method_ICollection__.COUNT(renewPrice)) ? renewPrice[optionRenewCount] : renewPrice[__Extension_Method_ICollection__.COUNT(renewPrice) - 1]);
		int renewableOptionCount = data2.RenewableOptionCount;
		string title = LocalizationManager.GetString(NShared.NLocalization.EquipOptionRenew.ASK_RENEW_OPTION_TITLE);
		string message = LocalizationManager.GetString(NShared.NLocalization.EquipOptionRenew.ASK_RENEW_OPTION_MESSAGE, renewableOptionCount);
		string action = LocalizationManager.GetString(NShared.NLocalization.EquipOptionRenew.ACTION);
		int num2 = num;
		DropBonusEvent effectiveEvent = GlobalAutoBehaviour<DropBonusEventManager>.instance.GetEffectiveEvent(EventBonusType.DiscountEquipItemOptionRenew, null);
		if (effectiveEvent != null)
		{
			int num3 = num2 * effectiveEvent.BonusPercentage / 100;
			num2 = Mathf.Max(num2 - num3, 0);
		}
		PricedMessagePopup.Open(title, message, action, priceType, num, num2, delegate
		{
			RequestRenewEquipOptionInternal(equipItemSlotIndex, optionSlotIndex, 0);
		});
	}

	private void RequestRenewEquipOptionInternal(int equipItemSlotIndex, int optionSlotIndex, int couponItemIndex)
	{
		NShared.RenewEquipOption.Request request = new NShared.RenewEquipOption.Request();
		request.EquipItemSlotIndex = equipItemSlotIndex;
		request.OptionSlotIndex = optionSlotIndex;
		request.CouponItemIndex = couponItemIndex;
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.RenewEquipOption.Response response)
		{
			if (response != null)
			{
				if (response.BaseResult != BaseResultType.Success)
				{
					MessagePopup.OpenError(response.BaseResult);
				}
				else if (response.Result != NShared.RenewEquipOption.ResultType.Success)
				{
					if (response.Result == NShared.RenewEquipOption.ResultType.NotEnoughGem)
					{
						string title = LocalizationManager.GetString(NShared.NLocalization.Common.ERROR);
						string message = LocalizationManager.GetString(NShared.NLocalization.System.MOVE_GEM_SHOP_CONFIRM);
						MessagePopup.Open(title, message, delegate
						{
							if (SceneAutoBehaviour<LobbyManagement>.isValidInstance)
							{
								SceneAutoBehaviour<PayShopManagement>.instance.OpenNewPayShop(NewPayShopCategoryType.Ruby.ToString());
								GetInstance<EquipOptionRenewScene>().Close();
							}
						}, null);
					}
					else
					{
						MessagePopup.OpenError(response.Result);
					}
				}
				else
				{
					GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.CurrencyResult);
					GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.ItemResults);
					GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Apply(response.ResultEquipItem);
					GlobalAutoBehaviour<AccountManager>.instance.UserManager.MiscInfo.RenewOptionEquipItemSlotIndex = equipItemSlotIndex;
					GlobalAutoBehaviour<AccountManager>.instance.UserManager.MiscInfo.RenewOptionResultIndex = response.NewOptionIndex;
					GlobalAutoBehaviour<AccountManager>.instance.UserManager.MiscInfo.RenewOptionResultStep = response.NewOptionStep;
					GetInstance<EquipOptionRenewScene>().Open(this, equipItemSlotIndex, optionSlotIndex);
				}
			}
		});
	}

	void NGame2.NUI.NWindow.EquipOptionRenew.Listener.OpenLastRenewResult(NGame2.NUI.NWindow.EquipOptionRenew window, int equipItemSlotIndex, int optionSlotIndex)
	{
		GetInstance<EquipOptionRenewScene>().Open(this, equipItemSlotIndex, optionSlotIndex);
	}

	void EquipOptionRenewScene.Listener.SelectEquipOption(EquipOptionRenewScene caller, int optionSlotIndex, int optionIndex, EquipOptionRenewScene.OptionSelectResult result)
	{
		NShared.RenewConfirmEquipOption.Request request = new NShared.RenewConfirmEquipOption.Request();
		request.EquipItemSlotIndex = caller.EquipItemInfo.SlotIndex;
		request.ConfirmOptionIndex = optionIndex;
		request.OptionSlotIndex = optionSlotIndex;
		request.IsNew = result == EquipOptionRenewScene.OptionSelectResult.New;
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.RenewConfirmEquipOption.Response response)
		{
			if (response != null)
			{
				if (response.BaseResult != BaseResultType.Success)
				{
					MessagePopup.OpenError(response.BaseResult);
				}
				else if (response.Result != NShared.RenewConfirmEquipOption.ResultType.Success)
				{
					MessagePopup.OpenError(response.Result);
				}
				else
				{
					GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Apply(response.ResultEquipItem);
					GlobalAutoBehaviour<AccountManager>.instance.UserManager.MiscInfo.RenewOptionEquipItemSlotIndex = 0;
					GlobalAutoBehaviour<AccountManager>.instance.UserManager.MiscInfo.RenewOptionResultIndex = 0;
					GlobalAutoBehaviour<AccountManager>.instance.UserManager.MiscInfo.RenewOptionResultStep = 0;
					caller.ShowRenewOptionResult();
					MyHeroManagement myHeroManagement = SceneAutoBehaviour<MyHeroManagement>.instance;
					if (myHeroManagement != null)
					{
						HeroEquipTreasure heroEquipTreasure = myHeroManagement.GetInstance<HeroEquipTreasure>(openedOnly: true);
						if (heroEquipTreasure != null && heroEquipTreasure.IsOpen())
						{
							heroEquipTreasure.Refresh();
						}
					}
				}
			}
		});
	}

	void EquipOptionRenewScene.Listener.AgainOptionRenew(int equipItemSlotIndex, int optionSlotIndex)
	{
		RenewEquipOptionInternal(null, equipItemSlotIndex, optionSlotIndex);
	}

	void NGame2.NUI.NWindow.EquipTierUpgrade.Listener.OnUpgrade(NGame2.NUI.NWindow.EquipTierUpgrade window)
	{
		EquipItemInfo itemInfo = window.SelectedEquipItemInfo;
		if (itemInfo == null)
		{
			MessagePopup.OpenError(NShared.UpgradeEquipTier.ResultType.ItemDataNotFound);
			return;
		}
		if (!itemInfo.CLI_CanTierUpgrade())
		{
			MessagePopup.OpenError(NShared.UpgradeEquipTier.ResultType.NotEquipMaxStar);
			return;
		}
		if (!window.IsEnoughCost)
		{
			MessagePopup.OpenError(NShared.UpgradeEquipTier.ResultType.NotEnoughGold);
			return;
		}
		if (!window.IsEnoughMaterial)
		{
			MessagePopup.OpenError(NShared.UpgradeEquipTier.ResultType.NotEnoughMaterialItem);
			return;
		}
		new WebServiceTask<NShared.UpgradeEquipTier.Request, NShared.UpgradeEquipTier.Response>(new NShared.UpgradeEquipTier.Request
		{
			EquipItemSlotIndex = itemInfo.SlotIndex
		}, delegate(NShared.UpgradeEquipTier.Request request, NShared.UpgradeEquipTier.Response response)
		{
			if (response.Result != NShared.UpgradeEquipTier.ResultType.Success)
			{
				return base.TaskFactory.Run(delegate
				{
					MessagePopup.OpenError(response.Result);
				});
			}
			GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.CurrencyResults);
			GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.ItemResults);
			CachedHeroInfo heroOfEquipItem = GlobalAutoBehaviour<AccountManager>.instance.GetHeroOfEquipItem(itemInfo.SlotIndex);
			if (heroOfEquipItem != null)
			{
				int heroIndex = heroOfEquipItem.HeroIndex;
				int partIndexOfEquipItem = heroOfEquipItem.GetPartIndexOfEquipItem(itemInfo.SlotIndex);
				if (partIndexOfEquipItem == 0)
				{
					VespaDebug.LogError("Part index not found : equipItemSlotIndex=" + itemInfo.SlotIndex + " HeroIndex=" + heroIndex);
					return (ITask)null;
				}
				heroOfEquipItem.SetEquipItemInfo(partIndexOfEquipItem, null);
				GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.ApplyEquippedHeroIndex(itemInfo, heroIndex, 0);
			}
			if (response.EquipHeroIndex > 0)
			{
				CachedHeroInfo heroInfo = GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfo(response.EquipHeroIndex);
				if (heroInfo != null)
				{
					ItemData<EquipItemData> data = GameRoot.Instance.ItemDataContainer.GetData<EquipItemData>(response.ResultEquipItem.ItemIndex);
					if (data != null)
					{
						heroInfo.SetEquipItemInfo((int)data.SubData.PartType, response.ResultEquipItem);
					}
				}
			}
			GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Apply(itemInfo.SlotIndex, null);
			GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Apply(response.ResultEquipItem);
			GlobalAutoBehaviour<AccountManager>.instance.EquipPresetSotrageManger.Apply(response.EquipPresetInfos);
			BaseRewardPopup.OpenPopup(new BaseRewardPopup.PopupOpenData().SetTitle(LocalizationManager.GetString(NShared.NLocalization.EquipTierUpgrade.TIER_UPGRADE_SUCCESS_TITLE)).SetEquipResultInfos(new EquipItemInfo[1] { response.ResultEquipItem }), isGlobal: false);
			window.OnFinishUpgrade();
			return (ITask)null;
		}).Commit();
	}

	void EquipEnchantOption.Listener.OnEnchantOption(EquipEnchantOption window)
	{
		if (window.SelectedEquipItemInfo != null)
		{
			RequestEnchantEquip(window.SelectedEquipItemInfo.SlotIndex, window.ItemDataToEnchantOption);
		}
	}

	void EquipEnchantOptionScene.Listener.SelectEquipEnchantOption(EquipEnchantOptionScene caller, int optionIndex, EquipEnchantOptionScene.EnchantOptionSelectResult result)
	{
		NShared.ConfirmEquipEnchantOption.Request request = new NShared.ConfirmEquipEnchantOption.Request();
		request.EquipItemSlotIndex = caller.EquipItemInfo.SlotIndex;
		request.ConfirmEnchantOptionIndex = optionIndex;
		request.IsNew = result == EquipEnchantOptionScene.EnchantOptionSelectResult.New;
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.ConfirmEquipEnchantOption.Response response)
		{
			if (response != null)
			{
				if (response.BaseResult != BaseResultType.Success)
				{
					MessagePopup.OpenError(response.BaseResult);
				}
				else if (response.Result != NShared.ConfirmEquipEnchantOption.ResultType.Success)
				{
					MessagePopup.OpenError(response.Result);
				}
				else
				{
					GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Apply(response.ResultEquipItem);
					GlobalAutoBehaviour<AccountManager>.instance.UserManager.MiscInfo.RenewEnchantOptionEquipItemSlotIndex = 0;
					GlobalAutoBehaviour<AccountManager>.instance.UserManager.MiscInfo.RenewEnchantOptionResultIndex = 0;
					GlobalAutoBehaviour<AccountManager>.instance.UserManager.MiscInfo.RenewEnchantOptionResultStep = 0;
					caller.ShowEnchantOptionResult();
				}
			}
		});
	}

	void EquipEnchantOptionScene.Listener.AgainEnchantOptionRenew(int equipItemSlotIndex, ItemData EquipEnchantOptionItemData, int optionIndex)
	{
		if (EquipEnchantOptionItemData == null || Helper.GetItemCount(EquipEnchantOptionItemData.Index) == 0)
		{
			MessagePopup.OpenError(NShared.EnchantEquip.ResultType.NotEnoughMaterialItem);
			return;
		}
		string stringDirect = LocalizationManager.GetStringDirect("ENCHANT_REPEAT_MESSAGE");
		string confirmLabel = LocalizationManager.GetString(NShared.NLocalization.Common.USE);
		string cancelLabel = LocalizationManager.GetString(NShared.NLocalization.Common.COMMON_CANCEL);
		IconConfirmPopup.Open(stringDirect, confirmLabel, cancelLabel, delegate
		{
			RequestEnchantEquip(equipItemSlotIndex, EquipEnchantOptionItemData);
		}).AddItem(EquipEnchantOptionItemData.Index, 1);
	}

	private void RequestEnchantEquip(int equipItemSlotIndex, ItemData ItemDataToEnchantOption)
	{
		if (equipItemSlotIndex == 0)
		{
			return;
		}
		if (ItemDataToEnchantOption == null || Helper.GetItemCount(ItemDataToEnchantOption.Index) == 0)
		{
			MessagePopup.OpenError(NShared.EnchantEquip.ResultType.NotEnoughMaterialItem);
			return;
		}
		NShared.EnchantEquip.Request request = new NShared.EnchantEquip.Request
		{
			EquipItemSlotIndex = equipItemSlotIndex,
			EnchantItemIndex = ItemDataToEnchantOption.Index
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.EnchantEquip.Response response)
		{
			if (response != null)
			{
				if (response.BaseResult != BaseResultType.Success)
				{
					MessagePopup.OpenError(response.BaseResult);
				}
				else if (response.Result != NShared.EnchantEquip.ResultType.Success)
				{
					MessagePopup.OpenError(response.Result);
				}
				else
				{
					GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.CurrencyResult);
					GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.ItemResult);
					GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Apply(response.ResultEquipItem);
					if (response.NewEnchantOptionIndex != 0)
					{
						GlobalAutoBehaviour<AccountManager>.instance.UserManager.MiscInfo.RenewEnchantOptionEquipItemSlotIndex = response.ResultEquipItem.SlotIndex;
						GlobalAutoBehaviour<AccountManager>.instance.UserManager.MiscInfo.RenewEnchantOptionResultIndex = response.NewEnchantOptionIndex;
						GlobalAutoBehaviour<AccountManager>.instance.UserManager.MiscInfo.RenewEnchantOptionResultStep = response.NewEnchantOptionStep;
					}
					GetInstance<EquipEnchantOptionScene>().Open(this, ItemDataToEnchantOption, (response.NewEnchantOptionIndex == 0) ? response.ResultEquipItem : null);
				}
			}
		});
	}

	void EquipEnchantOption.Listener.OpenLastEnchantResult(EquipEnchantOption caller)
	{
		GetInstance<EquipEnchantOptionScene>().Open(this, caller.ItemDataToEnchantOption, null);
	}
}
