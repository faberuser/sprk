using System;

namespace NGame2.NUI.NWindow;

[Serializable]
public class MoveDirectRegistInspector
{
	public enum MoveDirectType
	{
		MoveNode,
		MoveMap
	}

	public int ChapterIndex;

	public int DungeonIndex;

	public MoveDirectType Type;
}
