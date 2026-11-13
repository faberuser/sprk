using System;
using UnityEngine;

namespace NGame2.NUI.NWindow.NHonoraryChamber;

[Serializable]
public class NameUI
{
	public Transform TransformRoot;

	public UILabel[] LabelLv;

	public UILabel[] LabelName;

	public void SetLevel(int level)
	{
		SetLabelsText(LabelLv, "Lv. " + level);
		Reposition();
	}

	public void SetName(string name)
	{
		SetLabelsText(LabelName, name);
		Reposition();
	}

	private void SetLabelsText(UILabel[] labels, string text)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(labels); i++)
		{
			labels[i].overflowMethod = UILabel.Overflow.ResizeFreely;
			UIHelper.SetLabelText(labels[i], text);
			_ = labels[i].printedSize;
		}
	}

	private void Reposition()
	{
		if (TransformRoot == null || LabelLv == null || LabelLv.Length == 0 || LabelName == null || LabelName.Length == 0) return;
		foreach (UITableHorizontal table in TransformRoot.GetComponentsInChildren<UITableHorizontal>(true))
		{
			// Keep the level/name pairs and their nested shadows together as text changes.
			table.padding = new Vector2(Mathf.Max(4f, table.padding.x), table.padding.y);
			table.Reposition();
			Bounds bounds = NGUIMath.CalculateRelativeWidgetBounds(TransformRoot, LabelLv[0].transform, true);
			bounds.Encapsulate(NGUIMath.CalculateRelativeWidgetBounds(TransformRoot, LabelName[0].transform, true));
			table.transform.position -= TransformRoot.TransformVector(new Vector3(bounds.center.x, 0f, 0f));
		}
	}
}
