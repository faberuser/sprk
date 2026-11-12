using System.Collections.Generic;
using NShared.NJit;
using NShared.NJit.NCommon;

namespace NClientData.NLocalization;

public class HashedPacker : ILocalizePacker
{
	private JitPacker _packer = new JitPacker();

	private JitDictionary<int, string> _dic = new JitDictionary<int, string>();

	JitPacker ILocalizePacker.Packer => _packer;

	public HashedPacker()
	{
		_packer.RegisterAndBind(_dic);
	}

	public byte[] Serialize(Dictionary<string, string> mappingDic, List<string> csvList, string jitPath)
	{
		new SimplePacker();
		foreach (KeyValuePair<string, string> item in mappingDic)
		{
			int key = JitOption.StringToHashCode(item.Key);
			if (_dic.JsonLoad_Get().ContainsKey(key))
			{
				return null;
			}
			_dic.JsonLoad_Get().Add(key, item.Value);
		}
		byte[] mD5FromList = JsonSyncLoader2.GetMD5FromList(csvList);
		return _packer.SerializeBytes(jitPath, mD5FromList, null, 0);
	}

	bool ILocalizePacker.TryGet(string key, out string value)
	{
		int key2 = JitOption.StringToHashCode(key);
		return _dic.TryGetValue(key2, out value);
	}

	void ILocalizePacker.Deserialize(ByteSpan readSpan)
	{
		_packer.Deserialize(readSpan);
	}
}
