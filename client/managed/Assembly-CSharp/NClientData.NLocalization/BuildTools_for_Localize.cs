using System.Collections.Generic;
using System.IO;
using NShared;
using NShared.NJit;
using NShared.NJit.NCommon;
using NShared.NLocalization;

namespace NClientData.NLocalization;

public static class BuildTools_for_Localize
{
	public enum EPackerType
	{
		Simple,
		Hashed
	}

	public static ILocalizePacker Deserialize(byte[] bytes)
	{
		ByteSpan nextSpan = new ByteSpan(bytes);
		ILocalizePacker localizePacker;
		switch ((EPackerType)nextSpan.ReadByMsgPackT<int>(0, out nextSpan))
		{
		case EPackerType.Simple:
			localizePacker = new SimplePacker();
			break;
		case EPackerType.Hashed:
			localizePacker = new HashedPacker();
			break;
		default:
			return null;
		}
		localizePacker.Deserialize(nextSpan);
		return localizePacker;
	}

	public static bool ValidatePacker(ILocalizePacker localizePacker, List<string> csvList)
	{
		if (localizePacker == null)
		{
			return false;
		}
		byte[] mD5FromList = JsonSyncLoader2.GetMD5FromList(csvList);
		if (!localizePacker.Packer.CheckMD5(mD5FromList, isTable: true))
		{
			return false;
		}
		return true;
	}

	public static ILocalizePacker Serialize(Dictionary<string, string> mappingDic, List<string> csvList, string jitPath)
	{
		HashedPacker packer;
		byte[] array = SerializeHashedPacker(mappingDic, csvList, jitPath, out packer);
		ILocalizePacker result = packer;
		if (array == null)
		{
			array = SerializeSimplePacker(mappingDic, csvList, jitPath, out var packer2);
			result = packer2;
		}
		string directoryName = Path.GetDirectoryName(jitPath);
		if (!Directory.Exists(directoryName))
		{
			Directory.CreateDirectory(directoryName);
		}
		File.WriteAllBytes(jitPath, array);
		return result;
	}

	private static byte[] SerializeHashedPacker(Dictionary<string, string> mappingDic, List<string> csvList, string jitPath, out HashedPacker packer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		packer = new HashedPacker();
		byte[] array = packer.Serialize(mappingDic, csvList, jitPath);
		if (array == null)
		{
			packer = null;
			return null;
		}
		simpleBufferWriter.WriteByMsgPack(1);
		simpleBufferWriter.WriteBytes(array);
		return simpleBufferWriter.ToArray();
	}

	private static byte[] SerializeSimplePacker(Dictionary<string, string> mappingDic, List<string> csvList, string jitPath, out SimplePacker packer)
	{
		using SimpleBufferWriter simpleBufferWriter = SimpleBufferWriter.CreateWriter();
		packer = new SimplePacker();
		byte[] bytes = packer.Serialize(mappingDic, csvList, jitPath);
		simpleBufferWriter.WriteByMsgPack(0);
		simpleBufferWriter.WriteBytes(bytes);
		return simpleBufferWriter.ToArray();
	}

	public static void RebuildAllJit()
	{
		JitOption.AppendLogToFile(string.Empty, "\n\n");
		JitOption.JitLog("[[ -- Localization RebuildAllJit starting...... -- ]]");
		using (new ForceRebuildJit())
		{
			foreach (LanguageType allLanguage in GameRoot.Instance.LocalizationValueDataContainer.GetAllLanguages())
			{
				LocalizeLoadDefine loadDefine = LocalizeLoadDefine.CreateLoadDefine_from_Basic(allLanguage);
				new JitLocalizeContainer(allLanguage, loadDefine).Load();
				LocalizeLoadDefine loadDefine2 = LocalizeLoadDefine.CreateLoadDefine_from_Patch(allLanguage);
				new JitLocalizeContainer(allLanguage, loadDefine2).Load();
			}
			new JitEnumRedirector(LocalizeLoadDefine.CreateLoadDefine_from_Patch("Localization_EnumRedirection.jit")).Load();
		}
	}

	public static List<LocalizeLoadDefine> GetLoadDefineList_from_Basic()
	{
		List<LocalizeLoadDefine> list = new List<LocalizeLoadDefine>();
		foreach (LanguageType allLanguage in GameRoot.Instance.LocalizationValueDataContainer.GetAllLanguages())
		{
			LocalizeLoadDefine item = LocalizeLoadDefine.CreateLoadDefine_from_Basic(allLanguage);
			list.Add(item);
		}
		return list;
	}
}
