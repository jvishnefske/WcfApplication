ContactsApp
===========
John Vishnefske

Example full stack .NET application. Console client (targeting .NET 8.0) consuming an ASP.NET Core gRPC API with a SQL backend. The API is designed to run on Linux.

![screenshot](screenshot.png)

## TODO

*   - [x] **Database Setup:** The `ContactsApi` now uses **SQLite**. The database file (`contacts.db`) will be automatically created and initialized with necessary tables (Contacts, Prefixes, Suffixes) and initial lookup data when the API starts for the first time. No manual setup is required.
*   - [x] **API Configuration:** The `ContactsApi`'s database connection string is now configured in `appsettings.json` under the `ConnectionStrings:DefaultConnection` key. The `Utilities` class is now a service that reads this configuration. Ensure the gRPC endpoint is correctly exposed (e.g., `https://localhost:7001`).
*   - [x] **Client Configuration:** The `Client` console application's gRPC endpoint address is now configurable via `Client/App.config` under the `GrpcApiAddress` key.
*   - [x] **Clean Build:** Achieve a build with zero errors and zero warnings across all projects.
*   - [ ] **Error Handling & Logging:** Enhance error handling in both client and API for more robust production use. Implement structured logging.
*   - [ ] **User Interface:** The current client is a console application. If a graphical user interface (GUI) is desired, a new client project would need to be developed using a cross-platform UI framework (e.g., Avalonia UI, .NET MAUI). (Out of Scope for this request)
*   - [x] **Testing:**
    *   - [x] **Unit Testing:** Unit tests are implemented for `ContactsApi`'s persistence layer (`UtilitiesTests.cs`) and gRPC services (`ContactsGrpcServiceTests.cs`, `LookupsGrpcServiceTests.cs`). These tests mock dependencies and use `TestServerCallContext` for isolated testing of service logic. (Updated `IConfiguration` mocking for robustness)
    *   - [ ] **Integration Testing:** Implement integration tests where the gRPC API is hosted in an in-memory test server (e.g., `Microsoft.AspNetCore.TestHost`) and called using a gRPC client.
*   - [ ] **Deployment:** Add basic instructions for deploying the `ContactsApi` to a Linux environment (e.g., Docker, Kestrel behind Nginx) and the `Client` console application. (Documentation Task)
*   **Best Practices for Simplicity:**
    *   - [x] **Asynchronous Programming:** Ensure all I/O-bound and long-running operations are truly asynchronous to avoid blocking calls and simplify concurrent execution flow.
    *   - [ ] **Minimize Exceptions for Control Flow:** Use exceptions only for truly exceptional conditions, not for normal program flow, to simplify logic and improve readability. (Review and minor adjustments if needed)
    *   - [ ] **Offload Long-Running Tasks:** Delegate CPU-intensive or long-duration operations to background services to keep request processing fast and responsive. (Not applicable to current simple CRUD).
    *   - [x] **Correct Context and Service Lifetime Management:** Pay close attention to `HttpContext` and dependency injection scope lifetimes, especially in background tasks, to prevent subtle bugs and ensure predictable behavior. (`HttpContext` is no longer relevant, `Utilities` is a singleton).
    *   - [ ] **Efficient Data Retrieval:** Retrieve only the necessary data from the database to simplify data models and reduce processing overhead. (General guideline, no specific code changes planned unless clear inefficiency is found).
