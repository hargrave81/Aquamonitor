using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using Iot.Device.Media;
using Microsoft.Extensions.Hosting;
using static System.Drawing.Image;

namespace AquaMonitor.Web.Services
{
    /// <summary>
    /// Generates a sky image of time range
    /// </summary>
    public class SkyService
    {
        private const float hour = 25f;
        private const float minute = .4166666667f;
        private const float second = .00694444444f;

        /// <summary>
        /// Light sky image
        /// </summary>
        private readonly byte[] LightSky;

        /// <summary>
        /// Dark sky image
        /// </summary>
        private readonly byte[] DarkSky;

        /// <summary>
        /// CTOR
        /// </summary>
        /// <param name="env"></param>
        public SkyService(IHostEnvironment env)
        {
            LightSky = File.ReadAllBytes(Path.Combine(env.ContentRootPath,"wwwroot/img/lightsky.png"));
            DarkSky = File.ReadAllBytes(Path.Combine(env.ContentRootPath,"wwwroot/img/darksky.png"));
        }

        /// <summary>
        /// Builds a sky
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public Bitmap BuildSky(DateTime? start, DateTime? end)
        {
            var block = new Bitmap(600, 24, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            using var msls = new MemoryStream(LightSky);
            using var msds = new MemoryStream(DarkSky);
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
            var startWidth = start.Value.Hour * hour + start.Value.Minute * minute + start.Value.Second * second;
            var endWidth = end.Value.Hour * hour + end.Value.Minute * minute + end.Value.Second * second;
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
            using var ms = new System.IO.MemoryStream();
            block.Save(ms, ImageFormat.Png);
            return ms.ToArray();
        }

    }
}
