using NGame2.NUI.NComponent2.NSoulWeapon;
using NGame2.NUI.NWindow.NTooltip;
using NShared;

namespace NGame2.NUI.NWindow.NSoulWeapon;

public class RenewSoulWeaponStatPopup : CommonRenewSoulWeaponStatPopup
{
	public interface IListener
	{
		void RenewSoulWeaponStat(SoulWeaponInfo soulWeaponInfo, RequirePrice requirePrice);
	}

	public static string PrefabPath = "Resources/Prefab2/Window/SoulWeapon/Popup/RenewSoulWeaponStatPopup";

	public SoulWeaponStatInfo SoulWeaponStatInfo;

	public SoulWeaponTotalStatInfo SoulWeaponTotalStatInfo;

	public UIWidget TooltipWidget;

	private IListener _iListener;

	private SoulWeaponInfo _soulWeaponInfo;

	public void Open(IListener iListener, SoulWeaponInfo soulWeaponInfo)
	{
		Open(soulWeaponInfo);
		Init(iListener, soulWeaponInfo);
	}

	public override void Close()
	{
		base.Close();
		_iListener = null;
	}

	private void Init(IListener iListener, SoulWeaponInfo soulWeaponInfo)
	{
		_iListener = iListener;
		_soulWeaponInfo = soulWeaponInfo;
		SoulWeaponStatInfo.Open(base.SoulWeaponInfo);
		SoulWeaponTotalStatInfo.Open(base.SoulWeaponInfo, isRenew: false);
		SetStatProgressComponent(base.SoulWeaponInfo, ref _statProgressComponent, ref StatProgressRootTransform, isRenew: false, SoulWeaponStatProgressComponent.BgColorType.Brown);
	}

	public void OnClickRenewSoulWeaponStat()
	{
		if (base.SoulWeaponInfo != null && _iListener != null)
		{
			_iListener.RenewSoulWeaponStat(base.SoulWeaponInfo, RequirePrice);
		}
	}

	public void ShowTooltip()
	{
		if (!(TooltipWidget == null))
		{
			TooltipTextInfo textInfo = new TooltipTextInfo();
			SoulWeaponStatTooltip.Open(TooltipWidget, textInfo, _soulWeaponInfo);
		}
	}

	public void HideTooltip()
	{
		if (!(TooltipWidget == null))
		{
			SoulWeaponStatTooltip.Hide(TooltipWidget);
		}
	}
}
