using System.Collections;
using System.Collections.Generic;
using NGame2.NConfig;
using NGame2.NScene;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NShared;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NLogin;

public abstract class StateBase
{
	public class MessageTimeData
	{
		public string Message;

		public float BeginTime;

		public float EndTime;
	}

	public interface IStateOwner
	{
		void SetNextState(StateBase nextState);

		void SetSubState(StateBase subState);
	}

	public static List<MessageTimeData> MessageLogList = new List<MessageTimeData>();

	public static List<string> FlowList = new List<string>();

	private static MessageTimeData _currentMessage;

	public bool isSubState;

	public IStateOwner StateOwner;

	protected List<VespaCoroutine.RuntimeData> _coRuntimeList = new List<VespaCoroutine.RuntimeData>();

	public static ClientConfig ClientConfig => SingletonAuto<Config>.instance.ClientConfig;

	protected VespaCoroutine.RuntimeData StartCoroutine(IEnumerator enumerator)
	{
		VespaCoroutine.RuntimeData runtimeData = VespaCoroutine.StartRuntime(enumerator);
		if (runtimeData == null)
		{
			return null;
		}
		_coRuntimeList.Add(runtimeData);
		return runtimeData;
	}

	protected void ErrorAndQuit(string message)
	{
		SetBgMessage(message, animate: false);
		GlobalMessagePopup.OpenGlobalError(message, GlobalAutoBehaviour<LoadingManager>.instance.Quit);
	}

	protected void ErrorNotify(string message)
	{
		SetBgMessage(message, animate: false);
		GlobalMessagePopup.OpenGlobalError(message);
	}

	public static void Reset()
	{
		MessageLogList.Clear();
		_currentMessage = null;
		FlowList.Clear();
	}

	public static bool SetNecessaryState(string[] classNames)
	{
		if (classNames == null)
		{
			return true;
		}
		for (int i = 0; i < classNames.Length; i++)
		{
			if (FlowList.Contains(classNames[i]))
			{
				return true;
			}
		}
		return false;
	}

	public static void SetBgMessage(string message, bool animate)
	{
		lock (MessageLogList)
		{
			if (_currentMessage != null)
			{
				_currentMessage.EndTime = Time.realtimeSinceStartup;
				MessageLogList.Add(_currentMessage);
				_currentMessage = null;
			}
			if (SceneAutoBehaviour<LoginManagement>.isValidInstance && SceneAutoBehaviour<LoginManagement>.instance.IsOpened<LoginBackground>())
			{
				if (String_Extension_Method.IsNullOrEmpty(message))
				{
					SceneAutoBehaviour<LoginManagement>.instance.GetInstance<LoginBackground>().HideMessage();
					return;
				}
				_currentMessage = new MessageTimeData();
				_currentMessage.Message = message;
				_currentMessage.BeginTime = Time.realtimeSinceStartup;
				SceneAutoBehaviour<LoginManagement>.instance.GetInstance<LoginBackground>().SetMessage(message, animate);
			}
		}
	}

	public bool IsRunning()
	{
		if (_coRuntimeList != null)
		{
			return _coRuntimeList.Count > 0;
		}
		return false;
	}

	public virtual void OnBegin()
	{
		string name = GetType().Name;
		FlowList.Add(name);
	}

	public virtual void OnEnd()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_coRuntimeList); i++)
		{
			VespaCoroutine.RuntimeData runtimeData = _coRuntimeList[i];
			if (!runtimeData.IsEnd)
			{
				VespaCoroutine.Stop(runtimeData);
			}
		}
		_coRuntimeList.Clear();
	}

	public virtual void OnSessionExpired()
	{
	}

	public virtual void ChainUpdate()
	{
	}

	public virtual void NextState()
	{
	}

	public virtual void EndSubState()
	{
	}

	public virtual IEnumerator coEntry()
	{
		VespaCoroutine.RuntimeData runtime = StartCoroutine(coRun());
		while (!runtime.IsEnd)
		{
			yield return null;
		}
		if (!isSubState)
		{
			NextState();
		}
		else
		{
			EndSubState();
		}
	}

	public virtual IEnumerator coRun()
	{
		yield return null;
	}
}
