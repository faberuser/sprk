using System;
using NGame2.NUI.NManager;
using NVespa.NNGUIExtension;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow;

public class EmptyEffectPopup : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/RewardPopup/EmptyEffectPopup";

	public UIPlayAssetSound RandomBoxSound;

	public UIGame3DWidget RandomBoxEffect;

	private bool isEffectPlaying;

	private Action _onClose;

	public new static void Open(Action onClose)
	{
		WindowPrefab<EmptyEffectPopup> windowPrefab = new WindowPrefab<EmptyEffectPopup>("Resources/Prefab2/Window/RewardPopup/EmptyEffectPopup", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal(onClose);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	public void OpenInternal(Action onClose)
	{
		base.Open();
		isEffectPlaying = true;
		_onClose = onClose;
		RandomBoxEffect.Stop(immediatelyClear: true);
		RandomBoxEffect.Play(delegate
		{
			isEffectPlaying = false;
		});
		RandomBoxSound.Play();
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		if (!isEffectPlaying && _onClose != null)
		{
			Action onClose = _onClose;
			_onClose = null;
			base.Close();
			Vespa_System_Action_Extension_Method.fire(onClose);
		}
	}

	public override void Reset()
	{
		base.Reset();
		isEffectPlaying = false;
		_onClose = null;
		RandomBoxEffect.Stop(immediatelyClear: true);
	}
}
