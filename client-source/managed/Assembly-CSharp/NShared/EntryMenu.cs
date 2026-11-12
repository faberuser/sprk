namespace NShared;

public class EntryMenu
{
	public int Index { get; set; }

	public string[] EntryMenuButton { get; set; }

	public string EntryMenuCondition { get; set; }

	public string[] EntryMenuEvent { get; set; }

	public int[] ChildIndices { get; set; }

	public int ParentIndex { get; set; }

	public bool IsValid
	{
		get
		{
			if (EntryMenuButton != null)
			{
				return EntryMenuButton.Length != 0;
			}
			return false;
		}
	}

	public bool HasChild
	{
		get
		{
			if (ChildIndices != null)
			{
				return ChildIndices.Length != 0;
			}
			return false;
		}
	}

	public bool IsChild => ParentIndex > 0;

	public EntryMenu(int index, string[] buttons, string condition, string[] events, int[] childIndices, int parentIndex)
	{
		Index = index;
		EntryMenuButton = buttons;
		EntryMenuCondition = condition;
		EntryMenuEvent = events;
		ChildIndices = childIndices;
		ParentIndex = parentIndex;
	}
}
