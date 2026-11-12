using NGame2.NUI.NComponent2;
using NShared;
using NVespa.NGlobal;
using UnityEngine;

namespace NGame2.NUI.NWindow.NSoulWeapon;

public class BreakSoulWeaponPopup : BaseWindow
{
	public interface IListener
	{
		void BreakSoulWeapon(EquipItemInfo equipItemInfo);
	}

	public static string PrefabPath = "Resources/Prefab2/Window/SoulWeapon/Popup/BreakSoulWeaponPopup";

	public BreakSoulWeaponMaterialInfo BreakSoulWeaponMaterialInfo;

	public Transform SoulWeaponItemIconComponentRootTransform;

	public UILabel BreakWarningLabel;

	private IListener _iListener;

	private ItemIconComponent _soulWeapontemIconComponent;

	private EquipItemInfo _equipItemInfo;

	public void Open(IListener iListener, EquipItemInfo equipItemInfo)
	{
		Open();
		Init(iListener, equipItemInfo);
	}

	public override void Close()
	{
		base.Close();
	}

	private void Init(IListener iListener, EquipItemInfo equipItemInfo)
	{
		_iListener = iListener;
		_equipItemInfo = equipItemInfo;
		InitSoulWeapon();
		SetBreakWarningLabel(equipItemInfo);
		if (equipItemInfo != null)
		{
			BreakSoulWeaponMaterialInfo.Open(equipItemInfo.SoulWeaponInfo);
		}
	}

	private void InitSoulWeapon()
	{
		if (SoulWeaponItemIconComponentRootTransform == null || _equipItemInfo == null)
		{
			return;
		}
		if (_soulWeapontemIconComponent == null)
		{
			_soulWeapontemIconComponent = ItemIconComponent.PrefabAuto.Factory.Create(SoulWeaponItemIconComponentRootTransform);
			if (_soulWeapontemIconComponent == null)
			{
				return;
			}
			Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_soulWeapontemIconComponent.transform);
		}
		_soulWeapontemIconComponent.Open(_equipItemInfo, null);
	}

	private void SetBreakWarningLabel(EquipItemInfo equipItemInfo)
	{
		UIHelper.SetLabelText(BreakWarningLabel, "-");
		if (equipItemInfo != null && equipItemInfo.ItemData != null)
		{
			UIHelper.SetLabelText(BreakWarningLabel, LocalizationManager.GetStringDirect("SOULWEPAON_INFO_BRAEK_POPUP", equipItemInfo.ItemData.Name));
		}
	}

	public void OnClickYes()
	{
		if (_equipItemInfo != null && _equipItemInfo.IsSoulWeapon() && _iListener != null)
		{
			_iListener.BreakSoulWeapon(_equipItemInfo);
			Close();
		}
	}
}
