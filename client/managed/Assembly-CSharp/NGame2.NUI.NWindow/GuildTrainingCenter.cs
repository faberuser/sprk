using System;
using System.Collections.Generic;
using NGame2.NGuild;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class GuildTrainingCenter : BaseWindow, GuildSkillLevelComponent.Listener
{
	public interface Listener
	{
		void InitGuildSkillLevel();

		void LevelUpSkill(int SkillIndex, byte SkillLevel);

		void OnClickVote(int skillIndex);
	}

	[Serializable]
	public class TabInfo
	{
		[HideInInspector]
		public UIToggle Toggle;

		public string LocalString;

		public CreatureTagType CreatureTagType;

		public bool StartingState;
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Guild/GuildTrainingCenter";

	private Listener _listener;

	public List<TabInfo> TabInfoList = new List<TabInfo>();

	public UILabel Label_GuildPoint;

	public UILabel Label_SupperessPoint;

	public UILabel Label_Level;

	public UILabel Label_InitSkill;

	public UIToggle Toggle_Form;

	public UIButton Button_InitSkill;

	public UIGrid ToggleGrid;

	public UIScrollGrid Grid_Skill;

	private List<GuildSkillLevelComponent> skillComponentList = new List<GuildSkillLevelComponent>();

	private bool _skillResetCoolElapsed;

	private float _lastUpdateSkillResetTime;

	private TabInfo _currentTab;

	public void Init(Listener listener)
	{
		Reset();
		_listener = listener;
		InitCommonSkillTab();
		InitLeftUI();
		SetGuildSkillResetRemainTime();
	}

	public override void Awake()
	{
		base.Awake();
		_lastUpdateSkillResetTime = Time.realtimeSinceStartup;
	}

	public override void Reset()
	{
		base.Reset();
		FactoryBase.ReleaseAndClearList(skillComponentList);
	}

	public void InitCommonSkillTab()
	{
		if (_currentTab == null)
		{
			TabInfo tabInfo = new TabInfo();
			tabInfo.CreatureTagType = CreatureTagType.None;
			tabInfo.LocalString = string.Empty;
			_currentTab = tabInfo;
		}
		InitGuildSkillList();
	}

	public void InitTabs()
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(TabInfoList))
		{
			return;
		}
		foreach (TabInfo tabInfo in TabInfoList)
		{
			if (tabInfo.Toggle == null)
			{
				tabInfo.Toggle = UnityEngine.Object.Instantiate(Toggle_Form);
				Vespa_Unity_Transform_Extension_Method.ParentResetTM(tabInfo.Toggle.gameObject.transform, ToggleGrid.transform);
				tabInfo.Toggle.GetComponentInChildren<UILabel>().text = LocalizationManager.GetStringDirect(tabInfo.LocalString);
				if (tabInfo.StartingState)
				{
					tabInfo.Toggle.startsActive = tabInfo.StartingState;
					_currentTab = tabInfo;
				}
			}
		}
		ToggleGrid.Reposition();
		InitGuildSkillList();
	}

	public void InitLeftUI()
	{
		UIHelper.SetLabelText(Label_GuildPoint, GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info.ActivityPoint);
		UIHelper.SetLabelText(Label_SupperessPoint, GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info.SuppressPoint);
		if (GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info != null)
		{
			GuildBuildingInfo guildBuildingInfo = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info.GetGuildBuildingInfo(EGuildBuildingType.TraningCenter);
			if (guildBuildingInfo != null)
			{
				int buildingLevel = guildBuildingInfo.BuildingLevel;
				string stringDirect = LocalizationManager.GetStringDirect("GUILD_TRAINING_CENTER_LEVEL", buildingLevel);
				UIHelper.SetLabelText(Label_Level, stringDirect);
			}
		}
	}

	public void RefreshWindow(bool initGuildSkillList)
	{
		InitLeftUI();
		if (initGuildSkillList)
		{
			InitGuildSkillList();
		}
		else
		{
			RefreshGuildSkillComponents();
		}
	}

	public void InitGuildSkillList()
	{
		FactoryBase.ReleaseAndClearList(skillComponentList);
		Grid_Skill.RepositionGrid();
		if (_currentTab == null)
		{
			return;
		}
		_ = _currentTab.CreatureTagType;
		IList<GuildSkillData> allData = GameRoot.Instance.GuildSkillDataContainer.GetAllData();
		if (__Extension_Method_ICollection__.IsNullOrEmpty(allData))
		{
			return;
		}
		foreach (GuildSkillData item in allData)
		{
			int guildSkillLevel = 1;
			GuildSkillInfo guildSkillInfo = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info.GetGuildSkillInfo(item.GuildSkillIndex);
			if (guildSkillInfo != null)
			{
				guildSkillLevel = guildSkillInfo.SkillLevel;
			}
			GuildSkillLevelData data = GameRoot.Instance.GuildSkillLevelDataContainer.GetData(item.GuildSkillIndex, guildSkillLevel);
			if (data != null)
			{
				GuildSkillLevelComponent guildSkillLevelComponent = UIHelper.AddGridItem(Grid_Skill, GuildSkillLevelComponent.PrefabAuto);
				guildSkillLevelComponent.Open(this, item, data, guildSkillInfo);
				skillComponentList.Add(guildSkillLevelComponent);
			}
		}
		Grid_Skill.RepositionGrid();
	}

	public void RefreshGuildSkillComponents()
	{
		foreach (GuildSkillLevelComponent skillComponent in skillComponentList)
		{
			int guildSkillLevel = 1;
			GuildSkillInfo guildSkillInfo = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info.GetGuildSkillInfo(skillComponent.GuildSkillIndex);
			if (guildSkillInfo != null)
			{
				guildSkillLevel = guildSkillInfo.SkillLevel;
			}
			GuildSkillLevelData data = GameRoot.Instance.GuildSkillLevelDataContainer.GetData(guildSkillInfo.SkillIndex, guildSkillLevel);
			if (data != null)
			{
				skillComponent.RefreshComponent(guildSkillInfo, data);
			}
		}
	}

	public void OnChangeToggle()
	{
		TabInfo tabInfo = null;
		foreach (TabInfo tabInfo2 in TabInfoList)
		{
			UIToggle toggle = tabInfo2.Toggle;
			if (!(toggle == null) && toggle.value)
			{
				tabInfo = tabInfo2;
				break;
			}
		}
		if (tabInfo != null && tabInfo != _currentTab)
		{
			_currentTab = tabInfo;
			InitGuildSkillList();
		}
	}

	public void OnClickInitGuildSkillLevel()
	{
		_listener.InitGuildSkillLevel();
	}

	void GuildSkillLevelComponent.Listener.OnClickLevelUp(GuildSkillLevelData skillLevelData)
	{
		_listener.LevelUpSkill(skillLevelData.GuildSkillIndex, (byte)(skillLevelData.GuildSkillLevel + 1));
	}

	void GuildSkillLevelComponent.Listener.OnClickVote(int skillIndex)
	{
		_listener.OnClickVote(skillIndex);
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		if ((double)(Time.realtimeSinceStartup - _lastUpdateSkillResetTime) > 0.5)
		{
			_lastUpdateSkillResetTime = Time.realtimeSinceStartup;
			SetGuildSkillResetRemainTime();
		}
	}

	private void SetGuildSkillResetRemainTime()
	{
		_skillResetCoolElapsed = true;
		if (!String_Extension_Method.IsNullOrEmpty(GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info.SkillInitTime))
		{
			int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("InitGuildSkillCoolTime");
			Vespa_String_Extension_Method2.TryParseNetworkTime2(GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info.SkillInitTime, out var o_dateTime);
			DateTime uTCNow = NGame2.NUI.NWindow.Common.GetUTCNow();
			TimeSpan timeSpan = o_dateTime.AddMinutes(valueInt) - uTCNow;
			if (timeSpan.TotalSeconds > 0.0)
			{
				_skillResetCoolElapsed = false;
				int days = timeSpan.Days;
				int hours = timeSpan.Hours;
				int minutes = timeSpan.Minutes;
				UIHelper.SetLabelText(Label_InitSkill, LocalizationManager.GetStringDirect("REMAIN_TIME_DAY_HOUR_MINUTE", days, hours, minutes));
			}
		}
		if (_skillResetCoolElapsed)
		{
			UIHelper.SetLabelText(Label_InitSkill, LocalizationManager.GetStringDirect("GUILD_RESET_SKILL"));
		}
		GuildMemberInfo myMemberInfo = GlobalAutoBehaviour<GuildManager>.instance.MemberManager.GetMyMemberInfo();
		if (myMemberInfo != null)
		{
			if (_skillResetCoolElapsed && myMemberInfo.Rank == 1)
			{
				UIHelper.EnableButton(Button_InitSkill, enable: true);
			}
			else
			{
				UIHelper.EnableButton(Button_InitSkill, enable: false);
			}
		}
	}
}
