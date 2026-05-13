# NexusERP: Clean Architecture Inventory Manager

An enterprise-grade, foundational Inventory Management and ERP system built on **.NET 8**. 

This project serves as a strict implementation of **Clean Architecture** and the **Model-View-Presenter (MVP)** pattern. By strictly decoupling the UI, business logic, and database layers, this codebase acts as a scalable starter template that can easily evolve into a modern, distributed system.

---

## 🏗️ Architecture Overview

The solution is divided into four distinct layers, ensuring that dependencies only point inwards toward the Domain:

1. **`NexusERP.Domain` (Core)**
   - Contains pure enterprise logic and models (`Product`, `Supplier`, `Category`, `InventoryTransaction`).
   - Defines standard Enums and Custom Exceptions.
   - **Zero external dependencies.**

2. **`NexusERP.Application` (Use Cases & Presenters)**
   - Contains the **Presenters** (the "brains" of the UI) and interfaces (`IProductView`, `IProductRepository`).
   - Orchestrates data flow, KPI calculations, and business validation.
   - **Does not know about WinForms or SQL.**

3. **`NexusERP.Infrastructure` (Data & External Services)**
   - Implements the Repositories using raw **ADO.NET** and **Stored Procedures**.
   - Handles external file generations using **ClosedXML** (Excel) and **QuestPDF** (PDF).

4. **`NexusERP.UI` (Presentation Layer)**
   - A **dumb** Windows Forms frontend. 
   - Powered by a modern Dependency Injection (DI) container.
   - Contains absolutely no business rules or direct database calls.

---

## 🚀 Current Features

* **Dashboard Analytics:** Real-time KPI calculations (Total Value, Potential Profit, Low Stock Alerts, Health Status).
* **Inventory Management:** Full CRUD operations for Products with automated transaction logging (IN/OUT/ADJ).
* **Supplier Directory:** Manage supplier contact information and associate them with specific inventory transactions.
* **Reporting Engine:** View historical transaction logs with one-click exports to **Excel (.xlsx)** and **PDF**.
* **Dependency Injection:** Centralized service registration using `Microsoft.Extensions.DependencyInjection`.

---

## 🛠️ Getting Started

### Prerequisites
* .NET 8.0 SDK
* SQL Server (Express or Developer edition)
* Visual Studio 2022

### Installation

1. **Database Setup:**
   - Open SQL Server Management Studio (SSMS).
   - Execute the provided `script.sql` file to generate the `Product_Inventory` database, tables, stored procedures, and initial seed data.

2. **Configure Connection:**
   - Open `NexusERP.UI/App.config`.
   - Update the `Server` in the connection string to match your local SQL Server instance:
     ```xml
     <add name="InventoryDb" connectionString="Server=YOUR_SERVER_NAME;Database=Product_Inventory;Trusted_Connection=True;TrustServerCertificate=True;Encrypt=False;" providerName="Microsoft.Data.SqlClient" />
     ```

3. **Build & Run:**
   - Open `NexusERP.sln` in Visual Studio.
   - Set `NexusERP.UI` as the Startup Project.
   - Clean, Rebuild, and Start the application.

---

## 🗺️ Future Roadmap (Phase 2 & Beyond)

**This project is currently a foundational starter.** Because the architecture strictly separates concerns, the system is primed for massive structural upgrades without needing to rewrite the core business logic. 

Planned future iterations include:

- [ ] **Entity Framework Core (EF Core):** Swap out the current raw ADO.NET/Stored Procedure Infrastructure layer with a modern ORM (Entity Framework Core) for easier migrations and LINQ querying.
- [ ] **RESTful API Layer:** Introduce an ASP.NET Core Web API project to sit between the frontend and the Application layer, transitioning the app from a monolithic desktop structure to a distributed client-server model.
- [ ] **Modern Frontend Integration:** Because the `NexusERP.UI` layer is "dumb", it can be seamlessly replaced. Future phases will involve building a modern web-based frontend (e.g., React, Angular, or Blazor) to consume the new API.
- [ ] **Authentication & Authorization:** Introduce JWT-based user roles (Admin, Manager, Viewer).
