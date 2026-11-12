using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class GameGuideDataContainer : JitDataContainerDefault<int, GameGuideData>
{
	public const string TABLENAME = "GameGuideTable";

	private JitDictionary<int, List<GameGuideData>> _typeListDics = new JitDictionary<int, List<GameGuideData>>();

	public override TablePathDefine PathDefine => new TablePathDefine("GameGuideTable");

	public GameGuideDataContainer()
	{
		base.Packer.RegisterAndBind(_typeListDics);
	}

	protected override void OnEndLoadFromJsonData()
	{
		foreach (GameGuideData item in base.BaseList.JsonLoad_GetOrNew())
		{
			_typeListDics.JsonLoad_AddToListValue(item.GuideType, item);
		}
	}

	public IList<GameGuideData> GetListByGuideType(int guideType)
	{
		return _typeListDics.GetOrDefault(guideType);
	}
}
