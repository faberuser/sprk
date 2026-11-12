using NGame2.NUI.NComponent2;
using NGame2.NUI.NHelper;
using NGame2.NUtil.NStringProxy;
using NShared;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class PunishmentRuneCraftResult : BaseWindow
{
	public interface IListener
	{
		void OnClickClose();
	}

	public const string PrefabPath = "Resources/Prefab2/Window/PunishmentRuneCraft/PunishmentRuneCraftResultWindow";

	public UILabel label_RuneName;

	public UILabel label_MainOptionType;

	public UILabel label_MainOptionValue;

	public UILabel label_SubOption;

	public Transform ItemIconRoot;

	private ItemIconComponent _resultComp;

	private IListener _listener;

	public void Open(IListener listener, EquipItemInfo equipItemInfo, PunishmentRuneOptionInfo optionInfo)
	{
		base.Open();
		_listener = listener;
		CreateItemComponent(equipItemInfo);
		ItemData itemData = equipItemInfo.ItemData;
		UIHelper.SetLabelText(label_RuneName, ItemNameBuilder.GetItemName(itemData));
		PunishmentRuneData data = GameRoot.Instance.PunishmentRuneDataContainer.GetData(itemData.Index);
		PunishmentRuneOptionData data2 = GameRoot.Instance.PunishmentRuneOptionDataContainer.GetData(data.MainOptionIndex);
		string arg = LocalizationManager.GetString(data2.StatTypes[0]);
		string statDisplayValue = StatData.GetStatDisplayValue(data2.StatTypes[0], optionInfo.MainOptionValue);
		string text = $"{arg} {statDisplayValue}";
		UIHelper.SetLabelText(label_MainOptionType, text);
		string text2 = string.Empty;
		for (int i = 1; i <= 3; i++)
		{
			int extraSkillIndex = optionInfo.GetExtraSkillIndex(i);
			int extraSkillValue = optionInfo.GetExtraSkillValue(i);
			SkillData data3 = GameRoot.Instance.SkillDataContainer.GetData(extraSkillIndex);
			if (data3 != null)
			{
				string displaySkillValue = RuneHelper.GetDisplaySkillValue(extraSkillValue, data.ExtraSkillFraction);
				string punishmentRuneEquipClassification = RuneHelper.GetPunishmentRuneEquipClassification(i);
				text2 = text2 + punishmentRuneEquipClassification + data3.BuildLocalize_CustomDescription(displaySkillValue) + "\n";
			}
		}
		UIHelper.SetLabelText(label_SubOption, text2);
	}

	private void CreateItemComponent(EquipItemInfo equipItemInfo)
	{
		_resultComp = ItemIconComponent.PrefabAuto.Factory.Create(ItemIconRoot);
		Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_resultComp.transform);
		_resultComp.Open(equipItemInfo.ItemData);
	}

	public override void Close()
	{
		base.Close();
		UIHelper.SetLabelText(label_RuneName, string.Empty);
		UIHelper.SetLabelText(label_MainOptionType, string.Empty);
		UIHelper.SetLabelText(label_MainOptionValue, string.Empty);
		UIHelper.SetLabelText(label_SubOption, string.Empty);
		FactoryBase.ReleaseComponent(_resultComp);
		_resultComp = null;
		if (_listener != null)
		{
			_listener.OnClickClose();
		}
	}
}
