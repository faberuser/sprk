using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NShared;
using NVespa;
using NVespa.NGameSharedSetting;
using NVespa.NGlobal;
using NVespa.NGui;
using NVespa.NSingleton;
using UnityEngine;
using UnityEngine.Rendering;

namespace NShader;

[ExecuteInEditMode]
public class ShaderVariableUpdater : GlobalAutoBehaviour<ShaderVariableUpdater>, IUpdater
{
	private static int _lastUpdateFrame = int.MinValue;

	private static bool _useRunTime;

	public static float IosOutlineFactor = 0.1f;

	public static float AosOutlineFactor = 0.1f;

	private void Start()
	{
		GlobalUpdateBehaviour.AddUpdater(this);
		if (!IsOpenGL())
		{
			AosOutlineFactor = -0.05f;
			IosOutlineFactor = -0.05f;
		}
		GlobalAutoBehaviour<GuiManager>.instance.Add("ShaderVariableUpdater", this, delegate
		{
			using (new BlockManager().Horizontal())
			{
				ShaderData.ShaderVariableData shaderVariable = GameSharedSetting.data.Shader.ShaderVariable;
				GUILayout.Label("Outline Color R", GUILayout.Width(150f));
				shaderVariable.OutlineColor.r = GUILayout.HorizontalSlider(shaderVariable.OutlineColor.r, 0f, 1f);
			}
			using (new BlockManager().Horizontal())
			{
				ShaderData.ShaderVariableData shaderVariable2 = GameSharedSetting.data.Shader.ShaderVariable;
				GUILayout.Label("Outline Color G", GUILayout.Width(150f));
				shaderVariable2.OutlineColor.g = GUILayout.HorizontalSlider(shaderVariable2.OutlineColor.g, 0f, 1f);
			}
			using (new BlockManager().Horizontal())
			{
				ShaderData.ShaderVariableData shaderVariable3 = GameSharedSetting.data.Shader.ShaderVariable;
				GUILayout.Label("Outline Color B", GUILayout.Width(150f));
				shaderVariable3.OutlineColor.b = GUILayout.HorizontalSlider(shaderVariable3.OutlineColor.b, 0f, 1f);
			}
			using (new BlockManager().Horizontal())
			{
				GUILayout.Label("IosOutlineFactor (" + IosOutlineFactor + ")", GUILayout.Width(150f));
				IosOutlineFactor = GUILayout.HorizontalSlider(IosOutlineFactor, -1f, 1f);
			}
			using (new BlockManager().Horizontal())
			{
				GUILayout.Label("AosOutlineFactor (" + AosOutlineFactor + ")", GUILayout.Width(150f));
				AosOutlineFactor = GUILayout.HorizontalSlider(AosOutlineFactor, -1f, 1f);
			}
			using (new BlockManager().Horizontal())
			{
				_useRunTime = GUILayout.Toggle(_useRunTime, "EditRunTime");
			}
			if (_useRunTime)
			{
				using (new BlockManager().Horizontal())
				{
					ShaderData.ShaderVariableData shaderVariable4 = GameSharedSetting.data.Shader.ShaderVariable;
					GUILayout.Label("ShadowDirection X :: " + shaderVariable4.ShadowDirection.x, GUILayout.Width(300f));
					shaderVariable4.ShadowDirection.x = GUILayout.HorizontalSlider(shaderVariable4.ShadowDirection.x, -1f, 1f);
				}
				using (new BlockManager().Horizontal())
				{
					ShaderData.ShaderVariableData shaderVariable5 = GameSharedSetting.data.Shader.ShaderVariable;
					GUILayout.Label("ShadowDirection Y :: " + shaderVariable5.ShadowDirection.y, GUILayout.Width(300f));
					shaderVariable5.ShadowDirection.y = GUILayout.HorizontalSlider(shaderVariable5.ShadowDirection.y, -1f, 1f);
				}
			}
		});
	}

	private bool IsOpenGL()
	{
		if (SystemInfo.graphicsDeviceType != GraphicsDeviceType.OpenGLCore && SystemInfo.graphicsDeviceType != GraphicsDeviceType.OpenGLES2)
		{
			return SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES3;
		}
		return true;
	}

	private void Update()
	{
	}

	void IUpdater.ChainUpdate()
	{
		UpdateInternal();
	}

	private bool NeedUpdate()
	{
		if (Time.frameCount == _lastUpdateFrame)
		{
			return false;
		}
		_lastUpdateFrame = Time.frameCount;
		return true;
	}

	private void UpdateInternal()
	{
		if (NeedUpdate())
		{
			UpdateGlobalVariable();
		}
	}

	private static Vector3 GetShadowDirection()
	{
		if (GlobalAutoBehaviour<GlobalManagement>.isValidInstance && !_useRunTime)
		{
			IWindowShadowShader topOpendWindwShadowShader = GlobalAutoBehaviour<GlobalManagement>.instance.GetTopOpendWindwShadowShader();
			if (topOpendWindwShadowShader != null)
			{
				return topOpendWindwShadowShader.ShadowDirection;
			}
		}
		if (SceneAutoBehaviour<WorldMapManagement>.isValidInstance && !_useRunTime)
		{
			CampaignChapterData campaignChapterData = SceneAutoBehaviour<WorldMapManagement>.instance.GetCampaignChapterData();
			if (campaignChapterData != null)
			{
				float[] shadowDirection = campaignChapterData.ShadowDirection;
				if (__Extension_Method_ICollection__.COUNT(shadowDirection) == 2)
				{
					return new Vector3(shadowDirection[0], shadowDirection[1], -1f);
				}
			}
		}
		return GameSharedSetting.data.Shader.ShaderVariable.ShadowDirection;
	}

	public static void UpdateGlobalVariable()
	{
		ShaderData.ShaderVariableData shaderVariable = GameSharedSetting.data.Shader.ShaderVariable;
		Vector3 shadowDirection = GetShadowDirection();
		float value = ((Application.platform == RuntimePlatform.IPhonePlayer) ? IosOutlineFactor : AosOutlineFactor);
		Shader.SetGlobalVector("_RimDirOnView", new Vector4(shaderVariable.RimLightDirection.x, shaderVariable.RimLightDirection.y, shaderVariable.RimLightDirection.z, 0f));
		Shader.SetGlobalColor("_RimLightColor", shaderVariable.RimLightColor);
		Shader.SetGlobalVector("_ShadowDirection", new Vector4(shadowDirection.x, shadowDirection.z, shadowDirection.y, 0f));
		Shader.SetGlobalColor("_OutlineColor", shaderVariable.OutlineColor);
		Shader.SetGlobalFloat("_Outline", shaderVariable.OutlineWidth);
		Shader.SetGlobalFloat("_OutlineFactor", value);
	}
}
