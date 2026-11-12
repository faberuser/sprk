using System;
using UnityEngine;

namespace NVespa.NGui.NBlockManager;

public class VerticalBlock : IDisposable
{
	public bool m_drawColorBox;

	public Color m_boxColor;

	private float _thickness = 3f;

	public VerticalBlock Open(params GUILayoutOption[] options)
	{
		GUILayout.BeginVertical(options);
		return this;
	}

	public VerticalBlock Open(GUIStyle style, params GUILayoutOption[] options)
	{
		GUILayout.BeginVertical(style, options);
		return this;
	}

	public void ColorBox(Color i_color, float thickness = 3f)
	{
		m_drawColorBox = true;
		m_boxColor = i_color;
		_thickness = thickness;
	}

	public virtual void Dispose()
	{
		GUILayout.EndVertical();
		if (m_drawColorBox)
		{
			DrawTools.DrawOutline(GUILayoutUtility.GetLastRect(), m_boxColor, _thickness);
		}
	}
}
