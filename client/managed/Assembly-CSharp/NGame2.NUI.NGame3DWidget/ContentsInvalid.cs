using System;
using NVespa;

namespace NGame2.NUI.NGame3DWidget;

public class ContentsInvalid : IContents
{
	void IContents.Play(ITransformData extraTransform, ILocalTimer localTimer, Action onComplete, bool playSync)
	{
	}

	void IContents.PlayPhase(string phaseName)
	{
	}

	void IContents.Stop(bool immediatelyClear)
	{
	}

	void IContents.ChainUpdate()
	{
	}

	void IContents.ReleaseAll()
	{
	}

	int IContents.GetInstanceCount()
	{
		return 0;
	}

	void IContents.UpdateExtraRenderQueue(UIPanel panel, RenderQueueVisitor visitor)
	{
	}

	void IContents.UpdateAlpha(UIWidget widget)
	{
	}

	bool IContents.ISDONE()
	{
		return false;
	}
}
