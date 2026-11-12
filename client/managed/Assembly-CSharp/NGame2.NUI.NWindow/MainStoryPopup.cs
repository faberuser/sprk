using System;
using NGame2.NUtil;
using NNguiExtension;
using NShared;
using NVespa;
using NVespa.NGlobal;

namespace NGame2.NUI.NWindow;

public class MainStoryPopup : BaseWindow
{
	public interface Listener
	{
		void OnSelectNextStory();

		void OnSelectReplayStory();
	}

	public const string PrefabPath = "Resources/Prefab2/Window/MainStory/MainStoryPopup";

	private Listener _listener;

	private Action _onCloseComplete;

	private MainStoryData _nextStoryData;

	public UILabel StoryTitle;

	public UILabel StoryName;

	public UILabel StoryName2;

	public UIButton NextBtn;

	public UISpriteSelector IconSprite;

	public void Open(Listener listener, MainStoryData nextStoryData, Action onCloseComplete = null)
	{
		base.Open();
		_listener = listener;
		_onCloseComplete = onCloseComplete;
		_nextStoryData = nextStoryData;
		Init();
	}

	private void Init()
	{
		string text = "";
		string text2 = "";
		bool flag = true;
		if (_nextStoryData == null)
		{
			text = LocalizationManager.GetStringDirect("STORY_RE_POPUP_DESC_NONEXT");
			text2 = LocalizationManager.GetStringDirect("STORY_RE_POPUP_DESC_LASTSTORY");
			flag = false;
		}
		else
		{
			text = LocalizationManager.GetStringDirect("STORY_RE_POPUP_DESC");
			text2 = _nextStoryData.MainStoryName;
			if ((bool)IconSprite)
			{
				IconSprite.SelectEnum(_nextStoryData.Command);
			}
			string[] condition = _nextStoryData.Condition;
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(condition) && !SingletonAuto<ConditionChecker>.instance.Check(condition))
			{
				text = LocalizationManager.GetStringDirect("STORY_RE_POPUP_DESC_NONEXT");
				text2 = LocalizationManager.GetStringDirect("STORY_RE_POPUP_DESC_NOTOPEN");
				flag = false;
			}
		}
		StoryTitle.text = text;
		UIHelper.EnableButton(NextBtn, flag);
		UIHelper.EnableGameObject(IconSprite.gameObject, flag);
		UIHelper.EnableGameObject(StoryName.gameObject, flag);
		UIHelper.EnableGameObject(StoryName2.gameObject, !flag);
		if (flag)
		{
			StoryName.text = text2;
		}
		else
		{
			StoryName2.text = text2;
		}
	}

	public override void Reset()
	{
		base.Reset();
		_listener = null;
		_nextStoryData = null;
	}

	public void OnClickNextStory()
	{
		if (_listener != null)
		{
			_listener.OnSelectNextStory();
		}
	}

	public void OnClickReplayStory()
	{
		if (_listener != null)
		{
			_listener.OnSelectReplayStory();
		}
	}

	public override void Close()
	{
		if (_onCloseComplete != null)
		{
			_onCloseComplete();
		}
		base.Close();
	}

	public void CloseImmediately()
	{
		Close2(immediately: true);
	}
}
