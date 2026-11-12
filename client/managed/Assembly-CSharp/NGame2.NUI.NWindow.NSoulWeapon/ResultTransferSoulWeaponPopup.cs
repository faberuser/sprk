using System.Collections;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NGameAsset;
using NShared;
using NVespa;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow.NSoulWeapon;

public class ResultTransferSoulWeaponPopup : BaseWindow
{
	public static string PrefabPath = "Resources/Prefab2/Window/SoulWeapon/Popup/ResultTransferSoulWeaponPopup";

	public Transform SoulWeaponRootTransform;

	public UIGame3DWidget Game3DWidget;

	public float StartAnimDelayTime;

	private ItemIconComponent _soulWeaponItemIconComponent;

	public void Open(SoulWeaponInfo soulWeaponInfo)
	{
		base.Open();
		Init(soulWeaponInfo);
	}

	public override void Close()
	{
		base.Close();
		FactoryBase.SafeReleaseComponent(ref _soulWeaponItemIconComponent);
	}

	private void Init(SoulWeaponInfo soulWeaponInfo)
	{
		SetSoulWeaponItemIconComponent(soulWeaponInfo, ref SoulWeaponRootTransform, ref _soulWeaponItemIconComponent);
		VespaCoroutine.Start(CoroutinePlayAnim());
	}

	private IEnumerator CoroutinePlayAnim()
	{
		yield return new WaitForSeconds(StartAnimDelayTime);
		if (Game3DWidget != null)
		{
			Game3DWidget.Play(null);
			PlaySound("UI.SoulWeapon.ReLocate");
		}
		yield return null;
	}

	private void PlaySound(string soundCode)
	{
		SoundStock stock = new SoundStock
		{
			Code = soundCode
		};
		SingletonAuto<SoundContainer>.instance.PlayOnce(stock, 1f);
	}

	private void SetSoulWeaponItemIconComponent(SoulWeaponInfo soulWeaponInfo, ref Transform rootTranform, ref ItemIconComponent itemIconComponent)
	{
		if (soulWeaponInfo == null)
		{
			return;
		}
		EquipItemInfo equipItemInfo = GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Get(soulWeaponInfo.EquipItemSlotIndex);
		if (equipItemInfo == null)
		{
			return;
		}
		if (itemIconComponent == null)
		{
			itemIconComponent = ItemIconComponent.PrefabAuto.Factory.Create(rootTranform);
			if (itemIconComponent == null)
			{
				return;
			}
			Vespa_Unity_Transform_Extension_Method.ResetLocalTM(itemIconComponent.transform);
		}
		itemIconComponent.Open(equipItemInfo, null);
	}
}
