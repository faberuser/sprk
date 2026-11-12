using NShared.NJit;

namespace NShared;

public class PatchDownloadSpecDataContainer : JitDataContainerDefault<int, PatchDownloadSpecData>, IData_from_PreloadInAppBytes
{
	public const string TABLENAME = "DefaultTableData/PatchDownloadSpecTable";

	private PatchDownloadSpecData _recommandedSpecData;

	public override TablePathDefine PathDefine => new TablePathDefine("DefaultTableData/PatchDownloadSpecTable");

	public PatchDownloadSpecData GetRecommendedSpecData(PatchPlatform platform, int deviceRam_MBSize, bool IsATSCSupported)
	{
		if (_recommandedSpecData != null)
		{
			return _recommandedSpecData;
		}
		if (!IsATSCSupported)
		{
			_recommandedSpecData = GetData(PatchDownloadSpecData.GetKey(PatchSpecType.Low, platform));
		}
		else
		{
			ForEachData(delegate(PatchDownloadSpecData value)
			{
				if (value.Platform == platform)
				{
					int ramMin = value.RamMin;
					int num = value.RamMax;
					if (num < 0)
					{
						num = int.MaxValue;
					}
					if (deviceRam_MBSize >= ramMin && deviceRam_MBSize <= num)
					{
						_recommandedSpecData = value;
					}
				}
			});
		}
		if (_recommandedSpecData == null)
		{
			_recommandedSpecData = GetData(PatchDownloadSpecData.GetKey(PatchSpecType.Mid, platform));
		}
		return _recommandedSpecData;
	}
}
