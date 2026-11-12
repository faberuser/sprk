using NGame2.NUI.NManager;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow;

public class GuildNoticePopup : BaseWindow
{
	public interface Listener
	{
		void ApplyChangeNotice(string _notice);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/MessagePopup/GuildNoticePopup";

	public UIInput Input_NoticeContent;

	private Listener _listener;

	public static void Open(Listener listener, string notice)
	{
		WindowPrefab<GuildNoticePopup> windowPrefab = new WindowPrefab<GuildNoticePopup>("Resources/Prefab2/Window/MessagePopup/GuildNoticePopup", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal(listener, notice);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	protected void OpenInternal(Listener listener, string notice)
	{
		base.Open();
		_listener = listener;
		Input_NoticeContent.characterLimit = GameRoot.Instance.ConstantDataContainer.GetValueInt("MaxGuildNoticeLength");
		Input_NoticeContent.value = notice;
	}

	public override void Reset()
	{
		base.Reset();
		if (Input_NoticeContent != null)
		{
			Input_NoticeContent.value = "";
		}
	}

	public void OnClickApply()
	{
		if (_listener != null)
		{
			_listener.ApplyChangeNotice(Input_NoticeContent.value);
			Close();
		}
	}

	public void OnClickClose()
	{
		Close();
	}
}
