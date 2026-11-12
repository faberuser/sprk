namespace NGame2.NUI.NWindow;

public class TranscendSkillIconTooltip : SimpleTooltip<TooltipTextInfo>
{
	public const string PrefabPath = "Resources/Prefab2/Window/Tooltip/TranscendSkillIconTooltip";

	private static WindowPrefab<TranscendSkillIconTooltip> _instance = new WindowPrefab<TranscendSkillIconTooltip>("Resources/Prefab2/Window/Tooltip/TranscendSkillIconTooltip", GameUIRoot.ELayerType.UI);

	public static TranscendSkillIconTooltip instance => _instance.instance;

	public static void Open(UIWidget ownerWidget, TooltipTextInfo textInfo)
	{
		SimpleTooltip<TooltipTextInfo>.Open(_instance, ownerWidget, textInfo);
	}

	public static void Hide(UIWidget ownerWidget)
	{
		SimpleTooltip<TooltipTextInfo>.Hide(_instance, ownerWidget);
	}
}
