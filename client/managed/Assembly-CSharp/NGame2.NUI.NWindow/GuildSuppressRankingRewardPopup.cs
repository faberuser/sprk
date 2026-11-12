using System.Collections.Generic;
using NGame2.NScene.NPreloader;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;

namespace NGame2.NUI.NWindow;

public class GuildSuppressRankingRewardPopup : BaseWindow
{
	public enum EToggle
	{
		None,
		Repeat,
		ServerSeason,
		Global,
		TotalRank
	}

	public const string PrefabPath = "Resources/Prefab2/Window/GuildSuppress/GuildSuppressRankingRewardPopup";

	public UIScrollGrid RewardGrid;

	public UIToggle ToggleRepeat;

	public UIToggle ToggleServerSeason;

	public UIToggle ToggleGlobal;

	public UIToggle ToggleTotalRank;

	public UILabel Label_Desc;

	private List<GuildSuppressRankingRewardComponent> _componentList;

	private EToggle _toggle;

	public new void Open()
	{
		Open(Init, null);
	}

	public void Init()
	{
		InitToggle(EToggle.Repeat);
	}

	public override void ReserveCache(UiPreloader preloader)
	{
		base.ReserveCache(preloader);
	}

	public override void Reset()
	{
		base.Reset();
		InitToggle(EToggle.None);
		_toggle = EToggle.None;
		ClearRewardList();
	}

	public void OnClickClose()
	{
		Close();
	}

	private void InitReward(IList<GuildSuppressRewardData> datas)
	{
		ClearRewardList();
		if (_componentList == null)
		{
			_componentList = new List<GuildSuppressRankingRewardComponent>();
		}
		foreach (GuildSuppressRewardData data in datas)
		{
			GuildSuppressRankingRewardComponent guildSuppressRankingRewardComponent = UIHelper.AddGridItem(RewardGrid, GuildSuppressRankingRewardComponent.PrefabAuto);
			guildSuppressRankingRewardComponent.SetData(data);
			_componentList.Add(guildSuppressRankingRewardComponent);
		}
		UIHelper.RepositionGrid(RewardGrid);
	}

	private void ClearRewardList()
	{
		FactoryBase.ReleaseAndClearList(_componentList);
	}

	private void InitToggle(EToggle startToggle)
	{
		ToggleRepeat.Set(startToggle == EToggle.Repeat);
		ToggleServerSeason.Set(startToggle == EToggle.ServerSeason);
		ToggleGlobal.Set(startToggle == EToggle.Global);
		ToggleTotalRank.Set(startToggle == EToggle.TotalRank);
	}

	public void OnToggleChanged()
	{
		EToggle eToggle = EToggle.None;
		if (ToggleRepeat.value)
		{
			eToggle = EToggle.Repeat;
		}
		else if (ToggleServerSeason.value)
		{
			eToggle = EToggle.ServerSeason;
		}
		else if (ToggleGlobal.value)
		{
			eToggle = EToggle.Global;
		}
		else if (ToggleTotalRank.value)
		{
			eToggle = EToggle.TotalRank;
		}
		if (eToggle != EToggle.None && _toggle != eToggle)
		{
			_toggle = eToggle;
			if (_toggle == EToggle.Repeat)
			{
				ShowRepeatReward();
			}
			else if (_toggle == EToggle.ServerSeason)
			{
				ShowServerSeasonReward();
			}
			else if (_toggle == EToggle.Global)
			{
				ShowGlobalReward();
			}
			else if (_toggle == EToggle.TotalRank)
			{
				ShowTotalRankReward();
			}
		}
	}

	private void ShowRepeatReward()
	{
		InitReward(GameRoot.Instance.GuildSuppressRewardDataContainer.GetAllData());
		string stringDirect = LocalizationManager.GetStringDirect("PUNITIVE_EXPEDITION_REPEAT_RANK_REWARD_DESC");
		SetRewardDesc(stringDirect);
	}

	private void ShowServerSeasonReward()
	{
		InitReward(GameRoot.Instance.GuildSuppressServerRewardDataContainer.GetAllData());
		string stringDirect = LocalizationManager.GetStringDirect("PUNITIVE_EXPEDITION_SEASON_SERVER_RANK_REWARD_DESC");
		SetRewardDesc(stringDirect);
	}

	private void ShowGlobalReward()
	{
		InitReward(GameRoot.Instance.GuildSuppressGlobalRewardDataContainer.GetAllData());
		string stringDirect = LocalizationManager.GetStringDirect("PUNITIVE_EXPEDITION_SEASON_WORLD_RANK_REWARD_DESC");
		SetRewardDesc(stringDirect);
	}

	private void ShowTotalRankReward()
	{
		InitReward(GameRoot.Instance.GuildSuppressTotalRewardDataContainer.GetAllData());
		string stringDirect = LocalizationManager.GetStringDirect("PUNITIVE_EXPEDITION_WORLD_TOTAL_RANK_REWARD_DESC");
		SetRewardDesc(stringDirect);
	}

	private void SetRewardDesc(string msg)
	{
		UIHelper.SetLabelText(Label_Desc, msg);
	}
}
