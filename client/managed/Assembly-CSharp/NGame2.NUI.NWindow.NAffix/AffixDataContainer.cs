using System.Collections.Generic;
using NShared;
using NVespa;

namespace NGame2.NUI.NWindow.NAffix;

public class AffixDataContainer
{
	public IList<AffixData> _listAll = new List<AffixData>();

	public List<AffixData> _listSelect = new List<AffixData>();

	private int _groupIndex;

	private int _clearPoint;

	private int _advantagePoint;

	private int _penaltyPoint;

	private int _advantageCount;

	private int _penaltyCount;

	private int _sumPoint => _advantagePoint + _penaltyPoint;

	public AffixDataContainer(int affixGroupIndex, int clearPoint)
	{
		Set(affixGroupIndex, clearPoint);
	}

	private void Set(int affixGroupIndex, int clearPoint)
	{
		_groupIndex = affixGroupIndex;
		_clearPoint = clearPoint;
		IList<AffixData> affixDataListByGroupIndex = GameRoot.Instance.AffixDataContainer.GetAffixDataListByGroupIndex(affixGroupIndex);
		if (affixDataListByGroupIndex == null)
		{
			_listAll = new List<AffixData>();
			_listSelect = new List<AffixData>();
			VespaDebug.LogError("Affix Group Data Is Null GroupIndex :: " + affixGroupIndex);
		}
		else
		{
			_listAll = affixDataListByGroupIndex;
			_listSelect = new List<AffixData>(__Extension_Method_ICollection__.COUNT(_listAll));
			ResetSelectAffix();
		}
	}

	public void SelectAffixIndices(int[] affixIndices)
	{
		ResetSelectAffix();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(affixIndices); i++)
		{
			int key = affixIndices[i];
			AffixData data = GameRoot.Instance.AffixDataContainer.GetData(key);
			if (data != null && _listAll.Contains(data))
			{
				Select(data);
			}
		}
	}

	public void ResetSelectAffix()
	{
		_listSelect.Clear();
		_advantagePoint = 0;
		_advantageCount = 0;
		_penaltyCount = 0;
		_penaltyPoint = 0;
	}

	public void ChangeGroupIndex(int affixGroupIndex, int clearPoint)
	{
		_clearPoint = clearPoint;
		if (_groupIndex != affixGroupIndex)
		{
			Set(affixGroupIndex, clearPoint);
		}
	}

	public EAffixCompState GetAffixCompState(AffixData affixData)
	{
		if (affixData == null)
		{
			return EAffixCompState.Disable;
		}
		if (SelectContains(affixData))
		{
			return EAffixCompState.Select;
		}
		if (CanSelect(affixData))
		{
			return EAffixCompState.Enable;
		}
		return EAffixCompState.Disable;
	}

	public bool SelectContains(AffixData affixData)
	{
		return _listSelect.Contains(affixData);
	}

	public bool CanSelect(bool select, AffixData affixData)
	{
		if (affixData == null)
		{
			return false;
		}
		return ((!select) ? (_sumPoint + affixData.Point) : (_sumPoint - affixData.Point)) >= 0;
	}

	public bool CanSelect(AffixData affixData)
	{
		if (affixData == null)
		{
			return false;
		}
		if (affixData.IsPenalty)
		{
			return true;
		}
		return _sumPoint + affixData.Point >= 0;
	}

	public bool Select(AffixData affixData)
	{
		if (affixData == null)
		{
			return false;
		}
		bool flag = _listSelect.Remove(affixData);
		bool isPenalty = affixData.IsPenalty;
		int point = affixData.Point;
		AddPoint(point, flag, isPenalty);
		AddCount(flag, isPenalty);
		if (!flag)
		{
			_listSelect.Add(affixData);
		}
		return false;
	}

	private void AddPoint(int point, bool remove, bool isPenalty)
	{
		if (remove)
		{
			point = -point;
		}
		_advantagePoint += ((!isPenalty) ? point : 0);
		_penaltyPoint += (isPenalty ? point : 0);
	}

	private void AddCount(bool remove, bool isPenalty)
	{
		int num = ((!remove) ? 1 : (-1));
		_advantageCount += ((!isPenalty) ? num : 0);
		_penaltyCount += (isPenalty ? num : 0);
	}

	public IList<AffixData> GetFilteredList(EAffixFilterType filterType)
	{
		if (filterType == EAffixFilterType.All)
		{
			return _listAll;
		}
		List<AffixData> list = new List<AffixData>();
		bool flag = filterType == EAffixFilterType.Penalty;
		for (int i = 0; i < _listAll.Count; i++)
		{
			AffixData affixData = _listAll[i];
			if (affixData != null && affixData.IsPenalty == flag)
			{
				list.Add(affixData);
			}
		}
		return list;
	}

	public List<AffixData> GetSelectList()
	{
		return _listSelect;
	}

	public int GetClearPoint()
	{
		return _clearPoint;
	}

	public int GetSumPoint()
	{
		return _sumPoint;
	}

	public int GetAdvantagePoint()
	{
		return _advantagePoint;
	}

	public int GetAdvantageCount()
	{
		return _advantageCount;
	}

	public int GetTotalCount()
	{
		return __Extension_Method_ICollection__.COUNT(_listAll);
	}

	public int GetSelectCount()
	{
		return __Extension_Method_ICollection__.COUNT(_listSelect);
	}

	public int GetPenaltyPoint()
	{
		return _penaltyPoint;
	}

	public int GetPenaltyCount()
	{
		return _penaltyCount;
	}
}
