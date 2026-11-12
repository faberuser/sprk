using UnityEngine;

namespace NVespa.NUtil;

internal class EditorBoard : NVespa.NUtil.IBoard
{
	public void SetText(string str)
	{
		GUIUtility.systemCopyBuffer = str;
	}

	public string GetText()
	{
		return GUIUtility.systemCopyBuffer;
	}
}
