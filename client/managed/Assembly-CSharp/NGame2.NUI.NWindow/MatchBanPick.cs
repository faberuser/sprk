using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NGame2.NAccount;
using NGame2.NMatch;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow.BanPick;
using NGameAsset;
using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using NVespa.NUtil;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class MatchBanPick : BaseWindow, HeroPortraitComponent.Listener, MatchBanPickSlotComponent.Listener, PhaseBase.IListener
{
	public interface Listener
	{
		void OnSelectConfirm(MatchBanPickState state, int[] heroIndices, bool isPenalty = false);

		void OnMatchBanPickClose();
	}

	public interface IDataProvider
	{
		int GetBanPickMemberHeroIndex(int slotIndex, bool isPick);

		void SetBanPickMemberHeroIndex(int slotIndex, int heroIndex, bool isPick = true);
	}

	[Serializable]
	public class Player
	{
		public UIGrid DeckGrid;

		public UILabel Nick;

		public UITweener HighlightTween;

		public UITexture TextureBan1;

		public Transform PickNoti;

		public Transform BanNoti;

		public Transform SlotShadow;

		[HideInInspector]
		public List<MatchBanPickSlotComponent> DeckSlotList = new List<MatchBanPickSlotComponent>();

		[HideInInspector]
		public MatchedAccountInfo AccountInfo;
	}

	private class LockAnimationStock
	{
		public Transform Trs;

		public Vector3 From;

		public Vector3 To;
	}

	public class StateSnapshot
	{
		public MatchBanPickState state;

		public MatchedAccountInfo[] accountInfos;

		public BanPickAccountInfo[] banPickAccountInfos;

		public int[] heroIndices;

		public int remainTime;

		public StateSnapshot(MatchBanPickState state, MatchedAccountInfo[] accountInfos, BanPickAccountInfo[] banPickAccountInfos, int[] heroIndices, int remainTime)
		{
			this.state = state;
			this.accountInfos = accountInfos;
			this.banPickAccountInfos = banPickAccountInfos;
			this.heroIndices = heroIndices;
			this.remainTime = remainTime;
		}
	}

	[Serializable]
	public class StateSound
	{
		public MatchBanPickState State;

		public List<SoundObjectTransform> TransformList;
	}

	[Serializable]
	public class SoundObjectTransform
	{
		public bool PlayOnMyturn;

		public Transform Transform;
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Match/MatchBanPickRenewal";

	public const int DefaultPartySlotCount = 5;

	public Player Player1;

	public Player Player2;

	public MatchBanPickSlotComponent PartySlotTemplate;

	public Transform PickHeroListShadow;

	public Transform WaitingLabel;

	public UILabel DescBan;

	public UILabel DescPick;

	public UITweener DescTween;

	public UILabel RemainTime;

	public Transform TimeGaugeRoot;

	public UISprite TimeGaugeNormal;

	public UISprite TimeGaugeImminent;

	public TweenColor TimeGaugeTween;

	public UIButton ConfirmButton;

	public UIScrollGrid HeroGrid;

	public HeroPortraitComponent.PrefabSelector HeroPortraitSelector = new HeroPortraitComponent.PrefabSelector(HeroPortraitComponent.PrefabSelector.EPrefabType.Short);

	public Transform BanAnimPanel;

	public Transform BanAnimatorPrefab;

	public Transform FlagAnimPanel;

	public Transform FlagAnimatorPrefab;

	public UIGame3DWidget TimeSound;

	public UIToggle ToggleAll;

	public UIToggle ToggleKnight;

	public UIToggle ToggleWarrior;

	public UIToggle ToggleAssassin;

	public UIToggle ToggleArcher;

	public UIToggle ToggleMechanic;

	public UIToggle ToggleWizard;

	public UIToggle TogglePriest;

	public float LockAnimaionWaitTime = 1.5f;

	public float LockSlotMoveTime = 0.5f;

	public float StartBattleDelay = 2f;

	public float AlphaTime = 0.5f;

	public Transform[] TransformArrayActiveOnBan;

	public Transform[] TransformArrayActiveOnPick;

	private bool _isLock;

	private Animator _flagAnimator;

	private UISprite _currentTimeGauge;

	private Player _me;

	private Player _opponent;

	private Listener _listener;

	private List<int> _pickList = new List<int>();

	private List<int> _banList = new List<int>();

	private Dictionary<int, HeroPortraitComponent> _heroPortraitDic = new Dictionary<int, HeroPortraitComponent>();

	private List<HeroPortraitComponent> _confirmPortraitList = new List<HeroPortraitComponent>();

	protected IDataProvider _dataProvider;

	private Dictionary<MatchBanPickState, PhaseBase> _handlerDictionary = new Dictionary<MatchBanPickState, PhaseBase>();

	private PhaseBase _currentPhase;

	private string _preEndAnimmaionName = string.Empty;

	private List<GameObject> _animGameObjectList = new List<GameObject>();

	private List<MatchBanPickSlotComponent> _lockSlotList = new List<MatchBanPickSlotComponent>();

	private CreatureTagType _currentClassTab = CreatureTagType.End;

	public Transform LockAnimPanel;

	public Transform LockAnimTemplate;

	private List<LockAnimationStock> _stockList = new List<LockAnimationStock>();

	private const string LastBanLockAnimName = "HonorLeague_LastBan_Chain_1";

	private const string FirstBanAnimName = "HonorLeague_BanEff_1";

	public List<StateSound> StateActiveObejctContainer;

	private List<StateSnapshot> _snapshotList = new List<StateSnapshot>();

	public bool GiveUpPending { get; set; }

	public object _currentTab { get; private set; }

	protected void ApplySnapshot()
	{
		if (__Extension_Method_ICollection__.COUNT(_snapshotList) != 0)
		{
			VespaCoroutine.Start(coChangeState(GetState(_snapshotList[0].state), _snapshotList[0].accountInfos, _snapshotList[0].banPickAccountInfos, _snapshotList[0].heroIndices, _snapshotList[0].remainTime));
			_snapshotList.RemoveAt(0);
		}
	}

	protected void InputForTest()
	{
		if (Input.GetKeyDown(KeyCode.F2))
		{
			ApplySnapshot();
		}
	}

	public void Open(Listener listener, MatchedAccountInfo[] matchedAccountInfo)
	{
		base.Open();
		Init(listener, matchedAccountInfo);
	}

	private void Init(Listener listener, MatchedAccountInfo[] matchedAccountInfo)
	{
		Reset();
		_listener = listener;
		_flagAnimator = CreateAnimator(FlagAnimatorPrefab, FlagAnimPanel);
		_dataProvider = new BanPickSettingDataProdiver(GetType().Name);
		_isLock = false;
		InitPlayerDeck(Player1);
		InitPlayerDeck(Player2);
		AssignAccountInfo(matchedAccountInfo);
		InitUIs();
		InitPhaseState();
		DisableSoundObjects();
	}

	private void AssignAccountInfo(MatchedAccountInfo[] matchedAccountInfo)
	{
		if (__Extension_Method_ICollection__.COUNT(matchedAccountInfo) == 0)
		{
			VespaDebug.LogError("Error : matchedAccountInfo null");
			return;
		}
		MatchedAccountInfo matchedAccountInfo2 = matchedAccountInfo.FirstOrDefault((MatchedAccountInfo e) => e.AccountId == GlobalAutoBehaviour<AccountManager>.instance.UserManager.Id);
		MatchedAccountInfo matchedAccountInfo3 = matchedAccountInfo.FirstOrDefault((MatchedAccountInfo e) => e.AccountId != GlobalAutoBehaviour<AccountManager>.instance.UserManager.Id);
		if (matchedAccountInfo2 == null || matchedAccountInfo3 == null)
		{
			VespaDebug.LogError("Error : info null");
			return;
		}
		PrintAccountInfo("-- myInfo --", matchedAccountInfo2);
		PrintAccountInfo("-- opponentInfo --", matchedAccountInfo3);
		_me = Player1;
		_opponent = Player2;
		_me.AccountInfo = matchedAccountInfo2;
		_opponent.AccountInfo = matchedAccountInfo3;
	}

	private void PrintAccountInfo(string prefix, MatchedAccountInfo info)
	{
		if (!Define.IsLiveBuild())
		{
			if (info == null)
			{
				VespaDebug.LogError(" MatchedAccountInfo is null ");
				return;
			}
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(prefix);
			stringBuilder.Append($" Id: {info.AccountId}, Nick : {info.Nick}");
			stringBuilder.Append($" TeamType : {info.TeamType}");
			VespaDebug.LogError(stringBuilder.ToString());
		}
	}

	private Animator CreateAnimator(Transform animatorTemplete, Transform parent)
	{
		if (animatorTemplete == null || parent == null)
		{
			VespaDebug.LogError("CreateAnimator :: animatorTemplete == null || parent == null");
			return null;
		}
		animatorTemplete.gameObject.SetActive(value: false);
		Transform obj = UnityEngine.Object.Instantiate(animatorTemplete);
		obj.gameObject.SetActive(value: true);
		obj.parent = parent.transform;
		Vespa_Unity_Transform_Extension_Method.ResetLocalTM(obj.transform);
		return obj.GetComponent<Animator>();
	}

	private void InitUIs()
	{
		UIHelper.EnableGameObject(PartySlotTemplate, enable: false);
		_me.HighlightTween.ResetToBeginning();
		_opponent.HighlightTween.ResetToBeginning();
		DescTween.ResetToBeginning();
		UIHelper.SetLabelText(Player1.Nick, _me.AccountInfo.Nick);
		string stringDirect = LocalizationManager.GetStringDirect("MATCH_BANPICK_CHALLENGER_NICKNAME");
		UIHelper.SetLabelText(Player2.Nick, stringDirect);
		SetDescLabel(LocalizationManager.GetStringDirect("MATCH_BANPICK_WAITING_MSG"));
		UIHelper.EnableButton(ConfirmButton, enable: false);
		UIHelper.EnableGameObject(TimeGaugeRoot, enable: true);
		UIHelper.EnableGameObject(Player1.SlotShadow, enable: true);
		UIHelper.EnableGameObject(Player2.SlotShadow, enable: true);
		InitTrsArryAlpha();
		InitTimeUI();
	}

	private void SetDescLabel(string desc)
	{
		if (_currentPhase == null)
		{
			UIHelper.EnableGameObject(DescBan, enable: true);
			UIHelper.SetLabelText(DescBan, LocalizationManager.GetStringDirect("MATCH_BANPICK_WAITING_MSG"));
			UIHelper.EnableGameObject(DescPick, enable: false);
			UIHelper.SetLabelText(DescPick, string.Empty);
		}
		else
		{
			UIHelper.EnableGameObject(DescPick, _currentPhase.IsPickMode());
			UIHelper.SetLabelText(DescPick, _currentPhase.IsPickMode() ? desc : string.Empty);
			UIHelper.EnableGameObject(DescBan, !_currentPhase.IsPickMode());
			UIHelper.SetLabelText(DescBan, _currentPhase.IsPickMode() ? string.Empty : desc);
		}
	}

	private void InitPhaseState()
	{
		RegistPhase(new None(_me, _opponent, this));
		RegistPhase(new PHASE1_ALL_BAN_ALL(_me, _opponent, this));
		RegistPhase(new PHASE2_1P_PICK_1P(_me, _opponent, this));
		RegistPhase(new PHASE3_2P_PICK_2P(_me, _opponent, this));
		RegistPhase(new PHASE4_1P_PICK_1P(_me, _opponent, this));
		RegistPhase(new PHASE5_2P_PICK_2P(_me, _opponent, this));
		RegistPhase(new PHASE6_1P_PICK_1P(_me, _opponent, this));
		RegistPhase(new PHASE7_2P_PICK_2P(_me, _opponent, this));
		RegistPhase(new PHASE8_ALL_BAN_EACH_PICK(_me, _opponent, this));
		RegistPhase(new PHASE9_ALL_WAIT_SETTING(_me, _opponent, this));
		RegistPhase(new PHASE10_BATTLE_START(_me, _opponent, this));
		_currentPhase = GetPhaseHandler(MatchBanPickState.None);
	}

	private void RegistPhase(PhaseBase stateBase)
	{
		_handlerDictionary[stateBase.MatchBanPickState] = stateBase;
	}

	private PhaseBase GetPhaseHandler(MatchBanPickState state)
	{
		PhaseBase value = null;
		if (_handlerDictionary.TryGetValue(state, out value))
		{
			return value;
		}
		VespaDebug.LogError("GetStateHandler Faile state : " + state);
		return new PhaseBase(_me, _opponent, MatchBanPickState.None, this);
	}

	private void InitTrsArryAlpha()
	{
		Array.ForEach(TransformArrayActiveOnPick, delegate(Transform item)
		{
			UIWidget component = item.GetComponent<UIWidget>();
			if (component != null)
			{
				component.alpha = 0f;
			}
		});
	}

	private void InitTimeUI()
	{
		UIHelper.EnableGameObject(TimeGaugeNormal, enable: true);
		UIHelper.EnableGameObject(TimeGaugeImminent, enable: false);
		UIHelper.SetSpriteFillAmount(TimeGaugeNormal, 1f);
		UIHelper.SetSpriteFillAmount(TimeGaugeImminent, 1f);
		UIHelper.SetLabelText(RemainTime, "-- : --");
	}

	private void InitPlayerDeck(Player player)
	{
		FactoryBase.ReleaseAndClearList(player.DeckSlotList);
		for (int i = 0; i < 5; i++)
		{
			GameObject obj = UnityEngine.Object.Instantiate(PartySlotTemplate.gameObject);
			obj.transform.parent = (player.DeckGrid ? player.DeckGrid.transform : PartySlotTemplate.transform.parent);
			obj.transform.localScale = PartySlotTemplate.transform.localScale;
			obj.transform.localRotation = PartySlotTemplate.transform.localRotation;
			UIHelper.EnableGameObject(obj, enable: true);
			MatchBanPickSlotComponent component = obj.GetComponent<MatchBanPickSlotComponent>();
			component.SetState(MatchBanPickSlotComponent.SlotState.None);
			component.gameObject.SetActive(value: true);
			player.DeckSlotList.Add(component);
		}
		player.DeckGrid.Reposition();
	}

	public override void Reset()
	{
		base.Reset();
		_listener = null;
		ResetPlayerDeck(Player1);
		ResetPlayerDeck(Player2);
		ClearHeroGrid();
		_lockSlotList.ForEach(delegate(MatchBanPickSlotComponent e)
		{
			VespaObject.DestroyGo(e);
		});
		_lockSlotList.Clear();
		_animGameObjectList.ForEach(delegate(GameObject e)
		{
			VespaObject.DestroyGo(e);
		});
		_animGameObjectList.Clear();
		if (_flagAnimator != null)
		{
			VespaObject.DestroyGo(_flagAnimator.gameObject);
			_flagAnimator = null;
		}
	}

	private void ResetPlayerDeck(Player player)
	{
		player.DeckSlotList.ForEach(delegate(MatchBanPickSlotComponent e)
		{
			VespaObject.DestroyGo(e);
		});
		player.DeckSlotList.Clear();
		UIHelper.ReleaseTextureInfo(player.TextureBan1);
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		UpdateState();
		InputForTest();
	}

	private void UpdateState()
	{
		if (_currentPhase != null)
		{
			_currentPhase.Update();
		}
	}

	public void OnRecvChangeState(MatchBanPickState newState, MatchedAccountInfo[] accountInfos, BanPickAccountInfo[] banPickAccountInfos, int[] heroIndices, int remainTime)
	{
		StateSnapshot item = new StateSnapshot(newState, accountInfos, banPickAccountInfos, heroIndices, remainTime);
		_snapshotList.Add(item);
		VespaCoroutine.Start(coChangeState(GetState(newState), accountInfos, banPickAccountInfos, heroIndices, remainTime));
	}

	private PhaseBase GetState(MatchBanPickState newState)
	{
		PhaseBase value = null;
		_handlerDictionary.TryGetValue(newState, out value);
		return value;
	}

	private IEnumerator coChangeState(PhaseBase newState, MatchedAccountInfo[] accountInfos, BanPickAccountInfo[] banPickAccountInfos, int[] heroIndices, int remainTime)
	{
		ChangeState(_currentPhase, newState, accountInfos, banPickAccountInfos, heroIndices, remainTime);
		OnChangeState();
		if (!Define.IsLiveBuild())
		{
			SetStateLog(_currentPhase.MatchBanPickState, banPickAccountInfos);
		}
		_isLock = true;
		_pickList.Clear();
		_banList.Clear();
		yield return ExtensionMethod_VespaCoroutine.StartCoroutineThis(this, coPlayAnimation(_flagAnimator, _preEndAnimmaionName));
		OnEndPhaseEndAnimation();
		yield return ExtensionMethod_VespaCoroutine.StartCoroutineThis(this, coPlayAnimation(_flagAnimator, _currentPhase.GetBeginAnimName()));
		OnEndPhaseBeginAnimationEnd();
		_preEndAnimmaionName = _currentPhase.GetEndAnimName();
		_isLock = false;
	}

	private void ChangeState(PhaseBase curState, PhaseBase newState, MatchedAccountInfo[] accountInfos, BanPickAccountInfo[] banPickAccountInfos, int[] heroIndices, int remainTime)
	{
		if (_currentPhase == curState)
		{
			if (_currentPhase != null)
			{
				_currentPhase.OnEnd();
			}
			_currentPhase = newState;
			if (_currentPhase != null)
			{
				_currentPhase.SetRecvInfo(accountInfos, banPickAccountInfos, heroIndices);
				_currentPhase.SetRemainTime(remainTime);
				_currentPhase.OnBegin();
			}
		}
	}

	private IEnumerator coPlayAnimation(Animator animator, string animationName, Action<Animator> onComplete = null)
	{
		if (String_Extension_Method.IsNullOrEmpty(animationName))
		{
			yield break;
		}
		if (animator == null)
		{
			VespaDebug.LogError("coPlayAnimation :: animator is null");
			yield break;
		}
		animator.Play(animationName, 0);
		yield return new WaitForSeconds(0.1f);
		AnimatorClipInfo[] currentAnimatorClipInfo = animator.GetCurrentAnimatorClipInfo(0);
		if (currentAnimatorClipInfo != null)
		{
			float length = currentAnimatorClipInfo[0].clip.length;
			yield return new WaitForSeconds(length);
			Vespa_System_Action_Extension_Method.fire(onComplete, animator);
		}
	}

	private void OnChangeState()
	{
		HandleSountObjects();
		CloseInspectHero();
		UIHelper.EnableGameObject(PickHeroListShadow, _currentPhase.IsHidePickHeroList());
		UIHelper.EnableGameObject(WaitingLabel, !_currentPhase.IsMyturn());
		UIHelper.EnableButton(ConfirmButton, enable: false);
		SetPlayerDeck(_me);
		SetPlayerDeck(_opponent);
		SetHeroListAndBannedPortrait();
	}

	private void HandleSountObjects()
	{
		ExtensionMethod_VespaCoroutine.StartCoroutineThis(this, CoHandleSoundObject());
	}

	private void DisableSoundObjects()
	{
		foreach (StateSound item in StateActiveObejctContainer)
		{
			if (item != null && item.TransformList != null)
			{
				item.TransformList.ForEach(delegate(SoundObjectTransform trs)
				{
					trs.Transform.gameObject.SetActive(value: false);
				});
			}
		}
	}

	private IEnumerator CoHandleSoundObject()
	{
		if (__Extension_Method_ICollection__.COUNT(StateActiveObejctContainer) != 0)
		{
			DisableSoundObjects();
			yield return null;
			StateActiveObejctContainer.ForEach(PlayStateObject);
		}
	}

	private void PlayStateObject(StateSound stateSound)
	{
		if (stateSound == null || stateSound.TransformList == null || stateSound.State != _currentPhase.MatchBanPickState)
		{
			return;
		}
		stateSound.TransformList.ForEach(delegate(SoundObjectTransform soundTransform)
		{
			if (soundTransform.Transform != null && (!soundTransform.PlayOnMyturn || _currentPhase.IsMyturn()))
			{
				soundTransform.Transform.gameObject.SetActive(value: true);
			}
		});
	}

	public void CloseInspectHero()
	{
		InspectHeroInfo inspectHeroInfo = GlobalAutoBehaviour<PopupManegement>.instance.FindWindow<InspectHeroInfo>();
		if (inspectHeroInfo != null)
		{
			inspectHeroInfo.Close();
		}
	}

	public void SetPlayerDeck(Player player)
	{
		if (player == null || player.AccountInfo == null)
		{
			VespaDebug.LogError("player == null || player.AccountInfo == null");
			return;
		}
		long accountId = player.AccountInfo.AccountId;
		bool flag = accountId == _me.AccountInfo.AccountId;
		int[] pickHeroIndices = _currentPhase.GetPickHeroIndices(accountId);
		int[] banHeroIndices = _currentPhase.GetBanHeroIndices(accountId);
		ReserveLockSlotAnim(player, pickHeroIndices, banHeroIndices, flag);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(player.DeckSlotList); i++)
		{
			MatchBanPickSlotComponent matchBanPickSlotComponent = player.DeckSlotList[i];
			if (i < __Extension_Method_ICollection__.COUNT(pickHeroIndices))
			{
				CachedHeroInfo heroInfo = GetHeroInfo(pickHeroIndices[i], flag);
				matchBanPickSlotComponent.Init(this, heroInfo, !flag, player.AccountInfo.AccountId);
			}
			else
			{
				matchBanPickSlotComponent.InitEmpty();
			}
		}
	}

	private int FindLastBanedHeroIndex(int[] pickedHeroIndices, int[] bannedHeroIndices)
	{
		if (__Extension_Method_ICollection__.COUNT(pickedHeroIndices) == 0 || __Extension_Method_ICollection__.COUNT(bannedHeroIndices) == 0)
		{
			Debug.LogError("pickedHeroIndices.COUNT() == 0 || bannedHeroIndices.COUNT() ==0 ");
			return 0;
		}
		return Array.Find(pickedHeroIndices, (int index) => TypeIterator.Contains(bannedHeroIndices, index));
	}

	private int GetLastBannedHeroIndex(List<MatchBanPickSlotComponent> playerSlotList, int[] pickedHeroIndices, int[] bannedHeroIndices)
	{
		if (__Extension_Method_ICollection__.COUNT(pickedHeroIndices) == 5 && __Extension_Method_ICollection__.COUNT(bannedHeroIndices) == 2)
		{
			return FindLastBanedHeroIndex(pickedHeroIndices, bannedHeroIndices);
		}
		List<int> heroListInPlayerDeck = new List<int>();
		playerSlotList.ForEach(delegate(MatchBanPickSlotComponent slot)
		{
			if (slot.HeroInfo != null)
			{
				heroListInPlayerDeck.Add(slot.HeroInfo.HeroIndex);
			}
		});
		if (__Extension_Method_ICollection__.COUNT(heroListInPlayerDeck) <= __Extension_Method_ICollection__.COUNT(pickedHeroIndices))
		{
			VespaDebug.LogError($" HeroLisInPlayerDeck {__Extension_Method_ICollection__.COUNT(heroListInPlayerDeck)} <=  PickedHeroIndices {__Extension_Method_ICollection__.COUNT(pickedHeroIndices)} ");
			return 0;
		}
		IEnumerable<int> source = heroListInPlayerDeck.Except(pickedHeroIndices);
		if (source.Count() == 0)
		{
			BanPickSettingDataProdiver.PrintListWtihHeroName("heroListPlayerDeck : ", heroListInPlayerDeck);
			BanPickSettingDataProdiver.PrintListWtihHeroName("pickedHeroIndices  : ", new List<int>(pickedHeroIndices));
			VespaDebug.LogError("bannedHero.Count() == 0");
			return 0;
		}
		return source.First();
	}

	private void ReserveLockSlotAnim(Player player, int[] pickedHeroIndices, int[] bannedHeroIndices, bool playerIs1P)
	{
		if (!_currentPhase.IsLockAnimationPlayPhase())
		{
			return;
		}
		List<MatchBanPickSlotComponent> deckSlotList = player.DeckSlotList;
		int lastBannedHeroIndex = GetLastBannedHeroIndex(deckSlotList, pickedHeroIndices, bannedHeroIndices);
		if (lastBannedHeroIndex == 0)
		{
			VespaDebug.LogError("error LastBanned HeroIndex == 0");
			return;
		}
		int lockedHeroSlotIndex = deckSlotList.FindIndex((MatchBanPickSlotComponent deckSlot) => deckSlot.HeroInfo != null && deckSlot.HeroInfo.HeroIndex == lastBannedHeroIndex);
		int toMoveSlotIndex = (playerIs1P ? (__Extension_Method_ICollection__.COUNT(deckSlotList) - 1) : 0);
		Func<int, bool> func = (int index) => toMoveSlotIndex <= index && index < lockedHeroSlotIndex;
		Func<int, bool> func2 = (int index) => toMoveSlotIndex >= index && index > lockedHeroSlotIndex;
		Func<int, bool> i_func = (playerIs1P ? func2 : func);
		int num = ((!playerIs1P) ? 1 : (-1));
		LockAnimationStock lockAnimationStock = null;
		for (int num2 = 0; num2 < __Extension_Method_ICollection__.COUNT(deckSlotList); num2++)
		{
			if (Vespa_System_Action_Extension_Method.fire(i_func, num2))
			{
				Vector3 position = deckSlotList[num2].transform.position;
				Vector3 position2 = deckSlotList[num2 + num].transform.position;
				lockAnimationStock = new LockAnimationStock
				{
					Trs = deckSlotList[num2].transform,
					From = position,
					To = position2
				};
				_stockList.Add(lockAnimationStock);
			}
		}
		MatchBanPickSlotComponent matchBanPickSlotComponent = deckSlotList[lockedHeroSlotIndex];
		lockAnimationStock = new LockAnimationStock
		{
			Trs = matchBanPickSlotComponent.transform,
			From = matchBanPickSlotComponent.transform.position,
			To = deckSlotList[toMoveSlotIndex].transform.position
		};
		_stockList.Add(lockAnimationStock);
		GameObject lockAnimObject = GetLockAnimObject();
		lockAnimObject.transform.position = matchBanPickSlotComponent.transform.position;
		lockAnimationStock = new LockAnimationStock
		{
			Trs = lockAnimObject.transform,
			From = matchBanPickSlotComponent.transform.position,
			To = deckSlotList[toMoveSlotIndex].transform.position
		};
		_stockList.Add(lockAnimationStock);
		matchBanPickSlotComponent.SetState(MatchBanPickSlotComponent.SlotState.None);
		_lockSlotList.Add(matchBanPickSlotComponent);
		deckSlotList.RemoveAt(lockedHeroSlotIndex);
		SingletonUpdater<Executer>.instance.DelayRun(LockAnimaionWaitTime, this, delegate
		{
			ExtensionMethod_VespaCoroutine.StartCoroutineThis(this, coAnimationMove());
		});
	}

	private GameObject GetLockAnimObject()
	{
		Animator animator = CreateAnimator(LockAnimTemplate, LockAnimPanel);
		_animGameObjectList.Add(animator.gameObject);
		animator.Play("HonorLeague_LastBan_Chain_1");
		return animator.gameObject;
	}

	private IEnumerator coAnimationMove()
	{
		float timer = 0f;
		while (timer < LockSlotMoveTime)
		{
			timer += Time.deltaTime;
			foreach (LockAnimationStock stock in _stockList)
			{
				stock.Trs.position = Vector3.Lerp(stock.From, stock.To, timer / LockSlotMoveTime);
			}
			yield return null;
		}
		_stockList.Clear();
	}

	public void SetBannedPortrait(Player player, int[] bannedIndices)
	{
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(bannedIndices))
		{
			int num = bannedIndices[0];
			int heroStar = 1;
			CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(num);
			if (data != null && data.StartHeroStar > 0)
			{
				heroStar = data.StartHeroStar;
			}
			CostumeData dataByHeroStar = GameRoot.Instance.CostumeDataContainer.GetDataByHeroStar(num, heroStar);
			if (dataByHeroStar != null)
			{
				SetBannedIcon(new UIAvatarHelper.AvatarInfo(dataByHeroStar).AvatarIndex, player.TextureBan1);
			}
		}
	}

	private void SetBannedIcon(int avatarIndex, UITexture texture)
	{
		string iconPath = UIAvatarHelper.GetIconPath(avatarIndex);
		if (!String_Extension_Method.IsNullOrEmpty(iconPath))
		{
			if (texture.mainTexture == null)
			{
				PlayBanEffect(texture.transform);
			}
			AssetTexture plainTexture = UIHelper.GetPlainTexture(iconPath);
			UIHelper.ResetPlaneAssetTextureAsync(null, texture, plainTexture);
		}
	}

	private void PlayBanEffect(Transform targetTransform)
	{
		Animator animator = CreateAnimator(BanAnimatorPrefab, BanAnimPanel);
		animator.gameObject.transform.position = targetTransform.position;
		_animGameObjectList.Add(animator.gameObject);
		ExtensionMethod_VespaCoroutine.StartCoroutineThis(this, coPlayAnimation(animator, "HonorLeague_BanEff_1", delegate(Animator playEndAnimator)
		{
			VespaObject.DestroyGo(playEndAnimator);
		}));
	}

	private void OnEndPhaseEndAnimation()
	{
		OnOffTransforms();
	}

	private void OnOffTransforms()
	{
		if (TransformArrayActiveOnBan != null)
		{
			Array.ForEach(TransformArrayActiveOnBan, delegate(Transform item)
			{
				UIHelper.EnableGameObject(item, !_currentPhase.IsPickMode());
			});
		}
		if (TransformArrayActiveOnPick == null)
		{
			return;
		}
		Array.ForEach(TransformArrayActiveOnPick, delegate(Transform item)
		{
			bool active = _currentPhase.IsPickMode();
			UIWidget component = item.GetComponent<UIWidget>();
			if (component != null)
			{
				Action<UIWidget, float> updateAction = delegate(UIWidget uiWidget, float time)
				{
					float alpha = uiWidget.alpha;
					float b = (active ? 1f : 0f);
					uiWidget.alpha = Mathf.Lerp(alpha, b, time);
				};
				ExtensionMethod_VespaCoroutine.StartCoroutineThis(this, coUpdateAction(component, updateAction, AlphaTime));
			}
		});
	}

	private IEnumerator coUpdateAction(UIWidget uiWidget, Action<UIWidget, float> updateAction, float timeToUpdate)
	{
		float accumulateTime = 0f;
		while (accumulateTime < timeToUpdate)
		{
			accumulateTime += Time.deltaTime;
			Vespa_System_Action_Extension_Method.fire(updateAction, uiWidget, accumulateTime / timeToUpdate);
			yield return null;
		}
	}

	private void SetHeroListAndBannedPortrait()
	{
		Tuple<MatchedAccountInfo, Dictionary<int, CachedHeroInfo>, BanPickAccountInfo, BanPickAccountInfo> currentTurnHeroInfos = _currentPhase.GetCurrentTurnHeroInfos();
		MatchedAccountInfo item = currentTurnHeroInfos.Item1;
		Dictionary<int, CachedHeroInfo> item2 = currentTurnHeroInfos.Item2;
		BanPickAccountInfo item3 = currentTurnHeroInfos.Item3;
		BanPickAccountInfo item4 = currentTurnHeroInfos.Item4;
		HeroPortraitComponent.OwnerInfo owner = ((item != null) ? new HeroPortraitComponent.OwnerInfo
		{
			AccountId = item.AccountId
		} : null);
		SetHeroList(owner, item2, item3.PickHeroIndices, item3.BanHeroIndices, item4.PickHeroIndices, item4.BanHeroIndices);
		SetBannedPortrait(_me, (item3.AccountId == _me.AccountInfo.AccountId) ? item3.BanHeroIndices : item4.BanHeroIndices);
		SetBannedPortrait(_opponent, (item3.AccountId == _opponent.AccountInfo.AccountId) ? item3.BanHeroIndices : item4.BanHeroIndices);
	}

	private void SetDeckDefaultState()
	{
		foreach (MatchBanPickSlotComponent deckSlot in _me.DeckSlotList)
		{
			deckSlot.SetState(MatchBanPickSlotComponent.SlotState.None);
		}
		foreach (MatchBanPickSlotComponent deckSlot2 in _opponent.DeckSlotList)
		{
			deckSlot2.SetState(MatchBanPickSlotComponent.SlotState.None);
		}
	}

	public void OnEndPhaseBeginAnimationEnd()
	{
		_currentPhase.OnEndAnimation();
		SetNotiObject();
		DescTween.PlayForwardReset();
		SetDescLabel(_currentPhase.GetDescMessage());
		SetHighlightTween();
		UIHelper.EnableGameObject(_me.SlotShadow, _currentPhase.IsHideDeckMine());
		UIHelper.EnableGameObject(_opponent.SlotShadow, _currentPhase.IsHideDeckOpponent());
		SetDeckDefaultState();
		SetDeckSlotState();
	}

	private void SetDeckSlotState()
	{
		int num = _currentPhase.GetDeckSlotIndicateCount();
		bool flag = num == 5;
		foreach (MatchBanPickSlotComponent deckSlot in ((_currentPhase.IsPickMode() && _currentPhase.IsMyturn()) ? _me : _opponent).DeckSlotList)
		{
			if (num == 0)
			{
				break;
			}
			bool flag2 = deckSlot.HeroInfo != null;
			if (!(!flag && flag2))
			{
				MatchBanPickSlotComponent.SlotState state = (_currentPhase.IsPickMode() ? MatchBanPickSlotComponent.SlotState.Pick : MatchBanPickSlotComponent.SlotState.Ban);
				deckSlot.SetState(state);
				num--;
			}
		}
	}

	private void SetNotiObject()
	{
		bool flag = _currentPhase.IsMyturn();
		bool flag2 = _currentPhase.IsPickMode();
		UIHelper.EnableGameObject(_me.PickNoti, flag2 && flag);
		UIHelper.EnableGameObject(_opponent.PickNoti, flag2 && !flag);
		UIHelper.EnableGameObject(_me.BanNoti, !flag2);
		UIHelper.EnableGameObject(_opponent.BanNoti, !flag2);
	}

	private void SetHighlightTween()
	{
		bool flag = _currentPhase.IsMyturn();
		(flag ? _me : _opponent).HighlightTween.PlayForwardReset();
		UIHelper.EnableGameObject(_me.HighlightTween, flag);
		UIHelper.EnableGameObject(_opponent.HighlightTween, !flag);
	}

	private void ClearHeroGrid()
	{
		FactoryBase.ReleaseAndClearList(_heroPortraitDic);
		if (HeroGrid != null)
		{
			HeroGrid.ClearGridItem<HeroPortraitComponent>();
		}
	}

	public void MakeHeroGrid(Dictionary<int, CachedHeroInfo> heroInfoDictionary)
	{
		ClearHeroGrid();
		if (heroInfoDictionary == null)
		{
			return;
		}
		if (_currentPhase.IsMyturn())
		{
			List<CachedHeroInfo> list = new List<CachedHeroInfo>();
			List<CachedHeroInfo> prevBanPickHeroInfoList = GetPrevBanPickHeroInfoList(heroInfoDictionary, _currentPhase.IsPickMode());
			list.AddRange(prevBanPickHeroInfoList);
			List<CachedHeroInfo> list2 = new List<CachedHeroInfo>(heroInfoDictionary.Values.ToList().Except(prevBanPickHeroInfoList));
			if (_currentPhase.IsPickMode())
			{
				list2.Sort(new HeroComparer());
			}
			list.AddRange(list2);
			foreach (CachedHeroInfo item in list)
			{
				AddHeroSmallPortraitToGrid(item);
			}
		}
		else
		{
			foreach (KeyValuePair<int, CachedHeroInfo> item2 in heroInfoDictionary)
			{
				AddHeroSmallPortraitToGrid(item2.Value);
			}
		}
		InitToggle();
	}

	private void AddHeroSmallPortraitToGrid(CachedHeroInfo info)
	{
		if (info != null)
		{
			HeroPortraitComponent heroPortraitComponent = UIHelper.AddGridItem(HeroGrid, HeroPortraitSelector.PrefabAuto);
			heroPortraitComponent.Open(info, this, null, HeroPortraitComponent.EPortraitMode.Icon);
			heroPortraitComponent.SetDisable(disable: true);
			if (!_heroPortraitDic.ContainsKey(info.HeroIndex))
			{
				_heroPortraitDic.Add(info.HeroIndex, heroPortraitComponent);
			}
		}
	}

	private List<CachedHeroInfo> GetPrevBanPickHeroInfoList(Dictionary<int, CachedHeroInfo> heroInfos, bool isPick)
	{
		List<CachedHeroInfo> list = new List<CachedHeroInfo>();
		if (!(_dataProvider is BanPickSettingDataProdiver banPickSettingDataProdiver))
		{
			VespaDebug.LogError("BanPickSettingDataProdiver == null");
			return list;
		}
		List<int> list2 = (isPick ? banPickSettingDataProdiver.GetPickSaveList() : banPickSettingDataProdiver.GetBanSaveList());
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(list2); i++)
		{
			int num = list2[i];
			CachedHeroInfo value = null;
			if (num != 0 && heroInfos.TryGetValue(num, out value))
			{
				list.Add(value);
			}
		}
		return list;
	}

	private CachedHeroInfo GetHeroInfo(int heroIndex, bool useMytHeroInfo)
	{
		CachedHeroInfo cachedHeroInfo = null;
		if (useMytHeroInfo)
		{
			cachedHeroInfo = Array.Find(GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfos(), (CachedHeroInfo e) => e.HeroIndex == heroIndex);
			if (cachedHeroInfo != null)
			{
				return cachedHeroInfo;
			}
		}
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(heroIndex);
		if (data == null)
		{
			VespaDebug.LogError("Not exist creatrueData of heroIndex : " + heroIndex);
			return null;
		}
		return new CachedHeroInfo(data);
	}

	private void SetDefault(HeroPortraitComponent heroPortrait)
	{
		heroPortrait.SetDisable(disable: true);
		heroPortrait.SetPortraitState(EHeroState.Free);
		heroPortrait.SetBan(ban: false);
		heroPortrait.SetLevel(show: false);
		heroPortrait.ShowStar(show: false);
		heroPortrait.HeroInfo = null;
		heroPortrait.Owner = null;
		heroPortrait.SetHeroImage();
	}

	public void SetHeroList(HeroPortraitComponent.OwnerInfo owner, Dictionary<int, CachedHeroInfo> cachedHeroDictionary, int[] pickedIndices, int[] bannedIndices, int[] opponentPickedIndices, int[] opponentBannedIndices)
	{
		if (cachedHeroDictionary == null)
		{
			MakeHeroGrid(null);
			return;
		}
		MakeHeroGrid(cachedHeroDictionary);
		bool flag = owner.AccountId == _opponent.AccountInfo.AccountId;
		Dictionary<int, bool> disableHeroIndices = new Dictionary<int, bool>();
		Array.ForEach(bannedIndices, delegate(int item)
		{
			if (!disableHeroIndices.ContainsKey(item))
			{
				disableHeroIndices.Add(item, value: true);
			}
		});
		Array.ForEach(opponentPickedIndices, delegate(int item)
		{
			if (!disableHeroIndices.ContainsKey(item))
			{
				disableHeroIndices.Add(item, value: false);
			}
		});
		Array.ForEach(opponentBannedIndices, delegate(int item)
		{
			if (!disableHeroIndices.ContainsKey(item))
			{
				disableHeroIndices.Add(item, value: true);
			}
		});
		foreach (KeyValuePair<int, CachedHeroInfo> item in cachedHeroDictionary)
		{
			CachedHeroInfo value = item.Value;
			if (value == null)
			{
				VespaDebug.LogError("SetupHeroInfos :: cachedHeroInfo == null");
				continue;
			}
			HeroPortraitComponent value2 = null;
			if (!_heroPortraitDic.TryGetValue(value.HeroIndex, out value2))
			{
				VespaDebug.LogError("SetupHeroInfos :: _allHeroDic.TryGetValue == false");
			}
			SetDefault(value2);
			bool flag2 = true;
			if (TypeIterator.Contains(pickedIndices, value.HeroIndex))
			{
				if (flag)
				{
					value2.SetDisable(disable: true);
				}
				else
				{
					value2.SetPortraitState(EHeroState.Party);
					value2.SetDisable(disable: false);
				}
				flag2 = false;
			}
			if (disableHeroIndices.ContainsKey(value.HeroIndex))
			{
				bool flag3 = disableHeroIndices[value.HeroIndex];
				value2.SetBan(flag3);
				value2.SetDisable(!flag3);
				flag2 = false;
			}
			if (flag2)
			{
				value2.Open(value, this, owner, HeroPortraitComponent.EPortraitMode.Icon);
				value2.SetDisable(disable: false);
			}
			if (flag)
			{
				value2.SetLevel(show: false);
				value2.ShowStar(show: false);
				value2.HeroInfo = null;
				value2.SetHeroImage();
			}
		}
		UIHelper.RepositionGrid(HeroGrid);
	}

	public void OnClickClose()
	{
		if (_listener != null && !GiveUpPending)
		{
			_listener.OnMatchBanPickClose();
		}
	}

	public override void OnSystemBackButton()
	{
		OnClickClose();
	}

	public void InitToggle()
	{
		ToggleAll.Set(state: true);
		foreach (KeyValuePair<int, HeroPortraitComponent> item in _heroPortraitDic)
		{
			UIHelper.EnableGameObject(item.Value, enable: true);
		}
		UIHelper.RepositionGrid(HeroGrid);
	}

	public void OnToggleChange()
	{
		CreatureTagType creatureTagType = CreatureTagType.None;
		if (ToggleKnight.value)
		{
			creatureTagType = CreatureTagType.Knight;
		}
		if (ToggleWarrior.value)
		{
			creatureTagType = CreatureTagType.Warrior;
		}
		if (ToggleAssassin.value)
		{
			creatureTagType = CreatureTagType.Assassin;
		}
		if (ToggleArcher.value)
		{
			creatureTagType = CreatureTagType.Archer;
		}
		if (ToggleMechanic.value)
		{
			creatureTagType = CreatureTagType.Mechanic;
		}
		if (ToggleWizard.value)
		{
			creatureTagType = CreatureTagType.Wizard;
		}
		if (TogglePriest.value)
		{
			creatureTagType = CreatureTagType.Priest;
		}
		if (creatureTagType == _currentClassTab)
		{
			return;
		}
		_currentClassTab = creatureTagType;
		foreach (KeyValuePair<int, HeroPortraitComponent> item in _heroPortraitDic)
		{
			HeroPortraitComponent value = item.Value;
			bool enable = _currentClassTab == CreatureTagType.None || value.CreatureData.TagType == _currentClassTab;
			UIHelper.EnableGameObject(value, enable);
		}
		UIHelper.RepositionGrid(HeroGrid);
	}

	public void CancelBanPick()
	{
		if (_currentPhase.IsPickMode())
		{
			for (int num = _pickList.Count - 1; num >= 0; num--)
			{
				int i_key = _pickList[num];
				HeroPortraitComponent vALUE = Vespa_Generic_IDictionary_Extension_Method.GETVALUE(_heroPortraitDic, i_key, null);
				RemovePick(vALUE);
			}
		}
		else
		{
			for (int num2 = _banList.Count - 1; num2 >= 0; num2--)
			{
				int i_key2 = _banList[num2];
				HeroPortraitComponent vALUE2 = Vespa_Generic_IDictionary_Extension_Method.GETVALUE(_heroPortraitDic, i_key2, null);
				RemoveBan(vALUE2);
			}
		}
		_isLock = false;
		UIHelper.EnableButton(ConfirmButton, enable: false);
	}

	public void InsertBan(HeroPortraitComponent portrait)
	{
		portrait.SetBan(ban: true);
		_banList.Add(portrait.CreatureData.Index);
		UIHelper.EnableButton(ConfirmButton, _banList.Count == _currentPhase.GetSelectableCount());
	}

	public void RemoveBan(HeroPortraitComponent portrait)
	{
		if (_banList.FindIndex((int e) => portrait.CreatureData.Index == e) != -1)
		{
			portrait.SetBan(ban: false);
			_banList.Remove(portrait.CreatureData.Index);
			UIHelper.EnableButton(ConfirmButton, _banList.Count == _currentPhase.GetSelectableCount());
		}
	}

	public void InsertPick(HeroPortraitComponent heroPortrait)
	{
		if (heroPortrait == null)
		{
			VespaDebug.LogError("RemovePick :: heroPortrait == null");
			return;
		}
		heroPortrait.SetPortraitState(EHeroState.Party);
		_pickList.Add(heroPortrait.HeroInfo.HeroIndex);
		UIHelper.EnableButton(ConfirmButton, _pickList.Count == _currentPhase.GetSelectableCount());
	}

	public void RemovePick(HeroPortraitComponent heroPortrait)
	{
		if (heroPortrait == null)
		{
			VespaDebug.LogError("RemovePick :: heroPortrait == null");
			return;
		}
		heroPortrait.SetPortraitState(EHeroState.Free);
		_pickList.Remove(heroPortrait.HeroInfo.HeroIndex);
		UIHelper.EnableButton(ConfirmButton, _pickList.Count == _currentPhase.GetSelectableCount());
	}

	public void OnClickConfirm()
	{
		if (_listener != null)
		{
			List<int> list = (_currentPhase.IsPickMode() ? _pickList : _banList);
			if (list.Count >= _currentPhase.GetSelectableCount())
			{
				_isLock = true;
				_listener.OnSelectConfirm(_currentPhase.MatchBanPickState, list.ToArray());
			}
		}
	}

	public void OnResponseConfirm(BanPickAccountInfo[] banPickAccountInfos)
	{
		if (Array.Find(banPickAccountInfos, (BanPickAccountInfo e) => e.AccountId == _me.AccountInfo.AccountId) == null)
		{
			VespaDebug.LogError("OnResponseConfirm :: banPickInfo == null");
			return;
		}
		_isLock = true;
		UIHelper.EnableButton(ConfirmButton, enable: false);
	}

	public void OnTimeoutState(long ownerAccountId)
	{
		if (_me.AccountInfo.AccountId == ownerAccountId)
		{
			_currentPhase.RandomBanPick();
		}
	}

	void HeroPortraitComponent.Listener.OnClick(HeroPortraitComponent heroPortrait)
	{
		if (_isLock || !_currentPhase.IsMyturn())
		{
			return;
		}
		Player player = (_currentPhase.IsPickMode() ? _me : _opponent);
		if (heroPortrait.OwnerId == player.AccountInfo.AccountId)
		{
			if (_currentPhase.IsPickMode())
			{
				Pick(heroPortrait);
			}
			else
			{
				Ban(heroPortrait);
			}
			PreviewSelectedPick();
		}
	}

	private void PreviewSelectedPick()
	{
		if (!_currentPhase.IsMyturn() || !_currentPhase.IsPickMode())
		{
			return;
		}
		List<MatchBanPickSlotComponent> list = _me.DeckSlotList.FindAll((MatchBanPickSlotComponent slot) => slot.CurrentSlotState == MatchBanPickSlotComponent.SlotState.Pick);
		if (__Extension_Method_ICollection__.COUNT(list) == 0)
		{
			return;
		}
		list.ForEach(delegate(MatchBanPickSlotComponent slot)
		{
			slot.InitEmpty(slot.CurrentSlotState);
		});
		long accountId = _me.AccountInfo.AccountId;
		for (int num = 0; num < __Extension_Method_ICollection__.COUNT(_pickList) && num < __Extension_Method_ICollection__.COUNT(list); num++)
		{
			MatchBanPickSlotComponent matchBanPickSlotComponent = list[num];
			if (!(matchBanPickSlotComponent == null))
			{
				CachedHeroInfo heroInfo = GetHeroInfo(_pickList[num], useMytHeroInfo: true);
				matchBanPickSlotComponent.Init(this, heroInfo, hideInfo: false, accountId, matchBanPickSlotComponent.CurrentSlotState);
			}
		}
	}

	private void Pick(HeroPortraitComponent heroPortrait)
	{
		if (heroPortrait.IsBanned || heroPortrait.IsDisabled)
		{
			return;
		}
		if (heroPortrait.IsSelected)
		{
			RemovePick(heroPortrait);
		}
		else if (_pickList.Count >= _currentPhase.GetSelectableCount())
		{
			int key = _pickList.First();
			if (_heroPortraitDic.ContainsKey(key))
			{
				HeroPortraitComponent heroPortrait2 = _heroPortraitDic[key];
				RemovePick(heroPortrait2);
				InsertPick(heroPortrait);
			}
		}
		else
		{
			InsertPick(heroPortrait);
		}
	}

	private void Ban(HeroPortraitComponent heroPortrait)
	{
		if (heroPortrait.IsSelected || heroPortrait.IsDisabled)
		{
			return;
		}
		if (heroPortrait.IsBanned)
		{
			RemoveBan(heroPortrait);
		}
		else if (_banList.Count >= _currentPhase.GetSelectableCount())
		{
			int key = _banList.First();
			if (_heroPortraitDic.ContainsKey(key))
			{
				HeroPortraitComponent portrait = _heroPortraitDic[key];
				RemoveBan(portrait);
				InsertBan(heroPortrait);
			}
		}
		else
		{
			InsertBan(heroPortrait);
		}
	}

	void HeroPortraitComponent.Listener.OnViewInfo(HeroPortraitComponent heroPortrait, bool isMine)
	{
	}

	void HeroPortraitComponent.Listener.OnClickHeroNotify(HeroNotifyComponent.ENotifyInfo notifyInfo, CachedHeroInfo heroInfo)
	{
	}

	void HeroPortraitComponent.Listener.OnLongClick(HeroPortraitComponent heroPortrait, CachedHeroInfo heroInfo)
	{
		if (!_isLock && _currentPhase.IsMyturn())
		{
			Player player = (_currentPhase.IsPickMode() ? _me : _opponent);
			if (heroPortrait.OwnerId == player.AccountInfo.AccountId)
			{
				OpenInspectHero(heroInfo, heroPortrait.CreatureData);
			}
		}
	}

	private void OpenInspectHero(CachedHeroInfo cachedHeroInfo, CreatureData creatureData)
	{
		if (cachedHeroInfo == null)
		{
			cachedHeroInfo = new CachedHeroInfo();
			cachedHeroInfo.HeroIndex = creatureData.Index;
			cachedHeroInfo.Star = creatureData.StartHeroStar;
			cachedHeroInfo.Level = (short)creatureData.StartHeroLevel;
		}
		else
		{
			CachedHeroInfo[] heroInfos = GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfos();
			cachedHeroInfo = Array.Find(heroInfos, (CachedHeroInfo e) => e.HeroIndex == cachedHeroInfo.HeroIndex);
		}
		bool flag = _currentPhase.IsPickMode();
		string confirmText = LocalizationManager.GetString(flag ? NShared.NLocalization.Common.SELECT : NShared.NLocalization.Common.BAN);
		InspectHeroInfo.InspectOption option = new InspectHeroInfo.InspectOption
		{
			ShowEquipSlot = flag,
			ShowHeroStar = flag,
			ShowHeroLevel = flag,
			ShowPresetButton = flag,
			ConfirmText = confirmText
		};
		InspectHeroInfo.Open(cachedHeroInfo, option, null);
	}

	void MatchBanPickSlotComponent.Listener.OnLongClick(CachedHeroInfo heroInfo, long accountId)
	{
		OpenInspectHero(heroInfo, accountId);
	}

	public void OpenInspectHero(CachedHeroInfo heroInfo, long accontId)
	{
		if (_me != null && _me.AccountInfo != null && _me.AccountInfo.AccountId == accontId)
		{
			InspectHeroInfo.InspectOption option = new InspectHeroInfo.InspectOption
			{
				ShowEquipSlot = true,
				ShowHeroStar = true,
				ShowHeroLevel = true,
				ShowPresetButton = true
			};
			InspectHeroInfo.Open(heroInfo, option, null);
		}
	}

	void MatchBanPickSlotComponent.Listener.OnClick(CachedHeroInfo heroInfo, long accountId)
	{
		if (!_isLock)
		{
			if (_listener == null)
			{
				Debug.LogError("_listener == null");
			}
			else if (heroInfo != null)
			{
				OnClickBanPickSlotComponent(heroInfo.HeroIndex);
			}
		}
	}

	private void OnClickBanPickSlotComponent(int selectedHeroIndex)
	{
		if (_currentPhase.IsMyturn() && _currentPhase.GetSelectableCount() != 0)
		{
			if (_currentPhase.IsPickMode())
			{
				OnClickSlotOnPickMode(selectedHeroIndex);
			}
			else
			{
				OnClickSlotOnBanMode(selectedHeroIndex);
			}
		}
	}

	private void OnClickSlotOnPickMode(int selectedHeroIndex)
	{
		HeroPortraitComponent value = null;
		if (!_heroPortraitDic.TryGetValue(selectedHeroIndex, out value))
		{
			VespaDebug.LogError("not found selectedHeroIndex in allHeroDic ");
		}
		else if ((object)this == null)
		{
			VespaDebug.LogError("failed to casting Matchbanpick to HeroPortraitComponent.Listener");
		}
		else
		{
			((HeroPortraitComponent.Listener)this).OnClick(value);
		}
	}

	private void OnClickSlotOnBanMode(int selectedHeroIndex)
	{
		MatchBanPickSlotComponent matchBanPickSlotComponent = _opponent.DeckSlotList.Find((MatchBanPickSlotComponent component) => component.HeroInfo != null && component.HeroInfo.HeroIndex == selectedHeroIndex);
		if (matchBanPickSlotComponent == null)
		{
			Debug.Log("opponent.DeckSlotList.Contains(" + selectedHeroIndex + ") == false");
		}
		else
		{
			if (_banList.Contains(selectedHeroIndex))
			{
				return;
			}
			foreach (MatchBanPickSlotComponent deckSlot in _opponent.DeckSlotList)
			{
				if (deckSlot.HeroInfo != null)
				{
					deckSlot.SetState(MatchBanPickSlotComponent.SlotState.None);
				}
			}
			matchBanPickSlotComponent.SetState(MatchBanPickSlotComponent.SlotState.Lock);
			_banList.Add(selectedHeroIndex);
			if (__Extension_Method_ICollection__.COUNT(_banList) > _currentPhase.GetSelectableCount())
			{
				MatchBanPickSlotComponent matchBanPickSlotComponent2 = _opponent.DeckSlotList.Find((MatchBanPickSlotComponent component) => component.HeroInfo != null && component.HeroInfo.HeroIndex == _banList[0]);
				if (matchBanPickSlotComponent2 != null)
				{
					matchBanPickSlotComponent2.SetState(MatchBanPickSlotComponent.SlotState.None);
					_banList.RemoveAt(0);
				}
			}
			UIHelper.EnableButton(ConfirmButton, _banList.Count == _currentPhase.GetSelectableCount());
		}
	}

	private void SetStateLog(MatchBanPickState state, BanPickAccountInfo[] banPickAccountInfos)
	{
		if (banPickAccountInfos == null)
		{
			return;
		}
		foreach (BanPickAccountInfo banPickAccountInfo in banPickAccountInfos)
		{
			string text = banPickAccountInfo.AccountId + "/Picked : ";
			int[] pickHeroIndices = banPickAccountInfo.PickHeroIndices;
			foreach (int key in pickHeroIndices)
			{
				CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(key);
				if (data != null)
				{
					text = text + data.Name + "/";
				}
			}
			text += "Banned : ";
			pickHeroIndices = banPickAccountInfo.BanHeroIndices;
			foreach (int key2 in pickHeroIndices)
			{
				CreatureData data2 = GameRoot.Instance.CreatureDataContainer.GetData(key2);
				if (data2 != null)
				{
					text = text + data2.Name + "/";
				}
			}
			VespaDebug.LogError(text);
		}
		VespaDebug.LogError("===========================" + state.ToString() + "=========================");
	}

	Dictionary<int, HeroPortraitComponent> PhaseBase.IListener.GetAllHeroOnGrid()
	{
		return _heroPortraitDic;
	}

	bool PhaseBase.IListener.CanSelect()
	{
		return !_isLock;
	}

	void PhaseBase.IListener.SetTimer(float remainTime, float gaugeRatio, bool isImminent)
	{
		_currentTimeGauge = (isImminent ? TimeGaugeImminent : TimeGaugeNormal);
		UIHelper.EnableGameObject(TimeGaugeImminent, isImminent);
		UIHelper.EnableGameObject(TimeGaugeNormal, !isImminent);
		UIHelper.SetSpriteFillAmount(_currentTimeGauge, gaugeRatio);
		UIHelper.SetLabelText(RemainTime, $"00:{(int)remainTime:D2}");
	}

	void PhaseBase.IListener.RandomPick(MatchBanPickState matchBanPickState, int[] randomHeroIndices, bool isPickMode)
	{
		List<int> list = (isPickMode ? _pickList : _banList);
		int num = __Extension_Method_ICollection__.COUNT(randomHeroIndices) - __Extension_Method_ICollection__.COUNT(list);
		list.Clear();
		list.AddRange(randomHeroIndices);
		bool isPenalty = num > 0;
		BanPickSettingDataProdiver.PrintListWtihHeroName("Random Pick : ", _pickList);
		BanPickSettingDataProdiver.PrintListWtihHeroName("Random Ban  : ", _banList);
		SceneAutoBehaviour<MatchManagement>.instance.OnSelectConfirm(matchBanPickState, randomHeroIndices, isPenalty);
	}

	void PhaseBase.IListener.StartBattle(MatchedAccountInfo[] accountInfos)
	{
		if (_dataProvider is BanPickSettingDataProdiver banPickSettingDataProdiver)
		{
			banPickSettingDataProdiver.SaveBanPickHeroIndex(GetToSavePickList(), GetToSaveBanList());
		}
		SingletonUpdater<Executer>.instance.DelayRun(StartBattleDelay, this, delegate
		{
			SingletonUpdater<MatchManager>.instance.EnterBanPickArena(accountInfos);
		});
	}

	private List<int> GetToSavePickList()
	{
		List<int> list = new List<int>();
		long accountId = _me.AccountInfo.AccountId;
		int[] pickHeroIndices = _currentPhase.GetPickHeroIndices(accountId);
		if (__Extension_Method_ICollection__.COUNT(pickHeroIndices) != 0)
		{
			list.AddRange(pickHeroIndices);
		}
		int[] pickHeroIndices2 = GetState(MatchBanPickState.PHASE8_ALL_BAN_EACH_PICK).GetPickHeroIndices(accountId);
		if (__Extension_Method_ICollection__.COUNT(pickHeroIndices2) != 0)
		{
			list.AddRange(pickHeroIndices2);
		}
		list = list.Distinct().ToList();
		BanPickSettingDataProdiver.PrintListWtihHeroName("toSaveList ", list);
		return list;
	}

	private List<int> GetToSaveBanList()
	{
		List<int> list = new List<int>();
		long accountId = _opponent.AccountInfo.AccountId;
		int[] banHeroIndices = _currentPhase.GetBanHeroIndices(accountId);
		if (__Extension_Method_ICollection__.COUNT(banHeroIndices) != 0)
		{
			list.AddRange(banHeroIndices);
		}
		int[] pickHeroIndices = _currentPhase.GetPickHeroIndices(accountId);
		int[] pickHeroIndices2 = GetState(MatchBanPickState.PHASE8_ALL_BAN_EACH_PICK).GetPickHeroIndices(accountId);
		if (__Extension_Method_ICollection__.COUNT(pickHeroIndices) != 0 && __Extension_Method_ICollection__.COUNT(pickHeroIndices) < __Extension_Method_ICollection__.COUNT(pickHeroIndices2))
		{
			IEnumerable<int> enumerable = pickHeroIndices2.Except(pickHeroIndices);
			if (enumerable != null)
			{
				list.AddRange(enumerable);
			}
		}
		list = list.Distinct().ToList();
		BanPickSettingDataProdiver.PrintListWtihHeroName("toBanList ", list);
		return list;
	}

	void PhaseBase.IListener.CloseInspectHero()
	{
		CloseInspectHero();
	}

	List<int> PhaseBase.IListener.GetCurrentSelectedHeroList()
	{
		if (_currentPhase.IsPickMode())
		{
			return _pickList;
		}
		return _banList;
	}
}
