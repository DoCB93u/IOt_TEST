using System;
using System.Collections.Generic;
using System.Linq;
using System.Printing;
using System.Text;
using System.Threading.Tasks;

namespace IOt_TEST
{
    public static class Config
    {
        /* curl -v -X POST http://192.168.111.254:9090/api/v1/7ruOP9OA3u7nbPUdS1gH/telemetry --header Content-Type:application/json --data "{temperature:32}"*/
        
        private readonly static string DEVICE_ID = "f519eed0-f28e-11ef-88fc-6309100975a0";
        private readonly static string TELEMETRY_KEY = "temperature";
        private readonly static string HOST = "http://192.168.111.254:9090";

        public static string GetDeviceID() { return DEVICE_ID; }

        public static string GetTelemetryKey() { return TELEMETRY_KEY; }

        public static string GetHost() { return HOST; }
    }
}
