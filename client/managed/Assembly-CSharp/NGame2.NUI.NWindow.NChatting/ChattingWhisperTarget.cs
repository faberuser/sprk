namespace NGame2.NUI.NWindow.NChatting;

public class ChattingWhisperTarget : BaseWindow
{
	public interface Listener
	{
		void OnWhisperTarget(string userName);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Chatting/ChattingWhisperTarget";

	public UIInput NameInput;

	private Listener _listener;

	public void Open(Listener listener)
	{
		base.Open();
		_listener = listener;
	}

	public void Init(Listener listener)
	{
		_listener = listener;
	}

	public override void Reset()
	{
		base.Reset();
		_listener = null;
	}

	public void OnConfirm()
	{
		if (_listener != null && !string.IsNullOrEmpty(NameInput.value))
		{
			_listener.OnWhisperTarget(NameInput.value);
		}
		Close();
	}
}
