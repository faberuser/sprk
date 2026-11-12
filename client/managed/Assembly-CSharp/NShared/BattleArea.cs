using System.Collections.Generic;
using NShared.NArea;
using NShared.NUtil;
using NVespa;

namespace NShared;

public class BattleArea
{
	private ExtendArea _area;

	private Dictionary<int, ExtendArea> _dic;

	public int MinMz => _area.MinMz;

	public int MaxMz => _area.MaxMz;

	public int MinMx => _area.MinMx;

	public int MaxMx => _area.MaxMx;

	public int MzCenter => _area.MzCenter;

	public int MxCenter => _area.MxCenter;

	public BattleArea(BattleInstance battleInstance)
	{
		_dic = new Dictionary<int, ExtendArea>();
		_area = new ExtendArea();
		_dic.Add(0, _area);
	}

	public void InitByGroupIndex(int minMz, int maxMz, int minMx, int maxMx, int groupIndex)
	{
		if (!_dic.ContainsKey(groupIndex))
		{
			_dic.Add(groupIndex, new ExtendArea());
		}
		_dic[groupIndex].Init(minMx, maxMx, minMz, maxMz);
	}

	public void InitByGroupIndex(int minMz, int maxMz, int minMx, int maxMx, int angle, int groupIndex)
	{
		if (!_dic.ContainsKey(groupIndex))
		{
			_dic.Add(groupIndex, new ExtendArea());
		}
		_dic[groupIndex].Init(minMx, maxMx, minMz, maxMz, angle);
	}

	public void InitMxByGroupIndex(int minMx, int maxMx, int groupIndex)
	{
		if (!_dic.ContainsKey(groupIndex))
		{
			VespaDebug.LogError("[BattleArea] Not Exist Area GroupIndex : " + groupIndex);
		}
		else
		{
			_dic[groupIndex].InitMx(minMx, maxMx);
		}
	}

	public void ValidateByGroupIndex(ref Vector3 position, int groupIndex)
	{
		if (!_dic.ContainsKey(groupIndex))
		{
			VespaDebug.LogError("[BattleArea] Not Exist Area GroupIndex : " + groupIndex);
		}
		else
		{
			_dic[groupIndex].Validate(ref position);
		}
	}

	public ExtendArea GetBattleAreaByGroupIndex(int groupIndex)
	{
		ExtendArea value = null;
		_dic.TryGetValue(groupIndex, out value);
		return value;
	}

	public Dictionary<int, ExtendArea> GetBattleAreaList()
	{
		return _dic;
	}

	public int GetMinMx(int groupIndex)
	{
		if (!_dic.ContainsKey(groupIndex))
		{
			VespaDebug.LogError("[BattleArea] Not Exist Area GroupIndex : " + groupIndex);
			return 0;
		}
		return _dic[groupIndex].MinMx;
	}

	public int GetMaxMx(int groupIndex)
	{
		if (!_dic.ContainsKey(groupIndex))
		{
			VespaDebug.LogError("[BattleArea] Not Exist Area GroupIndex : " + groupIndex);
			return 0;
		}
		return _dic[groupIndex].MaxMx;
	}

	public int GetMinMz(int groupIndex)
	{
		if (!_dic.ContainsKey(groupIndex))
		{
			VespaDebug.LogError("[BattleArea] Not Exist Area GroupIndex : " + groupIndex);
			return 0;
		}
		return _dic[groupIndex].MinMz;
	}

	public int GetMaxMz(int groupIndex)
	{
		if (!_dic.ContainsKey(groupIndex))
		{
			VespaDebug.LogError("[BattleArea] Not Exist Area GroupIndex : " + groupIndex);
			return 0;
		}
		return _dic[groupIndex].MaxMz;
	}

	public int GetMxCenter(int groupIndex)
	{
		if (!_dic.ContainsKey(groupIndex))
		{
			VespaDebug.LogError("[BattleArea] Not Exist Area GroupIndex : " + groupIndex);
			return 0;
		}
		return _dic[groupIndex].MxCenter;
	}

	public int GetMzCenter(int groupIndex)
	{
		if (!_dic.ContainsKey(groupIndex))
		{
			VespaDebug.LogError("[BattleArea] Not Exist Area GroupIndex : " + groupIndex);
			return 0;
		}
		return _dic[groupIndex].MxCenter;
	}
}
