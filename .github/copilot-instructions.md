# Copilot Instructions

## General Guidelines
- Combine Login and Register into a single auth view (modal/partial).
- Set the default first page to the combined auth view.
- After successful authentication, redirect to HomeController.Index; the 'Gestión de Usuarios' card on Home Index should link to DashboardController.Index.
- After login, display a dashboard composed of rectangular module cards, with the primary module being "Gestión de Usuarios."
- Dashboard should expose other module indexes.
- Remove main navigation links from the header; navigation will be inside dashboard/module views.

## Code Style
- Use specific formatting rules.
- Follow naming conventions.

## Project-Specific Rules
- Custom requirement A.
- Custom requirement B.