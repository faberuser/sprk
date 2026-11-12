namespace NGame2.NUI.NManager.NOpenNode;

public class ReserveEffectComp
{
	public int ChapterIndex { get; set; }

	public int NodeIndex1 { get; set; }

	public int NodeIndex2 { get; set; }

	public int Depth { get; set; }

	public bool LastNodeAlreadyOpened { get; set; }

	public ReserveEffectComp(int chapterIndex, int nodeIndex1, int nodeIndex2, int depth, bool lastNodeAlreadyOpened)
	{
		ChapterIndex = chapterIndex;
		NodeIndex1 = nodeIndex1;
		NodeIndex2 = nodeIndex2;
		Depth = depth;
		LastNodeAlreadyOpened = lastNodeAlreadyOpened;
	}
}
