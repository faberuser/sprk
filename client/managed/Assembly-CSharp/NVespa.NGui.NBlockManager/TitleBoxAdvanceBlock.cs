using System;
using System.Collections.Generic;
using UnityEngine;

namespace NVespa.NGui.NBlockManager;

public class TitleBoxAdvanceBlock : TitleBoxBlock
{
	private Action _onDrawGui;

	private bool _isExpand;

	private static readonly GUIStyle LabelStyle = new GUIStyle(GUI.skin.label)
	{
		richText = true
	};

	private static Dictionary<Tuple<object, string>, bool> _infoDic = new Dictionary<Tuple<object, string>, bool>();

	public TitleBoxAdvanceBlock Open(string i_title, Color i_color, object keyObj, string keyName, out bool isExpand, Action onDrawGui, params GUILayoutOption[] i_options)
	{
		_isExpand = GetInfo(keyObj, keyName);
		_onDrawGui = onDrawGui;
		Init(i_title, 0f, i_color, i_options);
		isExpand = _isExpand;
		SetInfo(keyObj, keyName, _isExpand);
		return this;
	}

	private static bool GetInfo(object keyObj, string keyName)
	{
		lock (_infoDic)
		{
			if (_infoDic.TryGetValue(Tuple.Create(keyObj, keyName), out var value))
			{
				return value;
			}
			_infoDic[Tuple.Create(keyObj, keyName)] = true;
		}
		return true;
	}

	public static void SetInfo(object keyObj, string keyName, bool isExpand)
	{
		lock (_infoDic)
		{
			_infoDic[Tuple.Create(keyObj, keyName)] = isExpand;
		}
	}

	protected override void Init(string i_title, float boxWidth, Color i_color, params GUILayoutOption[] i_options)
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
		if (_isExpand)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Space(1f);
			GUILayout.BeginHorizontal(PredefineStyle.ASTextArea, options);
			GUILayout.BeginVertical();
			GUILayout.Space(3f);
		}
	}

	public override void Dispose()
	{
		if (_isExpand)
		{
			base.Dispose();
			return;
		}
		GUILayout.Space(3f);
		GUILayout.EndVertical();
		GUILayout.Space(3f);
		GUILayout.EndHorizontal();
		GUI.backgroundColor = _backup;
	}

	protected override void DrawHeader(string title)
	{
	}
}
