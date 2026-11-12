using System;
using System.Collections;
using System.Collections.Generic;
using NGame2.NBattleContext;
using NGame2.NBattleField.NManager.NPlay;
using NGame2.NParty;
using NGame2.NScene;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow.NBattle;

public class EndBattleLose : FrameControlWindow
{
	public interface Listener
	{
		void BattleRepeat();

		void ReturnToLobby();

		void ShowStatistics();

		void ReturnToLobbyAndOpenHeroDeck();

		void ReturnToLobbyAndOpenPartySetting();

		void ReturnToLobbyAndOpenInventory();
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Battle/EndBattleLose";

	private static WindowPrefab<EndBattleLose> _instance = new WindowPrefab<EndBattleLose>("Resources/Prefab2/Window/Battle/EndBattleLose", GameUIRoot.ELayerType.UI);

	private Action _reservedAction;

	public UIButton RepeatButton;

	public UIButton Button_Inventory;

	public UIButton Button_PartySetting;

	public UIButton Button_HeroDeck;

	public List<BattleType> DisableButtonBattleTypeList;

	[SerializeField]
	private UIGame3DWidget _loseEffect;

	private Listener _listener;

	private Coroutine _playEffectCo;

	public float AnimationDelay;

	private float _openedTime;

	private bool _animationFinished;

	private IDataProvider _dataProvider;

	public static EndBattleLose instance => _instance.instance;

	public void Open(Listener listener, IDataProvider dataProvider = null)
	{
		Open(delegate
		{
			Init(listener, dataProvider);
		}, null);
	}

	private void Init(Listener listener, IDataProvider dataProvider = null)
	{
		_openedTime = Time.time;
		if (_playEffectCo != null)
		{
			VespaCoroutine.Stop(this, _playEffectCo);
		}
		_playEffectCo = VespaCoroutine.Start(this, coPlayEffect());
		_listener = listener;
		_dataProvider = dataProvider;
		UpdateRepeatable();
		SetLeftButtons();
	}

	private void SetLeftButtons()
	{
		bool flag = true;
		bool flag2 = true;
		bool flag3 = true;
		if (_dataProvider != null)
		{
			BattleDefineData battleDefine = _dataProvider.GetBattleDefine();
			if (battleDefine != null)
			{
				flag = !battleDefine.DisableInventoryInResult;
				flag2 = !battleDefine.DisablePartySettingInResult;
				flag3 = !battleDefine.DisableHeroListInResult;
			}
		}
		bool flag4 = ActiveButton();
		UIHelper.EnableButton(Button_Inventory, flag4 && flag, grayscaleIfDisable: true);
		UIHelper.EnableButton(Button_PartySetting, flag4 && flag2, grayscaleIfDisable: true);
		UIHelper.EnableButton(Button_HeroDeck, flag4 && flag3, grayscaleIfDisable: true);
	}

	private bool ActiveButton()
	{
		if (AutoRepeatBattle.IsEnabled() && AutoRepeatBattle.IsPartyPlay)
		{
			return false;
		}
		if (__Extension_Method_ICollection__.COUNT(DisableButtonBattleTypeList) == 0)
		{
			return true;
		}
		BattleType battleType = SingletonAuto<BattleContext>.instance.DataProvider.GetBattleType();
		if (battleType == BattleType.GatewaytoTrial)
		{
			return false;
		}
		return !DisableButtonBattleTypeList.Contains(battleType);
	}

	private IEnumerator coPlayEffect()
	{
		_loseEffect.Stop(immediatelyClear: true);
		yield return null;
		_loseEffect.Play(null);
	}

	private void UpdateRepeatable()
	{
		bool enable = SingletonAuto<BattleContext>.instance.DataProvider.CanRetryBattle(BattleRetryConditionType.Lose);
		UIHelper.EnableButton(RepeatButton, enable, grayscaleIfDisable: true);
	}

	public void SetAnimationFinishedAction(Action action)
	{
		if (_animationFinished)
		{
			Vespa_System_Action_Extension_Method.fire(action);
		}
		else
		{
			_reservedAction = action;
		}
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		if (Time.time - _openedTime >= AnimationDelay)
		{
			_animationFinished = true;
		}
		if (_animationFinished && _reservedAction != null)
		{
			Vespa_System_Action_Extension_Method.fire(_reservedAction);
			_reservedAction = null;
		}
	}

	public void OnClickRetry()
	{
		if (!CheckRepeatPartyBattle() && _listener != null)
		{
			_listener.BattleRepeat();
		}
	}

	public void OnClickToMap()
	{
		if (AutoRepeatBattle.IsEnabled() && AutoRepeatBattle.IsPartyPlay)
		{
			GlobalAutoBehaviour<PartyManager>.instance.SendLeaveParty();
			LobbyScene2.ListReturnState.Clear();
			SingletonAuto<BattleContext>.instance.MoveSceneToLobby(null, EBoolCheck.DontCare);
			return;
		}
		if (AutoNextBattle.IsEnabled())
		{
			AutoBattle.Stop();
		}
		if (_listener != null)
		{
			_listener.ReturnToLobby();
		}
	}

	public void OnClickStatistics()
	{
		if (!CheckRepeatPartyBattle() && _listener != null)
		{
			_listener.ShowStatistics();
		}
	}

	public void OnClickReturnOpenHeroList()
	{
		if (!CheckRepeatPartyBattle() && _listener != null)
		{
			_listener.ReturnToLobbyAndOpenHeroDeck();
		}
	}

	public void OnClickToHeroDeck()
	{
		if (!CheckRepeatPartyBattle() && _listener != null)
		{
			_listener.ReturnToLobbyAndOpenHeroDeck();
		}
	}

	public void OnClickToPartySetting()
	{
		if (!CheckRepeatPartyBattle() && _listener != null)
		{
			_listener.ReturnToLobbyAndOpenPartySetting();
		}
	}

	public void OnClickToInventory()
	{
		if (!CheckRepeatPartyBattle() && _listener != null)
		{
			_listener.ReturnToLobbyAndOpenInventory();
		}
	}

	public bool CheckRepeatPartyBattle()
	{
		if (AutoRepeatBattle.IsEnabled() && AutoRepeatBattle.IsPartyPlay)
		{
			CenterMessage.Open(LocalizationManager.GetStringDirect("REPEATBATTLE_MULTI_DISABLE_CONTENTS"));
			return true;
		}
		return false;
	}
}
