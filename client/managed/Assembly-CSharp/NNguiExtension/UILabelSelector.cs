using System;
using System.Collections.Generic;
using NVespa;
using NVespa.NUtil;
using UnityEngine;

namespace NNguiExtension;

public class UILabelSelector : UIBasicSelector, IInspectorGuiDrawer
{
	[Serializable]
	public class SelectData
	{
		public bool IsDefault;

		public string Name = string.Empty;

		public bool UseColor;

		public Color LabelColor = Color.black;

		public Color LabelEffectColor = Color.black;

		public LocalizeLinkData LocalizeLink = new LocalizeLinkData();
	}

	[SerializeField]
	private UILabel _target;

	[HideInInspector]
	public List<SelectData> SelectList = new List<SelectData>();

	[HideInInspector]
	private Color InitColor;

	[HideInInspector]
	private Color InitEffectColor;

	public UILabel GetTargetLabel()
	{
		if (_target != null)
		{
			return _target;
		}
		return GetComponent<UILabel>();
	}

	private void Awake()
	{
		UILabel targetLabel = GetTargetLabel();
		if (!(targetLabel == null))
		{
			InitColor = targetLabel.color;
			InitEffectColor = targetLabel.effectColor;
		}
	}

	public void Show(bool show)
	{
		UILabel targetLabel = GetTargetLabel();
		if ((bool)targetLabel)
		{
			targetLabel.gameObject.SetActive(show);
		}
	}

	public override bool Select(string name)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(SelectList))
		{
			return false;
		}
		for (int i = 0; i < SelectList.Count; i++)
		{
			if (SelectList[i].Name == name)
			{
				SelectByIndex(i);
				return true;
			}
		}
		for (int j = 0; j < SelectList.Count; j++)
		{
			if (SelectList[j].IsDefault)
			{
				SelectByIndex(j);
				return true;
			}
		}
		return false;
	}

	public void SelectArgs(string name, params object[] args)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(SelectList))
		{
			return;
		}
		for (int i = 0; i < SelectList.Count; i++)
		{
			if (SelectList[i].Name == name)
			{
				SelectArgsByIndex(i, args);
				return;
			}
		}
		for (int j = 0; j < SelectList.Count; j++)
		{
			if (SelectList[j].IsDefault)
			{
				SelectArgsByIndex(j, args);
				break;
			}
		}
	}

	public override void SelectByIndex(int index)
	{
		if (!Vespa_Generic_IList_Extension_Method.IsValidIndex(SelectList, index))
		{
			return;
		}
		SelectData selectData = SelectList[index];
		UILabel targetLabel = GetTargetLabel();
		if (targetLabel != null)
		{
			targetLabel.text = selectData.LocalizeLink.GetLocalize();
			if (selectData.UseColor)
			{
				targetLabel.color = selectData.LabelColor;
				targetLabel.effectColor = selectData.LabelEffectColor;
			}
			else
			{
				targetLabel.color = InitColor;
				targetLabel.effectColor = InitEffectColor;
			}
		}
	}

	protected void SelectArgsByIndex(int index, params object[] args)
	{
		if (Vespa_Generic_IList_Extension_Method.IsValidIndex(SelectList, index))
		{
			SelectData selectData = SelectList[index];
			UILabel targetLabel = GetTargetLabel();
			if (targetLabel != null)
			{
				targetLabel.text = selectData.LocalizeLink.GetLocalize(args);
			}
		}
	}
}
