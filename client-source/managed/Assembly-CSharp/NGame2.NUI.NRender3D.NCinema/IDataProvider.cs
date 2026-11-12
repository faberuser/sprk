using System.Collections.Generic;
using NGameAsset.NCreature;
using UnityEngine;

namespace NGame2.NUI.NRender3D.NCinema;

public interface IDataProvider
{
	List<ICommandMessageHandler> HandlerList { get; }

	List<NodeTreeKeyword> KeywordList { get; }

	HashSet<AnimationKeyData> PreloadAnimationSet { get; }

	int TextureWidth { get; set; }

	int TextureHeight { get; set; }

	string NodeTreePack { get; set; }

	string NodeTreeName { get; set; }

	bool EnableInteraction { get; set; }

	UITexture Texture { get; }

	RenderFacadeInteractionProvider GetRenderFacadeInteractionDataProvider(string code);

	void SetTexture(UITexture nodeTreeTexture);

	void RemoveTexture(UITexture nodeTreeTexture);

	void SetRenderTexture(RenderTexture renderTexture);

	void Close();
}
