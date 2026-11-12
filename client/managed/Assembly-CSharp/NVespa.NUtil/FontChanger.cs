using System;
using System.Collections.Generic;
using NVespa.NGlobal;
using UnityEngine;

namespace NVespa.NUtil;

public class FontChanger : MonoBehaviour
{
	[Serializable]
	public class ChangeFontInfo
	{
		public SystemLanguage Language = SystemLanguage.Korean;

		public UIFont Font;

		public FontStyle FontStyle;

		public void SetFont(UILabel label)
		{
			label.bitmapFont = Font;
			label.fontStyle = FontStyle;
		}
	}

	public UILabel Label;

	public bool IncludeAllChild;

	[SerializeField]
	public List<ChangeFontInfo> FontInfos = new List<ChangeFontInfo>();

	private void Reset()
	{
		if (Label == null)
		{
			Label = GetComponent<UILabel>();
		}
	}

	private void Start()
	{
		SystemLanguage language = SingletonAuto<LocalizationManager>.instance.Language;
		ChangeFontInfo changeFontInfo = FontInfos.Find((ChangeFontInfo fontInfo) => fontInfo.Language == language);
		if (changeFontInfo == null)
		{
			return;
		}
		if (Label != null)
		{
			changeFontInfo.SetFont(Label);
		}
		if (IncludeAllChild)
		{
			UILabel[] componentsInChildren = GetComponentsInChildren<UILabel>(includeInactive: true);
			foreach (UILabel font in componentsInChildren)
			{
				changeFontInfo.SetFont(font);
			}
		}
	}
}
