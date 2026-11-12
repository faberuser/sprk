using NGame2.NUI.NWindow;
using NShared;
using NVespa;

namespace NGame2.NUI.NManager;

public class ValanceManagement : BaseManagement<ValanceManagement>, ValanceManageWindow.IListener, ValanceIdentifiedWindow.IListener, ValanceAwakenWindow.IListener, ValanceOptionRenewWindow.IListener, ValanceOptionRenewResultWindow.Listener, ValanceTierUpgradeWindow.IListener, ValanceCraftWindow.IListener, ValanceEnchantWindow.IListener, ValanceWebManager.IListener
{
	protected override void Awake()
	{
		base.Awake();
		AddInstance<NGame2.NUI.NWindow.ValanceMainLobbyWindow>("Resources/Prefab2/Window/Valance/ValanceMainLobbyWindow");
		AddInstance<ValanceIdentifiedWindow>("Resources/Prefab2/Window/Valance/ValanceIdentifiedWindow");
		AddInstance<ValanceIdentifiedResultWindow>("Resources/Prefab2/Window/Valance/ValanceIdentifiedResultWindow");
		AddInstance<ValanceCraftWindow>("Resources/Prefab2/Window/Valance/ValanceCraftWindow");
		AddInstance<ValanceCraftResultWindow>("Resources/Prefab2/Window/Valance/ValanceCraftResultWindow");
		AddInstance<ValanceManageWindow>("Resources/Prefab2/Window/Valance/ValanceManageWindow");
		AddInstance<ValanceAwakenWindow>("Resources/Prefab2/Window/Valance/ValanceAwakenWindow");
		AddInstance<ValanceAwakenResultWindow>("Resources/Prefab2/Window/Valance/ValanceAwakenResultWindow");
		AddInstance<ValanceOptionRenewWindow>("Resources/Prefab2/Window/Valance/ValanceOptionRenewWindow");
		AddInstance<ValanceOptionRenewResultWindow>("Resources/Prefab2/Window/Valance/ValanceOptionRenewResultWindow");
		AddInstance<ValanceTierUpgradeWindow>("Resources/Prefab2/Window/Valance/ValanceTierUpgradeWindow");
		AddInstance<ValanceTierUpgradeResultWindow>("Resources/Prefab2/Window/Valance/ValanceTierUpgradeResultWindow");
		AddInstance<ValanceEnchantWindow>("Resources/Prefab2/Window/Valance/ValanceEnchantWindow");
		SingletonAuto<ValanceWebManager>.instance.AddListener(this);
	}

	protected override void OnDestroy()
	{
		SingletonAuto<ValanceWebManager>.instance.RemoveListener(this);
		base.OnDestroy();
	}

	public BaseWindow OpenMainLobby(CampaignNodeData nodeData)
	{
		NGame2.NUI.NWindow.ValanceMainLobbyWindow valanceMainLobbyWindow = GetInstance<NGame2.NUI.NWindow.ValanceMainLobbyWindow>();
		if (valanceMainLobbyWindow == null || valanceMainLobbyWindow.IsOpen())
		{
			return null;
		}
		valanceMainLobbyWindow.Open(nodeData);
		return valanceMainLobbyWindow;
	}

	public void OpenIdentified()
	{
		ValanceIdentifiedWindow valanceIdentifiedWindow = GetInstance<ValanceIdentifiedWindow>();
		if (!(valanceIdentifiedWindow == null) && !valanceIdentifiedWindow.IsOpen())
		{
			valanceIdentifiedWindow.Open(this);
		}
	}

	public void OpenIdentifiedInven(EquipItemInfo selectedEquipItemInfo)
	{
		ValanceIdentifiedWindow valanceIdentifiedWindow = GetInstance<ValanceIdentifiedWindow>();
		if (!(valanceIdentifiedWindow == null) && !valanceIdentifiedWindow.IsOpen())
		{
			valanceIdentifiedWindow.OpenCustomCinema(selectedEquipItemInfo, this);
		}
	}

	public void OpenIdentifiedResult(EquipItemInfo[] resultInfos)
	{
		ValanceIdentifiedResultWindow valanceIdentifiedResultWindow = GetInstance<ValanceIdentifiedResultWindow>();
		if (!(valanceIdentifiedResultWindow == null) && !valanceIdentifiedResultWindow.IsOpen())
		{
			valanceIdentifiedResultWindow.Open(resultInfos);
		}
	}

	public void OpenCraft()
	{
		ValanceCraftWindow valanceCraftWindow = GetInstance<ValanceCraftWindow>();
		if (!(valanceCraftWindow == null) && !valanceCraftWindow.IsOpen())
		{
			valanceCraftWindow.Open(this);
		}
	}

	public void OpenCraftInven()
	{
		ValanceCraftWindow valanceCraftWindow = GetInstance<ValanceCraftWindow>();
		if (!(valanceCraftWindow == null) && !valanceCraftWindow.IsOpen())
		{
			valanceCraftWindow.OpenCutomCinema(this);
		}
	}

	public void OpenCraftResult(EquipItemInfo result, bool isAdvanced)
	{
		ValanceCraftResultWindow valanceCraftResultWindow = GetInstance<ValanceCraftResultWindow>();
		if (!(valanceCraftResultWindow == null) && !valanceCraftResultWindow.IsOpen())
		{
			valanceCraftResultWindow.Open(result, isAdvanced);
		}
	}

	public void OpenManage()
	{
		ValanceManageWindow valanceManageWindow = GetInstance<ValanceManageWindow>();
		if (!(valanceManageWindow == null) && !valanceManageWindow.IsOpen())
		{
			valanceManageWindow.Open(this);
		}
	}

	public void OpenManageInven()
	{
		ValanceManageWindow valanceManageWindow = GetInstance<ValanceManageWindow>();
		if (!(valanceManageWindow == null) && !valanceManageWindow.IsOpen())
		{
			valanceManageWindow.OpenCustomCinema(this);
		}
	}

	public void OpenOptionRenew()
	{
		ValanceOptionRenewWindow valanceOptionRenewWindow = GetInstance<ValanceOptionRenewWindow>();
		if (!(valanceOptionRenewWindow == null) && !valanceOptionRenewWindow.IsOpen())
		{
			valanceOptionRenewWindow.Open(this);
		}
	}

	public void OpenRenewInven(EquipItemInfo itemInfo)
	{
		ValanceOptionRenewWindow valanceOptionRenewWindow = GetInstance<ValanceOptionRenewWindow>();
		if (!(valanceOptionRenewWindow == null) && !valanceOptionRenewWindow.IsOpen())
		{
			valanceOptionRenewWindow.OpenCustomCinema(itemInfo, this);
		}
	}

	public void OpenOptionRenewResult(int itemSlotIndex, int optionIndex)
	{
		ValanceOptionRenewResultWindow valanceOptionRenewResultWindow = GetInstance<ValanceOptionRenewResultWindow>();
		if (!(valanceOptionRenewResultWindow == null))
		{
			valanceOptionRenewResultWindow.Open(this, itemSlotIndex, optionIndex);
		}
	}

	public void OpenSkillRenewResult(int itemSlotIndex, int skillIndex)
	{
		ValanceOptionRenewResultWindow valanceOptionRenewResultWindow = GetInstance<ValanceOptionRenewResultWindow>();
		if (!(valanceOptionRenewResultWindow == null))
		{
			valanceOptionRenewResultWindow.Open(this, itemSlotIndex, skillIndex, ValanceOptionRenewResultWindow.ERenewType.Skill);
		}
	}

	public void OpenAwaken()
	{
		ValanceAwakenWindow valanceAwakenWindow = GetInstance<ValanceAwakenWindow>();
		if (!(valanceAwakenWindow == null) && !valanceAwakenWindow.IsOpen())
		{
			valanceAwakenWindow.Open(this);
		}
	}

	public void OpenAwakenInven(EquipItemInfo itemInfo)
	{
		ValanceAwakenWindow valanceAwakenWindow = GetInstance<ValanceAwakenWindow>();
		if (!(valanceAwakenWindow == null) && !valanceAwakenWindow.IsOpen())
		{
			valanceAwakenWindow.OpenCutomCinema(itemInfo, this);
		}
	}

	public void OpenAwakenResult(EquipItemInfo oldEquipItemInfo, EquipItemInfo resultInfo)
	{
		ValanceAwakenResultWindow valanceAwakenResultWindow = GetInstance<ValanceAwakenResultWindow>();
		if (!(valanceAwakenResultWindow == null) && !valanceAwakenResultWindow.IsOpen())
		{
			valanceAwakenResultWindow.Open(oldEquipItemInfo, resultInfo);
		}
	}

	public void OpenTierUpgrade()
	{
		ValanceTierUpgradeWindow valanceTierUpgradeWindow = GetInstance<ValanceTierUpgradeWindow>();
		if (!(valanceTierUpgradeWindow == null) && !valanceTierUpgradeWindow.IsOpen())
		{
			valanceTierUpgradeWindow.Open(this);
		}
	}

	public void OpenTierUpInven(EquipItemInfo itemInfo)
	{
		ValanceTierUpgradeWindow valanceTierUpgradeWindow = GetInstance<ValanceTierUpgradeWindow>();
		if (!(valanceTierUpgradeWindow == null) && !valanceTierUpgradeWindow.IsOpen())
		{
			valanceTierUpgradeWindow.OpenCustomCinema(itemInfo, this);
		}
	}

	public void OpenTierUpgradeResult(EquipItemInfo oldInfo, EquipItemInfo resultInfo)
	{
		ValanceTierUpgradeResultWindow valanceTierUpgradeResultWindow = GetInstance<ValanceTierUpgradeResultWindow>();
		if (!(valanceTierUpgradeResultWindow == null) && !valanceTierUpgradeResultWindow.IsOpen())
		{
			valanceTierUpgradeResultWindow.Open(oldInfo, resultInfo);
		}
	}

	public void OpenEnchant()
	{
		ValanceEnchantWindow valanceEnchantWindow = GetInstance<ValanceEnchantWindow>();
		if (!(valanceEnchantWindow == null) && !valanceEnchantWindow.IsOpen())
		{
			valanceEnchantWindow.Open(this);
		}
	}

	public void OpenEnchantInven(EquipItemInfo itemInfo)
	{
		ValanceEnchantWindow valanceEnchantWindow = GetInstance<ValanceEnchantWindow>();
		if (!(valanceEnchantWindow == null) && !valanceEnchantWindow.IsOpen())
		{
			valanceEnchantWindow.OpenCustomCinema(itemInfo, this);
		}
	}

	public void ManageOnOtherWindow()
	{
		ValanceManageWindow valanceManageWindow = GetInstance<ValanceManageWindow>(openedOnly: true);
		if (!(valanceManageWindow == null) && valanceManageWindow.IsOpen())
		{
			valanceManageWindow.HideButtonRoot();
		}
	}

	public void ManageReOpen()
	{
		ValanceManageWindow valanceManageWindow = GetInstance<ValanceManageWindow>(openedOnly: true);
		if (!(valanceManageWindow == null) && valanceManageWindow.IsOpen())
		{
			valanceManageWindow.ShowButtonRoot();
		}
	}

	void ValanceManageWindow.IListener.OnClickAwaken()
	{
		OpenAwaken();
	}

	void ValanceManageWindow.IListener.OnClickOptionRenew()
	{
		OpenOptionRenew();
	}

	void ValanceManageWindow.IListener.OnClickTierUpgrade()
	{
		OpenTierUpgrade();
	}

	void ValanceManageWindow.IListener.OnClickEnchant()
	{
		OpenEnchant();
	}

	void ValanceIdentifiedWindow.IListener.OnClickIdentified(int[] slotIndices)
	{
		SingletonAuto<ValanceWebManager>.instance.RequestValanceIdentified(slotIndices);
	}

	void ValanceAwakenWindow.IListener.OnClickAwaken(int slotIndex)
	{
		SingletonAuto<ValanceWebManager>.instance.RequestValanceAwaken(slotIndex);
	}

	void ValanceOptionRenewWindow.IListener.OnClickRenew(int equipItemSlotIndex, int optionSlotIndex)
	{
		SingletonAuto<ValanceWebManager>.instance.RequestValanceOptionRenew(equipItemSlotIndex, optionSlotIndex);
	}

	void ValanceOptionRenewWindow.IListener.OpenLastRenewResult(int equipItemSlotIndex, int optionSlotIndex)
	{
		OpenOptionRenewResult(equipItemSlotIndex, optionSlotIndex);
	}

	void ValanceOptionRenewWindow.IListener.OnClickSkillRenew(int equipItemSlotIndex, int skillSlotIndex)
	{
		SingletonAuto<ValanceWebManager>.instance.RequestValanceSkillRenew(equipItemSlotIndex, skillSlotIndex);
	}

	void ValanceOptionRenewWindow.IListener.OpenLastSkillRenewResult(int equipItemSlotIndex, int skillSlotIndex)
	{
		OpenSkillRenewResult(equipItemSlotIndex, skillSlotIndex);
	}

	void ValanceOptionRenewResultWindow.Listener.SelectEquipOption(int equipItemSlotIndex, int optionSlotIndex, int optionIndex, ValanceOptionRenewResultWindow.OptionSelectResult result)
	{
		SingletonAuto<ValanceWebManager>.instance.RequestValanceOptionConfirm(equipItemSlotIndex, optionSlotIndex, optionIndex, result);
	}

	void ValanceOptionRenewResultWindow.Listener.AgainOptionRenew(int equipItemSlotIndex, int optionSlotIndex)
	{
		SingletonAuto<ValanceWebManager>.instance.RequestValanceOptionRenew(equipItemSlotIndex, optionSlotIndex);
	}

	void ValanceOptionRenewResultWindow.Listener.SelectEquipSkill(int equipItemSlotIndex, int skillSlotIndex, int optionIndex, ValanceOptionRenewResultWindow.OptionSelectResult result)
	{
		SingletonAuto<ValanceWebManager>.instance.RequestValanceSkillConfirm(equipItemSlotIndex, skillSlotIndex, optionIndex, result);
	}

	void ValanceOptionRenewResultWindow.Listener.AgainSkillRenew(int equipItemSlotIndex, int skillSlotIndex)
	{
		SingletonAuto<ValanceWebManager>.instance.RequestValanceSkillRenew(equipItemSlotIndex, skillSlotIndex);
	}

	void ValanceTierUpgradeWindow.IListener.OnClickTierUpgrade(int slotIndex)
	{
		SingletonAuto<ValanceWebManager>.instance.RequestValanceTierUpgrade(slotIndex);
	}

	void ValanceCraftWindow.IListener.OnClickCraft(ValanceCraftDataKey key, int extraEquipOptionIndex)
	{
		if (key != null)
		{
			SingletonAuto<ValanceWebManager>.instance.RequestValanceCraft(key, extraEquipOptionIndex);
		}
	}

	void ValanceEnchantWindow.IListener.OnClickEnchant(int equipItemSlotIndex, int enchantCoreItemIndex)
	{
		SingletonAuto<ValanceWebManager>.instance.RequestValanceEnchant(equipItemSlotIndex, enchantCoreItemIndex);
	}

	void ValanceEnchantWindow.IListener.OnClickConfirmEnchant(int equipItemSlotIndex, bool isNew)
	{
		SingletonAuto<ValanceWebManager>.instance.RequestValanceEnchantConfirm(equipItemSlotIndex, isNew);
	}

	void ValanceWebManager.IListener.OnValanceAwaken(EquipItemInfo oldEquipInfo, EquipItemInfo resultInfo)
	{
		OpenAwakenResult(oldEquipInfo, resultInfo);
	}

	void ValanceWebManager.IListener.OnValanceCraft(EquipItemInfo result, bool isAvanced)
	{
		ValanceCraftWindow valanceCraftWindow = GetInstance<ValanceCraftWindow>();
		if (valanceCraftWindow != null && valanceCraftWindow.IsOpen())
		{
			valanceCraftWindow.Refresh();
		}
		OpenCraftResult(result, isAvanced);
	}

	void ValanceWebManager.IListener.OnValanceIdentified(EquipItemInfo[] resultInfos)
	{
		OpenIdentifiedResult(resultInfos);
		ValanceIdentifiedWindow valanceIdentifiedWindow = GetInstance<ValanceIdentifiedWindow>();
		if (valanceIdentifiedWindow != null && valanceIdentifiedWindow.IsOpen())
		{
			valanceIdentifiedWindow.Refresh();
		}
	}

	void ValanceWebManager.IListener.OnValanceOptionRenew(int itemSlotIndex, int optionIndex)
	{
		OpenOptionRenewResult(itemSlotIndex, optionIndex);
	}

	void ValanceWebManager.IListener.OnValanceOptionRenewConfirm()
	{
		ValanceOptionRenewResultWindow valanceOptionRenewResultWindow = GetInstance<ValanceOptionRenewResultWindow>();
		if (valanceOptionRenewResultWindow != null && valanceOptionRenewResultWindow.IsOpen())
		{
			valanceOptionRenewResultWindow.Refresh();
		}
	}

	void ValanceWebManager.IListener.OnValanceSkillRenew(int itemSlotIndex, int skillIndex)
	{
		OpenSkillRenewResult(itemSlotIndex, skillIndex);
	}

	void ValanceWebManager.IListener.OnValanceSkillRenewConfirm()
	{
		ValanceOptionRenewResultWindow valanceOptionRenewResultWindow = GetInstance<ValanceOptionRenewResultWindow>();
		if (valanceOptionRenewResultWindow != null && valanceOptionRenewResultWindow.IsOpen())
		{
			valanceOptionRenewResultWindow.Refresh();
		}
	}

	void ValanceWebManager.IListener.OnValanceTierUpgrade(EquipItemInfo oldInfo, EquipItemInfo resultInfo)
	{
		ValanceTierUpgradeWindow valanceTierUpgradeWindow = GetInstance<ValanceTierUpgradeWindow>();
		if ((object)valanceTierUpgradeWindow != null && valanceTierUpgradeWindow.IsOpen())
		{
			valanceTierUpgradeWindow.Refresh();
		}
		OpenTierUpgradeResult(oldInfo, resultInfo);
	}

	void ValanceWebManager.IListener.OnValanceEnchant(ValanceEnchantOptionInfo newInfo)
	{
		GetInstance<ValanceEnchantWindow>().OnEnchantResponse(newInfo);
	}

	void ValanceWebManager.IListener.OnValanceEnchantConfirm()
	{
		GetInstance<ValanceEnchantWindow>().OnEnchantConfirmResponse();
	}
}
