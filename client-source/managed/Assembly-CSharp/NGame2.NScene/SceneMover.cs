using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NScene;

public class SceneMover : MonoBehaviour
{
	private void Start()
	{
		while (true)
		{
			GameObject gameObject = GameObject.Find("<Global.Singleton>");
			if (gameObject == null)
			{
				break;
			}
			VespaObject.DestroyGo(gameObject);
		}
		GlobalAutoBehaviour<LoadingManager>.instance.MoveScene(LoadingManager.ESceneType.Login);
	}
}
