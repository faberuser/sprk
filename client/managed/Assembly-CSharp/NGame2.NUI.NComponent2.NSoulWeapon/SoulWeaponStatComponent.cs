using NGame2.NUI.NWindow.NSoulWeapon;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NSoulWeapon;

public class SoulWeaponStatComponent : UIScrollItemBehaviour, InitOnCreate
{
	public new interface IListener
	{
		void OnClick(SoulWeaponInfo soulWeaponInfo);
	}

	public static string PrefabPath = "Resources/Prefab2/Component/SoulWeapon/SoulWeaponStatComponent";

	public static PrefabAuto<SoulWeaponStatComponent> PrefabAuto = new PrefabAuto<SoulWeaponStatComponent>(PrefabPath);

	public SoulWeaponStatInfo SoulWeaponStatInfo;

	public SoulWeaponTotalStatInfo SoulWeaponTotalStatInfo;

	public Transform StatProgressRootTransform;

	private SoulWeaponStatProgressComponent _statProgressComponent;

	private IListener _iListener;

	private SoulWeaponInfo _soulWeaponInfo;

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		FactoryBase.SafeReleaseComponent(ref _statProgressComponent);
	}

	public void Open(IListener iListener, SoulWeaponInfo soulWeaponInfo)
	{
		Init(iListener, soulWeaponInfo);
	}

	private void Init(IListener iListener, SoulWeaponInfo soulWeaponInfo)
	{
		_iListener = iListener;
		_soulWeaponInfo = soulWeaponInfo;
		SoulWeaponStatInfo.Open(_soulWeaponInfo);
		SoulWeaponTotalStatInfo.Open(_soulWeaponInfo, isRenew: false);
		SetStatProgressComponent();
	}

	private void SetStatProgressComponent()
	{
		if (_soulWeaponInfo == null)
		{
			return;
		}
		if (_statProgressComponent == null)
		{
			_statProgressComponent = SoulWeaponStatProgressComponent.PrefabAuto.Factory.Create(StatProgressRootTransform);
			if (_statProgressComponent == null)
			{
				return;
			}
			Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_statProgressComponent.transform);
		}
		_statProgressComponent.Open(_soulWeaponInfo, isRenew: false);
	}

	public void OnClick()
	{
		if (_soulWeaponInfo != null && _iListener != null)
		{
			_iListener.OnClick(_soulWeaponInfo);
		}
	}
}
