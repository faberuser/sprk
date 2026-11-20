using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NNguiExtension;
using UnityEngine;

public sealed class SprkHeroGridState : MonoBehaviour
{
	public UIComboBox classes;

	public List<HeroPortraitComponent> heroes = new List<HeroPortraitComponent>();

	// Party grids: whether a filter currently hides portraits, and the grid's own hideInactive.
	public bool filtering;

	public bool hideInactive;

	// How far the row was lowered into the grid's soft-clip band.
	public float drop;

	private void OnDisable()
	{
		if (classes != null && classes.RootWindow != null)
		{
			classes.RootWindow.HidePopup();
		}
	}
}
