using System;
using UnityEngine;

namespace NVespa.NGui.NBlockManager;

public class TitleBoxBlock : IDisposable
{
	private static GUIStyle _tabStyle;

	protected Color _backup;

	private static GUIStyle TabStyle
	{
		get
		{
			if (_tabStyle == null)
			{
				_tabStyle = "dragtab";
			}
			return _tabStyle;
		}
	}

	public TitleBoxBlock Open(string i_title, params GUILayoutOption[] i_options)
	{
		Init(i_title, 0f, GUI.backgroundColor, i_options);
		return this;
	}

	public TitleBoxBlock Open(string i_title, float boxWidth, params GUILayoutOption[] i_options)
	{
		Init(i_title, boxWidth, GUI.backgroundColor, i_options);
		return this;
	}

	public TitleBoxBlock Open(string i_title, Color i_color, params GUILayoutOption[] i_options)
	{
		Init(i_title, 0f, i_color, i_options);
		return this;
	}

	public TitleBoxBlock Open(string i_title, float boxWidth, Color i_color, params GUILayoutOption[] i_options)
	{
		Init(i_title, boxWidth, i_color, i_options);
		return this;
	}

	protected virtual void Init(string i_title, float boxWidth, Color i_color, params GUILayoutOption[] i_options)
	{
		_backup = GUI.backgroundColor;
		GUI.backgroundColor = i_color;
		GUILayoutOption[] options = ((i_options != null && i_options.Length != 0) ? i_options : new GUILayoutOption[1] { GUILayout.MinHeight(10f) });
		if (Mathf.Approximately(boxWidth, 0f))
		{
			GUILayout.BeginHorizontal();
		}
		else
		{
			GUILayout.BeginHorizontal(GUILayout.Width(boxWidth));
		}
		GUILayout.Space(3f);
		GUILayout.BeginVertical();
		GUILayout.Space(3f);
		DrawHeader(i_title);
		GUILayout.BeginHorizontal();
		GUILayout.Space(1f);
		GUILayout.BeginHorizontal(PredefineStyle.ASTextArea, options);
		GUILayout.BeginVertical();
		GUILayout.Space(3f);
	}

	public virtual void Dispose()
	{
		GUILayout.Space(3f);
		GUILayout.EndVertical();
		GUILayout.EndHorizontal();
		GUILayout.Space(1f);
		GUILayout.EndHorizontal();
		GUILayout.Space(3f);
		GUILayout.EndVertical();
		GUILayout.Space(3f);
		GUILayout.EndHorizontal();
		GUI.backgroundColor = _backup;
	}

	protected virtual void DrawHeader(string i_title)
	{
		string text = Vespa_Gui_RichTextBuilder_Extension_Method.RichText(i_title).Bold().m_text;
		using (new BlockManager().GUIColor())
		{
			GUILayout.Toggle(true, text, "dragtab");
		}
	}
}
