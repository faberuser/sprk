using System;
using NGame2.NUI.NGame3DWidget;
using NVespa;

namespace NGame2.NUI.NGame3DWidgetFromRes;

public class ContentsInvalid : IContents
{
	void IContents.Play(TransformData extraTransform, ILocalTimer localTimer, Action onComplete)
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
}
