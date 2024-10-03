namespace LazyTest.Constants
{
    enum ColorByResponseCode
    {
        // 2xx Success
        Green = 200, // OK
        LightGreen = 201, // Created

        // 3xx Redirection
        Blue = 301, // Moved Permanently
        LightBlue = 302, // Found

        // 4xx Client Errors
        Red = 400, // Bad Request
        DarkRed = 401, // Unauthorized
        Orange = 403, // Forbidden
        DarkOrange = 404, // Not Found
        Purple = 414, // URI Too Long

        // 5xx Server Errors
        Yellow = 500, // Internal Server Error
        DarkYellow = 502, // Bad Gateway
        LightYellow = 503 // Service Unavailable
    }

    public static class DefaultConstants
    {
        public const string DefaultBrowserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.3";
    }
}
