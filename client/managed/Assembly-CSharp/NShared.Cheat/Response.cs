namespace NShared.Cheat;

[Response]
public class Response : GenericResponse<ResultType>
{
	public string Message { get; set; }

	public EquipItemInfo[] EquipItemResults { get; set; }

	public int[] RemovedEquipItemSlotIndices { get; set; }

	public ItemResultInfo[] ItemResults { get; set; }

	public SoulWeaponInfo SoulWeaponInfo { get; set; }

	public CurrencyResultInfo3[] CurrencyResults { get; set; }

	public FriendshipPointResultInfo FriendshipPointResult { get; set; }

	public HeroInfo[] HeroInfos { get; set; }

	public ExpResultInfo TeamExpResult { get; set; }

	public TowerInfo[] TowerInfos { get; set; }

	public ChapterDungeonInfo[] DungeonInfos { get; set; }

	public TutorialInfo[] TutorialInfos { get; set; }

	public StaminaResultInfo[] StaminaResults { get; set; }

	public PlayerBattleInfo BattleInfo { get; set; }

	public RaidInfo[] RaidInfos { get; set; }

	public PlayerMiscInfo MiscInfo { get; set; }

	public MyGuildInfo MyGuildInfo { get; set; }

	public GuildBuildingInfo GuildBuildingInfo { get; set; }

	public GuildActivityPointResultInfo GuildActivityPointResult { get; set; }

	public GuildSuppressPointResultInfo GuildSuppressPointResult { get; set; }

	public PlayerHeroFriendlyInfo HeroFriendlyInfo { get; set; }

	public PlayerPurchaseTimeDurationInfo[] PurchaseTimeDurations { get; set; }

	public GuildPointResultInfo GuildPointResult { get; set; }

	public GuildRaidDungeonInfo GuildRaidInfo { get; set; }

	public WorldBossPointResultInfo WorldBossPointResult { get; set; }

	public NPCFriendlyInfo NpcFriendlyInfo { get; set; }

	public int RemovedHeroIndex { get; set; }

	public ShopItemInfo[] ShopItemInfos { get; set; }

	public StepUpEquipGachaInfo StepUpEquipGachaInfo { get; set; }

	public PetInfo[] PetInfos { get; set; }

	public PetMiscInfo[] PetMiscInfos { get; set; }

	public PetGachaCountInfo[] PetGachaCountInfos { get; set; }

	public PlayerTreasureHouseInfo PlayerTreasureHouseInfo { get; set; }

	public PunishmentRaidInfo[] PunishmentRaidInfos { get; set; }

	public OpenPunishmentRaidInfo OpenPunishmentRaidInfo { get; set; }

	public PetExploreInfo[] PetExploreInfos { get; set; }

	public PayShopDiscountInfo[] PayShopDiscountInfos { get; set; }

	public PlayerProductPurchaseInfo[] PlayerProductPurchaseInfo { get; set; }
}
