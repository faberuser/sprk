using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class ProbabilityInfoComponent : UIScrollItemBehaviour, InitOnCreate
{
	public const string PrefabPath = "Resources/Prefab2/Component/PayShop2/PayShopProbabilityInfoComponent";

	public static PrefabAuto<ProbabilityInfoComponent> PrefabAuto = new PrefabAuto<ProbabilityInfoComponent>("Resources/Prefab2/Component/PayShop2/PayShopProbabilityInfoComponent");

	public UILabel Label_Title;

	public UILabel Label_Count;

	public UILabel Label_Rate;

	public ItemTooltipComponent Tooltip;

	public UIWidget WidgetThis;

	private bool _unUseTooltip;

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		_unUseTooltip = false;
	}

	public void Open(ProbabilityBaseData probabilityData, bool isShowCountLabel)
	{
		if (isShowCountLabel)
		{
			Open(probabilityData.ComponentTitle, probabilityData.ComponentCount, probabilityData.ComponentRate, probabilityData.Index);
		}
		else
		{
			Open(probabilityData.ComponentTitle, probabilityData.ComponentRate, probabilityData.Index);
		}
	}

	public void Open(string title, int count, string rate, int itemIndex)
	{
		Open(title, rate, itemIndex);
		UIHelper.EnableGameObject(Label_Count, enable: true);
		UIHelper.SetLabelText(Label_Count, count);
	}

	public void Open(string title, string rate, int itemIndex)
	{
		string stringDirect = LocalizationManager.GetStringDirect(title);
		UIHelper.SetLabelText(Label_Title, stringDirect);
		UIHelper.SetLabelText(Label_Rate, rate);
		UIHelper.EnableGameObject(Label_Count, enable: false);
		Tooltip.Open(WidgetThis, "", itemIndex);
	}

	public void OpenGachaSelectShopTabInfo(ProbabilityBaseData probabilityData)
	{
		_unUseTooltip = true;
		Open(probabilityData.ComponentTitle, probabilityData.ComponentRate, probabilityData.Index);
	}

	public void OnShowTooltip()
	{
		if (!_unUseTooltip && Tooltip.ItemIndex != 0)
		{
			Tooltip.OnShowTooltipByItemIndex(forceSetup: true);
		}
	}

	public void OnHideTooltip()
	{
		if (!_unUseTooltip)
		{
			Tooltip.OnHideTooltip();
		}
	}
}
