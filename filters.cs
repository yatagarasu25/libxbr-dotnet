using MathEx;
using System.Runtime.InteropServices;

namespace libxbr_dotnet;
public unsafe static class xbr
{
	public enum mode
	{
		xbr2x,
		xbr3x,
		xbr4x,
		hq2x,
		hq3x,
		hq4x,
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct xbr_params
	{
		public byte* input;
		public byte* output;
		public int inWidth, inHeight;
		public int inPitch, outPitch;
		public byte* data;
	};

	public static Int32[] allocate_data() => new Int32[1 << 24];


	[DllImport("libxbr.dll")]
	public static extern void xbr_init_data(byte* data);
	public static void xbr_init_data(Int32[] data)
	{
		fixed (int* dataf = &data[0])
		{
			xbr_init_data((byte*)dataf);
		}
	}

	[DllImport("libxbr.dll")]
	public static extern void xbr_filter_xbr2x(xbr_params* ctx);
	[DllImport("libxbr.dll")]
	public static extern void xbr_filter_xbr3x(xbr_params* ctx);
	[DllImport("libxbr.dll")]
	public static extern void xbr_filter_xbr4x(xbr_params* ctx);

	[DllImport("libxbr.dll")]
	public static extern void xbr_filter_hq2x(xbr_params* ctx);
	[DllImport("libxbr.dll")]
	public static extern void xbr_filter_hq3x(xbr_params* ctx);
	[DllImport("libxbr.dll")]
	public static extern void xbr_filter_hq4x(xbr_params* ctx);


	public static (vec2i size, colorb[] data) filter(mode mode, colorb[] input, vec2i size, int[] xbr_data)
	{
		fixed (colorb* input_pin = &input[0])
		fixed (int* xbr_data_pin = &xbr_data[0])
		{
			return filter(mode, (byte*)input_pin, size, (byte*)xbr_data_pin);
		}
	}

	public static (vec2i size, colorb[] data) filter(mode mode, colorb[] input, vec2i size, byte* xbr_data)
	{
		fixed (colorb* input_pin = &input[0])
		{
			return filter(mode, (byte*)input_pin, size, xbr_data);
		}
	}

	public static (vec2i size, colorb[] data) filter(mode mode, byte* inputb, vec2i size, int[] xbr_data)
	{
		fixed (int* xbr_data_pin = &xbr_data[0])
		{
			return filter(mode, inputb, size, (byte*)xbr_data_pin);
		}
	}

	public static (vec2i size, colorb[] data) filter(mode mode, byte* inputb, vec2i size, byte* xbr_data)
	{
		var scaleFactor = mode switch {
				mode.xbr2x => 2,
				mode.xbr3x => 3,
				mode.xbr4x => 4,
				mode.hq2x => 2,
				mode.hq3x => 3,
				mode.hq4x => 4,
				_ => throw new NotImplementedException(),
			};
		var outputSize = size * scaleFactor;
		var output = new colorb[outputSize.product];

		fixed (colorb* output_pin = &output[0])
		{
			byte* outputb = (byte*)output_pin;

			var xbr_params = new xbr_params {
				input = inputb,
				output = outputb,
				data = xbr_data,
				inWidth = size.x,
				inHeight = size.y,
				inPitch = size.x * 4,
				outPitch = size.x * 4 * scaleFactor
			};

			switch (mode)
			{
				case mode.xbr2x:
					xbr_filter_xbr2x(&xbr_params);
					break;
				case mode.xbr3x:
					xbr_filter_xbr3x(&xbr_params);
					break;
				case mode.xbr4x:
					xbr_filter_xbr4x(&xbr_params);
					break;
				case mode.hq2x:
					xbr_filter_hq2x(&xbr_params);
					break;
				case mode.hq3x:
					xbr_filter_hq3x(&xbr_params);
					break;
				case mode.hq4x:
					xbr_filter_hq4x(&xbr_params);
					break;
			}
		}

		return (outputSize, output);
	}
}
