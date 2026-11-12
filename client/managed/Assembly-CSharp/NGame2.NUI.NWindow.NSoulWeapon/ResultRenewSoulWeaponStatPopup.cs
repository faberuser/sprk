using NShared;

namespace NGame2.NUI.NWindow.NSoulWeapon;

public class ResultRenewSoulWeaponStatPopup : CommonRenewSoulWeaponStatPopup
{
	public interface IListener
	{
		void RetryRenew(SoulWeaponInfo soulWeaponInfo, RequirePrice requirePrice);
	}

	public static string PrefabPath = "Resources/Prefab2/Window/SoulWeapon/Popup/ResultRenewSoulWeaponStatPopup";

	public SoulWeaponStatInfo OriginSoulWeaponStatInfo;

	public SoulWeaponStatInfo SoulWeaponStatInfo;

	public SoulWeaponTotalStatInfo OriginTotalStatInfo;

	public SoulWeaponTotalStatInfo TotalStatInfo;

	public UIGame3DWidget Game3DWidget;

	private IListener _iListener;

	public void Open(IListener iListener, SoulWeaponInfo soulWeaponInfo, SoulWeaponInfo resultSoulWeaponInfo)
	{
		Open(soulWeaponInfo);
		Init(iListener, resultSoulWeaponInfo);
	}

	public override void Close()
	{
		base.Close();
		_iListener = null;
	}

	private void Init(IListener iListener, SoulWeaponInfo resultSoulWeaponInfo)
	{
		_iListener = iListener;
		OriginSoulWeaponStatInfo.Open(base.SoulWeaponInfo);
		SoulWeaponStatInfo.Open(resultSoulWeaponInfo);
		OriginTotalStatInfo.Open(base.SoulWeaponInfo, isRenew: false);
		TotalStatInfo.Open(resultSoulWeaponInfo, isRenew: false);
		SetStatProgressComponent(resultSoulWeaponInfo, ref _statProgressComponent, ref StatProgressRootTransform, isRenew: false);
		PlayAnim();
	}

	private void PlayAnim()
	{
		if (Game3DWidget != null)
		{
			Game3DWidget.Play(null);
			PlaySound("UI.SoulWeapon.ResultStatus");
		}
	}

	public void OnClickRetry()
	{
		if (_iListener != null)
		{
			_iListener.RetryRenew(SoulWeaponStatInfo.SoulWeaponInfo, RequirePrice);
		}
	}
}
