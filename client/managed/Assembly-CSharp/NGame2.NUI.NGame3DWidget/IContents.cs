using System;
using NVespa;

namespace NGame2.NUI.NGame3DWidget;

public interface IContents
{
	void Play(ITransformData extraTransform, ILocalTimer localTimer, Action onComplete, bool playSync = false);

	void PlayPhase(string phaseName);

	void Stop(bool immediatelyClear);

	void ChainUpdate();

	void ReleaseAll();

	int GetInstanceCount();

	void UpdateExtraRenderQueue(UIPanel panel, RenderQueueVisitor visitor);

	void UpdateAlpha(UIWidget widget);

	bool ISDONE();
}
