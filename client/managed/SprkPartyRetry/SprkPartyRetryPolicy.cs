using System;

public static class SprkPartyRetryPolicy
{
	public enum Kind
	{
		None,
		Vote,
		Cancel,
		Restart
	}

	public struct Notice
	{
		public Kind Kind;

		public int Vote;

		public long Proposer;

		public int Seconds;

		public string Reason;

		public string Who;

		public string Start;
	}

	public const string Prefix = "sprk-retry:";

	public const string MessageType = "SprkRetry";

	public static bool Eligible(bool multiplay, int battleType)
	{
		if (!multiplay)
		{
			return false;
		}
		switch (battleType)
		{
		case 1:
		case 10:
		case 12:
		case 29:
		case 30:
		case 33:
		case 40:
		case 41:
		case 42:
		case 45:
		case 46:
			return true;
		default:
			return false;
		}
	}

	public static string Propose()
	{
		return "sprk-retry:propose";
	}

	public static string Answer(int vote, bool yes)
	{
		return "sprk-retry:" + (yes ? "yes:" : "no:") + vote;
	}

	public static Notice Parse(string type, string content)
	{
		Notice result = new Notice
		{
			Kind = Kind.None
		};
		if (type != "SprkRetry" || string.IsNullOrEmpty(content))
		{
			return result;
		}
		string[] array = content.Split(new char[1] { '|' }, 2);
		try
		{
			switch (array[0])
			{
			case "Vote":
			{
				string[] array4 = content.Split(new char[1] { '|' }, 5);
				if (array4.Length < 5)
				{
					return result;
				}
				return new Notice
				{
					Kind = Kind.Vote,
					Vote = int.Parse(array4[1]),
					Proposer = long.Parse(array4[2]),
					Seconds = int.Parse(array4[3]),
					Who = array4[4]
				};
			}
			case "Cancel":
			{
				string[] array3 = content.Split(new char[1] { '|' }, 4);
				if (array3.Length < 4)
				{
					return result;
				}
				return new Notice
				{
					Kind = Kind.Cancel,
					Vote = int.Parse(array3[1]),
					Reason = array3[2],
					Who = array3[3]
				};
			}
			case "Restart":
			{
				string[] array2 = content.Split(new char[1] { '|' }, 3);
				if (array2.Length < 3 || !array2[2].StartsWith("{"))
				{
					return result;
				}
				return new Notice
				{
					Kind = Kind.Restart,
					Vote = int.Parse(array2[1]),
					Start = array2[2]
				};
			}
			}
		}
		catch (FormatException)
		{
		}
		catch (OverflowException)
		{
		}
		return result;
	}

	public static string CancelMessage(Notice notice, Func<string, string, string> text, Func<string, string> failure)
	{
		switch (notice.Reason)
		{
		case "Declined":
			return notice.Who + " declined the retry.";
		case "Timeout":
			if (!string.IsNullOrEmpty(notice.Who))
			{
				return "Retry cancelled. No answer from " + notice.Who + ".";
			}
			return "The retry vote timed out.";
		case "Left":
			return text("RAID_USER_OUT_CENTER_MSG", notice.Who + " has left.") + "\nRetry cancelled.";
		case "Ended":
			return "The battle has ended. Retry cancelled.";
		case "Failed":
		{
			string text2 = failure(notice.Who);
			return "Retry could not start. The battle continues." + (string.IsNullOrEmpty(text2) ? "" : ("\n" + text2));
		}
		case "Busy":
			return "A retry vote is already in progress.";
		default:
			return "Retry is not available right now.";
		}
	}
}
