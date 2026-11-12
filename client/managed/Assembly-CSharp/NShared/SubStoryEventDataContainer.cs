using NShared.NJit;

namespace NShared;

public class SubStoryEventDataContainer : JitDataContainerListOnly<SubStoryEventData>
{
	public const string TABLENAME = "SubStoryEventTable";

	private JitDictionaryWithKey<SubStoryEventKey, SubStoryEventData> _baseDic = new JitDictionaryWithKey<SubStoryEventKey, SubStoryEventData>();

	public override TablePathDefine PathDefine => new TablePathDefine("SubStoryEventTable");

	public SubStoryEventDataContainer()
	{
		base.Packer.RegisterAndBind(_baseDic);
	}

	protected override void OnEndLoadFromJsonData()
	{
		int num = 0;
		int num2 = 0;
		foreach (SubStoryEventData item in base.BaseList.JsonLoad_GetOrNew())
		{
			if (item.Index == num)
			{
				num2++;
			}
			else
			{
				num = item.Index;
				num2 = 0;
			}
			item.Index = num;
			item.Sequence = num2;
			_baseDic.JsonLoad_Get().Add(item.GETKEY(), item);
		}
	}

	public SubStoryEventData GetData(int index, int sequence)
	{
		return _baseDic.GetOrDefault(new SubStoryEventKey(index, sequence));
	}
}
