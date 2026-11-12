using System.Collections.Generic;
using NShared;

namespace NGame2.NUI.NComponent2.NPayShop;

public class PayShopPairData<TData> : IDataKeySelector<int> where TData : IDataKeySelector<int>
{
	public List<TData> Datas = new List<TData>();

	public int Key { get; set; }
}
