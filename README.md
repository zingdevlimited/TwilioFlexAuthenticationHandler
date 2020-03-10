# Twilio Flex Authentication Handler
Library providing a single extension method for adding Twilio Flex token authentication into a .NET API.

## Installation

## Usage

1. In `startup.cs` import the library:
``` csharp
using Zing.TwilioFlexAuthenticationHandler;
```
2. Ensure you have an IConfiguration object available:
``` csharp
public IConfiguration Configuration { get; }

public startup(IConfiguration configuration) 
{
    Configuration = configuration;
}
```
3. Configure the service:
``` csharp
public void ConfigureServices(IServiceCollection services)
{
    ...

    services.AddTwilioFlex("Bearer", 
        options => { options.TokenPrefix = "Bearer " }, 
        Configuration
    );

    ...
}
```
> Note: AddTwilioFlex can optionaly take an AuthenticationBuilder as it's fourth parameter to allow chanining with other authentication builders

4. Add the following configuration section to your API configuration:
``` json
"TwilioSettings": {
    "AccountSID": "",
    "AuthToken": ""
}
```

Twilio Flex tokens can now be used via the standard AuthorizeAttribute.
``` csharp
[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = "Bearer")]
public class ExampleController : ControllerBase
```
The claims principal will be populated with the claims `Email`, `WorkerSID` and `WorkerRole`.