using NGame2.NUI.NManager;
using NShared;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow.NBattle;

public class BattleChat : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/Battle/BattleChat";

	public Transform OpenedRoot;

	public void OpenChat()
	{
		base.Open();
		UIHelper.EnableGameObject(OpenedRoot, enable: false);
	}

	public override void OnClosing()
	{
		base.OnClosing();
	}

	public void OnClickChat()
	{
		SceneAutoBehaviour<ChattingManagement>.instance.ToggleChatting(ChatType.None, 0L);
	}

	public void OnClickClose()
	{
		SceneAutoBehaviour<ChattingManagement>.instance.CloseAllActiveWindow();
	}
}
