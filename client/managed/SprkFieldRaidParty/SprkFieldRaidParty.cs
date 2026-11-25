using NGame2.NUI.NWindow;

/// RaidSinglePartySetting only has PartySlotSpacings (180), tuned for the
/// half-width Main/Sub panels. Its main-only layout (Lava Raid, any raid with
/// no sub party) uses larger slots and cards, so 180 makes them overlap.
/// PunishmentRaidPartySetting ships a separate OnlyMainPartySlotSpacings for
/// the same layout; use its value (210) here.
public static class SprkFieldRaidParty
{
	public const int OnlyMainSpacing = 210;

	public static RaidPartySetting.PartySlotSpacing Spacing(BattlePartySetting.SquardUi.ESquardType type, RaidPartySetting.PartySlotSpacing native)
	{
		if (type != BattlePartySetting.SquardUi.ESquardType.OnlyMain || native == null)
		{
			return native;
		}
		// Grids lay out right to left: keep the native sign.
		return new RaidPartySetting.PartySlotSpacing
		{
			MinSlotCount = native.MinSlotCount,
			MaxSlotCount = native.MaxSlotCount,
			Spacing = (native.Spacing < 0) ? (-OnlyMainSpacing) : OnlyMainSpacing
		};
	}
}
