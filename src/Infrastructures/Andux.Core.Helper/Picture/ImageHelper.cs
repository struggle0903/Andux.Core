using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.Fonts;

namespace Andux.Core.Helper.Picture
{
    /// <summary>
    /// 图片处理帮助类
    /// </summary>
    public static class ImageHelper
    {
        /// <summary>
        /// 按比例缩放图片并保存到目标路径。
        /// </summary>
        /// <param name="sourcePath">原始图片的文件路径。</param>
        /// <param name="targetPath">缩放后保存的目标图片路径。</param>
        /// <param name="scale">缩放比例，例如 0.5 表示缩小一半，2.0 表示放大两倍。</param>
        public static void ResizeImage(string sourcePath, string targetPath, double scale)
        {
            using var image = Image.Load(sourcePath);
            int width = (int)(image.Width * scale);
            int height = (int)(image.Height * scale);
            image.Mutate(x => x.Resize(width, height));
            image.Save(targetPath);
        }

        /// <summary>
        /// 裁剪图片并保存到目标路径。
        /// </summary>
        /// <param name="sourcePath">原始图片的文件路径。</param>
        /// <param name="targetPath">裁剪后保存的目标图片路径。</param>
        /// <param name="x">裁剪区域的起始 X 坐标（从左到右）。</param>
        /// <param name="y">裁剪区域的起始 Y 坐标（从上到下）。</param>
        /// <param name="width">裁剪区域的宽度。</param>
        /// <param name="height">裁剪区域的高度。</param>
        public static void CropImage(string sourcePath, string targetPath, int x, int y, int width, int height)
        {
            using var image = Image.Load(sourcePath);
            image.Mutate(ctx => ctx.Crop(new Rectangle(x, y, width, height)));
            image.Save(targetPath);
        }

        /// <summary>
        /// 将图像文件转换为 Base64 字符串。
        /// </summary>
        /// <param name="path">图像文件的路径。</param>
        /// <returns>返回图像的 Base64 编码字符串。</returns>
        public static string ImageToBase64(string path)
        {
            using FileStream stream = File.OpenRead(path);
            using var image = Image.Load(stream); // 不需要 out format
            using var ms = new MemoryStream();
            image.Save(ms, image.Metadata.DecodedImageFormat); // 使用图像元数据的格式保存
            return Convert.ToBase64String(ms.ToArray());
        }

        /// <summary>
        /// 将 Base64 字符串还原为图像文件。
        /// </summary>
        /// <param name="base64">图像的 Base64 字符串。</param>
        /// <param name="outputPath">还原后图像保存的目标路径。</param>
        public static void Base64ToImage(string base64, string outputPath)
        {
            var bytes = Convert.FromBase64String(base64);
            using var ms = new MemoryStream(bytes);
            using var image = Image.Load(ms);
            image.Save(outputPath);
        }

        /// <summary>
        /// 向图像添加文字水印并保存到指定路径。
        /// </summary>
        /// <param name="sourcePath">源图像的文件路径。</param>
        /// <param name="targetPath">添加水印后保存图像的目标路径。</param>
        /// <param name="text">要绘制的水印文本内容。</param>
        /// <param name="fontSize">水印字体大小，默认值为 24。</param>
        public static void AddTextWatermark(string sourcePath, string targetPath, string text, int fontSize = 24)
        {
            // 加载图像，使用 Rgba32 格式以支持透明度
            using var image = Image.Load<Rgba32>(sourcePath);

            // 创建字体，默认使用系统字体 Arial
            var font = SystemFonts.CreateFont("Arial", fontSize);

            // 设置水印颜色和透明度（红色，50% 透明）
            var color = Color.Red.WithAlpha(0.5f);

            // 创建文字绘制选项
            var textOptions = new TextOptions(font)
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Origin = new PointF(10, 10), // 文本起始坐标位置
                WrappingLength = image.Width - 20 // 控制换行宽度，可选
            };

            // 执行图像变换操作，绘制文字水印
            image.Mutate(ctx => ctx.DrawText((RichTextOptions)textOptions, text, color));

            // 保存最终带水印的图像
            image.Save(targetPath);
        }

    }
}
