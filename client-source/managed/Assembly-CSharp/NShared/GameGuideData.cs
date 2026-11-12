using MessagePack;

namespace NShared;

[MessagePackObject(false)]
[JsonSerializable(Javascript = true)]
public class GameGuideData : IDataKeySelector<int>
{
	int IDataKeySelector<int>.Key => Index;

	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public string Name { get; set; }

	[Key(2)]
	public int GuideType { get; set; }

	[Key(3)]
	public string GuideImage { get; set; }

	[Key(4)]
	public string MoveTaskType { get; set; }

	[Key(5)]
	public string[] MoveTaskValue { get; set; }

	[Key(6)]
	public string SummaryKEY { get; set; }

	[Key(7)]
	public string Question1 { get; set; }

	[Key(8)]
	public string Answer1 { get; set; }

	[Key(9)]
	public string Question2 { get; set; }

	[Key(10)]
	public string Answer2 { get; set; }

	[Key(11)]
	public string Question3 { get; set; }

	[Key(12)]
	public string Answer3 { get; set; }

	[Key(13)]
	public string Question4 { get; set; }

	[Key(14)]
	public string Answer4 { get; set; }

	[Key(15)]
	public string Question5 { get; set; }

	[Key(16)]
	public string Answer5 { get; set; }

	[Key(17)]
	public string Question6 { get; set; }

	[Key(18)]
	public string Answer6 { get; set; }

	[Key(19)]
	public string Question7 { get; set; }

	[Key(20)]
	public string Answer7 { get; set; }

	[Key(21)]
	public string Question8 { get; set; }

	[Key(22)]
	public string Answer8 { get; set; }

	public bool GetQandA(int index, out string q, out string a)
	{
		q = string.Empty;
		a = string.Empty;
		switch (index)
		{
		case 0:
			if (string.IsNullOrEmpty(Question1) || string.IsNullOrEmpty(Answer1))
			{
				return false;
			}
			q = Question1;
			a = Answer1;
			break;
		case 1:
			if (string.IsNullOrEmpty(Question2) || string.IsNullOrEmpty(Answer2))
			{
				return false;
			}
			q = Question2;
			a = Answer2;
			break;
		case 2:
			if (string.IsNullOrEmpty(Question3) || string.IsNullOrEmpty(Answer3))
			{
				return false;
			}
			q = Question3;
			a = Answer3;
			break;
		case 3:
			if (string.IsNullOrEmpty(Question4) || string.IsNullOrEmpty(Answer4))
			{
				return false;
			}
			q = Question4;
			a = Answer4;
			break;
		case 4:
			if (string.IsNullOrEmpty(Question5) || string.IsNullOrEmpty(Answer5))
			{
				return false;
			}
			q = Question5;
			a = Answer5;
			break;
		case 5:
			if (string.IsNullOrEmpty(Question6) || string.IsNullOrEmpty(Answer6))
			{
				return false;
			}
			q = Question6;
			a = Answer6;
			break;
		case 6:
			if (string.IsNullOrEmpty(Question7) || string.IsNullOrEmpty(Answer7))
			{
				return false;
			}
			q = Question7;
			a = Answer7;
			break;
		case 7:
			if (string.IsNullOrEmpty(Question8) || string.IsNullOrEmpty(Answer8))
			{
				return false;
			}
			q = Question8;
			a = Answer8;
			break;
		default:
			return false;
		}
		return true;
	}
}
