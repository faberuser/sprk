using System;
using System.Collections;
using NGame2.NAccount;
using NGameAsset;
using NShared;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow.NSoulWeapon;

public class SoulWeaponReinforceResultWindow : BaseWindow
{
	public interface IListener
	{
		void CloseResultWindow(SoulWeaponInfo soulWeaponInfo);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/SoulWeapon/SoulWeaponReinforceResultWindow";

	public NodeTreeUIPlayer NodeTreePlayer;

	public SoulWeaponStatInfo SoulWeaponStatInfo;

	public SoulWeaponStatInfo ReinforceSoulWeaponStatInfo;

	public UILabel BeforeReinforceLevelLabel;

	public UILabel AfterReinforceLevelLabel;

	public Transform SoulWeaponItemIconRootTranform;

	public Animator SuccessAnimator;

	public Animator FailAnimator;

	private IListener _iListener;

	private SoulWeaponInfo _resultSoulWeaponInfo;

	private Action _onClose;

	public void Open(IListener iListener, SoulWeaponInfo soulWeaponInfo, bool isSuccess, Action onClose)
	{
		VespaCoroutine.Start(OpenAsync(iListener, soulWeaponInfo, isSuccess, onClose));
	}

	private IEnumerator OpenAsync(IListener iListener, SoulWeaponInfo soulWeaponInfo, bool isSuccess, Action onClose)
	{
		yield return new WaitForSeconds(1f);
		Open(delegate
		{
			Init(iListener, soulWeaponInfo, isSuccess, onClose);
		}, null);
		yield return null;
	}

	private void Init(IListener iListener, SoulWeaponInfo soulWeaponInfo, bool isSuccess, Action onClose)
	{
		_iListener = iListener;
		_onClose = onClose;
		SetStatInfo(soulWeaponInfo);
		SetReinforceStatInfo(soulWeaponInfo?.EquipItemSlotIndex ?? 0);
		SetNodeTree(_resultSoulWeaponInfo, isSuccess);
		PlayAnim(isSuccess);
	}

	private void SetStatInfo(SoulWeaponInfo soulWeaponInfo)
	{
		SoulWeaponStatInfo.Open(soulWeaponInfo);
		UIHelper.SetLabelText(BeforeReinforceLevelLabel, "-");
		if (soulWeaponInfo != null)
		{
			UIHelper.SetLabelText(BeforeReinforceLevelLabel, soulWeaponInfo.ReinforceLevel);
		}
	}

	private void SetReinforceStatInfo(int equpItemSlotIndex)
	{
		SoulWeaponInfo soulWeaponInfo = GlobalAutoBehaviour<AccountManager>.instance.SoulWeaponManager.Get(equpItemSlotIndex);
		if (soulWeaponInfo != null)
		{
			_resultSoulWeaponInfo = soulWeaponInfo;
			ReinforceSoulWeaponStatInfo.Open(_resultSoulWeaponInfo);
			UIHelper.SetLabelText(AfterReinforceLevelLabel, "-");
			if (soulWeaponInfo != null)
			{
				UIHelper.SetLabelText(AfterReinforceLevelLabel, _resultSoulWeaponInfo.ReinforceLevel);
			}
		}
	}

	private void SetNodeTree(SoulWeaponInfo soulWeaponInfo, bool isSuccess)
	{
		EquipItemInfo equipItemBySlotIndex = GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.GetEquipItemBySlotIndex(soulWeaponInfo.EquipItemSlotIndex);
		SoulWeaponNodeTreeType type = (isSuccess ? SoulWeaponNodeTreeType.ReinforceSuccess : SoulWeaponNodeTreeType.ReinforceFail);
		UIHelper.PlaySoulWeaponNodeTree(NodeTreePlayer, equipItemBySlotIndex, type, null, null);
	}

	private void PlayNodeTree()
	{
		if (NodeTreePlayer != null)
		{
			NodeTreePlayer.PlayOnce();
		}
	}

	private void PlayAnim(bool isSuccess)
	{
		if (!(SuccessAnimator == null) && !(FailAnimator == null))
		{
			UIHelper.EnableGameObject(SuccessAnimator.transform, isSuccess);
			UIHelper.EnableGameObject(FailAnimator.transform, !isSuccess);
			if (isSuccess)
			{
				SuccessAnimator.Play("Anim_Success01");
				PlaySound("UI.SoulWeapon.EtherSuccess");
			}
			else if (!isSuccess)
			{
				FailAnimator.Play("Anim_Fail01");
				PlaySound("UI.SoulWeapon.EtherFailed");
			}
		}
	}

	private void PlaySound(string soundCode)
	{
		SoundStock stock = new SoundStock
		{
			Code = soundCode
		};
		SingletonAuto<SoundContainer>.instance.PlayOnce(stock, 10f);
	}

	public void OnClickClose()
	{
		if (_iListener != null)
		{
			_iListener.CloseResultWindow(_resultSoulWeaponInfo);
		}
		if (NodeTreePlayer != null)
		{
			NodeTreePlayer.Close();
		}
		Close();
		Vespa_System_Action_Extension_Method.fire(_onClose);
	}
}
