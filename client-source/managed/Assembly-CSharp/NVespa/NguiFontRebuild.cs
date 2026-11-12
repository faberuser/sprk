using System.Collections;
using UnityEngine;

namespace NVespa;

public class NguiFontRebuild
{
	public enum EPanelState
	{
		Begin,
		PanelUpdating,
		EndOfUpdate
	}

	public static bool IsDirty;

	public static EPanelState PanelState;

	private static Coroutine _endOfFrameCo;

	public static void OnFontChanged(Font font)
	{
		UILabel.OnFontChanged(font);
		IsDirty = true;
		if (_endOfFrameCo == null)
		{
			_endOfFrameCo = VespaCoroutine.StartGlobal(coEndOfFrame(), "(ngui font)");
		}
		if (PanelState == EPanelState.EndOfUpdate)
		{
			UIPanel.UpdateAllPanels();
		}
	}

	private static IEnumerator coEndOfFrame()
	{
		while (true)
		{
			yield return new WaitForEndOfFrame();
			PanelState = EPanelState.Begin;
		}
	}
}
