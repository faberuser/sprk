using System;
using NShared.NJit;

namespace NShared;

public class VideoDataContainer : JitDataContainerDefault<int, VideoData>
{
	public const string TABLENAME = "VideoTable";

	public override TablePathDefine PathDefine => new TablePathDefine("VideoTable");

	public string GetVideoPath(string videoCode)
	{
		string result = string.Empty;
		foreach (VideoData allDatum in GetAllData())
		{
			if (allDatum.Code.Equals(videoCode, StringComparison.OrdinalIgnoreCase))
			{
				result = allDatum.Path;
				break;
			}
		}
		return result;
	}
}
