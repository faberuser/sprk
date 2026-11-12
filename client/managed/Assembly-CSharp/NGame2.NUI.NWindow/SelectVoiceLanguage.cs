using System;
using NGameAsset.NSound;
using NNguiExtension;
using NVespa;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class SelectVoiceLanguage : BaseWindow
{
	[Serializable]
	public class ButtonInfo
	{
		public SystemLanguage Language;

		public GameObject Button;

		public UISpriteSelector SpriteSelect;
	}

	public const string PrefabPath = "Resources/Prefab2/Window/GameOption/SelectVoiceLanguage";

	public ButtonInfo[] ButtonInfos;

	private Action<SystemLanguage> _onSelectVoiceLanguage;

	public void Open(Action<SystemLanguage> onSelectVoiceLanguage)
	{
		base.Open();
		_onSelectVoiceLanguage = onSelectVoiceLanguage;
		SystemLanguage baseLanguage = SingletonAuto<LanguageSetting>.instance.GetBaseLanguage();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(ButtonInfos); i++)
		{
			ButtonInfo buttonInfo = ButtonInfos[i];
			if ((bool)buttonInfo.SpriteSelect)
			{
				buttonInfo.SpriteSelect.SelectInt((baseLanguage == buttonInfo.Language) ? 1 : 0);
			}
		}
	}

	public override void Reset()
	{
		base.Reset();
		_onSelectVoiceLanguage = null;
	}

	public void OnClickExitButton()
	{
		Close();
	}

	public void OnClickVoiceButton(GameObject sender)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(ButtonInfos); i++)
		{
			ButtonInfo buttonInfo = ButtonInfos[i];
			if (buttonInfo.Button == sender)
			{
				SystemLanguage language = buttonInfo.Language;
				SingletonAuto<LanguageSetting>.instance.SetBaseLanguage(language);
				Action<SystemLanguage> onSelectVoiceLanguage = _onSelectVoiceLanguage;
				Close();
				onSelectVoiceLanguage?.Invoke(language);
				break;
			}
		}
	}
}
