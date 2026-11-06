# gc-NorthwindTraders (gc-port)

Graftcode port of `NorthwindTraders` focused on the customers business slice.

## Testing note

We intentionally rolled back the recent attempt to restore legacy in-repo unit tests for this port. In this GRAFT shape, public entry points are static methods, and the original .NET test architecture does not isolate or mock static boundaries well without intrusive refactoring.

That restoration produced brittle tests with high maintenance cost and low confidence gain. For this port we prioritize integration and E2E verification of the exposed GRAFT surface, where business behavior is validated end-to-end.

## Exposed public surface

- Module: `Northwind.WebUI.dll`
- Type: `Northwind.WebUI.Controllers.CustomersController`
- Methods:
  - `GetCustomers()`
  - `GetCustomerById(string id)`
  - `RegisterCustomer(string id, string companyName, string contactName, string contactTitle, string address, string city, string region, string postalCode, string country, string phone, string fax)`
  - `UpdateCustomer(string id, string companyName, string contactName, string contactTitle, string address, string city, string region, string postalCode, string country, string phone, string fax)`
  - `DeleteCustomer(string id)`

Allowlist file: `Src/Northwind.GraftHost/graft-method-allowlist.json`

## Restored business rules

- `GetCustomerById` validates `id` as required and exact length `5`.
- `RegisterCustomer` restores Application validator constraints:
  - required: `id` (length `5`), `companyName` (max `40`), `postalCode` (max `10`);
  - optional max lengths: `address` `60`, `city` `15`, `contactName` `30`, `contactTitle` `30`, `country` `15`, `fax` `24`, `phone` `24`, `region` `15`.
- `UpdateCustomer` restores Application validator constraints:
  - required: `id` (max `5`), `companyName` (max `40`), `phone` (max `24`), `fax` (max `24`);
  - optional max lengths: `address` `60`, `city` `15`, `contactName` `30`, `contactTitle` `30`, `country` `15`, `postalCode` `10`, `region` `15`;
  - conditional rules: Australia postcode must have 4 digits, and Queensland (`postalCode` starts with `4`) requires at least one `07` landline on `phone` or `fax`.
- `DeleteCustomer` is restored with:
  - required `id` length `5`;
  - not found behavior;
  - existing orders protection when `dbo.Orders` exists.

## Limitations / residual gaps

- Existing-orders delete protection depends on `dbo.Orders` being present in the backing database. The compose seed currently guarantees `Customers` only, so in minimal demo DBs without `Orders`, this rule is skipped intentionally rather than failing every delete call.

## Legacy cleanup - second pass

Removed with high confidence (outside active GRAFT demo path):

- SPA/UI leftovers under `Src/WebUI/ClientApp`.
- Legacy in-repo test surface under `Tests/`.
- Related test project entries from `Northwind.sln`.

Residual candidates intentionally left after second pass:

- No additional high-confidence candidates were found on the active customers GRAFT path.

## Start stack (DB + idempotent seed + GG host)

```powershell
docker compose -f C:\Coding\Marketing\ports\gc-NorthwindTraders\Src\docker-compose.graft.yml up -d --build
```

Ports:

- GG WebSocket/API: `ws://localhost:8096/ws`
- Graft Vision: `http://localhost:8097`
- SQL Server: `localhost,1438`

Stop stack:

```powershell
docker compose -f C:\Coding\Marketing\ports\gc-NorthwindTraders\Src\docker-compose.graft.yml down -v
```

## Build host only

```powershell
dotnet build C:\Coding\Marketing\ports\gc-NorthwindTraders\Src\Northwind.GraftHost\Northwind.GraftHost.csproj -c Release
```

## Client configuration

Set runtime config for generated NuGet client:

```powershell
$env:GRAFTCODE_CONFIG_NUGET_NORTHWIND_WEBUI = "name=graft.nuget.Northwind.WebUI; runtime=netcore; modules=graft.nuget.Northwind.WebUI.dll; host=ws://localhost:8096/ws; stateless=true"
```

Current verified generated package:

- Package ID: `graft.nuget.northwind.webui_9eat7y`
- Version: `1.0.0`
- Feed: `https://grft.dev/c7f25212-2e89-4b01-8e80-6044543df771__free`

## Unstable suffix without `GG_PROJECT_KEY`

This demo host does **not** use `GG_PROJECT_KEY`.

- Every generation can produce a new package suffix (for example `_9eat7y`).
- `gg` logs show the current `dotnet add package ... --source ...` command.
- Update the external verifier (`gc-northwindtraders-graft-e2e`) with that package ID/feed.
- If package ID stays the same, NuGet may not refresh `1.0.0`; clear local cache and restore again.

## External verifier

Path: `C:\Coding\Marketing\gc-northwindtraders-graft-e2e`

Run:

```powershell
dotnet run --project C:\Coding\Marketing\gc-northwindtraders-graft-e2e\gc-northwindtraders-graft-e2e.csproj -c Release -- `
  --host "ws://localhost:8096/ws" `
  --stateless true `
  --report "C:\Coding\Marketing\gc-northwindtraders-graft-e2e\reports\northwindtraders-graft-e2e.json"
```