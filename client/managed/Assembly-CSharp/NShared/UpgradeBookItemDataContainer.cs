using System.Collections.Generic;
using MessagePack;
using NShared.NJit;

namespace NShared;

public class UpgradeBookItemDataContainer : SubItemDataContainer<UpgradeBookItemData>
{
	[MessagePackObject(false)]
	[JitKey]
	public struct TagGradeKey
	{
		[Key(0)]
		public CreatureTagType Tag;

		[Key(1)]
		public int Grade;

		public TagGradeKey(CreatureTagType tag, int grade)
		{
			Tag = tag;
			Grade = grade;
		}

		public bool Equals(TagGradeKey other)
		{
			if (Tag == other.Tag)
			{
				return Grade == other.Grade;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return JitOption.BuildHash(Tag, Grade);
		}

		public override string ToString()
		{
			return $"Tag={Tag} Grade={Grade}";
		}
	}

	public const string TABLENAME = "UpgradeBookItemTable";

	private JitDictionary<TagGradeKey, UpgradeBookItemData> _dicByTagGrade = new JitDictionary<TagGradeKey, UpgradeBookItemData>();

	public override TablePathDefine PathDefine => new TablePathDefine("UpgradeBookItemTable");

	public UpgradeBookItemDataContainer()
	{
		base.Packer.RegisterAndBind(_dicByTagGrade);
	}

	public override ItemType[] GetSubTypes()
	{
		return new ItemType[1] { ItemType.UpgradeBook };
	}

	protected override void OnEndLoadFromJsonData()
	{
		base.OnEndLoadFromJsonData();
		foreach (KeyValuePair<string, UpgradeBookItemData> item in base.BaseDic.JsonLoad_Get())
		{
			TagGradeKey key = new TagGradeKey(item.Value.TagType, item.Value.Grade);
			_dicByTagGrade.JsonLoad_Get().Add(key, item.Value);
		}
	}

	public UpgradeBookItemData GetDataByTagTypeGrade(CreatureTagType tagType, int grade)
	{
		return _dicByTagGrade.GetOrDefault(new TagGradeKey(tagType, grade));
	}
}
