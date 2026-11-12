using UnityEngine;

namespace NGame2.NEditor;

public static class DrawHelper
{
	public static int GetSize(string str)
	{
		int num = (String_Extension_Method.IsNullOrEmpty(str) ? 1 : (str.Length + 1));
		return GUI.skin.font.fontSize * (num + 1);
	}

	public static bool ButtonAutoSize(string name)
	{
		int size = GetSize(name);
		return ButtonCustom(name, size);
	}

	public static bool ButtonNormal(string name)
	{
		return ButtonCustom(name, 80f);
	}

	public static bool ButtonCustom(string name, float minWidth)
	{
		return GUILayout.Button(name, GUILayout.MinWidth(minWidth));
	}

	public static void LabelAutoSize(string name)
	{
		int size = GetSize(name);
		LabelCustom(name, size);
	}

	public static void LabelCustom(string name, float minWidth)
	{
		GUILayout.Label(name, GUILayout.MaxWidth(minWidth));
	}

	public static bool ToggleAutoSize(string name, bool value)
	{
		int size = GetSize(name);
		return ToggleCustom(name, value, size);
	}

	public static bool ToggleCustom(string name, bool value, float minWidth)
	{
		return GUILayout.Toggle(value, name, GUILayout.MaxWidth(minWidth));
	}

	public static string TextFieldAutoSize(string value)
	{
		int size = GetSize(value);
		return TextFieldCustom(value, size);
	}

	public static string TextFieldSmall(string value)
	{
		return TextFieldCustom(value, 80f);
	}

	public static string DrawTextFieldNormal(string value)
	{
		return TextFieldCustom(value, 160f);
	}

	public static string TextFieldLarge(string value)
	{
		return TextFieldCustom(value, 240f);
	}

	public static string TextFieldCustom(string value, float minWidth)
	{
		return GUILayout.TextField(value, GUILayout.MaxWidth(minWidth));
	}

	public static void FlexibleSpace()
	{
		GUILayout.FlexibleSpace();
	}
}
