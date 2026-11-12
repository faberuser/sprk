using System;
using System.Collections;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NContents;
using NGame2.NLobby;
using NGame2.NScene;
using NGame2.NScene.NPreloader;
using NGame2.NUI.NManager;
using NShared;
using NVespa;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWorldMap.NMapLayout;

public class MapNode : LinkFactoryComponent, NVespa.NPrefab2.InitOnCreate, UICamera.IOnPressListener, UICamera.IOnClickListener, UICamera.IOnDragStartListener, UICamera.IOnDragEndListener, UICamera.IOnDragListener
{
	[Serializable]
	public class NodeDisableInfo
	{
		public enum EffectState
		{
			Idle,
			PlayForward,
			PlayReverse
		}

		[Tooltip("Dont Change This Enum State")]
		public EffectState CurrEffectState;

		public UISprite ShakmehNode;

		public AnimationCurve AlphaCurve;

		public float EffectDuration;
	}

	public enum EDecorateType
	{
		None,
		Battle,
		Fraction,
		GuildRaid,
		Name,
		NameEvent
	}

	private static readonly string _mapNodePrefabPath = "Prefab2/WorldMap/Node/";

	private static readonly Dictionary<string, PrefabAuto<MapNode>> _prefabAutoDictionary = new Dictionary<string, PrefabAuto<MapNode>>();

	private static readonly PrefabAuto<MapNode> _emptyMapNodePrefabAuto = new PrefabAuto<MapNode>(_mapNodePrefabPath + "None");

	public GameObject VisitableObject;

	public GameObject UnvisitableObject;

	public GameObject CompletedObject;

	public EDecorateType DecorateType;

	public float EffectDuration;

	public float EffectEndOverlap;

	public UIWidget EffectTarget;

	public AnimationCurve ScaleCurve;

	public AnimationCurve AlphaCurve;

	[Header("[Shakemh]")]
	public NodeDisableInfo NodeDisableData;

	[NonSerialized]
	public List<MapNode> NextNodes = new List<MapNode>();

	[NonSerialized]
	public List<MapNode> PrevNodes = new List<MapNode>();

	[NonSerialized]
	public List<MapNode> OneWayNodes = new List<MapNode>();

	private bool _pressed;

	private bool _canVisit;

	private bool _canBypass;

	private bool _isCompleted;

	private bool _isVisibleAvatar;

	private bool _isShakmehNode;

	private bool _isOrdealArenaNode;

	private DateTime _pressedTime;

	private Component _decoratorComponent;

	public bool IsPlaying { get; private set; }

	public int ChapterIndex
	{
		get
		{
			if (NodeData == null)
			{
				return 0;
			}
			return NodeData.ChapterIndex;
		}
	}

	public int DungeonIndex { get; private set; }

	public int LinkedDungeonIndex
	{
		get
		{
			if (NodeData == null)
			{
				return DungeonIndex;
			}
			return NodeData.DungeonIndex;
		}
	}

	public Action<MapNode> OnTouchAction { get; private set; }

	public Action<MapNode, Vector2> OnDragAction { get; private set; }

	public Action<MapNode> OnDragStartAction { get; private set; }

	public Action<MapNode> OnDragEndAction { get; private set; }

	public Action<MapNode> OnLongPressAction { get; private set; }

	public bool CanVisit => _canVisit;

	public bool CanBypass => _canBypass;

	public bool IsCompleted => _isCompleted;

	public bool IsVisibleAvatar => _isVisibleAvatar;

	public bool IsShakmehNode => _isShakmehNode;

	public bool IsOrdealArenaNode => _isOrdealArenaNode;

	public CampaignNodeData NodeData { get; private set; }

	public NodeFunctionType Function
	{
		get
		{
			if (NodeData == null)
			{
				return NodeFunctionType.None;
			}
			return NodeData.Function;
		}
	}

	public static void ReserveCache(BasePreloader preloader)
	{
		MapEdge2.PrefabAuto.Factory.ReserveComponent(5);
	}

	public static PrefabAuto<MapNode> GetPrefabAuto(string prefabName)
	{
		if (string.IsNullOrEmpty(prefabName))
		{
			return _emptyMapNodePrefabAuto;
		}
		PrefabAuto<MapNode> value = null;
		if (_prefabAutoDictionary.TryGetValue(prefabName.ToLower(), out value) && value != null)
		{
			return value;
		}
		value = new PrefabAuto<MapNode>(_mapNodePrefabPath + prefabName);
		_prefabAutoDictionary.Add(prefabName.ToLower(), value);
		return value;
	}

	void NVespa.NPrefab2.InitOnCreate.OnCreate()
	{
		UIHelper.EnableGameObject(this, enable: true);
	}

	void NVespa.NPrefab2.InitOnCreate.OnRelease()
	{
		NextNodes.Clear();
		PrevNodes.Clear();
		OneWayNodes.Clear();
		DungeonIndex = 0;
		NodeData = null;
		OnTouchAction = null;
		OnDragAction = null;
		_canVisit = false;
		_canBypass = false;
		_isCompleted = false;
		_isVisibleAvatar = false;
		_isShakmehNode = false;
		_isOrdealArenaNode = false;
		_pressed = false;
		if (NodeDisableData.ShakmehNode != null)
		{
			UIHelper.EnableGameObject(NodeDisableData.ShakmehNode, enable: false);
		}
		FactoryBase.SafeReleaseComponent(ref _decoratorComponent);
	}

	public void Open(int layer, int dungeonIndex, CampaignNodeData nodeData, Vector3 position, Vector3 nodeScale, Action<MapNode> onTouchNode, Action<MapNode, Vector2> onDragNode, Action<MapNode> onDragStartNode, Action<MapNode> onDragEndNode, Action<MapNode> onLongTouchNode)
	{
		Vespa_Unity_GameObject_Extension_Method.ChangeLayerRecursive(base.gameObject, layer, forceRecursive: true);
		base.transform.localPosition = position;
		base.transform.localScale = nodeScale;
		if (nodeData.NodePrefab.IsNullOrEmpty())
		{
			base.name = $"{nodeData.DungeonIndex}_None";
		}
		else if (nodeData.NodePrefab.StartsWith("MoveChapter"))
		{
			base.name = $"{nodeData.DungeonIndex}_MoveChapter";
		}
		else if (nodeData.NodePrefab.StartsWith("OrdealArena"))
		{
			base.name = $"{nodeData.DungeonIndex}_{nodeData.Function.ToString()}";
		}
		else
		{
			base.name = $"{nodeData.DungeonIndex}_{nodeData.NodePrefab}";
		}
		DungeonIndex = dungeonIndex;
		NodeData = nodeData;
		OnTouchAction = onTouchNode;
		OnDragAction = onDragNode;
		OnDragStartAction = onDragStartNode;
		OnDragEndAction = onDragEndNode;
		OnLongPressAction = onLongTouchNode;
		_canVisit = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.CanVisit(ChapterIndex, DungeonIndex);
		_canBypass = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.CanBypass(ChapterIndex, DungeonIndex);
		_isCompleted = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.IsCompleted(ChapterIndex, DungeonIndex);
		_isVisibleAvatar = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.IsVisibleAvatar(ChapterIndex, dungeonIndex, NodeData);
		_isShakmehNode = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.IsShakmehNode(ChapterIndex, dungeonIndex, NodeData);
		_isOrdealArenaNode = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.IsOrdealArenaNode(ChapterIndex, dungeonIndex, NodeData);
		switch (DecorateType)
		{
		case EDecorateType.Battle:
		{
			Transform transform = ((EffectTarget == null) ? base.transform : EffectTarget.transform);
			DecoratorBattle decoratorBattle = DecoratorBattle.PrefabAuto.Factory.Create(transform.transform);
			if (decoratorBattle != null)
			{
				Vespa_Unity_Transform_Extension_Method.ResetLocalTM(decoratorBattle.transform);
				decoratorBattle.Open(ChapterIndex, DungeonIndex, CanVisit);
				_decoratorComponent = decoratorBattle;
			}
			break;
		}
		case EDecorateType.Fraction:
		{
			DecoratorFraction decoratorFraction = DecoratorFraction.PrefabAuto.Factory.Create(base.transform);
			if (decoratorFraction != null)
			{
				Vespa_Unity_Transform_Extension_Method.ResetLocalTM(decoratorFraction.transform);
				decoratorFraction.Open(ChapterIndex, DungeonIndex, CanVisit);
				_decoratorComponent = decoratorFraction;
			}
			break;
		}
		case EDecorateType.GuildRaid:
		{
			DecoratorGuildRaid decoratorGuildRaid = DecoratorGuildRaid.PrefabAuto.Factory.Create(base.transform);
			if (decoratorGuildRaid != null)
			{
				Vespa_Unity_Transform_Extension_Method.ResetLocalTM(decoratorGuildRaid.transform);
				decoratorGuildRaid.Open(ChapterIndex, DungeonIndex);
				_decoratorComponent = decoratorGuildRaid;
			}
			break;
		}
		case EDecorateType.Name:
		{
			DecoratorName decoratorName = DecoratorName.PrefabAuto.Factory.Create(base.transform);
			if (decoratorName != null)
			{
				Vespa_Unity_Transform_Extension_Method.ResetLocalTM(decoratorName.transform);
				decoratorName.Open(ChapterIndex, DungeonIndex);
				_decoratorComponent = decoratorName;
			}
			break;
		}
		case EDecorateType.NameEvent:
		{
			DecoratorNameEvent decoratorNameEvent = DecoratorNameEvent.PrefabAuto.Factory.Create(base.transform);
			if (decoratorNameEvent != null)
			{
				Vespa_Unity_Transform_Extension_Method.ResetLocalTM(decoratorNameEvent.transform);
				decoratorNameEvent.Open(ChapterIndex, DungeonIndex);
				_decoratorComponent = decoratorNameEvent;
			}
			break;
		}
		}
		UpdateNodeState();
		UpdateShakmehNodeState();
	}

	public Vector3 GetPosition()
	{
		return base.transform.localPosition;
	}

	public void UpdateNodeState()
	{
		UIHelper.EnableGameObject(CompletedObject, enable: false);
		UIHelper.EnableGameObject(VisitableObject, enable: false);
		UIHelper.EnableGameObject(UnvisitableObject, enable: false);
		if (!CanVisit)
		{
			UIHelper.EnableGameObject(UnvisitableObject, enable: true);
		}
		else if (_isCompleted)
		{
			UIHelper.EnableGameObject(CompletedObject ? CompletedObject : VisitableObject, enable: true);
		}
		else
		{
			UIHelper.EnableGameObject(VisitableObject, enable: true);
		}
		if (!SingletonAuto<ContentsOpenChecker>.instance.CheckNodeOpen(ChapterIndex, DungeonIndex))
		{
			UIHelper.EnableGameObject(UnvisitableObject, enable: true);
			UIHelper.EnableGameObject(VisitableObject, enable: false);
		}
	}

	public void UpdateShakmehNodeState()
	{
		if (!_isShakmehNode)
		{
			if (NodeDisableData.ShakmehNode != null)
			{
				UIHelper.EnableGameObject(NodeDisableData.ShakmehNode, enable: false);
			}
			return;
		}
		NGame2.NUI.NManager.ShakmehDungeonManagement instance = SceneAutoBehaviour<NGame2.NUI.NManager.ShakmehDungeonManagement>.instance;
		if (!(instance == null))
		{
			ShakmehBossData data = GameRoot.Instance.ShakmehBossDataContainer.GetData(ChapterIndex, DungeonIndex);
			NGame2.NUI.NManager.ShakmehDungeonManagement.ShakmehNpcActionState currShakmehNpcActionState = instance.CurrShakmehNpcActionState;
			ShakmehBossType bossType = data.BossType;
			if (NodeDisableData.ShakmehNode != null)
			{
				UIHelper.EnableGameObject(NodeDisableData.ShakmehNode, enable: true);
			}
			switch (currShakmehNpcActionState)
			{
			case NGame2.NUI.NManager.ShakmehDungeonManagement.ShakmehNpcActionState.Idle_Middle:
			case NGame2.NUI.NManager.ShakmehDungeonManagement.ShakmehNpcActionState.Idle_Final:
				SetNodeDisableDirect(bossType);
				break;
			case NGame2.NUI.NManager.ShakmehDungeonManagement.ShakmehNpcActionState.Appear_Middle:
			case NGame2.NUI.NManager.ShakmehDungeonManagement.ShakmehNpcActionState.Appear_Final:
				StartNodeDisable(currShakmehNpcActionState, bossType);
				break;
			}
		}
	}

	private void Update()
	{
		if (_pressed && (DateTime.UtcNow - _pressedTime).TotalSeconds >= 0.5)
		{
			_pressed = false;
			Vespa_System_Action_Extension_Method.fire(OnLongPressAction, this);
		}
	}

	public void OnClick()
	{
		_pressed = false;
		ActivityManager.RequestStop();
		Vespa_System_Action_Extension_Method.fire(OnTouchAction, this);
	}

	public void OnDrag(Vector2 delta)
	{
		Vespa_System_Action_Extension_Method.fire(OnDragAction, this, delta);
	}

	public void OnDragStart()
	{
		_pressed = false;
		Vespa_System_Action_Extension_Method.fire(OnDragStartAction, this);
	}

	public void OnDragEnd()
	{
		Vespa_System_Action_Extension_Method.fire(OnDragEndAction, this);
	}

	public void OnPress(bool state)
	{
		_pressed = state;
		_pressedTime = DateTime.UtcNow;
	}

	public float GetNextEffectStartTime()
	{
		if (EffectTarget == null || AlphaCurve == null || ScaleCurve == null)
		{
			return 0f;
		}
		if (!(EffectDuration > EffectEndOverlap))
		{
			return 0f;
		}
		return EffectDuration - EffectEndOverlap;
	}

	public IEnumerator coPlayEffect()
	{
		UIHelper.EnableGameObject(this, enable: true);
		if (EffectTarget == null || AlphaCurve == null || ScaleCurve == null)
		{
			yield break;
		}
		EffectTarget.alpha = AlphaCurve.Evaluate(0f);
		EffectTarget.transform.localScale = Vector3.one * ScaleCurve.Evaluate(0f);
		float _effectTime = 0f;
		while (_effectTime < EffectDuration)
		{
			_effectTime += GlobalTimer.deltaTime;
			float time = Mathf.Clamp01(_effectTime / EffectDuration);
			float alpha = AlphaCurve.Evaluate(time);
			float num = ScaleCurve.Evaluate(time);
			EffectTarget.alpha = alpha;
			EffectTarget.transform.localScale = Vector3.one * num;
			yield return null;
			if (EffectTarget == null || AlphaCurve == null || ScaleCurve == null)
			{
				yield break;
			}
		}
		if (!(EffectTarget == null) && AlphaCurve != null && ScaleCurve != null)
		{
			EffectTarget.alpha = AlphaCurve.Evaluate(1f);
			EffectTarget.transform.localScale = Vector3.one * ScaleCurve.Evaluate(1f);
		}
	}

	public IEnumerator coReverseEffect(bool hide)
	{
		yield return new WaitForSeconds(10f);
		UIHelper.EnableGameObject(this, enable: true);
		if (EffectTarget == null || AlphaCurve == null || ScaleCurve == null)
		{
			yield break;
		}
		EffectTarget.alpha = AlphaCurve.Evaluate(1f);
		EffectTarget.transform.localScale = Vector3.one * ScaleCurve.Evaluate(1f);
		float _effectTime = 0f;
		while (_effectTime < EffectDuration)
		{
			_effectTime += GlobalTimer.deltaTime;
			float time = EffectDuration - Mathf.Clamp01(_effectTime / EffectDuration);
			float alpha = AlphaCurve.Evaluate(time);
			float num = ScaleCurve.Evaluate(time);
			EffectTarget.alpha = alpha;
			EffectTarget.transform.localScale = Vector3.one * num;
			yield return null;
			if (EffectTarget == null || AlphaCurve == null || ScaleCurve == null)
			{
				yield break;
			}
		}
		if (!(EffectTarget == null) && AlphaCurve != null && ScaleCurve != null)
		{
			EffectTarget.alpha = AlphaCurve.Evaluate(0f);
			EffectTarget.transform.localScale = Vector3.one * ScaleCurve.Evaluate(0f);
			UIHelper.EnableGameObject(this, !hide);
		}
	}

	public bool IsEffectActivated()
	{
		if (EffectTarget == null || AlphaCurve == null || ScaleCurve == null)
		{
			return true;
		}
		if (UIHelper.IsEnabledGameObjet(EffectTarget))
		{
			float num = GameRoot.Instance.ConstantDataContainer.GetValueInt("EffectActivationAlpha");
			num /= 1000f;
			if (EffectTarget.alpha > num)
			{
				return true;
			}
		}
		return false;
	}

	private void StartNodeDisable(NGame2.NUI.NManager.ShakmehDungeonManagement.ShakmehNpcActionState shakmehNpcActionState, ShakmehBossType nodeBossType)
	{
		if (NodeDisableData.CurrEffectState != NodeDisableInfo.EffectState.Idle || NodeDisableData.ShakmehNode == null)
		{
			return;
		}
		NGame2.NUI.NManager.ShakmehDungeonManagement instance = SceneAutoBehaviour<NGame2.NUI.NManager.ShakmehDungeonManagement>.instance;
		if (instance == null)
		{
			return;
		}
		float shakmehNodeEffectDelay = instance.ShakmehNodeEffectDelay;
		switch (shakmehNpcActionState)
		{
		case NGame2.NUI.NManager.ShakmehDungeonManagement.ShakmehNpcActionState.Appear_Middle:
			switch (nodeBossType)
			{
			case ShakmehBossType.Middle:
				VespaCoroutine.Start(coNodeDisable(NodeDisableInfo.EffectState.PlayReverse, shakmehNodeEffectDelay));
				break;
			case ShakmehBossType.Final:
			{
				VespaCoroutine.Start(coNodeDisable(NodeDisableInfo.EffectState.PlayForward, 0f));
				MapLayout instance3 = SceneAutoBehaviour<WorldMapManagement>.instance.GetInstance<MapLayout>();
				if (!(instance3 == null))
				{
					instance3.PropManager?.ForceEnableProbComponent(ChapterIndex, DungeonIndex, isEnable: false);
				}
				break;
			}
			}
			break;
		case NGame2.NUI.NManager.ShakmehDungeonManagement.ShakmehNpcActionState.Appear_Final:
			switch (nodeBossType)
			{
			case ShakmehBossType.Middle:
				VespaCoroutine.Start(coNodeDisable(NodeDisableInfo.EffectState.PlayForward, 0f));
				break;
			case ShakmehBossType.Final:
			{
				VespaCoroutine.Start(coNodeDisable(NodeDisableInfo.EffectState.PlayReverse, shakmehNodeEffectDelay));
				MapLayout instance2 = SceneAutoBehaviour<WorldMapManagement>.instance.GetInstance<MapLayout>();
				if (!(instance2 == null))
				{
					instance2.PropManager?.ForceEnableProbComponent(ChapterIndex, DungeonIndex, isEnable: true, shakmehNodeEffectDelay);
				}
				break;
			}
			}
			break;
		}
		instance.SetShakmehNpcActionState();
	}

	private void SetNodeDisableDirect(ShakmehBossType thisNodeBossType)
	{
		if (NodeDisableData.ShakmehNode == null)
		{
			return;
		}
		NGame2.NUI.NManager.ShakmehDungeonManagement instance = SceneAutoBehaviour<NGame2.NUI.NManager.ShakmehDungeonManagement>.instance;
		if (instance == null)
		{
			return;
		}
		UISprite shakmehNode = NodeDisableData.ShakmehNode;
		AnimationCurve alphaCurve = NodeDisableData.AlphaCurve;
		_ = NodeDisableData.EffectDuration;
		if (!instance.IsEnoughShakmehBossGauge)
		{
			switch (thisNodeBossType)
			{
			case ShakmehBossType.Middle:
				shakmehNode.alpha = alphaCurve.Evaluate(0f);
				break;
			case ShakmehBossType.Final:
			{
				shakmehNode.alpha = alphaCurve.Evaluate(1f);
				MapLayout instance2 = SceneAutoBehaviour<WorldMapManagement>.instance.GetInstance<MapLayout>();
				if (instance2 == null)
				{
					return;
				}
				PropManager propManager = instance2.PropManager;
				if (propManager == null)
				{
					return;
				}
				propManager.ForceEnableProbComponent(ChapterIndex, DungeonIndex, isEnable: false);
				break;
			}
			}
		}
		else
		{
			switch (thisNodeBossType)
			{
			case ShakmehBossType.Middle:
				shakmehNode.alpha = alphaCurve.Evaluate(1f);
				break;
			case ShakmehBossType.Final:
			{
				shakmehNode.alpha = alphaCurve.Evaluate(0f);
				MapLayout instance3 = SceneAutoBehaviour<WorldMapManagement>.instance.GetInstance<MapLayout>();
				if (instance3 == null)
				{
					return;
				}
				PropManager propManager2 = instance3.PropManager;
				if (propManager2 == null)
				{
					return;
				}
				propManager2.ForceEnableProbComponent(ChapterIndex, DungeonIndex, isEnable: true);
				break;
			}
			}
		}
		instance.SetShakmehNpcActionState();
	}

	private IEnumerator coNodeDisable(NodeDisableInfo.EffectState effectState, float delay)
	{
		NodeDisableData.CurrEffectState = effectState;
		UISprite targetSprite = NodeDisableData.ShakmehNode;
		AnimationCurve alphaCurve = NodeDisableData.AlphaCurve;
		float effectDuration = NodeDisableData.EffectDuration;
		float elapTime = 0f;
		switch (effectState)
		{
		case NodeDisableInfo.EffectState.PlayForward:
			targetSprite.alpha = alphaCurve.Evaluate(0f);
			break;
		case NodeDisableInfo.EffectState.PlayReverse:
			targetSprite.alpha = alphaCurve.Evaluate(1f);
			break;
		}
		UIHelper.EnableGameObject(targetSprite, enable: true);
		while (GlobalAutoBehaviour<LoadingManager>.instance.IsSceneLoading)
		{
			yield return null;
		}
		yield return new WaitForSeconds(delay);
		NodeDisableInfo.EffectState effectState2 = effectState;
		if (effectState2 != NodeDisableInfo.EffectState.PlayForward)
		{
			if (effectState2 == NodeDisableInfo.EffectState.PlayReverse)
			{
				while (elapTime < effectDuration)
				{
					targetSprite.alpha = alphaCurve.Evaluate((effectDuration - elapTime) / effectDuration);
					elapTime += Time.deltaTime;
					yield return null;
				}
				targetSprite.alpha = alphaCurve.Evaluate(0f);
			}
		}
		else
		{
			while (elapTime < effectDuration)
			{
				targetSprite.alpha = alphaCurve.Evaluate(elapTime / effectDuration);
				elapTime += Time.deltaTime;
				yield return null;
			}
			targetSprite.alpha = alphaCurve.Evaluate(1f);
		}
		NodeDisableData.CurrEffectState = NodeDisableInfo.EffectState.Idle;
	}
}
