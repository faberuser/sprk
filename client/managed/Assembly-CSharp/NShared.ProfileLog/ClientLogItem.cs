using System;

namespace NShared.ProfileLog;

[Serializable]
[JsonSerializable]
public class ClientLogItem
{
	public string TableName
	{
		get
		{
			return "ClientProfile";
		}
		set
		{
		}
	}

	public long AccountId { get; set; }

	public string Version { get; set; }

	public string DeviceModel { get; set; }

	public string CpuArch { get; set; }

	public string MemorySpec { get; set; }

	public string TextureSpec { get; set; }

	public int ShaderLod { get; set; }

	public string ShadowType { get; set; }

	public string SceneName { get; set; }

	public string TopWindow { get; set; }

	public string ChapterIndice { get; set; }

	public long PeakMemoryMb { get; set; }

	public float TimeRange { get; set; }

	public int FrameCount { get; set; }

	public float SceneFps { get; set; }

	public float BadSceneFrameRatio { get; set; }

	public float BattleFps { get; set; }

	public float BadBattleFrameRatio { get; set; }

	public int SuperSlowBattleFrames { get; set; }

	public float RttAvgMs { get; set; }

	public float RttSd { get; set; }

	public float BattleLogUpdateTime { get; set; }

	public int SuperSlowBattleLogs { get; set; }

	public float HttpRttAvgMs { get; set; }

	public float HttpRttMax { get; set; }

	public string HttpRttMaxName { get; set; }
}
