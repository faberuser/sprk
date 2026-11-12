using NGame2.NAccount;
using NGame2.NGuild;
using NGameAsset;
using NNguiExtension;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class GuildSuppressRankerComponent : UIScrollItemBehaviour, NVespa.NPrefab2.InitOnCreate
{
	public interface Listener
	{
		void OnClickComponent(long accountId);
	}

	public const string PrefabPath = "Prefab2/Component/GuildSuppress/GuildSuppressRankerComponent";

	public static PrefabAuto<GuildSuppressRankerComponent> PrefabAuto = new PrefabAuto<GuildSuppressRankerComponent>("Prefab2/Component/GuildSuppress/GuildSuppressRankerComponent");

	public UITexture Texture_Portrait;

	public UITexture Texture_CountryFlag;

	public UILabel RankLabel_Other;

	public UILabel RankLabel_Me;

	public UISpriteSelector RankSpriteSelector;

	public UILabel Label_MemberName;

	public UILabel Label_GuildName;

	public UILabel Label_Point;

	public UILabel Label_Server;

	public UILabel LabelDisapperGuild;

	private GuildSuppressGroupRankerInfo _groupRankerInfo;

	private GuildSuppressMemberScoreInfo _memberRankerInfo;

	public Transform MemberNameRoot;

	public Transform GuildNameRoot;

	private Listener _Listener;

	void NVespa.NPrefab2.InitOnCreate.OnCreate()
	{
	}

	void NVespa.NPrefab2.InitOnCreate.OnRelease()
	{
		ClearRankerInfo();
	}

	public bool SetGuildRankingData(GuildSuppressGroupRankerInfo groupRankerInfo, Listener listener)
	{
		if (groupRankerInfo == null)
		{
			return false;
		}
		_groupRankerInfo = groupRankerInfo;
		_Listener = listener;
		InitRankerInfo(isMemberRanking: false);
		SetGuildLogo(groupRankerInfo.GuildLogo);
		SetServerGroupName(groupRankerInfo.ServerGroup);
		SetCountryFlag(groupRankerInfo.CountryCode);
		bool isMe = groupRankerInfo.GuildId == GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info.Id;
		SetGuildRank(groupRankerInfo.Rank, isMe);
		UIHelper.SetLabelText(Label_GuildName, groupRankerInfo.GuildName);
		UIHelper.SetLabelText(Label_Point, groupRankerInfo.Score.ToString("#,0"));
		if (!CheckAvailableGuild(groupRankerInfo))
		{
			UIHelper.EnableGameObject(LabelDisapperGuild, enable: true);
			UIHelper.EnableGameObject(GuildNameRoot, enable: false);
		}
		return true;
	}

	public bool SetGuildMemberRankingData(GuildSuppressMemberScoreInfo memberRankerInfo, int rank, Listener listener)
	{
		if (memberRankerInfo == null)
		{
			return false;
		}
		NGame2.NGuild.GuildMemberManager memberManager = GlobalAutoBehaviour<GuildManager>.instance.MemberManager;
		if (memberManager == null)
		{
			return false;
		}
		GuildMemberInfo guildMemberInfo = memberManager.Get(memberRankerInfo.AccountId);
		if (guildMemberInfo == null)
		{
			return false;
		}
		_memberRankerInfo = memberRankerInfo;
		_Listener = listener;
		InitRankerInfo(isMemberRanking: true);
		SetHeroPortrait(guildMemberInfo.AvatarHeroIndex);
		bool isMe = memberRankerInfo.AccountId == GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AccountId;
		SetRank(rank + 1, isMe);
		UIHelper.SetLabelText(Label_MemberName, guildMemberInfo.Nick);
		UIHelper.SetLabelText(Label_Point, memberRankerInfo.Score.ToString("#,0"));
		return true;
	}

	private void SetGuildLogo(int logoIndex)
	{
		AssetTexture guildLogoIcon = UIHelper.GetGuildLogoIcon2((logoIndex <= 0) ? 1 : logoIndex);
		UIHelper.ResetPlaneAssetTextureAsync(this, Texture_Portrait, guildLogoIcon);
	}

	private void SetHeroPortrait(int heroIndex)
	{
		string iconPath = UIAvatarHelper.GetIconPath(heroIndex);
		if (!String_Extension_Method.IsNullOrEmpty(iconPath))
		{
			AssetTexture plainTexture = UIHelper.GetPlainTexture(iconPath);
			UIHelper.ResetPlaneAssetTextureAsync(this, Texture_Portrait, plainTexture);
		}
	}

	private void SetGuildRank(int rank, bool isMe)
	{
		if (rank == -1)
		{
			if (isMe)
			{
				UIHelper.SetLabelText(RankLabel_Me, "-");
			}
			else
			{
				UIHelper.SetLabelText(RankLabel_Other, "-");
			}
			UIHelper.EnableGameObject(RankSpriteSelector, enable: false);
			UIHelper.EnableGameObject(RankLabel_Me, isMe);
			UIHelper.EnableGameObject(RankLabel_Other, !isMe);
		}
		else
		{
			SetRank(rank + 1, isMe);
		}
	}

	private void SetRank(int rank, bool isMe)
	{
		if (rank <= 3)
		{
			if (RankSpriteSelector != null)
			{
				RankSpriteSelector.Select(rank.ToString());
			}
			UIHelper.EnableGameObject(RankSpriteSelector, enable: true);
			UIHelper.EnableGameObject(RankLabel_Me, enable: false);
			UIHelper.EnableGameObject(RankLabel_Other, enable: false);
		}
		else
		{
			if (isMe)
			{
				UIHelper.SetLabelText(RankLabel_Me, rank);
			}
			else
			{
				UIHelper.SetLabelText(RankLabel_Other, rank);
			}
			UIHelper.EnableGameObject(RankSpriteSelector, enable: false);
			UIHelper.EnableGameObject(RankLabel_Me, isMe);
			UIHelper.EnableGameObject(RankLabel_Other, !isMe);
		}
	}

	private void SetServerGroupName(string serverGroup)
	{
		string serverName = GameRoot.Instance.ServerGroupNameDataContainer.GetServerName(serverGroup);
		if (!String_Extension_Method.IsNullOrEmpty(serverName))
		{
			UIHelper.SetLabelText(Label_Server, "[" + serverName + "]");
		}
	}

	private void SetCountryFlag(string countryCode)
	{
		CountryCodeData dataByCode = GameRoot.Instance.CountryCodeDataContainer.GetDataByCode(countryCode);
		if (dataByCode != null)
		{
			AssetTexture plainTexture = UIHelper.GetPlainTexture(dataByCode.IconPath);
			UIHelper.ResetPlaneAssetTextureAsync(this, Texture_CountryFlag, plainTexture);
		}
	}

	private void InitRankerInfo(bool isMemberRanking)
	{
		ClearRankerInfo();
		UIHelper.EnableGameObject(MemberNameRoot, isMemberRanking);
		UIHelper.EnableGameObject(GuildNameRoot, !isMemberRanking);
		UIHelper.EnableGameObject(Label_Server, !isMemberRanking);
	}

	private bool CheckAvailableGuild(GuildSuppressGroupRankerInfo groupRankerInfo)
	{
		if (String_Extension_Method.IsNullOrEmpty(groupRankerInfo.GuildName) || groupRankerInfo.GuildLogo == 0)
		{
			return false;
		}
		return true;
	}

	public void OnClickComponent()
	{
	}

	private void ClearRankerInfo()
	{
		UIHelper.EnableGameObject(LabelDisapperGuild, enable: false);
		UIHelper.EnableGameObject(Label_Server, enable: false);
		UIHelper.SetLabelText(Label_MemberName, string.Empty);
		UIHelper.SetLabelText(Label_GuildName, string.Empty);
		UIHelper.SetLabelText(Label_Point, string.Empty);
		UIHelper.SetLabelText(Label_Server, string.Empty);
		UIHelper.ClearPlaneAssetTexture(Texture_CountryFlag);
		UIHelper.ClearPlaneAssetTexture(Texture_Portrait);
	}
}
