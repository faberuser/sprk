using System;
using NGame2.NAccount;
using NGame2.NAchievement;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

[RequireComponent(typeof(UIWidget))]
public class MissionNodeComponent : CommonNode
{
	public enum ENodeType
	{
		Node,
		StartNode,
		MainCard,
		SubCard,
		Node3
	}

	public const string PrefabPath = "Prefab2/Component/QuestIntegration/MissionNodeComponent";

	public const string PrefabPath2 = "Prefab2/Component/QuestIntegration/MissionNode2Component";

	public const string PrefabPath3 = "Prefab2/Component/QuestIntegration/MissionNode2SubComponent";

	public const string PrefabPath4 = "Prefab2/Component/QuestIntegration/MissionNodeStartComponent";

	public const string PrefabPath5 = "Prefab2/Component/QuestIntegration/MissionNode3Component";

	public static PrefabAuto<MissionNodeComponent> PrefabAuto = new PrefabAuto<MissionNodeComponent>("Prefab2/Component/QuestIntegration/MissionNodeComponent");

	public static PrefabAuto<MissionNodeComponent> PrefabAuto2 = new PrefabAuto<MissionNodeComponent>("Prefab2/Component/QuestIntegration/MissionNode2Component");

	public static PrefabAuto<MissionNodeComponent> PrefabAuto3 = new PrefabAuto<MissionNodeComponent>("Prefab2/Component/QuestIntegration/MissionNode2SubComponent");

	public static PrefabAuto<MissionNodeComponent> PrefabAuto4 = new PrefabAuto<MissionNodeComponent>("Prefab2/Component/QuestIntegration/MissionNodeStartComponent");

	public static PrefabAuto<MissionNodeComponent> PrefabAuto5 = new PrefabAuto<MissionNodeComponent>("Prefab2/Component/QuestIntegration/MissionNode3Component");

	private const string ANIMATIONSTATE_NAME_OPEN = "Anim_MissionNode_Open";

	private const string ANIMATIONSTATE_NAME_BACK = "Anim_MissionNode_Back";

	private const string ANIMATIONSTATE_NAME_LOOP = "Anim_MissionNode_Loop";

	private const string ANIMATIONSTATE_NAME_CLEAR = "Anim_MissionNode_Clear";

	private const string ANIMATIONSTATE_NAME_CLEAR_LOOP = "Anim_MissionNode_ClearLoop";

	[Header("[MissionNodeComponent]")]
	public UISprite SpriteLinkedLine;

	public UISprite SpriteLinkedLineOff;

	public UILabel LabelNumber;

	public UILabel LabelGoal;

	public Animator Animator;

	private Vector2 componentSize;

	private int mainStep;

	private int subStep;

	private Vector2 position;

	private Vector2 scale;

	private ENodeType NodeType;

	public float LinkedLineThickness = 6f;

	public float LinkedLineOffThickness = 6f;

	private ENodeState PrevState;

	public override void Open(params object[] args)
	{
		base.Open(args);
		if (args != null && args.Length != 0)
		{
			MissionComponentData data = args[0] as MissionComponentData;
			IListner listner = args[1] as IListner;
			int num = Convert.ToInt32(args[2]);
			int num2 = Convert.ToInt32(args[3]);
			Vector2 vector = (Vector2)args[4];
			Vector2 vector2 = (Vector2)args[5];
			ENodeType nodeType = (ENodeType)args[6];
			mainStep = num;
			subStep = num2;
			position = vector;
			scale = vector2;
			NodeType = nodeType;
			Open(data, listner);
		}
	}

	public override void Init()
	{
		base.Init();
		componentSize = GetComponent<UIWidget>().localSize;
		SetPosition();
		SetDrawInfos();
	}

	public override void Clear()
	{
		base.Clear();
		if (SpriteLinkedLine != null)
		{
			SpriteLinkedLine.SetRect(0f, 0f, 10f, 10f);
			UIHelper.EnableGameObject(SpriteLinkedLine, enable: false);
		}
		if (SpriteLinkedLineOff != null)
		{
			SpriteLinkedLineOff.SetRect(0f, 0f, 10f, 10f);
			UIHelper.EnableGameObject(SpriteLinkedLineOff, enable: false);
		}
		mainStep = (subStep = 0);
		position = Vector2.zero;
		scale = Vector2.one;
		PrevState = ENodeState.Null;
	}

	public override void Refresh()
	{
		base.Refresh();
		SetDrawInfos();
	}

	private void SetGrayScale(bool enable)
	{
		if (UseGrayScale && NodeSprite != null)
		{
			NodeSprite.isGrayscale = enable;
		}
	}

	protected override void ProgressState()
	{
		base.ProgressState();
		SetGrayScale(enable: false);
		UIHelper.EnableGameObject(LockedObject, enable: false);
		UIHelper.EnableGameObject(DisableObject, enable: false);
		UIHelper.EnableGameObject(SelectedObject, enable: true);
		UIHelper.EnableGameObject(SpriteReward, enable: false);
		UIHelper.EnableGameObject(CompleteObject, enable: false);
	}

	protected override void QuestCompleteState()
	{
		base.QuestCompleteState();
		SetGrayScale(enable: false);
		UIHelper.EnableGameObject(LockedObject, enable: false);
		UIHelper.EnableGameObject(DisableObject, enable: true);
		UIHelper.EnableGameObject(SelectedObject, enable: false);
		UIHelper.EnableGameObject(CompleteObject, enable: true);
		UIHelper.EnableGameObject(SpriteReward, enable: false);
	}

	protected override void RewardState()
	{
		base.RewardState();
		SetGrayScale(enable: false);
		UIHelper.EnableGameObject(LockedObject, enable: false);
		UIHelper.EnableGameObject(SelectedObject, enable: false);
		UIHelper.EnableGameObject(DisableObject, enable: false);
		UIHelper.EnableGameObject(SpriteReward, enable: true);
		UIHelper.EnableGameObject(CompleteObject, enable: false);
	}

	protected override void LockedState()
	{
		base.LockedState();
		SetGrayScale(enable: true);
		UIHelper.EnableGameObject(DisableObject, enable: false);
		UIHelper.EnableGameObject(LockedObject, enable: true);
		UIHelper.EnableGameObject(SelectedObject, enable: false);
		UIHelper.EnableGameObject(SpriteReward, enable: false);
		UIHelper.EnableGameObject(CompleteObject, enable: false);
	}

	protected override void NodeProgressState()
	{
		base.NodeProgressState();
		SetGrayScale(enable: false);
		UIHelper.EnableGameObject(LockedObject, enable: false);
		UIHelper.EnableGameObject(DisableObject, enable: false);
		UIHelper.EnableGameObject(SelectedObject, enable: true);
		UIHelper.EnableGameObject(SpriteReward, enable: false);
		UIHelper.EnableGameObject(CompleteObject, enable: false);
	}

	protected override void NodeCompleteState()
	{
		base.NodeCompleteState();
		SetGrayScale(enable: false);
		UIHelper.EnableGameObject(LockedObject, enable: false);
		UIHelper.EnableGameObject(DisableObject, enable: false);
		UIHelper.EnableGameObject(SelectedObject, enable: false);
		UIHelper.EnableGameObject(CompleteObject, enable: true);
		UIHelper.EnableGameObject(SpriteReward, enable: false);
	}

	protected override void NodeRewardState()
	{
		base.NodeRewardState();
		SetGrayScale(enable: false);
		UIHelper.EnableGameObject(LockedObject, enable: false);
		UIHelper.EnableGameObject(SelectedObject, enable: false);
		UIHelper.EnableGameObject(DisableObject, enable: false);
		UIHelper.EnableGameObject(SpriteReward, enable: true);
		UIHelper.EnableGameObject(CompleteObject, enable: false);
	}

	protected override void NodeLockedState()
	{
		base.NodeLockedState();
		SetGrayScale(enable: true);
		UIHelper.EnableGameObject(DisableObject, enable: true);
		UIHelper.EnableGameObject(LockedObject, enable: false);
		UIHelper.EnableGameObject(SelectedObject, enable: false);
		UIHelper.EnableGameObject(SpriteReward, enable: false);
		UIHelper.EnableGameObject(CompleteObject, enable: false);
	}

	protected override void Node3ProgressState()
	{
		base.Node3ProgressState();
		NodeProgressState();
	}

	protected override void Node3CompleteState()
	{
		base.Node3CompleteState();
		SetRewardFirstItemGrayScale(grayScale: true);
		NodeCompleteState();
	}

	protected override void Node3RewardState()
	{
		base.Node3RewardState();
		NodeRewardState();
	}

	protected override void Node3LockedState()
	{
		base.Node3LockedState();
		NodeLockedState();
	}

	protected override void OnClick()
	{
		base.OnClick();
		if (eNodeState != ENodeState.Lock && NodeType != ENodeType.StartNode && iListner != null)
		{
			iListner.OnClick(base.ElementData);
		}
	}

	private void SetPosition()
	{
		base.transform.localPosition = position;
		base.transform.localScale = scale;
	}

	private void SetDrawInfos()
	{
		PrevState = eNodeState;
		switch (NodeType)
		{
		case ENodeType.StartNode:
			SetupStartNode();
			break;
		case ENodeType.Node:
			SetupNode();
			break;
		case ENodeType.MainCard:
			SetupMainCardNode();
			break;
		case ENodeType.SubCard:
			SetupSubCardNode();
			break;
		case ENodeType.Node3:
			SetupNode3();
			break;
		}
		PlayAnimation();
	}

	private void PlayAnimation()
	{
		switch (PrevState)
		{
		case ENodeState.Null:
			if (eNodeState == ENodeState.Progress)
			{
				Animator.Play("Anim_MissionNode_Open");
			}
			else if (eNodeState == ENodeState.Lock)
			{
				Animator.Play("Anim_MissionNode_Back");
			}
			else if (eNodeState == ENodeState.Reward)
			{
				Animator.Play("Anim_MissionNode_Loop");
			}
			else if (eNodeState == ENodeState.QuestComplete)
			{
				Animator.Play("Anim_MissionNode_ClearLoop");
			}
			break;
		case ENodeState.Lock:
			if (eNodeState == ENodeState.Progress)
			{
				Animator.Play("Anim_MissionNode_Open");
			}
			else if (eNodeState == ENodeState.Lock)
			{
				Animator.Play("Anim_MissionNode_Back");
			}
			else if (eNodeState == ENodeState.Reward)
			{
				Animator.Play("Anim_MissionNode_Open");
			}
			else if (eNodeState == ENodeState.QuestComplete)
			{
				Animator.Play("Anim_MissionNode_Clear");
			}
			break;
		case ENodeState.Progress:
			if (eNodeState == ENodeState.Progress)
			{
				Animator.Play("Anim_MissionNode_Loop");
			}
			else if (eNodeState == ENodeState.Lock)
			{
				Animator.Play("Anim_MissionNode_Back");
			}
			else if (eNodeState == ENodeState.Reward)
			{
				Animator.Play("Anim_MissionNode_Loop");
			}
			else if (eNodeState == ENodeState.QuestComplete)
			{
				Animator.Play("Anim_MissionNode_ClearLoop");
			}
			break;
		case ENodeState.Reward:
			if (eNodeState == ENodeState.Progress)
			{
				Animator.Play("Anim_MissionNode_Loop");
			}
			else if (eNodeState == ENodeState.Lock)
			{
				Animator.Play("Anim_MissionNode_Back");
			}
			else if (eNodeState == ENodeState.Reward)
			{
				Animator.Play("Anim_MissionNode_Loop");
			}
			else if (eNodeState == ENodeState.QuestComplete)
			{
				Animator.Play("Anim_MissionNode_Clear");
			}
			break;
		case ENodeState.QuestComplete:
			if (eNodeState == ENodeState.Progress)
			{
				Animator.Play("Anim_MissionNode_Loop");
			}
			else if (eNodeState == ENodeState.Lock)
			{
				Animator.Play("Anim_MissionNode_Back");
			}
			else if (eNodeState == ENodeState.Reward)
			{
				Animator.Play("Anim_MissionNode_Loop");
			}
			else if (eNodeState == ENodeState.QuestComplete)
			{
				Animator.Play("Anim_MissionNode_ClearLoop");
			}
			break;
		}
	}

	private void SetupStartNode()
	{
		if (base.ElementData != null && base.ElementData.VisualData != null)
		{
			UIHelper.SetLabelText(TitleLabel, LocalizationManager.GetStringDirect(base.ElementData.VisualData.NodeIconLocalKey));
			if (NodeSprite != null && !String_Extension_Method.IsNullOrEmpty(base.ElementData.VisualData.NodeIconPath))
			{
				NodeSprite.spriteName = base.ElementData.VisualData.NodeIconPath;
			}
		}
	}

	private void SetupNode()
	{
		if (base.ElementData == null)
		{
			return;
		}
		if (base.ElementData.VisualData != null)
		{
			UIHelper.SetLabelText(LabelNumber, base.ElementData.VisualData.SubStep);
		}
		if (base.ElementData.Data == null)
		{
			return;
		}
		UIHelper.SetLabelText(TitleLabel, base.ElementData.Title);
		if (base.ElementData.GetData<AchievementData>() != null)
		{
			AchievementInfo achievementInfo = GlobalAutoBehaviour<AccountManager>.instance.AchievementManager.Get(base.ElementData.Index);
			bool flag = AchievementChecker.Cleared(base.ElementData.Index);
			bool flag2 = AchievementChecker.Completed(base.ElementData.Index);
			if (achievementInfo == null)
			{
				SetState(ENodeState.NodeLock);
			}
			else
			{
				SetState(flag2 ? ENodeState.NodeComplete : (flag ? ENodeState.NodeReward : ENodeState.NodeProgress));
			}
		}
		else if (base.ElementData.GetData<SubQuestData>() != null)
		{
			switch (GlobalAutoBehaviour<AccountManager>.instance.QuestManager.GetSubQuestStatus(base.ElementData.GetData<SubQuestData>()))
			{
			case QuesetStatusType.Complete:
				SetState(ENodeState.NodeComplete);
				break;
			case QuesetStatusType.Lock:
				SetState(ENodeState.NodeLock);
				break;
			case QuesetStatusType.Progress:
				SetState(ENodeState.NodeProgress);
				break;
			case QuesetStatusType.Reward:
				SetState(ENodeState.NodeReward);
				break;
			default:
				SetState(ENodeState.NodeLock);
				break;
			}
		}
		else
		{
			VespaDebug.LogError("MissionNodeComp Data is Null. " + base.ElementData.DataType.ToString());
		}
	}

	private void SetupNode3()
	{
		if (base.ElementData == null || base.ElementData.Data == null)
		{
			return;
		}
		UIHelper.SetLabelText(TitleLabel, base.ElementData.Title);
		SetupNode3Reward();
		if (base.ElementData.GetData<AchievementData>() != null)
		{
			AchievementInfo achievementInfo = GlobalAutoBehaviour<AccountManager>.instance.AchievementManager.Get(base.ElementData.Index);
			bool flag = AchievementChecker.Cleared(base.ElementData.Index);
			bool flag2 = AchievementChecker.Completed(base.ElementData.Index);
			if (achievementInfo == null)
			{
				SetState(ENodeState.Node3Lock);
			}
			else
			{
				SetState(flag2 ? ENodeState.Node3Complete : (flag ? ENodeState.Node3Reward : ENodeState.Node3Progress), flag2);
			}
		}
		else if (base.ElementData.GetData<SubQuestData>() != null)
		{
			switch (GlobalAutoBehaviour<AccountManager>.instance.QuestManager.GetSubQuestStatus(base.ElementData.GetData<SubQuestData>()))
			{
			case QuesetStatusType.Complete:
				SetState(ENodeState.Node3Complete, forceDoState: true);
				break;
			case QuesetStatusType.Lock:
				SetState(ENodeState.Node3Lock);
				break;
			case QuesetStatusType.Progress:
				SetState(ENodeState.Node3Progress);
				break;
			case QuesetStatusType.Reward:
				SetState(ENodeState.Node3Reward);
				break;
			default:
				SetState(ENodeState.Node3Lock);
				break;
			}
		}
		else
		{
			VespaDebug.LogError("MissionNode3Comp Data is Null. " + base.ElementData.DataType.ToString());
		}
	}

	private void SetupMainCardNode()
	{
		if (base.ElementData == null || base.ElementData.Data == null)
		{
			return;
		}
		UIHelper.SetLabelText(TitleLabel, base.ElementData.Title);
		UIHelper.SetLabelText(LabelGoal, base.ElementData.GoalRequirement);
		if (base.ElementData.GetData<AchievementData>() != null)
		{
			AchievementInfo achievementInfo = GlobalAutoBehaviour<AccountManager>.instance.AchievementManager.Get(base.ElementData.Index);
			bool flag = AchievementChecker.Cleared(base.ElementData.Index);
			bool flag2 = AchievementChecker.Completed(base.ElementData.Index);
			if (achievementInfo == null)
			{
				SetState(ENodeState.Lock);
			}
			else
			{
				SetState(flag2 ? ENodeState.QuestComplete : ((!flag) ? ENodeState.Progress : ENodeState.Reward));
			}
		}
		else if (base.ElementData.GetData<SubQuestData>() != null)
		{
			switch (GlobalAutoBehaviour<AccountManager>.instance.QuestManager.GetSubQuestStatus(base.ElementData.GetData<SubQuestData>()))
			{
			case QuesetStatusType.Complete:
				SetState(ENodeState.QuestComplete);
				break;
			case QuesetStatusType.Lock:
				SetState(ENodeState.Lock);
				break;
			case QuesetStatusType.Progress:
				SetState(ENodeState.Progress);
				break;
			case QuesetStatusType.Reward:
				SetState(ENodeState.Reward);
				break;
			default:
				SetState(ENodeState.Lock);
				break;
			}
		}
		else
		{
			VespaDebug.LogError("MissionNodeComp Data is Null. " + base.ElementData.DataType.ToString());
		}
		SetupRewardList();
	}

	private void SetupSubCardNode()
	{
		if (base.ElementData == null || base.ElementData.Data == null)
		{
			return;
		}
		UIHelper.SetLabelText(TitleLabel, base.ElementData.Title);
		if (base.ElementData.GetData<AchievementData>() != null)
		{
			AchievementInfo achievementInfo = GlobalAutoBehaviour<AccountManager>.instance.AchievementManager.Get(base.ElementData.Index);
			bool flag = AchievementChecker.Cleared(base.ElementData.Index);
			bool flag2 = AchievementChecker.Completed(base.ElementData.Index);
			if (achievementInfo == null)
			{
				SetState(ENodeState.Lock);
			}
			else
			{
				SetState(flag2 ? ENodeState.QuestComplete : ((!flag) ? ENodeState.Progress : ENodeState.Reward));
			}
		}
		else if (base.ElementData.GetData<SubQuestData>() != null)
		{
			switch (GlobalAutoBehaviour<AccountManager>.instance.QuestManager.GetSubQuestStatus(base.ElementData.GetData<SubQuestData>()))
			{
			case QuesetStatusType.Complete:
				SetState(ENodeState.QuestComplete);
				break;
			case QuesetStatusType.Lock:
				SetState(ENodeState.Lock);
				break;
			case QuesetStatusType.Progress:
				SetState(ENodeState.Progress);
				break;
			case QuesetStatusType.Reward:
				SetState(ENodeState.Reward);
				break;
			default:
				SetState(ENodeState.Lock);
				break;
			}
		}
		else
		{
			VespaDebug.LogError("MissionNodeComp Data is Null. " + base.ElementData.DataType.ToString());
		}
		SetupRewardList();
	}

	public Vector2 GetSize()
	{
		return componentSize;
	}

	public Vector2 GetPosition()
	{
		return position;
	}

	public bool IsRewardAble()
	{
		return UIHelper.IsEnabledGameObjet(SpriteReward);
	}

	public bool IsProgress()
	{
		if (eNodeState != ENodeState.Progress && eNodeState != ENodeState.NodeProgress)
		{
			return eNodeState == ENodeState.Node3Progress;
		}
		return true;
	}

	public void DrawLinkedLine(Vector2 otherNode)
	{
		bool flag = IsLockState();
		UIHelper.EnableGameObject(SpriteLinkedLine, !flag);
		UIHelper.EnableGameObject(SpriteLinkedLineOff, flag);
		if (flag)
		{
			DrawLinkedLine(otherNode, LinkedLineOffThickness, ref SpriteLinkedLineOff);
		}
		else
		{
			DrawLinkedLine(otherNode, LinkedLineThickness, ref SpriteLinkedLine);
		}
	}

	private void DrawLinkedLine(Vector2 otherNode, float thickness, ref UISprite linkedLine)
	{
		if (!(linkedLine == null))
		{
			Vector2 vector = position;
			Vector3 vector2 = otherNode - vector;
			Vector2 vector3 = new Vector2(vector2.magnitude, thickness);
			linkedLine.pivot = UIWidget.Pivot.Left;
			Vector2 vector4 = vector;
			linkedLine.SetRect(vector4.x, vector4.y, vector3.x, vector3.y);
			float z = Mathf.Atan2(vector2.y, vector2.x) * 57.29578f;
			linkedLine.transform.localRotation = Quaternion.Euler(0f, 0f, z);
			linkedLine.transform.localPosition = Vector3.zero;
			linkedLine.transform.localScale = Vector3.one * (1f / scale.x);
			UIHelper.EnableGameObject(linkedLine, enable: true);
		}
	}
}
