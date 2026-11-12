namespace NGame2.NUI.NWindow;

public class GuildConstructionResourceTooltip : SimpleTooltip<TooltipTextInfo>
{
	public const string PrefabPath = "Resources/Prefab2/Window/Tooltip/GuildConstructionResourceTooltip";

	private static WindowPrefab<GuildConstructionResourceTooltip> _instance = new WindowPrefab<GuildConstructionResourceTooltip>("Resources/Prefab2/Window/Tooltip/GuildConstructionResourceTooltip", GameUIRoot.ELayerType.UI);

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

	protected override void SetupText(TooltipTextInfo textInfo)
	{
		UIHelper.SetLabelText(NameLabel, textInfo.Name.Value);
		UIHelper.SetLabelText(DescLabel, textInfo.Desc.Value);
	}
}
