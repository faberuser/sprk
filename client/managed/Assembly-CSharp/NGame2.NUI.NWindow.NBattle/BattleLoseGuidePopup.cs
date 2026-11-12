using System;
using System.Collections.Generic;
using NGame2.NParty;
using NShared;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow.NBattle;

public class BattleLoseGuidePopup : BaseWindow
{
	public interface Listener
	{
		void ReturnToLobbyAndGameGuide();
	}

	public const string PrefabPath = "Resources/Prefab2/Window/MessagePopup/BattleLoseGuidePopup";

	public static PrefabAuto<BattleLoseGuidePopup> PrefabAuto = new PrefabAuto<BattleLoseGuidePopup>("Resources/Prefab2/Window/MessagePopup/BattleLoseGuidePopup");

	private Listener _listenr;

	private BattleType _battleType;

	private BattleDefineData _battleDefine;

	public UIButton Button_Guide;

	public List<BattleType> GuideAbleBattleType;

	public void Open(Listener listenr, BattleType battleType, BattleDefineData battleDefine, Action onComplete)
	{
		base.Open(onComplete);
		_listenr = listenr;
		_battleType = battleType;
		_battleDefine = battleDefine;
		SetGuideButton();
	}

	private bool ActiveGuide()
	{
		if (_battleDefine != null && string.Equals(_battleDefine.Code, "SingleRaid"))
		{
			return false;
		}
		if (GlobalAutoBehaviour<PartyManager>.instance.IsValidParty())
		{
			return false;
		}
		if (GuideAbleBattleType.Contains(_battleType))
		{
			return true;
		}
		return false;
	}

	private void SetGuideButton()
	{
		UIHelper.EnableGameObject(Button_Guide, ActiveGuide());
	}

	public void OnClickGameGuide()
	{
		if (_listenr != null)
		{
			_listenr.ReturnToLobbyAndGameGuide();
		}
	}
}
