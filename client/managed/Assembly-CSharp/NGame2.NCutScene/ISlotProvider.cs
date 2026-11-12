namespace NGame2.NCutScene;

public interface ISlotProvider
{
	int Count { get; }

	int GetSlotIndexByIndex(int index);

	string GetCreatureCodeByIndex(int index);
}
