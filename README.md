# SeptaRail — Next Three Trains

[![Function Build & Deploy](https://github.com/NavneetHegde/SeptaRail/actions/workflows/function-deploy.yml/badge.svg)](https://github.com/NavneetHegde/SeptaRail/actions/workflows/function-deploy.yml)
[![MAUI iOS Build](https://github.com/NavneetHegde/SeptaRail/actions/workflows/maui-ios-build.yml/badge.svg)](https://github.com/NavneetHegde/SeptaRail/actions/workflows/maui-ios-build.yml)

SeptaRail shows the next three SEPTA Regional Rail trains between two stations. It is built from two independently-deployed .NET 10 components, orchestrated locally with .NET Aspire:

- **`src/ClientApp`** — a .NET MAUI cross-platform app (Android, iOS, MacCatalyst, Windows). The mobile/desktop UI.
- **`src/GetTrainsFunction`** — an isolated-worker Azure Function (HTTP, v4) that proxies the public SEPTA "NextToArrive" hackathon API.

The ClientApp never calls SEPTA directly — it always calls the Azure Function, which in turn calls SEPTA.

## Runtime data flow

```mermaid
flowchart LR
    subgraph Client["ClientApp (.NET MAUI)"]
        VM["HomePageViewModel<br/>SearchTrain()"]
        NTF["NextTrainFunction"]
        RS["RestService"]
        VM --> NTF --> RS
    end

    subgraph Azure["Azure Function (isolated worker)"]
        RUN["NextThreeTrainFunction.Run()"]
        HC["GET /api/health<br/>(anonymous)"]
    end

    SEPTA["SEPTA NextToArrive API<br/>www3.septa.org/hackathon"]

    RS -->|"POST {from,to} JSON<br/>(function key in URL)"| RUN
    RUN -->|"GET {from}/{to}/3"| SEPTA
    SEPTA -->|"next 3 trains (JSON)"| RUN
    RUN -->|"200 / 400 / 502 / 499 / 500<br/>(ProblemDetails on error)"| RS

    classDef client fill:#0969da,stroke:#0a3069,stroke-width:1px,color:#ffffff;
    classDef func fill:#8250df,stroke:#3b1e72,stroke-width:1px,color:#ffffff;
    classDef ext fill:#1a7f37,stroke:#0c4a1f,stroke-width:1px,color:#ffffff;
    class VM,NTF,RS client;
    class RUN,HC func;
    class SEPTA ext;
```

The Function returns specific status codes instead of swallowing errors: `400` for an empty/malformed body or missing stations, `502` when SEPTA is unreachable or returns a bad response, `499` if the caller cancels, and `500` for anything unexpected.

## CI/CD workflow

Both components deploy via GitHub Actions on pushes to `release/**` (path-filtered), replacing the old Azure DevOps pipelines.

```mermaid
flowchart TD
    push["push to release/**"]

    push -->|"src/GetTrainsFunction/**<br/>AppHost/** · infra/**"| FW["function-deploy.yml"]
    push -->|"src/ClientApp/**"| MW["maui-ios-build.yml"]

    subgraph FW["function-deploy.yml (ubuntu-latest)"]
        FB["build + test<br/>(XPlat coverage)"]
        FD["azure/login (OIDC)<br/>Aspire CLI<br/>aspire deploy AppHost"]
        FB --> FD
    end

    subgraph MW["maui-ios-build.yml (macos-15)"]
        MB["install MAUI workload<br/>import signing cert + profile"]
        MP["publish iOS .ipa (AdHoc)<br/>upload artifact"]
        MB --> MP
    end

    FD -->|"Bicep under infra/"| AZ["Azure: Function +<br/>storage + ACA env"]
    MP --> ART["septarail-ios artifact (.ipa)"]

    classDef trigger fill:#bf3989,stroke:#5e1d47,stroke-width:1px,color:#ffffff;
    classDef job fill:#0969da,stroke:#0a3069,stroke-width:1px,color:#ffffff;
    classDef out fill:#1a7f37,stroke:#0c4a1f,stroke-width:1px,color:#ffffff;
    class push trigger;
    class FB,FD,MB,MP job;
    class AZ,ART out;
```

## Local development

Run the Function and its dependencies under the Aspire dashboard (Azurite storage + OpenTelemetry), or run the Function on its own:

```bash
# Everything via Aspire (dashboard + Azurite + the Function)
aspire run AppHost/AppHost.cs

# Just the Function (needs Azure Functions Core Tools)
cd src/GetTrainsFunction && func start

# Tests
dotnet test test/GetTrainsFunction.Tests/GetTrainsFunction.Tests.csproj
```

The MAUI app multi-targets and must be built one TFM at a time:

```bash
dotnet build src/ClientApp/ClientApp.csproj -f net10.0-windows10.0.19041.0
dotnet build src/ClientApp/ClientApp.csproj -f net10.0-android
```

See [`CLAUDE.md`](./CLAUDE.md) for architecture details, build gotchas, and the full command reference, and [`docs/plan/`](./docs/plan/) for the Aspire integration plan.
