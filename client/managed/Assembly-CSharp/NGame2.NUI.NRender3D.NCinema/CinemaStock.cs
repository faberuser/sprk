using System.Collections.Generic;
using NGameAsset.NCreature;

namespace NGame2.NUI.NRender3D.NCinema;

public struct CinemaStock
{
	public bool CameraOrthographicMode;

	public int RenderTargetWidth;

	public int RenderTargetHeight;

	public string NodeTreePack;

	public string NodeTreeName;

	public HashSet<AnimationKeyData> PreloadAnimationSet;
}
