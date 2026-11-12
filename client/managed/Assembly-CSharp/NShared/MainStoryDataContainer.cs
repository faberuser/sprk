using System.Collections;
using NShared.NJit;

namespace NShared;

public class MainStoryDataContainer : JitDataContainerDefault<MainStoryKey, MainStoryData>
{
	public const string TABLENAME = "MainStoryTable";

	private int _currentChapterIndex;

	private int _lastStoryNoOfChapter;

	public override TablePathDefine PathDefine => new TablePathDefine("MainStoryTable");

	protected override void OnLoadingValue_from_JsonData(IDictionary jsonDic, MainStoryData value)
	{
		MainStoryKey kEY = value.GETKEY();
		if (kEY.ChapterIndex != _currentChapterIndex)
		{
			_currentChapterIndex = kEY.ChapterIndex;
			_lastStoryNoOfChapter = 0;
		}
		value.StoryNo = ++_lastStoryNoOfChapter;
	}

	public MainStoryData GetData(int chapterIndex, int storyNo)
	{
		return GetData(new MainStoryKey(chapterIndex, storyNo));
	}
}
