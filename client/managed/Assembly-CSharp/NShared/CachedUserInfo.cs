using System;

namespace NShared;

[JsonSerializable(Javascript = true)]
public class CachedUserInfo
{
	public DateTime RecvPingTime;

	public long AccountId { get; set; }

	public int TeamLevel { get; set; }

	public string Nick { get; set; }

	public int VIPLevel { get; set; }

	public int AvatarHeroIndex { get; set; }

	public string LastLoginTime { get; set; }

	public string LastLogoutTime { get; set; }

	public int TierIndex { get; set; }

	public int MatchScore { get; set; }

	public int SeasonWin { get; set; }

	public int SeasonLose { get; set; }

	public int SuccessiveWin { get; set; }

	public bool IsOnline { get; set; }

	public int LastChapterIndex { get; set; }

	public int LastDungeonIndex { get; set; }

	public int HeroIndex { get; set; }

	public int CostumeIndex { get; set; }

	public byte HideUniqueWeapon { get; set; }

	public int WeaponCostumeIndex { get; set; }

	public int HairCostumeIndex { get; set; }

	public int AccessoryCostumeIndex1 { get; set; }

	public string PositionInfo1 { get; set; }

	public int AccessoryCostumeIndex2 { get; set; }

	public string PositionInfo2 { get; set; }

	public int AccessoryCostumeIndex3 { get; set; }

	public string PositionInfo3 { get; set; }

	public int AccessoryCostumeIndex4 { get; set; }

	public string PositionInfo4 { get; set; }

	public int AccessoryCostumeIndex5 { get; set; }

	public string PositionInfo5 { get; set; }

	public int AccessoryCostumeIndex6 { get; set; }

	public string PositionInfo6 { get; set; }
}
