using System;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NTutorial;

public class TutorialTooltipComponent : LinkFactoryComponent
{
	public const string PrefabPath = "Prefab2/Component/Tutorial/TutorialTooltipComponent";

	public static PrefabAuto<TutorialTooltipComponent> PrefabAuto = new PrefabAuto<TutorialTooltipComponent>("Prefab2/Component/Tutorial/TutorialTooltipComponent");

	public UIWidget TooltipBgWidget;

	public UILabel TooltipLabel;

	public UILabel IconTooltipLabel;

	public Transform IconRoot;

	public GameObject[] IconObjects;

	public int MinX { get; private set; }

	public int MinY { get; private set; }

	public int MaxX { get; private set; }

	public int MaxY { get; private set; }

	public bool Open()
	{
		UIHelper.EnableGameObject(TooltipBgWidget, enable: false);
		return true;
	}

	public void SetTooltipText(int minX, int minY, int maxX, int maxY, string text, string iconName)
	{
		MinX = minX;
		MinY = minY;
		MaxX = maxX;
		MaxY = maxY;
		if ((bool)TooltipBgWidget)
		{
			UIHelper.EnableGameObject(TooltipBgWidget, enable: true);
			TooltipBgWidget.SetRect(minX, minY, maxX - minX, maxY - minY);
		}
		if (string.IsNullOrEmpty(iconName))
		{
			UIHelper.EnableGameObject(IconRoot, enable: false);
			UIHelper.EnableGameObject(IconTooltipLabel, enable: false);
			UIHelper.EnableGameObject(TooltipLabel, enable: true);
			UIHelper.SetLabelText(TooltipLabel, text);
			return;
		}
		UIHelper.EnableGameObject(IconRoot, enable: true);
		UIHelper.EnableGameObject(IconTooltipLabel, enable: true);
		UIHelper.EnableGameObject(TooltipLabel, enable: false);
		UIHelper.SetLabelText(IconTooltipLabel, text);
		GameObject[] iconObjects = IconObjects;
		foreach (GameObject obj in iconObjects)
		{
			bool enable = string.Equals(obj.name, "Sprite_" + iconName, StringComparison.InvariantCultureIgnoreCase);
			UIHelper.EnableGameObject(obj, enable);
		}
	}

	public void SetTooltipText2(UIWidget.Pivot pivot, int x, int y, int marginX, int marginY, string text, string iconName)
	{
		if (TooltipLabel != null)
		{
			TooltipLabel.UpdateNGUIText(useValidFontSize: true);
			NGUIText.regionWidth = 1920;
			NGUIText.regionHeight = 1080;
		}
		Vector2 vector = NGUIText.CalculatePrintedSize(text);
		Vector2 pivotOffset = NGUIMath.GetPivotOffset(pivot);
		Vector2 vector2 = Vector2.one - pivotOffset;
		int minX = x - Mathf.RoundToInt((vector.x + (float)marginX) * pivotOffset.x);
		int maxX = x + Mathf.RoundToInt((vector.x + (float)marginX) * vector2.x);
		int minY = y - Mathf.RoundToInt((vector.y + (float)marginY) * pivotOffset.y);
		int maxY = y + Mathf.RoundToInt((vector.y + (float)marginY) * vector2.y);
		SetTooltipText(minX, minY, maxX, maxY, text, iconName);
	}
}
