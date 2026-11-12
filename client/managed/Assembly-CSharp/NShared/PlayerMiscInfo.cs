using System;

namespace NShared;

[JsonSerializable(Javascript = true)]
public class PlayerMiscInfo
{
	public int InventoryExtend { get; set; }

	public int EquipStorageSlotExtend { get; set; }

	public int ChestExtend { get; set; }

	public int PunishmentRuneStorageSlotExtend { get; set; }

	public string DailyQuestResetTime { get; set; }

	public string WeeklyQuestResetTime { get; set; }

	public string DOWResetTime { get; set; }

	public int UTCOffsetHour { get; set; }

	public int HeroShopSlotExtend { get; set; }

	public long LastGlobalMailIndex { get; set; }

	public string QuestDailyRepeatResetTime { get; set; }

	[Obsolete("메인 퀘스트 단계{ (클라는 사용안함) DB구조용")]
	public int MainQuestStep { get; set; }

	[Obsolete("메인 퀘스트 진행 상황{ (클라는 사용안함) DB구조용")]
	public int MainQuestProgress { get; set; }

	[Obsolete("사용안함")]
	public int ScenarioQuestIndex { get; set; }

	[Obsolete("사용안함")]
	public int ScenarioQuestProgress { get; set; }

	public string LastRaidResetTime { get; set; }

	public string GuildWithdrawTime { get; set; }

	public int GuildWithdrawCount { get; set; }

	public long DailyAccFriendshipPoint { get; set; }

	public string DailyAccFriendshipPointResetTime { get; set; }

	public int RenewOptionEquipItemSlotIndex { get; set; }

	public int RenewOptionResultIndex { get; set; }

	public int RenewOptionResultStep { get; set; }

	public int RenewOptionSlotIndex { get; set; }

	public int RenewEnchantOptionEquipItemSlotIndex { get; set; }

	public int RenewEnchantOptionResultIndex { get; set; }

	public int RenewEnchantOptionResultStep { get; set; }

	public int LastEventChapterIndex { get; set; }

	public int LastEventDungeonIndex { get; set; }

	public string EventWorldBossCompletedTime { get; set; }

	public int LoginDailyCount { get; set; }

	public GuildArenaSessionInfo GuildArenaSessionInfo { get; set; }

	public string LoginDailyUpdatedTime { get; set; }

	public int SuccessiveDayCount { get; set; }

	public int MaxSuccessiveDayCount { get; set; }

	public string PlayTimeUpdatedTime { get; set; }

	public int IsArchiveUpdate { get; set; }

	public int NickChange { get; set; }

	public int AppReviewPopup { get; set; }

	public int MazeTowerRewardStep { get; set; }

	public string MazeTowerRewardedTime { get; set; }

	public string OpenGuildTime { get; set; }

	public string GuildMemberRewardedTime { get; set; }

	public string ChapterRenewedTime { get; set; }

	public string MainQuestRenewedTime { get; set; }

	public string SubQuestRenewedTime { get; set; }

	public long SoulStoneMileage { get; set; }

	public string RestoreSoulStoneIndices { get; set; }

	[NonDbSchema]
	public int HeroShopItemListIndex { get; set; }

	public string CountryCode { get; set; }

	public string NewbiePopUpEndTime { get; set; }

	public string NewbiePopUpCompleteTime { get; set; }

	public int[] ShakemehPassiveInfos { get; set; }

	public ValanceRenewOptionInfo ValanceRenewOptionInfo { get; set; }

	public ValanceEnchantOptionInfo ValanceEnchantOptionInfo { get; set; }

	public ValanceRenewSkillInfo ValanceRenewSkillInfo { get; set; }
}
