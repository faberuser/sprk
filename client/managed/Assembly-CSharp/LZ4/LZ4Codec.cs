using System;
using System.Runtime.CompilerServices;
using System.Text;
using LZ4.Services;

namespace LZ4;

public static class LZ4Codec
{
	private static readonly LZ4.ILZ4Service Encoder;

	private static readonly LZ4.ILZ4Service EncoderHC;

	private static readonly LZ4.ILZ4Service Decoder;

	private static LZ4.ILZ4Service _service_MM32;

	private static LZ4.ILZ4Service _service_MM64;

	private static LZ4.ILZ4Service _service_CC32;

	private static LZ4.ILZ4Service _service_CC64;

	private static LZ4.ILZ4Service _service_N32;

	private static LZ4.ILZ4Service _service_N64;

	private static LZ4.ILZ4Service _service_S32;

	private static LZ4.ILZ4Service _service_S64;

	public static string CodecName => string.Format("{0}/{1}/{2}HC", (Encoder == null) ? "<none>" : Encoder.CodecName, (Decoder == null) ? "<none>" : Decoder.CodecName, (EncoderHC == null) ? "<none>" : EncoderHC.CodecName);

	static LZ4Codec()
	{
		Try(InitializeLZ4s);
		if (IntPtr.Size == 4)
		{
			Encoder = _service_MM32 ?? _service_MM64 ?? _service_N32 ?? _service_CC32 ?? _service_N64 ?? _service_CC64 ?? _service_S32 ?? _service_S64;
			Decoder = _service_MM32 ?? _service_MM64 ?? _service_CC64 ?? _service_CC32 ?? _service_N64 ?? _service_N32 ?? _service_S64 ?? _service_S32;
			EncoderHC = _service_MM32 ?? _service_MM64 ?? _service_N32 ?? _service_CC32 ?? _service_N64 ?? _service_CC64 ?? _service_S32 ?? _service_S64;
		}
		else
		{
			Encoder = _service_MM64 ?? _service_MM32 ?? _service_N64 ?? _service_N32 ?? _service_CC64 ?? _service_CC32 ?? _service_S32 ?? _service_S64;
			Decoder = _service_MM64 ?? _service_N64 ?? _service_N32 ?? _service_CC64 ?? _service_MM32 ?? _service_CC32 ?? _service_S64 ?? _service_S32;
			EncoderHC = _service_MM64 ?? _service_MM32 ?? _service_CC32 ?? _service_CC64 ?? _service_N32 ?? _service_N64 ?? _service_S32 ?? _service_S64;
		}
		if (Encoder == null || Decoder == null)
		{
			throw new NotSupportedException("No LZ4 compression service found");
		}
	}

	private static void Try(Action method)
	{
		try
		{
			method();
		}
		catch
		{
		}
	}

	private static LZ4.ILZ4Service Try<T>() where T : LZ4.ILZ4Service, new()
	{
		try
		{
			return AutoTest(new T());
		}
		catch
		{
			return null;
		}
	}

	private static LZ4.ILZ4Service AutoTest(LZ4.ILZ4Service service)
	{
		byte[] bytes = Encoding.UTF8.GetBytes("Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.");
		byte[] array = new byte[MaximumOutputLength(bytes.Length)];
		int num = service.Encode(bytes, 0, bytes.Length, array, 0, array.Length);
		if (num < 0)
		{
			return null;
		}
		byte[] array2 = new byte[bytes.Length];
		if (service.Decode(array, 0, num, array2, 0, array2.Length, knownOutputLength: true) != bytes.Length)
		{
			return null;
		}
		if (Encoding.UTF8.GetString(array2) != "Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.")
		{
			return null;
		}
		if (service.Decode(array, 0, num, array2, 0, array2.Length, knownOutputLength: false) != bytes.Length)
		{
			return null;
		}
		if (Encoding.UTF8.GetString(array2) != "Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.")
		{
			return null;
		}
		byte[] array3 = new byte[MaximumOutputLength(bytes.Length)];
		int num2 = service.EncodeHC(bytes, 0, bytes.Length, array3, 0, array3.Length);
		if (num2 < 0)
		{
			return null;
		}
		byte[] array4 = new byte[bytes.Length];
		if (service.Decode(array3, 0, num2, array4, 0, array4.Length, knownOutputLength: true) != bytes.Length)
		{
			return null;
		}
		if (Encoding.UTF8.GetString(array4) != "Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.")
		{
			return null;
		}
		if (service.Decode(array3, 0, num2, array4, 0, array4.Length, knownOutputLength: false) != bytes.Length)
		{
			return null;
		}
		if (Encoding.UTF8.GetString(array4) != "Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.")
		{
			return null;
		}
		return service;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void InitializeLZ4s()
	{
		_service_S32 = Try<LZ4.Services.Safe32LZ4Service>();
		_service_S64 = Try<LZ4.Services.Safe64LZ4Service>();
	}

	public static int MaximumOutputLength(int inputLength)
	{
		return inputLength + inputLength / 255 + 16;
	}

	public static int Encode(byte[] input, int inputOffset, int inputLength, byte[] output, int outputOffset, int outputLength)
	{
		return Encoder.Encode(input, inputOffset, inputLength, output, outputOffset, outputLength);
	}

	public static byte[] Encode(byte[] input, int inputOffset, int inputLength)
	{
		if (inputLength < 0)
		{
			inputLength = input.Length - inputOffset;
		}
		if (input == null)
		{
			throw new ArgumentNullException("input");
		}
		if (inputOffset < 0 || inputOffset + inputLength > input.Length)
		{
			throw new ArgumentException("inputOffset and inputLength are invalid for given input");
		}
		byte[] array = new byte[MaximumOutputLength(inputLength)];
		int num = Encode(input, inputOffset, inputLength, array, 0, array.Length);
		if (num != array.Length)
		{
			if (num < 0)
			{
				throw new InvalidOperationException("Compression has been corrupted");
			}
			byte[] array2 = new byte[num];
			Buffer.BlockCopy(array, 0, array2, 0, num);
			return array2;
		}
		return array;
	}

	public static int EncodeHC(byte[] input, int inputOffset, int inputLength, byte[] output, int outputOffset, int outputLength)
	{
		return (EncoderHC ?? Encoder).EncodeHC(input, inputOffset, inputLength, output, outputOffset, outputLength);
	}

	public static byte[] EncodeHC(byte[] input, int inputOffset, int inputLength)
	{
		if (inputLength < 0)
		{
			inputLength = input.Length - inputOffset;
		}
		if (input == null)
		{
			throw new ArgumentNullException("input");
		}
		if (inputOffset < 0 || inputOffset + inputLength > input.Length)
		{
			throw new ArgumentException("inputOffset and inputLength are invalid for given input");
		}
		byte[] array = new byte[MaximumOutputLength(inputLength)];
		int num = EncodeHC(input, inputOffset, inputLength, array, 0, array.Length);
		if (num != array.Length)
		{
			if (num < 0)
			{
				throw new InvalidOperationException("Compression has been corrupted");
			}
			byte[] array2 = new byte[num];
			Buffer.BlockCopy(array, 0, array2, 0, num);
			return array2;
		}
		return array;
	}

	public static int Decode(byte[] input, int inputOffset, int inputLength, byte[] output, int outputOffset, int outputLength = 0, bool knownOutputLength = false)
	{
		return Decoder.Decode(input, inputOffset, inputLength, output, outputOffset, outputLength, knownOutputLength);
	}

	public static byte[] Decode(byte[] input, int inputOffset, int inputLength, int outputLength)
	{
		if (inputLength < 0)
		{
			inputLength = input.Length - inputOffset;
		}
		if (input == null)
		{
			throw new ArgumentNullException("input");
		}
		if (inputOffset < 0 || inputOffset + inputLength > input.Length)
		{
			throw new ArgumentException("inputOffset and inputLength are invalid for given input");
		}
		byte[] array = new byte[outputLength];
		if (Decode(input, inputOffset, inputLength, array, 0, outputLength, knownOutputLength: true) != outputLength)
		{
			throw new ArgumentException("outputLength is not valid");
		}
		return array;
	}
}
