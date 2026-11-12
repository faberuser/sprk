namespace NShared;

[JsonSerializable]
public class NpcGenerateNameData : IDataKeySelector<NpcGenerateNameKey>
{
	NpcGenerateNameKey IDataKeySelector<NpcGenerateNameKey>.Key => new NpcGenerateNameKey(Name1, Name2);

	public string Name1 { get; set; }

	public string Name2 { get; set; }
}
