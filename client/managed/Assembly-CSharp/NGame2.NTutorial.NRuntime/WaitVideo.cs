using System;
using System.Collections;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NTutorial;
using NShared;
using NShared.NEvent;
using NVespa;

namespace NGame2.NTutorial.NRuntime;

public class WaitVideo : ITutorialRuntime, IEventRuntime
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

		private bool _enabelSkip;

		private TutorialWindow.ActionInfo _touchCallbackInfo;

		private bool _skipButtonPressed;

		public void Step0_Ready(TutorialWindow window, TutorialData tutorialData)
		{
			try
			{
				_window = window;
				_tutorialData = tutorialData;
				_window.RemoveSkipCallback();
				_window.RemoveSkipStartTutorialData();
				_enabelSkip = TutorialUtility.GetArgBool(tutorialData, 0);
				if (_enabelSkip)
				{
					_window.SetSkipCallback(OnClickSkip);
				}
			}
			catch (Exception i_object)
			{
				State = EState.End;
				VespaDebug.LogError("WaitVideo.Holder.Step0_Ready Exception: " + Vespa_Common_Class_Extension_Method.TOSTRING(i_object));
			}
		}

		public void Step1_Loop()
		{
			try
			{
				if (!VideoManager.IsPlaying())
				{
					State = EState.End;
				}
				else if (_skipButtonPressed)
				{
					ProcessSkip();
				}
			}
			catch (Exception i_object)
			{
				ProcessSkip();
				VespaDebug.LogError("WaitVideo.Holder.Step1_Loop Exception: " + Vespa_Common_Class_Extension_Method.TOSTRING(i_object));
			}
		}

		public void Step99_Finalize()
		{
			try
			{
				_window.RemoveSkipCallback();
			}
			catch (Exception i_object)
			{
				ProcessSkip();
				VespaDebug.LogError("WaitVideo.Holder.Step99_Finalize Exception: " + Vespa_Common_Class_Extension_Method.TOSTRING(i_object));
			}
		}

		private void ProcessSkip()
		{
			_skipButtonPressed = false;
			VideoManager.SkipToClose(delegate
			{
				State = EState.End;
			});
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
		holder.Step99_Finalize();
	}
}
