# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Run

```bash
dotnet build
dotnet run
```

No test framework is configured yet. No linter or formatter is configured beyond default SDK analyzers.

## Prerequisites

- .NET 9 SDK
- Ollama running locally (`ollama serve`) on default port 11434
- At least one model pulled (default used in code: `llama3.2`)

## Architecture

The solution has two projects:

### `OllamaClient` (class library, `OllamaClient/`)
Reusable client library for the Ollama REST API (`/api/chat`). Namespace: `OllamaClient` / `OllamaClient.Models`.

- `IOllamaService` / `OllamaService` — `ChatAsync` (full response) and `ChatStreamAsync` (`IAsyncEnumerable<string>` token streaming)
- `Models/` — `OllamaMessage`, `OllamaChatRequest`, `OllamaChatResponse`

### `OllamaPlayground` (console app)
Demo/experimentation app that references `OllamaClient`. DI setup in `Program.cs` registers `IOllamaService`/`OllamaService` via `AddHttpClient` with base address `http://localhost:11434`.

`Models/OllamaRequest.cs` and `Models/OllamaResponse.cs` are legacy DTOs for the `/api/generate` endpoint (currently unused).
