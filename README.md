git - https://github.com/YakobchukArtem/SmartSupport.git

Instructions for running the SmartSupport project:

1. Install required software:

1.1 Basic tools:
- Install Git: https://git-scm.com
- Install Docker and Docker Compose: https://www.docker.com
- Install .NET 8.0 SDK: https://dotnet.microsoft.com

1.2 Local language model (Deepseek-R1):
- Install Ollama:
  curl -fsSL https://ollama.com/install.sh | sh
- Download the model:
  ollama pull deepseek-r1

1.3 Dify platform:
- Run Dify locally:
  docker run -d --name dify -p 3000:3000 -v dify-data:/data langgenius/dify:latest
  OR use the cloud version: https://cloud.dify.ai

2. Clone and configure the project:
git clone https://github.com/YakobchukArtem/SmartSupport.git
cd SmartSupport

- Update the appsettings.json file in the root directory with your configuration.
- Generate a Dify API key (if running locally) and place it in the appropriate config field (e.g., ApiLocalKeyField).

3. Create a local MSSQL database:
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourStrong(!)Password" -p 1433:1433 --name sqlserver -d mcr.microsoft.com/mssql/server:2022-latest

- Update the connection string in appsettings.json to match your local SQL Server setup.

4. Apply database migrations:
dotnet restore
dotnet ef database update

Note: If you don't have EF tools installed, run:
dotnet tool install --global dotnet-ef

5. Run the project:
dotnet run --project SmartSupport.API

6. Start Dify (if not already running):
docker compose up

7. Run Ollama model:
ollama run deepseek-r1

8. Set up the AI agent in Dify:
- Open Dify in your browser: http://localhost:3000
- Create a agent
- Provide instructions and configure it as needed
- Copy the generated API key and update it in your config (.env or appsettings.json)
- Ensure DIFY_ENDPOINT points to your agent (e.g., http://localhost:3000/api/v1/applications/{app_id})
- Connect dify to ollama (you will see button)

9. Testing:
- Open Swagger UI: http://localhost:8080/swagger
- Test endpoints such as:
  POST /api/identity/register
  POST /api/chat/send

10. Excel test endpoint:
- Upload an Excel file with test questions via:
  POST /api/ai/test
