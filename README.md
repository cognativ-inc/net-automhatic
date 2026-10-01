# Senior .NET Backend Assessment

Welcome, and thank you for your time.

This repository contains five short C# exercises. They need no database, no network, and no
external services. Everything runs in memory, and you need no domain knowledge.

## Instructions

* You have approximately **40 minutes**.
* Complete the exercises in order.
* The exercises increase in difficulty. It is normal not to finish every test.
* Prioritize correctness and clear code over clever solutions.
* You may use the .NET standard library.
* No database or external services are required.
* Run the test suite whenever you want to check your progress.

Exercise 5 contains advanced concurrency and cancellation tests (C09-C13). Completing all of
them is not required to demonstrate senior-level performance.

| # | Exercise | Difficulty | Suggested time |
| --- | --- | --- | --- |
| 1 | Identifier normalization | Very easy | 4 min |
| 2 | Configuration completeness | Easy | 6 min |
| 3 | Job state machine | Medium | 8 min |
| 4 | In-memory item query | Medium-hard | 10 min |
| 5 | Concurrent resource loader | Hard | 12 min |

If you run out of time, leave a short comment that describes what you would do next.

## Prerequisites

| Tool | Version |
| --- | --- |
| .NET SDK | `11.0.100-preview.5.26302.115` or a later .NET 11 feature band (see `global.json`) |
| An editor | Visual Studio, JetBrains Rider, or VS Code with C# Dev Kit |

.NET 11 is a preview release. The default stable download does not satisfy `global.json`.
Install the pinned SDK with the official script:

```bash
curl -sSL https://dot.net/v1/dotnet-install.sh -o dotnet-install.sh
bash dotnet-install.sh --version 11.0.100-preview.5.26302.115
rm dotnet-install.sh

export DOTNET_ROOT="$HOME/.dotnet"
export PATH="$DOTNET_ROOT:$PATH"
```

On Windows, use `dotnet-install.ps1` from the same location, or install the SDK from
<https://dotnet.microsoft.com/download/dotnet/11.0>.

Confirm the installation:

```bash
dotnet --version
```

## Run the tests

Restore, build, and run every test from the repository root:

```bash
dotnet test
```

At the start, every test fails with `NotImplementedException`. That is expected.

Run the tests for one exercise:

```bash
dotnet test --filter "FullyQualifiedName~Exercise01"
dotnet test --filter "FullyQualifiedName~Exercise02"
dotnet test --filter "FullyQualifiedName~Exercise03"
dotnet test --filter "FullyQualifiedName~Exercise04"
dotnet test --filter "FullyQualifiedName~Exercise05"
```

Rerun the tests every time you save a file:

```bash
dotnet watch test --project tests/AutoMHatic.Assessment.Tests
```

You can also use the test explorer in your IDE.

## Repository layout

```text
src/AutoMHatic.Assessment/           Your code. One folder per exercise.
  Exercise01/Identifier.cs
  Exercise02/ConfigurationProgress.cs
  Exercise03/Job.cs
  Exercise04/ItemQuery.cs
  Exercise05/ResourceLoader.cs
tests/AutoMHatic.Assessment.Tests/   The tests. Do not change them.
  Exercise01/ ... Exercise05/
```

## How to work

1. Open the exercise file in `src/AutoMHatic.Assessment/ExerciseNN/`. The comment at the top
   of the file describes the task.
2. Open the matching test file in `tests/AutoMHatic.Assessment.Tests/ExerciseNN/`.
3. Make the tests pass in order. Test names start with `C01`, `C02`, and so on. The runner
   executes them in that order, so the first failure it reports is the next case to work on.
4. Move to the next exercise.

Rules:

* Do not change the tests.
* Keep the public members that the tests use. Inside that surface, the design is yours. You
  can add private members, helper types, and files.
* Do not add NuGet packages.
* Write the code you would be comfortable shipping to production.
* You can use the official .NET documentation. Do not use AI assistants unless your
  interviewer says so.

## The exercises

### Exercise 1 - Identifier normalization

Turn free-form text such as `"  Hello World "` into a clean identifier such as
`"hello-world"`, or return `null` when that is not possible.

File: `src/AutoMHatic.Assessment/Exercise01/Identifier.cs`

### Exercise 2 - Configuration completeness

Calculate the percentage of required settings that have a value.

File: `src/AutoMHatic.Assessment/Exercise02/ConfigurationProgress.cs`

### Exercise 3 - Job state machine

Model a background job that allows only a fixed set of status transitions, records every
change with a timestamp, and limits retries.

File: `src/AutoMHatic.Assessment/Exercise03/Job.cs`

### Exercise 4 - In-memory item query

Search, sort, and page a sequence of items, with results that are the same for every
request.

File: `src/AutoMHatic.Assessment/Exercise04/ItemQuery.cs`

### Exercise 5 - Concurrent resource loader

Load expensive resources for many concurrent callers: cache successes, share in-flight
loads, retry transient failures, and handle cancellation.

File: `src/AutoMHatic.Assessment/Exercise05/ResourceLoader.cs`

## When you finish

Stop when the time ends, even if some tests still fail. Then walk your interviewer through
your code. Be ready to explain your decisions, the trade-offs you considered, and what you
would change with more time.
