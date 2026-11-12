using NGame2.NUtil;
using NNguiExtension;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class MainStoryComponent : UIScrollItemBehaviour
{
	public interface Listener
	{
		void OnClick(MainStoryComponent caller);
	}

	public const string PrefabPath = "Prefab2/Component/MainStory/MainStoryComponent";

	public static PrefabAuto<MainStoryComponent> PrefabAuto = new PrefabAuto<MainStoryComponent>("Prefab2/Component/MainStory/MainStoryComponent");

	public GameObject DisabledObj;

	public UILabel NameLabel;

	public UIButton PlayButton;

	public UISpriteSelector IconSprite;

	private Listener _listener;

	private MainStoryData _storyData;

	public MainStoryData StoryData => _storyData;

	public void Open(Listener listener, MainStoryData storyData)
	{
		_listener = listener;
		_storyData = storyData;
		if (storyData != null)
		{
			if ((bool)IconSprite)
			{
				IconSprite.SelectEnum(storyData.Command);
			}
			string[] condition = storyData.Condition;
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(condition) && !SingletonAuto<ConditionChecker>.instance.Check(condition))
			{
				UIHelper.EnableGameObject(DisabledObj, enable: true);
				UIHelper.EnableButton(PlayButton, enable: false);
				UIHelper.SetLabelText(NameLabel, LocalizationManager.GetStringDirect("STORY_RE_NOT_YET"));
			}
			else
			{
				UIHelper.EnableGameObject(DisabledObj, enable: false);
				UIHelper.EnableButton(PlayButton, enable: true);
				UIHelper.SetLabelText(NameLabel, storyData.MainStoryName);
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
