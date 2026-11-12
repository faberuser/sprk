using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class HeroAnimationViewDataContainer : JitDataContainerDefault<int, HeroAnimationViewData>
{
	public const string TABLENAME = "HeroAnimationViewTable";

	private JitDictionary<int, List<HeroAnimationViewData>> _lists_by_HeroIndex = new JitDictionary<int, List<HeroAnimationViewData>>();

	public override TablePathDefine PathDefine => new TablePathDefine("HeroAnimationViewTable");

	public HeroAnimationViewDataContainer()
	{
		base.Packer.RegisterAndBind(_lists_by_HeroIndex);
	}

	protected override void OnEndLoadFromJsonData()
	{
		foreach (HeroAnimationViewData item in base.BaseList.JsonLoad_GetOrNew())
		{
			_lists_by_HeroIndex.JsonLoad_AddToListValue(item.HeroIndex, item);
		}
	}

	public HeroAnimationViewData[] GetDatas(int heroIndex)
	{
		return _lists_by_HeroIndex.GetOrDefault(heroIndex)?.ToArray();
	}
}
