using System.Collections;
using NGame2.NScene;
using NGame2.NUI.NManager.NLobby;
using NShared;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class LobbyPlayGuideMenu : BaseWindow
{
	public interface Listener
	{
		void OnClickPlayGuide();

		void MoveDirect(PlayGuideInfo playGuideinfo);
	}

	public const string PrefabPath = "Resources/Prefab2/Lobby/LobbyPlayGuideMenu";

	public UILabel LbPlayGuideDesc;

	public TweenPosition TweenPositionPlayGuideMenu;

	public GameObject GoMenu;

	public Animation IconAnimation;

	private const int DefaultDelayTime = 5;

	private const string KeyPlayGuideShowupTime = "PlayGuideShowupTime";

	private Listener _listener;

	private int _delaySeconds;

	private PlayGuideInfo _currentPlayGuideInfo;

	private LobbyManagement _lobbyManagement
	{
		get
		{
			if (!SceneAutoBehaviour<LobbyManagement>.isValidInstance)
			{
				return null;
			}
			return SceneAutoBehaviour<LobbyManagement>.instance;
		}
	}

	public void Open(Listener listener)
	{
		Open();
		Init(listener);
	}

	private void Init(Listener listener)
	{
		_listener = listener;
		SetDelayTime();
		TweenPositionPlayGuideMenu.PlayReverse();
		if (_lobbyManagement != null)
		{
			_lobbyManagement.CheckUpdatePlayGuide = true;
			_lobbyManagement.CheckPlayGuideStartCourtineStatus = true;
		}
	}

	private void SetDelayTime()
	{
		if (GameRoot.Instance.ConstantDataContainer != null)
		{
			_delaySeconds = GameRoot.Instance.ConstantDataContainer.GetValueInt("PlayGuideShowupTime", 5);
		}
	}

	private void SetPlayGuideStatus()
	{
		if (_lobbyManagement != null && _lobbyManagement.ListPlayGuideInfo != null && _lobbyManagement.ListPlayGuideInfo.Count > 0)
		{
			_lobbyManagement.CheckUpdatePlayGuide = false;
			DeleteSortedPlayerGuideInfoByNotEnough();
			_lobbyManagement.SetPlayGuideStatus();
			VespaCoroutine.Start(ShowPlayGuide(), delegate
			{
				FinishedShowPlayGuide();
			});
		}
	}

	private void DeleteSortedPlayerGuideInfoByIndex(int index)
	{
		if (!(_lobbyManagement != null) || _lobbyManagement.ListSortedPlayGuideInfo == null)
		{
			return;
		}
		for (int num = _lobbyManagement.ListSortedPlayGuideInfo.Count - 1; num >= 0; num--)
		{
			if (_lobbyManagement.ListSortedPlayGuideInfo[num] != null && _lobbyManagement.ListSortedPlayGuideInfo[num].PlayGuideData != null && _lobbyManagement.ListSortedPlayGuideInfo[num].PlayGuideData.Index == index)
			{
				_lobbyManagement.ListSortedPlayGuideInfo.RemoveAt(num);
				break;
			}
		}
	}

	private void DeleteSortedPlayerGuideInfoByNotEnough()
	{
		if (!(_lobbyManagement != null) || _lobbyManagement.ListSortedPlayGuideInfo == null)
		{
			return;
		}
		for (int num = _lobbyManagement.ListSortedPlayGuideInfo.Count - 1; num >= 0; num--)
		{
			if (_lobbyManagement.ListSortedPlayGuideInfo[num] != null && _lobbyManagement.ListSortedPlayGuideInfo[num].Status == PlayGuideStatus.NOT_ENOUGH)
			{
				_lobbyManagement.ListSortedPlayGuideInfo.RemoveAt(num);
			}
		}
	}

	public void ShowPlayGuideMenu(bool show)
	{
		if (GoMenu != null)
		{
			UIHelper.EnableGameObject(GoMenu, show);
		}
	}

	public void OnClickPlayGuide()
	{
		if (_lobbyManagement != null && _listener != null)
		{
			_listener.OnClickPlayGuide();
		}
	}

	public void OnClickMoveDirect()
	{
		if (_listener != null)
		{
			_listener.MoveDirect(_currentPlayGuideInfo);
		}
	}

	private void Update()
	{
		if (GameOptionData.Show_PlayGuide && _lobbyManagement != null && _lobbyManagement.IsShowPlayGuideMenuByStatus() && !GlobalAutoBehaviour<LoadingManager>.instance.IsSceneLoading && _lobbyManagement.CheckUpdatePlayGuide)
		{
			SetPlayGuideStatus();
		}
	}

	private IEnumerator ShowPlayGuide()
	{
		if (!(_lobbyManagement != null))
		{
			yield break;
		}
		foreach (PlayGuideInfo playGuideInfo in _lobbyManagement.ListPlayGuideInfo)
		{
			while (!IsOpen() || !_lobbyManagement.CheckPlayGuideStartCourtineStatus)
			{
				yield return null;
			}
			if (playGuideInfo != null && playGuideInfo.PlayGuideData != null && playGuideInfo.Status == PlayGuideStatus.ENOUGH)
			{
				playGuideInfo.Status = PlayGuideStatus.SHOWN;
				yield return new WaitForSeconds(1f);
				_currentPlayGuideInfo = playGuideInfo;
				UIHelper.SetLabelText(LbPlayGuideDesc, LocalizationManager.GetStringDirect(playGuideInfo.PlayGuideData.TextKey));
				TweenPositionPlayGuideMenu.PlayForward();
				if (IconAnimation != null)
				{
					IconAnimation.Play();
				}
				DeleteSortedPlayerGuideInfoByIndex(playGuideInfo.PlayGuideData.Index);
				_lobbyManagement.ListSortedPlayGuideInfo.Insert(0, playGuideInfo);
				yield return new WaitForSeconds(_delaySeconds);
				TweenPositionPlayGuideMenu.PlayReverse();
			}
		}
	}

	private void FinishedShowPlayGuide()
	{
		if (_lobbyManagement != null)
		{
			_lobbyManagement.CheckUpdatePlayGuide = true;
		}
	}
}
