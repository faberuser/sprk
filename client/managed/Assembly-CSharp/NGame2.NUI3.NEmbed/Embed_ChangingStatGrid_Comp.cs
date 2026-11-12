using NGame2.NUI3.NCommon;
using NShared;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI3.NEmbed;

public class Embed_ChangingStatGrid_Comp : MonoBehaviour
{
	public int Margin;

	public ChangingStatListComponent.ColorInfo ColorInfo_StatName;

	public ChangingStatListComponent.ColorInfo ColorInfo_Value_Before;

	public ChangingStatListComponent.ColorInfo ColorInfo_Value_AfterPlus;

	public ChangingStatListComponent.ColorInfo ColorInfo_Value_AfterMinus;

	public UIPanel Panel;

	public UIScrollView ScrollView;

	private ChangingStatListComponent _statListComponent;

	public void ResetUi()
	{
		FactoryBase.SafeReleaseComponent(ref _statListComponent);
	}

	public void InitUi(EquipItemInfo beforeEquipInfo, StatData beforeStatData, EquipItemInfo afterEquipInfo, StatData afterStatData)
	{
		ResetUi();
		_statListComponent = ChangingStatListComponent.Create(Panel, Margin);
		_statListComponent.AddStatChange(beforeStatData, afterStatData, ColorInfo_StatName, ColorInfo_Value_Before, ColorInfo_Value_AfterPlus, ColorInfo_Value_AfterMinus);
		ItemData<EquipItemData> data = GameRoot.Instance.ItemDataContainer.GetData<EquipItemData>(beforeEquipInfo.ItemIndex);
		if (data != null)
		{
			for (int i = 1; i < 10; i++)
			{
				int fixedOptionIndex = data.GetSubData<EquipItemData>().GetFixedOptionIndex(i);
				if (fixedOptionIndex == 0)
				{
					break;
				}
				_statListComponent.AddOptionChangeSkill(fixedOptionIndex, beforeEquipInfo.Star, afterEquipInfo.Star, ColorInfo_Value_Before, ColorInfo_Value_AfterPlus, ColorInfo_Value_AfterMinus);
			}
		}
		foreach (EquipItemOptionInfo optionInfo in beforeEquipInfo.GetOptionInfoList())
		{
			_statListComponent.AddOptionChange(optionInfo, beforeEquipInfo.Star, afterEquipInfo.Star, ColorInfo_StatName, ColorInfo_Value_Before, ColorInfo_Value_AfterPlus, ColorInfo_Value_AfterMinus);
		}
		if ((bool)ScrollView)
		{
			ScrollView.ResetPosition();
		}
	}
}
