using System;
using NGame2.NUI.NManager;
using NShared;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class CenterMessage : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/MessagePopup/CenterMessage";

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

	public static CenterMessage Open(Enum e)
	{
		return Open(LocalizationManager.GetString(e));
	}

	public static CenterMessage Open(string message)
	{
		if (String_Extension_Method.IsNullOrEmpty(message))
		{
			return null;
		}
		IconCenterMessage iconCenterMessage = GlobalAutoBehaviour<PopupManegement>.instance.PopUpExist<IconCenterMessage>();
		if (iconCenterMessage != null)
		{
			iconCenterMessage.Close2(immediately: true);
		}
		CenterMessage centerMessage = GlobalAutoBehaviour<PopupManegement>.instance.PopUpExist<CenterMessage>();
		if ((bool)centerMessage)
		{
			centerMessage.CompleteAnimationWithoutCallback();
			centerMessage.SetMessage(message);
			centerMessage.PlayWindowOpenAnimation();
			return centerMessage;
		}
		WindowPrefab<CenterMessage> windowPrefab = new WindowPrefab<CenterMessage>("Resources/Prefab2/Window/MessagePopup/CenterMessage", GetCurrentLayerType());
		windowPrefab.instance.OpenInternal(message);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
		return windowPrefab.instance;
	}

	private static GameUIRoot.ELayerType GetCurrentLayerType()
	{
		if (GlobalAutoBehaviour<GlobalManagement>.instance.WindowOpenedUI3DUI())
		{
			return GameUIRoot.ELayerType.UI3DUI;
		}
		return GameUIRoot.ELayerType.UI;
	}

	protected void SetMessage(string message)
	{
		UIHelper.SetLabelText(Label_Message, message);
		UIHelper.SetLabelColor(Label_Message, Color.white);
		_curDelayTime = 0f;
	}

	protected void OpenInternal(string message)
	{
		base.Open();
		SetMessage(message);
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

	public void SetColor(Color color)
	{
		UIHelper.SetLabelColor(Label_Message, color);
	}
}
