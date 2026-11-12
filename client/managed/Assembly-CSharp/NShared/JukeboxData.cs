using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class JukeboxData : IDataKeySelector<string>
{
	[Key(0)]
	public string Code { get; set; }

	[Key(1)]
	public int Index { get; set; }

	[Key(2)]
	public string Title { get; set; }

	[Key(3)]
	public string SubTitle { get; set; }

	[Key(4)]
	public string IconPath { get; set; }

	[Key(5)]
	public string PortraitPath { get; set; }

	[Key(6)]
	public string EffectCode { get; set; }

	string IDataKeySelector<string>.Key => Code;
}
