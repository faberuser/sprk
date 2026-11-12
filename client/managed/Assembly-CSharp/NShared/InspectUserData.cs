using MessagePack;

namespace NShared;

[MessagePackObject(false)]
[JsonSerializable(Javascript = true)]
public class InspectUserData : IDataKeySelector<InspectUserKey>
{
	[Key(0)]
	public InspectUserCategory Category { get; set; }

	[Key(1)]
	public InspectUserType Type { get; set; }

	[Key(2)]
	public string StringKey { get; set; }

	InspectUserKey IDataKeySelector<InspectUserKey>.Key => new InspectUserKey(Category, Type);

	public override string ToString()
	{
		return "ButtonType = " + Type;
	}
}
