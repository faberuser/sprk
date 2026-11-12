using System.Collections.Generic;

namespace NGameAsset.NEffect;

public interface IModelData
{
	string GetName();

	CommonData GetCommonData();

	List<PlaySequenceData> GetPlaySequenceList();

	EndingDisappearData GetEndingDisappear();

	List<RendererExtraData> GetRenderExtraList();
}
