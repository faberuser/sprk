using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class CostumeGroupData : IDataKeySelector<int>
{
	int IDataKeySelector<int>.Key => CostumeGroupIndex;

	[Key(0)]
	public int CostumeGroupIndex { get; set; }

	[Key(1)]
	public string GroupName { get; set; }

	[Key(2)]
	public string TexturePath { get; set; }

	[Key(4)]
	public string SelectItemGroup { get; set; }

	[Key(5)]
	public EHeroCostumeType CostumeType { get; set; }

	[IgnoreMember]
	public string LocalizeGroupName => GameRoot.LocalizeProxy.GetCache_or_Build(this, "LocalizeGroupName", BuildLocalize_LocalizeGroupName);

	private string BuildLocalize_LocalizeGroupName()
	{
		string key = "COSTUME_GROUP_" + GroupName.ToUpper();
		return GameRoot.LocalizeProxy.GetString(key);
	}
}
