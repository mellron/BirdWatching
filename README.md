string originalUrl = "https://google'sitbaby.com";
string encodedUrl = Uri.EscapeDataString(originalUrl);
// encodedUrl will be "https%3A%2F%2Fgoogle'sitbaby.com"
// Note: In this specific case, the apostrophe is not encoded as it's considered a safe character in this context


string decodedUrl = Uri.UnescapeDataString(encodedUrl);
// This will return the original URL: "https://google'sitbaby.com"
