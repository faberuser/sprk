using UnityEngine;

namespace NGame2.NUI.NWindow;

public class OrdealArenaBuffTooltip : SimpleTooltip<OrdealArenaBuffTooltipInfo>
{
	public const string PrefabPath = "Resources/Prefab2/Window/Tooltip/OrdealArenaBuffTooltip";

	private static WindowPrefab<OrdealArenaBuffTooltip> _instance = new WindowPrefab<OrdealArenaBuffTooltip>("Resources/Prefab2/Window/Tooltip/OrdealArenaBuffTooltip", GameUIRoot.ELayerType.UI);

	public UILabel Desc1Label;

	public static OrdealArenaBuffTooltip instance => _instance.instance;

	public static void Open(UIWidget ownerWidget, OrdealArenaBuffTooltipInfo textInfo)
	{
		SimpleTooltip<OrdealArenaBuffTooltipInfo>.Open(_instance, ownerWidget, textInfo);
	}

	public static void Hide(UIWidget ownerWidget)
	{
		SimpleTooltip<OrdealArenaBuffTooltipInfo>.Hide(_instance, ownerWidget);
	}

	public override void Reset()
	{
		base.Reset();
	}

	public static void SetScale(float fPer)
	{
		_instance.instance.transform.localScale = new Vector3(fPer, fPer, 1f);
	}

	protected override void SetupText(OrdealArenaBuffTooltipInfo textInfo)
	{
		base.SetupText(textInfo);
		UIHelper.SetLabelText(Desc1Label, textInfo.Desc1.Value);
		UIHelper.EnableGameObject(Desc1Label, !String_Extension_Method.IsNullOrEmpty(textInfo.Desc1.Value));
	}
}
