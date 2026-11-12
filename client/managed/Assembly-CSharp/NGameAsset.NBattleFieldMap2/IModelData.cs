using System.Collections.Generic;

namespace NGameAsset.NBattleFieldMap2;

public interface IModelData
{
	string AssetName { get; }

	List<RenderExtraData> GetRenderExtraList();
}
