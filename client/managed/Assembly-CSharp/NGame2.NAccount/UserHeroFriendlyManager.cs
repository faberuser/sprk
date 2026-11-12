using System.Collections.Generic;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NAccount;

public class UserHeroFriendlyManager : NShared.UserHeroFriendlyManager
{
	public void Set(PlayerHeroFriendlyInfo info)
	{
		RemoveAll();
		info.ConverSelectedHeroindice = HeroindicesParser(info.SelectedHeroIndice);
		Add(info);
	}

	public void Apply(PlayerHeroFriendlyInfo info)
	{
		if (info != null)
		{
			Set(info);
		}
	}

	public void Apply(PlayerHeroFriendlyInfo[] infos)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(infos); i++)
		{
			Set(infos[i]);
		}
	}

	private int[] HeroindicesParser(string heroindices)
	{
		if (string.IsNullOrEmpty(heroindices))
		{
			return new int[1];
		}
		string[] array = heroindices.Split(',');
		List<int> list = new List<int>();
		for (int i = 0; i < array.Length; i++)
		{
			int result = 0;
			int.TryParse(array[i], out result);
			if (result != 0)
			{
				list.Add(result);
			}
		}
		return list.ToArray();
	}

	public int GetUserSelectHeroIndex()
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(GetValues()))
		{
			return 0;
		}
		return GetValues()[0].HeroIndex;
	}

	private int GetSelectHeroFriendlyPoint()
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(GetValues()))
		{
			return 0;
		}
		return GetValues()[0].FriendlyPoint;
	}

	public PlayerHeroFriendlyInfo GetFriendlyInfo()
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(GetValues()))
		{
			return new PlayerHeroFriendlyInfo
			{
				ConverSelectedHeroindice = new int[0]
			};
		}
		return GetValues()[0];
	}

	public bool IsAreadySelectHero()
	{
		return GetUserSelectHeroIndex() != 0;
	}

	public bool IsContainsIndicesHeroIndex(int heroIndex)
	{
		int[] converSelectedHeroindice = GetFriendlyInfo().ConverSelectedHeroindice;
		for (int i = 0; i < converSelectedHeroindice.Length; i++)
		{
			if (converSelectedHeroindice[i] == heroIndex)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsSelectHero(int heroIndex)
	{
		if (heroIndex == 0)
		{
			return false;
		}
		int userSelectHeroIndex = GetUserSelectHeroIndex();
		if (userSelectHeroIndex == 0)
		{
			return false;
		}
		return userSelectHeroIndex == heroIndex;
	}

	public int GetFriendlyPoint(int heroIndex)
	{
		if (heroIndex == 0)
		{
			return 0;
		}
		if (!IsSelectHero(heroIndex))
		{
			return 0;
		}
		return GetSelectHeroFriendlyPoint();
	}

	private CachedHeroInfo GetHaveHeroInfo(int heroIndex)
	{
		return GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfo(heroIndex);
	}

	public bool IsHaveHero(int heroIndex)
	{
		if (heroIndex == 0)
		{
			return false;
		}
		return GetHaveHeroInfo(heroIndex) != null;
	}

	public bool IsHaveHero(int heroIndex, out CachedHeroInfo cachedHeroInfo)
	{
		cachedHeroInfo = null;
		if (heroIndex == 0)
		{
			return false;
		}
		cachedHeroInfo = GetHaveHeroInfo(heroIndex);
		return cachedHeroInfo != null;
	}

	public int MaxClosenessHeroCount()
	{
		int[] converSelectedHeroindice = GetFriendlyInfo().ConverSelectedHeroindice;
		if (__Extension_Method_ICollection__.IsNullOrEmpty(converSelectedHeroindice))
		{
			return 0;
		}
		int num = 0;
		for (int i = 0; i < converSelectedHeroindice.Length; i++)
		{
			HeroFriendlyData data = GameRoot.Instance.HeroFriendlyDataContainer.GetData(converSelectedHeroindice[i]);
			if (data != null)
			{
				int needFriendlyPoint = data.NeedFriendlyPoint;
				int closenessPoint = GetClosenessPoint(converSelectedHeroindice[i]);
				if (needFriendlyPoint <= closenessPoint)
				{
					num++;
				}
			}
		}
		return num;
	}

	public int[] GetSelectedHeroIndices()
	{
		return SortHeroIndex(GetFriendlyInfo().ConverSelectedHeroindice);
	}

	private int[] SortHeroIndex(int[] heroinfos)
	{
		List<int> list = new List<int>(heroinfos);
		list.Sort(delegate(int lhf, int rhf)
		{
			if (lhf == rhf)
			{
				return 0;
			}
			bool flag = IsHaveHero(lhf);
			bool flag2 = IsHaveHero(rhf);
			if (IsSelectHero(lhf))
			{
				return -1;
			}
			if (IsSelectHero(rhf))
			{
				return 1;
			}
			if (flag && !flag2)
			{
				return 1;
			}
			if (flag2 && !flag)
			{
				return -1;
			}
			return (lhf >= rhf) ? 1 : (-1);
		});
		return list.ToArray();
	}

	public int GetClosenessPoint(int heroIndex)
	{
		return GetHaveHeroInfo(heroIndex)?.ClosenessPoint ?? 0;
	}

	public string GetClosenessGreetingTime(int heroIndex)
	{
		return GetHaveHeroInfo(heroIndex)?.LastGreetingTime;
	}

	public string GetClosenessConversationTime(int heroIndex)
	{
		return GetHaveHeroInfo(heroIndex)?.LastConversationTime;
	}

	public string GetClosenessGiftTime(int heroIndex)
	{
		return GetHaveHeroInfo(heroIndex)?.LastGiftTime;
	}
}
