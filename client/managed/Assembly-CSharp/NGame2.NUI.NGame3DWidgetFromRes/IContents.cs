using System;
using NGame2.NUI.NGame3DWidget;
using NVespa;

namespace NGame2.NUI.NGame3DWidgetFromRes;

public interface IContents
{
	void Play(TransformData extraTransform, ILocalTimer localTimer, Action onComplete);

	void PlayPhase(string phaseName);

	void Stop(bool immediatelyClear);

	void ChainUpdate();

	void ReleaseAll();

	int GetInstanceCount();

	void UpdateExtraRenderQueue(UIPanel panel, RenderQueueVisitor visitor);
}
