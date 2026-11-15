# Deploying to Azure (free tier)

This guide puts the whole application online as one Azure App Service. The API
already serves the Angular build from `wwwroot` and falls back to `index.html`,
so the browser calls `/api` on the same address. No separate front-end host and
no CORS changes are needed.

| Part | Azure service | Plan |
|------|---------------|------|
| API and Angular front end | App Service (Windows, .NET 8) | Free (F1) |
| Database | Azure SQL Database | Free offer (serverless) |
| Build and deploy | GitHub Actions (`.github/workflows/azure-app-service.yml`) | Free for public repositories |

You need an Azure account. Azure asks for a card to verify the account, but the
plans above do not charge as long as the database is set to auto-pause when its
free allowance runs out (step 1).

---

## 1. Create the database

1. In the Azure portal, open **Azure SQL** and create a **SQL database**.
2. Choose **Apply offer** on the free offer banner.
3. Create a new server. Use **SQL authentication** and note the server admin
   login and password.
4. Under **Behavior when free limit reached**, choose
   **Auto-pause the database until next month**. This is what keeps it free.
5. Under **Networking**, set **Allow Azure services and resources to access this
   server** to **Yes**.
6. After it is created, open **Connection strings** and copy the **ADO.NET**
   string. Replace `{your_password}` with the password from step 3, and add
   `MultipleActiveResultSets=True;` to the end.

The app applies all EF Core migrations on startup, so the tables, grades,
subjects and the default admin are created the first time it runs.

## 2. Create the App Service

1. Create a **Web App**.
2. **Publish**: Code. **Runtime stack**: .NET 8 (LTS). **Operating system**:
   Windows. **Pricing plan**: Free F1.
3. After it is created, open **Settings > Environment variables**.
4. Under **Application settings**, add:
   - `Jwt__Key`: a long random secret, at least 64 characters. The app refuses to
     start outside Development if this is missing, shorter than 32 bytes, or
     still the placeholder from `appsettings.json`.
5. Under **Connection strings**, add:
   - Name `DefaultConnection`, value the string from step 1.6, type `SQLAzure`.
6. Save. `ASPNETCORE_ENVIRONMENT` does not need to be set; App Service defaults
   to Production.

Email is optional. Without `Email__SmtpHost` the app logs a warning and skips
sending. To send real emails, add `Email__SmtpHost`, `Email__SmtpPort`,
`Email__Username`, `Email__Password`, `Email__FromEmail` and `Email__FromName`.

## 3. Connect GitHub Actions

1. In the Web App, open **Settings > Configuration > General settings** and turn
   **SCM Basic Auth Publishing Credentials** on. Save.
2. On the **Overview** page, choose **Download publish profile**.
3. In the GitHub repository, open **Settings > Secrets and variables > Actions**:
   - **Secrets** tab: add `AZURE_WEBAPP_PUBLISH_PROFILE` and paste the whole
     contents of the downloaded file.
   - **Variables** tab: add `AZURE_WEBAPP_NAME` with the Web App name.
4. Open **Actions > Build and deploy to Azure App Service > Run workflow**.

After that, every push to `main` builds and deploys automatically. Pull requests
only run the build, so broken code is caught before it reaches the live site.

## 4. Secure the default admin (do this first)

The database seed creates `admin@school.com` with the password `Admin@123`, and
that password is visible in this public repository. Change it straight after the
first deploy:

1. Open `https://<app-name>.azurewebsites.net/swagger`.
2. Call `POST /api/admins/login` with the seeded credentials and copy the token.
3. Choose **Authorize** and paste the token.
4. Call `PUT /api/admins/1/password` with:

```json
{
  "currentPassword": "Admin@123",
  "newPassword": "<a new strong password>",
  "confirmNewPassword": "<the same password>"
}
```

## 5. Known limits of the free tier

- **Cold starts.** The F1 plan has no Always On, and the free database pauses
  when idle. The first visit after a quiet period can take up to a minute while
  both wake up. Startup database calls retry automatically; if the app still
  fails to start, refresh after a minute.
- **CPU quota.** F1 has a daily CPU allowance. A portfolio demo stays well
  inside it.
- **Uploaded files** are stored on the App Service disk under
  `wwwroot/uploads/submissions`. They persist between restarts, but treat them
  as demo data and do not rely on them surviving every redeploy.
- **Public data.** Anyone with the link can see the site. Use made-up student
  and teacher records only, never real people's details.

## Troubleshooting

| Symptom | Likely cause |
|---------|--------------|
| HTTP 500.30 on first load | `Jwt__Key` or `DefaultConnection` missing. Check **Log stream** for the exact message. |
| Startup error mentioning login failed or a network error | Wrong password in the connection string, or **Allow Azure services** is off on the SQL server. |
| Deploy step fails with 401 | **SCM Basic Auth Publishing Credentials** is off, or the publish profile was downloaded before turning it on. Download it again. |
| Page refresh on a deep link shows 404 | Package is missing `wwwroot/index.html`. Re-run the workflow and check the "Copy front end" step. |
