using System.Collections.Generic;
using NGame2.NLogin;
using NGame2.NUI.NWindow.NBattle;
using NGame2.NUtil;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class LoginBackground : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/Login/LoginBackground";

	public UIGame3DWidgetFromRes WidgetEffect;

	public UIPlaySound EffectSound;

	public UILabel Label_Version;

	public UILabel Label_PatchVersion;

	public UILabel Label_Message;

	public Transform GuestLoginMessageRoot;

	public Transform UnsuitableMessage;

	public Transform BackEffectPanelRoot;

	public List<Transform> ActiveOnOpen = new List<Transform>();

	public string SecretKey;

	private bool _reserveWidgetEffect;

	private string _message;

	private float _lastMessageUpdateTime;

	private int _messageDotCount;

	private bool _updateMessage;

	private List<int> _developList = new List<int> { 1, 2, 2, 2, 1, 1, 2, 2, 2, 2 };

	private int _developPos;

	public static bool BlockMessage { get; set; }

	public void OnClickDevelop1()
	{
		OnClickDevelop(1);
	}

	public void OnClickDevelop2()
	{
		OnClickDevelop(2);
	}

	private void OnClickDevelop(int number)
	{
		if (!QueryMarketInfo.IsAvailableDevConsole)
		{
			return;
		}
		if (!Vespa_Generic_IList_Extension_Method.IsValidIndex(_developList, _developPos))
		{
			_developPos = 0;
			return;
		}
		if (_developList[_developPos] != number)
		{
			_developPos = 0;
			return;
		}
		_developPos++;
		if (_developPos == __Extension_Method_ICollection__.COUNT(_developList))
		{
			GlobalAutoBehaviour<GuiManager>.instance.SetEnable(enabled: true);
		}
	}

	protected override void OnDestroy()
	{
		if (!SystemStatus.IsApplicationQuit)
		{
			base.OnDestroy();
			if (_reserveWidgetEffect)
			{
				WidgetEffect = null;
			}
		}
	}

	public void Open(ClientConfig clientConfig)
	{
		Open(delegate
		{
			Init(clientConfig);
		}, null);
	}

	public void Init(ClientConfig clientConfig)
	{
		RestoredPortal.FinishStartupLogo();
		if (AutoNextBattle.IsEnabled())
		{
			AutoNextBattle.instance.StopRepeat();
		}
		if (AutoRepeatBattle.IsEnabled())
		{
			AutoRepeatBattle.instance.StopRepeat();
		}
		if (EffectSound != null)
		{
			EffectSound.Play();
		}
		_reserveWidgetEffect = true;
		if (WidgetEffect != null)
		{
			WidgetEffect.Play(null);
		}
		string text = ((clientConfig != null) ? clientConfig.Version : "");
		if (string.IsNullOrEmpty(text))
		{
			text = "-.-.-";
		}
		string text2 = "Ver. " + text;
		int remainSecond = 0;
		if (clientConfig != null && clientConfig.GetRemainExpireSecond(out remainSecond))
		{
			text2 = text2 + "." + remainSecond / 3600;
		}
		if (!String_Extension_Method.IsNullOrEmpty(SingletonUpdater<LoginManager>.instance.BinaryVersionNumber.Value))
		{
			text2 = Vespa_String_Extension_Method.NguiColor(text2, Color.red);
		}
		UIHelper.SetLabelText(Label_Version, text2);
		ShowGuestLoginMessage(show: false);
		ShowUnsuitableMessage(show: false);
		int width = Screen.width;
		int height = Screen.height;
		if ((float)width / (float)height <= 1.4f)
		{
			BackEffectPanelRoot.localPosition = Vector3.zero;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(ActiveOnOpen); i++)
		{
			UIHelper.EnableGameObject(ActiveOnOpen[i], enable: true);
		}
		SprkAccounts.CleanLoginBackground(this);
	}

	public void SetPatchVersion(int patchVersion)
	{
		UIHelper.EnableGameObject(Label_PatchVersion, enable: true);
		UIHelper.SetLabelText(Label_PatchVersion, "(" + patchVersion + ")");
		SprkAccounts.HidePatchVersion(this);
	}

	public void PlayEffectPhase()
	{
		if (WidgetEffect != null)
		{
			WidgetEffect.PlayPhase("Play");
		}
	}

	public override void Reset()
	{
		base.Reset();
		if (WidgetEffect != null)
		{
			WidgetEffect.Stop(immediatelyClear: true);
		}
	}

	public void SetMessage(string message, bool animate)
	{
		if (Label_Message == null)
		{
			return;
		}
		lock (Label_Message)
		{
			if (animate)
			{
				_message = message;
				_lastMessageUpdateTime = Time.time;
				_messageDotCount = 0;
				_updateMessage = true;
				UIHelper.EnableGameObject(Label_Message, enable: true);
				UIHelper.SetLabelText(Label_Message, message);
				Label_Message.UpdateAnchors();
			}
			else
			{
				UIHelper.EnableGameObject(Label_Message, enable: true);
				UIHelper.SetLabelText(Label_Message, message);
				_updateMessage = false;
			}
		}
	}

	public void HideMessage()
	{
		UIHelper.EnableGameObject(Label_Message, enable: false);
		_updateMessage = false;
	}

	private void UpdateMessageLabel()
	{
		if (_updateMessage && !(Time.time - _lastMessageUpdateTime < 0.3f))
		{
			_lastMessageUpdateTime = Time.time;
			_messageDotCount = (_messageDotCount + 1) % 3;
			string text = _message + new string('.', _messageDotCount);
			UIHelper.SetLabelText(Label_Message, text);
		}
	}

	public void ShowMessage(bool showUnsuitableMsg, bool showGuestLoginMsg)
	{
		UIHelper.EnableGameObject(UnsuitableMessage, showUnsuitableMsg);
		UIHelper.EnableGameObject(GuestLoginMessageRoot, showGuestLoginMsg);
		if (showGuestLoginMsg && showUnsuitableMsg)
		{
			Vector3 localPosition = GuestLoginMessageRoot.localPosition;
			localPosition.y = 60f;
			GuestLoginMessageRoot.localPosition = localPosition;
		}
	}

	public void ShowGuestLoginMessage(bool show)
	{
		UIHelper.EnableGameObject(GuestLoginMessageRoot, show);
	}

	public void ShowUnsuitableMessage(bool show)
	{
		UIHelper.EnableGameObject(UnsuitableMessage, show);
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		UpdateMessageLabel();
	}

	public bool IsFadeInComplete()
	{
		if (WindowRoot == null)
		{
			return true;
		}
		UIWidget component = WindowRoot.GetComponent<UIWidget>();
		if (component == null)
		{
			return true;
		}
		return component.alpha >= 1f;
	}
}
