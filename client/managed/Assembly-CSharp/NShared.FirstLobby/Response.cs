namespace NShared.FirstLobby;

[Response]
public class Response : GenericResponse<ResultType>
{
	public PlayerBookMarkHeroInfo PlayerBookMarkHeroInfo { get; set; }

	public PayShopUserGroup PayShopUserGroup { get; set; }

	public string UserTypeEndTime { get; set; }

	public HeroPresetStorageInfo[] HeroPresetStorages { get; set; }

	public EquipPresetInfo[] EquipPresetInfos { get; set; }

	public EquipItemIdentifiedInfo[] EquipItemIdentifiedInfos { get; set; }

	public int[] TechnoMagicEquipItems { get; set; }

	public PetInfo[] PetInfos { get; set; }

	public PetMiscInfo[] PetMiscInfos { get; set; }

	public PetGachaCountInfo[] PetGachaCountInfos { get; set; }

	public GachaCeilingCountInfo[] GachaCeilingCountInfos { get; set; }

	public KingsPassSeaonInfo KingsPassSeaonInfo { get; set; }

	public KingsPassMission[] KingsPassMissionInfos { get; set; }

	public OpenPunishmentRaidInfo[] OpenPunishmentRaidInfos { get; set; }

	public PunishmentRaidInfo[] PunishmentRaidInfos { get; set; }

	public EventCraftItemInfo[] EventCraftItemInfos { get; set; }

	public PetExploreInfo[] PetExploreInfos { get; set; }

	public TeamLevelBuff[] TeamLevelBuffInfos { get; set; }
}
