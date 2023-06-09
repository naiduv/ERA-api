# Emergency Roadside Assistance

## Setup (Windows)
- Clone this Repository
- Install docker desktop: https://docs.docker.com/compose/install/


## Running (Windows, PowerShell)
- Clone this Repo
- Open Powershell
- Change Directory to folder containing the docker-compose.yml
- Run `docker compose build` then `docker compose up`
- Navigate to http://localhost:8080/swagger/index.html
- This will also create a local postgres server with a database named "era", and create tables with seed data. Connect to it on port 5432: era/password

## Running (Windows, Visual Studio IDE)
- Clone this Repo
- Install VS2022 IDE: https://visualstudio.microsoft.com/vs/community/
- Open VS IDE
- Open this Solution 
- Right click the docker-compose node in solution explorer and click debug
- Navigate to http://localhost:8080/swagger/index.html
- This will also create a local postgres server with a database named "era", and create tables with seed data. Connect to it on port 5432: era/password

## Running Tests
- Requires the Local Postgres Database to be available. See Running steps above.
- After Cloning the repo, In Visual Studio, Open the Test Explorer and run tests.

## Map Client UI

![UI](./ui.png)

- A simple UI is available at locahost:8200. See Running instruction above.
- Allows user to see the Assitants, Update locations, Find the nearest 5 and Reserve/Release.

## Database Schema
- Postgres datbase provided
- Seeded using the init.sql script
- DB name: era
- Port: 15432
- Username: era
- Password: passowrd
- Tables: 
  - customer
    - id
    - has_reservation
  - assistant
    - id
    - is_reserved 
    - location 
  - reservation
    - id
    - customer_id
    - assistant_id
    - is_reserved
    - created_on
    - updated_on

## Postman Collection

A Postman collection is provided with all endpoints for local testing postman_collection.json

## Assumptions
- Geographic distance is used in place of actual travel distance 
- If 2 assistants are at the same distance, either is fine to be reserved
- Customers/Assistants already exist in DB
- Customers/Assistants can only have 1 active reservation
- API user will provide properly formatted, valid input json

## Compromises
- Only did integration tests, since DB heavy. These require postgres running in docker locally.
- Used Task/Async functionality in interface (so it is slightly different)
- Needs better logging
- Controllers Need to return better responses
