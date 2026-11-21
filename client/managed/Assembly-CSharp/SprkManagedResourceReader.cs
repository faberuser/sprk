using System;
using System.Collections.Generic;
using System.Text;

// Reads embedded (manifest) resources straight from a .NET assembly image (ECMA-335 II.24-25).
// On Android the game's assemblies are loaded from bytes by HybridCLR, and il2cpp looks for
// their resources in Data/Resources/<name>-resources.dat, which does not exist for them, so
// Assembly.GetManifestResourceStream returns null there (SprkEmbeddedResources). No Unity types here.
public static class SprkManagedResourceReader
{
	// Table schemas up to ManifestResource (0x28). Column codes: 2/4 = fixed bytes, 's' string heap,
	// 'g' guid heap, 'b' blob heap, 'T<table>' simple index, 'C<coded>' coded index.
	private static readonly string[][] tables = new string[41][]
	{
		new string[5] { "2", "s", "g", "g", "g" },
		new string[3] { "CResolutionScope", "s", "s" },
		new string[6] { "4", "s", "s", "CTypeDefOrRef", "T4", "T6" },
		new string[1] { "T4" },
		new string[3] { "2", "s", "b" },
		new string[1] { "T6" },
		new string[6] { "4", "2", "2", "s", "b", "T8" },
		new string[1] { "T8" },
		new string[3] { "2", "2", "s" },
		new string[2] { "T2", "CTypeDefOrRef" },
		new string[3] { "CMemberRefParent", "s", "b" },
		new string[3] { "2", "CHasConstant", "b" },
		new string[3] { "CHasCustomAttribute", "CCustomAttributeType", "b" },
		new string[2] { "CHasFieldMarshal", "b" },
		new string[3] { "2", "CHasDeclSecurity", "b" },
		new string[3] { "2", "4", "T2" },
		new string[2] { "4", "T4" },
		new string[1] { "b" },
		new string[2] { "T2", "T20" },
		new string[1] { "T20" },
		new string[3] { "2", "s", "CTypeDefOrRef" },
		new string[2] { "T2", "T23" },
		new string[1] { "T23" },
		new string[3] { "2", "s", "b" },
		new string[3] { "2", "T6", "CHasSemantics" },
		new string[3] { "T2", "CMethodDefOrRef", "CMethodDefOrRef" },
		new string[1] { "s" },
		new string[1] { "b" },
		new string[4] { "2", "CMemberForwarded", "s", "T26" },
		new string[2] { "4", "T4" },
		new string[2] { "4", "4" },
		new string[1] { "4" },
		new string[9] { "4", "2", "2", "2", "2", "4", "b", "s", "s" },
		new string[1] { "4" },
		new string[3] { "4", "4", "4" },
		new string[9] { "2", "2", "2", "2", "4", "b", "s", "s", "b" },
		new string[2] { "4", "T35" },
		new string[4] { "4", "4", "4", "T35" },
		new string[3] { "4", "s", "b" },
		new string[5] { "4", "4", "s", "s", "CImplementation" },
		new string[4] { "4", "4", "s", "CImplementation" }
	};

	private static readonly Dictionary<string, int[]> coded = new Dictionary<string, int[]>
	{
		{ "TypeDefOrRef", new int[3] { 2, 1, 27 } },
		{ "HasConstant", new int[3] { 4, 8, 23 } },
		{ "HasCustomAttribute", new int[22] { 6, 4, 1, 2, 8, 9, 10, 0, 14, 23, 20, 17, 26, 27, 32, 35, 38, 39, 40, 42, 44, 43 } },
		{ "HasFieldMarshal", new int[2] { 4, 8 } },
		{ "HasDeclSecurity", new int[3] { 2, 6, 32 } },
		{ "MemberRefParent", new int[5] { 2, 1, 26, 6, 27 } },
		{ "HasSemantics", new int[2] { 20, 23 } },
		{ "MethodDefOrRef", new int[2] { 6, 10 } },
		{ "MemberForwarded", new int[2] { 4, 6 } },
		{ "Implementation", new int[3] { 38, 35, 39 } },
		{ "CustomAttributeType", new int[5] { -1, -1, 6, 10, -1 } },
		{ "ResolutionScope", new int[4] { 0, 26, 35, 1 } }
	};

	// Resource name -> (offset of its bytes in the image, length).
	public static Dictionary<string, KeyValuePair<int, int>> Index(byte[] image)
	{
		Dictionary<string, KeyValuePair<int, int>> result = new Dictionary<string, KeyValuePair<int, int>>(StringComparer.Ordinal);
		int pe = I32(image, 60);
		if (I32(image, pe) != 0x4550)
		{
			throw new FormatException("not a PE image");
		}
		int coff = pe + 4;
		int sections = U16(image, coff + 2);
		int optional = coff + 20;
		int optionalSize = U16(image, coff + 16);
		int directories = optional + ((U16(image, optional) == 0x20B) ? 112 : 96);
		int cliRva = I32(image, directories + 14 * 8);
		int sectionTable = optional + optionalSize;
		Func<int, int> offset = delegate(int rva)
		{
			for (int i = 0; i < sections; i++)
			{
				int s = sectionTable + i * 40;
				int va = I32(image, s + 12);
				int size = Math.Max(I32(image, s + 8), I32(image, s + 16));
				if (rva >= va && rva < va + size)
				{
					return rva - va + I32(image, s + 20);
				}
			}
			throw new FormatException("RVA outside sections");
		};
		int cli = offset(cliRva);
		int metadata = offset(I32(image, cli + 8));
		int resourcesRva = I32(image, cli + 24);
		if (I32(image, metadata) != 0x424A5342)
		{
			throw new FormatException("no metadata");
		}
		int p = metadata + 16 + I32(image, metadata + 12);
		int streamCount = U16(image, p + 2);
		p += 4;
		int tablesAt = -1;
		int stringsAt = -1;
		for (int i = 0; i < streamCount; i++)
		{
			int streamOffset = I32(image, p);
			p += 8;
			int nameStart = p;
			while (image[p] != 0)
			{
				p++;
			}
			string name = Encoding.ASCII.GetString(image, nameStart, p - nameStart);
			p = nameStart + ((p - nameStart + 4) & ~3);
			if (name == "#~" || name == "#-")
			{
				tablesAt = metadata + streamOffset;
			}
			else if (name == "#Strings")
			{
				stringsAt = metadata + streamOffset;
			}
		}
		if (tablesAt < 0 || stringsAt < 0)
		{
			throw new FormatException("no tables");
		}
		int heapSizes = image[tablesAt + 6];
		long valid = BitConverter.ToInt64(image, tablesAt + 8);
		int[] rows = new int[64];
		int q = tablesAt + 24;
		for (int t = 0; t < 64; t++)
		{
			if ((valid & (1L << t)) != 0)
			{
				rows[t] = I32(image, q);
				q += 4;
			}
		}
		if ((heapSizes & 0x40) != 0)
		{
			q += 4;
		}
		if (rows[40] == 0 || resourcesRva == 0)
		{
			return result;
		}
		int stringSize = ((heapSizes & 1) != 0) ? 4 : 2;
		int guidSize = ((heapSizes & 2) != 0) ? 4 : 2;
		int blobSize = ((heapSizes & 4) != 0) ? 4 : 2;
		Func<string, int> width = delegate(string column)
		{
			switch (column[0])
			{
			case '2':
				return 2;
			case '4':
				return 4;
			case 's':
				return stringSize;
			case 'g':
				return guidSize;
			case 'b':
				return blobSize;
			case 'T':
				return (rows[int.Parse(column.Substring(1))] < 65536) ? 2 : 4;
			default:
			{
				int[] targets = coded[column.Substring(1)];
				int bits = 0;
				while ((1 << bits) < targets.Length)
				{
					bits++;
				}
				int max = 0;
				foreach (int target in targets)
				{
					if (target >= 0)
					{
						max = Math.Max(max, rows[target]);
					}
				}
				return (max < (1 << (16 - bits))) ? 2 : 4;
			}
			}
		};
		int resources = offset(resourcesRva);
		for (int t = 0; t <= 40; t++)
		{
			if (rows[t] == 0)
			{
				continue;
			}
			string[] columns = tables[t];
			int rowSize = 0;
			foreach (string column in columns)
			{
				rowSize += width(column);
			}
			if (t < 40)
			{
				q += rowSize * rows[t];
				continue;
			}
			for (int r = 0; r < rows[t]; r++)
			{
				int row = q + r * rowSize;
				int dataOffset = I32(image, row);
				int nameIndex = (stringSize == 4) ? I32(image, row + 8) : U16(image, row + 8);
				int implementation = (width(columns[3]) == 4) ? I32(image, row + 8 + stringSize) : U16(image, row + 8 + stringSize);
				if (implementation != 0)
				{
					continue;
				}
				int nameAt = stringsAt + nameIndex;
				int end = nameAt;
				while (image[end] != 0)
				{
					end++;
				}
				string name = Encoding.UTF8.GetString(image, nameAt, end - nameAt);
				int at = resources + dataOffset;
				result[name] = new KeyValuePair<int, int>(at + 4, I32(image, at));
			}
		}
		return result;
	}

	public static byte[] Read(byte[] image, string name)
	{
		if (!Index(image).TryGetValue(name, out var value))
		{
			return null;
		}
		byte[] array = new byte[value.Value];
		Buffer.BlockCopy(image, value.Key, array, 0, value.Value);
		return array;
	}

	private static int I32(byte[] b, int i)
	{
		return BitConverter.ToInt32(b, i);
	}

	private static int U16(byte[] b, int i)
	{
		return BitConverter.ToUInt16(b, i);
	}
}
