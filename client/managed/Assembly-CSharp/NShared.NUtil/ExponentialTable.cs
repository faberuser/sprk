using System.Collections.Generic;

namespace NShared.NUtil;

public class ExponentialTable
{
	private readonly object _ratioListSync = new object();

	private readonly object _ratiosDictionarySync = new object();

	private readonly object _invRatiosDictionarySync = new object();

	private readonly long _unit;

	private readonly Dictionary<int, List<long>> _ratiosDictionary = new Dictionary<int, List<long>>();

	private readonly Dictionary<int, List<long>> _invRatiosDictionary = new Dictionary<int, List<long>>();

	public ExponentialTable(long unit)
	{
		_unit = unit;
	}

	public long GetAccRatio(int ratio, int exponent)
	{
		switch (exponent)
		{
		case 0:
			return _unit;
		case 1:
			return ratio;
		default:
		{
			List<long> value = null;
			bool flag = false;
			if (exponent > 0)
			{
				_ratiosDictionary.TryGetValue(ratio, out value);
				if (value == null)
				{
					lock (_ratiosDictionarySync)
					{
						_ratiosDictionary.TryGetValue(ratio, out value);
						if (value == null)
						{
							value = new List<long>();
							value.Add(_unit);
							_ratiosDictionary[ratio] = value;
						}
					}
				}
			}
			else
			{
				exponent = -exponent;
				flag = true;
				_invRatiosDictionary.TryGetValue(ratio, out value);
				if (value == null)
				{
					lock (_invRatiosDictionarySync)
					{
						_invRatiosDictionary.TryGetValue(ratio, out value);
						if (value == null)
						{
							value = new List<long>();
							value.Add(_unit);
							_invRatiosDictionary[ratio] = value;
						}
					}
				}
			}
			if (exponent < value.Count)
			{
				return value[exponent];
			}
			lock (_ratioListSync)
			{
				long num = value[value.Count - 1];
				while (value.Count <= exponent)
				{
					num = ((!flag) ? (num * ratio / _unit) : (num * _unit / ratio));
					value.Add(num);
				}
			}
			return value[exponent];
		}
		}
	}
}
