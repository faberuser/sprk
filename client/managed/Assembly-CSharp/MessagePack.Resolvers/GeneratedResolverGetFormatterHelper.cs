using System;
using System.Collections.Generic;
using MessagePack.Formatters;
using MessagePack.Formatters.NShared;
using MessagePack.Formatters.NShared.NJit;
using MessagePack.Formatters.NShared.NLocalization;
using MessagePack.Formatters.NShared.NUtil;
using MessagePack.Formatters.NShared.NWorldMap;
using NShared;
using NShared.NJit;
using NShared.NLocalization;
using NShared.NUtil;
using NShared.NWorldMap;

namespace MessagePack.Resolvers;

internal static class GeneratedResolverGetFormatterHelper
{
	private static readonly Dictionary<Type, int> lookup;

	static GeneratedResolverGetFormatterHelper()
	{
		lookup = new Dictionary<Type, int>(762)
		{
			{
				typeof(AttackAttrType[]),
				0
			},
			{
				typeof(BattleRetryConditionType[]),
				1
			},
			{
				typeof(BattleType[]),
				2
			},
			{
				typeof(ContentsStateIcon[]),
				3
			},
			{
				typeof(CreatureTagType[]),
				4
			},
			{
				typeof(EquipItemSubType[]),
				5
			},
			{
				typeof(EquipPartType[]),
				6
			},
			{
				typeof(ERightMenuButton[]),
				7
			},
			{
				typeof(EventDungeonPropType[]),
				8
			},
			{
				typeof(GetNewHeroInfoPosition[]),
				9
			},
			{
				typeof(ItemType[]),
				10
			},
			{
				typeof(MatchSeasonType[]),
				11
			},
			{
				typeof(OperationFlag[]),
				12
			},
			{
				typeof(PatchSpecType[]),
				13
			},
			{
				typeof(RuneSlotType[]),
				14
			},
			{
				typeof(ShopCostType[]),
				15
			},
			{
				typeof(SkillAttrType[]),
				16
			},
			{
				typeof(SkillTargetType[]),
				17
			},
			{
				typeof(SkillTriggerFlag[]),
				18
			},
			{
				typeof(StaminaType[]),
				19
			},
			{
				typeof(StatType[]),
				20
			},
			{
				typeof(List<CampaignWaveData.CreatureDetailWritable>),
				21
			},
			{
				typeof(List<ItemGroupData.SubData>),
				22
			},
			{
				typeof(List<RewardData.SubData>),
				23
			},
			{
				typeof(List<SoulBreakData.ItemCodeMinMaxData>),
				24
			},
			{
				typeof(List<int>),
				25
			},
			{
				typeof(List<string>),
				26
			},
			{
				typeof(AccessoryPartPivotType),
				27
			},
			{
				typeof(AccessoryPartType),
				28
			},
			{
				typeof(AchievementCategoryType),
				29
			},
			{
				typeof(AchievementReqSubType),
				30
			},
			{
				typeof(AchievementReqType),
				31
			},
			{
				typeof(AchievementRewardType),
				32
			},
			{
				typeof(AchievementSubType),
				33
			},
			{
				typeof(AchievementType),
				34
			},
			{
				typeof(ArchiveMainType),
				35
			},
			{
				typeof(ArchiveSubType),
				36
			},
			{
				typeof(ArchiveSubType2),
				37
			},
			{
				typeof(ArchiveType),
				38
			},
			{
				typeof(AttackAttrType),
				39
			},
			{
				typeof(BannerPlayTypes),
				40
			},
			{
				typeof(BattleRetryConditionType),
				41
			},
			{
				typeof(BattleTacticType),
				42
			},
			{
				typeof(BattleType),
				43
			},
			{
				typeof(BossWarningData.EType),
				44
			},
			{
				typeof(BossWarningData.EWarningType),
				45
			},
			{
				typeof(CalendarCutSceneType),
				46
			},
			{
				typeof(CampaignNodeData.EMenuOpenCondition),
				47
			},
			{
				typeof(CampaignNodeData.ENodeOpenStyle),
				48
			},
			{
				typeof(CampaignWaveData.EWaveFlag),
				49
			},
			{
				typeof(CcMotionType),
				50
			},
			{
				typeof(ChapterType),
				51
			},
			{
				typeof(ClearMissionProvideType),
				52
			},
			{
				typeof(ClearMissionPurchaseType),
				53
			},
			{
				typeof(ClearMissionType),
				54
			},
			{
				typeof(ClientActivityType),
				55
			},
			{
				typeof(ContentsNoticePopupType),
				56
			},
			{
				typeof(ContentsNoticeType),
				57
			},
			{
				typeof(ContentsStateIcon),
				58
			},
			{
				typeof(ContentsStatusSubType),
				59
			},
			{
				typeof(ContentsStatusType),
				60
			},
			{
				typeof(ContentsType),
				61
			},
			{
				typeof(CostumeAbilityType),
				62
			},
			{
				typeof(CraftSortCategory),
				63
			},
			{
				typeof(CreatureOpenStatusType),
				64
			},
			{
				typeof(CreatureOpenType),
				65
			},
			{
				typeof(CreaturePositionType),
				66
			},
			{
				typeof(CreatureRaceType),
				67
			},
			{
				typeof(CreatureTagType),
				68
			},
			{
				typeof(CreatureTransformType),
				69
			},
			{
				typeof(CurrencyChangeType),
				70
			},
			{
				typeof(CurrencyShowType),
				71
			},
			{
				typeof(CurrencyType),
				72
			},
			{
				typeof(DamageMeterFlag),
				73
			},
			{
				typeof(DelayedSpawnType),
				74
			},
			{
				typeof(DeviceStyleType),
				75
			},
			{
				typeof(DispatchState),
				76
			},
			{
				typeof(DispatchType),
				77
			},
			{
				typeof(DisplayProgressType),
				78
			},
			{
				typeof(NShared.DungeonDifficultyType),
				79
			},
			{
				typeof(EAffixShowType),
				80
			},
			{
				typeof(EAffixType),
				81
			},
			{
				typeof(EAttendanceKeyword),
				82
			},
			{
				typeof(EBattleMenu),
				83
			},
			{
				typeof(EContentType),
				84
			},
			{
				typeof(ECostumeBuffType),
				85
			},
			{
				typeof(ECostumeSelectItemType),
				86
			},
			{
				typeof(EDelayType),
				87
			},
			{
				typeof(EDungeonGroupType),
				88
			},
			{
				typeof(EEquipCondition),
				89
			},
			{
				typeof(EEventDungeonCloseType),
				90
			},
			{
				typeof(EffectWidgetType),
				91
			},
			{
				typeof(EGroupEnableConditionType),
				92
			},
			{
				typeof(EGuildFlagDisplayType),
				93
			},
			{
				typeof(EHeroCostumeType),
				94
			},
			{
				typeof(EKarmaCardType),
				95
			},
			{
				typeof(EKarmaRarity),
				96
			},
			{
				typeof(EMapArrangement),
				97
			},
			{
				typeof(EMissionTopViewButtonGroup),
				98
			},
			{
				typeof(EMissionTopViewLayout),
				99
			},
			{
				typeof(EMissionTopViewRewardType),
				100
			},
			{
				typeof(EMissionType),
				101
			},
			{
				typeof(EOrdealArenaNodeType),
				102
			},
			{
				typeof(EOrdealEventType),
				103
			},
			{
				typeof(EPetAdventureType),
				104
			},
			{
				typeof(EPunishmentDungeonType),
				105
			},
			{
				typeof(EPunishmentPortraitLineType),
				106
			},
			{
				typeof(EquipGachaSubType),
				107
			},
			{
				typeof(EquipGachaType),
				108
			},
			{
				typeof(EquipItemDetailType),
				109
			},
			{
				typeof(EquipItemEquipGrade),
				110
			},
			{
				typeof(EquipItemEquipType),
				111
			},
			{
				typeof(EquipItemSetGroupType),
				112
			},
			{
				typeof(EquipItemSubType),
				113
			},
			{
				typeof(EquipItemWorkPlace),
				114
			},
			{
				typeof(EquipPartType),
				115
			},
			{
				typeof(EquipPartTypeFlag),
				116
			},
			{
				typeof(EquipTransitionType),
				117
			},
			{
				typeof(ERightMenuButton),
				118
			},
			{
				typeof(EShopItemFilterGroupType),
				119
			},
			{
				typeof(EShopItemType),
				120
			},
			{
				typeof(EShopSortType),
				121
			},
			{
				typeof(ESoulWeaponAbilityTicketType),
				122
			},
			{
				typeof(ESoulweaponLiberationResultSide),
				123
			},
			{
				typeof(EValanceFilterGroupType),
				124
			},
			{
				typeof(EventBonusType),
				125
			},
			{
				typeof(EventDungeonPropType),
				126
			},
			{
				typeof(EventRouletteType),
				127
			},
			{
				typeof(EventStepType),
				128
			},
			{
				typeof(EventTriggerCategory),
				129
			},
			{
				typeof(EventTriggerCommand),
				130
			},
			{
				typeof(ExInfoType),
				131
			},
			{
				typeof(FlaskRewardType),
				132
			},
			{
				typeof(FlaskSourceType),
				133
			},
			{
				typeof(FlaskType),
				134
			},
			{
				typeof(FloorDungeonSelectType),
				135
			},
			{
				typeof(FriendlyActionType),
				136
			},
			{
				typeof(GameIntroduceType),
				137
			},
			{
				typeof(GameOptionComponent),
				138
			},
			{
				typeof(GameOptionTab),
				139
			},
			{
				typeof(GetNewHeroInfoPosition),
				140
			},
			{
				typeof(GodkingTrialDungeonGateType),
				141
			},
			{
				typeof(GuildArenaTierType),
				142
			},
			{
				typeof(GuildConstructionResourceTopmostMenuType),
				143
			},
			{
				typeof(GuildSkillType),
				144
			},
			{
				typeof(GuildSuppressServerRewardType),
				145
			},
			{
				typeof(HeroSelectItemType),
				146
			},
			{
				typeof(HighGachaCategoryType),
				147
			},
			{
				typeof(InspectUserCategory),
				148
			},
			{
				typeof(InspectUserType),
				149
			},
			{
				typeof(InventoryType),
				150
			},
			{
				typeof(ItemActionType),
				151
			},
			{
				typeof(ItemData.GetEffectType),
				152
			},
			{
				typeof(ItemType),
				153
			},
			{
				typeof(KingsPassType),
				154
			},
			{
				typeof(LeftMenuButtonType),
				155
			},
			{
				typeof(LoadingData.ECategory),
				156
			},
			{
				typeof(LoadingData.EGroupType),
				157
			},
			{
				typeof(LuckyMatchRewardType),
				158
			},
			{
				typeof(LuckyMatchTierType),
				159
			},
			{
				typeof(MarketingType),
				160
			},
			{
				typeof(MatchArenaType),
				161
			},
			{
				typeof(MatchBanPickState),
				162
			},
			{
				typeof(MatchRewardType),
				163
			},
			{
				typeof(MatchSeasonType),
				164
			},
			{
				typeof(MatchTierType),
				165
			},
			{
				typeof(MonsterGradeType),
				166
			},
			{
				typeof(NewbieRewardProvideType),
				167
			},
			{
				typeof(NewbieRewardPurchaseType),
				168
			},
			{
				typeof(NewbieRewardType),
				169
			},
			{
				typeof(NewPayShopCategoryType),
				170
			},
			{
				typeof(NewPayShopProductCategoryType),
				171
			},
			{
				typeof(LanguageType),
				172
			},
			{
				typeof(NodeFunctionType),
				173
			},
			{
				typeof(CheckType),
				174
			},
			{
				typeof(EventType),
				175
			},
			{
				typeof(OpenConditionType),
				176
			},
			{
				typeof(OperationConditionActionType),
				177
			},
			{
				typeof(OperationConditionType),
				178
			},
			{
				typeof(OperationFlag),
				179
			},
			{
				typeof(OperationGroupConditionType),
				180
			},
			{
				typeof(OperationTargetSelectType),
				181
			},
			{
				typeof(OperationTargetType),
				182
			},
			{
				typeof(OperationType),
				183
			},
			{
				typeof(OptionSellCategory),
				184
			},
			{
				typeof(OptionSellSelectType),
				185
			},
			{
				typeof(PairEffectType),
				186
			},
			{
				typeof(PartyDungeonChapter),
				187
			},
			{
				typeof(PartyDungeonType),
				188
			},
			{
				typeof(PatchPlatform),
				189
			},
			{
				typeof(PatchSpecType),
				190
			},
			{
				typeof(PayShopIndexType),
				191
			},
			{
				typeof(PayShopUserGroup),
				192
			},
			{
				typeof(PetOptionConditionType),
				193
			},
			{
				typeof(PhaseStepReqType),
				194
			},
			{
				typeof(PhaseStepRewardType),
				195
			},
			{
				typeof(PlayGuideData.GuideType),
				196
			},
			{
				typeof(PlayRecordActionCalculateType),
				197
			},
			{
				typeof(PlayRecordCategory),
				198
			},
			{
				typeof(PlayRecordDisplayType),
				199
			},
			{
				typeof(PlayRecordDisplayValueType),
				200
			},
			{
				typeof(PortalPropConditionType),
				201
			},
			{
				typeof(PortalPropType),
				202
			},
			{
				typeof(PortalRenewalCategory),
				203
			},
			{
				typeof(PortalRenewalCompType),
				204
			},
			{
				typeof(PortalRenewalSubCategory),
				205
			},
			{
				typeof(ProductType),
				206
			},
			{
				typeof(PunishmentRaidRewardType),
				207
			},
			{
				typeof(QuestActionType),
				208
			},
			{
				typeof(QuestOpenConditionType),
				209
			},
			{
				typeof(RaidBanType),
				210
			},
			{
				typeof(RaidRewardType),
				211
			},
			{
				typeof(RaidType),
				212
			},
			{
				typeof(RankPanelType),
				213
			},
			{
				typeof(RankViewType),
				214
			},
			{
				typeof(RecordsOfHonorContentType),
				215
			},
			{
				typeof(ResetType),
				216
			},
			{
				typeof(RewardItemIconType),
				217
			},
			{
				typeof(RewardType),
				218
			},
			{
				typeof(RouletteType),
				219
			},
			{
				typeof(RuneColorType),
				220
			},
			{
				typeof(RuneSlotType),
				221
			},
			{
				typeof(SelectItemPopupType),
				222
			},
			{
				typeof(ShakmehBossType),
				223
			},
			{
				typeof(ShopCostType),
				224
			},
			{
				typeof(SkillAttrType),
				225
			},
			{
				typeof(SkillCcFlag),
				226
			},
			{
				typeof(SkillTargetFlag),
				227
			},
			{
				typeof(SkillTargetType),
				228
			},
			{
				typeof(SkillTriggerFlag),
				229
			},
			{
				typeof(SkillTriggerType),
				230
			},
			{
				typeof(StaminaType),
				231
			},
			{
				typeof(StaminaUpdateType),
				232
			},
			{
				typeof(StaminaValueType),
				233
			},
			{
				typeof(StateActionType),
				234
			},
			{
				typeof(StateConditionType),
				235
			},
			{
				typeof(StateFlag),
				236
			},
			{
				typeof(StateOverflowRuleType),
				237
			},
			{
				typeof(StateStackDurationRuleType),
				238
			},
			{
				typeof(StatType),
				239
			},
			{
				typeof(SweepDungeonType),
				240
			},
			{
				typeof(TeamLevelBuffOptionConditionType),
				241
			},
			{
				typeof(TeamLevelGrade),
				242
			},
			{
				typeof(TeamType),
				243
			},
			{
				typeof(ThreatType),
				244
			},
			{
				typeof(TowerType),
				245
			},
			{
				typeof(UnderPrisonType),
				246
			},
			{
				typeof(WinDeciderType),
				247
			},
			{
				typeof(WorldBossServerRewardType),
				248
			},
			{
				typeof(WorldMapPropConditionType),
				249
			},
			{
				typeof(WorldMapPropType),
				250
			},
			{
				typeof(AccessoryCostumeData),
				251
			},
			{
				typeof(AccessoryCostumeGroupData),
				252
			},
			{
				typeof(AccessorySelectItemData),
				253
			},
			{
				typeof(AccountBuffData),
				254
			},
			{
				typeof(AccumulateLoginRewardData),
				255
			},
			{
				typeof(AchievementData),
				256
			},
			{
				typeof(AchievementTextData),
				257
			},
			{
				typeof(AdjustTokenData),
				258
			},
			{
				typeof(AdventureNoticeData),
				259
			},
			{
				typeof(AffixData),
				260
			},
			{
				typeof(AppReviewConditionData),
				261
			},
			{
				typeof(ArtifactRestoreData),
				262
			},
			{
				typeof(AttendanceInfoData),
				263
			},
			{
				typeof(AttendanceRewardInfoData),
				264
			},
			{
				typeof(AttendanceRewardInfoKey),
				265
			},
			{
				typeof(AttendanceRewardUrlData),
				266
			},
			{
				typeof(AttendanceUrlData),
				267
			},
			{
				typeof(AttendanceUrlDataKey),
				268
			},
			{
				typeof(AutoSeparationData),
				269
			},
			{
				typeof(AwakenStoneData),
				270
			},
			{
				typeof(BanRuleData),
				271
			},
			{
				typeof(BattleDefineData),
				272
			},
			{
				typeof(BoosterDefineData),
				273
			},
			{
				typeof(BoosterIconActionData),
				274
			},
			{
				typeof(BoosterItemData),
				275
			},
			{
				typeof(BossWarningData),
				276
			},
			{
				typeof(BottomButtonData),
				277
			},
			{
				typeof(BottomButtonKey),
				278
			},
			{
				typeof(CampaignChapterData),
				279
			},
			{
				typeof(CampaignDungeonData),
				280
			},
			{
				typeof(CampaignDungeonSkillData),
				281
			},
			{
				typeof(CampaignFieldData),
				282
			},
			{
				typeof(CampaignMultiAvatarData),
				283
			},
			{
				typeof(CampaignMultiAvatarKey),
				284
			},
			{
				typeof(CampaignNodeData),
				285
			},
			{
				typeof(CampaignNodeNPCData),
				286
			},
			{
				typeof(CampaignTacticData),
				287
			},
			{
				typeof(CampaignTacticKey),
				288
			},
			{
				typeof(CampaignWaveData),
				289
			},
			{
				typeof(CampaignWaveData.CreatureDetailWritable),
				290
			},
			{
				typeof(CampaignWaveDataContainer.ChapterDungeonInfo),
				291
			},
			{
				typeof(CensoredWordData),
				292
			},
			{
				typeof(ChallengeRaidClearRewardData),
				293
			},
			{
				typeof(ChallengeRaidClearRewardKey),
				294
			},
			{
				typeof(ChallengeRaidData),
				295
			},
			{
				typeof(ChallengeRaidRewardData),
				296
			},
			{
				typeof(ChapterClearRewardData),
				297
			},
			{
				typeof(ChapterClearRewardKey),
				298
			},
			{
				typeof(ChapterDungeonKey2),
				299
			},
			{
				typeof(CinemaItemData),
				300
			},
			{
				typeof(CinemaWindowData),
				301
			},
			{
				typeof(ClassBuffData),
				302
			},
			{
				typeof(ClassBuffDataBase),
				303
			},
			{
				typeof(ClearMissionProductData),
				304
			},
			{
				typeof(ClearMissionProductMissionData),
				305
			},
			{
				typeof(ClientCreatureData),
				306
			},
			{
				typeof(ClientCreatureKey),
				307
			},
			{
				typeof(ClientCustomizingCameraData),
				308
			},
			{
				typeof(ClientCustomizingPivotData),
				309
			},
			{
				typeof(ClientCustomizingPivotDataKey),
				310
			},
			{
				typeof(ClientStatDisplayScoreData),
				311
			},
			{
				typeof(CodecSupportListData),
				312
			},
			{
				typeof(CommunityWebViewData),
				313
			},
			{
				typeof(ConstantData),
				314
			},
			{
				typeof(ContentsNoticePopupData),
				315
			},
			{
				typeof(ContentsOpenConditionData),
				316
			},
			{
				typeof(CostumeData),
				317
			},
			{
				typeof(CostumeDataContainer.HeroAndStarKey),
				318
			},
			{
				typeof(CostumeGroupData),
				319
			},
			{
				typeof(CostumeSelectItemData),
				320
			},
			{
				typeof(CountryCodeData),
				321
			},
			{
				typeof(CountryCodeGroupData),
				322
			},
			{
				typeof(CountryData),
				323
			},
			{
				typeof(CountryServerData),
				324
			},
			{
				typeof(CraftInstantGemData),
				325
			},
			{
				typeof(CraftItemData),
				326
			},
			{
				typeof(CraftSortData),
				327
			},
			{
				typeof(CraftSortKey),
				328
			},
			{
				typeof(CreatureAwakeChallengeItemData),
				329
			},
			{
				typeof(CreatureData),
				330
			},
			{
				typeof(CreatureGradeData),
				331
			},
			{
				typeof(CreatureGradeDataContainer.ContainerInfo),
				332
			},
			{
				typeof(CreatureLevelData),
				333
			},
			{
				typeof(CreatureLevelDataContainer.ContainerInfo),
				334
			},
			{
				typeof(CreatureLevelFactorData),
				335
			},
			{
				typeof(CreatureLevelFactorDataKey),
				336
			},
			{
				typeof(CreatureLevelStatData2),
				337
			},
			{
				typeof(CreatureOverlapData),
				338
			},
			{
				typeof(CreatureStarData),
				339
			},
			{
				typeof(CreatureStarDataKey),
				340
			},
			{
				typeof(CreatureStarGradeStatData),
				341
			},
			{
				typeof(CreatureStarPriceData),
				342
			},
			{
				typeof(CreatureStarPriceKey),
				343
			},
			{
				typeof(CreatureStarStatData),
				344
			},
			{
				typeof(CreatureStarStatDataKey),
				345
			},
			{
				typeof(CreatureTranscendOptionData),
				346
			},
			{
				typeof(CreatureVisualData),
				347
			},
			{
				typeof(CSMailData),
				348
			},
			{
				typeof(CurrencyTypeData),
				349
			},
			{
				typeof(CustomEquipItemData),
				350
			},
			{
				typeof(CutSceneForegroundData),
				351
			},
			{
				typeof(DbLogData),
				352
			},
			{
				typeof(DbLogDataKey),
				353
			},
			{
				typeof(DefenceLevelData),
				354
			},
			{
				typeof(DeviceModelData),
				355
			},
			{
				typeof(DispatchStateData),
				356
			},
			{
				typeof(DOWDungeonData),
				357
			},
			{
				typeof(DOWDungeonTextData),
				358
			},
			{
				typeof(DOWDungeonTextKey),
				359
			},
			{
				typeof(DownloadControlData),
				360
			},
			{
				typeof(DungeonHeroListSetData),
				361
			},
			{
				typeof(DungeonHeroListSetKey),
				362
			},
			{
				typeof(DungeonHeroSetData),
				363
			},
			{
				typeof(DungeonHeroSetKey),
				364
			},
			{
				typeof(DungeonPassiveEnvironmentData),
				365
			},
			{
				typeof(EclipseRewardData),
				366
			},
			{
				typeof(EclipseStartData),
				367
			},
			{
				typeof(EmoticonData),
				368
			},
			{
				typeof(EmoticonTabData),
				369
			},
			{
				typeof(EntryMenuSettingData),
				370
			},
			{
				typeof(EquipAutoData),
				371
			},
			{
				typeof(EquipAutoKey),
				372
			},
			{
				typeof(EquipAwakenPointData),
				373
			},
			{
				typeof(EquipAwakenPriceData),
				374
			},
			{
				typeof(EquipAwakenPriceDataKey),
				375
			},
			{
				typeof(EquipAwakenRatioData),
				376
			},
			{
				typeof(EquipAwakenRatioDataKey),
				377
			},
			{
				typeof(EquipEnchantableConditionData),
				378
			},
			{
				typeof(EquipEnchantableConditionKey),
				379
			},
			{
				typeof(EquipEnchantOptionItemData),
				380
			},
			{
				typeof(EquipFilterOptionData),
				381
			},
			{
				typeof(EquipFilterOptionGroupData),
				382
			},
			{
				typeof(EquipGachaClientData),
				383
			},
			{
				typeof(EquipGachaClientDataKey),
				384
			},
			{
				typeof(EquipItemBreakData),
				385
			},
			{
				typeof(EquipItemBreakDataKey),
				386
			},
			{
				typeof(EquipItemData),
				387
			},
			{
				typeof(EquipItemDetailData),
				388
			},
			{
				typeof(EquipItemLevelData),
				389
			},
			{
				typeof(EquipItemLevelDataContainer.ContainerInfo),
				390
			},
			{
				typeof(EquipItemStarData),
				391
			},
			{
				typeof(EquipOptionChangeData),
				392
			},
			{
				typeof(EquipOptionChangeDataKey),
				393
			},
			{
				typeof(EquipOptionData),
				394
			},
			{
				typeof(EquipOptionGroupData),
				395
			},
			{
				typeof(EquipOptionUpgradeData),
				396
			},
			{
				typeof(EquipOptionUpgradeDataKey),
				397
			},
			{
				typeof(EquipSetBonusData),
				398
			},
			{
				typeof(EquipSlotData),
				399
			},
			{
				typeof(EquipTransitionTicketData),
				400
			},
			{
				typeof(EquipUpgradeExpData),
				401
			},
			{
				typeof(EquipUpgradePriceData),
				402
			},
			{
				typeof(EquipUpgradePriceDataKey),
				403
			},
			{
				typeof(EquipUpgradeStatData),
				404
			},
			{
				typeof(EquipUpgradeTierData),
				405
			},
			{
				typeof(EventCraftData),
				406
			},
			{
				typeof(EventCraftEffectData),
				407
			},
			{
				typeof(EventDungeonData),
				408
			},
			{
				typeof(EventDungeonGroupData),
				409
			},
			{
				typeof(EventDungeonGroupKey),
				410
			},
			{
				typeof(EventDungeonKey),
				411
			},
			{
				typeof(EventForgeData),
				412
			},
			{
				typeof(EventRouletteData),
				413
			},
			{
				typeof(EventStepData),
				414
			},
			{
				typeof(EventTriggerData),
				415
			},
			{
				typeof(EventWorldBossData),
				416
			},
			{
				typeof(EventWorldBossDataKey),
				417
			},
			{
				typeof(EventWorldBossSeasonData),
				418
			},
			{
				typeof(ExpressionData),
				419
			},
			{
				typeof(FlaskItemData),
				420
			},
			{
				typeof(FrameControlData),
				421
			},
			{
				typeof(GachaProbabilityData),
				422
			},
			{
				typeof(GachaSelectItemData),
				423
			},
			{
				typeof(GachaSelectShopData),
				424
			},
			{
				typeof(GachaSelectShopEffectData),
				425
			},
			{
				typeof(GachaSelectShopEffectDataKey),
				426
			},
			{
				typeof(GachaSelectShopProbabilityData),
				427
			},
			{
				typeof(GachaShopProbabilityData),
				428
			},
			{
				typeof(GachaShopProbabilityDataKey),
				429
			},
			{
				typeof(GameGuideData),
				430
			},
			{
				typeof(GameMiniGuideData),
				431
			},
			{
				typeof(GameOptionData),
				432
			},
			{
				typeof(GameTitleSceneIntroduceData),
				433
			},
			{
				typeof(GatewayToTrialData),
				434
			},
			{
				typeof(GatewayToTrialDataContainer.ContainerInfo),
				435
			},
			{
				typeof(GetNewHeroSceneBundleData),
				436
			},
			{
				typeof(GlobalBanPickTotalRewardData),
				437
			},
			{
				typeof(GodkingTrialData),
				438
			},
			{
				typeof(GodkingTrialGroupData),
				439
			},
			{
				typeof(GuildArenaSeasonData),
				440
			},
			{
				typeof(GuildArenaSeasonRewardData),
				441
			},
			{
				typeof(GuildArenaServerBuffRewardData),
				442
			},
			{
				typeof(GuildArenaTierData),
				443
			},
			{
				typeof(GuildAttendanceRewardData),
				444
			},
			{
				typeof(GuildAttendanceRewardDataKey),
				445
			},
			{
				typeof(GuildBuildingData),
				446
			},
			{
				typeof(GuildBuildingDataKey),
				447
			},
			{
				typeof(GuildContributeRewardData),
				448
			},
			{
				typeof(GuildLevelData),
				449
			},
			{
				typeof(GuildPenaltyData),
				450
			},
			{
				typeof(GuildRaidBonusDungeonData),
				451
			},
			{
				typeof(GuildRaidBonusDungeonDataKey),
				452
			},
			{
				typeof(GuildRaidChapterData),
				453
			},
			{
				typeof(GuildRaidChapterRewardData),
				454
			},
			{
				typeof(GuildRaidChapterRewardDataKey),
				455
			},
			{
				typeof(GuildRaidDummyDungeonData),
				456
			},
			{
				typeof(GuildRaidDungeonData),
				457
			},
			{
				typeof(GuildRaidDungeonDataContainer.GuildRaidStepKey),
				458
			},
			{
				typeof(GuildRaidDungeonDataKey),
				459
			},
			{
				typeof(GuildSkillData),
				460
			},
			{
				typeof(GuildSkillDataBase),
				461
			},
			{
				typeof(GuildSkillLevelData),
				462
			},
			{
				typeof(GuildSkillLevelKey2),
				463
			},
			{
				typeof(GuildSuppressChapterData),
				464
			},
			{
				typeof(GuildSuppressDungeonData),
				465
			},
			{
				typeof(GuildSuppressRewardData),
				466
			},
			{
				typeof(GuildSuppressSessionData),
				467
			},
			{
				typeof(HairCostumeData),
				468
			},
			{
				typeof(HashString),
				469
			},
			{
				typeof(HelpListData),
				470
			},
			{
				typeof(HelpListDataKey),
				471
			},
			{
				typeof(HeroAnimationViewData),
				472
			},
			{
				typeof(HeroAwakeData),
				473
			},
			{
				typeof(HeroBonusData),
				474
			},
			{
				typeof(HeroBonusDataKey),
				475
			},
			{
				typeof(HeroFriendlyActionData),
				476
			},
			{
				typeof(HeroFriendlyData),
				477
			},
			{
				typeof(HeroGrowthItemData),
				478
			},
			{
				typeof(HeroLimitBreakData),
				479
			},
			{
				typeof(HeroLimitBreakDataKey),
				480
			},
			{
				typeof(HeroLimitBreakExpItemData),
				481
			},
			{
				typeof(HeroPresetStorageSlotData),
				482
			},
			{
				typeof(HeroPresetStorageSlotDataContainer.ContainerInfo),
				483
			},
			{
				typeof(HeroProfileData),
				484
			},
			{
				typeof(HeroSelectItemData),
				485
			},
			{
				typeof(HeroVoiceData),
				486
			},
			{
				typeof(HeroVoiceDataKey),
				487
			},
			{
				typeof(HonoraryChamberMenuData),
				488
			},
			{
				typeof(IconChatWindowData),
				489
			},
			{
				typeof(InfinityData),
				490
			},
			{
				typeof(InfinityRewardData),
				491
			},
			{
				typeof(InfinityRewardGroupData),
				492
			},
			{
				typeof(InspectUserData),
				493
			},
			{
				typeof(InspectUserKey),
				494
			},
			{
				typeof(InventoryExtendData),
				495
			},
			{
				typeof(ItemBaseData),
				496
			},
			{
				typeof(ItemBreakData),
				497
			},
			{
				typeof(ItemEffectWidgetData),
				498
			},
			{
				typeof(ItemEffectWidgetDataKey),
				499
			},
			{
				typeof(ItemGradeData),
				500
			},
			{
				typeof(ItemGroupData),
				501
			},
			{
				typeof(ItemGroupData.SubData),
				502
			},
			{
				typeof(ItemProbabilityData),
				503
			},
			{
				typeof(JukeboxData),
				504
			},
			{
				typeof(KarmaCardData),
				505
			},
			{
				typeof(KarmaDungeonData),
				506
			},
			{
				typeof(KarmaDungeonDataKey),
				507
			},
			{
				typeof(KarmaEnemyPowerUpData),
				508
			},
			{
				typeof(KarmaEnemyUnlockSkillData),
				509
			},
			{
				typeof(KarmaEnemyUnlockSkillKey),
				510
			},
			{
				typeof(KarmaGaugeExponentialData),
				511
			},
			{
				typeof(KarmaGaugeExponentialKey),
				512
			},
			{
				typeof(KarmaRewardData),
				513
			},
			{
				typeof(KarmaRewardExponentialData),
				514
			},
			{
				typeof(KarmaRewardExponentialKey),
				515
			},
			{
				typeof(KarmaRewardKey),
				516
			},
			{
				typeof(KingsPassExpData),
				517
			},
			{
				typeof(KingsPassExpKey),
				518
			},
			{
				typeof(KingsPassLevelData),
				519
			},
			{
				typeof(KingsPassLevelKey),
				520
			},
			{
				typeof(KingsPassMissionKey2),
				521
			},
			{
				typeof(KingsPassSeasonData),
				522
			},
			{
				typeof(KingsPassSeasonMissionData),
				523
			},
			{
				typeof(KingsPassSeasonMissionDataContainer.RewardKey2),
				524
			},
			{
				typeof(LanguageData),
				525
			},
			{
				typeof(LanguagePatternData),
				526
			},
			{
				typeof(LanguagePatternKey),
				527
			},
			{
				typeof(LCSkillNameData),
				528
			},
			{
				typeof(LeftMenuButtonData),
				529
			},
			{
				typeof(LevelStatData),
				530
			},
			{
				typeof(LoadingData),
				531
			},
			{
				typeof(LoadingDataContainer.BattleLoadingInfo),
				532
			},
			{
				typeof(LoadingDataContainer.NormalLoadingInfo),
				533
			},
			{
				typeof(LocalizationValueData),
				534
			},
			{
				typeof(LuckyArenaDeckPresetData),
				535
			},
			{
				typeof(LuckyArenaRewardData),
				536
			},
			{
				typeof(LuckyArenaRewardDataKey),
				537
			},
			{
				typeof(MainQuestData),
				538
			},
			{
				typeof(MainStoryChapterData),
				539
			},
			{
				typeof(MainStoryData),
				540
			},
			{
				typeof(MainStoryKey),
				541
			},
			{
				typeof(MatchBanPickRewardData),
				542
			},
			{
				typeof(MatchBanPickSeasonRewardData),
				543
			},
			{
				typeof(MatchBanPickStateData),
				544
			},
			{
				typeof(MatchBanPickTierData),
				545
			},
			{
				typeof(MatchBanPickTierRewardData),
				546
			},
			{
				typeof(MatchLuckyTierData),
				547
			},
			{
				typeof(MatchRewardData),
				548
			},
			{
				typeof(MatchRewardDataKey2),
				549
			},
			{
				typeof(MatchSeasonData),
				550
			},
			{
				typeof(MatchTierData),
				551
			},
			{
				typeof(MazeClearRewardData),
				552
			},
			{
				typeof(MazeInfoData),
				553
			},
			{
				typeof(MemoryFragmentData),
				554
			},
			{
				typeof(MissionCategoryData),
				555
			},
			{
				typeof(MissionCategoryKey),
				556
			},
			{
				typeof(MissionVisualData),
				557
			},
			{
				typeof(MissionVisualKey),
				558
			},
			{
				typeof(MonsterTierData),
				559
			},
			{
				typeof(MonthlyHeroData),
				560
			},
			{
				typeof(MultipleHeroSelectItemData),
				561
			},
			{
				typeof(NewbieRewardAttendanceData),
				562
			},
			{
				typeof(NewbieRewardBannerUrlData),
				563
			},
			{
				typeof(NewbieRewardBannerUrlKey),
				564
			},
			{
				typeof(NewbieRewardProductData),
				565
			},
			{
				typeof(NewbieRewardProductMissionData),
				566
			},
			{
				typeof(NewbieRewardTapOpenData),
				567
			},
			{
				typeof(NewEquipGachaData),
				568
			},
			{
				typeof(NewPayShopBannerData),
				569
			},
			{
				typeof(NewPayShopGroupData),
				570
			},
			{
				typeof(NewPayShopOpenConditionData),
				571
			},
			{
				typeof(NewPayShopProductData),
				572
			},
			{
				typeof(NewPayShopProductDataKey2),
				573
			},
			{
				typeof(NewPayShopTagData),
				574
			},
			{
				typeof(Sample_JitKey_2),
				575
			},
			{
				typeof(Sample_JitKey_3),
				576
			},
			{
				typeof(Sample_JitKey_4),
				577
			},
			{
				typeof(Sample_JitKey_5),
				578
			},
			{
				typeof(NPCData),
				579
			},
			{
				typeof(NPCFriendlyActionData),
				580
			},
			{
				typeof(NPCFriendlyActionKey2),
				581
			},
			{
				typeof(NPCFriendlyPointData),
				582
			},
			{
				typeof(NPCFriendlyPointDataKey),
				583
			},
			{
				typeof(NPCFriendlyPointItemData),
				584
			},
			{
				typeof(EventData),
				585
			},
			{
				typeof(OperationData.LoadData),
				586
			},
			{
				typeof(OptionSellData),
				587
			},
			{
				typeof(OptionSellKey),
				588
			},
			{
				typeof(OrdealArenaBuffData),
				589
			},
			{
				typeof(OrdealArenaEventData),
				590
			},
			{
				typeof(OrdealArenaNodeData),
				591
			},
			{
				typeof(OrdealArenaRewardAreaData),
				592
			},
			{
				typeof(OrdealArenaRewardAreaDataKey),
				593
			},
			{
				typeof(OrdealArenaRewardRatingData),
				594
			},
			{
				typeof(OrdealArenaTierData),
				595
			},
			{
				typeof(PackageItemData),
				596
			},
			{
				typeof(PartyDungeonData),
				597
			},
			{
				typeof(PatchDownloadSpecData),
				598
			},
			{
				typeof(PayShopDiscountData),
				599
			},
			{
				typeof(PayShopDiscountGroupData),
				600
			},
			{
				typeof(PayShopDiscountGroupKey),
				601
			},
			{
				typeof(PcAvatarData),
				602
			},
			{
				typeof(PcAvatarDataKey),
				603
			},
			{
				typeof(PetAdventureData),
				604
			},
			{
				typeof(PetAdventureRewardData),
				605
			},
			{
				typeof(PetAwakenData),
				606
			},
			{
				typeof(PetAwakenDataKey),
				607
			},
			{
				typeof(PetBonusData),
				608
			},
			{
				typeof(PetCommonOptionOrderData),
				609
			},
			{
				typeof(PetEggData),
				610
			},
			{
				typeof(PetIncubatorData),
				611
			},
			{
				typeof(PetIncubatorSlotData),
				612
			},
			{
				typeof(PetOptionData),
				613
			},
			{
				typeof(PetSelectItemData),
				614
			},
			{
				typeof(PetTierInteractiveData),
				615
			},
			{
				typeof(PetUpgradeTierData),
				616
			},
			{
				typeof(PhaseStepRewardData),
				617
			},
			{
				typeof(PhaseStepRewardDataKey),
				618
			},
			{
				typeof(PlayerArchiveData),
				619
			},
			{
				typeof(PlayerArchiveDataKey),
				620
			},
			{
				typeof(PlayGuideData),
				621
			},
			{
				typeof(PlayRecordData),
				622
			},
			{
				typeof(PortalData),
				623
			},
			{
				typeof(PortalPropData),
				624
			},
			{
				typeof(PortalRenewalData),
				625
			},
			{
				typeof(PortalRenewalDataKey),
				626
			},
			{
				typeof(PotionItemData),
				627
			},
			{
				typeof(ProbabilityBaseData),
				628
			},
			{
				typeof(PunishmentGroupData),
				629
			},
			{
				typeof(PunishmentGroupDataKey),
				630
			},
			{
				typeof(PunishmentRaidData),
				631
			},
			{
				typeof(PunishmentRaidRewardData),
				632
			},
			{
				typeof(PunishmentRaidRewardDataContainer.RewardAndIndexKey),
				633
			},
			{
				typeof(PunishmentRaidTriggerData),
				634
			},
			{
				typeof(PunishmentRuneData),
				635
			},
			{
				typeof(PunishmentRuneDataContainer.PunishmentRuneKey),
				636
			},
			{
				typeof(PunishmentRuneItemBreakData),
				637
			},
			{
				typeof(PunishmentRuneItemCountData),
				638
			},
			{
				typeof(PunishmentRuneOptionData),
				639
			},
			{
				typeof(PurchaseDungeonData),
				640
			},
			{
				typeof(PurchaseMarketingData),
				641
			},
			{
				typeof(PurchaseMarketingPopupData),
				642
			},
			{
				typeof(PushRewardData),
				643
			},
			{
				typeof(RaidData),
				644
			},
			{
				typeof(RaidDataKey2),
				645
			},
			{
				typeof(RaidMultiInfoData),
				646
			},
			{
				typeof(RelatedStatData),
				647
			},
			{
				typeof(RewardData),
				648
			},
			{
				typeof(RewardData.SubData),
				649
			},
			{
				typeof(RewardItemIconOrderData),
				650
			},
			{
				typeof(RewardItemIconTypeDataKey),
				651
			},
			{
				typeof(RouletteData),
				652
			},
			{
				typeof(RouletteRewardData),
				653
			},
			{
				typeof(RuneItemBreakData),
				654
			},
			{
				typeof(RuneItemData),
				655
			},
			{
				typeof(SelectItemData),
				656
			},
			{
				typeof(SelectRewardData),
				657
			},
			{
				typeof(ServerGroupNameData),
				658
			},
			{
				typeof(ShakmehBossData),
				659
			},
			{
				typeof(ShakmehDungeonData),
				660
			},
			{
				typeof(ShakmehDungeonKey),
				661
			},
			{
				typeof(ShardFlaskItemData),
				662
			},
			{
				typeof(ShardItemData),
				663
			},
			{
				typeof(ShopAndIndexKey),
				664
			},
			{
				typeof(ShopData),
				665
			},
			{
				typeof(ShopItemData),
				666
			},
			{
				typeof(ShopItemFilterData),
				667
			},
			{
				typeof(ShopItemGroupData),
				668
			},
			{
				typeof(ShopItemGroupKey),
				669
			},
			{
				typeof(ShopItemSortData),
				670
			},
			{
				typeof(ShopItemSortGroupData),
				671
			},
			{
				typeof(SimilarAlphabetData),
				672
			},
			{
				typeof(SkillData),
				673
			},
			{
				typeof(SkillLevelFactorData),
				674
			},
			{
				typeof(SkillPriceData),
				675
			},
			{
				typeof(SkillPriceKey),
				676
			},
			{
				typeof(SkillProjectileData),
				677
			},
			{
				typeof(SkillProjectileGroupData),
				678
			},
			{
				typeof(SoulBreakData),
				679
			},
			{
				typeof(SoulBreakData.ItemCodeMinMaxData),
				680
			},
			{
				typeof(SoulBreakKey),
				681
			},
			{
				typeof(SoulStoneRestoreData),
				682
			},
			{
				typeof(SoulWeaponAbilityRewardData),
				683
			},
			{
				typeof(SoulWeaponAbilityRewardKey),
				684
			},
			{
				typeof(SoulWeaponAbilityTicketData),
				685
			},
			{
				typeof(SoulWeaponEtherData),
				686
			},
			{
				typeof(SoulWeaponEtherRateData),
				687
			},
			{
				typeof(SoulWeaponEtherRateKey),
				688
			},
			{
				typeof(SoulWeaponNodeTreeData),
				689
			},
			{
				typeof(SoulWeaponOptionData),
				690
			},
			{
				typeof(SoulWeaponReinforceData),
				691
			},
			{
				typeof(SoulWeaponSkillData),
				692
			},
			{
				typeof(SoulWeaponUpgradeData),
				693
			},
			{
				typeof(SoulWeaponUpgradePriceData),
				694
			},
			{
				typeof(SoulWeaponUpgradePriceKey),
				695
			},
			{
				typeof(StaminaConsumePointData),
				696
			},
			{
				typeof(StaminaData),
				697
			},
			{
				typeof(StatAttenuationData),
				698
			},
			{
				typeof(StatAttributeData),
				699
			},
			{
				typeof(StatData),
				700
			},
			{
				typeof(StateActionData),
				701
			},
			{
				typeof(StateActionDataContainer.ConditionKey),
				702
			},
			{
				typeof(StateData),
				703
			},
			{
				typeof(StateToolTipData),
				704
			},
			{
				typeof(StepUpGachaRewardData),
				705
			},
			{
				typeof(StepUpGachaRewardDataContainer.GachaRoundKey),
				706
			},
			{
				typeof(StepupGachaRewardDataKey),
				707
			},
			{
				typeof(StorageSlotExtendData),
				708
			},
			{
				typeof(StoreCurrencyUnitData),
				709
			},
			{
				typeof(SubQuestData),
				710
			},
			{
				typeof(SubQuestDataKey),
				711
			},
			{
				typeof(SubStoryData),
				712
			},
			{
				typeof(SubStoryDataContainer.ChapterInfo),
				713
			},
			{
				typeof(SubStoryEventData),
				714
			},
			{
				typeof(SubStoryEventKey),
				715
			},
			{
				typeof(TeamLevelBonusBuffOptionData),
				716
			},
			{
				typeof(TeamLevelBuffOptionData),
				717
			},
			{
				typeof(TeamLevelCommonOptionOrderData),
				718
			},
			{
				typeof(TeamLevelData),
				719
			},
			{
				typeof(ThreatData),
				720
			},
			{
				typeof(ThreatDataContainer.ThreatKey),
				721
			},
			{
				typeof(TowerData),
				722
			},
			{
				typeof(TowerFloorData),
				723
			},
			{
				typeof(TowerFloorKey),
				724
			},
			{
				typeof(TreasureHouseDungeonData),
				725
			},
			{
				typeof(TreasureHouseDungeonKey),
				726
			},
			{
				typeof(TreasureHouseInfoData),
				727
			},
			{
				typeof(TutorialData),
				728
			},
			{
				typeof(TutorialKey),
				729
			},
			{
				typeof(UnderPrisonData),
				730
			},
			{
				typeof(UnderPrisonDungeonData),
				731
			},
			{
				typeof(UnderPrisonDungeonKey),
				732
			},
			{
				typeof(UpgradeBookItemData),
				733
			},
			{
				typeof(UpgradeBookItemDataContainer.TagGradeKey),
				734
			},
			{
				typeof(ValanceAwakenData),
				735
			},
			{
				typeof(ValanceAwakenDataKey),
				736
			},
			{
				typeof(ValanceCraftData),
				737
			},
			{
				typeof(ValanceCraftDataKey2),
				738
			},
			{
				typeof(ValanceCreatureFilterData),
				739
			},
			{
				typeof(ValanceEnchantData),
				740
			},
			{
				typeof(ValanceEnchantDataKey),
				741
			},
			{
				typeof(ValanceEnchantOptionCountData),
				742
			},
			{
				typeof(ValanceEnchantOptionCountDataKey),
				743
			},
			{
				typeof(ValanceEquipFilterData),
				744
			},
			{
				typeof(ValanceEquipFilterDataContainer.SortingOrderKey2),
				745
			},
			{
				typeof(ValanceFilterGroupData),
				746
			},
			{
				typeof(ValanceIdentifiedData),
				747
			},
			{
				typeof(VideoData),
				748
			},
			{
				typeof(WantedPathData),
				749
			},
			{
				typeof(WantedQuestData),
				750
			},
			{
				typeof(WeaponCostumeData),
				751
			},
			{
				typeof(WeaponUniqueSelectItemData),
				752
			},
			{
				typeof(WorldBossDailyAchievemenDataKey),
				753
			},
			{
				typeof(WorldBossDailyAchievementData),
				754
			},
			{
				typeof(WorldBossData),
				755
			},
			{
				typeof(WorldBossRewardData),
				756
			},
			{
				typeof(WorldBossScoreRewardData),
				757
			},
			{
				typeof(WorldBossScoreRewardDataKey),
				758
			},
			{
				typeof(WorldMapBuildData),
				759
			},
			{
				typeof(WorldMapPropData),
				760
			},
			{
				typeof(XCodeLocalizationData),
				761
			}
		};
	}

	internal static object GetFormatter(Type t)
	{
		if (!lookup.TryGetValue(t, out var value))
		{
			return null;
		}
		return value switch
		{
			0 => new ArrayFormatter<AttackAttrType>(), 
			1 => new ArrayFormatter<BattleRetryConditionType>(), 
			2 => new ArrayFormatter<BattleType>(), 
			3 => new ArrayFormatter<ContentsStateIcon>(), 
			4 => new ArrayFormatter<CreatureTagType>(), 
			5 => new ArrayFormatter<EquipItemSubType>(), 
			6 => new ArrayFormatter<EquipPartType>(), 
			7 => new ArrayFormatter<ERightMenuButton>(), 
			8 => new ArrayFormatter<EventDungeonPropType>(), 
			9 => new ArrayFormatter<GetNewHeroInfoPosition>(), 
			10 => new ArrayFormatter<ItemType>(), 
			11 => new ArrayFormatter<MatchSeasonType>(), 
			12 => new ArrayFormatter<OperationFlag>(), 
			13 => new ArrayFormatter<PatchSpecType>(), 
			14 => new ArrayFormatter<RuneSlotType>(), 
			15 => new ArrayFormatter<ShopCostType>(), 
			16 => new ArrayFormatter<SkillAttrType>(), 
			17 => new ArrayFormatter<SkillTargetType>(), 
			18 => new ArrayFormatter<SkillTriggerFlag>(), 
			19 => new ArrayFormatter<StaminaType>(), 
			20 => new ArrayFormatter<StatType>(), 
			21 => new ListFormatter<CampaignWaveData.CreatureDetailWritable>(), 
			22 => new ListFormatter<ItemGroupData.SubData>(), 
			23 => new ListFormatter<RewardData.SubData>(), 
			24 => new ListFormatter<SoulBreakData.ItemCodeMinMaxData>(), 
			25 => new ListFormatter<int>(), 
			26 => new ListFormatter<string>(), 
			27 => new AccessoryPartPivotTypeFormatter(), 
			28 => new AccessoryPartTypeFormatter(), 
			29 => new AchievementCategoryTypeFormatter(), 
			30 => new AchievementReqSubTypeFormatter(), 
			31 => new AchievementReqTypeFormatter(), 
			32 => new AchievementRewardTypeFormatter(), 
			33 => new AchievementSubTypeFormatter(), 
			34 => new AchievementTypeFormatter(), 
			35 => new ArchiveMainTypeFormatter(), 
			36 => new ArchiveSubTypeFormatter(), 
			37 => new ArchiveSubType2Formatter(), 
			38 => new ArchiveTypeFormatter(), 
			39 => new AttackAttrTypeFormatter(), 
			40 => new BannerPlayTypesFormatter(), 
			41 => new BattleRetryConditionTypeFormatter(), 
			42 => new BattleTacticTypeFormatter(), 
			43 => new BattleTypeFormatter(), 
			44 => new BossWarningData_ETypeFormatter(), 
			45 => new BossWarningData_EWarningTypeFormatter(), 
			46 => new CalendarCutSceneTypeFormatter(), 
			47 => new CampaignNodeData_EMenuOpenConditionFormatter(), 
			48 => new CampaignNodeData_ENodeOpenStyleFormatter(), 
			49 => new CampaignWaveData_EWaveFlagFormatter(), 
			50 => new CcMotionTypeFormatter(), 
			51 => new ChapterTypeFormatter(), 
			52 => new ClearMissionProvideTypeFormatter(), 
			53 => new ClearMissionPurchaseTypeFormatter(), 
			54 => new ClearMissionTypeFormatter(), 
			55 => new ClientActivityTypeFormatter(), 
			56 => new ContentsNoticePopupTypeFormatter(), 
			57 => new ContentsNoticeTypeFormatter(), 
			58 => new ContentsStateIconFormatter(), 
			59 => new ContentsStatusSubTypeFormatter(), 
			60 => new ContentsStatusTypeFormatter(), 
			61 => new ContentsTypeFormatter(), 
			62 => new CostumeAbilityTypeFormatter(), 
			63 => new CraftSortCategoryFormatter(), 
			64 => new CreatureOpenStatusTypeFormatter(), 
			65 => new CreatureOpenTypeFormatter(), 
			66 => new CreaturePositionTypeFormatter(), 
			67 => new CreatureRaceTypeFormatter(), 
			68 => new CreatureTagTypeFormatter(), 
			69 => new CreatureTransformTypeFormatter(), 
			70 => new CurrencyChangeTypeFormatter(), 
			71 => new CurrencyShowTypeFormatter(), 
			72 => new CurrencyTypeFormatter(), 
			73 => new DamageMeterFlagFormatter(), 
			74 => new DelayedSpawnTypeFormatter(), 
			75 => new DeviceStyleTypeFormatter(), 
			76 => new DispatchStateFormatter(), 
			77 => new DispatchTypeFormatter(), 
			78 => new DisplayProgressTypeFormatter(), 
			79 => new DungeonDifficultyTypeFormatter(), 
			80 => new EAffixShowTypeFormatter(), 
			81 => new EAffixTypeFormatter(), 
			82 => new EAttendanceKeywordFormatter(), 
			83 => new EBattleMenuFormatter(), 
			84 => new EContentTypeFormatter(), 
			85 => new ECostumeBuffTypeFormatter(), 
			86 => new ECostumeSelectItemTypeFormatter(), 
			87 => new EDelayTypeFormatter(), 
			88 => new EDungeonGroupTypeFormatter(), 
			89 => new EEquipConditionFormatter(), 
			90 => new EEventDungeonCloseTypeFormatter(), 
			91 => new EffectWidgetTypeFormatter(), 
			92 => new EGroupEnableConditionTypeFormatter(), 
			93 => new EGuildFlagDisplayTypeFormatter(), 
			94 => new EHeroCostumeTypeFormatter(), 
			95 => new EKarmaCardTypeFormatter(), 
			96 => new EKarmaRarityFormatter(), 
			97 => new EMapArrangementFormatter(), 
			98 => new EMissionTopViewButtonGroupFormatter(), 
			99 => new EMissionTopViewLayoutFormatter(), 
			100 => new EMissionTopViewRewardTypeFormatter(), 
			101 => new EMissionTypeFormatter(), 
			102 => new EOrdealArenaNodeTypeFormatter(), 
			103 => new EOrdealEventTypeFormatter(), 
			104 => new EPetAdventureTypeFormatter(), 
			105 => new EPunishmentDungeonTypeFormatter(), 
			106 => new EPunishmentPortraitLineTypeFormatter(), 
			107 => new EquipGachaSubTypeFormatter(), 
			108 => new EquipGachaTypeFormatter(), 
			109 => new EquipItemDetailTypeFormatter(), 
			110 => new EquipItemEquipGradeFormatter(), 
			111 => new EquipItemEquipTypeFormatter(), 
			112 => new EquipItemSetGroupTypeFormatter(), 
			113 => new EquipItemSubTypeFormatter(), 
			114 => new EquipItemWorkPlaceFormatter(), 
			115 => new EquipPartTypeFormatter(), 
			116 => new EquipPartTypeFlagFormatter(), 
			117 => new EquipTransitionTypeFormatter(), 
			118 => new ERightMenuButtonFormatter(), 
			119 => new EShopItemFilterGroupTypeFormatter(), 
			120 => new EShopItemTypeFormatter(), 
			121 => new EShopSortTypeFormatter(), 
			122 => new ESoulWeaponAbilityTicketTypeFormatter(), 
			123 => new ESoulweaponLiberationResultSideFormatter(), 
			124 => new EValanceFilterGroupTypeFormatter(), 
			125 => new EventBonusTypeFormatter(), 
			126 => new EventDungeonPropTypeFormatter(), 
			127 => new EventRouletteTypeFormatter(), 
			128 => new EventStepTypeFormatter(), 
			129 => new EventTriggerCategoryFormatter(), 
			130 => new EventTriggerCommandFormatter(), 
			131 => new ExInfoTypeFormatter(), 
			132 => new FlaskRewardTypeFormatter(), 
			133 => new FlaskSourceTypeFormatter(), 
			134 => new FlaskTypeFormatter(), 
			135 => new FloorDungeonSelectTypeFormatter(), 
			136 => new FriendlyActionTypeFormatter(), 
			137 => new GameIntroduceTypeFormatter(), 
			138 => new GameOptionComponentFormatter(), 
			139 => new GameOptionTabFormatter(), 
			140 => new GetNewHeroInfoPositionFormatter(), 
			141 => new GodkingTrialDungeonGateTypeFormatter(), 
			142 => new GuildArenaTierTypeFormatter(), 
			143 => new GuildConstructionResourceTopmostMenuTypeFormatter(), 
			144 => new GuildSkillTypeFormatter(), 
			145 => new GuildSuppressServerRewardTypeFormatter(), 
			146 => new HeroSelectItemTypeFormatter(), 
			147 => new HighGachaCategoryTypeFormatter(), 
			148 => new InspectUserCategoryFormatter(), 
			149 => new InspectUserTypeFormatter(), 
			150 => new InventoryTypeFormatter(), 
			151 => new ItemActionTypeFormatter(), 
			152 => new ItemData_GetEffectTypeFormatter(), 
			153 => new ItemTypeFormatter(), 
			154 => new KingsPassTypeFormatter(), 
			155 => new LeftMenuButtonTypeFormatter(), 
			156 => new LoadingData_ECategoryFormatter(), 
			157 => new LoadingData_EGroupTypeFormatter(), 
			158 => new LuckyMatchRewardTypeFormatter(), 
			159 => new LuckyMatchTierTypeFormatter(), 
			160 => new MarketingTypeFormatter(), 
			161 => new MatchArenaTypeFormatter(), 
			162 => new MatchBanPickStateFormatter(), 
			163 => new MatchRewardTypeFormatter(), 
			164 => new MatchSeasonTypeFormatter(), 
			165 => new MatchTierTypeFormatter(), 
			166 => new MonsterGradeTypeFormatter(), 
			167 => new NewbieRewardProvideTypeFormatter(), 
			168 => new NewbieRewardPurchaseTypeFormatter(), 
			169 => new NewbieRewardTypeFormatter(), 
			170 => new NewPayShopCategoryTypeFormatter(), 
			171 => new NewPayShopProductCategoryTypeFormatter(), 
			172 => new LanguageTypeFormatter(), 
			173 => new NodeFunctionTypeFormatter(), 
			174 => new CheckTypeFormatter(), 
			175 => new EventTypeFormatter(), 
			176 => new OpenConditionTypeFormatter(), 
			177 => new OperationConditionActionTypeFormatter(), 
			178 => new OperationConditionTypeFormatter(), 
			179 => new OperationFlagFormatter(), 
			180 => new OperationGroupConditionTypeFormatter(), 
			181 => new OperationTargetSelectTypeFormatter(), 
			182 => new OperationTargetTypeFormatter(), 
			183 => new OperationTypeFormatter(), 
			184 => new OptionSellCategoryFormatter(), 
			185 => new OptionSellSelectTypeFormatter(), 
			186 => new PairEffectTypeFormatter(), 
			187 => new PartyDungeonChapterFormatter(), 
			188 => new PartyDungeonTypeFormatter(), 
			189 => new PatchPlatformFormatter(), 
			190 => new PatchSpecTypeFormatter(), 
			191 => new PayShopIndexTypeFormatter(), 
			192 => new PayShopUserGroupFormatter(), 
			193 => new PetOptionConditionTypeFormatter(), 
			194 => new PhaseStepReqTypeFormatter(), 
			195 => new PhaseStepRewardTypeFormatter(), 
			196 => new PlayGuideData_GuideTypeFormatter(), 
			197 => new PlayRecordActionCalculateTypeFormatter(), 
			198 => new PlayRecordCategoryFormatter(), 
			199 => new PlayRecordDisplayTypeFormatter(), 
			200 => new PlayRecordDisplayValueTypeFormatter(), 
			201 => new PortalPropConditionTypeFormatter(), 
			202 => new PortalPropTypeFormatter(), 
			203 => new PortalRenewalCategoryFormatter(), 
			204 => new PortalRenewalCompTypeFormatter(), 
			205 => new PortalRenewalSubCategoryFormatter(), 
			206 => new ProductTypeFormatter(), 
			207 => new PunishmentRaidRewardTypeFormatter(), 
			208 => new QuestActionTypeFormatter(), 
			209 => new QuestOpenConditionTypeFormatter(), 
			210 => new RaidBanTypeFormatter(), 
			211 => new RaidRewardTypeFormatter(), 
			212 => new RaidTypeFormatter(), 
			213 => new RankPanelTypeFormatter(), 
			214 => new RankViewTypeFormatter(), 
			215 => new RecordsOfHonorContentTypeFormatter(), 
			216 => new ResetTypeFormatter(), 
			217 => new RewardItemIconTypeFormatter(), 
			218 => new RewardTypeFormatter(), 
			219 => new RouletteTypeFormatter(), 
			220 => new RuneColorTypeFormatter(), 
			221 => new RuneSlotTypeFormatter(), 
			222 => new SelectItemPopupTypeFormatter(), 
			223 => new ShakmehBossTypeFormatter(), 
			224 => new ShopCostTypeFormatter(), 
			225 => new SkillAttrTypeFormatter(), 
			226 => new SkillCcFlagFormatter(), 
			227 => new SkillTargetFlagFormatter(), 
			228 => new SkillTargetTypeFormatter(), 
			229 => new SkillTriggerFlagFormatter(), 
			230 => new SkillTriggerTypeFormatter(), 
			231 => new StaminaTypeFormatter(), 
			232 => new StaminaUpdateTypeFormatter(), 
			233 => new StaminaValueTypeFormatter(), 
			234 => new StateActionTypeFormatter(), 
			235 => new StateConditionTypeFormatter(), 
			236 => new StateFlagFormatter(), 
			237 => new StateOverflowRuleTypeFormatter(), 
			238 => new StateStackDurationRuleTypeFormatter(), 
			239 => new StatTypeFormatter(), 
			240 => new SweepDungeonTypeFormatter(), 
			241 => new TeamLevelBuffOptionConditionTypeFormatter(), 
			242 => new TeamLevelGradeFormatter(), 
			243 => new TeamTypeFormatter(), 
			244 => new ThreatTypeFormatter(), 
			245 => new TowerTypeFormatter(), 
			246 => new UnderPrisonTypeFormatter(), 
			247 => new WinDeciderTypeFormatter(), 
			248 => new WorldBossServerRewardTypeFormatter(), 
			249 => new WorldMapPropConditionTypeFormatter(), 
			250 => new WorldMapPropTypeFormatter(), 
			251 => new AccessoryCostumeDataFormatter(), 
			252 => new AccessoryCostumeGroupDataFormatter(), 
			253 => new AccessorySelectItemDataFormatter(), 
			254 => new AccountBuffDataFormatter(), 
			255 => new AccumulateLoginRewardDataFormatter(), 
			256 => new AchievementDataFormatter(), 
			257 => new AchievementTextDataFormatter(), 
			258 => new AdjustTokenDataFormatter(), 
			259 => new AdventureNoticeDataFormatter(), 
			260 => new AffixDataFormatter(), 
			261 => new AppReviewConditionDataFormatter(), 
			262 => new ArtifactRestoreDataFormatter(), 
			263 => new AttendanceInfoDataFormatter(), 
			264 => new AttendanceRewardInfoDataFormatter(), 
			265 => new AttendanceRewardInfoKeyFormatter(), 
			266 => new AttendanceRewardUrlDataFormatter(), 
			267 => new AttendanceUrlDataFormatter(), 
			268 => new AttendanceUrlDataKeyFormatter(), 
			269 => new AutoSeparationDataFormatter(), 
			270 => new AwakenStoneDataFormatter(), 
			271 => new BanRuleDataFormatter(), 
			272 => new BattleDefineDataFormatter(), 
			273 => new BoosterDefineDataFormatter(), 
			274 => new BoosterIconActionDataFormatter(), 
			275 => new BoosterItemDataFormatter(), 
			276 => new BossWarningDataFormatter(), 
			277 => new BottomButtonDataFormatter(), 
			278 => new BottomButtonKeyFormatter(), 
			279 => new CampaignChapterDataFormatter(), 
			280 => new CampaignDungeonDataFormatter(), 
			281 => new CampaignDungeonSkillDataFormatter(), 
			282 => new CampaignFieldDataFormatter(), 
			283 => new CampaignMultiAvatarDataFormatter(), 
			284 => new CampaignMultiAvatarKeyFormatter(), 
			285 => new CampaignNodeDataFormatter(), 
			286 => new CampaignNodeNPCDataFormatter(), 
			287 => new CampaignTacticDataFormatter(), 
			288 => new CampaignTacticKeyFormatter(), 
			289 => new CampaignWaveDataFormatter(), 
			290 => new CampaignWaveData_CreatureDetailWritableFormatter(), 
			291 => new CampaignWaveDataContainer_ChapterDungeonInfoFormatter(), 
			292 => new CensoredWordDataFormatter(), 
			293 => new ChallengeRaidClearRewardDataFormatter(), 
			294 => new ChallengeRaidClearRewardKeyFormatter(), 
			295 => new ChallengeRaidDataFormatter(), 
			296 => new ChallengeRaidRewardDataFormatter(), 
			297 => new ChapterClearRewardDataFormatter(), 
			298 => new ChapterClearRewardKeyFormatter(), 
			299 => new ChapterDungeonKey2Formatter(), 
			300 => new CinemaItemDataFormatter(), 
			301 => new CinemaWindowDataFormatter(), 
			302 => new ClassBuffDataFormatter(), 
			303 => new ClassBuffDataBaseFormatter(), 
			304 => new ClearMissionProductDataFormatter(), 
			305 => new ClearMissionProductMissionDataFormatter(), 
			306 => new ClientCreatureDataFormatter(), 
			307 => new ClientCreatureKeyFormatter(), 
			308 => new ClientCustomizingCameraDataFormatter(), 
			309 => new ClientCustomizingPivotDataFormatter(), 
			310 => new ClientCustomizingPivotDataKeyFormatter(), 
			311 => new ClientStatDisplayScoreDataFormatter(), 
			312 => new CodecSupportListDataFormatter(), 
			313 => new CommunityWebViewDataFormatter(), 
			314 => new ConstantDataFormatter(), 
			315 => new ContentsNoticePopupDataFormatter(), 
			316 => new ContentsOpenConditionDataFormatter(), 
			317 => new CostumeDataFormatter(), 
			318 => new CostumeDataContainer_HeroAndStarKeyFormatter(), 
			319 => new CostumeGroupDataFormatter(), 
			320 => new CostumeSelectItemDataFormatter(), 
			321 => new CountryCodeDataFormatter(), 
			322 => new CountryCodeGroupDataFormatter(), 
			323 => new CountryDataFormatter(), 
			324 => new CountryServerDataFormatter(), 
			325 => new CraftInstantGemDataFormatter(), 
			326 => new CraftItemDataFormatter(), 
			327 => new CraftSortDataFormatter(), 
			328 => new CraftSortKeyFormatter(), 
			329 => new CreatureAwakeChallengeItemDataFormatter(), 
			330 => new CreatureDataFormatter(), 
			331 => new CreatureGradeDataFormatter(), 
			332 => new CreatureGradeDataContainer_ContainerInfoFormatter(), 
			333 => new CreatureLevelDataFormatter(), 
			334 => new CreatureLevelDataContainer_ContainerInfoFormatter(), 
			335 => new CreatureLevelFactorDataFormatter(), 
			336 => new CreatureLevelFactorDataKeyFormatter(), 
			337 => new CreatureLevelStatData2Formatter(), 
			338 => new CreatureOverlapDataFormatter(), 
			339 => new CreatureStarDataFormatter(), 
			340 => new CreatureStarDataKeyFormatter(), 
			341 => new CreatureStarGradeStatDataFormatter(), 
			342 => new CreatureStarPriceDataFormatter(), 
			343 => new CreatureStarPriceKeyFormatter(), 
			344 => new CreatureStarStatDataFormatter(), 
			345 => new CreatureStarStatDataKeyFormatter(), 
			346 => new CreatureTranscendOptionDataFormatter(), 
			347 => new CreatureVisualDataFormatter(), 
			348 => new CSMailDataFormatter(), 
			349 => new CurrencyTypeDataFormatter(), 
			350 => new CustomEquipItemDataFormatter(), 
			351 => new CutSceneForegroundDataFormatter(), 
			352 => new DbLogDataFormatter(), 
			353 => new DbLogDataKeyFormatter(), 
			354 => new DefenceLevelDataFormatter(), 
			355 => new DeviceModelDataFormatter(), 
			356 => new DispatchStateDataFormatter(), 
			357 => new DOWDungeonDataFormatter(), 
			358 => new DOWDungeonTextDataFormatter(), 
			359 => new DOWDungeonTextKeyFormatter(), 
			360 => new DownloadControlDataFormatter(), 
			361 => new DungeonHeroListSetDataFormatter(), 
			362 => new DungeonHeroListSetKeyFormatter(), 
			363 => new DungeonHeroSetDataFormatter(), 
			364 => new DungeonHeroSetKeyFormatter(), 
			365 => new DungeonPassiveEnvironmentDataFormatter(), 
			366 => new EclipseRewardDataFormatter(), 
			367 => new EclipseStartDataFormatter(), 
			368 => new EmoticonDataFormatter(), 
			369 => new EmoticonTabDataFormatter(), 
			370 => new EntryMenuSettingDataFormatter(), 
			371 => new EquipAutoDataFormatter(), 
			372 => new EquipAutoKeyFormatter(), 
			373 => new EquipAwakenPointDataFormatter(), 
			374 => new EquipAwakenPriceDataFormatter(), 
			375 => new EquipAwakenPriceDataKeyFormatter(), 
			376 => new EquipAwakenRatioDataFormatter(), 
			377 => new EquipAwakenRatioDataKeyFormatter(), 
			378 => new EquipEnchantableConditionDataFormatter(), 
			379 => new EquipEnchantableConditionKeyFormatter(), 
			380 => new EquipEnchantOptionItemDataFormatter(), 
			381 => new EquipFilterOptionDataFormatter(), 
			382 => new EquipFilterOptionGroupDataFormatter(), 
			383 => new EquipGachaClientDataFormatter(), 
			384 => new EquipGachaClientDataKeyFormatter(), 
			385 => new EquipItemBreakDataFormatter(), 
			386 => new EquipItemBreakDataKeyFormatter(), 
			387 => new EquipItemDataFormatter(), 
			388 => new EquipItemDetailDataFormatter(), 
			389 => new EquipItemLevelDataFormatter(), 
			390 => new EquipItemLevelDataContainer_ContainerInfoFormatter(), 
			391 => new EquipItemStarDataFormatter(), 
			392 => new EquipOptionChangeDataFormatter(), 
			393 => new EquipOptionChangeDataKeyFormatter(), 
			394 => new EquipOptionDataFormatter(), 
			395 => new EquipOptionGroupDataFormatter(), 
			396 => new EquipOptionUpgradeDataFormatter(), 
			397 => new EquipOptionUpgradeDataKeyFormatter(), 
			398 => new EquipSetBonusDataFormatter(), 
			399 => new EquipSlotDataFormatter(), 
			400 => new EquipTransitionTicketDataFormatter(), 
			401 => new EquipUpgradeExpDataFormatter(), 
			402 => new EquipUpgradePriceDataFormatter(), 
			403 => new EquipUpgradePriceDataKeyFormatter(), 
			404 => new EquipUpgradeStatDataFormatter(), 
			405 => new EquipUpgradeTierDataFormatter(), 
			406 => new EventCraftDataFormatter(), 
			407 => new EventCraftEffectDataFormatter(), 
			408 => new EventDungeonDataFormatter(), 
			409 => new EventDungeonGroupDataFormatter(), 
			410 => new EventDungeonGroupKeyFormatter(), 
			411 => new EventDungeonKeyFormatter(), 
			412 => new EventForgeDataFormatter(), 
			413 => new EventRouletteDataFormatter(), 
			414 => new EventStepDataFormatter(), 
			415 => new EventTriggerDataFormatter(), 
			416 => new EventWorldBossDataFormatter(), 
			417 => new EventWorldBossDataKeyFormatter(), 
			418 => new EventWorldBossSeasonDataFormatter(), 
			419 => new ExpressionDataFormatter(), 
			420 => new FlaskItemDataFormatter(), 
			421 => new FrameControlDataFormatter(), 
			422 => new GachaProbabilityDataFormatter(), 
			423 => new GachaSelectItemDataFormatter(), 
			424 => new GachaSelectShopDataFormatter(), 
			425 => new GachaSelectShopEffectDataFormatter(), 
			426 => new GachaSelectShopEffectDataKeyFormatter(), 
			427 => new GachaSelectShopProbabilityDataFormatter(), 
			428 => new GachaShopProbabilityDataFormatter(), 
			429 => new GachaShopProbabilityDataKeyFormatter(), 
			430 => new GameGuideDataFormatter(), 
			431 => new GameMiniGuideDataFormatter(), 
			432 => new GameOptionDataFormatter(), 
			433 => new GameTitleSceneIntroduceDataFormatter(), 
			434 => new GatewayToTrialDataFormatter(), 
			435 => new GatewayToTrialDataContainer_ContainerInfoFormatter(), 
			436 => new GetNewHeroSceneBundleDataFormatter(), 
			437 => new GlobalBanPickTotalRewardDataFormatter(), 
			438 => new GodkingTrialDataFormatter(), 
			439 => new GodkingTrialGroupDataFormatter(), 
			440 => new GuildArenaSeasonDataFormatter(), 
			441 => new GuildArenaSeasonRewardDataFormatter(), 
			442 => new GuildArenaServerBuffRewardDataFormatter(), 
			443 => new GuildArenaTierDataFormatter(), 
			444 => new GuildAttendanceRewardDataFormatter(), 
			445 => new GuildAttendanceRewardDataKeyFormatter(), 
			446 => new GuildBuildingDataFormatter(), 
			447 => new GuildBuildingDataKeyFormatter(), 
			448 => new GuildContributeRewardDataFormatter(), 
			449 => new GuildLevelDataFormatter(), 
			450 => new GuildPenaltyDataFormatter(), 
			451 => new GuildRaidBonusDungeonDataFormatter(), 
			452 => new GuildRaidBonusDungeonDataKeyFormatter(), 
			453 => new GuildRaidChapterDataFormatter(), 
			454 => new GuildRaidChapterRewardDataFormatter(), 
			455 => new GuildRaidChapterRewardDataKeyFormatter(), 
			456 => new GuildRaidDummyDungeonDataFormatter(), 
			457 => new GuildRaidDungeonDataFormatter(), 
			458 => new GuildRaidDungeonDataContainer_GuildRaidStepKeyFormatter(), 
			459 => new GuildRaidDungeonDataKeyFormatter(), 
			460 => new GuildSkillDataFormatter(), 
			461 => new GuildSkillDataBaseFormatter(), 
			462 => new GuildSkillLevelDataFormatter(), 
			463 => new GuildSkillLevelKey2Formatter(), 
			464 => new GuildSuppressChapterDataFormatter(), 
			465 => new GuildSuppressDungeonDataFormatter(), 
			466 => new GuildSuppressRewardDataFormatter(), 
			467 => new GuildSuppressSessionDataFormatter(), 
			468 => new HairCostumeDataFormatter(), 
			469 => new HashStringFormatter(), 
			470 => new HelpListDataFormatter(), 
			471 => new HelpListDataKeyFormatter(), 
			472 => new HeroAnimationViewDataFormatter(), 
			473 => new HeroAwakeDataFormatter(), 
			474 => new HeroBonusDataFormatter(), 
			475 => new HeroBonusDataKeyFormatter(), 
			476 => new HeroFriendlyActionDataFormatter(), 
			477 => new HeroFriendlyDataFormatter(), 
			478 => new HeroGrowthItemDataFormatter(), 
			479 => new HeroLimitBreakDataFormatter(), 
			480 => new HeroLimitBreakDataKeyFormatter(), 
			481 => new HeroLimitBreakExpItemDataFormatter(), 
			482 => new HeroPresetStorageSlotDataFormatter(), 
			483 => new HeroPresetStorageSlotDataContainer_ContainerInfoFormatter(), 
			484 => new HeroProfileDataFormatter(), 
			485 => new HeroSelectItemDataFormatter(), 
			486 => new HeroVoiceDataFormatter(), 
			487 => new HeroVoiceDataKeyFormatter(), 
			488 => new HonoraryChamberMenuDataFormatter(), 
			489 => new IconChatWindowDataFormatter(), 
			490 => new InfinityDataFormatter(), 
			491 => new InfinityRewardDataFormatter(), 
			492 => new InfinityRewardGroupDataFormatter(), 
			493 => new InspectUserDataFormatter(), 
			494 => new InspectUserKeyFormatter(), 
			495 => new InventoryExtendDataFormatter(), 
			496 => new ItemBaseDataFormatter(), 
			497 => new ItemBreakDataFormatter(), 
			498 => new ItemEffectWidgetDataFormatter(), 
			499 => new ItemEffectWidgetDataKeyFormatter(), 
			500 => new ItemGradeDataFormatter(), 
			501 => new ItemGroupDataFormatter(), 
			502 => new ItemGroupData_SubDataFormatter(), 
			503 => new ItemProbabilityDataFormatter(), 
			504 => new JukeboxDataFormatter(), 
			505 => new KarmaCardDataFormatter(), 
			506 => new KarmaDungeonDataFormatter(), 
			507 => new KarmaDungeonDataKeyFormatter(), 
			508 => new KarmaEnemyPowerUpDataFormatter(), 
			509 => new KarmaEnemyUnlockSkillDataFormatter(), 
			510 => new KarmaEnemyUnlockSkillKeyFormatter(), 
			511 => new KarmaGaugeExponentialDataFormatter(), 
			512 => new KarmaGaugeExponentialKeyFormatter(), 
			513 => new KarmaRewardDataFormatter(), 
			514 => new KarmaRewardExponentialDataFormatter(), 
			515 => new KarmaRewardExponentialKeyFormatter(), 
			516 => new KarmaRewardKeyFormatter(), 
			517 => new KingsPassExpDataFormatter(), 
			518 => new KingsPassExpKeyFormatter(), 
			519 => new KingsPassLevelDataFormatter(), 
			520 => new KingsPassLevelKeyFormatter(), 
			521 => new KingsPassMissionKey2Formatter(), 
			522 => new KingsPassSeasonDataFormatter(), 
			523 => new KingsPassSeasonMissionDataFormatter(), 
			524 => new KingsPassSeasonMissionDataContainer_RewardKey2Formatter(), 
			525 => new LanguageDataFormatter(), 
			526 => new LanguagePatternDataFormatter(), 
			527 => new LanguagePatternKeyFormatter(), 
			528 => new LCSkillNameDataFormatter(), 
			529 => new LeftMenuButtonDataFormatter(), 
			530 => new LevelStatDataFormatter(), 
			531 => new LoadingDataFormatter(), 
			532 => new LoadingDataContainer_BattleLoadingInfoFormatter(), 
			533 => new LoadingDataContainer_NormalLoadingInfoFormatter(), 
			534 => new LocalizationValueDataFormatter(), 
			535 => new LuckyArenaDeckPresetDataFormatter(), 
			536 => new LuckyArenaRewardDataFormatter(), 
			537 => new LuckyArenaRewardDataKeyFormatter(), 
			538 => new MainQuestDataFormatter(), 
			539 => new MainStoryChapterDataFormatter(), 
			540 => new MainStoryDataFormatter(), 
			541 => new MainStoryKeyFormatter(), 
			542 => new MatchBanPickRewardDataFormatter(), 
			543 => new MatchBanPickSeasonRewardDataFormatter(), 
			544 => new MatchBanPickStateDataFormatter(), 
			545 => new MatchBanPickTierDataFormatter(), 
			546 => new MatchBanPickTierRewardDataFormatter(), 
			547 => new MatchLuckyTierDataFormatter(), 
			548 => new MatchRewardDataFormatter(), 
			549 => new MatchRewardDataKey2Formatter(), 
			550 => new MatchSeasonDataFormatter(), 
			551 => new MatchTierDataFormatter(), 
			552 => new MazeClearRewardDataFormatter(), 
			553 => new MazeInfoDataFormatter(), 
			554 => new MemoryFragmentDataFormatter(), 
			555 => new MissionCategoryDataFormatter(), 
			556 => new MissionCategoryKeyFormatter(), 
			557 => new MissionVisualDataFormatter(), 
			558 => new MissionVisualKeyFormatter(), 
			559 => new MonsterTierDataFormatter(), 
			560 => new MonthlyHeroDataFormatter(), 
			561 => new MultipleHeroSelectItemDataFormatter(), 
			562 => new NewbieRewardAttendanceDataFormatter(), 
			563 => new NewbieRewardBannerUrlDataFormatter(), 
			564 => new NewbieRewardBannerUrlKeyFormatter(), 
			565 => new NewbieRewardProductDataFormatter(), 
			566 => new NewbieRewardProductMissionDataFormatter(), 
			567 => new NewbieRewardTapOpenDataFormatter(), 
			568 => new NewEquipGachaDataFormatter(), 
			569 => new NewPayShopBannerDataFormatter(), 
			570 => new NewPayShopGroupDataFormatter(), 
			571 => new NewPayShopOpenConditionDataFormatter(), 
			572 => new NewPayShopProductDataFormatter(), 
			573 => new NewPayShopProductDataKey2Formatter(), 
			574 => new NewPayShopTagDataFormatter(), 
			575 => new Sample_JitKey_2Formatter(), 
			576 => new Sample_JitKey_3Formatter(), 
			577 => new Sample_JitKey_4Formatter(), 
			578 => new Sample_JitKey_5Formatter(), 
			579 => new NPCDataFormatter(), 
			580 => new NPCFriendlyActionDataFormatter(), 
			581 => new NPCFriendlyActionKey2Formatter(), 
			582 => new NPCFriendlyPointDataFormatter(), 
			583 => new NPCFriendlyPointDataKeyFormatter(), 
			584 => new NPCFriendlyPointItemDataFormatter(), 
			585 => new EventDataFormatter(), 
			586 => new OperationData_LoadDataFormatter(), 
			587 => new OptionSellDataFormatter(), 
			588 => new OptionSellKeyFormatter(), 
			589 => new OrdealArenaBuffDataFormatter(), 
			590 => new OrdealArenaEventDataFormatter(), 
			591 => new OrdealArenaNodeDataFormatter(), 
			592 => new OrdealArenaRewardAreaDataFormatter(), 
			593 => new OrdealArenaRewardAreaDataKeyFormatter(), 
			594 => new OrdealArenaRewardRatingDataFormatter(), 
			595 => new OrdealArenaTierDataFormatter(), 
			596 => new PackageItemDataFormatter(), 
			597 => new PartyDungeonDataFormatter(), 
			598 => new PatchDownloadSpecDataFormatter(), 
			599 => new PayShopDiscountDataFormatter(), 
			600 => new PayShopDiscountGroupDataFormatter(), 
			601 => new PayShopDiscountGroupKeyFormatter(), 
			602 => new PcAvatarDataFormatter(), 
			603 => new PcAvatarDataKeyFormatter(), 
			604 => new PetAdventureDataFormatter(), 
			605 => new PetAdventureRewardDataFormatter(), 
			606 => new PetAwakenDataFormatter(), 
			607 => new PetAwakenDataKeyFormatter(), 
			608 => new PetBonusDataFormatter(), 
			609 => new PetCommonOptionOrderDataFormatter(), 
			610 => new PetEggDataFormatter(), 
			611 => new PetIncubatorDataFormatter(), 
			612 => new PetIncubatorSlotDataFormatter(), 
			613 => new PetOptionDataFormatter(), 
			614 => new PetSelectItemDataFormatter(), 
			615 => new PetTierInteractiveDataFormatter(), 
			616 => new PetUpgradeTierDataFormatter(), 
			617 => new PhaseStepRewardDataFormatter(), 
			618 => new PhaseStepRewardDataKeyFormatter(), 
			619 => new PlayerArchiveDataFormatter(), 
			620 => new PlayerArchiveDataKeyFormatter(), 
			621 => new PlayGuideDataFormatter(), 
			622 => new PlayRecordDataFormatter(), 
			623 => new PortalDataFormatter(), 
			624 => new PortalPropDataFormatter(), 
			625 => new PortalRenewalDataFormatter(), 
			626 => new PortalRenewalDataKeyFormatter(), 
			627 => new PotionItemDataFormatter(), 
			628 => new ProbabilityBaseDataFormatter(), 
			629 => new PunishmentGroupDataFormatter(), 
			630 => new PunishmentGroupDataKeyFormatter(), 
			631 => new PunishmentRaidDataFormatter(), 
			632 => new PunishmentRaidRewardDataFormatter(), 
			633 => new PunishmentRaidRewardDataContainer_RewardAndIndexKeyFormatter(), 
			634 => new PunishmentRaidTriggerDataFormatter(), 
			635 => new PunishmentRuneDataFormatter(), 
			636 => new PunishmentRuneDataContainer_PunishmentRuneKeyFormatter(), 
			637 => new PunishmentRuneItemBreakDataFormatter(), 
			638 => new PunishmentRuneItemCountDataFormatter(), 
			639 => new PunishmentRuneOptionDataFormatter(), 
			640 => new PurchaseDungeonDataFormatter(), 
			641 => new PurchaseMarketingDataFormatter(), 
			642 => new PurchaseMarketingPopupDataFormatter(), 
			643 => new PushRewardDataFormatter(), 
			644 => new RaidDataFormatter(), 
			645 => new RaidDataKey2Formatter(), 
			646 => new RaidMultiInfoDataFormatter(), 
			647 => new RelatedStatDataFormatter(), 
			648 => new RewardDataFormatter(), 
			649 => new RewardData_SubDataFormatter(), 
			650 => new RewardItemIconOrderDataFormatter(), 
			651 => new RewardItemIconTypeDataKeyFormatter(), 
			652 => new RouletteDataFormatter(), 
			653 => new RouletteRewardDataFormatter(), 
			654 => new RuneItemBreakDataFormatter(), 
			655 => new RuneItemDataFormatter(), 
			656 => new SelectItemDataFormatter(), 
			657 => new SelectRewardDataFormatter(), 
			658 => new ServerGroupNameDataFormatter(), 
			659 => new ShakmehBossDataFormatter(), 
			660 => new ShakmehDungeonDataFormatter(), 
			661 => new ShakmehDungeonKeyFormatter(), 
			662 => new ShardFlaskItemDataFormatter(), 
			663 => new ShardItemDataFormatter(), 
			664 => new ShopAndIndexKeyFormatter(), 
			665 => new ShopDataFormatter(), 
			666 => new ShopItemDataFormatter(), 
			667 => new ShopItemFilterDataFormatter(), 
			668 => new ShopItemGroupDataFormatter(), 
			669 => new ShopItemGroupKeyFormatter(), 
			670 => new ShopItemSortDataFormatter(), 
			671 => new ShopItemSortGroupDataFormatter(), 
			672 => new SimilarAlphabetDataFormatter(), 
			673 => new SkillDataFormatter(), 
			674 => new SkillLevelFactorDataFormatter(), 
			675 => new SkillPriceDataFormatter(), 
			676 => new SkillPriceKeyFormatter(), 
			677 => new SkillProjectileDataFormatter(), 
			678 => new SkillProjectileGroupDataFormatter(), 
			679 => new SoulBreakDataFormatter(), 
			680 => new SoulBreakData_ItemCodeMinMaxDataFormatter(), 
			681 => new SoulBreakKeyFormatter(), 
			682 => new SoulStoneRestoreDataFormatter(), 
			683 => new SoulWeaponAbilityRewardDataFormatter(), 
			684 => new SoulWeaponAbilityRewardKeyFormatter(), 
			685 => new SoulWeaponAbilityTicketDataFormatter(), 
			686 => new SoulWeaponEtherDataFormatter(), 
			687 => new SoulWeaponEtherRateDataFormatter(), 
			688 => new SoulWeaponEtherRateKeyFormatter(), 
			689 => new SoulWeaponNodeTreeDataFormatter(), 
			690 => new SoulWeaponOptionDataFormatter(), 
			691 => new SoulWeaponReinforceDataFormatter(), 
			692 => new SoulWeaponSkillDataFormatter(), 
			693 => new SoulWeaponUpgradeDataFormatter(), 
			694 => new SoulWeaponUpgradePriceDataFormatter(), 
			695 => new SoulWeaponUpgradePriceKeyFormatter(), 
			696 => new StaminaConsumePointDataFormatter(), 
			697 => new StaminaDataFormatter(), 
			698 => new StatAttenuationDataFormatter(), 
			699 => new StatAttributeDataFormatter(), 
			700 => new StatDataFormatter(), 
			701 => new StateActionDataFormatter(), 
			702 => new StateActionDataContainer_ConditionKeyFormatter(), 
			703 => new StateDataFormatter(), 
			704 => new StateToolTipDataFormatter(), 
			705 => new StepUpGachaRewardDataFormatter(), 
			706 => new StepUpGachaRewardDataContainer_GachaRoundKeyFormatter(), 
			707 => new StepupGachaRewardDataKeyFormatter(), 
			708 => new StorageSlotExtendDataFormatter(), 
			709 => new StoreCurrencyUnitDataFormatter(), 
			710 => new SubQuestDataFormatter(), 
			711 => new SubQuestDataKeyFormatter(), 
			712 => new SubStoryDataFormatter(), 
			713 => new SubStoryDataContainer_ChapterInfoFormatter(), 
			714 => new SubStoryEventDataFormatter(), 
			715 => new SubStoryEventKeyFormatter(), 
			716 => new TeamLevelBonusBuffOptionDataFormatter(), 
			717 => new TeamLevelBuffOptionDataFormatter(), 
			718 => new TeamLevelCommonOptionOrderDataFormatter(), 
			719 => new TeamLevelDataFormatter(), 
			720 => new ThreatDataFormatter(), 
			721 => new ThreatDataContainer_ThreatKeyFormatter(), 
			722 => new TowerDataFormatter(), 
			723 => new TowerFloorDataFormatter(), 
			724 => new TowerFloorKeyFormatter(), 
			725 => new TreasureHouseDungeonDataFormatter(), 
			726 => new TreasureHouseDungeonKeyFormatter(), 
			727 => new TreasureHouseInfoDataFormatter(), 
			728 => new TutorialDataFormatter(), 
			729 => new TutorialKeyFormatter(), 
			730 => new UnderPrisonDataFormatter(), 
			731 => new UnderPrisonDungeonDataFormatter(), 
			732 => new UnderPrisonDungeonKeyFormatter(), 
			733 => new UpgradeBookItemDataFormatter(), 
			734 => new UpgradeBookItemDataContainer_TagGradeKeyFormatter(), 
			735 => new ValanceAwakenDataFormatter(), 
			736 => new ValanceAwakenDataKeyFormatter(), 
			737 => new ValanceCraftDataFormatter(), 
			738 => new ValanceCraftDataKey2Formatter(), 
			739 => new ValanceCreatureFilterDataFormatter(), 
			740 => new ValanceEnchantDataFormatter(), 
			741 => new ValanceEnchantDataKeyFormatter(), 
			742 => new ValanceEnchantOptionCountDataFormatter(), 
			743 => new ValanceEnchantOptionCountDataKeyFormatter(), 
			744 => new ValanceEquipFilterDataFormatter(), 
			745 => new ValanceEquipFilterDataContainer_SortingOrderKey2Formatter(), 
			746 => new ValanceFilterGroupDataFormatter(), 
			747 => new ValanceIdentifiedDataFormatter(), 
			748 => new VideoDataFormatter(), 
			749 => new WantedPathDataFormatter(), 
			750 => new WantedQuestDataFormatter(), 
			751 => new WeaponCostumeDataFormatter(), 
			752 => new WeaponUniqueSelectItemDataFormatter(), 
			753 => new WorldBossDailyAchievemenDataKeyFormatter(), 
			754 => new WorldBossDailyAchievementDataFormatter(), 
			755 => new WorldBossDataFormatter(), 
			756 => new WorldBossRewardDataFormatter(), 
			757 => new WorldBossScoreRewardDataFormatter(), 
			758 => new WorldBossScoreRewardDataKeyFormatter(), 
			759 => new WorldMapBuildDataFormatter(), 
			760 => new WorldMapPropDataFormatter(), 
			761 => new XCodeLocalizationDataFormatter(), 
			_ => null, 
		};
	}
}
