using System;
using System.Collections.Generic;
using UnityEngine;

namespace NVespa.NUtil;

public class FontSelector : MonoBehaviour, IInspectorGuiDrawer
{
	[Serializable]
	public class LanguageFont
	{
		public SystemLanguage Language;

		public string ContainerPath;
	}

	private const string PrefabPath = "Resources/Prefab/FontSelector";

	private static FontSelector _instance;

	public UIFont ReferenceFont;

	public UIFont ReferenceBoldFont;

	public string DefaultFontPath;

	[HideInInspector]
	public List<LanguageFont> LanguageFontList = new List<LanguageFont>();

	public static FontSelector instance
	{
		get
		{
			if (_instance != null)
			{
				return _instance;
			}
			GameObject gameObject = VespaResource.Load("Resources/Prefab/FontSelector", typeof(GameObject)) as GameObject;
			if (gameObject == null)
			{
				return null;
			}
			_instance = gameObject.GetComponent<FontSelector>();
			return _instance;
		}
	}

	public void SetLanguage(SystemLanguage language)
	{
		if (ReferenceFont == null || ReferenceBoldFont == null)
		{
			return;
		}
		string i_path = DefaultFontPath;
		for (int i = 0; i < LanguageFontList.Count; i++)
		{
			LanguageFont languageFont = LanguageFontList[i];
			if (language == languageFont.Language)
			{
				i_path = languageFont.ContainerPath;
				break;
			}
		}
		FontContainer fontContainer = VespaResource.Load<FontContainer>(i_path);
		if (fontContainer != null)
		{
			if (fontContainer.TargetFont != null)
			{
				ReferenceFont.dynamicFont = fontContainer.TargetFont;
			}
			if (fontContainer.TargetBoldFont != null)
			{
				ReferenceBoldFont.dynamicFont = fontContainer.TargetBoldFont;
			}
		}
	}
}
