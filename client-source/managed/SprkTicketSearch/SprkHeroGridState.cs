using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NNguiExtension;
using UnityEngine;

public sealed class SprkHeroGridState : MonoBehaviour
{
	public UIComboBox classes;

	public List<HeroPortraitComponent> heroes = new List<HeroPortraitComponent>();

	private void OnDisable()
	{
		if (classes != null && classes.RootWindow != null)
		{
			classes.RootWindow.HidePopup();
		}
	}
}
