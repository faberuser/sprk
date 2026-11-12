using System.Collections.Generic;
using NGame2.NContentsInfo;
using NShared;
using NVespa;

namespace NGame2.NAccount;

public class AttendanceDataContainer : SingletonAuto<AttendanceDataContainer>
{
	private Dictionary<int, AttendanceData> _dicByIndex = new Dictionary<int, AttendanceData>();

	private Dictionary<EAttendanceKeyword, AttendanceData> _dicByKeyword = new Dictionary<EAttendanceKeyword, AttendanceData>();

	private List<AttendanceKeywordGroup> _listSortedByShowIndex = new List<AttendanceKeywordGroup>();

	public void SetAttendanceData(AttendanceData[] datas)
	{
		if (datas == null)
		{
			return;
		}
		_dicByIndex.Clear();
		_dicByKeyword.Clear();
		_listSortedByShowIndex.Clear();
		AttendanceData[] array = datas;
		foreach (AttendanceData attendanceData in array)
		{
			if (_dicByIndex.ContainsKey(attendanceData.Index))
			{
				VespaDebug.ErrorWithDialog("NShared.AttendanceData.Index 중복: " + attendanceData.Index);
			}
			else
			{
				_dicByIndex.Add(attendanceData.Index, attendanceData);
			}
		}
		array = datas;
		foreach (AttendanceData attendanceData2 in array)
		{
			if (_dicByKeyword.ContainsKey(attendanceData2.Keyword))
			{
				VespaDebug.ErrorWithDialog("NShared.AttendanceData.Keyword 중복: " + attendanceData2.Keyword);
			}
			else
			{
				_dicByKeyword.Add(attendanceData2.Keyword, attendanceData2);
			}
		}
		array = datas;
		foreach (AttendanceData attendanceData3 in array)
		{
			if (!AttendanceKeywordGroup.Helper.IsSubKeyword(attendanceData3.Keyword))
			{
				_listSortedByShowIndex.Add(new AttendanceKeywordGroup(attendanceData3.Keyword));
			}
		}
		_listSortedByShowIndex.Sort((AttendanceKeywordGroup lhs, AttendanceKeywordGroup rhs) => lhs.ShowIndex.CompareTo(rhs.ShowIndex));
	}

	public AttendanceData GetDataByKeyword(EAttendanceKeyword keyword)
	{
		return Vespa_Generic_IDictionary_Extension_Method.GETVALUE(_dicByKeyword, keyword, null);
	}

	public AttendanceData GetDataByIndex(int attendanceIndex)
	{
		return Vespa_Generic_IDictionary_Extension_Method.GETVALUE(_dicByIndex, attendanceIndex, null);
	}

	public IList<AttendanceKeywordGroup> GetGroupList_SortedByShowIndex()
	{
		return _listSortedByShowIndex;
	}
}
