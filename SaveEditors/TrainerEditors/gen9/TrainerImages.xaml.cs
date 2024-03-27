using PKHeX.Core;
using System.Drawing;
using System.Drawing.Imaging;
using Microsoft.Maui.Graphics.Platform;
using System.Runtime.InteropServices;
using static PKHeX.Core.SaveBlockAccessor9SV;
using System.Reflection;
using CommunityToolkit.Maui.Converters;
using SkiaSharp;
namespace PKHeXMAUI
{

    public partial class TrainerImages : ContentPage
    {
        public SAV9SV SAV = (SAV9SV)MainPage.sav;

        public TrainerImages()
        {
            InitializeComponent();


            var result = GetImageData(KPictureProfileCurrent, KPictureProfileCurrentWidth, KPictureProfileCurrentHeight);
            var testenc = result.Encode(SKEncodedImageFormat.Png, 100);
            Microsoft.Maui.Graphics.IImage testimage;
            using (Stream stream = testenc.AsStream())
            {
                testimage = PlatformImage.FromStream(stream);
            }
            MainTrainerImage.Source = new ByteArrayToImageSourceConverter().ConvertFrom(testimage.AsBytes());
            result = GetImageData(KPictureIconCurrent, KPictureIconCurrentWidth, KPictureIconCurrentHeight);
            testenc = result.Encode(SKEncodedImageFormat.Png, 100);
            using (Stream stream = testenc.AsStream())
            {
                testimage = PlatformImage.FromStream(stream);
            }
            CurrTrainerIcon.Source = new ByteArrayToImageSourceConverter().ConvertFrom(testimage.AsBytes());
            result = GetImageData(KPictureIconInitial, KPictureIconInitialWidth, KPictureIconInitialHeight);
            testenc = result.Encode(SKEncodedImageFormat.Png, 100);
            using (Stream stream = testenc.AsStream())
            {
                testimage = PlatformImage.FromStream(stream);
            }
            InitialTrainerIcon.Source = new ByteArrayToImageSourceConverter().ConvertFrom(testimage.AsBytes());
        }
        public SKBitmap GetImageData(uint kd, uint kw, uint kh)
        {
            SCBlockAccessor blocks = ((SAV9SV)MainPage.sav).Blocks;
            var data = blocks.GetBlock(kd).Data;
            var width = blocks.GetBlockValue<uint>(kw);
            var height = blocks.GetBlockValue<uint>(kh);
            var result = DXT1.Decompress(data, (int)width, (int)height);
            return GetBitmap(result, (int)width, (int)height);
        }
        public static SKBitmap GetBitmap(byte[] data, int width, int height, int length, PixelFormat format = PixelFormat.Format32bppArgb)
        {
            var bmp = new SKBitmap(width, height,SKColorType.Bgra8888,SKAlphaType.Opaque);
            var ptr = bmp.GetPixels();
            Marshal.Copy(data, 0, ptr, length);
            return bmp;
        }

        public static SKBitmap GetBitmap(byte[] data, int width, int height, PixelFormat format = PixelFormat.Format32bppArgb)
        {
            return GetBitmap(data, width, height, data.Length, format);
        }
    }
   
}