namespace NShared.BeginCampaign;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "campaign/begin_campaign";

	public int ChapterIndex { get; set; }

	public int DungeonIndex { get; set; }

	public int LinkedDungeonIndex { get; set; }

	public int DungeonDifficulty { get; set; }

	public bool ScenarioDungeon { get; set; }

	public int[] HeroIndices { get; set; }

	public int[] GroupHeroIndices { get; set; }

	public int[] AiHeroIndices { get; set; }

	public int LeaderHeroIndex { get; set; }

	public long[] AccountIds { get; set; }

	public bool Repeat { get; set; }

	public int BuyKey { get; set; }

	public int WorldMapEventIndex { get; set; }

	public int WorldMapNodeIndex { get; set; }

	public long MultiplayMasterId { get; set; }

	public long[] MultiplayMemberIds { get; set; }

	public long[] MultiplayObserverIds { get; set; }

	public string MultiplayMasterServerIp { get; set; }

	public int MultiplayMasterServerPort { get; set; }

	public int TowerIndex { get; set; }

	public int TowerFloor { get; set; }

	public int RaidIndex { get; set; }

	public int RaidLevel { get; set; }

	public int WorldBossIndex { get; set; }

	public int EventWorldBossIndex { get; set; }

	public int EventWorldBossLevel { get; set; }

	public long RaidRoomNo { get; set; }

	public long MultiRoomNo { get; set; }

	public long ItemBoosterCount { get; set; }

	public bool UseStamina { get; set; }

	public string[] SelectedRewardItemCodes { get; set; }

	public int OnlineGameSpeedRatio { get; set; }

	public int EnterTicketCount { get; set; }

	public int[] DeltaHeroIndices { get; set; }

	public int[] ZetaHeroIndices { get; set; }

	public int[] AffixIndices { get; set; }
}
