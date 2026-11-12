using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using NVespa.NUtil;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class PlayRecord : BaseWindow, IUserObserver<PlayRecordInfo>, IUserObserver
{
	public interface Listener
	{
	}

	[Serializable]
	public class TabInfo
	{
		public PlayRecordCategory Categoty;

		public UIToggle Tab;
	}

	public const string PrefabPath = "Resources/Prefab2/Window/PlayRecord/PlayRecord";

	public TabInfo[] Tabs;

	public UIBaseGrid PlayRecordGrid;

	public UILabel NickLabel;

	public LocalizeLinkData LocalizeLinkNick = new LocalizeLinkData();

	private Listener _listener;

	private List<Component> _playRecordComponentList = new List<Component>();

	private PlayRecordCategory _currentCategory;

	private PlayRecordCategory _selectedCategory;

	public void Open(Listener listener)
	{
		base.Open();
		_listener = listener;
		Setup();
	}

	public override void Reset()
	{
		base.Reset();
		ClearRecord();
	}

	public override void Close()
	{
		base.Close();
	}

	public override void OnClosing()
	{
		base.OnClosing();
	}

	private void Setup()
	{
		UIHelper.SetLabelText(NickLabel, LocalizeLinkNick.GetLocalize(GlobalAutoBehaviour<AccountManager>.instance.UserInfo.Nick));
		PlayRecordCategory category = PlayRecordCategory.None;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(Tabs); i++)
		{
			TabInfo tabInfo = Tabs[i];
			if (tabInfo.Tab.startsActive)
			{
				category = tabInfo.Categoty;
				tabInfo.Tab.value = true;
				break;
			}
		}
		InitRecord(category);
	}

	private void InitRecord(PlayRecordCategory category)
	{
		ClearRecord();
		_currentCategory = category;
		_selectedCategory = category;
		foreach (PlayRecordData allDatum in GameRoot.Instance.PlayRecordDataContainer.GetAllData())
		{
			if (allDatum.Category != category)
			{
				continue;
			}
			if (!String_Extension_Method.IsNullOrEmpty(allDatum.SubCategoryTitle))
			{
				PlayRecordTitleComponent playRecordTitleComponent = PlayRecordGrid.AddGridItem(PlayRecordTitleComponent.PrefabAuto);
				if ((bool)playRecordTitleComponent)
				{
					playRecordTitleComponent.Open(allDatum.SubCategoryTitle);
					_playRecordComponentList.Add(playRecordTitleComponent);
				}
			}
			PlayRecordComponent playRecordComponent = PlayRecordGrid.AddGridItem(PlayRecordComponent.PrefabAuto);
			if (playRecordComponent == null)
			{
				VespaDebug.LogError("PlayRecordComponent comonent is null");
				continue;
			}
			PlayRecordInfo playRecordInfo = GlobalAutoBehaviour<AccountManager>.instance.PlayRecordManager.Get(allDatum.Index);
			playRecordComponent.Open(allDatum, playRecordInfo);
			_playRecordComponentList.Add(playRecordComponent);
		}
		PlayRecordGrid.RepositionGrid();
	}

	private void ClearRecord()
	{
		FactoryBase.ReleaseAndClearList(_playRecordComponentList);
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		if (_selectedCategory != _currentCategory)
		{
			InitRecord(_selectedCategory);
		}
	}

	public void ActivateTab()
	{
		PlayRecordCategory selectedCategory = PlayRecordCategory.General;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(Tabs); i++)
		{
			TabInfo tabInfo = Tabs[i];
			if (tabInfo.Tab.value)
			{
				selectedCategory = tabInfo.Categoty;
				break;
			}
		}
		_selectedCategory = selectedCategory;
	}

	void IUserObserver<PlayRecordInfo>.OnChanged(PlayRecordInfo changedInfo)
	{
	}

	bool IUserObserver.IsGlobal()
	{
		return false;
	}
}
