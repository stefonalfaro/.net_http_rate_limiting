# .net_http_rate_limiting
Endpoint Rate Limiting for HTTP APIs

## Startup
RateLimit.initialize(configuration.systemSettings.rateLimitConfig);

Add this object to your configuration

    ```
    public class RateLimitConfig
    {
        public int globalRequests { get; set; }
        public int overridePerEndpointLimit { get; set; }
        public int thresholdDuration { get; set; }
    }
    ```

## Useage for Global Rate Limit
```
if (!RateLimit.processRequest(req))
    return new TooManyRequestsResult();
```

## Usage for Specific Limit for Endpoints
```
if (!RateLimit.processRequest(req, true))
    return new TooManyRequestsResult();
```
