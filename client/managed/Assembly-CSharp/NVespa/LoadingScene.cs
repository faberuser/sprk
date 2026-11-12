using UnityEngine;
using UnityEngine.SceneManagement;

namespace NVespa;

public class LoadingScene : MonoBehaviour
{
	public static string s_loadLevel = "Battle2";

	public static void LoadLevel(string i_loadLevel)
	{
		s_loadLevel = i_loadLevel;
		SceneManager.LoadScene("Loading");
	}

	private void Start()
	{
		Resources.UnloadUnusedAssets();
		GlobalTimer.Reset();
		MainTimer.Reset();
		UnityTimeScaler.Reset();
		Invoke("LoadNextLevel", 0.1f);
	}

	private void LoadNextLevel()
	{
		SceneManager.LoadScene(s_loadLevel);
	}
}
