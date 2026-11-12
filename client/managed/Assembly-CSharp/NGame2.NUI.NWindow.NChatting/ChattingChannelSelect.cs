namespace NGame2.NUI.NWindow.NChatting;

public class ChattingChannelSelect : BaseWindow
{
	public interface Listener
	{
		void OnSelectChannel(int channelNo);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Chatting/ChattingChannelSelect";

	public UIInput ChannelNoInput;

	private Listener _listener;

	public void Open(Listener listener, int currentChannelNo)
	{
		Open(delegate
		{
			Init(listener, currentChannelNo);
		}, null);
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
			int channelNo = Vespa_String_Extension_Method.TOINT(ChannelNoInput.value, 0);
			_listener.OnSelectChannel(channelNo);
		}
		Close();
	}
}
