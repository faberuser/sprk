using NShared;
using NVespa.NGlobal;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NPet;

public class IncubatorProfilePanel : MonoBehaviour, IIncubatorManagePanel
{
	public UIWidget RootWidget;

	public UILabel Label_Grade;

	public UILabel Label_IncubatorName;

	public UILabel Label_IncubatorDesc;

	public UILabel Label_TimePer;

	[Header("Incubator Grage Color")]
	public Color Color_Grade_1;

	public Color Color_Grade_2;

	public Color Color_Grade_3;

	public Color Color_Grade_4;

	public Color Color_Grade_5;

	public Color Color_Grade_6;

	private IIncubatorDataProvider _dataProvider;

	public void Open(IIncubatorDataProvider dataProvider)
	{
		_dataProvider = dataProvider;
		SetIncubatorProfile();
	}

	public void SetIncubatorProfile()
	{
		PetIncubatorSlotInfo selectedSlotInfo = _dataProvider.GetSelectedSlotInfo();
		if (selectedSlotInfo != null)
		{
			PetIncubatorData data = GameRoot.Instance.PetIncubatorDataContainer.GetData(selectedSlotInfo.MountingIndex);
			int grade = data.Grade;
			string stringDirect = LocalizationManager.GetStringDirect($"ITEM_GRADE_{grade}");
			UIHelper.SetLabelText(Label_Grade, stringDirect);
			UIHelper.SetLabelColor(Label_Grade, GetGradeLabelColor(grade));
			string stringDirect2 = LocalizationManager.GetStringDirect(data.NameLocalKey);
			UIHelper.SetLabelText(Label_IncubatorName, stringDirect2);
			string stringDirect3 = LocalizationManager.GetStringDirect(data.DescLocalKey);
			UIHelper.SetLabelText(Label_IncubatorDesc, stringDirect3);
			string stringDirect4 = LocalizationManager.GetStringDirect("TRANSITION_SOULWEAPON_STAT_PERCENT", data.TimePer);
			UIHelper.SetLabelText(Label_TimePer, stringDirect4);
		}
	}

	private Color GetGradeLabelColor(int grade)
	{
		return grade switch
		{
			2 => Color_Grade_2, 
			3 => Color_Grade_3, 
			4 => Color_Grade_4, 
			5 => Color_Grade_5, 
			6 => Color_Grade_6, 
			_ => Color_Grade_1, 
		};
	}

	public void EnableRoot(bool isEnable)
	{
		UIHelper.EnableGameObject(RootWidget, isEnable);
	}

	public void Close()
	{
		_dataProvider = null;
	}
}
