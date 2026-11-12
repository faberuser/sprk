using NGame2.NUI.NManager.NBooster;
using NShared;
using NVespa.NGlobal;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class LegendCostumeTotalBuffComponent : LinkFactoryComponent, InitOnCreate
{
	public const string PrefabPath = "Prefab2/Component/LegendCostume/LegendCostumeTotalBuffComponent";

	public static PrefabAuto<LegendCostumeTotalBuffComponent> PrefabAuto = new PrefabAuto<LegendCostumeTotalBuffComponent>("Prefab2/Component/LegendCostume/LegendCostumeTotalBuffComponent");

	public UILabel LabelName;

	public UILabel LabelValue;

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
	}

	public void Open(BoosterInfo boosterInfo)
	{
		Init(boosterInfo);
	}

	private void Init(BoosterInfo boosterInfo)
	{
		BoosterDefineData data = GameRoot.Instance.BoosterDefineDataContainer.GetData(boosterInfo.BonusType.ToString());
		UIHelper.SetLabelText(LabelName, LocalizationManager.GetStringDirect(data.PopUpBuffCategory));
		UIHelper.SetLabelText(LabelValue, LocalizationManager.GetStringDirect(data.PopUpBuffDetail, boosterInfo.Value));
	}
}
