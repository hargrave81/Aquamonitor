using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using AquaMonitor.Data.Models;
using AquaMonitor.Web.Global;
using AquaMonitor.Web.Services;
using MartinCostello.Logging.XUnit;
using Xunit;
using Xunit.Abstractions;
using PowerState = AquaMonitor.Data.Models.PowerState;

namespace AquaTest
{
    public class TestSystemOps: IDisposable
    {
        private readonly ILoggerFactory injectLog;
        private readonly ITestOutputHelper outputHelper;
        private readonly PowerRelayService powerRelay;
        private readonly IGlobalState globalData;
        public TestSystemOps(ITestOutputHelper outputHelper)
        {
            this.outputHelper = outputHelper;
            injectLog = new LoggerFactory();
            injectLog.AddProvider(new XUnitLoggerProvider(this.outputHelper, new XUnitLoggerOptions()));
            globalData = GlobalData.Create(System.IO.File.ReadAllText("test.json"));
            powerRelay = new PowerRelayService(injectLog.CreateLogger<PowerRelayService>(), globalData);
        }

        public void Dispose()
        {
            injectLog.Dispose();
        }

        [Fact]
        public void TestIntervalOn()
        {
            SystemOperationsService sos = new SystemOperationsService(injectLog.CreateLogger<SystemOperationsService>(), globalData, powerRelay, null);
            globalData.WaterLevels.First(t => t.Id == 1).FloatHigh = true; // we have enough water
            Assert.Equal(1, sos.ProcessRelay(RelayLocation.A, DateTime.Parse("05/09/2020 10:31:00")));
        }

        [Fact]
        public void TestIntervalOff()
        {
            SystemOperationsService sos = new SystemOperationsService(injectLog.CreateLogger<SystemOperationsService>(), globalData, powerRelay, null);
            globalData.WaterLevels.First(t => t.Id == 1).FloatHigh = true; // we have enough water
            Assert.Equal(-1, sos.ProcessRelay(RelayLocation.A, DateTime.Parse("05/09/2020 10:46:00")));
        }

        [Fact]
        public void TestIntervalTooEarly()
        {
            SystemOperationsService sos = new SystemOperationsService(injectLog.CreateLogger<SystemOperationsService>(), globalData, powerRelay, null);
            globalData.WaterLevels.First(t => t.Id == 1).FloatHigh = true; // we have enough water
            Assert.Equal(-1, sos.ProcessRelay(RelayLocation.A, DateTime.Parse("05/09/2020 3:31:00")));
        }


        [Fact]
        public void TestIntervalTooLate()
        {
            SystemOperationsService sos = new SystemOperationsService(injectLog.CreateLogger<SystemOperationsService>(), globalData, powerRelay, null);
            globalData.WaterLevels.First(t => t.Id == 1).FloatHigh = true; // we have enough water
            Assert.Equal(-1, sos.ProcessRelay(RelayLocation.A, DateTime.Parse("05/09/2020 23:31:00")));
        }

        [Fact]
        public void TestIntervalWaterLow()
        {
            SystemOperationsService sos = new SystemOperationsService(injectLog.CreateLogger<SystemOperationsService>(), globalData, powerRelay, null);
            globalData.WaterLevels.First(t => t.Id == 1).FloatHigh = true; // we have enough water
            Assert.Equal(1, sos.ProcessRelay(RelayLocation.A, DateTime.Parse("05/09/2020 10:31:00")));
            globalData.WaterLevels.First(t => t.Id == 1).FloatHigh = false; // water fell too low
            Assert.Equal(-1, sos.ProcessRelay(RelayLocation.A, DateTime.Parse("05/09/2020 10:31:03")));
        }

        [Fact]
        public void TestTempRangeOk()
        {
            SystemOperationsService sos = new SystemOperationsService(injectLog.CreateLogger<SystemOperationsService>(), globalData, powerRelay, null);
            globalData.TemperatureF = 80; // desirable start temp
            Assert.Equal(1, sos.ProcessRelay(RelayLocation.B, DateTime.Parse("05/09/2020 10:30:00")));
        }

        
        [Fact]
        public void TestTempRangeTooLow()
        {
            SystemOperationsService sos = new SystemOperationsService(injectLog.CreateLogger<SystemOperationsService>(), globalData, powerRelay, null);
            globalData.TemperatureF = 60; // desirable start temp
            Assert.Equal(-1, sos.ProcessRelay(RelayLocation.B, DateTime.Parse("05/09/2020 10:30:00")));
        }

        
        [Fact]
        public void TestTempRangeTooHigh()
        {
            SystemOperationsService sos = new SystemOperationsService(injectLog.CreateLogger<SystemOperationsService>(), globalData, powerRelay, null);
            globalData.TemperatureF = 160; // desirable start temp
            Assert.Equal(-1, sos.ProcessRelay(RelayLocation.B, DateTime.Parse("05/09/2020 10:30:00")));
        }

        
        [Fact]
        public void TestTempRangeEntersRunningRange()
        {
            int t= 0;
            outputHelper.WriteLine("lowTemp {0}  highTemp {1}", globalData.GetRelay(RelayLocation.B).MinTempF, globalData.GetRelay(RelayLocation.B).MaxTempF);
            SystemOperationsService sos = new SystemOperationsService(injectLog.CreateLogger<SystemOperationsService>(), globalData, powerRelay, null);
            for(int x = (int)globalData.GetRelay(RelayLocation.B).MinTempF - 10; x< globalData.GetRelay(RelayLocation.B).MinTempF+globalData.GetRelay(RelayLocation.B).TempVariance*2;x ++)
            {
                outputHelper.WriteLine("Temp: {0} @ {1}", x, DateTime.Parse("05/09/2020 10:" + t.ToString("00")  + ":00"));
                globalData.TemperatureF = x; // set temp
                var result = sos.ProcessRelay(RelayLocation.B, DateTime.Parse("05/09/2020 10:" + t.ToString("00")  + ":00"));
                System.Threading.Thread.Sleep(25);                
                if(result != -1 && globalData.GetRelay(RelayLocation.B).MinTempF.Value.Equals(x))
                {
                    outputHelper.WriteLine("System reached starting point");
                    Assert.Equal((int)globalData.GetRelay(RelayLocation.B).MinTempF,x);
                    Assert.Equal(1,result);                    
                }
                else if(result != -1 && x <globalData.GetRelay(RelayLocation.B).MinTempF)
                {
                    Assert.Equal(-1, result);
                }
                t++;
            }
            for(int x = (int)globalData.GetRelay(RelayLocation.B).MinTempF+(int)globalData.GetRelay(RelayLocation.B).TempVariance*2; x > globalData.GetRelay(RelayLocation.B).MinTempF - 10; x--)
            {
                outputHelper.WriteLine("Temp: {0} @ {1}", x,DateTime.Parse("05/09/2020 10:" + t.ToString("00")  + ":00"));
                globalData.TemperatureF = x; // set temp
                var result = sos.ProcessRelay(RelayLocation.B, DateTime.Parse("05/09/2020 10:" + t.ToString("00")  + ":00"));
                System.Threading.Thread.Sleep(25);
                if(result == -1)
                {
                    outputHelper.WriteLine("System claims it should be shut off");
                    Assert.Equal((int)globalData.GetRelay(RelayLocation.B).MinTempF -(int)globalData.GetRelay(RelayLocation.B).TempVariance,x);
                    Assert.Equal(-1,result);   
                    break;                 
                }
                t++;
            }
        }

        [Fact]
        public void TestTempRangeLeavesRunningRange()
        {
            int t= 0;
            outputHelper.WriteLine("lowTemp {0}  highTemp {1}", globalData.GetRelay(RelayLocation.B).MinTempF, globalData.GetRelay(RelayLocation.B).MaxTempF);
            SystemOperationsService sos = new SystemOperationsService(injectLog.CreateLogger<SystemOperationsService>(), globalData, powerRelay, null);
            globalData.GetRelay(RelayLocation.B).CurrentState = PowerState.On; // set it to on
            for(int x = (int)globalData.GetRelay(RelayLocation.B).MaxTempF - 10; x< globalData.GetRelay(RelayLocation.B).MaxTempF+globalData.GetRelay(RelayLocation.B).TempVariance*2;x ++)
            {
                outputHelper.WriteLine("Temp: {0} @ {1}", x, DateTime.Parse("05/09/2020 10:" + t.ToString("00")  + ":00"));
                globalData.TemperatureF = x; // set temp
                var result = sos.ProcessRelay(RelayLocation.B, DateTime.Parse("05/09/2020 10:" + t.ToString("00")  + ":00"));
                System.Threading.Thread.Sleep(25);                
                if(result == -1 && (globalData.GetRelay(RelayLocation.B).MaxTempF + globalData.GetRelay(RelayLocation.B).TempVariance).Equals(x))
                {
                    outputHelper.WriteLine("System reached top point");
                    Assert.Equal((int)globalData.GetRelay(RelayLocation.B).MaxTempF + (int)globalData.GetRelay(RelayLocation.B).TempVariance,x);
                    Assert.Equal(-1,result);                    
                }
                else if(result == -1 && x <globalData.GetRelay(RelayLocation.B).MaxTempF)
                {
                    // bad
                    Assert.Equal(1, result);
                }
                t++;
            }
            Assert.True(true);
        }
    }
}
