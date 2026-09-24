# AutoMHatic - Senior .NET Backend Assessment

Welcome, and thank you for your time.

This repository contains five short C# exercises based on the AutoMHatic Lender Portal,
where loan applications are received, reviewed by a Data Entry team, and submitted to
Encompass, a Loan Origination System. You do not need to know anything about lending. Each
exercise explains the domain it needs.

The exercises get harder as you go. They need no database, no network, and no external
services. Everything runs in memory.

## Time limit

You have **40 minutes** for all five exercises.

We do not expect every candidate to finish. We care more about the quality and reasoning of
the code you write than about the number of green tests. If you run out of time, leave a
short comment that describes what you would do next.

| # | Exercise | Difficulty | Suggested time |
| --- | --- | --- | --- |
| 1 | Phone number normalization | Very easy | 4 min |
| 2 | Review progress indicator | Easy | 6 min |
| 3 | Application lifecycle | Medium | 8 min |
| 4 | Application queue | Medium-hard | 10 min |
| 5 | Submitting to Encompass | Hard | 12 min |

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

## Run the project

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
AutoMHatic.Assessment.slnx
src/AutoMHatic.Assessment/           Your code. One folder per exercise.
  Exercise01/PhoneNumber.cs
  Exercise02/ReviewProgress.cs
  Exercise03/LoanApplication.cs
  Exercise04/ApplicationQueue.cs
  Exercise05/EncompassSubmitter.cs
tests/AutoMHatic.Assessment.Tests/   The tests. Do not change them.
  Exercise01/ ... Exercise05/
```

## How to work

1. Open the exercise file in `src/AutoMHatic.Assessment/ExerciseNN/`. The header at the top
   of the file explains the premise and the task.
2. Open the matching test file in `tests/AutoMHatic.Assessment.Tests/ExerciseNN/`.
3. Make the tests pass **in order**. Test names start with `C01`, `C02`, and so on. Each case
   builds on the previous ones. The runner executes them in that order, so the first failure
   it reports is the next case to work on.
4. Move to the next exercise.

Rules:

* Do not change the tests.
* Keep the public members that the tests use. Inside that surface, the design is yours. You
  can add private members, helper types, and files.
* Use only the .NET base class library. Do not add NuGet packages.
* Write the code you would be comfortable shipping to production.
* You can use the official .NET documentation. Do not use AI assistants unless your
  interviewer says so.

## The exercises

### Exercise 1 - Phone number normalization

Operators and the extraction pipeline type phone numbers in many shapes. Convert any valid
US phone number into its canonical 10-digit form. Return `null` for anything that cannot be
a phone number.

File: `src/AutoMHatic.Assessment/Exercise01/PhoneNumber.cs`

### Exercise 2 - Review progress indicator

The review workbench shows how close a loan application is to being submittable, based on
its required fields. Operators press Submit when they see 100. Calculate that number.

File: `src/AutoMHatic.Assessment/Exercise02/ReviewProgress.cs`

### Exercise 3 - Application lifecycle

A loan application moves through a fixed set of business statuses. Model the application so
that only the agreed transitions are possible, every change is audited with a timestamp, and
submission retries are limited.

File: `src/AutoMHatic.Assessment/Exercise03/LoanApplication.cs`

### Exercise 4 - Application queue

The Application Queue is the operators' landing screen. Implement its query: search, sorting,
and pagination. It must behave the same for every user and every request.

File: `src/AutoMHatic.Assessment/Exercise04/ApplicationQueue.cs`

### Exercise 5 - Submitting to Encompass

Encompass creates a new loan file every time it is called, and several parts of the system
can submit the same application at the same time. Implement a submitter that never creates
a duplicate loan and handles transient failures, rejections, concurrency, and cancellation
correctly.

File: `src/AutoMHatic.Assessment/Exercise05/EncompassSubmitter.cs`

## When you finish

Stop when the 40 minutes end, even if some tests still fail. Then walk your interviewer
through your code. Be ready to explain your decisions, the trade-offs you considered, and
what you would change with more time.
