using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow.NEventStepReward;
using NGameAsset;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using NVespa.NTask.NRuntime;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class EventStepRewardView : HeroRecruitView, EventStepRewardComponent.Listener, ItemIconComponent.Listener, IUserObserver<NPCFriendlyInfo>, IUserObserver, IUserObserver<ItemInfo>
{
	public interface Listener
	{
		void OnClickConfrimPresent(int heroIndex, int itemIndex, int itemCount, Action action = null);

		void OnClickGetReward(int heroIndex, int step, NpcFriendlyType type, Action action = null);
	}

	private enum EGaugeType
	{
		Current,
		Next
	}

	public enum EGaugeAnimation
	{
		Idle,
		Running
	}

	public class ViewDatas
	{
		private NPCData _npcData;

		public NPCData NpcData => _npcData;

		public HeroInfo HeroInfo { get; private set; }

		public NPCFriendlyInfo NpcFriendlyInfo { get; private set; }

		public List<NPCFriendlyPointData> PointDataList { get; private set; }

		public int CurrStep { get; private set; }

		public int CurrRewardStep { get; private set; }

		public int MaxStep { get; private set; }

		public int CurrPoint { get; private set; }

		public int ReqPoint { get; private set; }

		public int EndRewardCount { get; private set; }

		public void SetNpcData(NPCData npcData, int npcIndex)
		{
			_npcData = npcData;
			CreatureData creatureData = ((npcIndex == 0) ? GameRoot.Instance.CreatureDataContainer.GetData(npcData.CreatureIndex) : GameRoot.Instance.CreatureDataContainer.GetData(npcIndex));
			if (creatureData != null)
			{
				HeroInfo = SceneAutoBehaviour<StatDataCache>.instance.GetReferenceHero(creatureData.Index, 1);
				PointDataList = GameRoot.Instance.NPCFriendlyPointDataContainer.GetPointDataList(HeroInfo.HeroIndex);
				MaxStep = PointDataList.Max((NPCFriendlyPointData x) => x.Step);
				UpdateNpcFriendlyInfo(GlobalAutoBehaviour<AccountManager>.instance.NPCFriendlyManager.Get(npcData.HeroIndex));
			}
		}

		public void UpdateNpcFriendlyInfo(NPCFriendlyInfo data)
		{
			NpcFriendlyInfo = data;
			if (NpcFriendlyInfo == null)
			{
				NpcFriendlyInfo = new NPCFriendlyInfo
				{
					FriendlyPoint = 0,
					Step = 1,
					HeroIndex = _npcData.HeroIndex
				};
			}
			CurrStep = 0;
			CurrPoint = NpcFriendlyInfo.FriendlyPoint;
			int num = PointDataList.Sum((NPCFriendlyPointData x) => x.ReqFriendlyPoint);
			num -= PointDataList.Last().ReqFriendlyPoint;
			if (CurrPoint > num)
			{
				CurrPoint -= num;
				CurrStep = __Extension_Method_ICollection__.COUNT(PointDataList) - 1;
				int reqFriendlyPoint = PointDataList.Last().ReqFriendlyPoint;
				while (CurrPoint >= reqFriendlyPoint)
				{
					CurrStep++;
					CurrPoint -= reqFriendlyPoint;
				}
				ReqPoint = reqFriendlyPoint;
			}
			else
			{
				for (int num2 = 0; num2 < __Extension_Method_ICollection__.COUNT(PointDataList); num2++)
				{
					int reqFriendlyPoint2 = PointDataList[num2].ReqFriendlyPoint;
					if (CurrPoint < reqFriendlyPoint2)
					{
						break;
					}
					CurrStep++;
					CurrPoint -= reqFriendlyPoint2;
				}
				ReqPoint = PointDataList[CurrStep].ReqFriendlyPoint;
			}
			CurrRewardStep = NpcFriendlyInfo.Step;
			EndRewardCount = NpcFriendlyInfo.EndRewardCount;
		}

		public float GetCurrGaugeValue()
		{
			return (float)CurrPoint / (float)ReqPoint;
		}

		public float GetNextGaugeValue(NPCFriendshipItemComponent present, int amount)
		{
			if (present == null)
			{
				return 0f;
			}
			int friendshipPoint = present.FriendshipPoint;
			return (float)(amount * friendshipPoint + CurrPoint) / (float)ReqPoint;
		}
	}

	public const string PrefabPath = "Resources/Prefab2/Window/EventStepReward/EventStepRewardView";

	[Space(10f)]
	[Header("[NPC Group]")]
	public Transform HeroRoot;

	public TweenAlpha NpcSpeakBubble;

	public UILabel NpcSpeakLabel;

	[Header("[Reward UI Group]")]
	public UIWidget ProgressArea;

	public Transform NodeRoot;

	[Header("[Present UI Group]")]
	public UILabel PresentStepLabel;

	public UISlider CurrGaugeSlider;

	public UISlider NextGaugeSlider;

	public UILabel CurrPointLabel;

	public UILabel AddPointLabel;

	public ParticleSystem PointMaxParticle;

	public UILabel NotEnoughPresentLabel;

	public UISimpleGrid PresentGrid;

	public UIInput PresentAmountInput;

	public UIButton IncreaseButton;

	public UIButton DecreaseButton;

	public UIButton MaxButton;

	public UIButton PresentButton;

	public ViewDatas _vData = new ViewDatas();

	private Listener _listener;

	private int _currPresentAmount;

	private List<EventStepRewardComponent> _nodeCompList = new List<EventStepRewardComponent>();

	private List<NPCFriendshipItemComponent> _ItemCompList = new List<NPCFriendshipItemComponent>();

	private List<EventStepRewardComponent> _stampAniList = new List<EventStepRewardComponent>();

	private NPCFriendshipItemComponent _currItemComp;

	public EGaugeAnimation _gaugeAnimation { get; private set; }

	public void Open(NPCData npcData, int colLimit, Listener listener, string componentCreateValue, int shopNpcIndex)
	{
		FactoryBase.ReleaseAndClearList(_nodeCompList);
		FactoryBase.ReleaseAndClearList(_ItemCompList);
		_vData.SetNpcData(npcData, shopNpcIndex);
		_listener = listener;
		_currPresentAmount = 0;
		_currItemComp = null;
		_gaugeAnimation = EGaugeAnimation.Idle;
		Vespa_Generic_ICollection_Extension_Method.CLEAR(_stampAniList);
		InitAction = (Action)Delegate.Combine(InitAction, new Action(WelcomeNpcReaction));
		NodeGenerator nodeGenerator = new NodeGenerator(ProgressArea.width, ProgressArea.height, colLimit);
		nodeGenerator.SetComponentCreateFunc((Transform nodeRoot) => EventStepRewardComponent.CreateComponent(nodeRoot, componentCreateValue));
		_nodeCompList = nodeGenerator.Generate(this, _vData, NodeRoot);
		InitPresentList();
		ChangeSelectGaugeValue(EGaugeType.Current);
		ChangeSelectGaugeValue(EGaugeType.Next);
		CheckNodeStampState();
		UIHelper.EnableGameObject(NpcSpeakBubble, enable: false);
		UIHelper.EnableGameObject(PointMaxParticle, enable: false);
		GlobalAutoBehaviour<AccountManager>.instance.NPCFriendlyManager.ObserverManager.AddObserver(this);
		GlobalAutoBehaviour<AccountManager>.instance.ItemManager.ObserverManager.AddObserver(this);
		EntryMenu instance = SceneAutoBehaviour<EntryMenuManagement>.instance.GetInstance<EntryMenu>(openedOnly: true);
		if (instance != null && instance.BubbleTweenAlpha != null)
		{
			UIHelper.EnableGameObject(instance.BubbleTweenAlpha, enable: false);
		}
		if (npcData.GetStepRewardViewHero(out var _, out var _, out var _, out var _))
		{
			new DelegateTask(OpenAsync(_vData.HeroInfo, isSelectHero: true, npcData.Code)).Commit();
		}
		else
		{
			new DelegateTask(OpenAsync(_vData.HeroInfo, isSelectHero: true)).Commit();
		}
	}

	public override void Close()
	{
		FactoryBase.ReleaseAndClearList(_nodeCompList);
		FactoryBase.ReleaseAndClearList(_ItemCompList);
		GlobalAutoBehaviour<AccountManager>.instance.NPCFriendlyManager.ObserverManager.RemoveObserver(this);
		GlobalAutoBehaviour<AccountManager>.instance.ItemManager.ObserverManager.RemoveObserver(this);
		base.Close();
	}

	public void WelcomeNpcReaction()
	{
		NPCFriendlyActionData data = GameRoot.Instance.NPCFriendlyActionDataContainer.GetData(_vData.NpcData.HeroIndex, 0);
		if (data != null)
		{
			string[] eventRandom = data.GetEventRandom();
			ResponseAction(eventRandom[2], eventRandom[1], bBlendAction: false);
			UIHelper.EnableGameObject(NpcSpeakBubble, enable: true);
			string stringDirect = LocalizationManager.GetStringDirect(eventRandom[0]);
			UIHelper.SetLabelText(NpcSpeakLabel, stringDirect);
			NpcSpeakBubble.ResetToBeginning();
			NpcSpeakBubble.PlayForward();
		}
	}

	public void ResetControl()
	{
		_currPresentAmount = 0;
		PresentAmountInput.value = _currPresentAmount.ToString();
		if (_vData.CurrStep >= _vData.MaxStep - 1)
		{
			EventStepRewardComponent eventStepRewardComponent = _nodeCompList.Last();
			if (eventStepRewardComponent.RewardType != EventStepRewardComponent.ERewardType.Repeatable && eventStepRewardComponent.RewardType != EventStepRewardComponent.ERewardType.LimitedRepeat)
			{
				NPCFriendlyPointData nPCFriendlyPointData = _vData.PointDataList.Last();
				if (nPCFriendlyPointData != null && nPCFriendlyPointData.EndRewardRepeatCount != 0)
				{
					if (_vData.CurrRewardStep >= _vData.MaxStep - 1)
					{
						eventStepRewardComponent.RewardType = EventStepRewardComponent.ERewardType.LimitedRepeat;
						eventStepRewardComponent.UpdateData(_vData.PointDataList.Last());
					}
				}
				else
				{
					eventStepRewardComponent.RewardType = EventStepRewardComponent.ERewardType.Repeatable;
					eventStepRewardComponent.UpdateData(_vData.PointDataList.Last());
				}
			}
		}
		CheckNodeRewardState();
		ChangeSelectGaugeValue(EGaugeType.Next);
	}

	public void ResetPresentItemList()
	{
		for (int num = __Extension_Method_ICollection__.COUNT(_ItemCompList) - 1; num >= 0; num--)
		{
			NPCFriendshipItemComponent nPCFriendshipItemComponent = _ItemCompList[num];
			if (!(nPCFriendshipItemComponent == null))
			{
				ItemInfo itemInfo = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Get(nPCFriendshipItemComponent.ItemIndex);
				if (itemInfo == null || itemInfo.Count <= 0)
				{
					FactoryBase.ReleaseComponent(nPCFriendshipItemComponent);
					_ItemCompList.Remove(nPCFriendshipItemComponent);
				}
				nPCFriendshipItemComponent.UpdateCount(itemInfo.Count);
			}
		}
		if (__Extension_Method_ICollection__.COUNT(_ItemCompList) > 0)
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_ItemCompList); i++)
			{
				NPCFriendshipItemComponent nPCFriendshipItemComponent2 = _ItemCompList[i];
				if (i == 0)
				{
					nPCFriendshipItemComponent2.Select(isSelect: true);
					_currItemComp = nPCFriendshipItemComponent2;
				}
				else
				{
					nPCFriendshipItemComponent2.Select(isSelect: false);
				}
			}
		}
		else
		{
			_currItemComp = null;
		}
		UIHelper.EnableCollider(PresentAmountInput, !__Extension_Method_ICollection__.IsNullOrEmpty(_ItemCompList));
		UIHelper.EnableGameObject(NotEnoughPresentLabel, __Extension_Method_ICollection__.IsNullOrEmpty(_ItemCompList));
	}

	private void InitPresentList()
	{
		IList<NPCFriendlyPointItemData> itemListByHeroIndex = GameRoot.Instance.NPCFriendlyPointItemDataContainer.GetItemListByHeroIndex(_vData.NpcData.HeroIndex);
		if (__Extension_Method_ICollection__.IsNullOrEmpty(itemListByHeroIndex))
		{
			_currItemComp = null;
			return;
		}
		foreach (NPCFriendlyPointItemData item in itemListByHeroIndex)
		{
			ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(item.ItemCode);
			if (dataByCode != null)
			{
				ItemInfo itemInfo = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Get(dataByCode.Index);
				if (itemInfo != null && itemInfo.Count > 0)
				{
					NPCFriendshipItemComponent nPCFriendshipItemComponent = UIHelper.AddGridItem(PresentGrid, NPCFriendshipItemComponent.PrefabAuto2);
					nPCFriendshipItemComponent.Open(item, _vData.NpcFriendlyInfo, itemInfo.Count, NpcFriendlyType.None, null);
					nPCFriendshipItemComponent.Select(isSelect: false);
					_ItemCompList.Add(nPCFriendshipItemComponent);
				}
			}
		}
		if (__Extension_Method_ICollection__.COUNT(_ItemCompList) > 0)
		{
			NPCFriendshipItemComponent nPCFriendshipItemComponent2 = _ItemCompList[0];
			nPCFriendshipItemComponent2.Select(isSelect: true);
			_currItemComp = nPCFriendshipItemComponent2;
		}
		UIHelper.EnableCollider(PresentAmountInput, !__Extension_Method_ICollection__.IsNullOrEmpty(_ItemCompList));
		UIHelper.EnableGameObject(NotEnoughPresentLabel, __Extension_Method_ICollection__.IsNullOrEmpty(_ItemCompList));
	}

	private void CheckNodeRewardState()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_nodeCompList); i++)
		{
			EventStepRewardComponent eventStepRewardComponent = _nodeCompList[i];
			if (eventStepRewardComponent == null)
			{
				continue;
			}
			eventStepRewardComponent.RefreshComponent();
			if (eventStepRewardComponent.RewardType == EventStepRewardComponent.ERewardType.Repeatable)
			{
				if (_vData.CurrStep >= _vData.CurrRewardStep)
				{
					eventStepRewardComponent.ChangeRewardState(EventStepRewardComponent.ERewardReciveType.CanTake);
				}
				else
				{
					eventStepRewardComponent.ChangeRewardState(EventStepRewardComponent.ERewardReciveType.Already);
				}
			}
			else if (eventStepRewardComponent.RewardType == EventStepRewardComponent.ERewardType.LimitedRepeat)
			{
				if (_vData.CurrStep >= _vData.CurrRewardStep)
				{
					bool flag = true;
					NPCFriendlyPointData nPCFriendlyPointData = eventStepRewardComponent.GetNPCFriendlyPointData();
					if (nPCFriendlyPointData != null && nPCFriendlyPointData.EndRewardRepeatCount > 0 && _vData.EndRewardCount >= nPCFriendlyPointData.EndRewardRepeatCount)
					{
						flag = false;
					}
					eventStepRewardComponent.ChangeRewardState(flag ? EventStepRewardComponent.ERewardReciveType.CanTake : EventStepRewardComponent.ERewardReciveType.RepeatAlready);
				}
				else
				{
					eventStepRewardComponent.ChangeRewardState(EventStepRewardComponent.ERewardReciveType.Waiting);
				}
			}
			else if (_vData.CurrStep < i)
			{
				if (eventStepRewardComponent.RewardType == EventStepRewardComponent.ERewardType.Repeatable || eventStepRewardComponent.RewardType == EventStepRewardComponent.ERewardType.LimitedRepeat)
				{
					if (_vData.CurrRewardStep < _vData.CurrStep)
					{
						eventStepRewardComponent.ChangeRewardState(EventStepRewardComponent.ERewardReciveType.CanTake);
					}
					else
					{
						eventStepRewardComponent.ChangeRewardState(EventStepRewardComponent.ERewardReciveType.Waiting);
					}
				}
				else
				{
					eventStepRewardComponent.ChangeRewardState(EventStepRewardComponent.ERewardReciveType.Waiting);
				}
			}
			else if (_vData.CurrStep == i)
			{
				if (_vData.ReqPoint == _vData.CurrPoint)
				{
					eventStepRewardComponent.ChangeRewardState(EventStepRewardComponent.ERewardReciveType.CanTake);
				}
				else
				{
					eventStepRewardComponent.ChangeRewardState(EventStepRewardComponent.ERewardReciveType.Waiting);
				}
			}
			else if (_vData.CurrRewardStep - 1 <= i)
			{
				eventStepRewardComponent.ChangeRewardState(EventStepRewardComponent.ERewardReciveType.CanTake);
			}
			else
			{
				eventStepRewardComponent.ChangeRewardState(EventStepRewardComponent.ERewardReciveType.Already);
			}
		}
	}

	private void CheckNodeStampState()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_nodeCompList); i++)
		{
			EventStepRewardComponent eventStepRewardComponent = _nodeCompList[i];
			if (eventStepRewardComponent == null || _vData.CurrStep < i || eventStepRewardComponent.RewardType == EventStepRewardComponent.ERewardType.LastReward || eventStepRewardComponent.RewardType == EventStepRewardComponent.ERewardType.Repeatable)
			{
				continue;
			}
			if (eventStepRewardComponent.RewardType == EventStepRewardComponent.ERewardType.LimitedRepeat)
			{
				NPCFriendlyPointData nPCFriendlyPointData = eventStepRewardComponent.GetNPCFriendlyPointData();
				if (nPCFriendlyPointData == null || nPCFriendlyPointData.EndRewardRepeatCount == 0 || _vData.EndRewardCount < nPCFriendlyPointData.EndRewardRepeatCount)
				{
					continue;
				}
			}
			if (_vData.CurrRewardStep - 1 > i)
			{
				eventStepRewardComponent.SetStampAnimation(isDirect: true);
			}
		}
	}

	private void StartStampAni()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_stampAniList); i++)
		{
			EventStepRewardComponent eventStepRewardComponent = _stampAniList[i];
			if (!(eventStepRewardComponent == null))
			{
				eventStepRewardComponent.SetStampAnimation();
			}
		}
		Vespa_Generic_ICollection_Extension_Method.CLEAR(_stampAniList);
	}

	private void ChangeSelectGaugeValue(EGaugeType gaugeType)
	{
		if (_currItemComp == null)
		{
			NextGaugeSlider.value = 0f;
			UIHelper.SetLabelText(AddPointLabel, "");
		}
		int currStep = _vData.CurrStep;
		float currGaugeValue = _vData.GetCurrGaugeValue();
		float nextGaugeValue = _vData.GetNextGaugeValue(_currItemComp, _currPresentAmount);
		switch (gaugeType)
		{
		case EGaugeType.Current:
		{
			CurrGaugeSlider.value = currGaugeValue;
			string text = "";
			NPCFriendlyPointData nPCFriendlyPointData = _vData.PointDataList.Last();
			if (nPCFriendlyPointData != null && nPCFriendlyPointData.EndRewardRepeatCount != 0)
			{
				int num2 = _vData.CurrStep + 1;
				if (num2 >= nPCFriendlyPointData.Step)
				{
					num2 = nPCFriendlyPointData.Step;
				}
				text = LocalizationManager.GetStringDirect("EVENT_NPC_GIFT_STEP", num2);
			}
			else
			{
				text = ((_vData.CurrStep >= _vData.MaxStep - 1) ? LocalizationManager.GetStringDirect("CHALLENGETOWER_REPEAT_REWARD") : LocalizationManager.GetStringDirect("EVENT_NPC_GIFT_STEP", _vData.CurrStep + 1));
			}
			UIHelper.SetLabelText(PresentStepLabel, text);
			UIHelper.SetLabelText(CurrPointLabel, $"{_vData.CurrPoint} / {_vData.ReqPoint}");
			for (int j = 0; j < __Extension_Method_ICollection__.COUNT(_nodeCompList); j++)
			{
				EventStepRewardComponent eventStepRewardComponent2 = _nodeCompList[j];
				if (eventStepRewardComponent2 == null)
				{
					continue;
				}
				if (j < currStep)
				{
					if (eventStepRewardComponent2.RewardType == EventStepRewardComponent.ERewardType.Repeatable || eventStepRewardComponent2.RewardType == EventStepRewardComponent.ERewardType.LimitedRepeat)
					{
						eventStepRewardComponent2.SetCurrGauge(_vData.GetCurrGaugeValue());
						UpdateRepeatRewardAmount(eventStepRewardComponent2);
					}
					else
					{
						eventStepRewardComponent2.SetCurrGauge(1f);
					}
				}
				else if (j == currStep)
				{
					eventStepRewardComponent2.SetCurrGauge(currGaugeValue);
				}
				else
				{
					eventStepRewardComponent2.SetCurrGauge(0f);
				}
				eventStepRewardComponent2.SetNextGauge(0f);
			}
			CheckNodeRewardState();
			break;
		}
		case EGaugeType.Next:
		{
			if (_currItemComp == null || _currPresentAmount <= 0)
			{
				NextGaugeSlider.value = 0f;
				UIHelper.EnableGameObject(AddPointLabel, enable: false);
				UIHelper.SetLabelText(AddPointLabel, "");
				break;
			}
			int num = _currItemComp.FriendshipPoint * _currPresentAmount;
			NextGaugeSlider.value = nextGaugeValue;
			UIHelper.EnableGameObject(AddPointLabel, _currPresentAmount > 0);
			UIHelper.SetLabelText(AddPointLabel, $"+{num}");
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_nodeCompList); i++)
			{
				EventStepRewardComponent eventStepRewardComponent = _nodeCompList[i];
				if (!(eventStepRewardComponent == null))
				{
					if (i == currStep)
					{
						eventStepRewardComponent.SetNextGauge(nextGaugeValue);
					}
					else
					{
						eventStepRewardComponent.SetNextGauge(0f);
					}
				}
			}
			break;
		}
		}
		PresentAmountInput.value = _currPresentAmount.ToString();
		int num3 = ((!(_currItemComp == null)) ? _currItemComp.ItemMaxCount : 0);
		UIHelper.EnableButton(IncreaseButton, _currPresentAmount < num3 && nextGaugeValue < 1f);
		UIHelper.EnableButton(DecreaseButton, _currPresentAmount > 0);
		UIHelper.EnableButton(MaxButton, _currItemComp != null);
		UIHelper.EnableButton(PresentButton, _currPresentAmount > 0);
	}

	public void UpdateRepeatRewardAmount(EventStepRewardComponent curComp = null)
	{
		if (curComp == null)
		{
			curComp = _nodeCompList.Find((EventStepRewardComponent x) => x.RewardType == EventStepRewardComponent.ERewardType.Repeatable);
			if (curComp == null)
			{
				return;
			}
		}
		curComp.SetRepeatRewardAmount(0);
		if (_vData.CurrRewardStep >= _vData.MaxStep)
		{
			int repeatRewardAmount = _vData.CurrStep - _vData.CurrRewardStep + 1;
			curComp.SetRepeatRewardAmount(repeatRewardAmount);
			return;
		}
		EventStepRewardComponent eventStepRewardComponent = _nodeCompList.Last();
		if (eventStepRewardComponent != null && eventStepRewardComponent.RewardType == EventStepRewardComponent.ERewardType.Repeatable)
		{
			int currStep = _vData.CurrStep;
			int maxStep = _vData.MaxStep;
			int num = currStep - maxStep + 1;
			if (_vData.CurrRewardStep <= _vData.MaxStep)
			{
				num++;
			}
			curComp.SetRepeatRewardAmount(num);
		}
	}

	public void OnClickPresentAmountIncrease()
	{
		if (_gaugeAnimation == EGaugeAnimation.Idle)
		{
			_currPresentAmount++;
			if (_currPresentAmount == int.MaxValue)
			{
				_currPresentAmount--;
			}
			ChangeSelectGaugeValue(EGaugeType.Next);
		}
	}

	public void OnClickPresentAmountDecrease()
	{
		if (_gaugeAnimation == EGaugeAnimation.Idle)
		{
			_currPresentAmount--;
			if (_currPresentAmount < 0)
			{
				_currPresentAmount = 0;
			}
			ChangeSelectGaugeValue(EGaugeType.Next);
		}
	}

	public void OnClickAmountMax()
	{
		if (!(_currItemComp == null) && _gaugeAnimation == EGaugeAnimation.Idle)
		{
			int friendshipPoint = _currItemComp.FriendshipPoint;
			int num = (int)Mathf.Ceil((float)(_vData.ReqPoint - _vData.CurrPoint) / (float)friendshipPoint);
			if (num > _currItemComp.ItemMaxCount)
			{
				_currPresentAmount = _currItemComp.ItemMaxCount;
			}
			else
			{
				_currPresentAmount = num;
			}
			ChangeSelectGaugeValue(EGaugeType.Next);
		}
	}

	public void OnClickConfrimPresent()
	{
		if (_currItemComp == null)
		{
			CenterMessage.Open(LocalizationManager.GetStringDirect("RESULT_TYPE.EVENT_ITEM_NOT_SELLECT"));
		}
		else
		{
			if (_currPresentAmount <= 0 || _gaugeAnimation != EGaugeAnimation.Idle)
			{
				return;
			}
			int currStep = _vData.CurrStep;
			int nextStep = currStep + 1;
			bool flag = false;
			NPCFriendlyPointData nPCFriendlyPointData = _vData.PointDataList.Find((NPCFriendlyPointData x) => x.Step == nextStep);
			if (nPCFriendlyPointData != null)
			{
				flag = nPCFriendlyPointData.LimitFriendlyPoint;
			}
			if (flag && _vData.CurrRewardStep <= currStep)
			{
				CenterMessage.Open(LocalizationManager.GetStringDirect("HERO_PRESENT_LIMITPOINT"));
				return;
			}
			if (nPCFriendlyPointData == null)
			{
				int num = _vData.PointDataList.Max((NPCFriendlyPointData x) => x.Step);
				if (nextStep >= num)
				{
					NPCFriendlyPointData nPCFriendlyPointData2 = _vData.PointDataList.Last();
					if (nPCFriendlyPointData2 != null && nPCFriendlyPointData2.EndRewardRepeatCount != 0)
					{
						int num2 = 0;
						foreach (NPCFriendlyPointData pointData in _vData.PointDataList)
						{
							num2 += pointData.ReqFriendlyPoint;
						}
						int num3 = nPCFriendlyPointData2.EndRewardRepeatCount - 1;
						if (num3 > 0)
						{
							num2 += nPCFriendlyPointData2.ReqFriendlyPoint * num3;
						}
						NPCFriendlyInfo nPCFriendlyInfo = GlobalAutoBehaviour<AccountManager>.instance.NPCFriendlyManager.Get(nPCFriendlyPointData2.HeroIndex);
						if (nPCFriendlyInfo != null && nPCFriendlyInfo.FriendlyPoint >= num2)
						{
							CenterMessage.Open(LocalizationManager.GetStringDirect("RESULT_TYPE.MAX_FRIENDLY_POINT"));
							return;
						}
					}
				}
			}
			if (_listener != null)
			{
				_listener.OnClickConfrimPresent(_vData.NpcData.HeroIndex, _currItemComp.ItemIndex, _currPresentAmount);
			}
		}
	}

	public void SubmitPresentAmountInput()
	{
		int result = 0;
		if (!int.TryParse(PresentAmountInput.value, out result))
		{
			_currPresentAmount = 0;
		}
		else
		{
			int friendshipPoint = _currItemComp.FriendshipPoint;
			int num = (int)Mathf.Ceil((float)(_vData.ReqPoint - _vData.CurrPoint) / (float)friendshipPoint);
			int itemMaxCount = _currItemComp.ItemMaxCount;
			if (result < 0)
			{
				_currPresentAmount = 0;
			}
			else if (result >= num || result >= itemMaxCount)
			{
				if (itemMaxCount >= num)
				{
					_currPresentAmount = num;
				}
				else
				{
					_currPresentAmount = itemMaxCount;
				}
			}
			else
			{
				_currPresentAmount = result;
			}
		}
		ChangeSelectGaugeValue(EGaugeType.Next);
	}

	public void StartNpcReaction()
	{
		int num = 1;
		num = ((_vData.CurrStep < _vData.MaxStep) ? (_vData.CurrStep + 1) : _vData.MaxStep);
		NPCFriendlyActionData data = GameRoot.Instance.NPCFriendlyActionDataContainer.GetData(_vData.NpcData.HeroIndex, num);
		if (data != null)
		{
			string[] eventRandom = data.GetEventRandom();
			ResponseAction(eventRandom[2], eventRandom[1], bBlendAction: false);
			UIHelper.EnableGameObject(NpcSpeakBubble, enable: true);
			string stringDirect = LocalizationManager.GetStringDirect(eventRandom[0]);
			UIHelper.SetLabelText(NpcSpeakLabel, stringDirect);
			NpcSpeakBubble.ResetToBeginning();
			NpcSpeakBubble.PlayForward();
		}
	}

	public void StartCoroutineGaugeAnimation(NPCFriendlyInfo _prevInfo, NPCFriendlyInfo _currInfo, float _aniTime, Action _action = null)
	{
		NPCFriendlyInfo nPCFriendlyInfo = _prevInfo;
		if (nPCFriendlyInfo == null)
		{
			nPCFriendlyInfo = _vData.NpcFriendlyInfo;
		}
		float currGaugeValue = _vData.GetCurrGaugeValue();
		int currStep = _vData.CurrStep;
		int currRewardStep = _vData.CurrRewardStep;
		_vData.UpdateNpcFriendlyInfo(_currInfo);
		float nextVal = _vData.GetCurrGaugeValue();
		int currStep2 = _vData.CurrStep;
		int currRewardStep2 = _vData.CurrRewardStep;
		if (currRewardStep != currRewardStep2)
		{
			CheckNodeRewardState();
			if (currRewardStep >= _vData.MaxStep - 1)
			{
				_stampAniList.Add(_nodeCompList.Last());
			}
			else
			{
				_stampAniList.Add(_nodeCompList[currRewardStep - 1]);
			}
			UpdateRepeatRewardAmount();
		}
		else if (currRewardStep == _vData.MaxStep && currStep > currStep2)
		{
			CheckNodeRewardState();
			_stampAniList.Add(_nodeCompList.Last());
			UpdateRepeatRewardAmount();
		}
		else if (currRewardStep == _vData.MaxStep && nPCFriendlyInfo.EndRewardCount < _currInfo.EndRewardCount)
		{
			CheckNodeRewardState();
			_stampAniList.Add(_nodeCompList.Last());
			UpdateRepeatRewardAmount();
		}
		else
		{
			if (currStep2 > currStep)
			{
				nextVal = 1f;
			}
			StartCoroutineGaugeAnimation(CurrGaugeSlider, currGaugeValue, nextVal, _aniTime, _action);
		}
	}

	public void StartCoroutineGaugeAnimation(UISlider target, float currVal, float nextVal, float aniTime, Action action = null)
	{
		if (_gaugeAnimation != EGaugeAnimation.Running)
		{
			StartNpcReaction();
			ExtensionMethod_VespaCoroutine.StartCoroutineThis(this, coGaugeAnimaion(target, currVal, nextVal, aniTime, action));
		}
	}

	public void StartCoroutineFullGaugeParticle()
	{
		ExtensionMethod_VespaCoroutine.StartCoroutineThis(this, coFullGaugeParticle());
	}

	private IEnumerator coGaugeAnimaion(UISlider target, float currVal, float nextVal, float aniTime, Action action = null)
	{
		if (_gaugeAnimation == EGaugeAnimation.Idle)
		{
			_gaugeAnimation = EGaugeAnimation.Running;
			float elaps = 0f;
			EventStepRewardComponent curNode;
			if (_vData.CurrStep < __Extension_Method_ICollection__.COUNT(_nodeCompList))
			{
				int num = _vData.CurrStep;
				if (nextVal >= 1f && num > 0)
				{
					num--;
				}
				curNode = _nodeCompList[num];
			}
			else
			{
				curNode = _nodeCompList.Last();
			}
			while (true)
			{
				float currGauge = (target.value = Mathf.Lerp(currVal, nextVal, elaps / aniTime));
				curNode.SetCurrGauge(currGauge);
				elaps += Time.deltaTime;
				if (elaps >= aniTime)
				{
					break;
				}
				yield return null;
			}
			target.value = nextVal;
			curNode.SetCurrGauge(nextVal);
			if (nextVal >= 1f)
			{
				StartCoroutineFullGaugeParticle();
				curNode.PlayReciveEffect();
			}
			yield return new WaitForSeconds(1f);
			_gaugeAnimation = EGaugeAnimation.Idle;
		}
		ChangeSelectGaugeValue(EGaugeType.Current);
		if (action != null)
		{
			Vespa_System_Action_Extension_Method.fire(action);
		}
	}

	private IEnumerator coFullGaugeParticle()
	{
		if (!PointMaxParticle.isPlaying)
		{
			UIHelper.EnableGameObject(PointMaxParticle, enable: true);
			PointMaxParticle.time = 0f;
			PointMaxParticle.Play();
			SoundStock stock = new SoundStock
			{
				Code = "UI2.TutorialBlink"
			};
			SingletonAuto<SoundContainer>.instance.PlayOnce(stock, 1f);
			while (PointMaxParticle.isPlaying)
			{
				yield return null;
			}
			UIHelper.EnableGameObject(PointMaxParticle, enable: false);
		}
	}

	protected override int CheckReplaceBasicCostume(int costumeIndex)
	{
		return -1;
	}

	void EventStepRewardComponent.Listener.OnClickGetReward(int heroIndex, int step, NpcFriendlyType type, Action action = null)
	{
		if (_listener == null || _gaugeAnimation != EGaugeAnimation.Idle)
		{
			return;
		}
		int num = step;
		if (_vData.CurrRewardStep < _vData.MaxStep)
		{
			num++;
			if (_vData.CurrRewardStep < num)
			{
				CenterMessage.Open(LocalizationManager.GetStringDirect("EVENT_REWARD_GET_NOTICE_MESSAGE"));
				return;
			}
		}
		else
		{
			num = _vData.MaxStep;
		}
		_listener.OnClickGetReward(heroIndex, num, type, delegate
		{
			StartStampAni();
		});
	}

	void EventStepRewardComponent.Listener.OverlapRewardView(int rewardIndex, int heroIndex, int step)
	{
		if (_listener != null && _gaugeAnimation == EGaugeAnimation.Idle)
		{
			CreatureOverlapData data = GameRoot.Instance.CreatureOverlapContainer.GetData(heroIndex);
			if (data != null)
			{
				OverlapRewardPopup.Open(rewardIndex, data.OverlapRewardIndex);
			}
		}
	}

	ViewDatas EventStepRewardComponent.Listener.GetViewDatas()
	{
		return _vData;
	}

	void IUserObserver<NPCFriendlyInfo>.OnChanged(NPCFriendlyInfo changedInfo)
	{
		ResetControl();
		StartCoroutineGaugeAnimation(null, changedInfo, 1f);
	}

	bool IUserObserver.IsGlobal()
	{
		return false;
	}

	void IUserObserver<ItemInfo>.OnChanged(ItemInfo changedInfo)
	{
		ResetPresentItemList();
	}

	void ItemIconComponent.Listener.OnClick(ItemIconComponent itemIconComponent)
	{
		throw new NotImplementedException();
	}
}
