using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IndexGeneration.Models
{
    public static class Global
    {
        public static string LogExecuteablePath { get; set; }
    }

    public enum ClientAppType
    {
        CCX4000,
        CCX3000,
        CCX2000,
        CCX1000
    }
}
