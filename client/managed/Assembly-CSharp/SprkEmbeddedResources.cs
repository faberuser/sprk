using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

// Embedded resources of SPRK assemblies on every platform. Windows (Mono) serves them through
// Assembly.GetManifestResourceStream; on Android the assemblies are loaded from bytes by HybridCLR
// and il2cpp cannot find their resources, so they are read from the assembly image instead:
// the bytes the bootstrap registered (AppDomain data "SPRK_HOT_DLL:<name>") or the hot copy the
// updater installs (persistentDataPath/sprk/hot/<name>.dll.bytes).
public static class SprkEmbeddedResources
{
	public const string HotDllKey = "SPRK_HOT_DLL:";

	private static readonly Dictionary<string, byte[]> images = new Dictionary<string, byte[]>(StringComparer.Ordinal);

	// The resource's bytes, or null when the assembly has no such resource.
	public static byte[] Read(Assembly assembly, string name)
	{
		using (Stream stream = assembly.GetManifestResourceStream(name))
		{
			if (stream != null)
			{
				using MemoryStream memoryStream = new MemoryStream();
				stream.CopyTo(memoryStream);
				return memoryStream.ToArray();
			}
		}
		byte[] image = Image(assembly);
		if (image == null)
		{
			return null;
		}
		try
		{
			return SprkManagedResourceReader.Read(image, name);
		}
		catch (Exception ex)
		{
			Debug.LogError("[SPRK] Cannot read resource " + name + " from " + assembly.GetName().Name + ": " + ex.Message);
			return null;
		}
	}

	private static byte[] Image(Assembly assembly)
	{
		string name = assembly.GetName().Name;
		lock (images)
		{
			if (images.TryGetValue(name, out var value))
			{
				return value;
			}
			value = AppDomain.CurrentDomain.GetData(HotDllKey + name) as byte[];
			if (value == null)
			{
				try
				{
					string text = Path.Combine(Path.Combine(Application.persistentDataPath, "sprk"), Path.Combine("hot", name + ".dll.bytes"));
					if (File.Exists(text))
					{
						value = File.ReadAllBytes(text);
					}
					else if (!string.IsNullOrEmpty(assembly.Location) && File.Exists(assembly.Location))
					{
						value = File.ReadAllBytes(assembly.Location);
					}
				}
				catch (Exception ex)
				{
					Debug.LogError("[SPRK] Cannot read assembly image " + name + ": " + ex.Message);
				}
			}
			if (value == null)
			{
				Debug.LogError("[SPRK] Embedded resources of " + name + " are unavailable on this platform");
			}
			images[name] = value;
			return value;
		}
	}
}
