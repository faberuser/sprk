using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Microsoft.Win32.SafeHandles;

public static class SprkIllustrationUpscaleCore
{
	private struct JobLimits
	{
		public long ProcessTime;

		public long JobTime;

		public uint Flags;

		public UIntPtr MinWorkingSet;

		public UIntPtr MaxWorkingSet;

		public uint ActiveProcesses;

		public UIntPtr Affinity;

		public uint Priority;

		public uint Scheduling;

		public ulong ReadOperations;

		public ulong WriteOperations;

		public ulong OtherOperations;

		public ulong ReadBytes;

		public ulong WriteBytes;

		public ulong OtherBytes;

		public UIntPtr ProcessMemory;

		public UIntPtr JobMemory;

		public UIntPtr PeakProcessMemory;

		public UIntPtr PeakJobMemory;
	}

	public const int MaxInput = 1024;

	public const int MaxPngBytes = 83886080;

	public const long DiskBudget = 1073741824L;

	public const string Interface = "sprk-ncnn-rrdb-rgb-x4-v1";

	public const string DefaultHash = "f18ef1a86726f26288739b107f2bb6d4b815d04bc14f6bb7b3d0dacbc1a1fdba";

	public const int DefaultSize = 33564831;

	public const string DefaultUrl = "https://updates.krinfo.net/updates/models/remacri-4x-v1/model.sprkmodel";

	public const string LegacyDefaultHash = "3aae12f7f04f602e301d82f84e7fa507a09827d99935e045c39d1d5ecccc7d67";

	private static readonly SemaphoreSlim Gate = new SemaphoreSlim(1, 1);

	private static readonly uint[] CrcTable = MakeCrcTable();

	public static string ModelPath(string storage)
	{
		string text = Path.Combine(storage, "active.sprkmodel");
		if (File.Exists(text) && new FileInfo(text).Length == 8973806 && Hash(File.ReadAllBytes(text)) == "3aae12f7f04f602e301d82f84e7fa507a09827d99935e045c39d1d5ecccc7d67")
		{
			return Path.Combine(storage, "default-remacri-4x-v1.sprkmodel");
		}
		return text;
	}

	public static string ModelUrlFromManifest(string manifest)
	{
		if (!Uri.TryCreate(manifest, UriKind.Absolute, out var result) || (!(result.Scheme == "https") && (!(result.Scheme == "http") || !result.IsLoopback)) || !string.IsNullOrEmpty(result.UserInfo) || !string.IsNullOrEmpty(result.Fragment))
		{
			throw new InvalidDataException("Invalid server update URL for illustration models");
		}
		return new Uri(result, "../models/remacri-4x-v1/model.sprkmodel").AbsoluteUri;
	}

	public static string ModelUrlFromProfile(SprkIllustrationLauncherConfig config, string activeHost)
	{
		SprkIllustrationLauncherProfile[] array = ((config == null || config.Profiles == null) ? new SprkIllustrationLauncherProfile[0] : config.Profiles);
		SprkIllustrationLauncherProfile sprkIllustrationLauncherProfile = null;
		if (!string.IsNullOrEmpty(activeHost))
		{
			if (!Uri.TryCreate(activeHost, UriKind.Absolute, out var host))
			{
				throw new InvalidDataException("Invalid active server profile");
			}
			Uri result;
			SprkIllustrationLauncherProfile[] array2 = array.Where((SprkIllustrationLauncherProfile p) => p != null && Uri.TryCreate(p.HostUrl, UriKind.Absolute, out result) && result.Equals(host)).ToArray();
			sprkIllustrationLauncherProfile = array2.FirstOrDefault((SprkIllustrationLauncherProfile p) => p.Id == config.SelectedProfile);
			if (sprkIllustrationLauncherProfile == null && array2.Length == 1)
			{
				sprkIllustrationLauncherProfile = array2[0];
			}
			if (sprkIllustrationLauncherProfile == null)
			{
				throw new InvalidDataException("No update profile matches the active server; launch through the configured launcher");
			}
		}
		else if (array.Length != 0)
		{
			sprkIllustrationLauncherProfile = array.FirstOrDefault((SprkIllustrationLauncherProfile p) => p != null && p.Id == config.SelectedProfile);
			if (sprkIllustrationLauncherProfile == null)
			{
				throw new InvalidDataException("The selected server update profile is missing");
			}
		}
		if (sprkIllustrationLauncherProfile != null)
		{
			return ModelUrlFromManifest(sprkIllustrationLauncherProfile.ManifestUrl);
		}
		if (config != null && !string.IsNullOrEmpty(config.ManifestUrl))
		{
			return ModelUrlFromManifest(config.ManifestUrl);
		}
		return "https://updates.krinfo.net/updates/models/remacri-4x-v1/model.sprkmodel";
	}

	public static string Hash(byte[] data)
	{
		using SHA256 sHA = SHA256.Create();
		return BitConverter.ToString(sHA.ComputeHash(data)).Replace("-", "").ToLowerInvariant();
	}

	public static string CacheKey(byte[] source, string model, int factor)
	{
		if (factor != 4)
		{
			throw new InvalidDataException("Only native x4 enhancement is supported");
		}
		return Hash(Encoding.UTF8.GetBytes("crop-srgb-alpha-tile64-v2|sprk-ncnn-rrdb-rgb-x4-v1|" + Hash(source) + "|" + model + "|4"));
	}

	public static bool Nonempty(string path)
	{
		if (File.Exists(path))
		{
			return new FileInfo(path).Length > 0;
		}
		return false;
	}

	private static void Delete(string path)
	{
		try
		{
			if (File.Exists(path))
			{
				File.Delete(path);
			}
		}
		catch (IOException)
		{
		}
		catch (UnauthorizedAccessException)
		{
		}
	}

	[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	private static extern SafeFileHandle CreateFile(string name, uint access, uint share, IntPtr security, uint disposition, uint flags, IntPtr template);

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern bool GetFileSizeEx(SafeFileHandle file, out long size);

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern bool SetFileInformationByHandle(SafeFileHandle file, int infoClass, ref int delete, int size);

	[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	private static extern SafeFileHandle CreateJobObject(IntPtr security, string name);

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern bool SetInformationJobObject(SafeFileHandle job, int infoClass, ref JobLimits limits, uint length);

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern bool AssignProcessToJobObject(SafeFileHandle job, IntPtr process);

	private static SafeFileHandle BoundProcess(Process process)
	{
		SafeFileHandle safeFileHandle = CreateJobObject(IntPtr.Zero, null);
		try
		{
			JobLimits limits = new JobLimits
			{
				Flags = 8448u,
				ProcessMemory = new UIntPtr(2147483648uL)
			};
			if (safeFileHandle.IsInvalid || !SetInformationJobObject(safeFileHandle, 9, ref limits, (uint)Marshal.SizeOf(typeof(JobLimits))) || !AssignProcessToJobObject(safeFileHandle, process.Handle))
			{
				throw new IOException("Could not apply the inference memory limit");
			}
			return safeFileHandle;
		}
		catch
		{
			safeFileHandle.Dispose();
			try
			{
				process.Kill();
				process.WaitForExit();
			}
			catch (InvalidOperationException)
			{
			}
			throw;
		}
	}

	public static void InstallAbsent(string temp, string destination)
	{
		if (Nonempty(destination))
		{
			return;
		}
		if (File.Exists(destination))
		{
			using SafeFileHandle safeFileHandle = CreateFile(destination, 2147549184u, 0u, IntPtr.Zero, 3u, 0u, IntPtr.Zero);
			if (safeFileHandle.IsInvalid || !GetFileSizeEx(safeFileHandle, out var size))
			{
				throw new IOException("Local model is busy; retry selection");
			}
			if (size != 0L)
			{
				return;
			}
			int delete = 1;
			if (!SetFileInformationByHandle(safeFileHandle, 4, ref delete, 4))
			{
				throw new IOException("Empty model could not be removed");
			}
		}
		try
		{
			File.Move(temp, destination);
		}
		catch (IOException)
		{
			if (!Nonempty(destination))
			{
				throw;
			}
		}
	}

	public static void Provision(string active, CancellationToken token, string sourceUrl = null, SprkIllustrationProgress progress = null)
	{
		if (Nonempty(active))
		{
			return;
		}
		Directory.CreateDirectory(Path.GetDirectoryName(active));
		string text = active + "." + Guid.NewGuid().ToString("N") + ".tmp";
		try
		{
			progress?.Report("Downloading model", 0f);
			HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(sourceUrl ?? "https://updates.krinfo.net/updates/models/remacri-4x-v1/model.sprkmodel");
			httpWebRequest.Timeout = 30000;
			httpWebRequest.ReadWriteTimeout = 30000;
			httpWebRequest.AllowAutoRedirect = false;
			using (token.Register(httpWebRequest.Abort))
			{
				using WebResponse webResponse = httpWebRequest.GetResponse();
				using Stream stream = webResponse.GetResponseStream();
				using FileStream fileStream = new FileStream(text, FileMode.CreateNew, FileAccess.Write, FileShare.None);
				byte[] array = new byte[65536];
				int num = 0;
				int num2;
				while ((num2 = stream.Read(array, 0, array.Length)) > 0)
				{
					token.ThrowIfCancellationRequested();
					num += num2;
					if (num > 33564831)
					{
						throw new InvalidDataException("Default model exceeds verified size");
					}
					fileStream.Write(array, 0, num2);
					progress?.Report("Downloading model", (float)num / 33564830f);
				}
				if (num != 33564831)
				{
					throw new InvalidDataException("Incomplete default model");
				}
				fileStream.Flush(flushToDisk: true);
			}
			if (Hash(File.ReadAllBytes(text)) != "f18ef1a86726f26288739b107f2bb6d4b815d04bc14f6bb7b3d0dacbc1a1fdba")
			{
				throw new InvalidDataException("Default model integrity check failed");
			}
			token.ThrowIfCancellationRequested();
			InstallAbsent(text, active);
		}
		finally
		{
			Delete(text);
		}
	}

	public static byte[] ReadModel(string path)
	{
		using FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
		if (fileStream.Length < 16 || fileStream.Length > 68157440)
		{
			throw new InvalidDataException("Incompatible local model; original retained");
		}
		byte[] array = new byte[(int)fileStream.Length];
		int i;
		int num;
		for (i = 0; i < array.Length; i += num)
		{
			if ((num = fileStream.Read(array, i, array.Length - i)) <= 0)
			{
				break;
			}
		}
		if (i != array.Length)
		{
			throw new InvalidDataException("Local model changed while loading");
		}
		ValidateModel(array);
		return array;
	}

	public static void ValidateModel(byte[] data)
	{
		if (data.Length < 16 || Encoding.ASCII.GetString(data, 0, 8) != "SPRKSR01")
		{
			throw new InvalidDataException("Local model must use sprk-ncnn-rrdb-rgb-x4-v1");
		}
		int num = BitConverter.ToInt32(data, 8);
		int num2 = BitConverter.ToInt32(data, 12);
		if (num < 1 || num > 524288 || num2 < 1 || num2 > 67108864 || 16L + (long)num + num2 != data.Length)
		{
			throw new InvalidDataException("Invalid model lengths");
		}
		string text = Encoding.ASCII.GetString(data, 16, num);
		if (!text.StartsWith("7767517") || !text.Contains("data") || !text.Contains("output"))
		{
			throw new InvalidDataException("Incompatible NCNN tensor names");
		}
	}

	private static void SplitModel(byte[] data, string directory)
	{
		ValidateModel(data);
		Directory.CreateDirectory(directory);
		int num = BitConverter.ToInt32(data, 8);
		int count = BitConverter.ToInt32(data, 12);
		using (FileStream fileStream = File.Create(Path.Combine(directory, "realesrgan-x4plus-anime.param")))
		{
			fileStream.Write(data, 16, num);
		}
		using FileStream fileStream2 = File.Create(Path.Combine(directory, "realesrgan-x4plus-anime.bin"));
		fileStream2.Write(data, 16 + num, count);
	}

	private static int BigEndian(byte[] data, int offset)
	{
		return (data[offset] << 24) | (data[offset + 1] << 16) | (data[offset + 2] << 8) | data[offset + 3];
	}

	private static uint[] MakeCrcTable()
	{
		uint[] array = new uint[256];
		for (uint num = 0u; num < 256; num++)
		{
			uint num2 = num;
			for (int i = 0; i < 8; i++)
			{
				num2 = (((num2 & 1) != 0) ? (0xEDB88320u ^ (num2 >> 1)) : (num2 >> 1));
			}
			array[num] = num2;
		}
		return array;
	}

	public static void ValidatePng(byte[] data, int width, int height)
	{
		byte[] second = new byte[8] { 137, 80, 78, 71, 13, 10, 26, 10 };
		if (data.Length < 33 || data.Length > 83886080 || !data.Take(8).SequenceEqual(second) || Encoding.ASCII.GetString(data, 12, 4) != "IHDR" || BigEndian(data, 16) != width || BigEndian(data, 20) != height || data[24] != 8 || (data[25] != 6 && data[25] != 2))
		{
			throw new InvalidDataException("Invalid enhanced image");
		}
		int num = 8;
		bool flag = false;
		while (num < data.Length)
		{
			if (num > data.Length - 12)
			{
				throw new InvalidDataException("Truncated PNG chunk");
			}
			int num2 = BigEndian(data, num);
			if (num2 < 0 || num2 > data.Length - num - 12)
			{
				throw new InvalidDataException("Invalid PNG chunk length");
			}
			uint num3 = uint.MaxValue;
			for (int i = num + 4; i < num + 8 + num2; i++)
			{
				num3 = CrcTable[(num3 ^ data[i]) & 0xFF] ^ (num3 >> 8);
			}
			if ((num3 ^ 0xFFFFFFFFu) != (uint)BigEndian(data, num + 8 + num2))
			{
				throw new InvalidDataException("Corrupt cached PNG");
			}
			flag = Encoding.ASCII.GetString(data, num + 4, 4) == "IEND";
			num += 12 + num2;
			if (flag)
			{
				break;
			}
		}
		if (!flag || num != data.Length)
		{
			throw new InvalidDataException("Incomplete PNG");
		}
	}

	public static void Prune(string directory, long budget)
	{
		FileInfo[] array = (from f in new DirectoryInfo(directory).GetFiles("*.png")
			orderby f.LastWriteTimeUtc
			select f).ToArray();
		long num = array.Sum((FileInfo f) => f.Length);
		FileInfo[] array2 = array;
		foreach (FileInfo fileInfo in array2)
		{
			if (num > budget)
			{
				long length = fileInfo.Length;
				Delete(fileInfo.FullName);
				fileInfo.Refresh();
				if (!fileInfo.Exists)
				{
					num -= length;
				}
				continue;
			}
			break;
		}
	}

	private static string Quote(string value)
	{
		if (value.IndexOf('"') >= 0 || value.EndsWith("\\"))
		{
			throw new InvalidDataException("Invalid inference path");
		}
		return "\"" + value + "\"";
	}

	public static bool TryProgress(string line, out float fraction)
	{
		fraction = 0f;
		if (string.IsNullOrEmpty(line))
		{
			return false;
		}
		line = line.Trim();
		if (!line.EndsWith("%") || !float.TryParse(line.Substring(0, line.Length - 1), NumberStyles.Float, CultureInfo.InvariantCulture, out var result) || float.IsNaN(result) || float.IsInfinity(result) || result < 0f || result > 100f)
		{
			return false;
		}
		fraction = result / 100f;
		return true;
	}

	public static byte[] Run(byte[] source, int width, int height, int factor, string active, string runtime, string cache, CancellationToken token, string sourceUrl = null, SprkIllustrationProgress progress = null, SprkIllustrationReuse reuse = null)
	{
		if (width < 1 || height < 1 || width > 1024 || height > 1024)
		{
			throw new InvalidDataException("Artwork exceeds the 1024 pixel input limit");
		}
		if (progress != null)
		{
			progress.Report("Waiting for GPU", -1f);
		}
		Gate.Wait(token);
		string text = null;
		try
		{
			token.ThrowIfCancellationRequested();
			Provision(active, token, sourceUrl, progress);
			if (progress != null)
			{
				progress.Report("Checking cache", -1f);
			}
			byte[] data = ReadModel(active);
			string model = Hash(data);
			string text2 = CacheKey(source, model, factor);
			if (reuse != null)
			{
				byte[] array = reuse.Get(text2);
				if (array != null)
				{
					if (progress != null)
					{
						progress.Report("Loading saved image", -1f);
					}
					return array;
				}
			}
			bool flag = true;
			try
			{
				Directory.CreateDirectory(cache);
				Prune(cache, 1073741824L);
			}
			catch (IOException)
			{
				flag = false;
			}
			catch (UnauthorizedAccessException)
			{
				flag = false;
			}
			string text3 = Path.Combine(cache, text2 + ".png");
			if (flag && File.Exists(text3))
			{
				try
				{
					if (new FileInfo(text3).Length > 83886080)
					{
						throw new InvalidDataException("Oversized cache entry");
					}
					byte[] array2 = File.ReadAllBytes(text3);
					ValidatePng(array2, width * 4, height * 4);
					try
					{
						File.SetLastWriteTimeUtc(text3, DateTime.UtcNow);
					}
					catch (IOException)
					{
					}
					catch (UnauthorizedAccessException)
					{
					}
					if (progress != null)
					{
						progress.Report("Loading cached image", -1f);
					}
					reuse?.Set(text2, array2);
					return array2;
				}
				catch (IOException)
				{
					Delete(text3);
				}
				catch (UnauthorizedAccessException)
				{
				}
				catch (InvalidDataException)
				{
					Delete(text3);
				}
			}
			token.ThrowIfCancellationRequested();
			text = Path.Combine(Path.GetTempPath(), "sprk-illustration-" + Guid.NewGuid().ToString("N"));
			Directory.CreateDirectory(text);
			string text4 = Path.Combine(text, "models");
			string text5 = Path.Combine(text, "input.png");
			string text6 = Path.Combine(text, "output.png");
			SplitModel(data, text4);
			data = null;
			File.WriteAllBytes(text5, source);
			ProcessStartInfo processStartInfo = new ProcessStartInfo(Path.Combine(runtime, "realesrgan-ncnn-vulkan.exe"));
			processStartInfo.Arguments = "-i " + Quote(text5) + " -o " + Quote(text6) + " -m " + Quote(text4) + " -n realesrgan-x4plus-anime -s 4 -t 64 -j 1:1:1";
			processStartInfo.UseShellExecute = false;
			processStartInfo.CreateNoWindow = true;
			processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
			processStartInfo.RedirectStandardError = true;
			processStartInfo.RedirectStandardOutput = true;
			processStartInfo.WorkingDirectory = runtime;
			ProcessStartInfo startInfo = processStartInfo;
			Process process = new Process
			{
				StartInfo = startInfo
			};
			try
			{
				if (progress != null)
				{
					progress.Report("Upscaling", 0f);
				}
				process.Start();
				using (BoundProcess(process))
				{
					process.ErrorDataReceived += delegate(object sender, DataReceivedEventArgs args)
					{
						if (progress != null && TryProgress(args.Data, out var fraction))
						{
							progress.Report("Upscaling", fraction);
						}
					};
					process.OutputDataReceived += delegate
					{
					};
					process.BeginErrorReadLine();
					process.BeginOutputReadLine();
					using (token.Register(delegate
					{
						try
						{
							if (!process.HasExited)
							{
								process.Kill();
							}
						}
						catch (InvalidOperationException)
						{
						}
						catch (Win32Exception)
						{
						}
					}))
					{
						if (!process.WaitForExit(120000))
						{
							process.Kill();
							process.WaitForExit();
							throw new TimeoutException("GPU processing timed out");
						}
						process.WaitForExit();
						token.ThrowIfCancellationRequested();
						if (process.ExitCode != 0)
						{
							throw new InvalidOperationException("GPU processing unavailable (exit " + process.ExitCode + ")");
						}
					}
				}
			}
			finally
			{
				if (process != null)
				{
					((IDisposable)process).Dispose();
				}
			}
			token.ThrowIfCancellationRequested();
			if (progress != null)
			{
				progress.Report("Finishing image", -1f);
			}
			if (!File.Exists(text6) || new FileInfo(text6).Length > 83886080)
			{
				throw new InvalidDataException("Missing/oversized enhanced image");
			}
			byte[] array3 = File.ReadAllBytes(text6);
			ValidatePng(array3, width * 4, height * 4);
			reuse?.Set(text2, array3);
			if (flag)
			{
				string text7 = text3 + ".tmp";
				try
				{
					File.WriteAllBytes(text7, array3);
					Delete(text3);
					File.Move(text7, text3);
					Prune(cache, 1073741824L);
				}
				catch (IOException)
				{
				}
				catch (UnauthorizedAccessException)
				{
				}
				finally
				{
					Delete(text7);
				}
			}
			return array3;
		}
		finally
		{
			if (text != null)
			{
				try
				{
					Directory.Delete(text, recursive: true);
				}
				catch (IOException)
				{
				}
				catch (UnauthorizedAccessException)
				{
				}
			}
			Gate.Release();
		}
	}

	public static void RemoveCache(byte[] source, string active, int factor, string cache)
	{
		try
		{
			Delete(Path.Combine(cache, CacheKey(source, Hash(ReadModel(active)), factor) + ".png"));
		}
		catch (Exception)
		{
		}
	}
}
