using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class AccessoryCostumeGroupData : IDataKeySelector<int>
{
	int IDataKeySelector<int>.Key => CostumeGroupIndex;

	[Key(0)]
	public int CostumeGroupIndex { get; set; }

	[Key(1)]
	public string CategoryName { get; set; }

	[Key(2)]
	public string TexturePath { get; set; }

	[IgnoreMember]
	public string LocalizeGroupName => GameRoot.LocalizeProxy.GetCache_or_Build(this, "LocalizeGroupName", BuildLocalize_LocalizeGroupName);

	private string BuildLocalize_LocalizeGroupName()
	{
		string key = "COSTUME_GROUP_" + CategoryName.ToUpper();
		return GameRoot.LocalizeProxy.GetString(key);
	}
}
