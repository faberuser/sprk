using System.Collections;
using NGame2.NCutScene;
using NGame2.NFunctionality;
using NGame2.NUI.NWindow.NTutorial;
using NGameAsset.NPatch;
using NShared;
using NShared.NEvent;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NTutorial.NRuntime;

public class PlayCutScene : ITutorialRuntime, IEventRuntime
{
	bool ITutorialRuntime.OnEvent(IEvent triggerEvent)
	{
		return false;
	}

	IEnumerator ITutorialRuntime.Run(TutorialWindow window, TutorialData tutorialData)
	{
		GlobalAutoBehaviour<FunctionalityManager>.instance.SetValue(FunctionalityType.WaitOnCloseLoadingWindow, "True");
		string cutscenePath = TutorialUtility.GetArg(tutorialData, 0);
		CutSceneManager cutSceneManager = SceneAutoBehaviour<CutSceneManager>.instance;
		cutSceneManager.Open(cutscenePath, canSkip: false, null);
		bool isDownloading = GlobalAutoBehaviour<PatchDownloader>.instance.IsPatchDownloading();
		float waitStartTime = Time.time;
		while (!cutSceneManager.IsPlaying)
		{
			if (!isDownloading && Time.time - waitStartTime > 10f)
			{
				VespaDebug.LogError(Define.LogPrefix + " Failed to wait cutscene play : " + cutscenePath);
				break;
			}
			yield return null;
		}
		GlobalAutoBehaviour<FunctionalityManager>.instance.SetDefaultValue(FunctionalityType.WaitOnCloseLoadingWindow);
	}
}
