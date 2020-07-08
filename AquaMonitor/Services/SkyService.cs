using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using Microsoft.Extensions.Hosting;
using static System.Drawing.Image;

namespace AquaMonitor.Web.Services
{
    /// <summary>
    /// Generates a sky image of time range
    /// </summary>
    public class SkyService
    {
        private const float Hour = 25f;
        private const float Minute = .4166666667f;
        private const float Second = .00694444444f;

        /// <summary>
        /// Light sky image
        /// </summary>
        private readonly byte[] lightSky;

        /// <summary>
        /// Dark sky image
        /// </summary>
        private readonly byte[] darkSky;

        /// <summary>
        /// CTOR
        /// </summary>
        /// <param name="env"></param>
        public SkyService(IHostEnvironment env)
        {
            lightSky = File.ReadAllBytes(Path.Combine(env.ContentRootPath,"wwwroot/img/lightsky.png"));
            darkSky = File.ReadAllBytes(Path.Combine(env.ContentRootPath,"wwwroot/img/darksky.png"));
        }

        /// <summary>
        /// Builds a sky
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public Bitmap BuildSky(DateTime? start, DateTime? end)
        {
            var block = new Bitmap(600, 24, PixelFormat.Format32bppPArgb);
            using var msls = new MemoryStream(lightSky);
            using var msds = new MemoryStream(darkSky);
            using var ls = FromStream(msls);
            using var ds = FromStream(msds);
            using var g = Graphics.FromImage(block);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.DrawImage(ls,0,0, 600,24);
                
            // calculate how much of the dark sky we draw to the left and how much to draw to the right
            if (!start.HasValue)
                start = DateTime.Parse("0:00:00");
            if (!end.HasValue)
                end = DateTime.Parse("23:59:59.999");
            var startWidth = start.Value.Hour * Hour + start.Value.Minute * Minute + start.Value.Second * Second;
            var endWidth = end.Value.Hour * Hour + end.Value.Minute * Minute + end.Value.Second * Second;
            if (endWidth < startWidth)
            {
                //flip flop
                var holder = startWidth;
                startWidth = endWidth;
                endWidth = holder;
            }
            g.DrawImage(ds, new RectangleF(startWidth,0,endWidth - startWidth, 24), new RectangleF(startWidth,0, endWidth - startWidth, 24),GraphicsUnit.Pixel);
            g.DrawRectangle(Pens.Black, 0,0,599,23);
            return block;
        }

        /// <summary>
        /// Builds a sky into PNG
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public Byte[] BuildSkyBytes(DateTime? start, DateTime? end)
        {
            using var block = BuildSky(start, end);
            using var ms = new MemoryStream();
            block.Save(ms, ImageFormat.Png);
            return ms.ToArray();
        }

    }
}
