using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class PcAvatarDataContainer : JitDataContainerDefault<PcAvatarDataKey, PcAvatarData>
{
	public const string TABLENAME = "PcAvatarTable";

	private JitDictionary<int, List<PcAvatarData>> _dicByChapter = new JitDictionary<int, List<PcAvatarData>>();

	public override TablePathDefine PathDefine => new TablePathDefine("PcAvatarTable");

	public PcAvatarDataContainer()
	{
		base.Packer.RegisterAndBind(_dicByChapter);
	}

	protected override void OnEndLoadFromJsonData()
	{
		foreach (PcAvatarData item in base.BaseList.JsonLoad_GetOrNew())
		{
			_dicByChapter.JsonLoad_AddToListValue(item.ChapterIndex, item);
		}
	}

	public IList<PcAvatarData> GetDatasByChapter(int index)
	{
		return _dicByChapter.GetOrDefault(index);
	}
}
