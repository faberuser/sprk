using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NShared;
using NShared.NUtil;

namespace NPatchGenerator;

public class PatchGenerator
{
	public delegate void AssetRootIterateCallback(List<KeyValuePair<string, PatchAssetRootData>> dictionary, Action<KeyValuePair<string, PatchAssetRootData>> onIterate);

	public delegate void AssetFileIterateCallback(IList<string> list, Action<string> onIterate);

	private static readonly object _assetInfoListSync = new object();

	public AssetRootIterateCallback AssetRootIterator;

	public AssetFileIterateCallback AssetFilesIterator;

	public static Aes128 Aes = new Aes128();

	private string _archivePrefix = string.Empty;

	private int _minArchiveSize = 4000000;

	private bool _incremental;

	private string _outputPrefix = string.Empty;

	private string _externPath = string.Empty;

	private string _inputDirectory = string.Empty;

	private string _outputDirectory = string.Empty;

	private bool _option_BundlePack_Create;

	private readonly List<KeyValuePair<string, PatchAssetRootData>> _assetRootList = new List<KeyValuePair<string, PatchAssetRootData>>();

	private readonly HashSet<string> _exceptHeroTemplatePatterns = new HashSet<string>();

	private readonly HashSet<Regex> _exceptHeroPatterns = new HashSet<Regex>();

	private readonly HashSet<Regex> _exceptPatterns = new HashSet<Regex>();

	private readonly HashSet<Regex> _includePatterns = new HashSet<Regex>();

	private static readonly string[] _originalTextExtensions = new string[3] { ".csv", ".json", ".txt" };

	private static readonly string[] _convertedTextExtensions = new string[3] { ".vs", ".vo", ".vx" };

	public static string GetTest(bool first)
	{
		if (first)
		{
			return "rta5TAu4GPff";
		}
		return "XFR4S2bCjg==";
	}

	public static string GetPatchInfoFileName(bool isBinary = false)
	{
		if (isBinary)
		{
			return "patch.bjs";
		}
		return "patch.json";
	}

	public static string[] GetOriginalTextExtensions()
	{
		return _originalTextExtensions;
	}

	public static string[] GetConvertedTextExtensions()
	{
		return _convertedTextExtensions;
	}

	public static int GetOriginalTextExtensionIndex(string file)
	{
		string[] originalTextExtensions = GetOriginalTextExtensions();
		GetConvertedTextExtensions();
		for (int i = 0; i < originalTextExtensions.Length; i++)
		{
			if (file.EndsWith(originalTextExtensions[i]))
			{
				return i;
			}
		}
		return -1;
	}

	public static string GetMessagePackExtension()
	{
		return ".mpb";
	}

	public static string GetBinaryJsonExtension(bool encrypt)
	{
		if (!encrypt)
		{
			return ".bjs";
		}
		return ".bvo";
	}
}
