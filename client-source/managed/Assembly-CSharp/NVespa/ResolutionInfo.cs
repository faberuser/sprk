using NGameAsset;
using NShared;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NVespa;

public class ResolutionInfo : SingletonAuto<ResolutionInfo>
{
	public const int LowSpecResolutionWidth = 1280;

	public const int MidSpecResolutionWidth = 1280;

	public const int HighSpecResolutionWidth = 1920;

	public int RealScreenWidth = 1920;

	public int RealScreenHeight = 1080;

	public int IdealScreenWidth = 1920;

	private LocalStorageBool _disableResolutionChange = new LocalStorageBool(i_default: true, typeof(ResolutionInfo).FullName + "_disableResolutionChange");

	private static string _deviceStyle = "";

	public int RealMetricsWidth;

	public int RealMetricsHeight;

	public int OriginalWidth { get; private set; }

	public int OriginalHeight { get; private set; }

	public int IdealScreenHeight => IdealScreenWidth * RealScreenHeight / RealScreenWidth;

	public ResolutionInfo()
	{
		OriginalWidth = Screen.width;
		OriginalHeight = Screen.height;
		RealScreenWidth = Screen.width;
		RealScreenHeight = Screen.height;
		RealMetricsWidth = OriginalWidth;
		RealMetricsHeight = OriginalHeight;
		GlobalAutoBehaviour<GuiManager>.instance.Add("ResolutionInfo", this, delegate
		{
			GUILayout.Label($"OriginalResolution {OriginalWidth}x{OriginalHeight}");
			GUILayout.Label($"Current Resolution {RealScreenWidth}x{RealScreenHeight}");
			GUILayout.Label($"Safe Rect {Screen.safeArea.width}x{Screen.safeArea.height}");
			GUILayout.Label($"RealMetrics {RealMetricsWidth}x{RealMetricsHeight}");
			_disableResolutionChange.Value = GUILayout.Toggle(_disableResolutionChange.Value, "Disable Resolution Change");
			if (String_Extension_Method.IsNullOrEmpty(_deviceStyle) && GameRoot.isValidInstance && GameRoot.Instance.DeviceModelDataContainer != null)
			{
				DeviceModelData data = GameRoot.Instance.DeviceModelDataContainer.GetData(SystemInfo.deviceModel);
				if (data == null)
				{
					_deviceStyle = DeviceStyleType.None.ToString();
				}
				else
				{
					_deviceStyle = data.DeviceStyle.ToString();
				}
			}
			GUILayout.Label("------------------------------------------------------------------------------");
			GUILayout.Label("Device Model = " + SystemInfo.deviceModel);
			if (!String_Extension_Method.IsNullOrEmpty(_deviceStyle))
			{
				GUILayout.Label("Device Style = " + _deviceStyle);
			}
			GUILayout.Label("Screen.fullScreen = " + Screen.fullScreen);
			GUILayout.Label("Screen.fullScreenMode = " + Screen.fullScreenMode);
			Resolution[] resolutions = Screen.resolutions;
			if (__Extension_Method_ICollection__.COUNT(resolutions) > 0)
			{
				GUILayout.Label("Screen.resolutions");
				GUILayout.BeginHorizontal();
				Resolution[] array = resolutions;
				for (int i = 0; i < array.Length; i++)
				{
					Resolution resolution = array[i];
					GUILayout.Label($"[{resolution.width}x{resolution.height}]");
				}
				GUILayout.EndHorizontal();
			}
			Resolution currentResolution = Screen.currentResolution;
			GUILayout.Label($"Screen.currentResolution = [{currentResolution.width}x{currentResolution.height}]");
		});
	}

	public void ResetResolution()
	{
		if (!_disableResolutionChange.Value)
		{
			int num = ((SystemOptionData.Spec == ESpecType.Low) ? 1280 : ((SystemOptionData.Spec != ESpecType.Mid) ? 1920 : 1280));
			if (OriginalWidth < Mathf.RoundToInt((float)num * 1.125f))
			{
				num = OriginalWidth;
			}
			RealScreenWidth = num;
			RealScreenHeight = RealScreenWidth * OriginalHeight / OriginalWidth;
			if (RealScreenWidth != Screen.width)
			{
				Screen.SetResolution(RealScreenWidth, RealScreenHeight, fullscreen: true);
			}
		}
	}

	public void GetScreenRenderTargetSize(out int width, out int height)
	{
		width = RealScreenWidth;
		height = RealScreenHeight;
	}

	public void CalcRenderTargetSize(int idealWidth, int idealHeight, out int width, out int height)
	{
		GetScreenRenderTargetSize(out var width2, out var _);
		width = idealWidth * width2 / IdealScreenWidth;
		height = idealHeight * width2 / IdealScreenWidth;
	}
}
