using System;

namespace OMS.API.Models
{
    public class ApiRequest
    {
        public string Userid { get; set; }

        public string Version { get; set; }

        public string Format { get; set; }

        public string Timestamp { get; set; }

        public string Sign { get; set; }
    }
}