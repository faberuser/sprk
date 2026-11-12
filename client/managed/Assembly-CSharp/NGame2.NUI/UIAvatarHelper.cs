using NGame2.N3D;
using NShared;
using UnityEngine;

namespace NGame2.NUI;

public static class UIAvatarHelper
{
	public struct AvatarInfo
	{
		public const int CostumeMagicNumber = 10000;

		public int AvatarIndex;

		public int HeroIndex;

		public int Star;

		public int CostumeIndex;

		public AvatarInfo(int avatarIndex)
		{
			AvatarIndex = avatarIndex;
			if (AvatarIndex < 10000)
			{
				HeroIndex = avatarIndex / 10;
				Star = avatarIndex % 10;
				CostumeIndex = 0;
				CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(HeroIndex);
				if (data != null)
				{
					Star = Mathf.Clamp(Star, data.StartHeroStar, GameRoot.Instance.ConstantDataContainer.GetValueInt("HeroMaxStar"));
					return;
				}
				HeroIndex = 1;
				Star = 1;
				return;
			}
			CostumeIndex = AvatarIndex - 10000;
			CostumeData data2 = GameRoot.Instance.CostumeDataContainer.GetData(CostumeIndex);
			if (data2 != null)
			{
				HeroIndex = data2.HeroIndex;
				Star = data2.HeroStar;
				CreatureData data3 = GameRoot.Instance.CreatureDataContainer.GetData(HeroIndex);
				if (data3 != null)
				{
					Star = Mathf.Clamp(Star, data3.StartHeroStar, GameRoot.Instance.ConstantDataContainer.GetValueInt("HeroMaxStar"));
				}
			}
			else
			{
				HeroIndex = 1;
				Star = 1;
				CostumeIndex = 0;
			}
		}

		public AvatarInfo(CostumeData costumeData)
		{
			HeroIndex = costumeData.HeroIndex;
			Star = costumeData.HeroStar;
			CostumeIndex = costumeData.CostumeIndex;
			AvatarIndex = 10000 + costumeData.CostumeIndex;
		}

		public CreatureData GetCreatureData()
		{
			return GameRoot.Instance.CreatureDataContainer.GetData(HeroIndex);
		}
	}

	public static string GetIconPath(int avatarIndex)
	{
		AvatarInfo avatarInfo = new AvatarInfo(avatarIndex);
		if (avatarInfo.CostumeIndex != 0)
		{
			CostumeData data = GameRoot.Instance.CostumeDataContainer.GetData(avatarInfo.CostumeIndex);
			if (data != null)
			{
				return data.IconPortraitPath;
			}
		}
		return UIHelper.GetHeroIconPath(avatarInfo.HeroIndex, avatarInfo.Star);
	}

	public static string GetPortraitPath(int avatarIndex)
	{
		AvatarInfo avatarInfo = new AvatarInfo(avatarIndex);
		if (avatarInfo.CostumeIndex != 0)
		{
			CostumeData data = GameRoot.Instance.CostumeDataContainer.GetData(avatarInfo.CostumeIndex);
			if (data != null)
			{
				return data.PortraitPath;
			}
		}
		return UIHelper.GetHeroPortraitPath(avatarInfo.GetCreatureData().CodeName, avatarInfo.Star);
	}

	public static string GetBattlePortraitPath(int avatarIndex)
	{
		AvatarInfo avatarInfo = new AvatarInfo(avatarIndex);
		if (avatarInfo.CostumeIndex != 0)
		{
			CostumeData data = GameRoot.Instance.CostumeDataContainer.GetData(avatarInfo.CostumeIndex);
			if (data != null)
			{
				return data.BattlePortraitPath;
			}
		}
		CreatureData creatureData = avatarInfo.GetCreatureData();
		if (creatureData == null)
		{
			return string.Empty;
		}
		return UIHelper.GetBattleHeroPortraitPath(creatureData.CodeName, avatarInfo.Star, 0);
	}

	public static RenderFacadeBuilder CreateRenderFacadeBuilder(int avatarIndex)
	{
		AvatarInfo avatarInfo = new AvatarInfo(avatarIndex);
		RenderFacadeBuilder renderFacadeBuilder = new RenderFacadeBuilder(null, TeamType.Left, avatarInfo.GetCreatureData().CodeName, avatarInfo.Star);
		renderFacadeBuilder.SetCostume(avatarInfo.CostumeIndex);
		return renderFacadeBuilder;
	}
}
