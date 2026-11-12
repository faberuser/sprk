using System.Collections.Generic;
using NGame2.NGuild;
using NGame2.NUI.NComponent2;
using NGameAsset;
using NNguiExtension;
using NShared.GetGuildRaidGuildRanking;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow;

public class GuildRaidGuildRanking : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/GuildRaid/GuildRaidGuildRanking";

	public UIBaseGrid GuildRankingGrid;

	public UILabel txt_Title;

	public UILabel txt_NoneRanking;

	public UISpriteSelector MyRankSpriteSelector;

	public UITexture MyGuildMark;

	public UILabel MyGuildRankLabel;

	public UILabel MyGuildNameLabel;

	public UILabel MyGuildClearTimeLabel;

	private List<GuildRaidGuildRankingComponent> _guildRankingComponentList = new List<GuildRaidGuildRankingComponent>();

	private GuildRankingInfo[] _rankingInfos;

	public void Open(GuildRankingInfo[] rankingInfos)
	{
		if (rankingInfos != null)
		{
			_rankingInfos = rankingInfos;
			Open(Init, null);
		}
	}

	public override void Reset()
	{
		base.Reset();
		ClearRanker();
	}

	private void ClearRanker()
	{
		FactoryBase.ReleaseAndClearList(_guildRankingComponentList);
	}

	public void Init()
	{
		InitUI();
		InitMyGuildInfo();
		InitGuildRanking();
	}

	private void InitUI()
	{
		UIHelper.SetLabelText(txt_Title, "길드 종합 랭킹");
		UIHelper.SetLabelText(txt_NoneRanking, "랭킹에 등록된 길드가 없습니다.");
	}

	private void InitMyGuildInfo()
	{
		UIHelper.SetLabelText(MyGuildRankLabel, "-");
		UIHelper.SetLabelText(MyGuildClearTimeLabel, "-");
		UIHelper.SetLabelText(MyGuildNameLabel, GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info.Name);
		AssetTexture guildLogoIcon = UIHelper.GetGuildLogoIcon2(GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info.Logo);
		UIHelper.ResetPlaneAssetTextureAsync(this, MyGuildMark, guildLogoIcon);
	}

	private void InitGuildRanking()
	{
		ClearRanker();
		int num = 1;
		int num2 = _rankingInfos.Length;
		UIHelper.EnableGameObject(txt_NoneRanking.gameObject, num2 == 0);
		long id = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info.Id;
		for (int i = 0; i < num2; i++)
		{
			GuildRaidGuildRankingComponent guildRaidGuildRankingComponent = UIHelper.AddGridItem(GuildRankingGrid, GuildRaidGuildRankingComponent.PrefabAuto);
			guildRaidGuildRankingComponent.SetRankData(num, _rankingInfos[i]);
			_guildRankingComponentList.Add(guildRaidGuildRankingComponent);
			if (_rankingInfos[i].GuildId == id)
			{
				SetMyGuildInfo(num, _rankingInfos[i]);
			}
			num++;
		}
		GuildRankingGrid.RepositionGrid();
	}

	private void SetMyGuildInfo(int rank, GuildRankingInfo rankingInfo)
	{
		SetMyRank(rank);
		UILabel myGuildClearTimeLabel = MyGuildClearTimeLabel;
		int clearTime = rankingInfo.ClearTime;
		UIHelper.SetLabelText(myGuildClearTimeLabel, UIHelper.GetRemainTimeLabelText(clearTime));
	}

	private void SetMyRank(int rank)
	{
		if (rank <= 3)
		{
			if (MyRankSpriteSelector != null)
			{
				MyRankSpriteSelector.Select(rank.ToString());
			}
			UIHelper.EnableGameObject(MyRankSpriteSelector, enable: true);
			UIHelper.EnableGameObject(MyGuildRankLabel, enable: false);
		}
		else
		{
			UIHelper.SetLabelText(MyGuildRankLabel, rank);
			UIHelper.EnableGameObject(MyRankSpriteSelector, enable: false);
			UIHelper.EnableGameObject(MyGuildRankLabel, enable: true);
		}
	}

	public void OnClickClose()
	{
		Reset();
		base.Close();
	}
}
