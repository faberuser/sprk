using System;
using System.Collections.Generic;

namespace NGameAsset;

public class BundleAssetKey
{
	public class EqualityComparer : IEqualityComparer<BundleAssetKey>
	{
		public bool Equals(BundleAssetKey x, BundleAssetKey y)
		{
			if (x.BundleFilename == y.BundleFilename && x.AssetName == y.AssetName)
			{
				return x.AssetType == y.AssetType;
			}
			return false;
		}

		public int GetHashCode(BundleAssetKey obj)
		{
			return obj._hashCode;
		}
	}

	private int _hashCode;

	public string BundleFilename { get; private set; }

	public string AssetName { get; private set; }

	public Type AssetType { get; private set; }

	public BundleAssetKey(string bundleFileName, string assetName, Type assetType)
	{
		BundleFilename = bundleFileName;
		AssetName = assetName;
		AssetType = assetType;
		_hashCode = BundleFilename.GetHashCode() ^ AssetName.GetHashCode() ^ AssetType.GetHashCode();
	}
}
