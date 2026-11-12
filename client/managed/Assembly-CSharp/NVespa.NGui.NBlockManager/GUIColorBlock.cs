using System;
using UnityEngine;

namespace NVespa.NGui.NBlockManager;

public class GUIColorBlock : IDisposable
{
	private Color GUI_color;

	private Color GUI_contentColor;

	private Color GUI_backgroundColor;

	public GUIColorBlock Open()
	{
		GUI_color = GUI.color;
		GUI_contentColor = GUI.contentColor;
		GUI_backgroundColor = GUI.backgroundColor;
		return this;
	}

	public virtual void Dispose()
	{
		GUI.color = GUI_color;
		GUI.contentColor = GUI_contentColor;
		GUI.backgroundColor = GUI_backgroundColor;
	}
}
