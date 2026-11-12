using NGame2.NUI.NWindow.NSoulWeapon;
using NShared;

namespace NGame2.NUI.NWindow.NTooltip;

public class SoulWeaponStatTooltip : SimpleTooltip<TooltipTextInfo>
{
	public const string PrefabPath = "Resources/Prefab2/Window/Tooltip/SoulWeaponStatTooltip";

	private static WindowPrefab<SoulWeaponStatTooltip> _instance = new WindowPrefab<SoulWeaponStatTooltip>("Resources/Prefab2/Window/Tooltip/SoulWeaponStatTooltip", GameUIRoot.ELayerType.UI);

	public SoulWeaponTotalStatInfo MaxTotalStatInfo;

	public SoulWeaponTotalStatInfo MinTotalStatInfo;

	private SoulWeaponInfo _soulWeaponInfo;

	public static void Open(UIWidget ownerWidget, TooltipTextInfo textInfo, SoulWeaponInfo soulWeaponInfo)
	{
		_instance.instance.SetSoulWeaponInfo(soulWeaponInfo);
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

	public override void Reset()
	{
		base.Reset();
	}

	protected override void SetupText(TooltipTextInfo textInfo)
	{
		MaxTotalStatInfo.Open(_soulWeaponInfo, isRenew: false, EStatType.Max);
		MinTotalStatInfo.Open(_soulWeaponInfo, isRenew: false, EStatType.Min);
	}

	protected override void OnClose()
	{
		base.OnClose();
	}

	private void SetSoulWeaponInfo(SoulWeaponInfo soulWeaponInfo)
	{
		_soulWeaponInfo = soulWeaponInfo;
	}
}
