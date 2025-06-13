ContactsApp
===========
John Vishnefske

Example full stack .NET application. Console client (targeting .NET 8.0) consuming an ASP.NET Core gRPC API with a SQL backend. The API is designed to run on Linux.

![screenshot](screenshot.png)

## TODO

*   **Database Setup:** Provide clear instructions for setting up the `ContactsDb` SQL Server database, including schema creation (Contacts, Prefixes, Suffixes tables) and initial data population.
*   **API Configuration:** Document how to configure the `ContactsApi`'s database connection string (in `appsettings.json` or `Utilities.cs`) and ensure the gRPC endpoint is correctly exposed (e.g., `https://localhost:7001`).
*   **Client Configuration:** Document how to configure the `Client` console application's gRPC endpoint address in `ApiClient.cs` to match the running `ContactsApi`.
*   **Error Handling & Logging:** Enhance error handling in both client and API for more robust production use. Implement structured logging.
*   **User Interface:** The current client is a console application. If a graphical user interface (GUI) is desired, a new client project would need to be developed using a cross-platform UI framework (e.g., Avalonia UI, .NET MAUI).
*   **Testing:** Add unit and integration tests for both the `ContactsApi` and `Client` projects.
*   **Deployment:** Add basic instructions for deploying the `ContactsApi` to a Linux environment (e.g., Docker, Kestrel behind Nginx) and the `Client` console application.
