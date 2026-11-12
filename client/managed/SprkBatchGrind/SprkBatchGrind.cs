using System;
using System.Collections.Generic;
using System.Text;
using NShared;

public static class SprkBatchGrind
{
	public static string Summary(string original, IEnumerable<EquipItemInfo> items)
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		SortedDictionary<string, long[]> sortedDictionary = new SortedDictionary<string, long[]>();
		bool missing = false;
		foreach (EquipItemInfo item in items)
		{
			if (item == null)
			{
				continue;
			}
			num++;
			if (item.IsUniqueWeapon || item.IsUniqueTreasure)
			{
				if (item.IsUniqueWeapon)
				{
					num2++;
				}
				else
				{
					num3++;
				}
				ItemData data = GameRoot.Instance.ItemDataContainer.GetData(item.ItemIndex);
				EquipItemData subData = data.GetSubData<EquipItemData>();
				EquipItemBreakData data2 = GameRoot.Instance.EquipItemBreakDataContainer.GetData(subData.Tier, subData.DetailIndex, data.Code, item.Star, subData.SetIndex);
				if (data2 == null)
				{
					missing = true;
					continue;
				}
				Add(sortedDictionary, data2.ItemCode1, data2.MinItemCount1, data2.MaxItemCount1, data2.ItemCount1_1, data2.ItemRate1_1, data2.ItemCount1_2, data2.ItemRate1_2);
				Add(sortedDictionary, data2.ItemCode2, data2.MinItemCount2, data2.MaxItemCount2, data2.ItemCount2_1, data2.ItemRate2_1, data2.ItemCount2_2, data2.ItemRate2_2);
				Add(sortedDictionary, data2.ItemCode3, data2.MinItemCount3, data2.MaxItemCount3, data2.ItemCount3_1, data2.ItemRate3_1, data2.ItemCount3_2, data2.ItemRate3_2);
				Add(sortedDictionary, data2.ItemCode4, data2.MinItemCount4, data2.MaxItemCount4, data2.ItemCount4_1, data2.ItemRate4_1, data2.ItemCount4_2, data2.ItemRate4_2);
				Add(sortedDictionary, data2.ItemCode5, data2.MinItemCount5, data2.MaxItemCount5, data2.ItemCount5_1, data2.ItemRate5_1, data2.ItemCount5_2, data2.ItemRate5_2);
			}
		}
		if (num2 + num3 == 0)
		{
			return original;
		}
		SortedDictionary<string, long[]> sortedDictionary2 = new SortedDictionary<string, long[]>();
		foreach (KeyValuePair<string, long[]> item2 in sortedDictionary)
		{
			ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(item2.Key);
			string key = ((dataByCode == null) ? item2.Key : dataByCode.Name);
			if (!sortedDictionary2.TryGetValue(key, out var value))
			{
				value = (sortedDictionary2[key] = new long[2]);
			}
			value[0] += item2.Value[0];
			value[1] += item2.Value[1];
		}
		return Format(original, num, num2, num3, sortedDictionary2, missing);
	}

	public static void Add(SortedDictionary<string, long[]> totals, string code, int min, int max, int bonus1, int rate1, int bonus2, int rate2)
	{
		if (string.IsNullOrEmpty(code))
		{
			return;
		}
		long num = ((max > 0) ? min : 0);
		long num2 = Math.Max(0, max);
		if (rate1 >= 1000)
		{
			num += bonus1;
		}
		if (rate2 >= 1000)
		{
			num += bonus2;
		}
		if (rate1 > 0)
		{
			num2 += bonus1;
		}
		if (rate2 > 0)
		{
			num2 += bonus2;
		}
		if (num2 > 0)
		{
			if (!totals.TryGetValue(code, out var value))
			{
				value = (totals[code] = new long[2]);
			}
			value[0] += num;
			value[1] += num2;
		}
	}

	public static string Format(string original, int total, int weapons, int treasures, SortedDictionary<string, long[]> rewards, bool missing)
	{
		if (weapons + treasures == 0)
		{
			return original;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("Grind ").Append(total).Append(" selected items?\n");
		if (weapons > 0)
		{
			stringBuilder.Append("Unique Weapons: ").Append(weapons).Append('\n');
		}
		if (treasures > 0)
		{
			stringBuilder.Append("Unique Treasures: ").Append(treasures).Append('\n');
		}
		if (total > weapons + treasures)
		{
			stringBuilder.Append("Other gear: ").Append(total - weapons - treasures).Append('\n');
		}
		stringBuilder.Append("\nUnique item rewards:\n");
		foreach (KeyValuePair<string, long[]> reward in rewards)
		{
			stringBuilder.Append(reward.Key).Append(": ").Append(reward.Value[0].ToString("N0"));
			if (reward.Value[0] != reward.Value[1])
			{
				stringBuilder.Append(" - ").Append(reward.Value[1].ToString("N0"));
			}
			stringBuilder.Append('\n');
		}
		if (missing)
		{
			stringBuilder.Append("Some rewards cannot be previewed.\n");
		}
		stringBuilder.Append("\nAll selected items will be permanently destroyed.\nThis cannot be undone.");
		return stringBuilder.ToString();
	}
}
