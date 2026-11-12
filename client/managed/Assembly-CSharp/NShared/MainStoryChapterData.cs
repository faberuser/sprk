using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class MainStoryChapterData : IDataKeySelector<int>
{
	[Key(0)]
	public int StoryIndex { get; set; }

	[Key(1)]
	public int ChapterIndex { get; set; }

	[Key(2)]
	public string Name { get; set; }

	[Key(3)]
	public string BackgroundAssetPath { get; set; }

	int IDataKeySelector<int>.Key => StoryIndex;

	public override string ToString()
	{
		return "StoryIndex=" + StoryIndex + " " + Name;
	}
}
