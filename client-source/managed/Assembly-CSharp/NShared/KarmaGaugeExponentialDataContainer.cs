using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class KarmaGaugeExponentialDataContainer : JitDataContainerDefault<KarmaGaugeExponentialKey, KarmaGaugeExponentialData>
{
	public const string TABLENAME = "KarmaGaugeExponentialTable";

	private readonly Dictionary<int, SortedList<int, float>> _expononetialDic = new Dictionary<int, SortedList<int, float>>();

	public override TablePathDefine PathDefine => new TablePathDefine("KarmaGaugeExponentialTable");

	protected override void OnEndLoad_Final()
	{
		foreach (KarmaGaugeExponentialData allDatum in GetAllData())
		{
			if (!_expononetialDic.TryGetValue(allDatum.PunishmentGroupIndex, out var value))
			{
				value = new SortedList<int, float>();
				_expononetialDic.Add(allDatum.PunishmentGroupIndex, value);
			}
			value.Add(allDatum.Wave, allDatum.ExponentialValue);
		}
	}

	public float GetExponentValue(int punishGroupIndex, int wave)
	{
		if (!_expononetialDic.TryGetValue(punishGroupIndex, out var value))
		{
			return 0f;
		}
		if (value == null)
		{
			return 0f;
		}
		float result = 0f;
		foreach (KeyValuePair<int, float> item in value)
		{
			if (item.Key <= wave)
			{
				result = item.Value;
			}
		}
		return result;
	}
}
