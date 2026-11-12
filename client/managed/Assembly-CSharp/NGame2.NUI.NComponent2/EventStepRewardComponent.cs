using System;
using System.Collections;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NWindow;
using NGameAsset;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class EventStepRewardComponent : LinkFactoryComponent, ItemIconComponent.Listener, CurrencyIconComponent.Listener
{
	public interface Listener
	{
		void OnClickGetReward(int heroIndex, int step, NpcFriendlyType type, Action action = null);

		void OverlapRewardView(int rewardIndex, int heroIndex, int step);

		EventStepRewardView.ViewDatas GetViewDatas();
	}

	public enum ECustomDir
	{
		Top,
		Bot,
		Left,
		Right
	}

	public enum ERewardType
	{
		NONE = -1,
		StepReward,
		LastReward,
		Repeatable,
		LimitedRepeat
	}

	public enum ENodeType
	{
		Start,
		Bridge,
		End
	}

	public enum EGaugeAnimation
	{
		Idle,
		Running
	}

	public enum ERewardReciveType
	{
		NONE = -1,
		Already,
		CanTake,
		Waiting,
		RepeatAlready
	}

	[Serializable]
	public class RewardData
	{
		public ERewardType RewardType;

		public Transform RewardParent;

		public Transform RewardGenPos;
	}

	[Serializable]
	public class StampObject
	{
		public string EventCode;

		public ParticleSystem StampParticle;

		public TweenAlpha StampAnimationAlpha;

		public TweenScale StampAnimationScale;

		public Transform StampSprite;
	}

	protected const string PrefabPath = "Resources/Prefab2/Window/EventStepReward/EventStepRewardComponent";

	protected static PrefabAuto<EventStepRewardComponent> PrefabAuto = new PrefabAuto<EventStepRewardComponent>("Resources/Prefab2/Window/EventStepReward/EventStepRewardComponent");

	protected const string PancironnePrefabPath = "Resources/Prefab2/Window/EventStepReward/PancironneEventStepRewardComponent";

	protected static PrefabAuto<EventStepRewardComponent> PancironnePrefabAuto = new PrefabAuto<EventStepRewardComponent>("Resources/Prefab2/Window/EventStepReward/PancironneEventStepRewardComponent");

	[Header("[Node Setting]")]
	public ENodeType NodeType;

	public ECustomDir GaugeDir = ECustomDir.Right;

	public ECustomDir RewardDir;

	public ERewardType RewardType;

	public ERewardReciveType ReciveType = ERewardReciveType.NONE;

	[Header("[Parent Group]")]
	public Transform StartPosParent;

	public Transform EndPosParent;

	public Transform RewardParent;

	[Header("[Gague Group]")]
	public Transform GaugeRot;

	public UISlider GaugeSlider;

	public UISprite[] GaugeArray;

	public float GaugeOffset;

	[Header("[Rot Group]")]
	public List<Transform> RepeatSliderRot;

	public List<Transform> RepeatableRot;

	[Header("[Reward Group]")]
	public List<RewardData> RewardDatas;

	public Transform[] CanTakeReward;

	[Header("[Gauge Slider]")]
	public UISlider GaugeCurrent;

	public UISlider GaugeNext;

	public List<UISprite> RepeatSprite;

	public List<UISlider> RepeatableSlider;

	[Header("[Stamp Group]")]
	public Transform StampAnimationParent;

	public StampObject[] StampObjects;

	[Header("[ETC]")]
	public Transform OffHeart;

	public ParticleSystem PointMaxParticle;

	public UILabel LastRewardAmount;

	public UILabel RepeatRewardAmount;

	[Header("[ColorCode]")]
	public List<UIWidget> ColorCodeObjs;

	private NPCFriendlyPointData _npcFriendlyPointData;

	public int _nodeStep;

	private int _heroIndex;

	private ItemIconComponent _rewardItemIcon;

	private CurrencyIconComponent _rewardCurrencyIconComponent;

	private Listener _listener;

	private int _gaugeWidth;

	public EGaugeAnimation _gaugeAnimation { get; private set; }

	private RewardData _currRewardData => RewardDatas.Find((RewardData x) => x.RewardType == RewardType);

	public static EventStepRewardComponent CreateComponent(Transform nodeRoot, string componentTypeVal = "")
	{
		if (string.Equals(componentTypeVal, "Pancironne", StringComparison.OrdinalIgnoreCase))
		{
			return PancironnePrefabAuto.Factory.Create(nodeRoot);
		}
		return PrefabAuto.Factory.Create(nodeRoot);
	}

	public void Open(NPCFriendlyPointData data, Listener listener, int nodeCnt, ENodeType nType, ECustomDir gDir, ECustomDir rDir, ERewardType rType, int gWidth, Vector3 pos)
	{
		_npcFriendlyPointData = data;
		_listener = listener;
		_nodeStep = nodeCnt;
		_gaugeWidth = gWidth;
		NodeType = nType;
		GaugeDir = gDir;
		RewardDir = rDir;
		RewardType = rType;
		SetReward();
		SetRepeatable();
		base.transform.localPosition = pos;
		UIHelper.EnableGameObject(StampAnimationParent, enable: false);
		UIHelper.EnableGameObject(PointMaxParticle, enable: false);
		SetColorCode(data);
		Execute();
	}

	private void SetColorCode(NPCFriendlyPointData data)
	{
		if (data == null || __Extension_Method_ICollection__.COUNT(data.UIColor) < 3 || !ColorUtility.TryParseHtmlString(data.UIColor[0], out var color))
		{
			return;
		}
		foreach (UIWidget colorCodeObj in ColorCodeObjs)
		{
			UIHelper.SetWidgetColor(colorCodeObj, color);
		}
		if (ColorUtility.TryParseHtmlString(data.UIColor[1], out var color2))
		{
			UIHelper.SetWidgetColor(GaugeCurrent.GetComponent<UIWidget>(), color2);
			if (ColorUtility.TryParseHtmlString(data.UIColor[2], out var color3))
			{
				UIHelper.SetWidgetColor(GaugeNext.GetComponent<UIWidget>(), color3);
			}
		}
	}

	private void SetReward()
	{
		if (_npcFriendlyPointData == null)
		{
			return;
		}
		NShared.RewardData data = GameRoot.Instance.RewardDataContainer.GetData(_npcFriendlyPointData.RewardIndex);
		if (data != null)
		{
			CurrencyType currencyType = data.GetCurrencyType();
			if (currencyType != CurrencyType.None)
			{
				FactoryBase.SafeReleaseComponent(ref _rewardCurrencyIconComponent);
				_rewardCurrencyIconComponent = CurrencyIconComponent.PrefabAuto.Factory.Create(_currRewardData.RewardGenPos.transform);
				CurrencyIconComponent rewardCurrencyIconComponent = _rewardCurrencyIconComponent;
				int gemMax = data.GemMax;
				rewardCurrencyIconComponent.Open(currencyType, gemMax, this);
				Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_rewardCurrencyIconComponent.transform);
			}
			else
			{
				SetRewardItemIcon();
			}
		}
	}

	public void UpdateData(NPCFriendlyPointData data)
	{
		_npcFriendlyPointData = data;
		SetRewardItemIcon();
		SetRepeatable();
		Execute();
	}

	private void SetRewardItemIcon()
	{
		Transform iconRoot = _currRewardData.RewardGenPos.transform;
		FactoryBase.SafeReleaseComponent(ref _rewardItemIcon);
		_rewardItemIcon = UIHelper.CreateRewardMainItemIconComponent(_npcFriendlyPointData.RewardIndex, iconRoot, this);
		if (_npcFriendlyPointData != null)
		{
			EventStepRewardView.ViewDatas viewDatas = _listener.GetViewDatas();
			if (viewDatas != null && viewDatas.CurrRewardStep <= _npcFriendlyPointData.Step)
			{
				_rewardItemIcon.SetRewardNotice(IsOverlapReward());
			}
		}
	}

	private void SetRepeatable()
	{
		bool enable = false;
		if (RewardType == ERewardType.Repeatable)
		{
			enable = true;
		}
		else if (RewardType == ERewardType.LimitedRepeat)
		{
			enable = _npcFriendlyPointData != null && _npcFriendlyPointData.EndRewardRepeatCount == 0;
		}
		foreach (Transform item in RepeatableRot)
		{
			UIHelper.EnableGameObject(item, enable);
		}
	}

	public void RefreshComponent()
	{
		if (_rewardItemIcon != null)
		{
			_rewardItemIcon.Refresh();
		}
	}

	public void SetStampAnimation(bool isDirect = false)
	{
		if (isDirect)
		{
			if (!UIHelper.IsEnabledGameObjet(StampAnimationParent.gameObject))
			{
				StampObject stampObject = GetStampObject();
				UIHelper.EnableGameObject(StampAnimationParent, enable: true);
				UIHelper.EnableGameObject(stampObject.StampParticle, enable: false);
				stampObject.StampAnimationAlpha.enabled = false;
				stampObject.StampAnimationScale.enabled = false;
				stampObject.StampSprite.localScale = Vector3.one;
				UIHelper.SetWidgetColor(stampObject.StampSprite.GetComponent<UIWidget>(), Color.white);
			}
		}
		else
		{
			ExtensionMethod_VespaCoroutine.StartCoroutineThis(this, coStampAnimationForward());
		}
	}

	private IEnumerator coStampAnimationForward()
	{
		UIHelper.EnableGameObject(StampAnimationParent, enable: true);
		StampObject stampObj = GetStampObject();
		stampObj.StampAnimationAlpha.ResetToBeginning();
		stampObj.StampAnimationScale.ResetToBeginning();
		stampObj.StampAnimationAlpha.PlayForward();
		stampObj.StampAnimationScale.PlayForward();
		UIHelper.EnableGameObject(stampObj.StampParticle, enable: true);
		stampObj.StampParticle.time = 0f;
		stampObj.StampParticle.Play();
		SoundStock stock = new SoundStock
		{
			Code = "UI2.Stamp"
		};
		SingletonAuto<SoundContainer>.instance.PlayOnce(stock, 1f);
		while (stampObj.StampAnimationAlpha.isActiveAndEnabled || stampObj.StampAnimationScale.isActiveAndEnabled)
		{
			yield return null;
		}
		if (RewardType == ERewardType.LastReward || RewardType == ERewardType.Repeatable)
		{
			ExtensionMethod_VespaCoroutine.StartCoroutineThis(this, coStampAnimationReverse());
		}
		else if (RewardType == ERewardType.LimitedRepeat)
		{
			EventStepRewardView.ViewDatas viewDatas = _listener.GetViewDatas();
			if (viewDatas != null && _npcFriendlyPointData.EndRewardRepeatCount > 0 && viewDatas.EndRewardCount < _npcFriendlyPointData.EndRewardRepeatCount)
			{
				ExtensionMethod_VespaCoroutine.StartCoroutineThis(this, coStampAnimationReverse());
			}
		}
	}

	private IEnumerator coStampAnimationReverse()
	{
		StampObject stampObj = GetStampObject();
		stampObj.StampAnimationAlpha.PlayReverse();
		while (stampObj.StampAnimationAlpha.isActiveAndEnabled)
		{
			yield return null;
		}
		UIHelper.EnableGameObject(StampAnimationParent, enable: false);
	}

	[ContextMenu("Execute Progress Node")]
	public void Execute()
	{
		if (_gaugeWidth == 0)
		{
			_gaugeWidth = 200;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(RewardDatas); i++)
		{
			ERewardType rewardType = RewardDatas[i].RewardType;
			UIHelper.EnableGameObject(RewardDatas[i].RewardParent, CompareRewardType(rewardType, RewardType));
		}
		switch (NodeType)
		{
		case ENodeType.Start:
			StartPosParent.gameObject.SetActive(value: true);
			EndPosParent.gameObject.SetActive(value: true);
			break;
		case ENodeType.Bridge:
			StartPosParent.gameObject.SetActive(value: false);
			EndPosParent.gameObject.SetActive(value: true);
			break;
		case ENodeType.End:
			StartPosParent.gameObject.SetActive(value: false);
			EndPosParent.gameObject.SetActive(value: true);
			break;
		}
		UISprite[] gaugeArray = GaugeArray;
		for (int j = 0; j < gaugeArray.Length; j++)
		{
			gaugeArray[j].width = _gaugeWidth - (int)(GaugeOffset * 2f);
		}
		switch (GaugeDir)
		{
		case ECustomDir.Top:
			EndPosParent.transform.localPosition = new Vector2(0f, _gaugeWidth);
			RewardParent.transform.localPosition = new Vector2(0f, _gaugeWidth);
			GaugeRot.localEulerAngles = new Vector3(0f, 0f, 90f);
			break;
		case ECustomDir.Bot:
			EndPosParent.transform.localPosition = new Vector2(0f, -_gaugeWidth);
			RewardParent.transform.localPosition = new Vector2(0f, -_gaugeWidth);
			GaugeRot.localEulerAngles = new Vector3(0f, 0f, -90f);
			break;
		case ECustomDir.Left:
			EndPosParent.transform.localPosition = new Vector2(-_gaugeWidth, 0f);
			RewardParent.transform.localPosition = new Vector2(-_gaugeWidth, 0f);
			GaugeRot.localEulerAngles = new Vector3(0f, 180f, 0f);
			break;
		case ECustomDir.Right:
			EndPosParent.transform.localPosition = new Vector2(_gaugeWidth, 0f);
			RewardParent.transform.localPosition = new Vector2(_gaugeWidth, 0f);
			GaugeRot.localEulerAngles = Vector3.zero;
			break;
		}
		StampObject stampObject = GetStampObject();
		switch (RewardDir)
		{
		case ECustomDir.Top:
			RewardParent.transform.localEulerAngles = Vector3.zero;
			_currRewardData.RewardGenPos.transform.localEulerAngles = Vector3.zero;
			RepeatSliderRot.ForEach(delegate(Transform x)
			{
				x.localEulerAngles = Vector3.forward * 180f;
			});
			RepeatSprite.ForEach(delegate(UISprite x)
			{
				x.invert = true;
			});
			RepeatableRot.ForEach(delegate(Transform x)
			{
				x.localEulerAngles = Vector3.zero;
			});
			stampObject.StampSprite.localEulerAngles = Vector3.zero;
			break;
		case ECustomDir.Bot:
			RewardParent.transform.localEulerAngles = Vector3.forward * 180f;
			_currRewardData.RewardGenPos.transform.localEulerAngles = Vector3.forward * 180f;
			RepeatSliderRot.ForEach(delegate(Transform x)
			{
				x.localEulerAngles = Vector3.forward * 180f;
			});
			RepeatSprite.ForEach(delegate(UISprite x)
			{
				x.invert = true;
			});
			RepeatableRot.ForEach(delegate(Transform x)
			{
				x.localEulerAngles = Vector3.forward * 180f;
			});
			stampObject.StampSprite.localEulerAngles = Vector3.forward * 180f;
			break;
		case ECustomDir.Left:
			RewardParent.transform.localEulerAngles = Vector3.forward * 90f;
			_currRewardData.RewardGenPos.transform.localEulerAngles = Vector3.forward * -90f;
			RepeatSliderRot.ForEach(delegate(Transform x)
			{
				x.localEulerAngles = Vector3.forward * 180f;
			});
			RepeatSprite.ForEach(delegate(UISprite x)
			{
				x.invert = true;
			});
			RepeatableRot.ForEach(delegate(Transform x)
			{
				x.localEulerAngles = Vector3.forward * -90f;
			});
			stampObject.StampSprite.localEulerAngles = Vector3.forward * -90f;
			break;
		case ECustomDir.Right:
			RewardParent.transform.localEulerAngles = Vector3.forward * -90f;
			_currRewardData.RewardGenPos.transform.localEulerAngles = Vector3.forward * 90f;
			RepeatSliderRot.ForEach(delegate(Transform x)
			{
				x.localEulerAngles = Vector3.forward * 180f;
			});
			RepeatSprite.ForEach(delegate(UISprite x)
			{
				x.invert = false;
			});
			RepeatableRot.ForEach(delegate(Transform x)
			{
				x.localEulerAngles = Vector3.forward * 90f;
			});
			stampObject.StampSprite.localEulerAngles = Vector3.forward * 90f;
			break;
		}
		if (RewardDir == ECustomDir.Bot)
		{
			stampObject.StampSprite.localPosition = new Vector2(0f, 120f);
		}
		else
		{
			stampObject.StampSprite.localPosition = new Vector2(0f, 125f);
		}
	}

	private bool CompareRewardType(ERewardType a, ERewardType b)
	{
		if (a != b)
		{
			if ((a == ERewardType.Repeatable && b == ERewardType.LimitedRepeat) || (b == ERewardType.Repeatable && a == ERewardType.LimitedRepeat))
			{
				return true;
			}
			return false;
		}
		return true;
	}

	public void SetCurrGauge(float num)
	{
		if (RewardType == ERewardType.Repeatable || RewardType == ERewardType.LimitedRepeat)
		{
			RepeatableSlider.ForEach(delegate(UISlider x)
			{
				x.value = num;
			});
			GaugeCurrent.value = 1f;
		}
		else
		{
			GaugeCurrent.value = num;
		}
	}

	public void SetRepeatRewardAmount(int num)
	{
		UIHelper.EnableGameObject(RepeatRewardAmount, num > 0);
		UIHelper.SetLabelText(RepeatRewardAmount, num.ToString());
	}

	public void PlayReciveEffect()
	{
		Transform rewardGenPos = _currRewardData.RewardGenPos;
		PointMaxParticle.transform.position = rewardGenPos.position;
		UIHelper.EnableGameObject(PointMaxParticle, enable: true);
		PointMaxParticle.time = 0f;
		PointMaxParticle.Play();
	}

	public void SetNextGauge(float num)
	{
		GaugeNext.value = num;
	}

	public void ChangeRewardState(ERewardReciveType reciveType)
	{
		ReciveType = reciveType;
		UIHelper.EnableGameObject(OffHeart, ReciveType == ERewardReciveType.Already || ReciveType == ERewardReciveType.RepeatAlready);
		UIHelper.EnableGameObject(LastRewardAmount, ReciveType == ERewardReciveType.CanTake && RewardType == ERewardType.LastReward);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(CanTakeReward); i++)
		{
			Transform transform = CanTakeReward[i];
			if (!(transform == null))
			{
				UIHelper.EnableGameObject(transform, ReciveType == ERewardReciveType.CanTake);
			}
		}
	}

	public void StartCoroutineGaugeAnimation(UISlider target, float currVal, float nextVal, float maxVal, float aniTime)
	{
		if (_gaugeAnimation != EGaugeAnimation.Running)
		{
			ExtensionMethod_VespaCoroutine.StartCoroutineThis(this, coGaugeAnimaion(target, currVal, nextVal, maxVal, aniTime));
		}
	}

	public NPCFriendlyPointData GetNPCFriendlyPointData()
	{
		return _npcFriendlyPointData;
	}

	private IEnumerator coGaugeAnimaion(UISlider target, float currVal, float nextVal, float maxVal, float aniTime)
	{
		if (_gaugeAnimation != EGaugeAnimation.Idle)
		{
			yield break;
		}
		_gaugeAnimation = EGaugeAnimation.Running;
		float elaps = 0f;
		while (true)
		{
			target.value = Mathf.Lerp(currVal, nextVal, elaps / aniTime);
			elaps += Time.deltaTime;
			if (elaps >= aniTime)
			{
				break;
			}
			yield return null;
		}
		_gaugeAnimation = EGaugeAnimation.Idle;
	}

	private void ReceiveReward()
	{
		bool flag = IsOverlapReward();
		switch (ReciveType)
		{
		case ERewardReciveType.Already:
			if (RewardType == ERewardType.Repeatable || (RewardType == ERewardType.LimitedRepeat && !flag))
			{
				CenterMessage.Open(LocalizationManager.GetStringDirect("RESULT_TYPE.POINT_NOT_ENOUGH"));
			}
			break;
		case ERewardReciveType.CanTake:
			if (_listener != null)
			{
				_listener.OnClickGetReward(_npcFriendlyPointData.HeroIndex, _nodeStep, NpcFriendlyType.Event);
			}
			break;
		case ERewardReciveType.Waiting:
			if (!flag)
			{
				CenterMessage.Open(LocalizationManager.GetStringDirect("RESULT_TYPE.POINT_NOT_ENOUGH"));
			}
			break;
		case ERewardReciveType.RepeatAlready:
			break;
		}
	}

	private bool IsOverlapReward()
	{
		if (_npcFriendlyPointData == null)
		{
			return false;
		}
		NShared.RewardData data = GameRoot.Instance.RewardDataContainer.GetData(_npcFriendlyPointData.RewardIndex);
		if (data == null)
		{
			return false;
		}
		List<string> rewardItemCodeList = data.GetRewardItemCodeList();
		if (rewardItemCodeList == null || __Extension_Method_ICollection__.COUNT(rewardItemCodeList) != 1)
		{
			return false;
		}
		ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(rewardItemCodeList[0]);
		if (dataByCode == null)
		{
			return false;
		}
		if (dataByCode.Type != ItemType.Hero)
		{
			return false;
		}
		_heroIndex = dataByCode.GetValueInt(0);
		if (GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(_heroIndex) == null)
		{
			return false;
		}
		return true;
	}

	private void OverlapRewardView()
	{
		if (!IsOverlapReward())
		{
			return;
		}
		if (_npcFriendlyPointData != null)
		{
			EventStepRewardView.ViewDatas viewDatas = _listener.GetViewDatas();
			if (viewDatas != null && viewDatas.CurrRewardStep > _npcFriendlyPointData.Step)
			{
				return;
			}
		}
		if (_listener != null)
		{
			_listener.OverlapRewardView(_npcFriendlyPointData.RewardIndex, _heroIndex, _nodeStep);
		}
	}

	private StampObject GetStampObject()
	{
		StampObject[] stampObjects = StampObjects;
		for (int i = 0; i < stampObjects.Length; i++)
		{
			UIHelper.EnableGameObject(stampObjects[i].StampSprite, enable: false);
		}
		if (_npcFriendlyPointData == null)
		{
			UIHelper.EnableGameObject(StampObjects[0].StampSprite, enable: true);
			return StampObjects[0];
		}
		string stampName = _npcFriendlyPointData.StampName;
		if (string.IsNullOrEmpty(stampName))
		{
			UIHelper.EnableGameObject(StampObjects[0].StampSprite, enable: true);
			return StampObjects[0];
		}
		stampObjects = StampObjects;
		foreach (StampObject stampObject in stampObjects)
		{
			if (stampObject.EventCode == stampName)
			{
				UIHelper.EnableGameObject(stampObject.StampSprite, enable: true);
				return stampObject;
			}
		}
		UIHelper.EnableGameObject(StampObjects[0].StampSprite, enable: true);
		return StampObjects[0];
	}

	public void OnClick(ItemIconComponent itemIconComponent)
	{
		ReceiveReward();
		if (ReciveType != ERewardReciveType.CanTake)
		{
			OverlapRewardView();
		}
	}

	void CurrencyIconComponent.Listener.OnClick(CurrencyIconComponent component)
	{
		ReceiveReward();
		if (ReciveType != ERewardReciveType.CanTake)
		{
			OverlapRewardView();
		}
	}
}
