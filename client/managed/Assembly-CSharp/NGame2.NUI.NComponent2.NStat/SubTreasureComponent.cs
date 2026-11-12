using System.Collections.Generic;
using NShared;
using NVespa;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NStat;

public class SubTreasureComponent : MonoBehaviour, IStatInfo
{
	public int HeightSize;

	public UIBundleLoader BundleTex_ItemIcon;

	public UILabel Label_Name;

	public SingleStatInfo[] StatInfos;

	public StatListComponent.ColorInfo SubjectColor;

	public StatListComponent.ColorInfo StatColor;

	public StatListComponent.ColorInfo ValueColor;

	public void SetItemInfo(ItemData itemData, EquipItemInfo equipItemInfo)
	{
		StatListComponent.ColorInfo colorInfo = SubjectColor ?? StatListComponent.DefaultStatColor;
		UIHelper.SetBundleTexture(BundleTex_ItemIcon, UIHelper.GetItemIconPath(itemData.Code));
		UIHelper.SetLabelText(Label_Name, NGUIText.EncodeColor(itemData.Name, colorInfo.LabelColor));
		StatListComponent.SetLabelGradient(Label_Name, colorInfo.Gradient);
		List<EquipItemOptionInfo> optionInfoList = equipItemInfo.GetOptionInfoList();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(optionInfoList) && i < __Extension_Method_ICollection__.COUNT(StatInfos); i++)
		{
			int optionOpenLevel = equipItemInfo.GetOptionOpenLevel(optionInfoList[i].SlotIndex);
			StatSetOptionStat(StatInfos[i], optionInfoList[i].OptionIndex, equipItemInfo.Star, optionInfoList[i].OptionStep, optionInfoList[i].IsOptionOpened, optionOpenLevel, optionInfoList[i].IsRenewedOption, isFixedOption: false);
		}
	}

	private void StatSetOptionStat(SingleStatInfo component, int optionIndex, int star, int optionStep, bool opened, int openLevel, bool renew, bool isFixedOption)
	{
		EquipOptionData data = GameRoot.Instance.EquipOptionDataContainer.GetData(optionIndex);
		if (data == null)
		{
			VespaDebug.LogError("Option data not found : " + optionIndex);
			return;
		}
		OperationType optionType = data.OptionType;
		int optionValue = data.GetOptionValue(star, optionStep);
		string optionDisplay = StatData.GetOptionDisplay(optionType, optionValue, opened, openLevel, !renew, isFixedOption);
		SingleStatInfo.StatIconType iconType = (renew ? SingleStatInfo.StatIconType.Renew : SingleStatInfo.StatIconType.Normal);
		CreateNormalStat(component, optionDisplay, "", iconType);
	}

	private void CreateNormalStat(SingleStatInfo component, string typeName, string value, SingleStatInfo.StatIconType iconType = SingleStatInfo.StatIconType.Normal)
	{
		StatListComponent.ColorInfo colorInfo = StatColor ?? StatListComponent.DefaultStatColor;
		StatListComponent.ColorInfo colorInfo2 = ValueColor ?? StatListComponent.DefaultStatColor;
		UIHelper.SetLabelText(component.NameLabel, NGUIText.EncodeColor(typeName, colorInfo.LabelColor));
		UIHelper.SetLabelText(component.ValueLabel, NGUIText.EncodeColor(value, colorInfo2.LabelColor));
		StatListComponent.SetLabelGradient(component.NameLabel, colorInfo.Gradient);
		StatListComponent.SetLabelGradient(component.ValueLabel, colorInfo2.Gradient);
		component.SelectIcon(iconType);
	}

	int IStatInfo.GetHeight()
	{
		if (HeightSize <= 0)
		{
			return GetComponent<UIWidget>().height;
		}
		return HeightSize;
	}
}
