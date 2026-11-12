namespace NGame2.NUI.NWindow;

public class KarmaBuffTooltip : SimpleTooltip<TooltipTextInfo>
{
	public const string PrefabPath = "Resources/Prefab2/Window/Tooltip/KarmaBuffTooltip";

	private static WindowPrefab<KarmaBuffTooltip> _instance = new WindowPrefab<KarmaBuffTooltip>("Resources/Prefab2/Window/Tooltip/KarmaBuffTooltip", GameUIRoot.ELayerType.UI);

	public static bool IsVisible(UIWidget ownerWidget)
	{
		return SimpleTooltip<TooltipTextInfo>.IsVisible(_instance, ownerWidget);
	}

	public static void Open(UIWidget ownerWidget, TooltipTextInfo textInfo)
	{
		SimpleTooltip<TooltipTextInfo>.Open(_instance, ownerWidget, textInfo, repositionWithCoroutine: true);
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
		UIHelper.EnableGameObject(TypeLabel, !String_Extension_Method.IsNullOrEmpty(textInfo.TypeText.Value));
		if (!String_Extension_Method.IsNullOrEmpty(textInfo.TypeText.Value))
		{
			UIHelper.SetLabelText(TypeLabel, textInfo.TypeText.Value);
		}
	}
}
