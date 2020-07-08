using AquaMonitor.Data.Models;
using System.Collections.Generic;

namespace AquaMonitor.Web.Models
{
    /// <summary>
    /// Water level request model
    /// </summary>
    public class WaterLevelRequestMessageModel
    {
        /// <summary>
        /// List of water levels
        /// </summary>
        public IEnumerable<WaterLevelModel> WaterLevels { get; set; }
    }

    /// <summary>
    /// Model to change water level settings
    /// </summary>
    public class WaterLevelModel
    {
        /// <summary>
        /// ID of water level
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Logical name of water level
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// GPIO Pin
        /// </summary>
        /// <value></value>
        public int Pin { get; set; }

        /// <summary>
        /// Converts model to DbModel
        /// </summary>
        /// <returns></returns>
        public WaterLevel ToWaterLevel()
        {
            return new WaterLevel()
            {
                Id = this.Id,
                Name = this.Name,                
                Pin = this.Pin
            };
        }

        /// <summary>
        /// Updates Db Model from view Model
        /// </summary>
        /// <param name="fromDb"></param>
        public void UpdateWaterLevel(WaterLevel fromDb)
        {
            fromDb.Name = this.Name;
            fromDb.Pin = this.Pin;
           
        }
    }
}
