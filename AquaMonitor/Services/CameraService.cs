using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using AquaMonitor.Data.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Exception = System.Exception;
using IDisposable = System.IDisposable;
using Random = System.Random;
using Task = System.Threading.Tasks.Task;
using TimeSpan = System.TimeSpan;

namespace AquaMonitor.Web.Services
{
    //rtsp://admin:Tcapmoc81@10.0.0.37:554/live/ch0

    /// <summary>
    /// Services to take frames from a camera stream
    /// </summary>
    public class CameraService: IHostedService, IDisposable
    {
        private int executionCount;
        private readonly ILogger<CameraService> logger;
        private Timer timer;
        private readonly IGlobalState globalData;
        private readonly Random random;
        private readonly Queue<string> frames = new Queue<string>();

        
        /// <summary>
        /// CTor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="globalData"></param>
        public CameraService(ILogger<CameraService> logger, IGlobalState globalData)
        {
            this.logger = logger;
            this.globalData = globalData;
            random = new Random();
        }

        /// <summary>
        /// Starts service
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        public Task StartAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Camera Service running.");

            string path = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                $"wwwroot/img/camera");
            if (!System.IO.Directory.Exists(path))
                System.IO.Directory.CreateDirectory(path);

            timer = new Timer(DoWork, null, TimeSpan.Zero,
                TimeSpan.FromSeconds(15 + random.NextDouble())); // this ensures the .net timer issues with DHT  sensors isn't constantly hit

            return Task.CompletedTask;
        }

        async private void DoWork(object state)
        {
            if (string.IsNullOrEmpty(globalData.More.CameraJPGUrl))
                return; // no work to do

            var count = Interlocked.Increment(ref executionCount);

            logger.LogInformation(
                "Camera Service is working. Count: {Count}", count);
            try
            {
                await ProcessWork(state);
            }
            catch (Exception ex)
            {
                logger.LogError("Failed to process Atmosphere: " + ex.Message);
            }
        }

        /// <summary>
        /// Stops service
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        public Task StopAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Camera Service is stopping.");

            timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }


        /// <summary>
        /// Process work
        /// </summary>
        /// <param name="state"></param>
#pragma warning disable IDE0060 // Remove unused parameter
        private async Task ProcessWork(object state)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            try
            {
                await Task.Run(() =>
                {
                    if (frames.Count > 10)
                    {
                        var lastFrame = frames.Dequeue();
                        try
                        {
                            System.IO.File.Delete(lastFrame);
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "Failed to remove local camera file clean up: " + ex.Message);
                        }
                    }

                    var thumbNail = System.IO.Path.Combine(
                        System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                        $"wwwroot/img/camera/frame{Guid.NewGuid().ToString()}.jpg");
                    var args =
                        $"-rtsp_transport tcp -y -i \"{globalData.More.CameraJPGUrl}\" -ss 00:00:01.500 -frames:v 1 \"{thumbNail}\"";
                    logger.LogInformation($"Spawning Process: ffmpeg.exe {args}");
                    var proc = Process.Start(
                        "ffmpeg", args);
                    var abortTime = DateTime.Now.AddSeconds(14);
                    while (!proc.HasExited)
                    {
                        System.Threading.Thread.Sleep(100);
                        if (DateTime.Now > abortTime)
                        {
                            proc.Kill();
                            proc.Close();
                            break; // its taken too long
                        }
                    }

                    if (System.IO.File.Exists(thumbNail))
                    {
                        if (new System.IO.FileInfo(thumbNail).Length > 0)
                        {
                            // add image to stack
                            frames.Enqueue(thumbNail);
                        }
                        else
                        {
                            // delete file
                            System.Threading.Thread.Sleep(5000);
                            // try to delete
                            try
                            {
                                System.IO.File.Delete(thumbNail);
                            }
                            catch (Exception ex)
                            {
                                logger.LogError(ex, "Failed to remove local camera file: " + ex.Message);
                            }
                        }
                    }
                });

            }
            catch (Exception ex)
            {
                logger.LogError("failed to process camera frame: " + ex.Message);
            }        
        }


        /// <summary>
        /// Dispose Service
        /// </summary>
        public void Dispose()
        {
            timer?.Dispose();
        }
    }

}
