using UnityEngine;

namespace NShader;

public interface IFilter
{
	void ImageProcess(RenderTexture source, RenderTexture dest, bool discardDest);
}
