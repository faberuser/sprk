using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class PlayGuidePopup : BaseWindow, PlayGuideComponent.Listener
{
	public interface Listener
	{
		void MoveDirect(PlayGuideInfo playGuideinfo);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/PlayGuide/PlayGuidePopup";

	private List<PlayGuideComponent> _listPlayGuideComponent = new List<PlayGuideComponent>();

	[SerializeField]
	private UIScrollGrid ScrollGridComponent;

	private Listener _listener;

	public void Open(Listener listener, List<PlayGuideInfo> list)
	{
		Open();
		Init(listener, list);
	}

	private void Init(Listener listener, List<PlayGuideInfo> list)
	{
		_listener = listener;
		SetPlayGuideList(list);
	}

	private void SetPlayGuideList(List<PlayGuideInfo> list)
	{
		FactoryBase.ReleaseAndClearList(_listPlayGuideComponent);
		if (list != null && list.Count > 0)
		{
			PlayGuideComponent playGuideComponent = null;
			foreach (PlayGuideInfo item in list)
			{
				if (item != null)
				{
					playGuideComponent = UIHelper.AddGridItem(ScrollGridComponent, PlayGuideComponent.PrefabAuto);
					if (playGuideComponent != null)
					{
						playGuideComponent.Open(item, this);
						_listPlayGuideComponent.Add(playGuideComponent);
					}
				}
			}
		}
		UIHelper.RepositionGrid(ScrollGridComponent);
	}

	void PlayGuideComponent.Listener.OnClickMove(PlayGuideInfo playGuideinfo)
	{
		if (_listener != null)
		{
			_listener.MoveDirect(playGuideinfo);
			Close();
		}
	}
}
