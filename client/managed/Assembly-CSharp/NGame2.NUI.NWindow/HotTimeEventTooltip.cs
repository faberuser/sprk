namespace NGame2.NUI.NWindow;

public class HotTimeEventTooltip : SimpleTooltip<TooltipTextInfo>
{
	public const string PrefabPath = "Resources/Prefab2/Window/Tooltip/HotTimeEventTooltip";

	private static WindowPrefab<HotTimeEventTooltip> _instance = new WindowPrefab<HotTimeEventTooltip>("Resources/Prefab2/Window/Tooltip/HotTimeEventTooltip", GameUIRoot.ELayerType.UI);

	public static bool IsVisible(UIWidget ownerWidget)
	{
		return SimpleTooltip<TooltipTextInfo>.IsVisible(_instance, ownerWidget);
	}

	public static void Open(UIWidget ownerWidget, TooltipTextInfo textInfo)
	{
		SimpleTooltip<TooltipTextInfo>.Open(_instance, ownerWidget, textInfo);
	}

	public static void Hide(UIWidget ownerWidget)
	{
		SimpleTooltip<TooltipTextInfo>.Hide(_instance, ownerWidget);
	}

	public static void Refresh(UIWidget ownerWidget, TooltipTextInfo textInfo)
	{
		SimpleTooltip<TooltipTextInfo>.Refresh(_instance, ownerWidget, textInfo);
	}
}
