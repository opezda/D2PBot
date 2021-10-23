using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;

namespace D2InfoBot {
    public static class Image {
        public static ColorThiefDotNet.Color GetDominateColor(string url) {
            return new ColorThiefDotNet.ColorThief().GetColor(DownloadImage(url)).Color;
        }
        public static Bitmap MergeImages(string firstImageUrl, string secondImageUrl){
            Bitmap firstImage = DownloadImage(firstImageUrl);
            Bitmap secondImage = DownloadImage(secondImageUrl);
            int outputImageWidth = firstImage.Width > secondImage.Width ? firstImage.Width : secondImage.Width;

            int outputImageHeight = firstImage.Height;

            Bitmap outputImage = new Bitmap(outputImageWidth, outputImageHeight, PixelFormat.Format32bppArgb);

            Graphics graphics = Graphics.FromImage(outputImage);
            graphics.DrawImage(firstImage, new Rectangle(new Point(), firstImage.Size),
                new Rectangle(new Point(), firstImage.Size), GraphicsUnit.Pixel);
            graphics.DrawImage(secondImage, new Rectangle(new Point(0, 0), secondImage.Size),
                new Rectangle(new Point(), secondImage.Size), GraphicsUnit.Pixel);

            return outputImage;
        }
        public static Bitmap DownloadImage(string url){
            WebRequest request = WebRequest.Create(url);
            WebResponse response = request.GetResponse();
            Stream responseStream = response.GetResponseStream();
            return new Bitmap(responseStream!);
        }
        public static async Task<string> SaveImageToDsServer(Bitmap image, CommandContext ctx) {
            image.Save("rankI.png", ImageFormat.Png);
            DiscordMessage m = await ctx.Channel.SendMessageAsync(new DiscordMessageBuilder().WithFile(File.OpenRead("rankI.png")));
            string t = m.Attachments[0].Url;
            await m.DeleteAsync();
            return t;
        }
    }
}