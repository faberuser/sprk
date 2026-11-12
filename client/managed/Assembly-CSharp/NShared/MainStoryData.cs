using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class MainStoryData : IDataKeySelector<MainStoryKey>
{
	MainStoryKey IDataKeySelector<MainStoryKey>.Key => new MainStoryKey(ChapterIndex, StoryNo);

	[Key(0)]
	public int ChapterIndex { get; set; }

	[Key(1)]
	public int StoryNo { get; set; }

	[Key(2)]
	public string Name { get; set; }

	[Key(3)]
	public string[] NameParam { get; set; }

	[Key(5)]
	public string[] Condition { get; set; }

	[Key(6)]
	public EventTriggerCommand Command { get; set; }

	[Key(7)]
	public string[] Argument { get; set; }

	[IgnoreMember]
	public string MainStoryName => GameRoot.LocalizeProxy.GetCache_or_Build(this, "MainStoryName", BuildLocalize_MainStoryName);

	public override string ToString()
	{
		return " ChapterIndex=" + ChapterIndex + " StoryNo=" + StoryNo + " " + Name;
	}

	private string BuildLocalize_MainStoryName()
	{
		ILocalizeProxy localizeProxy = GameRoot.LocalizeProxy;
		string name = Name;
		object[] nameParam = NameParam;
		return localizeProxy.GetString(name, nameParam);
	}
}
