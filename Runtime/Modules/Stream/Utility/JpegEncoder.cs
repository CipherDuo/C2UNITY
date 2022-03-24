using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine.Experimental.Rendering;

public static class JpegEncoder
{
	public enum SubSample
	{
		TJ_444,
		TJ_422,
		TJ_420,
		TJ_GRAYSCALE
	}

	[Flags]
	public enum Flags
	{
		NONE = 0x0,
		TJ_BGR = 0x1,
		TJ_BOTTOMUP = 0x2,
		TJ_FORCEMMX = 0x8,
		TJ_FORCESSE = 0x10,
		TJ_FORCESSE2 = 0x20,
		TJ_ALPHAFIRST = 0x40,
		TJ_FORCESSE3 = 0x80,
		TJ_FASTUPSAMPLE = 0x100,
		TJFLAG_NOREALLOC = 0x400,
		TJFLAG_FASTDCT = 0x800
	}

	[Flags]
	public enum PixelFormat
	{
		TJPF_INVALID = -1,
		TJPF_RGB = 0x0,
		TJPF_BGR = 0x1,
		TJPF_RGBX = 0x2,
		TJPF_BGRX = 0x3,
		TJPF_XBGR = 0x4,
		TJPF_XRGB = 0x5,
		TJPF_GRAY = 0x6,
		TJPF_RGBA = 0x7,
		TJPF_BGRA = 0x8,
		TJPF_ABGR = 0x9,
		TJPF_ARGB = 0xA,
		TJPF_CMYK = 0xB,
		TJPF_UNKNOWN = -1
	}

	public static IntPtr decoder;

	public static IntPtr encoder;

	public static List<byte[]> buffer;

	[DllImport("turbojpeg")]
	private static extern IntPtr tjInitCompress();

	[DllImport("turbojpeg")]
	private static extern IntPtr tjInitDecompress();

	[DllImport("turbojpeg")]
	private static extern int tjCompress(IntPtr handle, byte[] srcBuf, int width, int pitch, int height, int pixelSize, byte[] dstBuf, ref int compressedSize, int jpegSubsamp, int jpegQual, int flags);

	[DllImport("turbojpeg")]
	private static extern int tjCompress2(IntPtr handle, byte[] srcBuf, int width, int pitch, int height, int pixelFormat, byte[] dstBuf, ref int compressedSize, int jpegSubsamp, int jpegQual, int flags);

	[DllImport("turbojpeg")]
	private static extern int tjDecompressHeader3(IntPtr handle, byte[] jpegBuf, int jpegSize, ref int width, ref int height, ref int jpegSubsamp, ref int jpegColorspace);

	[DllImport("turbojpeg")]
	private static extern int tjDecompress2(IntPtr handle, byte[] jpegBuf, int jpegSize, byte[] dstBuf, int width, int pitch, int height, int pixelFormat, int flags);

	[DllImport("turbojpeg")]
	private static extern int tjDestroy(IntPtr handle);

	private static PixelFormat GetPixelFormat(int pixelSize, GraphicsFormat format, ref Flags flags)
	{
		switch (pixelSize)
		{
			case 1:
				return PixelFormat.TJPF_GRAY;
			case 3:
				return PixelFormat.TJPF_RGB;
			case 4:
				if (format != GraphicsFormat.R8G8B8A8_UNorm)
				{
					if (format != GraphicsFormat.B8G8R8A8_UNorm)
					{
						break;
					}
					flags |= Flags.TJ_BGR;
					return PixelFormat.TJPF_BGRX;
				}
				return PixelFormat.TJPF_RGBX;
		}
		return PixelFormat.TJPF_INVALID;
	}

	public static byte[] Encode(byte[] raw, int width, int height, int pixelSize, GraphicsFormat textureFormat, int quality = 75, Flags flags = Flags.TJ_BOTTOMUP)
	{
		int compressedSize = 0;
		int jpegSubsamp = 2;
		int num = Marshal.SizeOf(raw.GetValue(0).GetType());
		byte[] array = new byte[raw.Length * num];
		
		if (tjCompress(encoder, raw, width, 0, height, pixelSize, array, ref compressedSize, jpegSubsamp, quality, (int)flags) < 0)
		{
			return null;
		}
		Array.Resize(ref array, compressedSize);
		return array;
	}

	public static byte[] Decode(byte[] jpg, int pixelSize, ref int width, ref int height, int index, GraphicsFormat textureFormat, Flags flags = Flags.TJ_BOTTOMUP)
	{
		int jpegSubsamp = 2;
		int jpegColorspace = 0;
		var pixelFormat = GetPixelFormat(pixelSize, textureFormat, ref flags);
		
		tjDecompressHeader3(decoder, jpg, jpg.Length, ref width, ref height, ref jpegSubsamp, ref jpegColorspace);
		if (buffer.Count <= index)
		{
			buffer.Add(new byte[width * height * pixelSize]);
		}
		if (buffer[index].Length != width * height * pixelSize)
		{
			buffer[index] = new byte[width * height * pixelSize];
		}
		
		tjDecompress2(decoder, jpg, jpg.Length, buffer[index], width, 0, height, (int)pixelFormat, (int)flags);
		return buffer[index];
	}

	public static void Init()
	{
		decoder = tjInitDecompress();
		encoder = tjInitCompress();
		buffer = new List<byte[]>();
	}
}
