using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NAchievement;
using NGame2.NContentsDefine;
using NGame2.NUI;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.Mission;
using NGame2.NUI.NWindow.NBattle;
using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using NVespa.NTask;
using NVespa.NTask.NRuntime;
using UnityEngine;

// Adds the Missions "Claim All" button to the Hero Achievements grid
// (AchievementList). It claims, for every owned hero, exactly what that hero's
// own "Get All" would claim, in one RewardAchievement request.
public sealed class SprkHeroAchievementClaimAll : MonoBehaviour
{
	private const float Margin = 24f;

	private const float RefreshInterval = 0.5f;

	private const float ClickCooldown = 1.5f;

	// Server limit on AchievementIndices per RewardAchievement request.
	private const int MaxBatch = 100;

	private AchievementList window;

	private UIButton button;

	private UIPanel panel;

	private Bounds bounds;

	private float nextRefresh;

	private float lockedUntil;

	private bool failed;

	public static void Attach(AchievementList window)
	{
		if (window == null)
		{
			return;
		}
		try
		{
			SprkHeroAchievementClaimAll sprkHeroAchievementClaimAll = window.GetComponent<SprkHeroAchievementClaimAll>();
			if (sprkHeroAchievementClaimAll == null)
			{
				sprkHeroAchievementClaimAll = window.gameObject.AddComponent<SprkHeroAchievementClaimAll>();
				sprkHeroAchievementClaimAll.window = window;
			}
			sprkHeroAchievementClaimAll.Build();
			sprkHeroAchievementClaimAll.nextRefresh = 0f;
		}
		catch (Exception ex)
		{
			Debug.LogWarning("SPRK hero achievement claim all: " + ex.GetType().Name + ": " + ex.Message);
		}
	}

	private void Build()
	{
		if (button != null || failed)
		{
			return;
		}
		UIButton uIButton = SourceButton();
		if (uIButton == null)
		{
			failed = true;
			Debug.LogWarning("SPRK hero achievement claim all: no Claim All button template was found.");
			return;
		}
		Transform transform = ((window.WindowRoot != null) ? window.WindowRoot : window.transform);
		GameObject gameObject = UnityEngine.Object.Instantiate(uIButton.gameObject, transform, worldPositionStays: false);
		gameObject.name = "SPRK Hero Claim All";
		gameObject.SetActive(value: true);
		DetachExternalReferences(gameObject);
		button = gameObject.GetComponent<UIButton>();
		button.onClick.Clear();
		EventDelegate.Add(button.onClick, OnClick);
		panel = gameObject.GetComponent<UIPanel>();
		if (panel == null)
		{
			panel = gameObject.AddComponent<UIPanel>();
		}
		int num = 0;
		UIPanel[] componentsInChildren = window.GetComponentsInChildren<UIPanel>(includeInactive: true);
		foreach (UIPanel uIPanel in componentsInChildren)
		{
			if (uIPanel != panel)
			{
				num = Mathf.Max(num, uIPanel.depth);
			}
		}
		panel.depth = num + 1;
		NGUITools.SetLayer(gameObject, window.gameObject.layer);
		gameObject.transform.localScale = Vector3.one;
		bounds = NGUIMath.CalculateRelativeWidgetBounds(gameObject.transform);
		window.MakeDirty();
		Place();
	}

	private static UIButton SourceButton()
	{
		GameObject gameObject = VespaResource.Load<GameObject>(MissionNodeWindowTopView2.PrefabPath);
		MissionNodeWindowTopView2 missionNodeWindowTopView = ((gameObject != null) ? gameObject.GetComponent<MissionNodeWindowTopView2>() : null);
		if (missionNodeWindowTopView != null && missionNodeWindowTopView.RewardButtonGroupList != null)
		{
			// Basic is the compact blue "Claim All" beside "Claim Reward" in the
			// Guideline; GetAllRewardOnly is a wide green one.
			UIButton uIButton = null;
			MissionNodeWindowTopView2.TopViewRewardButtonGroup[] rewardButtonGroupList = missionNodeWindowTopView.RewardButtonGroupList;
			foreach (MissionNodeWindowTopView2.TopViewRewardButtonGroup topViewRewardButtonGroup in rewardButtonGroupList)
			{
				if (topViewRewardButtonGroup != null && !(topViewRewardButtonGroup.GetAllRewardButton == null))
				{
					if (topViewRewardButtonGroup.ButtonGroupType == EMissionTopViewButtonGroup.Basic)
					{
						return topViewRewardButtonGroup.GetAllRewardButton;
					}
					if (uIButton == null)
					{
						uIButton = topViewRewardButtonGroup.GetAllRewardButton;
					}
				}
			}
			if (uIButton != null)
			{
				return uIButton;
			}
		}
		GameObject gameObject2 = VespaResource.Load<GameObject>(HeroAchievementList.PrefabPath);
		HeroAchievementList heroAchievementList = ((gameObject2 != null) ? gameObject2.GetComponent<HeroAchievementList>() : null);
		return (heroAchievementList != null) ? heroAchievementList.Button_AllGet : null;
	}

	// The template lives inside another window's prefab: drop anchors and
	// message forwarders that still point at that prefab.
	private static void DetachExternalReferences(GameObject root)
	{
		UIRect[] componentsInChildren = root.GetComponentsInChildren<UIRect>(includeInactive: true);
		foreach (UIRect uIRect in componentsInChildren)
		{
			if (Outside(root, uIRect.leftAnchor.target) || Outside(root, uIRect.rightAnchor.target) || Outside(root, uIRect.bottomAnchor.target) || Outside(root, uIRect.topAnchor.target))
			{
				uIRect.SetAnchor((Transform)null);
			}
		}
		UIButtonMessage[] componentsInChildren2 = root.GetComponentsInChildren<UIButtonMessage>(includeInactive: true);
		for (int j = 0; j < componentsInChildren2.Length; j++)
		{
			UnityEngine.Object.Destroy(componentsInChildren2[j]);
		}
		UIEventTrigger[] componentsInChildren3 = root.GetComponentsInChildren<UIEventTrigger>(includeInactive: true);
		for (int k = 0; k < componentsInChildren3.Length; k++)
		{
			UnityEngine.Object.Destroy(componentsInChildren3[k]);
		}
		UIButton[] componentsInChildren4 = root.GetComponentsInChildren<UIButton>(includeInactive: true);
		foreach (UIButton uIButton in componentsInChildren4)
		{
			if (uIButton.gameObject != root)
			{
				uIButton.onClick.Clear();
			}
		}
	}

	private static bool Outside(GameObject root, Transform target)
	{
		return target != null && !target.IsChildOf(root.transform);
	}

	private void LateUpdate()
	{
		if (button == null || window == null)
		{
			return;
		}
		bool flag = window.IsOpen() && window.HeroListGrid != null && window.HeroListGrid.gameObject.activeInHierarchy && !CoveredByHeroWindow();
		if (button.gameObject.activeSelf != flag)
		{
			button.gameObject.SetActive(flag);
			nextRefresh = 0f;
		}
		if (!flag)
		{
			return;
		}
		if (button.gameObject.layer != window.gameObject.layer)
		{
			NGUITools.SetLayer(button.gameObject, window.gameObject.layer);
		}
		Place();
		if (Time.unscaledTime >= nextRefresh)
		{
			nextRefresh = Time.unscaledTime + RefreshInterval;
			bool enable = Time.unscaledTime >= lockedUntil && ClaimableAchievements().Count > 0;
			UIHelper.EnableButton(button, enable, grayscaleIfDisable: true);
		}
	}

	private static bool CoveredByHeroWindow()
	{
		if (!SceneAutoBehaviour<AchievementManagement>.isValidInstance)
		{
			return false;
		}
		AchievementManagement instance = SceneAutoBehaviour<AchievementManagement>.instance;
		return instance.IsOpened<HeroAchievementList>() || instance.IsOpened<HeroDetailView>();
	}

	private void Place()
	{
		Camera camera = NGUITools.FindCameraForLayer(button.gameObject.layer);
		Transform parent = button.transform.parent;
		if (camera == null || parent == null)
		{
			return;
		}
		Vector3 vector = parent.InverseTransformPoint(camera.ViewportToWorldPoint(new Vector3(1f, 0f, 0f)));
		Vector3 localScale = button.transform.localScale;
		float x = Mathf.Round(vector.x - bounds.max.x * localScale.x - Margin);
		float y = Mathf.Round(vector.y - bounds.min.y * localScale.y + Margin);
		Vector3 localPosition = button.transform.localPosition;
		if (localPosition.x != x || localPosition.y != y)
		{
			button.transform.localPosition = new Vector3(x, y, localPosition.z);
		}
	}

	private void OnClick()
	{
		if (Time.unscaledTime < lockedUntil || !SceneAutoBehaviour<AchievementManagement>.isValidInstance)
		{
			return;
		}
		List<int> list = ClaimableAchievements();
		if (list.Count == 0)
		{
			return;
		}
		lockedUntil = Time.unscaledTime + ClickCooldown;
		UIHelper.EnableButton(button, enable: false, grayscaleIfDisable: true);
		nextRefresh = lockedUntil;
		HashSet<CurrencyType> hashSet = new HashSet<CurrencyType>();
		foreach (int item in list)
		{
			AchievementData data = GameRoot.Instance.AchievementDataContainer.GetData(item, AchievementChecker.GetLastStepOnAchievementInfo(item));
			HashSet<CurrencyType> hashSet2 = ((data != null) ? CommonMission.GetCurrencyTypesFromAchievementData(data) : null);
			if (hashSet2 != null)
			{
				hashSet.UnionWith(hashSet2);
			}
		}
		CurrencyMaxCap.WarningOrExecute(hashSet, delegate
		{
			Claim(list);
		});
	}

	// RewardAchievement accepts at most MaxBatch indices per request, so claim in
	// chunks and show every chunk's rewards in one popup at the end.
	private void Claim(List<int> list)
	{
		if (!SceneAutoBehaviour<AchievementManagement>.isValidInstance)
		{
			return;
		}
		AchievementManagement manager = SceneAutoBehaviour<AchievementManagement>.instance;
		BaseRewardPopup.PopupOpenData popup = new BaseRewardPopup.PopupOpenData().SetTitle(LocalizationManager.GetString(Quest.COMPLETE_QUEST_MSG));
		List<FriendshipPointResultInfo> friendship = new List<FriendshipPointResultInfo>();
		List<StaminaResultInfo> stamina = new List<StaminaResultInfo>();
		int claimed = 0;
		SequenceTask sequenceTask = new SequenceTask();
		for (int i = 0; i < list.Count; i += MaxBatch)
		{
			List<int> range = list.GetRange(i, Mathf.Min(MaxBatch, list.Count - i));
			sequenceTask.ADDTASK(manager.RequestRewardAchievementTask(range, delegate(NShared.RewardAchievement.Request request, NShared.RewardAchievement.Response response, bool isHero, Action afterReward)
			{
				claimed++;
				popup.AddItemResultInfos(response.itemResults).AddEquipResultInfos(response.equipItemInfos).AddCurrencyResultInfos3(response.CurrencyResults);
				if (response.expResult != null)
				{
					popup.SetExpResult(response.expResult);
				}
				if (response.FriendshipPointResults != null)
				{
					friendship.AddRange(response.FriendshipPointResults);
				}
				if (response.staminaResults != null)
				{
					stamina.AddRange(response.staminaResults);
				}
				int[] achievementIndices = request.AchievementIndices;
				for (int j = 0; j < __Extension_Method_ICollection__.COUNT(achievementIndices); j++)
				{
					SingletonAuto<AchievementNotiChecker>.instance.RewardAchievement(achievementIndices[j]);
				}
				Vespa_System_Action_Extension_Method.fire(afterReward);
			}, isHeroAchievement: true));
		}
		sequenceTask.ADDTASK(new DelegateTask(delegate
		{
			if (window != null)
			{
				window.SetDirty();
			}
			nextRefresh = 0f;
			if (claimed > 0)
			{
				popup.SetFriendshipPointResults(friendship.ToArray());
				if (stamina.Count > 0)
				{
					popup.SetStaminaResultInfos(stamina.ToArray());
				}
				BaseRewardPopup.OpenPopup(popup, GlobalAutoBehaviour<GlobalManagement>.instance.WindowOpenedUI3DUI(), null);
			}
		}));
		manager.AddTask(sequenceTask);
	}

	// Mirrors HeroAchievementList: standalone achievements that are cleared,
	// and the lowest cleared achievement of each sub-group, for owned heroes.
	public static List<int> ClaimableAchievements()
	{
		List<int> list = new List<int>();
		AchievementDataContainer achievementDataContainer = GameRoot.Instance.AchievementDataContainer;
		IList<CreatureData> list2 = GameRoot.Instance.CreatureDataContainer.GetAllData();
		NGame2.NAccount.UserHeroManager heroManager = GlobalAutoBehaviour<AccountManager>.instance.HeroManager;
		if (achievementDataContainer == null || list2 == null || heroManager == null)
		{
			return list;
		}
		HashSet<int> hashSet = new HashSet<int>();
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		for (int i = 0; i < list2.Count; i++)
		{
			CreatureData creatureData = list2[i];
			if (creatureData == null || heroManager.Get(creatureData.Index) == null)
			{
				continue;
			}
			HashSet<int> heroAchievementIndexSet = achievementDataContainer.GetHeroAchievementIndexSet(creatureData.Index);
			if (heroAchievementIndexSet == null)
			{
				continue;
			}
			dictionary.Clear();
			foreach (int item in heroAchievementIndexSet)
			{
				if (AchievementChecker.Completed(item) || !AchievementChecker.Cleared(item))
				{
					continue;
				}
				AchievementData data = achievementDataContainer.GetData(item, 1);
				if (data == null)
				{
					continue;
				}
				if (data.SubGroup != 0)
				{
					if (!dictionary.TryGetValue(data.SubGroup, out var value) || item < value)
					{
						dictionary[data.SubGroup] = item;
					}
				}
				else if (hashSet.Add(item))
				{
					list.Add(item);
				}
			}
			foreach (int value2 in dictionary.Values)
			{
				if (hashSet.Add(value2))
				{
					list.Add(value2);
				}
			}
		}
		list.Sort();
		return list;
	}
}
