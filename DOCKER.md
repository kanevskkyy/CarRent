# CarRent Docker setup

## Start the application

1. Copy `.env.example` to `.env` in the repository root. Change the local SQL and JWT values if you need custom credentials. Set the optional OAuth, Cloudinary, and PayPal values to enable those integrations.
2. Run `docker compose up --build` from the repository root.
3. Open the frontend at <http://localhost:3000>. The API is available at <http://localhost:8080>; Swagger is at <http://localhost:8080/swagger>.

The first API startup applies the checked-in Entity Framework migrations and then runs the existing seeders. Automatic migrations are enabled only by the Compose configuration. SQL Server and Redis wait for health checks before the API starts.

## Data and configuration

SQL Server and Redis data are kept in named Docker volumes, so `docker compose down` does not delete them. `docker compose down -v` removes those volumes and permanently deletes the local database and Redis data.

The frontend is built with `VITE_API_URL` and the public Google/GitHub client IDs. Rebuild the frontend after changing those values. Do not place private provider secrets in `VITE_*` variables; API-side secrets belong in the root `.env` file.

## OAuth local setup

Open the frontend using the same host and port you register with each provider. With the default Compose ports, use <http://localhost:3000>.

- **Google:** In Google Cloud Console, edit the OAuth 2.0 Web client used by `GOOGLE_CLIENT_ID` and add `http://localhost:3000` under **Authorized JavaScript origins**. If you open the app at a different origin (for example `http://localhost:5173` when running Vite), add that exact origin too. Origins include scheme and port, but no path.
- **GitHub:** In the GitHub OAuth App settings, set **Authorization callback URL** to `http://localhost:3000/auth/github/callback`. Keep `GITHUB_REDIRECT_URI` in the root `.env` set to that same URL. If you use another frontend host/port, update both values to the exact same callback URL.

After changing OAuth IDs or callback settings in `.env`, rebuild the frontend and restart the API with `docker compose up --build -d frontend api`.

The included default database password and JWT signing key are for local development only. Replace them before using this stack in a shared or public environment. The seeded demo accounts are also intended only for local development.
