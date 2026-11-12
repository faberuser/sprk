using System.Collections.Generic;
using NGame2.NCutScene;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class MainStoryList : BaseWindow, MainStoryComponent.Listener
{
	public interface Listener
	{
		void OnSelectStory(MainStoryData data);

		void OnPrevChapter();

		void OnNextChapter();
	}

	public const string PrefabPath = "Resources/Prefab2/Window/MainStory/MainStoryList";

	public UILabel ChapterNameLabel;

	public UIBundleLoader BackgroundLoader;

	public UIBaseGrid StoryListGrid;

	public Transform StoryListRoot;

	private Listener _listener;

	private MainStoryChapterData _chapterData;

	private List<Component> _componentList = new List<Component>();

	private int firstStoryIndex = 1;

	private int lastStoryIndex = 7;

	public GameObject PrevBtn;

	public GameObject NextBtn;

	public void Open(Listener listener, MainStoryChapterData chapterData, int focusingStoryNo)
	{
		Open(delegate
		{
			_listener = listener;
			_chapterData = chapterData;
			Init(focusingStoryNo);
		}, null);
	}

	public override void Reset()
	{
		base.Reset();
		_listener = null;
		ClearComponentList();
	}

	public override void OnSystemBackButton()
	{
		if (!SceneAutoBehaviour<CutSceneManager>.isValidInstance || !SceneAutoBehaviour<CutSceneManager>.instance.IsPlaying)
		{
			Close();
		}
	}

	private void Init(int focusingStoryNo)
	{
		InitTitle();
		InitBackground();
		InitComponentList(focusingStoryNo);
		InitBtn();
		ShowStoryList(show: true);
	}

	private void InitTitle()
	{
		if (_chapterData != null)
		{
			UIHelper.SetLabelText(ChapterNameLabel, LocalizationManager.GetStringDirect(_chapterData.Name));
		}
	}

	private void InitBackground()
	{
		if (_chapterData != null)
		{
			UIHelper.SetBundleTexture(BackgroundLoader, _chapterData.BackgroundAssetPath);
		}
	}

	private void InitComponentList(int focusingStoryNo)
	{
		ClearComponentList();
		MainStoryComponent mainStoryComponent = null;
		foreach (MainStoryData allDatum in GameRoot.Instance.MainStoryDataContainer.GetAllData())
		{
			if (_chapterData == null || allDatum.ChapterIndex == _chapterData.ChapterIndex)
			{
				MainStoryComponent mainStoryComponent2 = StoryListGrid.AddGridItem(MainStoryComponent.PrefabAuto);
				mainStoryComponent2.Open(this, allDatum);
				_componentList.Add(mainStoryComponent2);
				if (allDatum.StoryNo == focusingStoryNo)
				{
					mainStoryComponent = mainStoryComponent2;
				}
			}
		}
		StoryListGrid.RepositionGrid();
		if ((bool)mainStoryComponent)
		{
			StoryListGrid.ScrollTo(mainStoryComponent, 0f, 0f);
		}
	}

	private void InitBtn()
	{
		if (_chapterData == null)
		{
			return;
		}
		foreach (MainStoryChapterData allDatum in GameRoot.Instance.MainStoryChapterDataContainer.GetAllData())
		{
			firstStoryIndex = ((allDatum.StoryIndex < firstStoryIndex) ? allDatum.StoryIndex : firstStoryIndex);
			lastStoryIndex = ((allDatum.StoryIndex > lastStoryIndex) ? allDatum.StoryIndex : lastStoryIndex);
		}
		if (_chapterData.StoryIndex < lastStoryIndex)
		{
			int storyIndex = _chapterData.StoryIndex + 1;
			if (!SceneAutoBehaviour<MainStoryManagement>.instance.IsOpenChapterStory(storyIndex))
			{
				lastStoryIndex = _chapterData.StoryIndex;
			}
		}
		PrevBtn.SetActive(firstStoryIndex != _chapterData.StoryIndex);
		NextBtn.SetActive(lastStoryIndex != _chapterData.StoryIndex);
	}

	private void ClearComponentList()
	{
		FactoryBase.ReleaseAndClearList(_componentList);
	}

	public void ShowStoryList(bool show)
	{
		UIHelper.EnableGameObject(StoryListRoot, show);
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
	}

	void MainStoryComponent.Listener.OnClick(MainStoryComponent caller)
	{
		MainStoryData storyData = caller.StoryData;
		if (storyData != null && _listener != null)
		{
			_listener.OnSelectStory(storyData);
		}
	}

	public void OnClickPrev()
	{
		if (_listener != null)
		{
			_listener.OnPrevChapter();
		}
	}

	public void OnClickNext()
	{
		if (_listener != null)
		{
			_listener.OnNextChapter();
		}
	}
}
