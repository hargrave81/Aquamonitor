using System;
using System.Collections.Generic;
using System.Text;

namespace AquaMonitor.Data.Models
{
    /// <summary>
    /// Model used to change password
    /// </summary>
    public class PasswordModel
    {
        /// <summary>
        /// Password
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Confirm Password
        /// </summary>
        public string PasswordConfirm { get; set; }
    }
}
