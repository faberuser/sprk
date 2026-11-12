using System;
using NGame2.NUI.NManager;
using NShared;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class GlobalCenterMessage : BaseWindow, IGlobalWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/MessagePopup/GlobalCenterMessage";

	private static GlobalWindowPrefab<GlobalCenterMessage> _windowPrefab = new GlobalWindowPrefab<GlobalCenterMessage>("Resources/Prefab2/Window/MessagePopup/GlobalCenterMessage", GlobalUIRoot.ELayerType.GlobalUI);

	public UILabel Label_Message;

	public UISprite Sprite_Back;

	public float DelayTime;

	private float _curDelayTime;

	public static void OpenError(Enum error)
	{
		if (!(error.GetType() == typeof(BaseResultType)) || (BaseResultType)(object)error != BaseResultType.Disabled)
		{
			Open(LocalizationManager.GetString(error));
		}
	}

	public static void Open(string message)
	{
		if (!String_Extension_Method.IsNullOrEmpty(message))
		{
			GlobalCenterMessage globalCenterMessage = GlobalAutoBehaviour<PopupManegement>.instance.GlobalPopupExist<GlobalCenterMessage>();
			if (globalCenterMessage != null)
			{
				globalCenterMessage.Close2(immediately: true);
			}
			_windowPrefab.instance.OpenInternal(message);
			GlobalAutoBehaviour<PopupManegement>.instance.AddGlobal(_windowPrefab);
		}
	}

	public static void Open(Enum key)
	{
		Open(LocalizationManager.GetString(key));
	}

	protected void OpenInternal(string message)
	{
		base.Open();
		UIHelper.SetLabelText(Label_Message, message);
		_curDelayTime = 0f;
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		if (IsOpen())
		{
			_curDelayTime += Time.deltaTime;
			if (_curDelayTime > DelayTime)
			{
				Close();
				_curDelayTime = 0f;
			}
		}
	}

	public static void OnSceneChanged()
	{
		if (!_windowPrefab.IsOpen())
		{
			_windowPrefab.ReleaseWindow();
			_windowPrefab.ReleasePrefab();
		}
	}
}
