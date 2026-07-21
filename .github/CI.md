# CI checks and branch protection

Path-filtered GitHub Actions workflows live under `.github/workflows/`.

## Workflow / check names

After these workflows have run at least once on the default branch, require them under
**Settings → Branches → Branch protection rule** (or a ruleset) for `main`:

| Workflow file | Workflow name | Job check name (require this) |
|---------------|---------------|-------------------------------|
| `auth.yml` | Auth CI | `Lint, build & test` |
| `gateway.yml` | Gateway CI | `Lint, build & test` |
| `notification.yml` | Notification CI | `Lint, build & test` |
| `historystore.yml` | HistoryStore CI | `Lint, build & test` |
| `web.yml` | Web CI | `Lint, build & test` |

Optional: also require each workflow’s `Docker image` job if image builds must pass before merge.

In the GitHub UI the status check list often shows as `Lint, build & test` under the workflow name (e.g. **Auth CI / Lint, build & test**). Select all five service checks so a PR cannot merge while any triggered pipeline is red.

Note: path filters mean a PR that only changes Web will not run Auth CI — GitHub treats missing (skipped) required checks as pending unless you use a ruleset that allows skipped checks, or a no-op pass-through. Prefer a **ruleset** with “Allow skipped checks” if you keep path filters and require all five jobs.

## Lint rules

| Stack | Rules | CI Lint command |
|-------|--------|-----------------|
| .NET (Auth, Gateway, Notification, Shared) | Root `.editorconfig` + `Api/Directory.Build.props` / `Tests/Directory.Build.props` | `dotnet format whitespace` + `dotnet format style --severity warn` |
| HistoryStore | `Api/HistoryStore/history_store/eslint.config.js` | `npm run lint` |
| Web | `Web/web/eslint.config.js` | `npm run lint` |
