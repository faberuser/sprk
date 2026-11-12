using System;
using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class AttendanceInfoData : IDataKeySelector<int>
{
	public enum AttendanceIconStatus
	{
		None,
		Sell,
		Progress
	}

	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public string TitleKey { get; set; }

	[Key(2)]
	public string DescKey { get; set; }

	[Key(3)]
	public string[] IconStatus { get; set; }

	[Key(4)]
	public string IconPath { get; set; }

	[Key(5)]
	public bool SendMail { get; set; }

	[Key(6)]
	public int ShowIndex { get; set; }

	[Key(7)]
	public string[] MenuNames { get; set; }

	[Key(8)]
	public string NextStepPopupTitle { get; set; }

	[Key(9)]
	public string NextStepPopupDesc { get; set; }

	[Key(10)]
	public QuestOpenConditionType ConditionType1 { get; set; }

	[Key(11)]
	public string[] ConditionValue1 { get; set; }

	int IDataKeySelector<int>.Key => Index;

	public string GetBackGroundUrl(string language)
	{
		if (!GameRoot.isValidInstance)
		{
			return string.Empty;
		}
		AttendanceUrlData data = GameRoot.Instance.AttendanceUrlDataContainer.GetData(Index, language);
		if (data == null)
		{
			return string.Empty;
		}
		return data.Url;
	}

	public bool IsShowIcon(AttendanceIconStatus status)
	{
		if (IconStatus == null)
		{
			return false;
		}
		return Array.Find(IconStatus, (string s) => s == status.ToString()) != null;
	}

	public string GetMenuTitleKey(int index)
	{
		if (MenuNames == null)
		{
			return string.Empty;
		}
		if (MenuNames.Length <= index)
		{
			return string.Empty;
		}
		return MenuNames[index];
	}

	public override string ToString()
	{
		return $"Index : {Index}";
	}
}
