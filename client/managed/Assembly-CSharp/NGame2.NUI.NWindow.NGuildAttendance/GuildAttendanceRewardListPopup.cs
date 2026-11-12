using System.Collections.Generic;
using NGame2.NUI.NComponent2.NGuildAttendance;
using NShared;
using NVespa.NPrefab2;

namespace NGame2.NUI.NWindow.NGuildAttendance;

public class GuildAttendanceRewardListPopup : BaseWindow
{
	public const string PrefabPath = "Prefab2/Window/Guild/GuildAttendanceRewardListPopup";

	public UIGrid RewardGrid;

	private List<GuildAttendanceRewardListComponent> _rewardListComponentList = new List<GuildAttendanceRewardListComponent>();

	public new void Open()
	{
		Open(delegate
		{
			Init();
		}, null);
	}

	public override void Close()
	{
		base.Close();
		ClearRewardListComponentList();
	}

	public override void OnSystemBackButton()
	{
		base.OnSystemBackButton();
		Close();
	}

	private void Init()
	{
		SetRewardListComponentList();
	}

	private void ClearRewardListComponentList()
	{
		FactoryBase.ReleaseAndClearList(_rewardListComponentList);
	}

	private void SetRewardListComponentList()
	{
		ClearRewardListComponentList();
		if (RewardGrid == null)
		{
			return;
		}
		List<GuildAttendanceRewardData> list = GameRoot.Instance.GuildAttendanceRewardDataContainer.GetList(EGuildAttendanceType.Perfect);
		if (__Extension_Method_ICollection__.IsNullOrEmpty(list))
		{
			return;
		}
		GuildAttendanceRewardListComponent guildAttendanceRewardListComponent = null;
		GuildAttendanceRewardData guildAttendanceRewardData = null;
		for (int i = 0; i < list.Count; i++)
		{
			guildAttendanceRewardData = list[i];
			if (guildAttendanceRewardData != null)
			{
				guildAttendanceRewardListComponent = GuildAttendanceRewardListComponent.PrefabAuto.Factory.Create(RewardGrid.transform);
				if (!(guildAttendanceRewardListComponent == null))
				{
					guildAttendanceRewardListComponent.Open(guildAttendanceRewardData);
					_rewardListComponentList.Add(guildAttendanceRewardListComponent);
				}
			}
		}
		UIHelper.ResetGridPosition(RewardGrid);
	}
}
