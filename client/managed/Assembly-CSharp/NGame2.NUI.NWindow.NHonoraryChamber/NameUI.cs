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
	}

	public void SetName(string name)
	{
		SetLabelsText(LabelName, name);
	}

	private void SetLabelsText(UILabel[] labels, string text)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(labels); i++)
		{
			UIHelper.SetLabelText(labels[i], text);
		}
	}
}
