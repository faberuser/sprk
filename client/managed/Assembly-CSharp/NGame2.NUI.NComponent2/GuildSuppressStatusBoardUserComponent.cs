using NGame2.NUI.NHelper.NGuild;
using NNguiExtension;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class GuildSuppressStatusBoardUserComponent : UIScrollItemBehaviour, InitOnCreate
{
	public const string PrefabPath = "Prefab2/Component/GuildSuppress/GuildSuppressStatusBoardUserComponent";

	public static PrefabAuto<GuildSuppressStatusBoardUserComponent> PrefabAuto = new PrefabAuto<GuildSuppressStatusBoardUserComponent>("Prefab2/Component/GuildSuppress/GuildSuppressStatusBoardUserComponent");

	public UISpriteSelector RankSelector;

	public UILabel MyRankLabel;

	public UILabel OtherRankLabel;

	public GameObject MyGo;

	public UILabel NameLabel;

	public UILabel ScoreLabel;

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
	}

	protected virtual void Release()
	{
	}

	public void Open(GuildSuppressMemberScoreInfo scoreInfo, int rank, bool isMe)
	{
		string nickName = scoreInfo.NickName;
		long score = scoreInfo.Score;
		Open(rank, isMe, nickName, score);
	}

	public void Open(int rank, bool isMe, string name, long score)
	{
		SetRank(isMe, rank);
		SetMe(isMe);
		SetName(name);
		SetScore(score);
	}

	private void SetRank(bool isMe, int rank)
	{
		UILabel rankLabel = (isMe ? MyRankLabel : OtherRankLabel);
		UIHelper.EnableGameObject(MyRankLabel, enable: false);
		UIHelper.EnableGameObject(OtherRankLabel, enable: false);
		GuildSuppressHelper.SetRank(rank, RankSelector, rankLabel);
	}

	private void SetMe(bool isMe)
	{
		UIHelper.EnableGameObject(MyGo, isMe);
	}

	protected void SetName(string name)
	{
		UIHelper.SetLabelText(NameLabel, name);
	}

	protected void SetScore(long score)
	{
		UIHelper.SetLabelText(ScoreLabel, Vespa_Long_Extension_Method.ToCommaString(score));
	}
}
