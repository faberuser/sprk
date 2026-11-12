using NGame2.NUI.NWindow;
using NShared;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class StateIconComponent : MonoBehaviour, InitOnCreate
{
	public const string PrefabPath = "Resources/Prefab2/Component/Icon/StateIconComponent";

	public static PrefabAuto<StateIconComponent> PrefabAuto = new PrefabAuto<StateIconComponent>("Resources/Prefab2/Component/Icon/StateIconComponent");

	public UIWidget WidgetThis;

	public UIBundleLoader uiBundleLoader;

	private StateToolTipData _stateTooltipData;

	private string iconPath = string.Empty;

	public void Open(StateData stateData)
	{
		_stateTooltipData = GameRoot.Instance.StateToolTipDataContainer.GetStateToolTipData(stateData);
		if (_stateTooltipData != null)
		{
			Open(_stateTooltipData);
		}
	}

	public void Open(StateToolTipData stateToolTipData)
	{
		_stateTooltipData = stateToolTipData;
		iconPath = _stateTooltipData.IconPath;
		SetIcon();
	}

	public void SetIcon()
	{
		if (uiBundleLoader != null && !String_Extension_Method.IsNullOrEmpty(iconPath))
		{
			UIHelper.SetBundleTexture(uiBundleLoader, iconPath);
		}
	}

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
	}

	public void OnShowTooltip()
	{
		StateIconTooltipTextInfo stateIconTooltipTextInfo = new StateIconTooltipTextInfo();
		stateIconTooltipTextInfo.Name.Value = LocalizationManager.GetStringDirect(_stateTooltipData.StateName);
		stateIconTooltipTextInfo.Desc.Value = LocalizationManager.GetStringDirect(_stateTooltipData.StateDesc);
		stateIconTooltipTextInfo.Desc1.Value = LocalizationManager.GetStringDirect(_stateTooltipData.StateDesc);
		StateIconTooltip.Open(WidgetThis, stateIconTooltipTextInfo);
	}

	public void OnHideTooltip()
	{
		StateIconTooltip.Hide(WidgetThis);
	}
}
