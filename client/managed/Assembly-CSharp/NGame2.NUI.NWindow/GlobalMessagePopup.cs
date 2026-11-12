using System;
using System.Collections;
using NGame2.NUI.NManager;
using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using NVespa.NTask;
using NVespa.NTask.NRuntime;

namespace NGame2.NUI.NWindow;

public class GlobalMessagePopup : MessagePopup, IGlobalWindow
{
	public new const string PrefabPath = "Resources/Prefab2/Window/MessagePopup/GlobalMessagePopup";

	private static GlobalWindowPrefab<GlobalMessagePopup> _windowPrefab = new GlobalWindowPrefab<GlobalMessagePopup>("Resources/Prefab2/Window/MessagePopup/GlobalMessagePopup", GlobalUIRoot.ELayerType.GlobalUI);

	private const int MinWidth = 400;

	private const int MinHeight = 100;

	public static bool IsOpenGlobal()
	{
		return _windowPrefab.IsOpen();
	}

	public static void CloseGlobal()
	{
		if (IsOpenGlobal())
		{
			_windowPrefab.CloseWindow();
		}
	}

	public static ITask OpenGlobalTask(string title, string message)
	{
		return new DelegateTask(coOpenGlobal(title, message));
	}

	public static IEnumerator coOpenGlobal(string title, string message)
	{
		bool closed = false;
		OpenGlobal(title, message, delegate
		{
			closed = true;
		}, retryButton: false);
		while (!closed)
		{
			yield return null;
		}
	}

	public static void OpenGlobalInform(Enum e)
	{
		OpenGlobalInform(LocalizationManager.GetString(e));
	}

	public static void OpenGlobalInform(string message)
	{
		OpenGlobal(LocalizationManager.GetString(NShared.NLocalization.Common.INFORM), message);
	}

	public static void OpenGlobal(string title, string message)
	{
		OpenGlobal(title, message, null, retryButton: false);
	}

	public static void OpenGlobal(string title, string message, Action onOk, bool retryButton)
	{
		OpenGlobalMessagePopup();
		_windowPrefab.instance.OpenInternal(title, message, delegate
		{
			Vespa_System_Action_Extension_Method.fire(onOk);
		}, retryButton);
	}

	public static void OpenGlobal(string title, string message, Action onYes, Action onNo)
	{
		OpenGlobalMessagePopup();
		_windowPrefab.instance.OpenInternal(title, message, onYes, onNo);
	}

	public static void OpenGlobal(string title, string message, Action onYes, Action onNo, Enum YesLabelKey, Enum NoLabelKey)
	{
		OpenGlobalMessagePopup();
		_windowPrefab.instance.OpenInternal(title, message, onYes, onNo, YesLabelKey, NoLabelKey);
	}

	public static void OpenGlobal(string title, string message, Action onYes, Action onNo, bool isIgnoreOnClose)
	{
		OpenGlobalMessagePopup();
		_windowPrefab.instance.OpenInternal(title, message, onYes, onNo, isIgnoreOnClose);
	}

	public new static void OpenYesOnly(string title, string message, Action onYes, bool ignoreOnClickClose = false)
	{
		OpenGlobalMessagePopup();
		_windowPrefab.instance.OpenInternalYesOnly(title, message, onYes, ignoreOnClickClose);
	}

	public new static void OpenYesOnly(string title, string message, Action onYes, bool ignoreOnClickClose = false, bool showOnClickClose = false)
	{
		OpenGlobalMessagePopup();
		_windowPrefab.instance.OpenInternalYesOnly(title, message, onYes, ignoreOnClickClose, showOnClickClose);
	}

	public static void OpenGlobalError(string message, Action onOk = null, bool retryButton = false)
	{
		GlobalTopmostWindow.instance.CloseSceneChangers();
		string text = LocalizationManager.GetString(NShared.NLocalization.Common.ERROR);
		VespaDebug.LogError(message);
		if (string.IsNullOrEmpty(message))
		{
			message = text;
		}
		OpenGlobal(text, message, onOk, retryButton);
	}

	public static void OpenGlobalError(Enum error, Action onOk = null, bool retryButton = false)
	{
		if (!(error.GetType() == typeof(BaseResultType)) || (BaseResultType)(object)error != BaseResultType.Disabled)
		{
			OpenGlobalError(LocalizationManager.GetString(error), onOk, retryButton);
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

	public static void OpenGlobalMessagePopup()
	{
		if (GlobalAutoBehaviour<GlobalManagement>.instance.WindowOpenedUI3DUI())
		{
			MissionMilestonePopup.ReleaseWindow();
		}
	}
}
