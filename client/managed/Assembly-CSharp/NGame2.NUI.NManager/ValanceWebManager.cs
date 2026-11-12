using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NWindow;
using NShared;
using NShared.BeginCampaign;
using NShared.ConfirmValanceEnchant;
using NShared.NLocalization;
using NShared.RenewConfirmEquipOption;
using NShared.RenewConfirmEquipSkill;
using NShared.RenewEquipOption;
using NShared.RenewEquipSkill;
using NShared.UpgradeEquipTier;
using NShared.ValanceAwaken;
using NShared.ValanceCraft;
using NShared.ValanceEnchant;
using NShared.ValanceIdentified;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NUI.NManager;

public class ValanceWebManager : SingletonAuto<ValanceWebManager>
{
	public interface IListener
	{
		void OnValanceIdentified(EquipItemInfo[] result);

		void OnValanceCraft(EquipItemInfo result, bool isAdvanced);

		void OnValanceAwaken(EquipItemInfo oldEquipInfo, EquipItemInfo result);

		void OnValanceOptionRenew(int itemSlotIndex, int optionIndex);

		void OnValanceOptionRenewConfirm();

		void OnValanceSkillRenew(int itemSlotIndex, int skillIndex);

		void OnValanceSkillRenewConfirm();

		void OnValanceTierUpgrade(EquipItemInfo oldInfo, EquipItemInfo result);

		void OnValanceEnchant(ValanceEnchantOptionInfo newEnchantOption);

		void OnValanceEnchantConfirm();
	}

	public List<IListener> _listListener = new List<IListener>();

	public void AddListener(IListener listener)
	{
		if (!_listListener.Contains(listener))
		{
			_listListener.Add(listener);
		}
	}

	public void RemoveListener(IListener listener)
	{
		_listListener.Remove(listener);
	}

	public void RequestValanceIdentified(int[] slotIndices)
	{
		NShared.ValanceIdentified.Request request = new NShared.ValanceIdentified.Request
		{
			EquipItemSlotIndices = slotIndices
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.ValanceIdentified.Response res)
		{
			if (res.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(res.BaseResult);
			}
			else if (res.Result != NShared.ValanceIdentified.ResultType.Success)
			{
				MessagePopup.OpenError(res.Result);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(res.CurrencyResult);
				GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Apply(res.EquipItemResults);
				_listListener.ForEach(delegate(IListener e)
				{
					e.OnValanceIdentified(res.EquipItemResults);
				});
			}
		});
	}

	public void RequestValanceCraft(ValanceCraftDataKey key, int equipOptionIndex)
	{
		NShared.ValanceCraft.Request request = new NShared.ValanceCraft.Request();
		request.SetType = key.SetType;
		request.CreatureTagType = key.CreatureTagType;
		request.PartType = key.EquipPartType;
		request.SubPartType = key.EquipItemSubType;
		request.SelectEquipOptionIndices = ((equipOptionIndex == 0) ? null : new int[1] { equipOptionIndex });
		NShared.ValanceCraft.Request request2 = request;
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request2, delegate(NShared.ValanceCraft.Response res)
		{
			if (res.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(res.BaseResult);
			}
			else if (res.Result == NShared.ValanceCraft.ResultType.EquipItemFull)
			{
				string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
				string message = LocalizationManager.GetString(NShared.BeginCampaign.ResultType.EquipItemFull);
				MessagePopup.OpenEnabled(title, message, delegate
				{
					GlobalAutoBehaviour<GlobalManagement>.instance.CloseAllWindows();
					SceneAutoBehaviour<InventoryManagement>.instance.OpenInventory();
				}, null, "MOVE_BAG", bYesEnabled: true);
			}
			else if (res.Result != NShared.ValanceCraft.ResultType.Success)
			{
				MessagePopup.OpenError(res.Result);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(res.CurrencyResult);
				GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Apply(res.EquipItemResult);
				GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(res.ItemResults);
				bool isAdvanced = res.IsAdvanced == 1;
				_listListener.ForEach(delegate(IListener e)
				{
					e.OnValanceCraft(res.EquipItemResult, isAdvanced);
				});
			}
		});
	}

	public void RequestValanceAwaken(int slotIndex)
	{
		EquipItemInfo oldEquipInfo = GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Get(slotIndex);
		NShared.ValanceAwaken.Request request = new NShared.ValanceAwaken.Request
		{
			EquipItemSlotIndex = slotIndex
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.ValanceAwaken.Response res)
		{
			if (res.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(res.BaseResult);
			}
			else if (res.Result != NShared.ValanceAwaken.ResultType.Success)
			{
				MessagePopup.OpenError(res.Result);
			}
			else
			{
				_listListener.ForEach(delegate(IListener e)
				{
					e.OnValanceAwaken(oldEquipInfo, res.ResultEquipItem);
				});
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(res.CurrencyResult);
				GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(res.ItemResults);
				GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Apply(slotIndex, res.ResultEquipItem);
			}
		});
	}

	public void RequestValanceOptionRenew(int equipItemSlotIndex, int optionSlotIndex)
	{
		NShared.RenewEquipOption.Request request = new NShared.RenewEquipOption.Request
		{
			EquipItemSlotIndex = equipItemSlotIndex,
			OptionSlotIndex = optionSlotIndex
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.RenewEquipOption.Response res)
		{
			if (res.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(res.BaseResult);
			}
			else if (res.Result != NShared.RenewEquipOption.ResultType.Success)
			{
				MessagePopup.OpenError(res.Result);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(res.CurrencyResult);
				GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(res.ItemResults);
				GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Apply(res.ResultEquipItem);
				ValanceRenewOptionInfo valanceOptionRenew = new ValanceRenewOptionInfo
				{
					RenewOptionEquipItemSlotIndex = equipItemSlotIndex,
					RenewOptionResultIndex = res.NewOptionIndex,
					RenewOptionResultStep = res.NewOptionStep
				};
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.SetValanceOptionRenew(valanceOptionRenew);
				_listListener.ForEach(delegate(IListener e)
				{
					e.OnValanceOptionRenew(equipItemSlotIndex, optionSlotIndex);
				});
			}
		});
	}

	public void RequestValanceOptionConfirm(int slotIndex, int optionSlotIndex, int optionIndex, ValanceOptionRenewResultWindow.OptionSelectResult result)
	{
		NShared.RenewConfirmEquipOption.Request request = new NShared.RenewConfirmEquipOption.Request
		{
			EquipItemSlotIndex = slotIndex,
			ConfirmOptionIndex = optionIndex,
			OptionSlotIndex = optionSlotIndex,
			IsNew = (result == ValanceOptionRenewResultWindow.OptionSelectResult.New)
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.RenewConfirmEquipOption.Response res)
		{
			if (res.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(res.BaseResult);
			}
			else if (res.Result != NShared.RenewConfirmEquipOption.ResultType.Success)
			{
				MessagePopup.OpenError(res.Result);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Apply(res.ResultEquipItem);
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.SetValanceOptionRenew(null);
				MyHeroManagement myHeroManagement = SceneAutoBehaviour<MyHeroManagement>.instance;
				if (myHeroManagement != null)
				{
					HeroEquipTreasure heroEquipTreasure = myHeroManagement.GetInstance<HeroEquipTreasure>(openedOnly: true);
					if (heroEquipTreasure != null && heroEquipTreasure.IsOpen())
					{
						heroEquipTreasure.Refresh();
					}
				}
				_listListener.ForEach(delegate(IListener e)
				{
					e.OnValanceOptionRenewConfirm();
				});
			}
		});
	}

	public void RequestValanceSkillRenew(int equipItemSlotIndex, int skillSlotIndex)
	{
		NShared.RenewEquipSkill.Request request = new NShared.RenewEquipSkill.Request
		{
			EquipItemSlotIndex = equipItemSlotIndex,
			SkillSlotIndex = skillSlotIndex
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.RenewEquipSkill.Response res)
		{
			if (res.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(res.BaseResult);
			}
			else if (res.Result != NShared.RenewEquipSkill.ResultType.Success)
			{
				MessagePopup.OpenError(res.Result);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(res.CurrencyResult);
				GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(res.ItemResults);
				GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Apply(res.ResultEquipItem);
				ValanceRenewSkillInfo valanceSkillRenew = new ValanceRenewSkillInfo
				{
					RenewSkillEquipItemSlotIndex = equipItemSlotIndex,
					RenewSkillResultIndex = res.NewSkillIndex,
					RenewSkillResultStep = res.NewSkillStep
				};
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.SetValanceSkillRenew(valanceSkillRenew);
				_listListener.ForEach(delegate(IListener e)
				{
					e.OnValanceSkillRenew(equipItemSlotIndex, skillSlotIndex);
				});
			}
		});
	}

	public void RequestValanceSkillConfirm(int slotIndex, int skillSlotIndex, int skillIndex, ValanceOptionRenewResultWindow.OptionSelectResult result)
	{
		NShared.RenewConfirmEquipSkill.Request request = new NShared.RenewConfirmEquipSkill.Request
		{
			EquipItemSlotIndex = slotIndex,
			SkillSlotIndex = skillSlotIndex,
			ConfirmSkillIndex = skillIndex,
			IsNew = (result == ValanceOptionRenewResultWindow.OptionSelectResult.New)
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.RenewConfirmEquipSkill.Response res)
		{
			if (res.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(res.BaseResult);
			}
			else if (res.Result != NShared.RenewConfirmEquipSkill.ResultType.Success)
			{
				MessagePopup.OpenError(res.Result);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Apply(res.ResultEquipItem);
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.SetValanceSkillRenew(null);
				MyHeroManagement myHeroManagement = SceneAutoBehaviour<MyHeroManagement>.instance;
				if (myHeroManagement != null)
				{
					HeroEquipTreasure heroEquipTreasure = myHeroManagement.GetInstance<HeroEquipTreasure>(openedOnly: true);
					if (heroEquipTreasure != null && heroEquipTreasure.IsOpen())
					{
						heroEquipTreasure.Refresh();
					}
				}
				_listListener.ForEach(delegate(IListener e)
				{
					e.OnValanceSkillRenewConfirm();
				});
			}
		});
	}

	public void RequestValanceTierUpgrade(int slotIndex)
	{
		EquipItemInfo oldEquipInfo = GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Get(slotIndex);
		NShared.UpgradeEquipTier.Request request = new NShared.UpgradeEquipTier.Request
		{
			EquipItemSlotIndex = slotIndex
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.UpgradeEquipTier.Response res)
		{
			if (res.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(res.BaseResult);
			}
			else if (res.Result != NShared.UpgradeEquipTier.ResultType.Success)
			{
				MessagePopup.OpenError(res.Result);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(res.CurrencyResults);
				GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(res.ItemResults);
				CachedHeroInfo heroOfEquipItem = GlobalAutoBehaviour<AccountManager>.instance.GetHeroOfEquipItem(slotIndex);
				if (heroOfEquipItem != null)
				{
					int heroIndex = heroOfEquipItem.HeroIndex;
					int partIndexOfEquipItem = heroOfEquipItem.GetPartIndexOfEquipItem(slotIndex);
					if (partIndexOfEquipItem != 0)
					{
						heroOfEquipItem.SetEquipItemInfo(partIndexOfEquipItem, null);
						GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.ApplyEquippedHeroIndex(oldEquipInfo, heroIndex, 0);
					}
				}
				if (res.EquipHeroIndex > 0)
				{
					CachedHeroInfo heroInfo = GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfo(res.EquipHeroIndex);
					if (heroInfo != null)
					{
						ItemData<EquipItemData> data = GameRoot.Instance.ItemDataContainer.GetData<EquipItemData>(res.ResultEquipItem.ItemIndex);
						if (data != null)
						{
							heroInfo.SetEquipItemInfo((int)data.SubData.PartType, res.ResultEquipItem);
						}
					}
				}
				GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Apply(slotIndex, null);
				GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Apply(res.ResultEquipItem);
				GlobalAutoBehaviour<AccountManager>.instance.EquipPresetSotrageManger.Apply(res.EquipPresetInfos);
				SprkRefreshTechnomagicHero();
				_listListener.ForEach(delegate(IListener e)
				{
					e.OnValanceTierUpgrade(oldEquipInfo, res.ResultEquipItem);
				});
			}
		});
	}

	public void RequestValanceEnchant(int equipItemSlotIndex, int enchantCoreItemIndex)
	{
		NShared.ValanceEnchant.Request request = new NShared.ValanceEnchant.Request
		{
			EquipItemSlotIndex = equipItemSlotIndex,
			ConsumeItemIndex = enchantCoreItemIndex
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.ValanceEnchant.Response res)
		{
			if (res.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(res.BaseResult);
			}
			else if (res.Result != NShared.ValanceEnchant.ResultType.Success)
			{
				MessagePopup.OpenError(res.Result);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(res.CurrencyResult);
				GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(res.ItemResultInfo);
				GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Apply(res.ResultEquipItem);
				ValanceEnchantOptionInfo resOptionInfo = res.NewValanceEnchantOptionInfo;
				resOptionInfo.ConsumeItemIndex = enchantCoreItemIndex;
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.SetValanceEnchant(resOptionInfo);
				_listListener.ForEach(delegate(IListener e)
				{
					e.OnValanceEnchant(resOptionInfo);
				});
			}
		});
	}

	public void RequestValanceEnchantConfirm(int slotIndex, bool isNew)
	{
		NShared.ConfirmValanceEnchant.Request request = new NShared.ConfirmValanceEnchant.Request
		{
			EquipItemSlotIndex = slotIndex,
			IsNew = isNew
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.ConfirmValanceEnchant.Response res)
		{
			if (res.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(res.BaseResult);
			}
			else if (res.Result != NShared.ConfirmValanceEnchant.ResultType.Success)
			{
				MessagePopup.OpenError(res.Result);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Apply(res.ResultEquipItem);
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.SetValanceEnchant(null);
				MyHeroManagement myHeroManagement = SceneAutoBehaviour<MyHeroManagement>.instance;
				if (myHeroManagement != null)
				{
					HeroEquipTreasure heroEquipTreasure = myHeroManagement.GetInstance<HeroEquipTreasure>(openedOnly: true);
					if (heroEquipTreasure != null && heroEquipTreasure.IsOpen())
					{
						heroEquipTreasure.Refresh();
					}
				}
				_listListener.ForEach(delegate(IListener e)
				{
					e.OnValanceEnchantConfirm();
				});
			}
		});
	}

	private static void SprkRefreshTechnomagicHero()
	{
		SceneAutoBehaviour<MyHeroManagement>.instance?.SprkRefreshTechnomagicGear();
	}
}
