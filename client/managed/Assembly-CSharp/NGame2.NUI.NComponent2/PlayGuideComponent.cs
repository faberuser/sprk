using NGame2.NUI.NWindow;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class PlayGuideComponent : UIScrollItemBehaviour
{
	public interface Listener
	{
		void OnClickMove(PlayGuideInfo playGuideInfo);
	}

	public const string PrefabPath = "Prefab2/Component/PlayGuide/PlayGuideComponent";

	public static PrefabAuto<PlayGuideComponent> PrefabAuto = new PrefabAuto<PlayGuideComponent>("Prefab2/Component/PlayGuide/PlayGuideComponent");

	[SerializeField]
	private UILabel LbPlayGuideDesc;

	private Listener _listener;

	private PlayGuideInfo _playGuideInfo;

	public void Open(PlayGuideInfo playGuideInfo, Listener listener)
	{
		Init(playGuideInfo, listener);
	}

	private void Init(PlayGuideInfo playGuideInfo, Listener listener)
	{
		_playGuideInfo = playGuideInfo;
		_listener = listener;
		SetPlayGuideDesc();
	}

	private void SetPlayGuideDesc()
	{
		if (_playGuideInfo != null && _playGuideInfo.PlayGuideData != null)
		{
			UIHelper.SetLabelText(LbPlayGuideDesc, LocalizationManager.GetStringDirect(_playGuideInfo.PlayGuideData.TextKey));
		}
	}

	public void OnClickMove()
	{
		if (_listener != null && _playGuideInfo != null)
		{
			_listener.OnClickMove(_playGuideInfo);
		}
	}
}
