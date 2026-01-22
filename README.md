# PartShop API

Backend API for the PartShop inventory and e-commerce platform.

This project provides a .NET 6 Web API backed by SQL Server stored procedures and supports
admin inventory management, auditing, authentication, and frontend integration.

---

## Tech Stack

- ASP.NET Core 6 Web API
- SQL Server (stored procedures)
- Cookie-based authentication
- Role-based authorization
- React frontend (separate repo)
- Azure App Service & Azure SQL (deployment target)

---

## Core Features

- Inventory management (Parts, Categories, Makes, Models)
- Location hierarchy (Site → Area → Aisle → Shelf → Section)
- Safe partial updates (PATCH)
- Full audit logging for admin actions
- Image upload & validation
- Paginated browse and search endpoints
- Role-protected admin APIs

---

## Project Structure

