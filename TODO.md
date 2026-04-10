# BudgetTracker Implementation Roadmap

This document outlines the tasks required to transition from the current prototype to a fully functional production-ready application using .NET MAUI, MVVM, and SQLite.

## 🏗️ Phase 1: Infrastructure & Data Layer
*Essential foundation for data persistence and application architecture.*

### [Backend]
- [ ] **Setup SQLite Database**: Install `sqlite-net-sqlcipher` and configure the database connection string. [Infrastructure]
- [ ] **Define Database Schema**: Create tables for `Users`, `Transactions`, and `BudgetCategories` with proper relationships. [Infrastructure]
- [ ] **Create Repository Pattern**: Implement generic and specific repositories to abstract database operations. [Infrastructure]
- [ ] **Database Migration Service**: Implement logic to handle initial seeding and schema updates. [Infrastructure]

---

## 🔐 Phase 2: Authentication & User Management
*Moving from mock login to secure user sessions.*

### [Backend]
- [ ] **Authentication Service**: Create a service to handle Login/Register logic with SQLite lookups. [Auth]
- [ ] **Secure Password Hashing**: Implement BCrypt or PBKDF2 for hashing passwords before storing them. [Auth]
- [ ] **Session Management**: Use `SecureStorage` to persist user tokens or login states across app restarts. [Auth]

### [Frontend]
- [ ] **Form Validation**: Add real-time validation to Login and Register fields (Email format, password strength). [Auth]
- [ ] **Error Handling UI**: Implement toast notifications or alert dialogs for failed login attempts. [Auth]

---

## 💰 Phase 3: Core Budgeting Logic
*The heart of the app: managing financial goals and limits.*

### [Backend]
- [ ] **Budget Service**: Implement CRUD operations for budget categories linked to the logged-in user. [Budget]
- [ ] **Budget Calculation Logic**: Create logic to calculate "Remaining" balances by querying transactions against budget limits. [Budget]
- [ ] **Onboarding Data Persistence**: Save the goals and financial targets selected during onboarding to the DB. [Budget]

### [Frontend]
- [ ] **Category Management UI**: Finalize the "Add Category" bottom sheet and add "Edit/Delete" functionality. [Budget]
- [ ] **Dynamic Progress Bars**: Ensure progress bars update instantly when a transaction is added or a budget is modified. [Budget]

---

## 💸 Phase 4: Transaction Management
*Tracking every dollar in and out.*

### [Backend]
- [ ] **Transaction Service**: Implement methods to fetch transactions filtered by month, category, or type. [Transactions]
- [ ] **Balance Sync**: Create a trigger or service logic to update the User's total balance whenever a transaction is modified. [Transactions]

### [Frontend]
- [ ] **Transaction Entry Form**: Build a dedicated page/overlay for adding transactions with category pickers. [Transactions]
- [ ] **Search & Filter UI**: Add search functionality and date range filtering to the transactions list. [Transactions]
- [ ] **Swipe-to-Action**: Add "Swipe to Delete" or "Swipe to Edit" gestures on transaction list items. [Transactions]

---

## 📊 Phase 5: Analytics & Statistics
*Visualizing financial health.*

### [Backend]
- [ ] **Aggregation Engine**: Create optimized SQL queries for grouping expenses by category for charts. [Statistics]
- [ ] **Monthly Trends Logic**: Logic to compare current month spending vs. previous months. [Statistics]

### [Frontend]
- [ ] **Interactive Charts**: Integrate a charting library (like Microcharts or Syncfusion) for the breakdown view. [Statistics]
- [ ] **Insight Generation**: Create a UI component to display automated "tips" based on spending habits. [Statistics]

---

## 🎨 Phase 6: Refinement & Polish
*Ensuring a high-quality user experience.*

### [Frontend]
- [ ] **Dark Mode Support**: Audit and refine all styles for perfect appearance in Dark Mode. [Design]
- [ ] **Loading States**: Add `ActivityIndicators` or Skeleton screens during database operations. [UX]
- [ ] **Animations**: Enhance page transitions and FAB morphing for smoother visual flow. [UX]
- [ ] **Profile Customization**: Allow users to update their name, profile picture, and currency preferences. [Profile]
