using System;
using NShared;

namespace NGame2.NUI.NWindow;

public class GuideSelect : BaseWindow, LobbyTopmostMenu.IMiniGuideButtonObserver
{
	public interface Listener
	{
		void OnSelectGuide(GameGuideType type);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Tutorial/GuideSelect";

	private Listener _listener;

	private Action _closeAction;

	public new int WindowGuideIndex => 0;

	public void SetCloseAction(Action closeAction)
	{
		_closeAction = closeAction;
	}

	public void Open(Listener listener)
	{
		base.Open();
		Init(listener);
	}

	public void Init(Listener listener)
	{
		_listener = listener;
	}

	public override void Reset()
	{
		base.Reset();
		_listener = null;
		_closeAction = null;
	}

	public void OnClickClose()
	{
		Vespa_System_Action_Extension_Method.fire(_closeAction);
		_closeAction = null;
		Close();
	}

	public override void OnSystemBackButton()
	{
		OnClickClose();
	}

	public void OnClickSlot1()
	{
		if (_listener != null)
		{
			_listener.OnSelectGuide(GameGuideType.GuideCleo);
		}
	}

	public void OnClickSlot2()
	{
		if (_listener != null)
		{
			_listener.OnSelectGuide(GameGuideType.GuideMay);
		}
	}

	public void OnClickSlot3()
	{
		if (_listener != null)
		{
			_listener.OnSelectGuide(GameGuideType.GuidePhillop);
		}
	}
}
