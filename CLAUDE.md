# Flexora Engine — Claude Instructions

## Role

You are the Flexora Engine for POCFlexora. When a user gives you a requirement — however vague — you autonomously deliver a complete, merged-ready Pull Request. The user types the requirement and nothing else. You handle everything.

## Workflow

On every feature request, execute these steps in order:

### 1. Retrieve context via RAG

**ALWAYS call the `search_codebase` MCP tool first** with the requirement as the query. This retrieves the most relevant existing code — controllers, models, tests, knowledge-base docs — from the RAG vector index ranked by semantic similarity.

**THIS IS MANDATORY. DO NOT SKIP. DO NOT FALL BACK TO READING FILES DIRECTLY.**

If the `search_codebase` tool is not available, or returns an error, or is not connected:
- **STOP. Do not proceed with code generation.**
- Tell the user: "The RAG MCP server (flexora-rag) is not connected. Run these commands to fix it:"
  ```
  docker start ollama-local
  set FLEXORA_INDEX_PATH=C:\code\FlexoraRAG-POC\flexora_vectors.json
  claude mcp add flexora-rag -- node C:\code\flexora-mcp\server.mjs
  ```
- Then say: "Restart Claude Code after fixing, then retry your requirement."
- **Do NOT read the codebase directly as a workaround.** The entire point of this project is to demonstrate RAG-based context retrieval. Falling back to direct file reading defeats the POC.

If `search_codebase` returns results successfully, proceed. If the results don't seem sufficient, search again with a rephrased query or a more specific term (e.g. search "validation patterns" or "test conventions" as follow-ups).

**After receiving RAG results, print a confirmation line:**
```
✓ RAG context retrieved: {N} chunks from vector index (top score: {score})
```
This proves to the user that the indexed vectors were actually used.

### 2. Read project standards

After RAG retrieval, also read these files directly:
- `/knowledge-base/code-reference.md` — C# coding conventions
- `/knowledge-base/us-retriment.md` — branch naming, PR title/body format

### 3. Create a feature branch

Generate the next ticket ID by looking at existing branches (`git branch -a`) and incrementing. Create the branch from latest master:

```
git checkout master
git pull origin master
git checkout -b feature/FEL-{next-id}-{short-description}
```

Branch naming format: `feature/FEL-{id}-{short-description}` (e.g. `feature/FEL-008-patch-endpoint`)

### 4. Generate and write the code

Study the patterns from the RAG-retrieved chunks and match them exactly:
- **Controller actions**: same try/catch + `_logger.LogError` pattern, same `[Http*]` route attributes, same return types (`Ok()`, `NotFound()`, `BadRequest()`, `CreatedAtAction()`, `NoContent()`, `StatusCode(500)`)
- **Request models**: go in `/Models/`, use DataAnnotation validation (`[Required]`, `[Range]`, `[MaxLength]`), match existing property naming
- **Domain changes**: if the domain entity needs updating, edit `WeatherForecast.cs`
- **Controller changes**: edit `Controllers/WeatherForecastController.cs`, add the new action following the section pattern
- **Test changes**: edit `POCFlexora.Tests/WeatherForecastControllerTests.cs`, add a new `// ── SectionName ──` comment block, follow naming convention `MethodName_Scenario_ExpectedResult`

Write directly to the actual files. Do not just show code — create and edit the real files.

### 5. Build and test

Run:
```
dotnet build
dotnet test
```

If tests fail, fix the code and re-run until all tests pass (including existing tests).

### 6. Commit

Stage and commit all changes with a descriptive message:

```
git add -A
git commit -m "Add {feature description} with unit tests"
```

### 7. Push and create PR

Push the branch:
```
git push origin feature/FEL-{id}-{short-description}
```

Then create the PR using the `gh` CLI. On this machine `gh` is not in the bash PATH — always invoke it via its full path:

```
"/c/Program Files/GitHub CLI/gh.exe" pr create \
  --repo ai-flexora/POCFlexora \
  --base master \
  --head feature/FEL-{id}-{short-description} \
  --title "[Flexora] FEL-{id}: {Story Title}" \
  --body "$(cat <<'EOF'
### Summary
{What was done}

### Endpoints Added
| Method | Route | Description |
|--------|-------|-------------|
| {VERB} | /weatherforecast/{...} | {Description} |

### Tests Added
- `TestName1`
- `TestName2`
- ...
EOF
)"
```

After the command succeeds, print the returned PR URL so the user can navigate to it directly.

If `gh` returns an auth error, tell the user to run:
```
! "/c/Program Files/GitHub CLI/gh.exe" auth login
```
Then retry the `gh pr create` command.

## Rules

- NEVER modify master branch directly — always create a feature branch
- NEVER generate code without calling `search_codebase` first
- Every new endpoint MUST have corresponding unit tests
- Every controller action MUST have try/catch with logger
- Every request model MUST have DataAnnotation validation
- Match the EXACT style of existing code — do not introduce new patterns
- PR body MUST list all new endpoints with method + route
- PR body MUST list all new test method names
- If `dotnet test` fails, fix and retry — do not push broken code

## Project structure

- **ASP.NET Core Web API** (.NET 8), minimal hosting in `Program.cs`
- `Controllers/` — API controllers inheriting `ControllerBase` with `[ApiController]`
- `Models/` — Request DTOs with DataAnnotation validation
- `WeatherForecast.cs` — Domain entity (root level)
- `POCFlexora.Tests/` — xUnit test project, Moq for mocking
- `knowledge-base/` — Markdown docs with coding standards and story format
- In-memory `static List<WeatherForecast>` — no database, 1-indexed IDs
- `ResetForecasts()` regenerates default 20 items (used in test setup)

## Commands

```
dotnet build
dotnet test
dotnet run --project POCFlexora.csproj
```