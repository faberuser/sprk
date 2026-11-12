using System.Collections.Generic;
using NVespa;
using NVespa.NScene;
using NVespa.NSingleton;
using UnityEngine;

namespace NShader;

[ExecuteInEditMode]
public class SceneManager : SceneBehaviour<SceneManager>
{
	public Camera m_camera;

	public Light m_light;

	public DataSet m_dataSet;

	public bool m_drawGrid;

	public Vector2 m_shadowDirection;

	public List<GameObject> m_listToggle = new List<GameObject>();

	private DataSet _dataSetUsing
	{
		get
		{
			if (m_dataSet == null)
			{
				return DataSet.instance;
			}
			return m_dataSet;
		}
	}

	private void Start()
	{
		if (SystemStatus.IsPlayingCritical)
		{
			SceneAutoBehaviour<GuiManager>.instance.Add("SceneManager", this, DrawGUI);
		}
	}

	private void Update()
	{
		UpdateShaderVariable(m_camera);
	}

	private void UpdateShaderVariable(Camera i_camera)
	{
	}

	private void DrawGUI()
	{
		if (_dataSetUsing != null)
		{
			GUILayout.Label("Guideline");
			GUILayout.Label("Outline1: " + _dataSetUsing.m_toon.m_outlineWidth.ToString("F2"));
			_dataSetUsing.m_toon.m_outlineWidth = GUILayout.HorizontalSlider(_dataSetUsing.m_toon.m_outlineWidth, -1f, 10f);
			GUILayout.Label("Camera Size: " + m_camera.orthographicSize.ToString("F2"));
			m_camera.orthographicSize = GUILayout.HorizontalSlider(m_camera.orthographicSize, 1f, 5f);
		}
		for (int i = 0; i < m_listToggle.Count; i++)
		{
			GameObject gameObject = m_listToggle[i];
			bool flag = GUILayout.Toggle(gameObject.activeSelf, Vespa_Unity_Object_Extension_Method.NAME(gameObject));
			if (flag != gameObject.activeSelf)
			{
				gameObject.SetActive(flag);
			}
		}
	}

	private void OnDrawGizmos()
	{
		if (m_drawGrid)
		{
			Gizmos.color = Vespa_Unity_Color_Extension_Method.Alpha(Color.yellow, 0.2f);
			Gizmos.DrawCube(Vector3.zero, new Vector3(20f, 0f, 20f));
			Gizmos.color = Vespa_Unity_Color_Extension_Method.Alpha(Color.yellow, 0.5f);
			for (int i = -10; i <= 10; i++)
			{
				Gizmos.DrawLine(new Vector3(i, 0f, -10f), new Vector3(i, 0f, 10f));
				Gizmos.DrawLine(new Vector3(-10f, 0f, i), new Vector3(10f, 0f, i));
			}
		}
	}
}
