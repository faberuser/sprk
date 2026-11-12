using System;
using System.Collections;
using NGame2.NUI;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NGame2.NUtil.NWrappedInfo;
using NVespa.NNGUIExtension;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NDispatchBattle;

public class PopupUIDispatchBattleFx : BaseWindow
{
	public enum AniState
	{
		Start,
		EndSuccess,
		EndFail
	}

	public const string PrefabPathPet = "Resources/Prefab2/Window/Pet/Popup/PetExploreStartFxPopup";

	public const string PREFAB_PATH = "Resources/Prefab2/Window/DispatchBattle/PopupUIDispatchBattleFx";

	public UIWidget FxRoot;

	public UIPlayAssetSound DispatchSound;

	private Action _onClose;

	private bool _isBlocked = true;

	public static void OpenPet(AniState state, Action onClose)
	{
		WindowPrefab<PopupUIDispatchBattleFx> windowPrefab = new WindowPrefab<PopupUIDispatchBattleFx>("Resources/Prefab2/Window/Pet/Popup/PetExploreStartFxPopup", GameUIRoot.ELayerType.UI);
		if (windowPrefab != null && !(windowPrefab.instance == null))
		{
			windowPrefab.instance.Open(null, state, onClose);
			GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
		}
	}

	public static void OpenGlobal(WCL_DispatchBattleInfo info, AniState state, Action onClose)
	{
		WindowPrefab<PopupUIDispatchBattleFx> windowPrefab = new WindowPrefab<PopupUIDispatchBattleFx>("Resources/Prefab2/Window/DispatchBattle/PopupUIDispatchBattleFx", GameUIRoot.ELayerType.UI);
		if (windowPrefab != null && !(windowPrefab.instance == null))
		{
			windowPrefab.instance.Open(info, state, onClose);
			GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
		}
	}

	public static void CloseGlobal()
	{
		PopupUIDispatchBattleFx popupUIDispatchBattleFx = GlobalAutoBehaviour<PopupManegement>.instance.FindWindow<PopupUIDispatchBattleFx>();
		if (popupUIDispatchBattleFx != null)
		{
			popupUIDispatchBattleFx.Close();
		}
	}

	public override void OnClosing()
	{
		Vespa_System_Action_Extension_Method.fire(_onClose);
		base.OnClosing();
	}

	public void Open(WCL_DispatchBattleInfo info, AniState state, Action onClose)
	{
		base.Open();
		_onClose = onClose;
		_isBlocked = true;
		if (DispatchSound != null)
		{
			DispatchSound.Play();
		}
	}

	public void OnClick()
	{
		if (!_isBlocked)
		{
			Close();
		}
	}

	private void OnEnable()
	{
		VespaCoroutine.Start(this, CoProcessingPlayFx());
	}

	private IEnumerator CoProcessingPlayFx()
	{
		yield return new WaitForSeconds(1f);
		_isBlocked = false;
		yield return new WaitForSeconds(1f);
		Close();
	}
}
