using NGame2.NBattleView.NUIOn3D;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow.NChatting;
using NShared;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow.NBattle;

public class EmoticonPopupWindow : BaseWindow, ChattingEmoticonSelectPopup.Listener
{
	public const string PrefabPath = "Resources/Prefab2/Window/Battle/EmoticonPopupWindow";

	public Transform EmoticonRoot;

	public Transform EmoticonSelectPopupRoot;

	private BattleChattingEmoticonOnUI _battleEmoticonComp;

	public GameObject DisableRoot;

	public UILabel Label_Disable;

	private BattleType _battleType;

	public override void OnClosing()
	{
		base.OnClosing();
		BaseBattleChattingEmoticon.ReleaseInstance(_battleEmoticonComp);
		_battleEmoticonComp = null;
	}

	public void Open(BattleType battleType)
	{
		_battleType = battleType;
		base.Open();
	}

	public void OnClickEmotionPopup()
	{
		SceneAutoBehaviour<ChattingManagement>.instance.OpenBattleEmoticonSelectPopup(this, _battleType);
	}

	public void ShowEmoticon(EmoticonData data, string nickName)
	{
		if (data != null)
		{
			if (_battleEmoticonComp == null)
			{
				_battleEmoticonComp = BattleChattingEmoticonOnUI.PrefabAuto.Factory.Create(EmoticonRoot);
			}
			_battleEmoticonComp.Show(data, nickName);
		}
	}

	public int GetPartyChatRemainTime()
	{
		return SceneAutoBehaviour<ChattingManagement>.instance.GetChatRemainTime(ChatType.Party);
	}

	private void SetupDisableState(bool show)
	{
		if (DisableRoot != null && DisableRoot.activeSelf != show)
		{
			UIHelper.EnableGameObject(DisableRoot, show);
		}
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		UpdateEmoticon();
		UpdateDisableState();
	}

	private void UpdateEmoticon()
	{
		if (_battleEmoticonComp != null)
		{
			_battleEmoticonComp.ChainUpdate();
		}
	}

	public void UpdateDisableState()
	{
		if (CheckEnableChatting())
		{
			int partyChatRemainTime = GetPartyChatRemainTime();
			bool flag = partyChatRemainTime > 0;
			SetupDisableState(flag);
			if (flag)
			{
				UIHelper.SetLabelText(Label_Disable, partyChatRemainTime.ToString());
			}
		}
	}

	private bool CheckEnableChatting()
	{
		return false;
	}

	bool ChattingEmoticonSelectPopup.Listener.CheckEnableChatting()
	{
		return CheckEnableChatting();
	}

	Vector3 ChattingEmoticonSelectPopup.Listener.GetRootPosition()
	{
		if (EmoticonSelectPopupRoot == null)
		{
			return default(Vector3);
		}
		return EmoticonSelectPopupRoot.position;
	}

	ChatType ChattingEmoticonSelectPopup.Listener.GetChatType()
	{
		return ChatType.Party;
	}
}
