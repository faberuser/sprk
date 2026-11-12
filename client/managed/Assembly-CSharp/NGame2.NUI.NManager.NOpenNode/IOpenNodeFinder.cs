using System.Collections.Generic;

namespace NGame2.NUI.NManager.NOpenNode;

internal interface IOpenNodeFinder
{
	void Find(List<ReserveEffectComp> reserveEffectList, int chapterIndex, int nodeIndex, int startDepth);
}
