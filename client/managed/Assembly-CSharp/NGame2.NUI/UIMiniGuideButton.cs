using NGame2.NUI.NManager.NLobby;
using NShared;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI;

[RequireComponent(typeof(UIButton))]
public class UIMiniGuideButton : MonoBehaviour
{
	[SerializeField]
	private int _miniGuideIndex;

	private UIButton _button;

	private void Awake()
	{
		_button = GetComponent<UIButton>();
		_button.onClick.Add(new EventDelegate(delegate
		{
			OnOpenMiniGuide();
		}));
	}

	private void OnOpenMiniGuide()
	{
		GameMiniGuideData data = GameRoot.Instance.GameMiniGuideDataContainer.GetData(_miniGuideIndex);
		if (data == null)
		{
			VespaDebug.LogError("UIMiniGuideButton.OnOpenMiniGuide fail. gameMiniGuideData is null. _miniGuideIndex=" + _miniGuideIndex);
		}
		else if (!SceneAutoBehaviour<LobbyManagement>.isValidInstance)
		{
			VespaDebug.LogError("UIMiniGuideButton.OnOpenMiniGuide fail. LobbyManagement.isValidInstance is false. _miniGuideIndex=" + _miniGuideIndex);
		}
		else
		{
			SceneAutoBehaviour<LobbyManagement>.instance.OpenMiniGuidePopup(data);
		}
	}
}
