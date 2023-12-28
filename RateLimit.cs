public static class RateLimit
    {
        private class RateInformation
        {
            public int Requests;
            public DateTime WindowStart;
        }

        private static Dictionary<string, RateInformation> rateMemory = new Dictionary<string, RateInformation>();

        private static int GlobalMaxRequests = 1000; // Default max requests per time window per IP for global limit
        private static int EndpointSpecificOverride = 30;
        private static TimeSpan TimeWindow = TimeSpan.FromMinutes(5); // Time window for rate limit

        //This is called from Startup to set the values from the masterConfig.systemSettings.rateLimitConfig
        public static void initialize(RateLimitConfig config)
        {
            if (config != null)
            {
                GlobalMaxRequests = config.globalRequests;
                EndpointSpecificOverride = config.overridePerEndpointLimit;
                TimeWindow = TimeSpan.FromMinutes(config.thresholdDuration);
            }
        }

        // Return True if allowed, Return False if RateLimited
        public static bool processRequest(HttpRequest req, bool? endpointSpecificLock = false)
        {
            string ip = Utilities.GetClientIPAddress(req);
            string route = req.Path;
            string key = endpointSpecificLock == true ? $"{ip}_{route}" : ip; // Composite key for route-specific limit, else just IP

            if (!rateMemory.TryGetValue(key, out RateInformation rateInfo)) // First request from this IP for this route/global
            {
                rateInfo = new RateInformation { Requests = 1, WindowStart = DateTime.UtcNow };
                rateMemory[key] = rateInfo;
                return true;
            }

            if (DateTime.UtcNow - rateInfo.WindowStart > TimeWindow) // Time window has passed, reset
            {  
                rateInfo.Requests = 1;
                rateInfo.WindowStart = DateTime.UtcNow;
                return true;
            }

            int requestLimit = GlobalMaxRequests;

            if (endpointSpecificLock == true)
                requestLimit = EndpointSpecificOverride;

            if (rateInfo.Requests < requestLimit) // Within allowed request limit
            {               
                rateInfo.Requests++;
                return true;
            }

            // Exceeded request limit
            return false;
        }
    }
