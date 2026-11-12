using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
[JitKey]
public struct NewPayShopProductDataKey2
{
	[Key(0)]
	public string ProductId;

	[Key(1)]
	public int ProductIndex;

	public NewPayShopProductDataKey2(string productId, int productIndex)
	{
		ProductId = productId;
		ProductIndex = productIndex;
	}

	public bool Equals(NewPayShopProductDataKey2 other)
	{
		if (ProductId == other.ProductId)
		{
			return ProductIndex == other.ProductIndex;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(ProductId, ProductIndex);
	}

	public override string ToString()
	{
		return $"{GetType().Name}({ProductId}, {ProductIndex})";
	}
}
