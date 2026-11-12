using UnityEngine;

namespace NGame2.NUI.NWindow;

public class StateIconTooltip : SimpleTooltip<StateIconTooltipTextInfo>
{
	public const string PrefabPath = "Resources/Prefab2/Window/Tooltip/StateIconTooltip";

	private static WindowPrefab<StateIconTooltip> _instance = new WindowPrefab<StateIconTooltip>("Resources/Prefab2/Window/Tooltip/StateIconTooltip", GameUIRoot.ELayerType.UI);

	public UILabel Desc1Label;

	public static StateIconTooltip instance => _instance.instance;

	public static void Open(UIWidget ownerWidget, StateIconTooltipTextInfo textInfo)
	{
		SimpleTooltip<StateIconTooltipTextInfo>.Open(_instance, ownerWidget, textInfo);
	}

	public static void Hide(UIWidget ownerWidget)
	{
		SimpleTooltip<StateIconTooltipTextInfo>.Hide(_instance, ownerWidget);
	}

	public override void Reset()
	{
		base.Reset();
	}

	public static void SetScale(float fPer)
	{
		_instance.instance.transform.localScale = new Vector3(fPer, fPer, 1f);
	}

	protected override void SetupText(StateIconTooltipTextInfo textInfo)
	{
		base.SetupText(textInfo);
		UIHelper.SetLabelText(Desc1Label, textInfo.Desc1.Value);
		UIHelper.EnableGameObject(Desc1Label, !String_Extension_Method.IsNullOrEmpty(textInfo.Desc1.Value));
	}
}
