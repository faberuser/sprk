using System.Collections.Generic;
using System.Linq;
using NGame2.NAccount;
using NShared;
using NVespa.NSingleton;

public static class SprkShopTags
{
	private static readonly Dictionary<NewPayShopTagData, int[]> sourceIndices = new Dictionary<NewPayShopTagData, int[]>();

	public static bool Matches(int tag, string name, string category, int type)
	{
		name = (name ?? "").ToUpperInvariant();
		switch (tag)
		{
		case 1:
			if (name.Contains("HERO"))
			{
				return !name.Contains("HEROIC");
			}
			return false;
		case 2:
			return category == "Beginner";
		case 3:
			return category == "Return";
		case 4:
			return name.Contains("UNIQUE WEAPON");
		case 5:
			return name.Contains("UNIQUE TREASURE");
		case 6:
			return name.Contains("ARTIFACT");
		case 7:
		case 20:
			if (type != 2)
			{
				return name.Contains("RUBY");
			}
			return true;
		case 8:
			if (!name.Contains("SKILL BOOK"))
			{
				return name.Contains("SKILLBOOK");
			}
			return true;
		case 9:
			return name.Contains("AWAKEN");
		case 10:
			if (!name.Contains("EXP "))
			{
				return name.Contains("EXPERIENCE");
			}
			return true;
		case 11:
			if (!(category == "Booster"))
			{
				return name.Contains("BOOST");
			}
			return true;
		case 12:
			return name.Contains("FRAGMENT");
		case 13:
			if (type != 3)
			{
				return name.Contains("GOLD");
			}
			return true;
		case 14:
			if (!(category == "Stamina"))
			{
				return name.Contains("STAMINA");
			}
			return true;
		case 15:
			return name.Contains("ARMOR");
		case 16:
			return name.Contains("COSTUME");
		case 17:
			if (!name.Contains("SUMMON"))
			{
				return name.Contains("GACHA");
			}
			return true;
		case 18:
			return name.Contains("RUNE");
		case 19:
			return name.Contains("ATTENDANCE");
		case 21:
			return name.Contains("REFORGE");
		case 22:
			return name.Contains("NPC");
		case 24:
			if (!name.Contains("SOUL WEAPON"))
			{
				return name.Contains("SOULWEAPON");
			}
			return true;
		case 25:
			if (!name.Contains("SOULSTONE"))
			{
				return name.Contains("SOUL STONE");
			}
			return true;
		case 27:
			return name.Contains("ETHER");
		case 28:
			return name.Contains("TRANSCENDENCE POINT");
		case 30:
			return name.Contains("WEEKLY");
		case 31:
			if (!name.Contains("INVENTORY"))
			{
				return name.Contains("BAG EXPANSION");
			}
			return true;
		case 32:
			return name.Contains("ENCHANT");
		case 33:
			return name.Contains("ACCESSORY");
		case 34:
			if (!(category == "Technomagic"))
			{
				return name.Contains("TECHNOMAGIC");
			}
			return true;
		case 35:
			return name.Contains("CRYSTAL");
		case 36:
			return name.Contains("REINFORC");
		case 37:
			return category == "Event";
		case 38:
			return name.Contains("RESTORE");
		case 40:
			return name.Contains("CHANGE");
		case 41:
			return name.Contains("AWAKENING STONE");
		case 42:
			if (!name.Contains("HOT DEAL"))
			{
				return name.Contains("HOTDEAL");
			}
			return true;
		case 43:
			return name.Contains("SHARD");
		default:
			return false;
		}
	}

	public static void Refresh(NewPayShopTagDataContainer tags)
	{
		if (!GlobalAutoBehaviour<AccountManager>.isValidInstance)
		{
			return;
		}
		NGame2.NAccount.NewUserPayShopProductManager newPayShopProductManager = GlobalAutoBehaviour<AccountManager>.instance.NewPayShopProductManager;
		if (newPayShopProductManager == null)
		{
			return;
		}
		NewPayShopProductInfo[] values = newPayShopProductManager.GetValues();
		if (values == null || values.Length == 0)
		{
			return;
		}
		foreach (NewPayShopTagData tag in tags.GetAllData())
		{
			if (tag.IndexType == PayShopIndexType.PayShop)
			{
				if (!sourceIndices.TryGetValue(tag, out var original))
				{
					original = tag.Indices ?? new int[0];
					sourceIndices[tag] = original;
				}
				tag.Indices = (from p in values
					where original.Contains(p.Index) || Matches(tag.Index, p.ProductName, p.Category, (int)p.ProductType)
					orderby p.ShowIndex, p.Index
					select p.Index).ToArray();
			}
		}
	}
}
