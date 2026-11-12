using UnityEngine;

namespace NNguiExtension.NInternal;

public interface IDrawCallWrapper
{
	bool isDirty { get; set; }

	UIDrawCall Native { get; }

	int depthStart { get; }

	int depthEnd { get; }

	void UpdateRenderQueue(UIPanel panel, Vector3 pos, Quaternion rot, Vector3 scale, ref int curRenderQueue);

	void SetLayer(int layer);
}
