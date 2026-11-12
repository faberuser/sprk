namespace NShared;

[JsonSerializable(Javascript = true)]
public class ResultCreatureInfo
{
	public int Index { get; set; }

	public string Key { get; set; }

	public long Hp { get; set; }

	public int Mp { get; set; }

	public int TeamId { get; set; }

	public long MaxHp { get; set; }

	public int MaxMp { get; set; }

	public long GivedDamage { get; set; }

	public static Tuple<string, int, int> IsEqual(ResultCreatureInfo lhs, ResultCreatureInfo rhs)
	{
		if (lhs == null)
		{
			if (rhs != null)
			{
				return new Tuple<string, int, int>("TeamId", lhs.TeamId, rhs.TeamId);
			}
			return null;
		}
		if (lhs.Index != rhs.Index)
		{
			return new Tuple<string, int, int>("Index", lhs.Index, rhs.Index);
		}
		if (lhs.Hp != rhs.Hp)
		{
			return new Tuple<string, int, int>("Hp", (int)lhs.Hp, (int)rhs.Hp);
		}
		if (lhs.Mp != rhs.Mp)
		{
			return new Tuple<string, int, int>("Mp", lhs.Mp, rhs.Mp);
		}
		if (lhs.TeamId != rhs.TeamId)
		{
			return new Tuple<string, int, int>("TeamId", lhs.TeamId, rhs.TeamId);
		}
		return null;
	}

	public static string GetKey(int heroIndex, int waveSlotIndex, int teamId)
	{
		return heroIndex + "_" + waveSlotIndex + "_" + teamId;
	}

	public static bool ParseKey(string key, out int waveSlotIndex, out int teamId)
	{
		waveSlotIndex = 0;
		teamId = 0;
		if (string.IsNullOrEmpty(key))
		{
			return false;
		}
		string[] array = key.Split(new char[1] { '_' });
		if (array.Length < 3)
		{
			return false;
		}
		int result = 0;
		if (!int.TryParse(array[0], out result))
		{
			return false;
		}
		if (!int.TryParse(array[1], out waveSlotIndex))
		{
			return false;
		}
		if (!int.TryParse(array[2], out teamId))
		{
			return false;
		}
		return true;
	}
}
