using NShader;
using NVespa;
using NVespa.NScene;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.N3D;

[ExecuteInEditMode]
[RequireComponent(typeof(Light))]
public class ShaderSetting : MonoBehaviour, IInspectorGuiDrawer
{
	[SerializeField]
	private Light _light;

	[SerializeField]
	private Vector2 _shadowDirection = new Vector2(-0.5f, 1f);

	private void Start()
	{
		if (_light == null)
		{
			_light = GetComponent<Light>();
			_light.type = LightType.Directional;
			_light.shadows = LightShadows.None;
			_light.enabled = false;
		}
		if (Application.isPlaying)
		{
			SceneAutoBehaviour<GuiManager>.instance.Add("ShaderSetting", this, DrawGUI);
		}
	}

	private void DrawGUI()
	{
		if (DataSet.instance != null)
		{
			GUILayout.Label("Guideline");
			GUILayout.Label("Outline1: " + DataSet.instance.m_toon.m_outlineWidth.ToString("F2"));
			DataSet.instance.m_toon.m_outlineWidth = GUILayout.HorizontalSlider(DataSet.instance.m_toon.m_outlineWidth, -1f, 10f);
		}
	}
}
