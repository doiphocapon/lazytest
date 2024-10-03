# Define parameters for image, sitemapUrl, domcount, and token
param (
    [Parameter(Mandatory = $true)]
    [bool]$image,

    [Parameter(Mandatory = $true)]
    [bool]$domcount,

    [Parameter(Mandatory = $true)]
    [string]$sitemapUrl,

    [Parameter(Mandatory = $true)]
    [string]$token
)

# Hardcoded UserAgent, MaxRedirects, and API base URL
$UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/103.0.0.0 Safari/537.36"
$MaxRedirects = 10
$BaseUrl = "https://lazytest-grcebzdqftgncjcy.eastus-01.azurewebsites.net/"

# Convert boolean parameters to string values for URL query
$imageParam = if ($image) { "true" } else { "false" }
$domcountParam = if ($domcount) { "true" } else { "false" }

# Construct the initial API URL based on input parameters, including token
$ApiUrl = $BaseUrl + "api/testsite?image=" + [uri]::EscapeDataString($imageParam) + "&sitemapurl=" + [uri]::EscapeDataString($sitemapUrl) + "&token=" + [uri]::EscapeDataString($token)

# Define headers using the hardcoded UserAgent
$headers = @{
    "User-Agent" = $UserAgent
}

# Send a GET request using the constructed URL, hardcoded UserAgent, and MaxRedirects
$response = Invoke-RestMethod -Uri $ApiUrl -Method GET -Headers $headers -MaximumRedirection $MaxRedirects

# Check if the response is not null or empty
if ($response) {
    # Display the SiteId if it exists
    if ($null -ne $response.SiteId) {
        Write-Host "Site ID: $($response.SiteId)" -ForegroundColor Yellow
    } else {
        Write-Host "Site ID is null" -ForegroundColor Red
    }

    # Loop through each URL in the Urls list if they exist
    if ($response.Urls -and $response.Urls.Count -gt 0) {
        Write-Host "Processing URLs:" -ForegroundColor Green
        
        foreach ($url in $response.Urls) {
            Write-Host "Processing URL: $url" -ForegroundColor Cyan

            # Extract necessary parameters for the next GET request
            $itemurl = $url
            $siteid = $response.SiteId
            
            # Construct the new API URL for the next GET request
            $NextApiUrl = $BaseUrl + "api/testitem?itemurl=" + [uri]::EscapeDataString($itemurl) + "&siteid=" + $siteid + "&domcount=" + $domcountParam
            
            # Send the next GET request
            $urlResponse = Invoke-RestMethod -Uri $NextApiUrl -Method GET -Headers $headers -MaximumRedirection $MaxRedirects
            
            # Check if the response is not null or empty
            if ($urlResponse) {
                # Determine color based on StatusCode
                $statusColor = if ($urlResponse.StatusCode -eq "OK") { "Green" } else { "Yellow" }
                
                # Display details from the response
                Write-Host "Response from $NextApiUrl :" -ForegroundColor $statusColor
                Write-Host "  URL: $($urlResponse.Url)" -ForegroundColor $statusColor
                Write-Host "  Content Length: $($urlResponse.ContentLength)" -ForegroundColor $statusColor
                Write-Host "  DOM Count: $($urlResponse.DomCount)" -ForegroundColor $statusColor
                Write-Host "  Status Code: $($urlResponse.StatusCode)" -ForegroundColor $statusColor
                Write-Host "  HTTP Status Code: $($urlResponse.HttpStatusCode)" -ForegroundColor $statusColor
                Write-Host "  Response Time: $($urlResponse.ResponseTime) ms" -ForegroundColor $statusColor
                Write-Host "---------------------------" -ForegroundColor $statusColor
            } else {
                Write-Host "No data returned for $NextApiUrl." -ForegroundColor Red
            }
        }
    } else {
        Write-Host "No URLs found." -ForegroundColor Red
    }
} else {
    Write-Host "No data returned from API." -ForegroundColor Red
}
