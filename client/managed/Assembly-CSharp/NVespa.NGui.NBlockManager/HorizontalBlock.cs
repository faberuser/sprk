using System;
using UnityEngine;

namespace NVespa.NGui.NBlockManager;

public class HorizontalBlock : IDisposable
{
	public bool m_drawColorBox;

	public Color m_boxColor;

	public float m_thickness;

	public HorizontalBlock Open(params GUILayoutOption[] options)
	{
		GUILayout.BeginHorizontal(options);
		return this;
	}

	public HorizontalBlock Open(GUIStyle style, params GUILayoutOption[] options)
	{
		GUILayout.BeginHorizontal(style, options);
		return this;
	}

	public void ColorBox(Color i_color, float i_thickness)
	{
		m_drawColorBox = true;
		m_boxColor = i_color;
		m_thickness = i_thickness;
	}

	public virtual void Dispose()
	{
		GUILayout.EndHorizontal();
		if (m_drawColorBox && m_thickness > Mathf.Epsilon)
		{
			DrawTools.DrawOutline(GUILayoutUtility.GetLastRect(), m_boxColor, m_thickness);
		}
	}
}
