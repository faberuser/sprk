using System.Collections.Generic;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class CommonNode : MonoBehaviour, InitOnCreate, ItemIconComponent.Listener, CurrencyIconComponent.Listener
{
	public enum ENodeState
	{
		Null,
		Progress,
		Lock,
		QuestComplete,
		Reward,
		NodeProgress,
		NodeLock,
		NodeComplete,
		NodeReward,
		Node3Progress,
		Node3Lock,
		Node3Complete,
		Node3Reward
	}

	public interface IListner
	{
		void OnClick(MissionComponentData data);
	}

	[Header("[CommonNode]")]
	public UILabel TitleLabel;

	public UISprite NodeSprite;

	public GameObject DisableObject;

	public GameObject SelectedObject;

	public GameObject LockedObject;

	public GameObject CompleteObject;

	public UISprite SpriteNew;

	public UISprite SpriteReward;

	public bool UseGrayScale;

	public UISimpleGrid RewardListGrid;

	protected List<ItemIconComponent> _rewardItemIconComponentList = new List<ItemIconComponent>();

	protected List<CurrencyIconComponent> _rewardCurrencyIconComponentList = new List<CurrencyIconComponent>();

	protected ENodeState eNodeState;

	protected IListner iListner;

	public MissionComponentData ElementData { get; set; }

	void InitOnCreate.OnCreate()
	{
		Clear();
	}

	void InitOnCreate.OnRelease()
	{
		Clear();
	}

	public virtual void Open(params object[] args)
	{
	}

	public virtual void Open(MissionComponentData data, IListner listner)
	{
		ElementData = data;
		iListner = listner;
		Init();
	}

	public virtual void Init()
	{
	}

	public virtual void Refresh()
	{
	}

	public virtual void Clear()
	{
		FactoryBase.ReleaseAndClearList(_rewardCurrencyIconComponentList);
		FactoryBase.ReleaseAndClearList(_rewardItemIconComponentList);
		eNodeState = ENodeState.Null;
	}

	protected void SetTitleLabel(string text)
	{
		if (!(TitleLabel == null))
		{
			UIHelper.SetLabelText(TitleLabel, text);
		}
	}

	protected void SetState(ENodeState state, bool forceDoState = false)
	{
		if (eNodeState != state || forceDoState)
		{
			eNodeState = state;
			DoState();
		}
	}

	protected void DoState()
	{
		switch (eNodeState)
		{
		case ENodeState.Progress:
			ProgressState();
			break;
		case ENodeState.QuestComplete:
			QuestCompleteState();
			break;
		case ENodeState.Reward:
			RewardState();
			break;
		case ENodeState.Lock:
			LockedState();
			break;
		case ENodeState.NodeProgress:
			NodeProgressState();
			break;
		case ENodeState.NodeComplete:
			NodeCompleteState();
			break;
		case ENodeState.NodeLock:
			NodeLockedState();
			break;
		case ENodeState.NodeReward:
			NodeRewardState();
			break;
		case ENodeState.Node3Progress:
			Node3ProgressState();
			break;
		case ENodeState.Node3Complete:
			Node3CompleteState();
			break;
		case ENodeState.Node3Lock:
			Node3LockedState();
			break;
		case ENodeState.Node3Reward:
			Node3RewardState();
			break;
		}
	}

	protected virtual void ProgressState()
	{
	}

	protected virtual void QuestCompleteState()
	{
	}

	protected virtual void RewardState()
	{
	}

	protected virtual void LockedState()
	{
	}

	protected virtual void NodeProgressState()
	{
	}

	protected virtual void NodeCompleteState()
	{
	}

	protected virtual void NodeRewardState()
	{
	}

	protected virtual void NodeLockedState()
	{
	}

	protected virtual void Node3ProgressState()
	{
	}

	protected virtual void Node3CompleteState()
	{
	}

	protected virtual void Node3RewardState()
	{
	}

	protected virtual void Node3LockedState()
	{
	}

	protected virtual void OnClick()
	{
	}

	protected bool IsLockState()
	{
		if (eNodeState != ENodeState.Lock && eNodeState != ENodeState.NodeLock)
		{
			return eNodeState == ENodeState.Node3Lock;
		}
		return true;
	}

	protected void SetupNode3Reward()
	{
		if (ElementData != null)
		{
			if (ElementData.GetData<AchievementData>() != null)
			{
				SetupAchievementReward(showSingleReward: true);
			}
			else if (ElementData.GetData<SubQuestData>() != null)
			{
				SetupSubQuestReward(showSingleReward: true);
			}
			if (__Extension_Method_ICollection__.COUNT(_rewardItemIconComponentList) > 0)
			{
				_rewardItemIconComponentList[0].SetListener(this);
			}
			else if (__Extension_Method_ICollection__.COUNT(_rewardCurrencyIconComponentList) > 0)
			{
				_rewardCurrencyIconComponentList[0].SetListener(this);
			}
			UIHelper.EnableGameObject(RewardListGrid.gameObject, RewardListGrid != null && (__Extension_Method_ICollection__.COUNT(_rewardItemIconComponentList) > 0 || __Extension_Method_ICollection__.COUNT(_rewardCurrencyIconComponentList) > 0));
		}
	}

	protected void SetupRewardList()
	{
		if (ElementData == null)
		{
			return;
		}
		if (IsLockState())
		{
			UIHelper.EnableGameObject(RewardListGrid.gameObject, enable: false);
			return;
		}
		if (ElementData.GetData<AchievementData>() != null)
		{
			SetupAchievementReward();
		}
		else if (ElementData.GetData<SubQuestData>() != null)
		{
			SetupSubQuestReward();
		}
		UIHelper.EnableGameObject(RewardListGrid.gameObject, RewardListGrid != null && (__Extension_Method_ICollection__.COUNT(_rewardItemIconComponentList) > 0 || __Extension_Method_ICollection__.COUNT(_rewardCurrencyIconComponentList) > 0));
	}

	private void SetupAchievementReward(bool showSingleReward = false)
	{
		if (ElementData == null)
		{
			return;
		}
		AchievementData data = ElementData.GetData<AchievementData>();
		if (data != null)
		{
			FactoryBase.ReleaseAndClearList(_rewardCurrencyIconComponentList);
			FactoryBase.ReleaseAndClearList(_rewardItemIconComponentList);
			if (showSingleReward)
			{
				CommonMission.SetupNodeIconAchievementReward(data, ref _rewardItemIconComponentList, ref _rewardCurrencyIconComponentList, RewardListGrid, Color.green);
			}
			else
			{
				CommonMission.SetupAchievementReward(data, ref _rewardItemIconComponentList, ref _rewardCurrencyIconComponentList, RewardListGrid, Color.green);
			}
			UIHelper.RepositionGrid(RewardListGrid);
		}
	}

	private void SetupSubQuestReward(bool showSingleReward = false)
	{
		if (ElementData == null)
		{
			return;
		}
		SubQuestData data = ElementData.GetData<SubQuestData>();
		if (data != null)
		{
			FactoryBase.ReleaseAndClearList(_rewardCurrencyIconComponentList);
			FactoryBase.ReleaseAndClearList(_rewardItemIconComponentList);
			if (showSingleReward)
			{
				CommonMission.SetupNodeIconSubQuestReward(data, null, ref _rewardItemIconComponentList, ref _rewardCurrencyIconComponentList, RewardListGrid);
			}
			else
			{
				CommonMission.SetupSubQuestReward(data, null, ref _rewardItemIconComponentList, ref _rewardCurrencyIconComponentList, RewardListGrid);
			}
			UIHelper.RepositionGrid(RewardListGrid);
		}
	}

	protected void SetRewardFirstItemGrayScale(bool grayScale)
	{
		if (__Extension_Method_ICollection__.COUNT(_rewardItemIconComponentList) > 0)
		{
			_rewardItemIconComponentList[0].SetIconGrayScale(grayScale);
		}
		else if (__Extension_Method_ICollection__.COUNT(_rewardCurrencyIconComponentList) > 0)
		{
			_rewardCurrencyIconComponentList[0].SetIconGrayScale(grayScale);
		}
	}

	void ItemIconComponent.Listener.OnClick(ItemIconComponent itemIconComponent)
	{
		OnClick();
	}

	void CurrencyIconComponent.Listener.OnClick(CurrencyIconComponent component)
	{
		OnClick();
	}
}
