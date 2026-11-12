using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class EquipOptionSelectConfirmPopupComponent : UIScrollItemBehaviour, InitOnCreate
{
	public const string PrefabPath = "Resources/Prefab2/Component/ShopItem/EquipOptionSelectConfirmPopupComponent";

	public static PrefabAuto<EquipOptionSelectConfirmPopupComponent> PrefabAuto = new PrefabAuto<EquipOptionSelectConfirmPopupComponent>("Resources/Prefab2/Component/ShopItem/EquipOptionSelectConfirmPopupComponent");

	public UILabel Lbl_Pre;

	public UILabel Lbl_Option;

	public void Open(int slotIndex, int optionIndex)
	{
		EquipOptionData data = GameRoot.Instance.EquipOptionDataContainer.GetData(optionIndex);
		if (data != null)
		{
			int equipItemOptionLevel = GameRoot.Instance.EquipItemLevelDataContainer.GetEquipItemOptionLevel(slotIndex);
			string stringDirect = LocalizationManager.GetStringDirect("EQUIP_GRADE_STAT_MASSAGE", equipItemOptionLevel);
			UIHelper.SetLabelText(Lbl_Pre, stringDirect);
			int optionValueMin = data.GetOptionValueMin(0);
			int optionValueMax = data.GetOptionValueMax(0);
			string optionDisplay = StatData.GetOptionDisplay(data.OptionType, optionValueMin, optionValueMax, "~");
			UIHelper.SetLabelText(Lbl_Option, optionDisplay);
		}
	}

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		Reset();
	}

	public void Close()
	{
		Reset();
	}

	private void Reset()
	{
	}
}
