using NVespa.NGui;
using NVespa.NScene;
using NVespa.NSingleton;
using UnityEngine;

namespace NTestingTools;

public class ControlAnimationScene : MonoBehaviour
{
	public Camera m_mainCamera;

	public Camera m_faceCamera;

	private Camera _activeCamera
	{
		get
		{
			if (m_mainCamera.gameObject.activeSelf)
			{
				return m_mainCamera;
			}
			return m_faceCamera;
		}
	}

	private void Start()
	{
		SceneAutoBehaviour<GuiManager>.instance.Add("AnimationScene", this, DrawGUI);
	}

	private void DrawGUI()
	{
		if (GUILayout.Button("Toggle Camera"))
		{
			if (m_mainCamera.gameObject.activeSelf)
			{
				m_mainCamera.gameObject.SetActive(value: false);
				m_faceCamera.gameObject.SetActive(value: true);
			}
			else
			{
				m_mainCamera.gameObject.SetActive(value: true);
				m_faceCamera.gameObject.SetActive(value: false);
			}
		}
		GUILayout.Label("Zoom", PredefineStyle.SmallTitleLabel);
		float magnitude = _activeCamera.transform.localPosition.magnitude;
		float num = GUILayout.HorizontalSlider(magnitude, 1f, 5f);
		if (!Mathf.Approximately(num, magnitude))
		{
			_activeCamera.transform.localPosition = _activeCamera.transform.localPosition.normalized * num;
		}
	}
}
