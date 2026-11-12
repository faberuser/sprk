using System;
using System.Collections.Generic;
using NGame2.NAchievement;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NGameAsset;
using NShared;
using NVespa;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class HeroAchievementList : BaseWindow, MissionComponent.IListner
{
	public const string PrefabPath = "Resources/Prefab2/Window/Achievement/HeroAchievementList";

	[Header("[HeroAchievementList]")]
	public UIScrollGrid AchievementListGrid;

	public UITexture Texture_HeroFull;

	public UIButton Button_AllGet;

	public UIWidget WidgetCleared;

	public UILabel Label_Cleared;

	public WindowAnimation WindowAnimation;

	private List<MissionComponent> _ListHeroAchievements = new List<MissionComponent>();

	private int _HeroIndex;

	private bool _isDirty;

	private List<int> _ListClearedAchievementIndex = new List<int>();

	private List<int> _ListNotClearedAchievementIndex = new List<int>();

	private List<int> _ListCompletedAchievementIndex = new List<int>();

	private float _lastCheckedGrid;

	public override void Reset()
	{
		base.Reset();
		ResetComponentList();
		UIHelper.SetTextureMaterial(Texture_HeroFull, null);
		_isDirty = false;
	}

	public void Init(int HeroIndex)
	{
		_HeroIndex = HeroIndex;
		if (GlobalAutoBehaviour<GlobalManagement>.instance.WindowOpenedUI3DUI())
		{
			LayerInfo layerInfo = GameUIRoot.instance.GetLayerInfo(GameUIRoot.ELayerType.UI3DUI);
			ChangeLayerRecursive(layerInfo);
		}
		else
		{
			LayerInfo layerInfo2 = GameUIRoot.instance.GetLayerInfo(GameUIRoot.ELayerType.UI);
			ChangeLayerRecursive(layerInfo2);
		}
		if (AchievementListGrid != null)
		{
			UIHelper.GridOriginPositioning(AchievementListGrid.Grid);
		}
		SetupHeroFullPortrait();
		SetupHeroAchievementList();
		SetupClearedCount();
		RunOpenCloseAnimation(reversse: false, null);
	}

	private void UpdateGridParticleEffectRemove()
	{
		if (AchievementListGrid == null)
		{
			return;
		}
		MissionComponent[] componentsInChildren = AchievementListGrid.GetComponentsInChildren<MissionComponent>(includeInactive: true);
		if (componentsInChildren == null)
		{
			return;
		}
		MissionComponent[] array = componentsInChildren;
		foreach (MissionComponent obj in array)
		{
			bool RewardEffect = true;
			Vector3 localPosition = obj.transform.localPosition;
			localPosition.y += 20f;
			UIHelper.CheckOutOfGridSightItem(AchievementListGrid.Grid, localPosition, delegate
			{
				RewardEffect = false;
			}, null);
			obj.EnableRewardParticle(RewardEffect);
		}
	}

	public void SetDirty()
	{
		_isDirty = true;
	}

	public void RefreshList()
	{
		ResetComponentList();
		SetupHeroAchievementList();
		SetupClearedCount();
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		if (_isDirty)
		{
			RefreshList();
			_isDirty = false;
		}
		if (Time.time - _lastCheckedGrid >= 0.3f)
		{
			UpdateGridParticleEffectRemove();
			_lastCheckedGrid = Time.time;
		}
	}

	public override void Close()
	{
		RunOpenCloseAnimation(reversse: true, delegate
		{
			base.Close();
		});
	}

	private void RunOpenCloseAnimation(bool reversse, Action onPlayed)
	{
		if (!(WindowAnimation == null))
		{
			WindowAnimation.PlayAnimation(reversse, onPlayed);
		}
	}

	private void SetupHeroFullPortrait()
	{
		string code = GameRoot.Instance.CreatureDataContainer.GetCode(_HeroIndex);
		if (!String_Extension_Method.IsNullOrEmpty(code))
		{
			AssetTexture heroFullPortrait = UIHelper.GetHeroFullPortrait(code);
			UIHelper.ResetPlaneAssetTextureAsync(this, Texture_HeroFull, heroFullPortrait);
		}
	}

	private void ResetComponentList()
	{
		FactoryBase.ReleaseAndClearList(_ListHeroAchievements);
	}

	private void SetupClearedCount()
	{
		int rewardableHeroAchievementCount = SingletonAuto<AchievementChecker>.instance.GetRewardableHeroAchievementCount(_HeroIndex);
		UIHelper.EnableGameObject(WidgetCleared, rewardableHeroAchievementCount > 0);
		if (rewardableHeroAchievementCount > 0)
		{
			UIHelper.SetLabelText(Label_Cleared, rewardableHeroAchievementCount);
		}
	}

	private int GetGroupAchievementStatus(List<int> achievementIndices, out AchievementList.GroupAchievementStatus status)
	{
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		foreach (int achievementIndex in achievementIndices)
		{
			if (AchievementChecker.Completed(achievementIndex))
			{
				if (!flag3)
				{
					flag3 = true;
					num3 = achievementIndex;
				}
				else
				{
					num3 = ((num3 > achievementIndex) ? num3 : achievementIndex);
				}
			}
			else if (AchievementChecker.Cleared(achievementIndex))
			{
				if (!flag)
				{
					flag = true;
					num2 = achievementIndex;
				}
				else
				{
					num2 = ((num2 < achievementIndex) ? num2 : achievementIndex);
				}
			}
			else if (!flag2)
			{
				flag2 = true;
				num = achievementIndex;
			}
			else
			{
				num = ((num < achievementIndex) ? num : achievementIndex);
			}
		}
		if (flag)
		{
			status = AchievementList.GroupAchievementStatus.Cleared;
			return num2;
		}
		if (flag2)
		{
			status = AchievementList.GroupAchievementStatus.NotCleared;
			return num;
		}
		if (flag3)
		{
			status = AchievementList.GroupAchievementStatus.Completed;
			return num3;
		}
		status = AchievementList.GroupAchievementStatus.Completed;
		return achievementIndices[0];
	}

	private void SetupHeroAchievementList()
	{
		HashSet<int> heroAchievementIndexSet = GameRoot.Instance.AchievementDataContainer.GetHeroAchievementIndexSet(_HeroIndex);
		if (heroAchievementIndexSet == null)
		{
			return;
		}
		_ListClearedAchievementIndex.Clear();
		_ListNotClearedAchievementIndex.Clear();
		_ListCompletedAchievementIndex.Clear();
		HashSet<int>.Enumerator enumerator = heroAchievementIndexSet.GetEnumerator();
		Dictionary<int, List<int>> dictionary = new Dictionary<int, List<int>>();
		while (enumerator.MoveNext())
		{
			AchievementData data = GameRoot.Instance.AchievementDataContainer.GetData(enumerator.Current, 1);
			if (data.SubGroup != 0)
			{
				if (dictionary.ContainsKey(data.SubGroup))
				{
					dictionary[data.SubGroup].Add(enumerator.Current);
					continue;
				}
				List<int> list = new List<int>();
				list.Add(enumerator.Current);
				dictionary.Add(data.SubGroup, list);
			}
			else if (AchievementChecker.Completed(enumerator.Current))
			{
				_ListCompletedAchievementIndex.Add(enumerator.Current);
			}
			else if (AchievementChecker.Cleared(enumerator.Current))
			{
				_ListClearedAchievementIndex.Add(enumerator.Current);
			}
			else
			{
				_ListNotClearedAchievementIndex.Add(enumerator.Current);
			}
		}
		foreach (KeyValuePair<int, List<int>> item in dictionary)
		{
			if (item.Value.Count != 0)
			{
				AchievementList.GroupAchievementStatus status;
				int groupAchievementStatus = GetGroupAchievementStatus(item.Value, out status);
				switch (status)
				{
				case AchievementList.GroupAchievementStatus.Cleared:
					_ListClearedAchievementIndex.Add(groupAchievementStatus);
					break;
				case AchievementList.GroupAchievementStatus.NotCleared:
					_ListNotClearedAchievementIndex.Add(groupAchievementStatus);
					break;
				case AchievementList.GroupAchievementStatus.Completed:
					_ListCompletedAchievementIndex.Add(groupAchievementStatus);
					break;
				}
			}
		}
		_ListClearedAchievementIndex.Sort();
		_ListNotClearedAchievementIndex.Sort(delegate(int l, int r)
		{
			int achievedPercent = SingletonAuto<AchievementChecker>.instance.GetAchievedPercent(l);
			int achievedPercent2 = SingletonAuto<AchievementChecker>.instance.GetAchievedPercent(r);
			int num = -achievedPercent.CompareTo(achievedPercent2);
			return (num != 0) ? num : l.CompareTo(r);
		});
		_ListCompletedAchievementIndex.Sort();
		_ListClearedAchievementIndex.ForEach(delegate(int achievementIndex)
		{
			CreateAchievementComponent(achievementIndex);
		});
		_ListNotClearedAchievementIndex.ForEach(delegate(int achievementIndex)
		{
			CreateAchievementComponent(achievementIndex);
		});
		_ListCompletedAchievementIndex.ForEach(delegate(int achievementIndex)
		{
			CreateAchievementComponent(achievementIndex, Completed: true);
		});
		UIHelper.EnableButton(Button_AllGet, __Extension_Method_ICollection__.COUNT(_ListClearedAchievementIndex) > 0, grayscaleIfDisable: true);
		UIHelper.RepositionGrid(AchievementListGrid);
	}

	private void CreateAchievementComponent(int AchievementIndex, bool Completed = false)
	{
		MissionComponent missionComponent = UIHelper.AddGridItem(AchievementListGrid, MissionComponent.PrefabAuto);
		MissionComponentData missionComponentData = new MissionComponentData(AchievementIndex);
		missionComponent.ExpansionHeroOpen(missionComponentData, this);
		_ListHeroAchievements.Add(missionComponent);
	}

	public void OnClickHeroInfo()
	{
		if (SceneAutoBehaviour<AchievementManagement>.isValidInstance)
		{
			SceneAutoBehaviour<AchievementManagement>.instance.OpenHeroDetailView(_HeroIndex);
		}
	}

	public void OnClickGetAllReward()
	{
		if (_ListClearedAchievementIndex != null && SceneAutoBehaviour<AchievementManagement>.isValidInstance)
		{
			SceneAutoBehaviour<AchievementManagement>.instance.RequestRewardAchievements(_ListClearedAchievementIndex);
		}
	}

	public override void ReOpen()
	{
		base.ReOpen();
		SetDirty();
	}

	void MissionComponent.IListner.OnClick(MissionComponentData data)
	{
	}

	void MissionComponent.IListner.RequestReward(MissionComponentData data)
	{
		if (SceneAutoBehaviour<AchievementManagement>.isValidInstance)
		{
			SceneAutoBehaviour<AchievementManagement>.instance.RequestRewardAchievements(new List<int> { data.Index });
		}
	}

	void MissionComponent.IListner.RequestMove(string moveTaskType, string[] moveTaskValue)
	{
	}
}
