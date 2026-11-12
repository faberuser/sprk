using System;
using System.Collections.Generic;
using NShader;
using NShared;
using NTEC.PPU;
using NVespa;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace NGame2.NCutScene;

public class PostProcessManager
{
	private enum ConstantDataValue
	{
		Enabled,
		EnableWhenRimLightActivated,
		Disabled
	}

	public readonly Dictionary<Type, Func<PostProcessEffectSettings>> SettingCreatorPair = new Dictionary<Type, Func<PostProcessEffectSettings>>();

	private PostProcessProfile _profile;

	protected PostProcessProfile Profile
	{
		get
		{
			if (_profile == null)
			{
				PostProcessVolume volume = GetVolume();
				volume.enabled = IsEnabled();
				volume.isGlobal = true;
				_profile = volume.profile;
			}
			return _profile;
		}
	}

	public PostProcessManager()
	{
		RegistCreator<Bloom>();
		RegistCreator<UnityEngine.Rendering.PostProcessing.ChromaticAberration>();
		RegistCreator<Grayscale>();
		RegistCreator<LensDistortion>();
		RegistCreator<NShader.Sepia>();
		RegistCreator<Vignette>();
		RegistCreator<CrossedEyes>();
		RegistCreator<Drunk>();
		RegistCreator<Frame>();
		RegistCreator<IndustrialCamera>();
		RegistCreator<Infrared>();
		RegistCreator<Invert>();
		RegistCreator<Noctovision>();
		RegistCreator<Noise>();
		RegistCreator<OldFilm>();
		RegistCreator<Pixelize>();
		RegistCreator<RadialBlur2>();
		RegistCreator<SelectColor>();
		RegistCreator<Threshold>();
	}

	private void RegistCreator<T>() where T : PostProcessEffectSettings
	{
		SettingCreatorPair[typeof(T)] = () => Create<T>();
	}

	private T Create<T>() where T : PostProcessEffectSettings
	{
		T outSetting = null;
		Profile.TryGetSettings<T>(out outSetting);
		if (outSetting == null)
		{
			outSetting = _profile.AddSettings<T>();
			outSetting.GetType();
		}
		return outSetting;
	}

	public T GetCreator<T>() where T : PostProcessEffectSettings
	{
		return (T)Vespa_System_Action_Extension_Method.fire(SettingCreatorPair[typeof(T)]);
	}

	private bool IsEnabled()
	{
		return (ConstantDataValue)GameRoot.Instance.ConstantDataContainer.GetValueInt("CutScenePostProcessing") switch
		{
			ConstantDataValue.Enabled => true, 
			ConstantDataValue.Disabled => false, 
			_ => SystemOptionData.Render_EnableRimLight, 
		};
	}

	private PostProcessVolume GetVolume()
	{
		Camera cameraOf = GetCameraOf(GameUIRoot.ELayerType.CutScene3D);
		if (cameraOf == null)
		{
			VespaDebug.LogError($"Camera is null of layer({GameUIRoot.ELayerType.CutScene3D.ToString()})");
		}
		return Vespa_Unity_Component_Extension_Method.GetComponentAuto<PostProcessVolume>(cameraOf);
	}

	private Camera GetCameraOf(GameUIRoot.ELayerType layerType)
	{
		return GameUIRoot.instance.GetLayerInfo(layerType).Camera;
	}

	public void OnEndCutScene()
	{
		RemoveSettingsInternal();
	}

	private void RemoveSettingsInternal()
	{
		foreach (Type key in SettingCreatorPair.Keys)
		{
			RemoveSetting(key);
		}
	}

	public bool RemoveSetting(Type setting)
	{
		if (!Profile.HasSettings(setting))
		{
			return false;
		}
		Profile.RemoveSettings(setting);
		return true;
	}

	public void Release()
	{
	}
}
