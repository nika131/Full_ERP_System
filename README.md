# NexusERP: Clean Architecture Inventory Manager

An enterprise-grade, foundational Inventory Management and ERP system built on **.NET 8**. 

This project serves as a strict implementation of **Clean Architecture** and the **Model-View-Presenter (MVP)** pattern. By strictly decoupling the UI, business logic, and database layers, this codebase acts as a scalable starter template that is currently transitioning from a standalone desktop monolith to a distributed client-server architecture.

---

## 🏗️ Architecture Overview

The solution is divided into distinct layers, ensuring that dependencies only point inwards toward the Domain:

1. **`NexusERP.Domain` (Core)**
   - Contains pure enterprise logic and entities (`Product`, `User`, `InventoryTransaction`).
   - Defines standard Enums (Roles, Transaction Types) and Custom Exceptions.
   - **Zero external dependencies.**

2. **`NexusERP.Application` (Use Cases & Presenters)**
   - Contains the **Presenters** (the "brains" of the UI) and interfaces (`IProductView`, `IUserRepository`).
   - Orchestrates data flow, KPI calculations, authorization gates, and business validation.
   - **Does not know about WinForms, SQL, or Entity Framework.**

3. **`NexusERP.Infrastructure` (Data & External Services)**
   - Powered by **Entity Framework Core 8** for standard CRUD operations and state tracking.
   - Utilizes a **Hybrid SQL Execution Strategy**: Relies on EF Core for reads/updates, but executes raw SQL Stored Procedures for complex, atomic financial ledger logging.
   - Handles external file generations using **ClosedXML** (Excel) and **QuestPDF** (PDF).

4. **`NexusERP.UI` (Presentation Layer)**
   - A **stateless** Windows Forms frontend. 
   - Powered by `Microsoft.Extensions.DependencyInjection` to wire up DbContexts and Presenters.
   - Contains absolutely no business rules or direct database calls.

---

## 🚀 Current Features

* **Role-Based Access Control (RBAC):** Tiered security (Admin, Manager, Cashier) enforcing strict UI and data-level access.
* **Employee Management:** Full user lifecycle management featuring **Soft Deletion** (`IsActive` flags) to protect financial ledger integrity.
* **Dashboard Analytics:** Real-time KPI calculations (Total Value, Potential Profit, Low Stock Alerts).
* **Inventory Management:** Full operations for Products with automated, atomic transaction logging (IN/OUT/ADJ).
* **Reporting Engine:** View historical transaction logs with one-click exports to **Excel (.xlsx)** and **PDF**.

---

## 🛠️ Getting Started

### Prerequisites
* .NET 8.0 SDK
* SQL Server (Express or Developer edition)
* Visual Studio 2022

### Installation

1. **Database Setup:**
   - Open SQL Server Management Studio (SSMS).
   - Execute the provided `script.sql` file to generate the database, tables, and initial seed data.

2. **Configure Connection:**
   - Verify the database connection string in your Dependency Injection container (`Program.cs` or `appsettings.json`), ensuring it points to your local SQL Server instance.

3. **Build & Run:**
   - Open `NexusERP.sln` in Visual Studio.
   - Set `NexusERP.UI` as the Startup Project.
   - Clean, Rebuild, and Start the application. Log in with the default Admin credentials provided in your seed script.

---

## 🗺️ Roadmap: The Web API Transition

This project has successfully completed **Phase 1** (The Desktop Monolith). It is now actively being refactored into a distributed web system.

- [x] **Entity Framework Core (EF Core):** Swap out raw ADO.NET for EF Core for easier migrations, LINQ querying, and implicit transactions.
- [x] **Authentication & Authorization:** Introduce user roles and application-level session states.
- [ ] **RESTful API Layer (Current Phase):** Introduce an ASP.NET Core Web API project to sit between the frontend and the Application layer, transitioning the app to a stateless distributed model using JWT authentication.
- [ ] **Modern Frontend Integration:** Build a modern web-based frontend (Angular) to consume the new API, discarding the legacy WinForms UI.