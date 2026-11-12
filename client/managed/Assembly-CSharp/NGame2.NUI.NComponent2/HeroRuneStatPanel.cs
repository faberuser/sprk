using System;
using System.Collections;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NHelper;
using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class HeroRuneStatPanel : MonoBehaviour, IUserObserver<NGame2.NAccount.UserHeroManager.ChangeHeroInfo>, IUserObserver
{
	public interface IDataProvider
	{
		CachedHeroInfo GetHeroInfo();

		int GetRunePage();
	}

	public UITable Table_Stat;

	public UIWidget EmptyWidget;

	public UIWidget StatWidget;

	public UIScrollView Scroll_Stat;

	public UIPanel ClippingPanel;

	public UIResizeByGrid ResizeGrid_Stat;

	public UISimpleGrid Grid_Stat;

	public UITable Table_AddSkill;

	public UIWidget Widget_AddSkill;

	public UIWidget Widget_ExtraOption;

	public UILabel Label_EmptyAddSkill;

	public int ExtraHeight;

	public int ExtraEmptyAddSkillHeight;

	private IDataProvider _dataProvider;

	private bool _isEmptyStat = true;

	private bool _isEmptyAddSkill = true;

	private List<LabelFieldComponent> _statCompList = new List<LabelFieldComponent>();

	private List<LabelFieldComponent> _addSkillCompList = new List<LabelFieldComponent>();

	private Coroutine _coroutine;

	public void Open(IDataProvider dataProvider)
	{
		_dataProvider = dataProvider;
		GlobalAutoBehaviour<AccountManager>.instance.HeroManager.ObserverManager.AddObserver(this);
		if (Scroll_Stat != null)
		{
			Scroll_Stat.ResetPosition();
		}
		Refresh();
	}

	public void Refresh()
	{
		if (_dataProvider != null && base.gameObject.activeSelf)
		{
			UpdateRuneStat();
			UpdateRuneAddSkill();
			UIHelper.EnableGameObject(EmptyWidget, _isEmptyStat && _isEmptyAddSkill);
			UIHelper.EnableGameObject(StatWidget, !_isEmptyStat || !_isEmptyAddSkill);
			UIHelper.EnableGameObject(Label_EmptyAddSkill, _isEmptyAddSkill);
			if (_coroutine != null)
			{
				VespaCoroutine.Stop(_coroutine);
				_coroutine = null;
			}
			_coroutine = VespaCoroutine.Start(this, CoReposition());
		}
	}

	private void UpdateRuneStat()
	{
		FactoryBase.ReleaseAndClearList(_statCompList);
		Dictionary<Enum, long> appliedRuneTotalStat = RuneHelper.GetAppliedRuneTotalStat(_dataProvider.GetHeroInfo(), _dataProvider.GetRunePage());
		_isEmptyStat = __Extension_Method_ICollection__.IsNullOrEmpty(appliedRuneTotalStat);
		UIHelper.EnableGameObject(EmptyWidget, __Extension_Method_ICollection__.IsNullOrEmpty(appliedRuneTotalStat));
		UIHelper.EnableGameObject(StatWidget, !__Extension_Method_ICollection__.IsNullOrEmpty(appliedRuneTotalStat));
		if (Grid_Stat == null || appliedRuneTotalStat == null)
		{
			return;
		}
		foreach (KeyValuePair<Enum, long> item in appliedRuneTotalStat)
		{
			LabelFieldComponent labelFieldComponent = UIHelper.AddGridItem(Grid_Stat, LabelFieldComponent.PrefabAuto);
			string field = LocalizationManager.GetString(item.Key);
			string statDisplayValue = StatData.GetStatDisplayValue(item.Key, item.Value);
			labelFieldComponent.Open(field, statDisplayValue);
			_statCompList.Add(labelFieldComponent);
		}
		Grid_Stat.RepositionGrid();
		if (ResizeGrid_Stat != null)
		{
			ResizeGrid_Stat.Refresh();
		}
	}

	private void UpdateRuneAddSkill()
	{
		FactoryBase.ReleaseAndClearList(_addSkillCompList);
		if (Table_AddSkill == null || _dataProvider.GetHeroInfo() == null)
		{
			return;
		}
		List<string> appliedRuneTotalAddSkill = RuneHelper.GetAppliedRuneTotalAddSkill(_dataProvider.GetHeroInfo(), _dataProvider.GetRunePage());
		_isEmptyAddSkill = __Extension_Method_ICollection__.IsNullOrEmpty(appliedRuneTotalAddSkill);
		if (__Extension_Method_ICollection__.IsNullOrEmpty(appliedRuneTotalAddSkill))
		{
			return;
		}
		foreach (string item in appliedRuneTotalAddSkill)
		{
			LabelFieldComponent labelFieldComponent = LabelFieldComponent.PrefabAuto4.Factory.Create(Table_AddSkill.transform);
			labelFieldComponent.Open(item, "");
			_addSkillCompList.Add(labelFieldComponent);
		}
	}

	private IEnumerator CoReposition()
	{
		yield return null;
		yield return null;
		if (Table_AddSkill != null)
		{
			Table_AddSkill.Reposition();
		}
		UpdateAddSkillBG();
		yield return null;
		if (Table_Stat != null)
		{
			Table_Stat.Reposition();
		}
		yield return null;
	}

	private void UpdateAddSkillBG()
	{
		if (!(Table_AddSkill != null) || !(Widget_AddSkill != null))
		{
			return;
		}
		if (_isEmptyAddSkill && ClippingPanel != null && Widget_ExtraOption != null)
		{
			int num = (int)ClippingPanel.GetViewSize().y - Widget_ExtraOption.height;
			Widget_AddSkill.height = num + ExtraEmptyAddSkillHeight;
			return;
		}
		int num2 = 0;
		int childCount = Table_AddSkill.transform.childCount;
		for (int i = 0; i < childCount; i++)
		{
			Transform child = Table_AddSkill.transform.GetChild(i);
			if (!(child == null))
			{
				UISprite componentInChildren = child.GetComponentInChildren<UISprite>();
				if (!(componentInChildren == null))
				{
					num2 += componentInChildren.height;
				}
			}
		}
		Widget_AddSkill.height = num2 + ExtraHeight;
	}

	public void Close()
	{
		GlobalAutoBehaviour<AccountManager>.instance.HeroManager.ObserverManager.RemoveObserver(this);
		FactoryBase.ReleaseAndClearList(_statCompList);
		FactoryBase.ReleaseAndClearList(_addSkillCompList);
		if (_coroutine != null)
		{
			VespaCoroutine.Stop(_coroutine);
			_coroutine = null;
		}
	}

	void IUserObserver<NGame2.NAccount.UserHeroManager.ChangeHeroInfo>.OnChanged(NGame2.NAccount.UserHeroManager.ChangeHeroInfo changedInfo)
	{
		if (changedInfo.ChangeType == NGame2.NAccount.UserHeroManager.HeroChangeType.RunePage)
		{
			Refresh();
		}
	}

	bool IUserObserver.IsGlobal()
	{
		return false;
	}
}
