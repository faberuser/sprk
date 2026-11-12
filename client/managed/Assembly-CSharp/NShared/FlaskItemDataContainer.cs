using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class FlaskItemDataContainer : SubItemDataContainer<FlaskItemData>
{
	public const string TABLENAME = "FlaskItemTable";

	private JitDictionary<int, FlaskItemData> _dicByPotionItemIndex = new JitDictionary<int, FlaskItemData>();

	public override TablePathDefine PathDefine => new TablePathDefine("FlaskItemTable");

	public FlaskItemDataContainer()
	{
		base.Packer.RegisterAndBind(_dicByPotionItemIndex);
	}

	public override ItemType[] GetSubTypes()
	{
		return new ItemType[1] { ItemType.Flask };
	}

	protected override void OnEndLoadFromJsonData()
	{
		base.OnEndLoadFromJsonData();
		foreach (KeyValuePair<string, FlaskItemData> item in base.BaseDic.JsonLoad_Get())
		{
			int potionItemIndex = item.Value.PotionItemIndex;
			if (!_dicByPotionItemIndex.JsonLoad_Get().ContainsKey(potionItemIndex))
			{
				_dicByPotionItemIndex.JsonLoad_Get().Add(potionItemIndex, item.Value);
			}
		}
	}

	public FlaskItemData GetDataByPotionItemIndex(int potionItemIndex)
	{
		return _dicByPotionItemIndex.GetOrDefault(potionItemIndex);
	}
}
