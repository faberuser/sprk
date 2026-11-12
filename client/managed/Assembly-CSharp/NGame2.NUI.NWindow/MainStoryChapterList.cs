using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class MainStoryChapterList : BaseWindow, MainStoryChapterComponent.Listener, WorldMapManagement.IAvatarHider
{
	public interface Listener
	{
		void OnSelectChapter(MainStoryChapterData data);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/MainStory/MainStoryChapterList";

	public UIBaseGrid ChapterListGrid;

	private Listener _listener;

	private List<Component> _componentList = new List<Component>();

	public void Open(Listener listener, int focusingStoryIndex)
	{
		Open(delegate
		{
			_listener = listener;
			Init(focusingStoryIndex);
		}, null);
	}

	public override void Reset()
	{
		base.Reset();
		_listener = null;
		ClearComponentList();
	}

	private void Init(int focusingStoryIndex)
	{
		DisableAvatar();
		ClearComponentList();
		MainStoryChapterComponent mainStoryChapterComponent = null;
		foreach (MainStoryChapterData allDatum in GameRoot.Instance.MainStoryChapterDataContainer.GetAllData())
		{
			MainStoryChapterComponent mainStoryChapterComponent2 = ChapterListGrid.AddGridItem(MainStoryChapterComponent.PrefabAuto);
			mainStoryChapterComponent2.Open(this, allDatum);
			_componentList.Add(mainStoryChapterComponent2);
			if (allDatum.StoryIndex == focusingStoryIndex)
			{
				mainStoryChapterComponent = mainStoryChapterComponent2;
			}
		}
		ChapterListGrid.RepositionGrid();
		if ((bool)mainStoryChapterComponent)
		{
			ChapterListGrid.ScrollTo(mainStoryChapterComponent, 0f, 0f);
		}
	}

	private void DisableAvatar()
	{
		SceneAutoBehaviour<WorldMapManagement>.instance.AddAvatarHider(this);
		SceneAutoBehaviour<EntryMenuManagement>.instance.SetSpeech(string.Empty);
	}

	private void ClearComponentList()
	{
		FactoryBase.ReleaseAndClearList(_componentList);
	}

	void MainStoryChapterComponent.Listener.OnClick(MainStoryChapterComponent caller)
	{
		MainStoryChapterData chapterData = caller.ChapterData;
		if (chapterData != null && _listener != null)
		{
			_listener.OnSelectChapter(chapterData);
		}
	}

	bool WorldMapManagement.IAvatarHider.IsOpen()
	{
		return base.IsOpen();
	}

	WorldMapManagement.AvatarHideFlag WorldMapManagement.IAvatarHider.GetAvatarHideFlag()
	{
		return WorldMapManagement.AvatarHideFlag.HideAll;
	}
}
