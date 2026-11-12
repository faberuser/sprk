using System;
using System.Collections;
using System.Linq;
using NGame2.NChatting;
using NGame2.NCutScene;
using NGame2.NDevice;
using NGame2.NLogin;
using NGame2.NTutorial;
using NGame2.NUI.NWindow.NGlobalTopmost;
using NShared;
using NShared.Login;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using NVespa.NWeb;
using UnityEngine;

namespace NGame2.NScene;

public class ServerChanger
{
	public class MoveToForwardedServerTouchDisablerBlock : ITouchDisabler
	{
		public bool IsValid => true;
	}

	private class ServiceDisablerWhileServerChange : IServiceDisabler
	{
		private readonly string _host;

		private readonly string[] _serviceName;

		public ServiceDisablerWhileServerChange(string host, string[] serviceName)
		{
			_host = host;
			_serviceName = serviceName;
		}

		bool IServiceDisabler.IsDisabled(string host, string serviceName)
		{
			for (int i = 0; i < _serviceName.Count(); i++)
			{
				if (string.Equals(_serviceName[i], serviceName, StringComparison.InvariantCultureIgnoreCase))
				{
					return false;
				}
			}
			return true;
		}
	}

	public const float TouchTimeInterval = 300f;

	public static void CheckForwardedServer()
	{
		if ((GlobalAutoBehaviour<LoadingManager>.isValidInstance && GlobalAutoBehaviour<LoadingManager>.instance.IsSceneLoading) || ((DateTime.UtcNow - SingletonAuto<SleepModeManager>.instance.TouchedTime).TotalSeconds < 300.0 && ((SingletonUpdater<TutorialManager>.isValidInstance && SingletonUpdater<TutorialManager>.instance.IsProgressing) || (SceneAutoBehaviour<CutSceneManager>.isValidInstance && SceneAutoBehaviour<CutSceneManager>.instance.IsPlaying))) || !SingletonUpdater<LoginManager>.instance.IsLogined)
		{
			return;
		}
		string text = SingletonUpdater<WebServiceManager>.instance.PopForwardedHost();
		if (String_Extension_Method.IsNullOrEmpty(text))
		{
			return;
		}
		ChangeServer(text, delegate(bool success)
		{
			if (success)
			{
				if (SingletonUpdater<TutorialManager>.isValidInstance && SingletonUpdater<TutorialManager>.instance.IsProgressing)
				{
					SingletonUpdater<TutorialManager>.instance.StopTutorial();
				}
				GlobalAutoBehaviour<LoadingManager>.instance.MoveScene(LoadingManager.ESceneType.Lobby);
			}
		});
	}

	public static void ChangeServer(string forwardedHost, Action<bool> onChanged)
	{
		VespaCoroutine.Start(coMoveToForwardedServer(forwardedHost, onChanged));
	}

	private static IEnumerator coMoveToForwardedServer(string forwardedHost, Action<bool> onChanged)
	{
		if (string.IsNullOrEmpty(forwardedHost))
		{
			Vespa_System_Action_Extension_Method.fire(onChanged, i_argA: false);
			yield break;
		}
		string host = SingletonUpdater<WebServiceManager>.instance.Requester.Host;
		string[] serviceName = new string[2] { "user/get_certificate", "user/login" };
		ServiceDisablerWhileServerChange serviceDisabler = new ServiceDisablerWhileServerChange(host, serviceName);
		SingletonUpdater<WebServiceManager>.instance.Requester.AddServiceDisabler(serviceDisabler);
		ResultType result = ResultType.Fail;
		bool isComplete = false;
		float loginStartTime = Time.time;
		SingletonUpdater<LoginManager>.instance.Relogin(delegate(ResultType iResult)
		{
			result = iResult;
			isComplete = true;
		}, forwardedHost);
		for (int count = 3; count >= 1; count--)
		{
			GlobalAutoBehaviour<ChattingManager>.instance.AddSystemChat(LocalizationManager.GetString(NShared.NLocalization.System.CHANGING_SERVER, count), ChatType.Channel, showPreview: true, 0L);
			yield return new WaitForSeconds(1f);
		}
		while (!isComplete)
		{
			if (Time.time - loginStartTime > 20f)
			{
				result = ResultType.Fail;
				break;
			}
			yield return null;
		}
		SingletonUpdater<WebServiceManager>.instance.Requester.RemoveServiceDisabler(serviceDisabler);
		if (result != ResultType.Success)
		{
			Vespa_System_Action_Extension_Method.fire(onChanged, i_argA: false);
		}
		else
		{
			Vespa_System_Action_Extension_Method.fire(onChanged, i_argA: true);
		}
	}
}
