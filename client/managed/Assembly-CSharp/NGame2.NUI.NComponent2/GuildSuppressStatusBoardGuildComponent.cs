using NGame2.NUI.NHelper.NGuild;
using NNguiExtension;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class GuildSuppressStatusBoardGuildComponent : UIScrollItemBehaviour, InitOnCreate
{
	public interface IClickListener
	{
		void OnClick(long guildId, string serverName);
	}

	public const string PrefabPath = "Prefab2/Component/GuildSuppress/GuildSuppressStatusBoardGuildComponent";

	public static PrefabAuto<GuildSuppressStatusBoardGuildComponent> PrefabAuto = new PrefabAuto<GuildSuppressStatusBoardGuildComponent>("Prefab2/Component/GuildSuppress/GuildSuppressStatusBoardGuildComponent");

	public UISpriteSelector RankSelector;

	public UILabel RankLabel;

	public GameObject MyGo;

	public UITexture LogoTexture;

	public UITexture LogoBgTexture;

	public UITexture CountryTexture;

	public UILabel LevelLabel;

	public UILabel NameLabel;

	public UILabel MemberCountLabel;

	public UILabel ScoreLabel;

	public GameObject SelectGo;

	private IClickListener _listener;

	private long _guildId;

	private string _serverName;

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		UIHelper.ClearPlaneAssetTexture(LogoTexture);
		UIHelper.ClearPlaneAssetTexture(LogoBgTexture);
		UIHelper.ClearPlaneAssetTexture(CountryTexture);
		_guildId = 0L;
		_serverName = "";
	}

	public void Open(GuildInfo guildInfo, GuildSuppressGroupRankerInfo rankerInfo, bool isMe, bool isSelect, IClickListener listener)
	{
		long guildId = rankerInfo.GuildId;
		string serverGroup = rankerInfo.ServerGroup;
		int rank = rankerInfo.Rank;
		int guildLogo = rankerInfo.GuildLogo;
		byte back = guildInfo.Back;
		string countryCode = rankerInfo.CountryCode;
		byte level = guildInfo.Level;
		string guildName = rankerInfo.GuildName;
		byte curMember = guildInfo.CurMember;
		long score = rankerInfo.Score;
		Open(guildId, serverGroup, rank, isMe, guildLogo, back, countryCode, level, guildName, curMember, score, isSelect, listener);
	}

	public void Open(long guildId, string serverName, int rank, bool isMe, int logoIndex, int logoBgIndex, string countryCode, int level, string name, int memberCount, long score, bool isSelect, IClickListener listener)
	{
		SetGuildInfoKey(guildId, serverName);
		SetRank(rank);
		SetMyGo(isMe);
		SetLogo(logoIndex, logoBgIndex);
		SetCounty(countryCode);
		SetLevel(level);
		SetName(name);
		SetMemberCount(memberCount, level);
		SetScore(score);
		SetSelect(isSelect);
		_listener = listener;
	}

	private void SetGuildInfoKey(long guildId, string serverName)
	{
		_guildId = guildId;
		_serverName = serverName;
	}

	private void SetRank(int rank)
	{
		GuildSuppressHelper.SetRank(rank, RankSelector, RankLabel);
	}

	private void SetMyGo(bool isMe)
	{
		UIHelper.EnableGameObject(MyGo, isMe);
	}

	private void SetLogo(int logoIndex, int logoBgIndex)
	{
		GuildSuppressHelper.SetGuildLogo(logoIndex, logoBgIndex, LogoTexture, LogoBgTexture, this);
	}

	private void SetCounty(string countyCode)
	{
		GuildSuppressHelper.SetGuildCountyTexture(countyCode, CountryTexture, this);
	}

	private void SetLevel(int level)
	{
		GuildSuppressHelper.SetGuildLevel(LevelLabel, level);
	}

	private void SetName(string name)
	{
		UIHelper.SetLabelText(NameLabel, name);
	}

	private void SetMemberCount(int memberCount, int level)
	{
		GuildSuppressHelper.SetGuildMemberCount(memberCount, level, MemberCountLabel);
	}

	private void SetScore(long score)
	{
		UIHelper.SetLabelText(ScoreLabel, Vespa_Long_Extension_Method.ToCommaString(score));
	}

	public void SetSelect(bool isSelect)
	{
		UIHelper.EnableGameObject(SelectGo, isSelect);
	}

	public void OnClick()
	{
		if (_listener != null)
		{
			_listener.OnClick(_guildId, _serverName);
		}
	}
}
