using System;
using System.Collections.Generic;
using System.Linq;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager.Mission;
using NGame2.NUI.NWindow;
using NShared;
using NVespa.NPrefab2;
using UnityEngine;

[RequireComponent(typeof(UIGrid))]
public class MissionCategoryPanel : MonoBehaviour, MissionCategoryComponent.IListner, WindowAnimation.IAlphaAnimator, ButtonSpecialEventComponent.Listner
{
	public interface IListener
	{
		void SelectCategoryMenu(MissionCategoryData data);
	}

	[Header("[MissionCategoryPanel]")]
	public UIGrid SpecialMissionCategoryGrid;

	public int MaxSpecialMissionCategoryNum = 3;

	private List<ButtonSpecialEventComponent> _specialMissionListComp = new List<ButtonSpecialEventComponent>();

	private List<MissionCategoryComponent> _listComp = new List<MissionCategoryComponent>();

	private IListener _listener;

	private float _lastUpdateTick = -9999f;

	public int SelectMainIndex { get; private set; }

	private void OnEnable()
	{
		MissionMilestonePopup.noticeOpenMissionPopup = (MissionMilestonePopup.NoticeOpenMissionPopup)Delegate.Combine(MissionMilestonePopup.noticeOpenMissionPopup, new MissionMilestonePopup.NoticeOpenMissionPopup(NoticeOpenMissionPopup));
		MissionMilestonePopup.noticeCloseMissionPopup = (MissionMilestonePopup.NoticeCloseMissionPopup)Delegate.Combine(MissionMilestonePopup.noticeCloseMissionPopup, new MissionMilestonePopup.NoticeCloseMissionPopup(NoticeCloseMissionPopup));
	}

	private void OnDisable()
	{
		MissionMilestonePopup.noticeOpenMissionPopup = (MissionMilestonePopup.NoticeOpenMissionPopup)Delegate.Remove(MissionMilestonePopup.noticeOpenMissionPopup, new MissionMilestonePopup.NoticeOpenMissionPopup(NoticeOpenMissionPopup));
		MissionMilestonePopup.noticeCloseMissionPopup = (MissionMilestonePopup.NoticeCloseMissionPopup)Delegate.Remove(MissionMilestonePopup.noticeCloseMissionPopup, new MissionMilestonePopup.NoticeCloseMissionPopup(NoticeCloseMissionPopup));
	}

	private void Start()
	{
		if ((bool)SpecialMissionCategoryGrid)
		{
			SpecialMissionCategoryGrid.onCustomSort = LeftMenu.RightMenuButtonGridSort;
		}
	}

	public void Init(IListener listener)
	{
		Clear();
		_listener = listener;
		SelectMainIndex = 0;
		CreateMainCategory();
		GetComponent<UIGrid>().repositionNow = true;
		SpecialMissionCategoryGrid.Reposition();
		foreach (MissionCategoryComponent item in _listComp)
		{
			item.ToggleState(state: false);
		}
		RunOpenCloseAnimation(reversse: false);
	}

	private void Update()
	{
		if (!(Time.time - _lastUpdateTick > 1f))
		{
			return;
		}
		_lastUpdateTick = Time.time;
		bool flag = false;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_specialMissionListComp); i++)
		{
			ButtonSpecialEventComponent buttonSpecialEventComponent = _specialMissionListComp[i];
			if (!(buttonSpecialEventComponent == null) && buttonSpecialEventComponent.IsTimeout())
			{
				_specialMissionListComp.RemoveAt(i);
				i--;
				FactoryBase.ReleaseComponent(buttonSpecialEventComponent);
				flag = true;
			}
		}
		if (flag)
		{
			SpecialMissionCategoryGrid.Reposition();
		}
	}

	public void Refresh()
	{
		foreach (MissionCategoryComponent item in _listComp)
		{
			item.Refresh();
		}
		foreach (ButtonSpecialEventComponent item2 in _specialMissionListComp)
		{
			if (item2 != null)
			{
				item2.Refresh();
			}
		}
	}

	public void Clear()
	{
		FactoryBase.ReleaseAndClearList(_listComp);
		FactoryBase.ReleaseAndClearList(_specialMissionListComp);
	}

	private void NoticeOpenMissionPopup()
	{
		RunOpenCloseAnimation(reversse: true);
	}

	private void NoticeCloseMissionPopup()
	{
		RunOpenCloseAnimation(reversse: false);
	}

	public void RunOpenCloseAnimation(bool reversse)
	{
		WindowAnimation component = GetComponent<WindowAnimation>();
		if (!(component == null))
		{
			component.PlayAnimation(reversse);
		}
	}

	private void CreateMainCategory()
	{
		IList<MissionCategoryData> allData = GameRoot.Instance.MissionCategoryDataContainer.GetAllData();
		Dictionary<int, MissionCategoryData> dictionary = new Dictionary<int, MissionCategoryData>();
		int num = 0;
		foreach (MissionCategoryData item in allData)
		{
			if (item.EqualMissionType(EMissionType.Special) || item.EqualMissionType(EMissionType.Special2) || item.EqualMissionType(EMissionType.Special3))
			{
				if (!String_Extension_Method.IsNullOrEmpty(item.ButtonPrefabName) && num < MaxSpecialMissionCategoryNum && SetupSpecialMissionButton(item))
				{
					num++;
				}
			}
			else if (!dictionary.ContainsKey(item.MainCategoryIndex))
			{
				dictionary.Add(item.MainCategoryIndex, item);
			}
		}
		dictionary.OrderBy((KeyValuePair<int, MissionCategoryData> i) => i.Key);
		int num2 = 0;
		foreach (MissionCategoryData value in dictionary.Values)
		{
			CreateCategoryComponent(value, num2++);
		}
	}

	private bool SetupSpecialMissionButton(MissionCategoryData data)
	{
		if (!MissionManagement.IsOpenSubCategory(data))
		{
			return false;
		}
		ButtonSpecialEventComponent buttonSpecialEventComponent = ButtonSpecialEventComponent.CreateComponent(data.ButtonPrefabName, SpecialMissionCategoryGrid.transform);
		if (buttonSpecialEventComponent == null)
		{
			return false;
		}
		buttonSpecialEventComponent.Open(data, this);
		_specialMissionListComp.Add(buttonSpecialEventComponent);
		return true;
	}

	private void CreateCategoryComponent(MissionCategoryData data, int toggleIndex, bool isSubList = false)
	{
		MissionCategoryComponent missionCategoryComponent = ((!isSubList) ? MissionCategoryComponent.PrefabAuto.Factory.Create(base.transform, base.gameObject.layer) : MissionCategoryComponent.PrefabAuto2.Factory.Create(base.transform, base.gameObject.layer));
		missionCategoryComponent.transform.localScale = Vector3.one;
		missionCategoryComponent.OpenComponent(toggleIndex, data, this);
		_listComp.Add(missionCategoryComponent);
	}

	public void SetupToggle(int mainCategoryIndex = -1)
	{
		foreach (MissionCategoryComponent item in _listComp)
		{
			item.ToggleState(state: false);
			if (item.MissionCategoryData != null && item.MissionCategoryData.MainCategoryIndex == mainCategoryIndex)
			{
				item.ToggleState(state: true);
			}
		}
	}

	void MissionCategoryComponent.IListner.OnClick(MissionCategoryComponent comp)
	{
		foreach (MissionCategoryComponent item in _listComp)
		{
			item.ToggleState(state: false);
		}
		comp.ToggleState(state: true);
		if (_listener != null)
		{
			_listener.SelectCategoryMenu(comp.MissionCategoryData);
		}
	}

	void WindowAnimation.IAlphaAnimator.SetAlpha(float alpha)
	{
		GetComponent<UIWidget>().alpha = alpha;
	}

	void ButtonSpecialEventComponent.Listner.OnClick(MissionCategoryData Data)
	{
		if (_listener != null)
		{
			_listener.SelectCategoryMenu(Data);
		}
	}
}
