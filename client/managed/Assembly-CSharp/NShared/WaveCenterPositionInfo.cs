using NShared.NUtil;

namespace NShared;

[JsonSerializable]
public class WaveCenterPositionInfo
{
	public long Mx { get; set; }

	public long My { get; set; }

	public long Mz { get; set; }

	public int GroupIndex { get; set; }

	public Vector3 WaveCenter => new Vector3(Mx, My, Mz);

	public WaveCenterPositionInfo()
	{
		GroupIndex = 0;
		Mx = 0L;
		My = 0L;
		Mz = 0L;
	}

	public WaveCenterPositionInfo(int groupIndex, int x, int y, int z)
	{
		GroupIndex = groupIndex;
		Mx = x;
		My = y;
		Mz = z;
	}

	public void SetPosition(Vector3 pos)
	{
		Mx = pos.Mx;
		My = pos.My;
		Mz = pos.Mz;
	}
}
