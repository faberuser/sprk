using System;
using NGame2.NUI.NWindow;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NScene;

public class BaseScene : MonoBehaviour
{
	public interface IPreload
	{
		float Progress { get; }

		int Current { get; }

		int Total { get; }

		bool IsEnd { get; }

		void Tick();

		void OnEnd();
	}

	public virtual void OnSceneUnloading(LoadingManager.ESceneType nextScene)
	{
	}

	public virtual void OnSceneLoaded(Action onComplete)
	{
		Vespa_System_Action_Extension_Method.fire(onComplete);
	}

	public virtual IPreload PreloadAsset()
	{
		return null;
	}

	public virtual void Update()
	{
	}

	protected virtual void ShowQuitPopup()
	{
		if (!QuitGamePopup.IsOpenGlobal())
		{
			QuitGamePopup.OpenGlobal(delegate
			{
				GlobalAutoBehaviour<LoadingManager>.instance.Quit();
			});
		}
	}
}
