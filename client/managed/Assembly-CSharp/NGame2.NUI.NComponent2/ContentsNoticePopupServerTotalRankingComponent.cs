using NNguiExtension;
using NShared;
using NVespa.NGlobal;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class ContentsNoticePopupServerTotalRankingComponent : LinkFactoryComponent, InitOnCreate
{
	public const string PrefabPath = "Prefab2/Component/ContentsNotice/ContentsNoticePopupServerTotalRankingComponent";

	public static PrefabAuto<ContentsNoticePopupServerTotalRankingComponent> PrefabAuto = new PrefabAuto<ContentsNoticePopupServerTotalRankingComponent>("Prefab2/Component/ContentsNotice/ContentsNoticePopupServerTotalRankingComponent");

	public UILabel LabelServerName;

	public UILabel LabelScore;

	public UILabel RankLabelOther;

	public UISpriteSelector RankSpriteSelector;

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		Clear();
	}

	public bool SetData(ContentsNoticePopupServerTotalRankingInfo data)
	{
		Clear();
		if (data == null)
		{
			return false;
		}
		SetRank(data.Rank + 1, data.Score);
		string serverName = GameRoot.Instance.ServerGroupNameDataContainer.GetServerName(data.ServerGroup);
		if (!String_Extension_Method.IsNullOrEmpty(serverName))
		{
			string stringDirect = LocalizationManager.GetStringDirect("PVP_WINNINGPOINTCOUNT", Vespa_Long_Extension_Method.ToCommaString(data.Score));
			UIHelper.SetLabelText(LabelServerName, serverName);
			UIHelper.SetLabelText(LabelScore, stringDirect);
			return true;
		}
		return false;
	}

	private void SetRank(int rank, long score)
	{
		if (score <= 0)
		{
			UIHelper.EnableGameObject(RankSpriteSelector, enable: false);
			UIHelper.EnableGameObject(RankLabelOther, enable: true);
			UIHelper.SetLabelText(RankLabelOther, "-");
		}
		else if (rank <= 3)
		{
			if (RankSpriteSelector != null)
			{
				RankSpriteSelector.Select(rank.ToString());
			}
			UIHelper.EnableGameObject(RankSpriteSelector, enable: true);
			UIHelper.EnableGameObject(RankLabelOther, enable: false);
		}
		else
		{
			UIHelper.SetLabelText(RankLabelOther, rank);
			UIHelper.EnableGameObject(RankSpriteSelector, enable: false);
			UIHelper.EnableGameObject(RankLabelOther, enable: true);
		}
	}

	private void Clear()
	{
		UIHelper.SetLabelText(RankLabelOther, string.Empty);
		UIHelper.SetLabelText(LabelServerName, string.Empty);
		UIHelper.SetLabelText(LabelScore, string.Empty);
		UIHelper.EnableGameObject(RankSpriteSelector, enable: false);
	}
}
