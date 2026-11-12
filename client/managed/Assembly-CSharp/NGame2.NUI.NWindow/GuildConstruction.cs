using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NGuild;
using NGame2.NUI.NComponent2;
using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class GuildConstruction : BaseWindow, BuildingComponent.Listener, ProgressComponent.Listener
{
	public enum ContentsType
	{
		Construction,
		WeekInfo
	}

	public interface Listener
	{
		void OnClickInvest(int buildingIndex, int wood, int stone, int metal, Action EndCallback);

		void OnClickLevelup(int buildingIndex, int level, Action EndCallback);
	}

	private class Resources
	{
		private Dictionary<GuildConstructionResource, int> _resourceContainer = new Dictionary<GuildConstructionResource, int>();

		public void SetResourceAmount(GuildConstructionResource type, int amount)
		{
			_resourceContainer[type] = amount;
		}

		public int GetResourceAmount(GuildConstructionResource type)
		{
			if (!_resourceContainer.ContainsKey(type))
			{
				return 0;
			}
			return _resourceContainer[type];
		}

		public void Clear()
		{
			_resourceContainer.Clear();
		}
	}

	public class ResourcesContainer
	{
		private Dictionary<int, Resources> _buildingInvestInfo = new Dictionary<int, Resources>();

		private bool _cacheInvestInfo;

		public void Reset(int buildingIndex)
		{
			if (_buildingInvestInfo.ContainsKey(buildingIndex))
			{
				_buildingInvestInfo[buildingIndex].Clear();
			}
		}

		public void OnChangeSelection()
		{
			if (!_cacheInvestInfo)
			{
				_buildingInvestInfo.Clear();
			}
		}

		public int Get(int buildingIndex, GuildConstructionResource type)
		{
			if (!_buildingInvestInfo.ContainsKey(buildingIndex))
			{
				return 0;
			}
			return _buildingInvestInfo[buildingIndex].GetResourceAmount(type);
		}

		public void Set(int buildingIndex, GuildConstructionResource type, int investValue)
		{
			if (!_buildingInvestInfo.ContainsKey(buildingIndex))
			{
				_buildingInvestInfo[buildingIndex] = new Resources();
			}
			_buildingInvestInfo[buildingIndex].SetResourceAmount(type, investValue);
		}
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Guild/GuildConstruction";

	public UIToggle Toggle_Construction;

	public UIToggle Toggle_WeekInfo;

	public Transform Root_Construction;

	public Transform Root_WeekInfo;

	public UILabel Label_NumMember;

	public UIButton Button_Invest;

	public UIButton Button_LevelUp;

	public UIButton Button_Reset;

	public UIScrollGrid Grid_ResourcePoints;

	public UIScrollGrid Grid_BuildingList;

	public UIScrollGrid Grid_ProgressBars;

	public UIScrollGrid Grid_ResourcePointsWeek;

	public UIScrollGrid Grid_GainInfos;

	private List<ResourcePointComponent> _resoucePointComponentList = new List<ResourcePointComponent>();

	private List<BuildingComponent> _buildingComponentList = new List<BuildingComponent>();

	private Dictionary<GuildConstructionResource, ProgressComponent> _progressComponentDic = new Dictionary<GuildConstructionResource, ProgressComponent>();

	private List<ResourcePointComponent> _resoucePointWeekComponentList = new List<ResourcePointComponent>();

	private List<GainInfoComponent> _gainInfoComponentList = new List<GainInfoComponent>();

	private GuildBuildingData _selectedBuildingData;

	private ResourcesContainer _investResourcesContainer = new ResourcesContainer();

	private UserGuildManager _userGuildManager;

	private Listener _listener;

	private ContentsType _contentsType;

	private Action _openComplete;

	public void Open(Listener listener, Action openComplete = null)
	{
		_listener = listener;
		_openComplete = openComplete;
		base.Open(OpenInternal);
	}

	private void OpenInternal()
	{
		if (!Init())
		{
			VespaDebug.LogError("GuildConstructionWindow Init Failed");
			Close();
		}
		else
		{
			Vespa_System_Action_Extension_Method.fire(_openComplete);
		}
	}

	private bool Init()
	{
		_userGuildManager = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager;
		if (_userGuildManager == null || _userGuildManager.Info == null)
		{
			VespaDebug.LogError("MyGuildManager.Info == null");
			return false;
		}
		_selectedBuildingData = null;
		Toggle_Construction.Set(state: true);
		SetNumGuildMember();
		SetContents(ContentsType.Construction);
		return true;
	}

	private void SetNumGuildMember()
	{
		if (GlobalAutoBehaviour<GuildManager>.instance.MemberManager != null)
		{
			int num = GlobalAutoBehaviour<GuildManager>.instance.MemberManager.Count();
			int num2 = GameRoot.Instance.GuildLevelDataContainer.GetData(GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info.Level)?.MaxMember ?? 0;
			UIHelper.SetLabelText(Label_NumMember, num + "/" + num2);
		}
	}

	private void SetContents(ContentsType contents)
	{
		_contentsType = contents;
		SetConstruction(_contentsType == ContentsType.Construction);
		SetWeekInfo(_contentsType == ContentsType.WeekInfo);
	}

	private void SetConstruction(bool show)
	{
		UIHelper.EnableGameObject(Root_Construction, show);
		if (show)
		{
			SetConstructionResource();
			SetBuildingList();
			InitProgressBar();
			SetProgressBar();
			SetButtonState();
		}
	}

	private void SetConstructionResource()
	{
		FactoryBase.ReleaseAndClearList(_resoucePointComponentList);
		foreach (object value in Enum.GetValues(typeof(GuildConstructionResource)))
		{
			ResourcePointComponent resourcePointComponent = UIHelper.AddGridItem(Grid_ResourcePoints, ResourcePointComponent.PrefabAuto);
			GuildConstructionResource guildConstructionResource = (GuildConstructionResource)value;
			resourcePointComponent.Open(guildConstructionResource, GetGuildConstructionResource(guildConstructionResource));
			_resoucePointComponentList.Add(resourcePointComponent);
		}
		UIHelper.RepositionGrid(Grid_ResourcePoints);
	}

	private int GetGuildConstructionResource(GuildConstructionResource resType)
	{
		return _userGuildManager.GetConstructionResource(resType);
	}

	private void SetBuildingList()
	{
		FactoryBase.ReleaseAndClearList(_buildingComponentList);
		IEnumerator<KeyValuePair<int, GuildBuildingData>> enumerator = CurrentBuildingList();
		while (enumerator.MoveNext())
		{
			BuildingComponent buildingComponent = UIHelper.AddGridItem(Grid_BuildingList, BuildingComponent.PrefabAuto);
			GuildBuildingData value = enumerator.Current.Value;
			buildingComponent.Open(this, value);
			_buildingComponentList.Add(buildingComponent);
			if (_selectedBuildingData == null)
			{
				_selectedBuildingData = buildingComponent.BuildingData;
			}
			bool flag = _selectedBuildingData.Index == buildingComponent.BuildingData.Index;
			if (flag)
			{
				_selectedBuildingData = buildingComponent.BuildingData;
			}
			buildingComponent.SetSelect(flag);
		}
		UIHelper.RepositionGrid(Grid_BuildingList);
	}

	private IEnumerator<KeyValuePair<int, GuildBuildingData>> CurrentBuildingList()
	{
		IList<GuildBuildingData> allData = GameRoot.Instance.GuildBuildingDataContainer.GetAllData();
		Dictionary<int, GuildBuildingData> dictionary = new Dictionary<int, GuildBuildingData>();
		foreach (GuildBuildingData item in allData)
		{
			if (item.ControlHeadQuarter && GetBuildingInfoLevel(item.Index) == item.Level)
			{
				dictionary[item.Index] = item;
			}
		}
		return dictionary.GetEnumerator();
	}

	private int GetBuildingInfoLevel(int buildingIndex)
	{
		return _userGuildManager.Info.GetGuildBuildingInfo(buildingIndex)?.BuildingLevel ?? 0;
	}

	private void InitProgressBar()
	{
		FactoryBase.ReleaseAndClearList(_progressComponentDic);
		bool hasAuthority = HasAuthority();
		foreach (object value in Enum.GetValues(typeof(GuildConstructionResource)))
		{
			ProgressComponent progressComponent = UIHelper.AddGridItem(Grid_ProgressBars, ProgressComponent.PrefabAuto);
			GuildConstructionResource guildConstructionResource = (GuildConstructionResource)value;
			progressComponent.Open(this, guildConstructionResource, hasAuthority);
			_progressComponentDic[guildConstructionResource] = progressComponent;
		}
		UIHelper.RepositionGrid(Grid_ProgressBars);
	}

	private void SetProgressBar()
	{
		if (_selectedBuildingData != null)
		{
			SetResourceProgress(GuildConstructionResource.Wood, GetBuildingInvestedInfo(_selectedBuildingData.Index, GuildConstructionResource.Wood), _selectedBuildingData.ReqWood, GetAddValue(GuildConstructionResource.Wood));
			SetResourceProgress(GuildConstructionResource.Stone, GetBuildingInvestedInfo(_selectedBuildingData.Index, GuildConstructionResource.Stone), _selectedBuildingData.ReqStone, GetAddValue(GuildConstructionResource.Stone));
			SetResourceProgress(GuildConstructionResource.Metal, GetBuildingInvestedInfo(_selectedBuildingData.Index, GuildConstructionResource.Metal), _selectedBuildingData.ReqMetal, GetAddValue(GuildConstructionResource.Metal));
		}
	}

	private int GetBuildingInvestedInfo(int buildingIndex, GuildConstructionResource res)
	{
		GuildBuildingInfo guildBuildingInfo = _userGuildManager.Info.GetGuildBuildingInfo(buildingIndex);
		if (guildBuildingInfo == null)
		{
			VespaDebug.LogError("info[" + buildingIndex + "]  == null");
			return 0;
		}
		int result = 0;
		switch (res)
		{
		case GuildConstructionResource.Wood:
			result = guildBuildingInfo.Wood;
			break;
		case GuildConstructionResource.Stone:
			result = guildBuildingInfo.Stone;
			break;
		case GuildConstructionResource.Metal:
			result = guildBuildingInfo.Metal;
			break;
		}
		return result;
	}

	private int GetAddValue(GuildConstructionResource res)
	{
		if (_selectedBuildingData == null)
		{
			return 0;
		}
		return _investResourcesContainer.Get(_selectedBuildingData.Index, res);
	}

	private void SetResourceProgress(GuildConstructionResource resource, int current, int max, int addValue)
	{
		if (_progressComponentDic.ContainsKey(resource))
		{
			_progressComponentDic[resource].SetProgress(current, max, addValue);
		}
	}

	private void SetButtonState()
	{
		bool flag = ActiveButton();
		bool flag2 = IsValidInvest();
		UIHelper.EnableButton(Button_Reset, flag && flag2, grayscaleIfDisable: true);
		UIHelper.EnableButton(Button_Invest, flag && flag2, grayscaleIfDisable: true);
		UIHelper.EnableButton(Button_LevelUp, flag, grayscaleIfDisable: true);
		if (!flag)
		{
			UIHelper.EnableGameObject(Button_LevelUp, enable: false);
			UIHelper.EnableGameObject(Button_Invest, enable: true);
		}
		else
		{
			bool flag3 = AllMaxInvest();
			UIHelper.EnableGameObject(Button_LevelUp, flag3);
			UIHelper.EnableGameObject(Button_Invest, !flag3);
		}
	}

	private bool ActiveButton()
	{
		if (_selectedBuildingData == null)
		{
			return false;
		}
		GuildBuildingInfo guildBuildingInfo = _userGuildManager.Info.GetGuildBuildingInfo(_selectedBuildingData.Index);
		if (guildBuildingInfo == null)
		{
			return false;
		}
		if (!GameRoot.Instance.GuildBuildingDataContainer.GetData(_selectedBuildingData.Index, guildBuildingInfo.BuildingLevel).CanUpgrade)
		{
			return false;
		}
		if (!HasAuthority())
		{
			return false;
		}
		return true;
	}

	private bool IsValidInvest()
	{
		if (_selectedBuildingData == null)
		{
			return false;
		}
		return _investResourcesContainer.Get(_selectedBuildingData.Index, GuildConstructionResource.Wood) + _investResourcesContainer.Get(_selectedBuildingData.Index, GuildConstructionResource.Stone) + _investResourcesContainer.Get(_selectedBuildingData.Index, GuildConstructionResource.Metal) > 0;
	}

	private bool AllMaxInvest()
	{
		if (_selectedBuildingData == null)
		{
			return false;
		}
		return IsMaxInvest(GuildConstructionResource.Wood, _selectedBuildingData.ReqWood) & IsMaxInvest(GuildConstructionResource.Stone, _selectedBuildingData.ReqStone) & IsMaxInvest(GuildConstructionResource.Metal, _selectedBuildingData.ReqMetal);
	}

	private bool IsMaxInvest(GuildConstructionResource type, int max)
	{
		return GetBuildingInvestedInfo(_selectedBuildingData.Index, type) >= max;
	}

	private bool HasAuthority()
	{
		if (GlobalAutoBehaviour<GuildManager>.instance.MemberManager == null)
		{
			VespaDebug.LogError("NGuild.GuildManager.instance.MemberManager == null");
			return false;
		}
		GuildMemberInfo myMemberInfo = GlobalAutoBehaviour<GuildManager>.instance.MemberManager.GetMyMemberInfo();
		if (myMemberInfo == null)
		{
			return false;
		}
		int guildConstructionReqRank = GetGuildConstructionReqRank();
		if (guildConstructionReqRank != 0)
		{
			return myMemberInfo.Rank <= guildConstructionReqRank;
		}
		return myMemberInfo.IsMaster();
	}

	private int GetGuildConstructionReqRank()
	{
		return GameRoot.Instance.ConstantDataContainer.GetValueInt("GuildConstructionReqRank");
	}

	private void SetWeekInfo(bool show)
	{
		UIHelper.EnableGameObject(Root_WeekInfo, show);
		if (show)
		{
			SetConstructonResourceWeek();
			SetMemberWeekInfos();
		}
	}

	private void SetConstructonResourceWeek()
	{
		FactoryBase.ReleaseAndClearList(_resoucePointWeekComponentList);
		foreach (GuildConstructionResource value in Enum.GetValues(typeof(GuildConstructionResource)))
		{
			ResourcePointComponent resourcePointComponent = UIHelper.AddGridItem(Grid_ResourcePointsWeek, ResourcePointComponent.PrefabAuto);
			resourcePointComponent.Open(value, GetGuildConstructionResource(value));
			_resoucePointWeekComponentList.Add(resourcePointComponent);
		}
		UIHelper.RepositionGrid(Grid_ResourcePointsWeek);
	}

	private void SetMemberWeekInfos()
	{
		FactoryBase.ReleaseAndClearList(_gainInfoComponentList);
		IEnumerator<GuildMemberInfo> enumerator = GlobalAutoBehaviour<GuildManager>.instance.MemberManager.GetEnumerator();
		while (enumerator.MoveNext())
		{
			GainInfoComponent gainInfoComponent = UIHelper.AddGridItem(Grid_GainInfos, GainInfoComponent.PrefabAuto);
			gainInfoComponent.Open(enumerator.Current);
			_gainInfoComponentList.Add(gainInfoComponent);
		}
		Grid_GainInfos.Grid.sorting = UIGrid.Sorting.Custom;
		Grid_GainInfos.Grid.onCustomSort = (Transform x, Transform y) => SortBase<GainInfoComponent>(x, y, SortManageMemberInfo);
		UIHelper.RepositionGrid(Grid_GainInfos);
	}

	private int SortManageMemberInfo(GainInfoComponent component1, GainInfoComponent component2)
	{
		GuildMemberInfo memberInfo = component1.MemberInfo;
		GuildMemberInfo memberInfo2 = component2.MemberInfo;
		if (memberInfo == null || memberInfo2 == null)
		{
			if (memberInfo != null)
			{
				return -1;
			}
			if (memberInfo2 != null)
			{
				return 1;
			}
			return 0;
		}
		int num = 0;
		if (GlobalAutoBehaviour<AccountManager>.instance.UserManager.Id == memberInfo.AccountId)
		{
			num = -1;
		}
		else if (GlobalAutoBehaviour<AccountManager>.instance.UserManager.Id == memberInfo2.AccountId)
		{
			num = 1;
		}
		if (num != 0)
		{
			return num;
		}
		num = memberInfo.Rank.CompareTo(memberInfo2.Rank);
		if (num != 0)
		{
			return num;
		}
		int num2 = memberInfo.AccMetal + memberInfo.AccStone + memberInfo.AccWood;
		int value = memberInfo2.AccMetal + memberInfo2.AccStone + memberInfo2.AccWood;
		return -num2.CompareTo(value);
	}

	private int SortBase<T>(Transform a, Transform b, Comparison<T> SortFunc)
	{
		T component = a.GetComponent<T>();
		T component2 = b.GetComponent<T>();
		if (component == null || component2 == null)
		{
			if (component != null)
			{
				return -1;
			}
			if (component2 != null)
			{
				return 1;
			}
			return 0;
		}
		return SortFunc(component, component2);
	}

	private string GetLocaleString(string key, params object[] args)
	{
		return LocalizationManager.GetStringDirect(key, args);
	}

	public void RefreshUI()
	{
		SetContents(_contentsType);
	}

	public void OnChangeToggle()
	{
		if (Toggle_Construction.value || Toggle_WeekInfo.value)
		{
			if (Toggle_Construction.value)
			{
				SetContents(ContentsType.Construction);
			}
			if (Toggle_WeekInfo.value)
			{
				SetContents(ContentsType.WeekInfo);
			}
		}
	}

	public void OnClickInvest()
	{
		if (_selectedBuildingData != null && _listener != null)
		{
			int index = _selectedBuildingData.Index;
			int num = _investResourcesContainer.Get(index, GuildConstructionResource.Wood);
			int num2 = _investResourcesContainer.Get(index, GuildConstructionResource.Stone);
			int num3 = _investResourcesContainer.Get(index, GuildConstructionResource.Metal);
			if (num + num2 + num3 != 0)
			{
				_investResourcesContainer.Reset(index);
				_listener.OnClickInvest(_selectedBuildingData.Index, num, num2, num3, delegate
				{
				});
			}
		}
	}

	public void OnClickLevelup()
	{
		if (_listener != null && CanLevelup())
		{
			int index = _selectedBuildingData.Index;
			int preLevel = _selectedBuildingData.Level;
			int level = _selectedBuildingData.Level + 1;
			_listener.OnClickLevelup(index, level, delegate
			{
				GuildBuildingLevelupPopup.Open(index, preLevel, level);
			});
		}
	}

	private bool CanLevelup()
	{
		if (_selectedBuildingData == null)
		{
			return false;
		}
		if (!_selectedBuildingData.CanUpgrade)
		{
			return false;
		}
		if (_userGuildManager.Info.Level < _selectedBuildingData.ReqGuildLevel)
		{
			string localeString = GetLocaleString("GUILD_BUILDING_LEVELUP_FAIL_POPUP", _selectedBuildingData.ReqGuildLevel);
			MessagePopup.Open(LocalizationManager.GetString(NShared.NLocalization.Common.INFORM), localeString);
			return false;
		}
		return true;
	}

	public void OnClickReset()
	{
		if (_selectedBuildingData != null)
		{
			_investResourcesContainer.Reset(_selectedBuildingData.Index);
			SetProgressBar();
			SetButtonState();
		}
	}

	void BuildingComponent.Listener.OnSelectBuilding(BuildingComponent caller)
	{
		if (_selectedBuildingData != null && _selectedBuildingData.Index != caller.BuildingData.Index)
		{
			_investResourcesContainer.OnChangeSelection();
		}
		_buildingComponentList.ForEach(delegate(BuildingComponent e)
		{
			e.SetSelect(select: false);
		});
		caller.SetSelect(select: true);
		_selectedBuildingData = caller.BuildingData;
		SetProgressBar();
		SetButtonState();
	}

	void ProgressComponent.Listener.OnInputValue(GuildConstructionResource resType, int investValue)
	{
		if (_selectedBuildingData != null)
		{
			investValue = System.Math.Min(investValue, GetGuildConstructionResource(resType));
			_investResourcesContainer.Set(_selectedBuildingData.Index, resType, investValue);
			SetProgressBar();
			SetButtonState();
		}
	}
}
