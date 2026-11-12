using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2.NEventForge;

public class EventForgePercentageInfoComponent : UIScrollItemBehaviour
{
	public const string PrefabPath = "Prefab2/Component/EventForge/EventForgePercentageInfoComponent";

	public static PrefabAuto<EventForgePercentageInfoComponent> PrefabAuto = new PrefabAuto<EventForgePercentageInfoComponent>("Prefab2/Component/EventForge/EventForgePercentageInfoComponent");

	public UILabel ReinforcementLevelLabel;

	public UILabel SuccessPercentageLabel;

	public UILabel FailPercentageLabel;

	public UILabel BreakPercentageLabel;

	public void Open(EventForgeData data)
	{
		Init(data);
	}

	private void Init(EventForgeData data)
	{
		if (data != null)
		{
			SetReinforcementLevelLabel(data.EnchantLevel);
			SetSuccessPercentageLabel(data.SuccessPercentage);
			SetFailPercentageLabel(data.FailPercentage);
			SetBreakPercentageLabel(data.BreakPercentage);
		}
	}

	private void SetReinforcementLevelLabel(int reinforcementLevel)
	{
		UIHelper.SetLabelText(ReinforcementLevelLabel, "+" + reinforcementLevel);
	}

	private void SetSuccessPercentageLabel(float percentage)
	{
		UIHelper.SetLabelText(SuccessPercentageLabel, GetPercentageStr(percentage));
	}

	private void SetFailPercentageLabel(float percentage)
	{
		UIHelper.SetLabelText(FailPercentageLabel, GetPercentageStr(percentage));
	}

	private void SetBreakPercentageLabel(float percentage)
	{
		UIHelper.SetLabelText(BreakPercentageLabel, GetPercentageStr(percentage));
	}

	private string GetPercentageStr(float percentage)
	{
		if (percentage <= 0f)
		{
			return "-";
		}
		return percentage + "%";
	}
}
