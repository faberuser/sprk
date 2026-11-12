using UnityEngine;

namespace NShader;

public class Define
{
	public static int COLOR_ID = Shader.PropertyToID("_Color");

	public static int COLORMIX_ID = Shader.PropertyToID("_ColorMix");

	public static int MAINTEX_ID = Shader.PropertyToID("_MainTex");

	public static int ALPHATEX_ID = Shader.PropertyToID("_AlphaTex");

	public static int GUILDFLAG_BG_TEX_ID = Shader.PropertyToID("_FlagBgTex");

	public static int GUILDFLAG_MARK_COLORTEX_ID = Shader.PropertyToID("_FlagMarkTex");

	public static int GUILDFLAG_MARK_ALPHATEX_ID = Shader.PropertyToID("_FlagMarkAlphaTex");

	public static int BLENDMODESRC_ID = Shader.PropertyToID("_BlendModeSrc");

	public static int BLENDMODEDST_ID = Shader.PropertyToID("_BlendModeDst");

	public static int EXTRAALPHA_ID = Shader.PropertyToID("_ExtraAlpha");

	public static int _ExtraRenderFx_Grayscale = Shader.PropertyToID("_ExtraRenderFx_Grayscale");

	public static int _ShaderTransform_IsEnable = Shader.PropertyToID("_ShaderTransform_IsEnable");

	public static int _ShaderTransform_PreTransform = Shader.PropertyToID("_ShaderTransform_PreTransform");

	public static int _ShaderTransform_PostTransform = Shader.PropertyToID("_ShaderTransform_PostTransform");

	public static int _ShaderTransform_NormalTransform = Shader.PropertyToID("_ShaderTransform_NormalTransform");

	public static int GLOBALTIME_ID = Shader.PropertyToID("_GlobalTime");
}
