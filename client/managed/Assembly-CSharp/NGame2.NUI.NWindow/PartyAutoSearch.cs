using NShared.NLocalization;
using NVespa.NGlobal;

namespace NGame2.NUI.NWindow;

public class PartyAutoSearch : BaseWindow, IGlobalWindow
{
	public interface Listener
	{
		void OnClickCancel();
	}

	public const string PrefabPath = "Resources/Prefab2/Window/PartyDungeon/PartyAutoSearch";

	private static GlobalWindowPrefab<PartyAutoSearch> _windowPrefab = new GlobalWindowPrefab<PartyAutoSearch>("Resources/Prefab2/Window/PartyDungeon/PartyAutoSearch", GlobalUIRoot.ELayerType.GlobalUI);

	public UIButton Button;

	private Listener _listener;

	public UIGame3DWidget ButtonEffect;

	public static PartyAutoSearch instance => _windowPrefab.instance;

	public static bool IsValidInstance => _windowPrefab.IsOpen();

	public static void OpenGlobal(Listener listener)
	{
		_windowPrefab.instance.OpenInternal(listener);
	}

	protected void OpenInternal(Listener listener)
	{
		base.Open();
		_listener = listener;
		EnableButton(isEnable: true);
		EffectPlay();
	}

	private void EffectPlay()
	{
		if (!(ButtonEffect == null))
		{
			if (!ButtonEffect.IsPlaying)
			{
				ButtonEffect.Play(null);
			}
			else if (ButtonEffect.IsPlaying)
			{
				ButtonEffect.Stop(immediatelyClear: true);
			}
		}
	}

	public void ShowCenterMessageJoinRoomFail()
	{
		GlobalCenterMessage.Open(LocalizationManager.GetStringDirect("PARTY_SEARCH_ENTER_FAILURE"));
	}

	public void OnClickCancel()
	{
		string stringDirect = LocalizationManager.GetStringDirect("PARTY_SEARCH");
		string stringDirect2 = LocalizationManager.GetStringDirect("PARTY_SEARCH_CANCEL_DES");
		MessagePopup3Button.OpenGlobal(stringDirect, stringDirect2, delegate
		{
			CancelInternal();
		}, null, NShared.NLocalization.Common.COMMON_CONFIRM, NShared.NLocalization.Common.COMMON_CANCEL);
	}

	public void CancelInternal()
	{
		EnableButton(isEnable: false);
	}

	private void EnableButton(bool isEnable)
	{
		UIHelper.EnableButton(Button, isEnable);
		if (!isEnable)
		{
			if (_listener != null)
			{
				_listener.OnClickCancel();
			}
			_windowPrefab.ReleaseWindow();
		}
	}
}
