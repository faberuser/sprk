using NVespa.NScene;
using NVespa.NSingleton;
using UnityEngine;

namespace NVespa.NUtil;

public class SceneLoader : MonoBehaviour
{
	public string m_sceneName;

	private void Start()
	{
		if (String_Extension_Method.IsNullOrEmpty(m_sceneName))
		{
			return;
		}
		SceneAutoBehaviour<GuiManager>.instance.Add(null, this, delegate
		{
			if (GUILayout.Button("Load " + m_sceneName, GUILayout.Height(30f)))
			{
				LoadingScene.LoadLevel(m_sceneName);
			}
		});
	}
}
