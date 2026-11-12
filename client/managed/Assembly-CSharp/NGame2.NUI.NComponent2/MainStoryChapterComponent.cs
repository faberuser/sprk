using NGame2.NUI.NManager;
using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class MainStoryChapterComponent : UIScrollItemBehaviour
{
	public interface Listener
	{
		void OnClick(MainStoryChapterComponent caller);
	}

	public const string PrefabPath = "Prefab2/Component/MainStory/MainStoryChapterComponent";

	public static PrefabAuto<MainStoryChapterComponent> PrefabAuto = new PrefabAuto<MainStoryChapterComponent>("Prefab2/Component/MainStory/MainStoryChapterComponent");

	public GameObject DisabledObj;

	public UILabel NameLabel;

	public UIButton ChapterButton;

	private Listener _listener;

	private MainStoryChapterData _chapterData;

	public MainStoryChapterData ChapterData => _chapterData;

	public void Open(Listener listener, MainStoryChapterData chapterData)
	{
		_listener = listener;
		_chapterData = chapterData;
		Init();
	}

	private void Init()
	{
		if (_chapterData != null)
		{
			if (!SceneAutoBehaviour<MainStoryManagement>.instance.IsOpenChapterStory(_chapterData.StoryIndex))
			{
				UIHelper.EnableGameObject(DisabledObj, enable: true);
				UIHelper.EnableButton(ChapterButton, enable: false);
				UIHelper.SetLabelText(NameLabel, LocalizationManager.GetStringDirect("STORY_RE_NOT_YET"));
			}
			else
			{
				UIHelper.EnableGameObject(DisabledObj, enable: false);
				UIHelper.EnableButton(ChapterButton, enable: true);
				UIHelper.SetLabelText(NameLabel, LocalizationManager.GetStringDirect(_chapterData.Name));
			}
		}
	}

	public void OnClickView()
	{
		if (_listener != null)
		{
			_listener.OnClick(this);
		}
	}
}
