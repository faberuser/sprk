using NGameAsset;
using NShader;
using NVespa;
using UnityEngine;

namespace NGame2.NRender;

internal class ReplaceTexVisitor : IGenericVisitor<RenderProxy>, IVisitor
{
	private Texture _markColorTex;

	private Rect _markUvRect;

	private Texture _markBgTex;

	private Rect _markBgUvRect;

	public ReplaceTexVisitor(AssetTexture markAssetTexture, AssetTexture markBgAssetTexture)
	{
		if (!markAssetTexture.ISDONE() || !markAssetTexture.ISVALID())
		{
			return;
		}
		AssetTexture.TextureInfo textureInfo = markAssetTexture.GetTextureInfo();
		if (textureInfo == null)
		{
			return;
		}
		_markColorTex = textureInfo.Tex;
		_markUvRect = textureInfo.UvRect;
		if (markBgAssetTexture.ISDONE() && markBgAssetTexture.ISVALID())
		{
			AssetTexture.TextureInfo textureInfo2 = markBgAssetTexture.GetTextureInfo();
			if (textureInfo2 != null && !(textureInfo2.Tex == null))
			{
				_markBgTex = textureInfo2.Tex;
				_markBgUvRect = textureInfo2.UvRect;
			}
		}
	}

	void IGenericVisitor<RenderProxy>.Visit(RenderProxy proxy)
	{
		Material[] materials = proxy.materials;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(materials); i++)
		{
			Material material = materials[i];
			if (!(material == null) && !(Vespa_Unity_Object_Extension_Method.NAME(material.shader) != "Vespa/ToonShading2/GuildFlag"))
			{
				material.SetTexture(NShader.Define.GUILDFLAG_MARK_COLORTEX_ID, _markColorTex);
				material.SetTextureOffset(NShader.Define.GUILDFLAG_MARK_COLORTEX_ID, _markUvRect.min);
				material.SetTextureScale(NShader.Define.GUILDFLAG_MARK_COLORTEX_ID, _markUvRect.size);
				if (_markBgTex != null)
				{
					material.SetTexture(NShader.Define.GUILDFLAG_BG_TEX_ID, _markBgTex);
					material.SetTextureOffset(NShader.Define.GUILDFLAG_BG_TEX_ID, _markBgUvRect.min);
					material.SetTextureScale(NShader.Define.GUILDFLAG_BG_TEX_ID, _markBgUvRect.size);
				}
			}
		}
	}
}
