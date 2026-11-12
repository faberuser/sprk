using NShared;

public sealed class SprkLobbyAppearance
{
	private readonly CachedHeroInfo saved = new CachedHeroInfo();

	private object renderSnapshot;

	private bool initialized;

	private string accessoryFingerprint;

	private static bool SamePosition(PositionInfo a, PositionInfo b)
	{
		a = a ?? PositionInfo.Create();
		b = b ?? PositionInfo.Create();
		if (a.PositionX == b.PositionX && a.PositionY == b.PositionY && a.PositionZ == b.PositionZ && a.RotationX == b.RotationX && a.RotationY == b.RotationY && a.RotationZ == b.RotationZ)
		{
			return a.Scale == b.Scale;
		}
		return false;
	}

	public int Observe(CachedHeroInfo hero)
	{
		if (hero == null)
		{
			return 0;
		}
		string text = SprkLobbyAvatar.Multi("Fingerprint", hero) as string;
		object resolvedSnapshot = SprkLobbyAvatar.GetResolvedSnapshot(hero);
		int num = ((initialized && (renderSnapshot != resolvedSnapshot || saved.Star != hero.Star || saved.HeroIndex != hero.HeroIndex || saved.CostumeIndex != hero.CostumeIndex || saved.HairCostumeIndex != hero.HairCostumeIndex || saved.WeaponCostumeIndex != hero.WeaponCostumeIndex || saved.HideUniqueWeapon != hero.HideUniqueWeapon)) ? 1 : 0);
		if (initialized && text != accessoryFingerprint)
		{
			num |= 2;
		}
		accessoryFingerprint = text;
		foreach (AccessoryPartType item in AccessoryPartTypeHelper.PartTypes())
		{
			PositionInfo accessoryPositionInfo = hero.GetAccessoryPositionInfo(item);
			if (initialized && (saved.GetAccessoryIndex(item) != hero.GetAccessoryIndex(item) || !SamePosition(saved.GetAccessoryPositionInfo(item), accessoryPositionInfo)))
			{
				num |= 2;
			}
			saved.SetAccessoryCostume(item, hero.GetAccessoryIndex(item), accessoryPositionInfo?.Clone());
		}
		saved.HeroIndex = hero.HeroIndex;
		saved.CostumeIndex = hero.CostumeIndex;
		saved.HairCostumeIndex = hero.HairCostumeIndex;
		saved.WeaponCostumeIndex = hero.WeaponCostumeIndex;
		saved.HideUniqueWeapon = hero.HideUniqueWeapon;
		saved.Star = hero.Star;
		renderSnapshot = resolvedSnapshot;
		initialized = true;
		return num;
	}
}
