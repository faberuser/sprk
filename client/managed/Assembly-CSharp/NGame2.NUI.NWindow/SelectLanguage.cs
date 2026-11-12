using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class SelectLanguage : BaseWindow, LanguageButtonComponent.Listener
{
	public interface Listener
	{
		void OnChangeLanguage(SystemLanguage language);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/GameOption/SelectLanguage";

	public UIScrollGrid ButtonGrid;

	private SystemLanguage _curLanguage;

	private Listener _listener;

	private List<LanguageButtonComponent> _componentList = new List<LanguageButtonComponent>();

	public void Open(Listener listener)
	{
		base.Open();
		_listener = listener;
		_curLanguage = SingletonAuto<LocalizationManager>.instance.Language;
		InitLanguageButtons();
	}

	public override void Reset()
	{
		base.Reset();
		FactoryBase.ReleaseAndClearList(_componentList);
	}

	public void OnClickExitButton()
	{
		Close();
	}

	private void InitLanguageButtons()
	{
		foreach (SystemLanguage allLanguage in SingletonAuto<LocalizationManager>.instance.GetAllLanguages())
		{
			LanguageButtonComponent languageButtonComponent = UIHelper.AddGridItem(ButtonGrid, LanguageButtonComponent.PrefabAuto);
			languageButtonComponent.Open(allLanguage, this);
			_componentList.Add(languageButtonComponent);
		}
		UIHelper.RepositionGrid(ButtonGrid);
	}

	void LanguageButtonComponent.Listener.OnClick(LanguageButtonComponent component)
	{
		if (_listener != null)
		{
			SystemLanguage language = component.Language;
			if (SingletonAuto<LocalizationManager>.instance.IsValidLanguage(language) && _curLanguage != language)
			{
				_listener.OnChangeLanguage(language);
			}
		}
	}
}
