using System.Collections.Generic;
using NShared.NJit;
using NShared.NJit.NCommon;

namespace NClientData.NLocalization;

public class SimplePacker : ILocalizePacker
{
	private JitPacker _packer = new JitPacker();

	private JitDictionary<string, string> _dic = new JitDictionary<string, string>();

	JitPacker ILocalizePacker.Packer => _packer;

	public SimplePacker()
	{
		_packer.RegisterAndBind(_dic);
	}

	public byte[] Serialize(Dictionary<string, string> mappingDic, List<string> csvList, string jitPath)
	{
		new SimplePacker();
		foreach (KeyValuePair<string, string> item in mappingDic)
		{
			_dic.JsonLoad_Get().Add(item.Key, item.Value);
		}
		byte[] mD5FromList = JsonSyncLoader2.GetMD5FromList(csvList);
		return _packer.SerializeBytes(jitPath, mD5FromList, null, 0);
	}

	bool ILocalizePacker.TryGet(string key, out string value)
	{
		return _dic.TryGetValue(key, out value);
	}

	void ILocalizePacker.Deserialize(ByteSpan readSpan)
	{
		_packer.Deserialize(readSpan);
	}
}
