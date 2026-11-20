using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using LitJson;
using UnityEngine;

/// <summary>
/// The parsed tree of a recovered login scene (LoginPresentation/&lt;id&gt;/scene.json).
///
/// Parsing the largest scene as JSON takes about 3 s in Android's interpreted game code,
/// all of it black screen before the title. So:
/// - the scene is cached in a compact binary form (<see cref="Encode"/>) under
///   persistentDataPath, keyed by the scene file's size and write time (the patch
///   downloader rewrites changed files, which invalidates the copy). The game's own
///   JsonBin is not used: it reads empty arrays and objects back as no value;
/// - the selected scene's binary form is prepared on a background thread when the game
///   starts (<see cref="Preload"/>), overlapping content and scene loading.
///
/// Only bytes cross threads: the JsonData tree is made on the main thread right before
/// the scene is built, as before, so no JSON object outlives the frame that uses it.
/// </summary>
public static class SprkLoginSceneData
{
	private const int CacheMagic = 0x324C5053; // "SPL2"
	private const int HeaderSize = 20; // magic, scene size, scene write time

	private static readonly object gate = new object();
	private static string preloadPath;
	private static Thread preloadThread;
	private static byte[] preloaded;

	/// <summary>Starts preparing a scene in the background (main thread; once per run).</summary>
	public static void Preload(string path)
	{
		string cacheRoot = CacheRoot();
		lock (gate)
		{
			if (preloadPath != null)
			{
				return;
			}
			preloadPath = path;
		}
		var thread = new Thread(() =>
		{
			byte[] body = null;
			try
			{
				body = ReadBody(path, cacheRoot);
			}
			catch (Exception)
			{
				// Load reads the scene again on the main thread and reports the error.
			}
			lock (gate)
			{
				preloaded = body;
			}
		})
		{
			IsBackground = true,
			Name = "SPRK login scene"
		};
		lock (gate)
		{
			preloadThread = thread;
		}
		thread.Start();
	}

	/// <summary>The scene's tree, from the preloaded or cached binary form when there is one.</summary>
	public static JsonData Load(string path)
	{
		Thread thread = null;
		lock (gate)
		{
			if (preloadThread != null && string.Equals(preloadPath, path, StringComparison.OrdinalIgnoreCase))
			{
				thread = preloadThread;
				preloadThread = null;
			}
		}
		byte[] body = null;
		if (thread != null)
		{
			thread.Join();
			lock (gate)
			{
				body = preloaded;
				preloaded = null;
			}
		}
		if (body == null)
		{
			var info = new FileInfo(path);
			long size = info.Length, ticks = info.LastWriteTimeUtc.Ticks;
			string cacheRoot = CacheRoot();
			string cache = cacheRoot == null ? null : Path.Combine(cacheRoot, CacheName(path));
			body = cache == null ? null : ReadCache(cache, size, ticks);
			if (body == null)
			{
				// Parse now; the binary form is written in the background for the next start.
				JsonData parsed = Parse(path);
				if (cache != null)
				{
					byte[] fresh = Encode(parsed);
					new Thread(() => WriteCache(cache, size, ticks, fresh)) { IsBackground = true, Name = "SPRK login scene cache" }.Start();
				}
				return parsed;
			}
		}
		try
		{
			return Decode(body);
		}
		catch (Exception)
		{
			return Parse(path); // a damaged copy; the next start writes a new one
		}
	}

	// Application.persistentDataPath is main-thread only; resolved before any thread starts.
	private static string CacheRoot()
	{
		try
		{
			return Path.Combine(Application.persistentDataPath, "sprk", "login-scenes");
		}
		catch (Exception)
		{
			return null;
		}
	}

	private static JsonData Parse(string path)
	{
		JsonData data = JsonMapper.ToObject(File.ReadAllText(path));
		if (data == null)
		{
			throw new InvalidDataException(path + " is not valid JSON");
		}
		return data;
	}

	// Background thread: the binary form from the cache, or parsed and cached now. The
	// parsed tree is serialized at once and then dropped.
	private static byte[] ReadBody(string path, string cacheRoot)
	{
		var info = new FileInfo(path);
		long size = info.Length;
		long ticks = info.LastWriteTimeUtc.Ticks;
		string cache = cacheRoot == null ? null : Path.Combine(cacheRoot, CacheName(path));
		byte[] body = cache == null ? null : ReadCache(cache, size, ticks);
		if (body != null)
		{
			return body;
		}
		body = Encode(Parse(path));
		if (cache != null)
		{
			WriteCache(cache, size, ticks, body);
		}
		return body;
	}

	private static byte[] ReadCache(string cache, long size, long ticks)
	{
		try
		{
			if (!File.Exists(cache))
			{
				return null;
			}
			byte[] bytes = File.ReadAllBytes(cache);
			if (bytes.Length <= HeaderSize || BitConverter.ToInt32(bytes, 0) != CacheMagic
				|| BitConverter.ToInt64(bytes, 4) != size || BitConverter.ToInt64(bytes, 12) != ticks)
			{
				return null;
			}
			byte[] body = new byte[bytes.Length - HeaderSize];
			Buffer.BlockCopy(bytes, HeaderSize, body, 0, body.Length);
			return body;
		}
		catch (Exception)
		{
			return null; // a damaged copy is replaced by the next parse
		}
	}

	private static void WriteCache(string cache, long size, long ticks, byte[] body)
	{
		string temporary = cache + ".tmp";
		try
		{
			Directory.CreateDirectory(Path.GetDirectoryName(cache));
			using (var stream = new FileStream(temporary, FileMode.Create, FileAccess.Write))
			{
				stream.Write(BitConverter.GetBytes(CacheMagic), 0, 4);
				stream.Write(BitConverter.GetBytes(size), 0, 8);
				stream.Write(BitConverter.GetBytes(ticks), 0, 8);
				stream.Write(body, 0, body.Length);
			}
			if (File.Exists(cache))
			{
				File.Delete(cache);
			}
			File.Move(temporary, cache);
		}
		catch (Exception e)
		{
			try { File.Delete(temporary); } catch (Exception) { }
			Debug.LogWarning("SPRK login scene cache not written: " + e.Message);
		}
	}

	// Tags of the binary form: a string table, then the tree.
	private const byte TagNull = 0, TagNone = 1, TagObject = 2, TagArray = 3, TagString = 4,
		TagLong = 5, TagInt = 6, TagDouble = 7, TagTrue = 8, TagFalse = 9;

	/// <summary>The exact binary form of a parsed tree (types, order, empty containers, nulls).</summary>
	public static byte[] Encode(JsonData root)
	{
		var strings = new Dictionary<string, int>();
		var order = new List<string>();
		var body = new MemoryStream();
		var writer = new BinaryWriter(body, Encoding.UTF8);
		EncodeValue(writer, root, strings, order);
		writer.Flush();
		var output = new MemoryStream((int)body.Length + order.Count * 16 + 8);
		var head = new BinaryWriter(output, Encoding.UTF8);
		head.Write(order.Count);
		foreach (string text in order)
		{
			head.Write(text);
		}
		head.Flush();
		body.WriteTo(output);
		return output.ToArray();
	}

	private static int StringIndex(string text, Dictionary<string, int> strings, List<string> order)
	{
		if (!strings.TryGetValue(text, out int index))
		{
			index = order.Count;
			strings.Add(text, index);
			order.Add(text);
		}
		return index;
	}

	private static void EncodeValue(BinaryWriter writer, JsonData value, Dictionary<string, int> strings, List<string> order)
	{
		if (value == null)
		{
			writer.Write(TagNull);
			return;
		}
		switch (value.GetJsonType())
		{
		case JsonType.Object:
		{
			var fields = value.ToDictionary();
			writer.Write(TagObject);
			writer.Write(fields.Count);
			foreach (KeyValuePair<string, JsonData> field in fields)
			{
				writer.Write(StringIndex(field.Key, strings, order));
				EncodeValue(writer, field.Value, strings, order);
			}
			break;
		}
		case JsonType.Array:
		{
			int count = value.Count;
			writer.Write(TagArray);
			writer.Write(count);
			for (int i = 0; i < count; i++)
			{
				EncodeValue(writer, value.GetFromArray(i), strings, order);
			}
			break;
		}
		case JsonType.String:
			writer.Write(TagString);
			writer.Write(StringIndex(value.valueStr ?? string.Empty, strings, order));
			break;
		case JsonType.Long:
		{
			long number = value.valueLong;
			if (number >= int.MinValue && number <= int.MaxValue)
			{
				writer.Write(TagInt);
				writer.Write((int)number);
			}
			else
			{
				writer.Write(TagLong);
				writer.Write(number);
			}
			break;
		}
		case JsonType.Double:
			writer.Write(TagDouble);
			writer.Write(value.valueDouble);
			break;
		case JsonType.Boolean:
			writer.Write(value.valueBool ? TagTrue : TagFalse);
			break;
		default:
			writer.Write(TagNone);
			break;
		}
	}

	/// <summary>Builds the tree as JsonMapper.ToObject does (new JsonData nodes, nulls as null).</summary>
	public static JsonData Decode(byte[] bytes)
	{
		var reader = new BinaryReader(new MemoryStream(bytes, false), Encoding.UTF8);
		var strings = new string[reader.ReadInt32()];
		for (int i = 0; i < strings.Length; i++)
		{
			strings[i] = reader.ReadString();
		}
		int position = (int)reader.BaseStream.Position;
		return DecodeValue(bytes, ref position, strings);
	}

	private static int ReadInt(byte[] b, ref int p)
	{
		int value = b[p] | (b[p + 1] << 8) | (b[p + 2] << 16) | (b[p + 3] << 24);
		p += 4;
		return value;
	}

	private static long ReadLong(byte[] b, ref int p)
	{
		long low = (uint)ReadInt(b, ref p);
		long high = (uint)ReadInt(b, ref p);
		return low | (high << 32);
	}

	private static JsonData DecodeValue(byte[] b, ref int p, string[] strings)
	{
		byte tag = b[p++];
		if (tag == TagNull)
		{
			return null;
		}
		var data = new JsonData();
		switch (tag)
		{
		case TagObject:
		{
			data.SetJsonType(JsonType.Object);
			int count = ReadInt(b, ref p);
			for (int i = 0; i < count; i++)
			{
				string key = strings[ReadInt(b, ref p)];
				data.AddField(key, DecodeValue(b, ref p, strings));
			}
			break;
		}
		case TagArray:
		{
			data.SetJsonType(JsonType.Array);
			int count = ReadInt(b, ref p);
			for (int i = 0; i < count; i++)
			{
				data.Add(DecodeValue(b, ref p, strings));
			}
			break;
		}
		case TagString:
			data.SetString(strings[ReadInt(b, ref p)]);
			break;
		case TagInt:
			data.SetLong(ReadInt(b, ref p));
			break;
		case TagLong:
			data.SetLong(ReadLong(b, ref p));
			break;
		case TagDouble:
			data.SetDouble(BitConverter.Int64BitsToDouble(ReadLong(b, ref p)));
			break;
		case TagTrue:
			data.SetBoolean(true);
			break;
		case TagFalse:
			data.SetBoolean(false);
			break;
		case TagNone:
			break;
		default:
			throw new InvalidDataException("Login scene cache: unknown tag " + tag);
		}
		return data;
	}

	// One copy per scene file: the folder name plus a stable hash of the full path, so a
	// test copy and the real game do not keep replacing each other's copy.
	private static string CacheName(string path)
	{
		uint hash = 2166136261;
		foreach (byte b in Encoding.UTF8.GetBytes(Path.GetFullPath(path).ToLowerInvariant()))
		{
			hash = (hash ^ b) * 16777619;
		}
		return Path.GetFileName(Path.GetDirectoryName(path)) + "-" + hash.ToString("x8") + ".bin";
	}
}
