# Cards56 Web Solution

## Prepare Development Environment

1. Download and install asp.net core 3.1 sdk

2. Download and install Visual Studio Code

3. Clone the source code
        git clone https://github.com/bheemboy/Cards56.git

4. When running in development mode you need to generate and store a dev certificate for the web application. You can do this using the following commands:


        dotnet dev-certs https -ep $env:USERPROFILE\.aspnet\https\cards56web.pfx -p crypticpassword
        dotnet dev-certs https --trust

    Replace 'crypticpassword' appropriately in the command above

5. Next you need to save the same password for Cards56Web.csproj in your .net user secrets. You can use the following command.
        dotnet user-secrets -p cards56web\cards56web.csproj set "Kestrel:Certificates:Development:Password" "crypticpassword"


## Running in Production Environment using docker
