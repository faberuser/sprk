using System;
using System.Collections.Generic;
using System.Linq;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa;
using NVespa.NGlobal;
using UnityEngine;

namespace NGame2.NUI.NWindow.BanPick;

public class PhaseBase
{
	public interface IListener
	{
		List<int> GetCurrentSelectedHeroList();

		Dictionary<int, HeroPortraitComponent> GetAllHeroOnGrid();

		bool CanSelect();

		void SetTimer(float remainTime, float gaugeRatio, bool isImminent);

		void RandomPick(MatchBanPickState matchBanPickState, int[] heroIndices, bool isPickMode);

		void StartBattle(MatchedAccountInfo[] accountInfos);

		void CloseInspectHero();
	}

	protected IListener _listener;

	protected MatchBanPick.Player _me;

	protected MatchBanPick.Player _opponent;

	protected float _limitTimePerHero = 10f;

	protected float _imminentTime;

	private const float AUTO_BAN_PICK_TIME = 4f;

	protected bool _pending;

	protected float _limitTime;

	protected float _startTime;

	protected float _animationEndTime;

	protected bool _waitAnimation;

	protected bool _isTimeOver;

	protected MatchedAccountInfo[] _recvAccountInfos;

	protected BanPickAccountInfo[] _recvBanPickAccountInfos;

	protected int[] _recvHeroIndices;

	public MatchBanPickState MatchBanPickState { get; protected set; }

	public MatchBanPickStateData StateData { get; protected set; }

	public PhaseBase(MatchBanPick.Player me, MatchBanPick.Player opponent, MatchBanPickState state, IListener listener)
	{
		if (me.AccountInfo == null || opponent.AccountInfo == null)
		{
			VespaDebug.LogError("Player acccount not assigned ");
		}
		if (listener == null)
		{
			VespaDebug.LogError("Listener is null");
		}
		_listener = listener;
		MatchBanPickState = state;
		_me = me;
		_opponent = opponent;
		StateData = GameRoot.Instance.MatchBanPickStateDataContainer.GetData(state);
		_limitTimePerHero = GameRoot.Instance.ConstantDataContainer.GetValueInt("MatchingBanPickTimePerHero");
		_imminentTime = GameRoot.Instance.ConstantDataContainer.GetValueInt("MatchingBanPickTimeImminent");
	}

	public virtual bool IsHidePickHeroList()
	{
		return !IsMyturn();
	}

	public virtual int GetDeckSlotIndicateCount()
	{
		return GetSelectableCount();
	}

	public virtual bool IsHideDeckMine()
	{
		return !IsMyturn();
	}

	public virtual bool IsHideDeckOpponent()
	{
		return IsMyturn();
	}

	public virtual void OnEnd()
	{
	}

	public virtual void OnBegin()
	{
		_pending = false;
		_waitAnimation = true;
		_animationEndTime = Time.realtimeSinceStartup;
	}

	public virtual void OnEndAnimation()
	{
		_waitAnimation = false;
		_animationEndTime = Time.realtimeSinceStartup;
	}

	public virtual Tuple<MatchedAccountInfo, Dictionary<int, CachedHeroInfo>, BanPickAccountInfo, BanPickAccountInfo> GetCurrentTurnHeroInfos()
	{
		MatchedAccountInfo targetMatchAccountInfo = null;
		MatchedAccountInfo noneTargetMatchAccountInfo = null;
		bool flag = IsMyturn();
		if (IsPickMode())
		{
			targetMatchAccountInfo = (flag ? _me.AccountInfo : (noneTargetMatchAccountInfo = _opponent.AccountInfo));
			noneTargetMatchAccountInfo = (flag ? _opponent.AccountInfo : _me.AccountInfo);
		}
		else
		{
			targetMatchAccountInfo = (flag ? _opponent.AccountInfo : _me.AccountInfo);
			noneTargetMatchAccountInfo = (flag ? _me.AccountInfo : _opponent.AccountInfo);
		}
		BanPickAccountInfo banPickAccountInfo = Array.Find(_recvBanPickAccountInfos, (BanPickAccountInfo e) => e.AccountId == targetMatchAccountInfo.AccountId);
		BanPickAccountInfo banPickAccountInfo2 = Array.Find(_recvBanPickAccountInfos, (BanPickAccountInfo e) => e.AccountId == noneTargetMatchAccountInfo.AccountId);
		if (banPickAccountInfo == null || banPickAccountInfo2 == null)
		{
			return null;
		}
		Dictionary<int, CachedHeroInfo> i_item = (IsHideHeroList() ? null : targetMatchAccountInfo.HeroInfos);
		return new Tuple<MatchedAccountInfo, Dictionary<int, CachedHeroInfo>, BanPickAccountInfo, BanPickAccountInfo>(targetMatchAccountInfo, i_item, banPickAccountInfo, banPickAccountInfo2);
	}

	private bool IsHideHeroList()
	{
		if (IsMyturn())
		{
			return false;
		}
		return GameRoot.Instance.ConstantDataContainer.GetValueBool("MatchingBanPickAllHeroBan");
	}

	public virtual void RandomBanPick()
	{
		_listener.CloseInspectHero();
		List<int> list = new List<int>(_listener.GetCurrentSelectedHeroList());
		List<HeroPortraitComponent> list2 = _listener.GetAllHeroOnGrid().Values.Where((HeroPortraitComponent e) => !e.IsDisabled && !e.IsBanned && !e.IsSelected).ToList();
		if (list2.Count < GetSelectableCount())
		{
			VespaDebug.LogError("heroComponents.Count less than selectableCount");
			return;
		}
		while (list.Count < GetSelectableCount())
		{
			int index = UnityEngine.Random.Range(0, list2.Count);
			HeroPortraitComponent heroPortraitComponent = list2[index];
			if (list.Count <= 0 || !list.Contains(heroPortraitComponent.CreatureData.Index))
			{
				if (IsPickMode())
				{
					heroPortraitComponent.SetPortraitState(EHeroState.Party);
				}
				else
				{
					heroPortraitComponent.SetBan(ban: true);
				}
				list.Add(heroPortraitComponent.CreatureData.Index);
				list2.Remove(heroPortraitComponent);
			}
		}
		_listener.RandomPick(MatchBanPickState, list.ToArray(), IsPickMode());
	}

	public void SetRecvInfo(MatchedAccountInfo[] accountInfos, BanPickAccountInfo[] banPickAccountInfos, int[] heroIndices)
	{
		_recvAccountInfos = accountInfos;
		_recvBanPickAccountInfos = banPickAccountInfos;
		_recvHeroIndices = heroIndices;
	}

	public void SetRemainTime(float remainTime)
	{
		_isTimeOver = false;
		_limitTime = remainTime + 1f;
	}

	public void Update()
	{
		UpdateTimer();
	}

	public void UpdateTimer()
	{
		if (_isTimeOver || _waitAnimation)
		{
			return;
		}
		float num = Time.realtimeSinceStartup - _animationEndTime;
		if (num > _limitTime)
		{
			if (IsMyturn() && _listener.CanSelect())
			{
				RandomBanPick();
			}
			_isTimeOver = true;
		}
		else
		{
			float num2 = _limitTime - num;
			float gaugeRatio = Mathf.Clamp01(num2 / GetWaitTime());
			_listener.SetTimer(num2, gaugeRatio, num2 < _imminentTime);
		}
	}

	private float GetWaitTime()
	{
		if (StateData != null)
		{
			return StateData.WaitTime4Deck;
		}
		return _limitTimePerHero * (float)GetSelectableCount();
	}

	public int[] GetPickHeroIndices(long accountId)
	{
		BanPickAccountInfo banPickAccountInfo = Array.Find(_recvBanPickAccountInfos, (BanPickAccountInfo e) => e.AccountId == accountId);
		if (banPickAccountInfo == null || banPickAccountInfo.PickHeroIndices == null)
		{
			return null;
		}
		return banPickAccountInfo.PickHeroIndices;
	}

	public int[] GetBanHeroIndices(long accountId)
	{
		BanPickAccountInfo banPickAccountInfo = Array.Find(_recvBanPickAccountInfos, (BanPickAccountInfo e) => e.AccountId == accountId);
		if (banPickAccountInfo == null || banPickAccountInfo.BanHeroIndices == null)
		{
			return null;
		}
		return banPickAccountInfo.BanHeroIndices;
	}

	public string GetBeginAnimName()
	{
		if (StateData == null)
		{
			VespaDebug.LogError("GetAnimationStateName :: StateData == null");
			return string.Empty;
		}
		string[] array = (Is1P() ? StateData.AnimMe1P : StateData.AnimMe2P);
		if (array == null || array.Length == 0)
		{
			return string.Empty;
		}
		return array[0].Trim();
	}

	public string GetEndAnimName()
	{
		if (StateData == null)
		{
			VespaDebug.LogError("GetAnimationStateName :: StateData == null");
			return string.Empty;
		}
		string[] array = (Is1P() ? StateData.AnimMe1P : StateData.AnimMe2P);
		if (array == null || array.Length < 2)
		{
			return string.Empty;
		}
		return array[1].Trim();
	}

	public bool IsLockAnimationPlayPhase()
	{
		return MatchBanPickState == MatchBanPickState.PHASE9_ALL_WAIT_SETTING;
	}

	public int GetSelectableCount()
	{
		if (StateData == null)
		{
			return 0;
		}
		return StateData.SelectableCount4Deck;
	}

	public bool IsTurnOf1P()
	{
		if (StateData == null)
		{
			return false;
		}
		return StateData.TeamType == TeamType.Left;
	}

	public bool Is1P()
	{
		return _me.AccountInfo.TeamType == TeamType.Left;
	}

	public bool IsMyturn()
	{
		if (StateData == null)
		{
			return false;
		}
		if (StateData.TeamType == TeamType.All)
		{
			return true;
		}
		return StateData.TeamType == TeamType.Left == Is1P();
	}

	public bool IsPickMode()
	{
		if (StateData == null)
		{
			return false;
		}
		return StateData.IsPick;
	}

	public string GetDescMessage()
	{
		if (StateData == null)
		{
			return string.Empty;
		}
		return LocalizationManager.GetStringDirect(Is1P() ? StateData.LocalMessage1P : StateData.LocalMessage2P, StateData.SelectableCount4Deck);
	}
}
