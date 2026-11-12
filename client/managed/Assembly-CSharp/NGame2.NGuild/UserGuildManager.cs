using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NWindow;
using NShared;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NGuild;

public class UserGuildManager
{
	public class GuildChangeInfo
	{
		public long ActivityPoint;

		public long SuppressPoint;
	}

	public class ConstructionResourceInfo
	{
		public int Wood;

		public int Stone;

		public int Metal;
	}

	private readonly UserObserverManager<GuildChangeInfo> _observerManager = new UserObserverManager<GuildChangeInfo>();

	private readonly GuildRaidManager _guildRaidManager = new GuildRaidManager();

	private readonly GuildSuppressManager _guildSuppressManager = new GuildSuppressManager();

	private readonly GuildBuildingManager _guildBuildingManager = new GuildBuildingManager();

	private readonly GuildArenaManager _guildArenaManager = new GuildArenaManager();

	public UserObserverManager<ConstructionResourceInfo> ResourceObserverManager = new UserObserverManager<ConstructionResourceInfo>();

	public MyGuildInfo Info { get; private set; }

	public GuildRaidManager GuildRaidManager => _guildRaidManager;

	public GuildSuppressManager GuildSuppressManager => _guildSuppressManager;

	public GuildBuildingManager GuildBuildingManager => _guildBuildingManager;

	public GuildArenaManager GuildArenaManager => _guildArenaManager;

	public IUserObserverManager ObserverManager => _observerManager;

	public UserGuildManager()
	{
		GlobalAutoBehaviour<GuiManager>.instance.Add("GuildInfo", this, delegate
		{
			if (Info == null)
			{
				GUILayout.Label("No Guild");
			}
			else
			{
				GUILayout.Label("Id : " + Info.Id + " Level : " + Info.Level + " Logo Index : " + Info.Logo);
				GUILayout.Label("ReqTeamLevel : " + Info.ReqTeamLevel + " Joinway : " + Info.JoinWay);
				GUILayout.Label("ActivityPoint : " + Info.ActivityPoint + " ContributedGem : " + Info.ContributedGem);
				GUILayout.Label("CurMember : " + Info.CurMember);
				GUILayout.Label("Skill1Level : " + Info.Skill1Level + " Skill2Level : " + Info.Skill2Level + " Skill3Level : " + Info.Skill3Level + "Skill4Level : " + Info.Skill4Level + " Skill5Level : " + Info.Skill5Level);
				GUILayout.Label("SkillInitTime : " + Info.SkillInitTime);
			}
		});
	}

	public void Set(MyGuildInfo info)
	{
		Info = info;
		GuildBuildingManager.SetMyGuildInfo(info);
		NotifyConstructionResource(info);
	}

	private void NotifyConstructionResource(MyGuildInfo info)
	{
		if (info != null)
		{
			ResourceObserverManager.AnnounceToObservers(new ConstructionResourceInfo
			{
				Wood = info.Wood,
				Stone = info.Stone,
				Metal = info.Metal
			});
		}
	}

	public void SetConstructionResource(int wood, int stone, int metal)
	{
		if (Info == null)
		{
			RestoredPortal.RefreshGuildConstruction();
			return;
		}
		if (Info.Wood != wood)
		{
			Info.Wood = wood;
		}
		if (Info.Stone != stone)
		{
			Info.Stone = stone;
		}
		if (Info.Metal != metal)
		{
			Info.Metal = metal;
		}
		NotifyConstructionResource(Info);
		RestoredPortal.RefreshGuildConstruction();
	}

	public void SetBuildingLevel(int buildingIndex, byte level, string updateTime)
	{
		if (Info != null)
		{
			GuildBuildingInfo guildBuildingInfo = Info.GetGuildBuildingInfo(buildingIndex);
			if (guildBuildingInfo != null)
			{
				guildBuildingInfo.BuildingLevel = level;
				guildBuildingInfo.UpdatedTime = updateTime;
				guildBuildingInfo.Wood = 0;
				guildBuildingInfo.Stone = 0;
				guildBuildingInfo.Metal = 0;
				guildBuildingInfo.ReservedUpdate = true;
			}
		}
	}

	public void SetBuildingInvest(int buildingIndex, int wood, int stone, int metal)
	{
		if (Info != null)
		{
			GuildBuildingInfo guildBuildingInfo = Info.GetGuildBuildingInfo(buildingIndex);
			if (guildBuildingInfo != null)
			{
				guildBuildingInfo.Wood = wood;
				guildBuildingInfo.Stone = stone;
				guildBuildingInfo.Metal = metal;
			}
		}
	}

	public void BuildingUpgradeApply(int buildingIndex, int level)
	{
		Action<int> applyAction = GuildBuildingManager.GetApplyAction(buildingIndex);
		if (applyAction != null)
		{
			Vespa_System_Action_Extension_Method.fire(applyAction, level);
		}
	}

	public void RegistBuildingApplyAction(EGuildBuildingType buildingType, Action<int> applyAction)
	{
		GuildBuildingManager.RegistApplyAction(buildingType, applyAction);
	}

	public void GetReservedUpgradeBuildingInfo(string lastGuildOpen)
	{
		if (Info == null || Info.GuildBuildingInfos == null || !Vespa_String_Extension_Method2.TryParseNetworkTime2(lastGuildOpen, out var o_dateTime))
		{
			return;
		}
		int num = -1;
		int num2 = 0;
		for (int i = 0; i < Info.GuildBuildingInfos.Length; i++)
		{
			GuildBuildingInfo guildBuildingInfo = Info.GuildBuildingInfos[i];
			if (Vespa_String_Extension_Method2.TryParseNetworkTime2(guildBuildingInfo.UpdatedTime, out var o_dateTime2))
			{
				int num3 = (int)(o_dateTime2 - o_dateTime).TotalSeconds;
				guildBuildingInfo.ReservedUpdate = false;
				if (num3 > 0 && num3 > num2)
				{
					num = i;
					num2 = num3;
				}
			}
		}
		if (num != -1)
		{
			Info.GuildBuildingInfos[num].ReservedUpdate = true;
		}
	}

	public long GetGuildId()
	{
		if (Info == null)
		{
			return 0L;
		}
		return Info.Id;
	}

	public void SetName(string name)
	{
		if (Info != null)
		{
			Info.Name = name;
		}
	}

	public void SetSuppressPoint(GuildSuppressPointResultInfo result)
	{
		if (result != null)
		{
			SetSuppressPoint(result.NewValue);
		}
	}

	public void SetSuppressPoint(int suppressPoint)
	{
		if (suppressPoint != Info.SuppressPoint)
		{
			Info.SuppressPoint = suppressPoint;
			_observerManager.AnnounceToObservers(new GuildChangeInfo
			{
				SuppressPoint = suppressPoint
			});
		}
	}

	public void SetBuildingInfo(GuildBuildingInfo[] buildingInfos)
	{
		if (Info != null && __Extension_Method_ICollection__.COUNT(buildingInfos) != 0)
		{
			Info.SetGuildBuildingInfo(buildingInfos);
		}
	}

	public void SetBuildingInfo(GuildBuildingInfo buildingInfo)
	{
		if (Info != null && buildingInfo != null)
		{
			Info.SetGuildBuildingInfo(buildingInfo);
		}
	}

	public void SetActivityPoint(GuildActivityPointResultInfo result)
	{
		if (result != null)
		{
			SetActivityPoint(result.NewValue);
		}
	}

	public void SetActivityPoint(int activityPoint)
	{
		if (activityPoint != Info.ActivityPoint)
		{
			Info.ActivityPoint = activityPoint;
			_observerManager.AnnounceToObservers(new GuildChangeInfo
			{
				ActivityPoint = activityPoint
			});
		}
	}

	public void IncActivityPoint(int incActivityPoint)
	{
		if (Info.ActivityPoint + incActivityPoint > 0)
		{
			Info.ActivityPoint += incActivityPoint;
			_observerManager.AnnounceToObservers(new GuildChangeInfo
			{
				ActivityPoint = Info.ActivityPoint
			});
		}
	}

	public void InitGuildSkillLevel(int activityPoint, int supperssPoint)
	{
		Info.Skill1Level = 1;
		Info.Skill2Level = 1;
		Info.Skill3Level = 1;
		Info.Skill4Level = 1;
		Info.Skill5Level = 1;
		Info.Skill6Level = 1;
		Info.ResetSkillLevel();
		SetActivityPoint(activityPoint);
		SetSuppressPoint(supperssPoint);
		Info.SkillInitTime = Vespa_DateTime_Extension_Method.ToNetworkString2(Common.GetUTCNow());
	}

	public int GetGuildSkillReq(int guildSkillIndex)
	{
		return guildSkillIndex switch
		{
			1 => Info.Skill1Req, 
			2 => Info.Skill2Req, 
			3 => Info.Skill3Req, 
			4 => Info.Skill4Req, 
			5 => Info.Skill5Req, 
			6 => Info.Skill6Req, 
			_ => 0, 
		};
	}

	public void AddGuildSkillReq(int guildSkillIndex, int add)
	{
		switch (guildSkillIndex)
		{
		case 1:
			Info.Skill1Req = (byte)Mathf.Clamp(Info.Skill1Req + add, 0, 255);
			break;
		case 2:
			Info.Skill2Req = (byte)Mathf.Clamp(Info.Skill2Req + add, 0, 255);
			break;
		case 3:
			Info.Skill3Req = (byte)Mathf.Clamp(Info.Skill3Req + add, 0, 255);
			break;
		case 4:
			Info.Skill4Req = (byte)Mathf.Clamp(Info.Skill4Req + add, 0, 255);
			break;
		case 5:
			Info.Skill5Req = (byte)Mathf.Clamp(Info.Skill5Req + add, 0, 255);
			break;
		case 6:
			Info.Skill6Req = (byte)Mathf.Clamp(Info.Skill6Req + add, 0, 255);
			break;
		}
		Info.AddGuildSkillReq(guildSkillIndex, add);
	}

	public int GetGuildSkillLevel(int GuildSkillIndex)
	{
		return GuildSkillIndex switch
		{
			1 => Info.Skill1Level, 
			2 => Info.Skill2Level, 
			3 => Info.Skill3Level, 
			4 => Info.Skill4Level, 
			5 => Info.Skill5Level, 
			6 => Info.Skill6Level, 
			_ => 0, 
		};
	}

	public GuildSkillLevelData GetGuildSkillLevelData(int GuildSkillIndex)
	{
		if (Info == null)
		{
			return null;
		}
		GuildSkillInfo guildSkillInfo = Info.GetGuildSkillInfo(GuildSkillIndex);
		if (guildSkillInfo == null)
		{
			return null;
		}
		return GameRoot.Instance.GuildSkillLevelDataContainer.GetData(GuildSkillIndex, guildSkillInfo.SkillLevel);
	}

	public List<GuildSkillLevelData> GetGuildSkillLevelDataList()
	{
		List<GuildSkillLevelData> list = new List<GuildSkillLevelData>();
		if (Info == null)
		{
			return list;
		}
		int[] allGuildSkillIndices = GameRoot.Instance.GuildSkillDataContainer.GetAllGuildSkillIndices();
		foreach (int guildSkillIndex in allGuildSkillIndices)
		{
			GuildSkillLevelData guildSkillLevelData = GetGuildSkillLevelData(guildSkillIndex);
			if (guildSkillLevelData != null)
			{
				list.Add(guildSkillLevelData);
			}
		}
		return list;
	}

	public int GetConstructionResource(GuildConstructionResource resourceType)
	{
		if (Info == null)
		{
			return 0;
		}
		return resourceType switch
		{
			GuildConstructionResource.Wood => Info.Wood, 
			GuildConstructionResource.Stone => Info.Stone, 
			GuildConstructionResource.Metal => Info.Metal, 
			_ => 0, 
		};
	}
}
