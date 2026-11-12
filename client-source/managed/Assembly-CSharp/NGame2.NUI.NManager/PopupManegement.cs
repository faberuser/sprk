using System;
using System.Collections.Generic;
using System.Linq;
using NGame2.NAchievement;
using NGame2.NScene;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager.Mission;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.Mission;
using NShared;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NManager;

public class PopupManegement : GlobalAutoBehaviour<PopupManegement>, AchievementClearedPopup.Listener
{
	private class Wrapper
	{
		public IWindowPrefab Instance;

		public WeakRef<BaseWindow> WindowRef;
	}

	private class GlobalWrapper
	{
		public IGlobalWindowPrefab Instance;

		public WeakRef<IGlobalWindow> WindowRef;
	}

	public int MaxDepth;

	private List<Wrapper> _popupList = new List<Wrapper>();

	private List<GlobalWrapper> _globalPopupList = new List<GlobalWrapper>();

	public bool AnyPopupExist()
	{
		return _popupList.Any();
	}

	public void Add<T>(WindowPrefab<T> prefab) where T : BaseWindow
	{
		Add(prefab, prefab.instance);
	}

	public void Add(IWindowPrefab prefab, BaseWindow window)
	{
		for (int i = 0; i < _popupList.Count; i++)
		{
			if (_popupList[i].WindowRef.Value == window)
			{
				return;
			}
		}
		WeakRef<BaseWindow> weakRef = new WeakRef<BaseWindow>();
		weakRef.Value = window;
		_popupList.Add(new Wrapper
		{
			Instance = prefab,
			WindowRef = weakRef
		});
		if (GlobalAutoBehaviour<GlobalManagement>.isValidInstance)
		{
			GlobalAutoBehaviour<GlobalManagement>.instance.ReorderDepth();
		}
	}

	public void AddGlobal<T>(GlobalWindowPrefab<T> prefab) where T : Component, IGlobalWindow
	{
		AddGlobal(prefab, prefab.instance);
	}

	public void AddGlobal(IGlobalWindowPrefab prefab, IGlobalWindow window)
	{
		for (int i = 0; i < _popupList.Count; i++)
		{
			if (_popupList[i].WindowRef.Value == window)
			{
				return;
			}
		}
		WeakRef<IGlobalWindow> weakRef = new WeakRef<IGlobalWindow>();
		weakRef.Value = window;
		_globalPopupList.Add(new GlobalWrapper
		{
			Instance = prefab,
			WindowRef = weakRef
		});
		if (GlobalAutoBehaviour<GlobalManagement>.isValidInstance)
		{
			GlobalAutoBehaviour<GlobalManagement>.instance.ReorderDepth();
		}
	}

	public IWindowPrefab Find(Func<IWindowPrefab, bool> selector)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_popupList); i++)
		{
			Wrapper wrapper = _popupList[i];
			if (wrapper.Instance != null && Vespa_System_Action_Extension_Method.fire(selector, wrapper.Instance))
			{
				return wrapper.Instance;
			}
		}
		return null;
	}

	public BaseWindow FindRef(Func<BaseWindow, bool> selector)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_popupList); i++)
		{
			Wrapper wrapper = _popupList[i];
			if (!(wrapper.WindowRef.Value == null) && Vespa_System_Action_Extension_Method.fire(selector, wrapper.WindowRef.Value))
			{
				return wrapper.WindowRef.Value;
			}
		}
		return null;
	}

	public T FindWindow<T>() where T : class, IWindow
	{
		IWindowPrefab windowPrefab = Find((IWindowPrefab e) => e.WindowRaw is T);
		if (windowPrefab != null)
		{
			return windowPrefab.WindowRaw as T;
		}
		BaseWindow baseWindow = FindRef((BaseWindow e) => e is T);
		if (baseWindow != null)
		{
			return baseWindow as T;
		}
		return null;
	}

	public bool Exists(BaseWindow window)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_popupList); i++)
		{
			if (window == _popupList[i].WindowRef.Value)
			{
				return true;
			}
		}
		return false;
	}

	public void CloseAll()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_popupList); i++)
		{
			Wrapper wrapper = _popupList[i];
			if (wrapper != null)
			{
				if (wrapper.Instance != null)
				{
					wrapper.Instance.CloseWindow();
				}
				else if (wrapper.WindowRef.Value != null)
				{
					wrapper.WindowRef.Value.Close();
				}
			}
		}
		for (int j = 0; j < __Extension_Method_ICollection__.COUNT(_globalPopupList); j++)
		{
			GlobalWrapper globalWrapper = _globalPopupList[j];
			if (globalWrapper != null)
			{
				if (!Vespa_Common_Class_Extension_Method.IsUnityNull(globalWrapper.Instance))
				{
					globalWrapper.Instance.CloseWindow();
				}
				else if (!Vespa_Common_Class_Extension_Method.IsUnityNull(globalWrapper.WindowRef.Value))
				{
					globalWrapper.WindowRef.Value.Close();
				}
			}
		}
	}

	private void Update()
	{
		ReleaseClosedWindow();
		UpdateAchievementClearedNoti();
	}

	private void UpdateAchievementClearedNoti()
	{
		if (SingletonAuto<AchievementNotiChecker>.isValidInstance && !GlobalAutoBehaviour<LoadingManager>.instance.IsSceneLoading)
		{
			bool isReward = false;
			AchievementData firstComponent = SingletonAuto<AchievementNotiChecker>.instance.GetFirstComponent(out isReward);
			if (firstComponent != null)
			{
				SceneAutoBehaviour<AchievementClearedPopupQueue>.instance.ReserveAchievementClearedPopUp(firstComponent, isReward);
			}
		}
	}

	private void ReleaseClosedWindow()
	{
		bool flag = false;
		for (int num = __Extension_Method_ICollection__.COUNT(_popupList) - 1; num >= 0; num--)
		{
			Wrapper wrapper = _popupList[num];
			BaseWindow value = wrapper.WindowRef.Value;
			if (value == null || (!value.ReservedOpenTask && !value.IsOpen() && !value.IsClosingWindowAnimating()))
			{
				if (value != null && wrapper.Instance != null)
				{
					wrapper.Instance.ReleaseWindow();
				}
				_popupList.RemoveAt(num);
				flag = true;
			}
		}
		for (int num2 = __Extension_Method_ICollection__.COUNT(_globalPopupList) - 1; num2 >= 0; num2--)
		{
			GlobalWrapper globalWrapper = _globalPopupList[num2];
			IGlobalWindow value2 = globalWrapper.WindowRef.Value;
			if (Vespa_Common_Class_Extension_Method.IsUnityNull(value2) || !value2.IsOpen())
			{
				if (!Vespa_Common_Class_Extension_Method.IsUnityNull(value2) && !Vespa_Common_Class_Extension_Method.IsUnityNull(globalWrapper.Instance))
				{
					globalWrapper.Instance.ReleaseWindow();
				}
				_globalPopupList.RemoveAt(num2);
				flag = true;
			}
		}
		if (flag && GlobalAutoBehaviour<GlobalManagement>.isValidInstance)
		{
			GlobalAutoBehaviour<GlobalManagement>.instance.ReorderDepth();
		}
	}

	public T PopUpExist<T>() where T : BaseWindow, IWindow
	{
		Type typeFromHandle = typeof(T);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_popupList); i++)
		{
			if (typeFromHandle == _popupList[i].WindowRef.Value.GetType())
			{
				return (T)_popupList[i].WindowRef.Value;
			}
		}
		return null;
	}

	public T GlobalPopupExist<T>() where T : BaseWindow, IGlobalWindow
	{
		Type typeFromHandle = typeof(T);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_globalPopupList); i++)
		{
			if (typeFromHandle == _globalPopupList[i].WindowRef.Value.GetType())
			{
				return (T)_globalPopupList[i].WindowRef.Value;
			}
		}
		return null;
	}

	public void ReorderDepth(int maxDepth)
	{
		maxDepth += 10;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_popupList); i++)
		{
			WeakRef<BaseWindow> windowRef = _popupList[i].WindowRef;
			if (!(windowRef.Value == null))
			{
				List<UIPanel> list = new List<UIPanel>(windowRef.Value.GetComponentsInChildren<UIPanel>(includeInactive: true));
				list.Sort((UIPanel l, UIPanel r) => l.depth.CompareTo(r.depth));
				for (int num = 0; num < __Extension_Method_ICollection__.COUNT(list); num++)
				{
					list[num].depth = maxDepth++;
				}
			}
		}
		for (int num2 = 0; num2 < __Extension_Method_ICollection__.COUNT(_globalPopupList); num2++)
		{
			WeakRef<IGlobalWindow> windowRef2 = _globalPopupList[num2].WindowRef;
			if (Vespa_Common_Class_Extension_Method.IsUnityNull(windowRef2.Value))
			{
				continue;
			}
			Component component = windowRef2.Value as Component;
			if (!(component == null))
			{
				List<UIPanel> list2 = new List<UIPanel>(component.GetComponentsInChildren<UIPanel>(includeInactive: true));
				list2.Sort((UIPanel l, UIPanel r) => l.depth.CompareTo(r.depth));
				for (int num3 = 0; num3 < __Extension_Method_ICollection__.COUNT(list2); num3++)
				{
					list2[num3].depth = maxDepth++;
				}
			}
		}
		MaxDepth = maxDepth;
	}

	void AchievementClearedPopup.Listener.OnClickAchievementClearedPopup(int QuestIndex)
	{
		if (GlobalAutoBehaviour<LoadingManager>.instance.CurrentSceneType != LoadingManager.ESceneType.Lobby || (SceneAutoBehaviour<LobbyPopupQueue>.isValidInstance && !SceneAutoBehaviour<LobbyPopupQueue>.instance.Empty()) || GlobalAutoBehaviour<GlobalManagement>.instance.WindowOpened<MissionMain>())
		{
			return;
		}
		MissionVisualData missionVisualDataByQuestIndex = GameRoot.Instance.MissionVisualDataContainer.GetMissionVisualDataByQuestIndex(QuestIndex);
		MissionCategoryData missionCategoryData = null;
		if (missionVisualDataByQuestIndex != null)
		{
			missionCategoryData = GameRoot.Instance.MissionCategoryDataContainer.GetData(missionVisualDataByQuestIndex.MainCategoryIndex, missionVisualDataByQuestIndex.SubCategoryIndex);
		}
		if (missionCategoryData != null)
		{
			if (missionCategoryData.IsHeroMission() || missionCategoryData.IsWebEvent())
			{
				SceneAutoBehaviour<MissionManagement>.instance.MissionWindowManager.OpenMissionMain(missionCategoryData.MainCategoryIndex);
				return;
			}
			SceneAutoBehaviour<MissionManagement>.instance.MissionWindowManager.OpenMissionMain(missionCategoryData.MainCategoryIndex, missionCategoryData.SubCategoryIndex, forceOpen: true);
			if (QuestIndex > 0)
			{
				MissionComponentData elementData = new MissionComponentData(QuestIndex, missionVisualDataByQuestIndex);
				SceneAutoBehaviour<MissionManagement>.instance.MissionWindowManager.OpenMissionPopup(elementData);
			}
			return;
		}
		AchievementData achievementData = SingletonAuto<AchievementChecker>.instance.GetAchievementData(QuestIndex);
		if (achievementData != null)
		{
			if (achievementData.Type == AchievementType.Hero)
			{
				foreach (MissionCategoryData allDatum in GameRoot.Instance.MissionCategoryDataContainer.GetAllData())
				{
					if (allDatum.IsHeroMission())
					{
						int subCategory = Vespa_String_Extension_Method.TOINT(achievementData.ReqValue, 0);
						SceneAutoBehaviour<MissionManagement>.instance.MissionWindowManager.OpenMissionMain(allDatum.MainCategoryIndex, subCategory);
						break;
					}
				}
				return;
			}
			SceneAutoBehaviour<MissionManagement>.instance.MissionWindowManager.OpenMissionMain();
		}
		else
		{
			SceneAutoBehaviour<MissionManagement>.instance.MissionWindowManager.OpenMissionMain();
		}
	}
}
