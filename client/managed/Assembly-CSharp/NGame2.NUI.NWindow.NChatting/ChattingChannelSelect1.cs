using NVespa;
using NVespa.NGlobal;
using UnityEngine;

namespace NGame2.NUI.NWindow.NChatting;

public class ChattingChannelSelect1 : BaseWindow
{
	public interface Listener
	{
		void OnSelectChannel(int channelNo);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Chatting/ChattingChannelSelect_1";

	public UIInput ChannelNoInput;

	public UIToggle Toggle1;

	public UIToggle Toggle2;

	public UIWidget activeSprite1;

	public UIWidget activeSprite2;

	public UILabel LangLabel;

	public int minValue = 1001;

	private Listener _listener;

	public void Open(Listener listener, int currentChannelNo)
	{
		Open(delegate
		{
			Init(listener, currentChannelNo);
		}, null);
		LangLabel.text = Vespa_String_Extension_Method.GetEnumString(SingletonAuto<LocalizationManager>.instance.Language);
	}

	private char validateChannelSelect(string text, int charIndex, char addedChar)
	{
		if (addedChar >= '1' && addedChar <= '9')
		{
			return addedChar;
		}
		if (addedChar == '0' && charIndex != 0)
		{
			return addedChar;
		}
		_ = text.Length;
		_ = 4;
		return '\0';
	}

	public void Init(Listener listener, int currentChannelNo)
	{
		_listener = listener;
		if ((bool)ChannelNoInput)
		{
			ChannelNoInput.value = currentChannelNo.ToString();
			ChannelNoInput.onValidate = validateChannelSelect;
		}
		SprkChatChannels.Configure(this);
	}

	public override void Reset()
	{
		base.Reset();
		_listener = null;
	}

	public void OnConfirm()
	{
		if (_listener != null)
		{
			if (Toggle2.value)
			{
				int num = SprkChatChannels.Normalize(Vespa_String_Extension_Method.TOINT(ChannelNoInput.value, 0));
				if (num <= minValue)
				{
					ChannelNoInput.value = minValue.ToString();
					num = minValue;
				}
				_listener.OnSelectChannel(num);
			}
			else
			{
				int num2 = Vespa_String_Extension_Method.TOINT(ChannelNoInput.value, 0);
				num2 = LanguageChannel(SingletonAuto<LocalizationManager>.instance.Language);
				_listener.OnSelectChannel(num2);
			}
		}
		Close();
	}

	// The default channel for each language; languages without one use the English channel.
	private static int LanguageChannel(SystemLanguage language)
	{
		switch (language)
		{
		case SystemLanguage.Korean:
			return 2;
		case SystemLanguage.German:
			return 3;
		case SystemLanguage.French:
			return 4;
		case SystemLanguage.Japanese:
			return 5;
		case SystemLanguage.Russian:
			return 6;
		case SystemLanguage.ChineseSimplified:
			return 7;
		case SystemLanguage.ChineseTraditional:
			return 8;
		case SystemLanguage.Thai:
			return 9;
		case SystemLanguage.Vietnamese:
			return 10;
		default:
			return 1;
		}
	}

	public void OnClickToggle()
	{
		if (Toggle2.value)
		{
			ChannelNoInput.enabled = true;
			activeSprite1.alpha = 1f;
			activeSprite2.alpha = 0f;
		}
		else
		{
			ChannelNoInput.enabled = false;
			activeSprite1.alpha = 0f;
			activeSprite2.alpha = 1f;
		}
	}
}
