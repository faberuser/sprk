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
				num2 = ((SingletonAuto<LocalizationManager>.instance.Language == SystemLanguage.English) ? 1 : ((SingletonAuto<LocalizationManager>.instance.Language == SystemLanguage.Korean) ? 2 : ((SingletonAuto<LocalizationManager>.instance.Language == SystemLanguage.German) ? 3 : ((SingletonAuto<LocalizationManager>.instance.Language == SystemLanguage.French) ? 4 : ((SingletonAuto<LocalizationManager>.instance.Language == SystemLanguage.Japanese) ? 5 : ((SingletonAuto<LocalizationManager>.instance.Language == SystemLanguage.Russian) ? 6 : ((SingletonAuto<LocalizationManager>.instance.Language == SystemLanguage.ChineseSimplified) ? 7 : ((SingletonAuto<LocalizationManager>.instance.Language != SystemLanguage.ChineseTraditional) ? 1 : 8))))))));
				_listener.OnSelectChannel(num2);
			}
		}
		Close();
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
