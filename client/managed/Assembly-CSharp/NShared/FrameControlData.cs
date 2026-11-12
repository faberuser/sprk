using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class FrameControlData : IDataKeySelector<string>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public string Name { get; set; }

	[Key(2)]
	public EContentType ContentType { get; set; }

	[Key(3)]
	public EDelayType DelayType { get; set; }

	[Key(4)]
	public int Auto { get; set; }

	[Key(5)]
	public int AutoPlus { get; set; }

	string IDataKeySelector<string>.Key => Name;

	public override string ToString()
	{
		return "Name: " + Name + "\nLayer: " + ContentType.ToString() + "\nDelayType: " + DelayType.ToString() + "\nAuto: " + Auto + "\nAuto+: " + AutoPlus;
	}
}
