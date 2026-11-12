using System;
using System.Collections.Generic;
using NGame2.NUI;
using UnityEngine;

namespace NGame2.NUI3.NEmbed;

public class Embed_DropdownButton_Comp : MonoBehaviour
{
	[Serializable]
	public class ButtonGroupUi
	{
		public UIButton Button;

		public UILabel Label;

		public string GetLabelText()
		{
			if (!(Label == null))
			{
				return Label.text;
			}
			return string.Empty;
		}
	}

	[Header("현재 선택")]
	public UIButton Button_OpenDropdownList;

	public UILabel Label_Value_CurrentSelection;

	[Header("드랍 다운 루트")]
	public UITable Table_DropdownRoot;

	[Header("드랍 다운 아이템")]
	public List<ButtonGroupUi> ButtonGroup_DowndownItems;

	[Header("선택 이벤트")]
	public List<EventDelegate> onChanged;

	private int _count;

	public int Selection { get; private set; }

	public void InitDropdown(int initSelection, params string[] labels)
	{
		_count = Mathf.Min(__Extension_Method_ICollection__.COUNT(labels), __Extension_Method_ICollection__.COUNT(ButtonGroup_DowndownItems));
		if (_count == 0)
		{
			SetEnable(enable: false);
			return;
		}
		UIHelper.EnableGameObject(Table_DropdownRoot, enable: false);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(ButtonGroup_DowndownItems); i++)
		{
			ButtonGroupUi buttonGroupUi = ButtonGroup_DowndownItems[i];
			if (i < _count)
			{
				UIHelper.EnableGameObject(buttonGroupUi.Button, enable: true);
				UIHelper.SetLabelText(buttonGroupUi.Label, labels[i]);
			}
			else
			{
				UIHelper.EnableGameObject(buttonGroupUi.Button, enable: false);
				UIHelper.SetLabelText(buttonGroupUi.Label, string.Empty);
			}
		}
		SetSelection(Mathf.Min(_count, initSelection), notify: false);
	}

	public void SetEnable(bool enable)
	{
		UIHelper.EnableGameObject(Button_OpenDropdownList, enable);
		UIHelper.EnableGameObject(Table_DropdownRoot, enable: false);
	}

	public void SetSelection(int index, bool notify = true)
	{
		if (index >= 0 && index < _count && index < __Extension_Method_ICollection__.COUNT(ButtonGroup_DowndownItems) && Selection != index)
		{
			Selection = index;
			ButtonGroupUi buttonGroupUi = ButtonGroup_DowndownItems[Selection];
			UIHelper.SetLabelText(Label_Value_CurrentSelection, buttonGroupUi.GetLabelText());
			if (notify && base.enabled)
			{
				EventDelegate.Execute(onChanged);
			}
		}
	}

	public void OnClick_OpenDropdownList()
	{
		UIHelper.EnableGameObject(Table_DropdownRoot, enable: true);
		Table_DropdownRoot.Reposition();
	}

	public void OnClick_DropdownItems(UIButton button)
	{
		UIHelper.EnableGameObject(Table_DropdownRoot, enable: false);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(ButtonGroup_DowndownItems); i++)
		{
			if (!(ButtonGroup_DowndownItems[i].Button != button))
			{
				SetSelection(i);
				break;
			}
		}
	}
}
