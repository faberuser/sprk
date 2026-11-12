using System;

public sealed class SprkHeroExpLevelPlan
{
	private readonly long current;

	private readonly long max;

	private readonly long perPotion;

	private readonly int maxPotions;

	private readonly int fromLevel;

	private readonly int maxLevel;

	private readonly Func<int, long> levelTotal;

	public readonly int MinLevel;

	public readonly int TopLevel;

	public bool CanChoose => TopLevel > fromLevel;

	public SprkHeroExpLevelPlan(long current, long max, long perPotion, int maxPotions, int fromLevel, int maxLevel, Func<int, long> levelTotal)
	{
		this.current = current;
		this.max = max;
		this.perPotion = Math.Max(1L, perPotion);
		this.maxPotions = Math.Max(1, maxPotions);
		this.fromLevel = fromLevel;
		this.maxLevel = Math.Max(fromLevel, maxLevel);
		this.levelTotal = levelTotal;
		MinLevel = LevelAfter(Potions(Math.Min(this.maxLevel, fromLevel + 1)));
		TopLevel = LevelAfter(this.maxPotions);
	}

	public long TotalAfter(int potions)
	{
		return Math.Min(max, current + perPotion * potions);
	}

	public int LevelAfter(int potions)
	{
		long num = TotalAfter(potions);
		int i;
		for (i = fromLevel; i < maxLevel && levelTotal(i + 1) <= num; i++)
		{
		}
		return i;
	}

	public int Potions(int level)
	{
		long num = levelTotal(level) - current;
		long val = ((num <= 0) ? 1 : ((num + perPotion - 1) / perPotion));
		return (int)Math.Max(1L, Math.Min(maxPotions, val));
	}

	public int Step(int level, int delta)
	{
		int num = Math.Max(MinLevel, Math.Min(TopLevel, level + delta));
		int num2 = Potions(num);
		int num3 = LevelAfter(num2);
		if (delta < 0 && num3 > num)
		{
			num3 = ((num2 > 1) ? LevelAfter(num2 - 1) : MinLevel);
		}
		return Math.Max(MinLevel, Math.Min(TopLevel, num3));
	}
}
