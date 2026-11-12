using System.Collections;
using NGame2.NBattleView;
using NGame2.NUI.NWindow.NTutorial;
using NShared;
using NShared.NEvent;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NTutorial.NRuntime;

public class SetBattleCamera : ITutorialRuntimeImmediate, ITutorialRuntime, IEventRuntime
{
	public enum ECameraType
	{
		Battle,
		WaveEnd,
		Freeze
	}

	bool ITutorialRuntime.OnEvent(IEvent triggerEvent)
	{
		return false;
	}

	IEnumerator ITutorialRuntime.Run(TutorialWindow window, TutorialData tutorialData)
	{
		Vespa_Common_Class_Extension_Method.Interface((ITutorialRuntimeImmediate)this).RunImmediate(window, tutorialData);
		yield break;
	}

	void ITutorialRuntimeImmediate.RunImmediate(TutorialWindow window, TutorialData tutorialData)
	{
		ECameraType eCameraType = EnumTools.Parse(TutorialUtility.GetArg(tutorialData, 0), ECameraType.Battle);
		int waveIndex = Vespa_String_Extension_Method.TOINT(TutorialUtility.GetArg(tutorialData, 1), -1);
		switch (eCameraType)
		{
		case ECameraType.Battle:
			SceneAutoBehaviour<CameraManager2>.instance.SwitchCam_Battle(waveIndex);
			break;
		case ECameraType.WaveEnd:
			SceneAutoBehaviour<CameraManager2>.instance.SwitchCam_WaveEnd(waveIndex);
			break;
		case ECameraType.Freeze:
			SceneAutoBehaviour<CameraManager2>.instance.SwitchCam_Freeze();
			break;
		}
	}
}
