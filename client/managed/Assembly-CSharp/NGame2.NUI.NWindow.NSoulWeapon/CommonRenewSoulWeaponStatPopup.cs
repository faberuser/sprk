using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NComponent2.NSoulWeapon;
using NGameAsset;
using NShared;
using NVespa;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow.NSoulWeapon;

public class CommonRenewSoulWeaponStatPopup : BaseWindow
{
	public Transform SoulWeaponItemIconComponetRootTransform;

	public RequirePrice RequirePrice;

	public Transform StatProgressRootTransform;

	protected SoulWeaponStatProgressComponent _statProgressComponent;

	private ItemIconComponent _soulWeaponItemIconComponent;

	public SoulWeaponInfo SoulWeaponInfo { get; protected set; }

	protected int RenewReqPrice
	{
		get
		{
			if (SoulWeaponInfo == null)
			{
				return -1;
			}
			int[] valueArray = GameRoot.Instance.ConstantDataContainer.GetValueArray<int>("SoulWeaponRenewGem");
			if (__Extension_Method_ICollection__.IsNullOrEmpty(valueArray))
			{
				return -1;
			}
			if (valueArray.Length > SoulWeaponInfo.RenewOptionCount)
			{
				return valueArray[SoulWeaponInfo.RenewOptionCount];
			}
			return valueArray[^1];
		}
	}

	public void Open(SoulWeaponInfo soulWeaponInfo)
	{
		Open();
		Init(soulWeaponInfo);
	}

	public override void Close()
	{
		base.Close();
		FactoryBase.SafeReleaseComponent(ref _soulWeaponItemIconComponent);
		FactoryBase.SafeReleaseComponent(ref _statProgressComponent);
		RequirePrice.Close();
	}

	private void Init(SoulWeaponInfo soulWeaponInfo)
	{
		SoulWeaponInfo = soulWeaponInfo;
		SetSoulWeaponItemIconComponent();
		RequirePrice.Open(RenewReqPrice, 0L);
	}

	private void SetSoulWeaponItemIconComponent()
	{
		if (SoulWeaponInfo == null)
		{
			return;
		}
		NGame2.NAccount.UserEquipItemManager equipItemManager = GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager;
		if (equipItemManager == null)
		{
			return;
		}
		EquipItemInfo equipItemInfo = equipItemManager.Get(SoulWeaponInfo.EquipItemSlotIndex);
		if (equipItemInfo == null)
		{
			return;
		}
		if (_soulWeaponItemIconComponent == null)
		{
			_soulWeaponItemIconComponent = ItemIconComponent.PrefabAuto.Factory.Create(SoulWeaponItemIconComponetRootTransform);
			if (_soulWeaponItemIconComponent == null)
			{
				return;
			}
			Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_soulWeaponItemIconComponent.transform);
		}
		_soulWeaponItemIconComponent.Open(equipItemInfo, null);
	}

	protected void SetStatProgressComponent(SoulWeaponInfo soulWeaponInfo, ref SoulWeaponStatProgressComponent statProgressComponent, ref Transform rootTranform, bool isRenew, SoulWeaponStatProgressComponent.BgColorType bgColorType = SoulWeaponStatProgressComponent.BgColorType.Black)
	{
		if (soulWeaponInfo == null || rootTranform == null)
		{
			return;
		}
		if (statProgressComponent == null)
		{
			statProgressComponent = SoulWeaponStatProgressComponent.PrefabAuto.Factory.Create(rootTranform);
			if (statProgressComponent == null)
			{
				return;
			}
			Vespa_Unity_Transform_Extension_Method.ResetLocalTM(statProgressComponent.transform);
		}
		statProgressComponent.Open(soulWeaponInfo, isRenew, bgColorType);
	}

	protected void PlaySound(string soundCode)
	{
		SoundStock stock = new SoundStock
		{
			Code = soundCode
		};
		SingletonAuto<SoundContainer>.instance.PlayOnce(stock, 10f);
	}
}
