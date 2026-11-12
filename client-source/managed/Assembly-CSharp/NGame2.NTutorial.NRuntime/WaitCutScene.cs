using System;
using System.Collections;
using NGame2.NCutScene;
using NGame2.NUI.NWindow.NTutorial;
using NShared;
using NShared.NEvent;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NTutorial.NRuntime;

public class WaitCutScene : ITutorialRuntime, IEventRuntime
{
	private class Holder
	{
		public enum EState
		{
			Playing,
			ProgressEnd,
			End
		}

		public EState State;

		private TutorialWindow _window;

		private TutorialData _tutorialData;

		private int _scriptGroupIndex;

		private bool _isContinue;

		private TutorialWindow.ActionInfo _touchCallbackInfo;

		private bool _skipButtonPressed;

		public void Step0_Ready(TutorialWindow window, TutorialData tutorialData)
		{
			try
			{
				_window = window;
				_tutorialData = tutorialData;
				_scriptGroupIndex = TutorialUtility.GetArgInt(tutorialData, 0);
				_isContinue = TutorialUtility.GetArgBool(tutorialData, 1);
				CutSceneManager instance = SceneAutoBehaviour<CutSceneManager>.instance;
				if (!instance.IsShowing())
				{
					instance.Show(show: true);
				}
				_touchCallbackInfo = window.SetTouchCallback(OnTouch, oneshot: false);
			}
			catch (Exception i_object)
			{
				State = EState.End;
				VespaDebug.LogError("WaitCutScene.Holder.Step0_Ready Exception: " + Vespa_Common_Class_Extension_Method.TOSTRING(i_object));
			}
		}

		public void Step1_Loop()
		{
			try
			{
				CutSceneManager instance = SceneAutoBehaviour<CutSceneManager>.instance;
				int currentScript = instance.CurrentScript;
				if (_scriptGroupIndex > 0 && currentScript > _scriptGroupIndex)
				{
					State = EState.End;
					return;
				}
				if (!instance.IsPlaying)
				{
					State = EState.End;
					return;
				}
				if (instance.CurrentScriptSkipType == ScriptGroup.ESkipType.StopThisAndHide)
				{
					_window.RemoveSkipCallback();
				}
				else
				{
					_window.SetSkipCallback(OnClickSkip);
				}
				if (_skipButtonPressed)
				{
					ProcessSkip();
				}
			}
			catch (Exception i_object)
			{
				ProcessSkip();
				VespaDebug.LogError("WaitCutScene.Holder.Step1_Loop Exception: " + Vespa_Common_Class_Extension_Method.TOSTRING(i_object));
			}
		}

		public void Step99_Finalize()
		{
			try
			{
				CutSceneManager instance = SceneAutoBehaviour<CutSceneManager>.instance;
				if (!_isContinue)
				{
					instance.Show(show: false);
				}
				_window.RemoveTouchCallback(_touchCallbackInfo);
				_window.RemoveSkipCallback();
			}
			catch (Exception i_object)
			{
				ProcessSkip();
				VespaDebug.LogError("WaitCutScene.Holder.Step99_Finalize Exception: " + Vespa_Common_Class_Extension_Method.TOSTRING(i_object));
			}
		}

		private void ProcessSkip()
		{
			CutSceneManager instance = SceneAutoBehaviour<CutSceneManager>.instance;
			_skipButtonPressed = false;
			if (_scriptGroupIndex > 0)
			{
				instance.SkipTo(_scriptGroupIndex);
				return;
			}
			if (!instance.CanSkipToEnd())
			{
				instance.Skip();
			}
			State = EState.ProgressEnd;
			instance.Close(stopImmediate: false, delegate
			{
				State = EState.End;
			});
		}

		public void OnTouch()
		{
			if (SceneAutoBehaviour<CutSceneManager>.isValidInstance)
			{
				SceneAutoBehaviour<CutSceneManager>.instance.OnClickWindow();
			}
		}

		public void OnClickSkip()
		{
			_skipButtonPressed = true;
		}
	}

	bool ITutorialRuntime.OnEvent(IEvent triggerEvent)
	{
		return false;
	}

	IEnumerator ITutorialRuntime.Run(TutorialWindow window, TutorialData tutorialData)
	{
		Holder holder = new Holder();
		holder.Step0_Ready(window, tutorialData);
		while (holder.State == Holder.EState.Playing)
		{
			yield return null;
			holder.Step1_Loop();
		}
		float beginWaitTime = Time.realtimeSinceStartup;
		while (holder.State == Holder.EState.ProgressEnd && !(Time.realtimeSinceStartup - beginWaitTime > 10f))
		{
			yield return null;
		}
		holder.Step99_Finalize();
	}
}
