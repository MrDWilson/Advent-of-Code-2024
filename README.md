# Advent of Code 2024 Solutions

Welcome to my solutions for the [Advent of Code 2024](https://adventofcode.com/2024) challenges! This repository contains my personal implementations of the puzzles, primarily using **C#**. Feel free to explore the code, provide feedback, or use it as inspiration for your own solutions.

## Overview

[Advent of Code](https://adventofcode.com/) is an annual programming event that takes place every December. It offers daily coding puzzles that are both fun and challenging, perfect for sharpening problem-solving skills and exploring new algorithms.

This repository serves as:

- A personal coding journal for the Advent of Code 2024.
- A showcase of my coding style and problem-solving approach.
- A resource for anyone interested in C# solutions to these puzzles.

## Getting Started

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) or later installed on your machine.

### Cloning the Repository

```bash
git clone https://github.com/MrDWilson/Advent-of-Code-2024.git
cd Advent-of-Code-2024
```

## Running the Solutions

The application is configured using the `appsettings.json` file. You can specify:

- **Day**: The day's puzzle you want to run.
- **SolutionType**: Which part of the puzzle to execute (`First` or `Second`).
- **RunType**: Whether to run the solution with the actual puzzle input (`Full`) or with test data (`Test`).

### Sample appsettings.json

```json
{
    "Solution": {
        "Day": "5",
        "SolutionType": "Second", // Options: "First" or "Second"
        "RunType": "Full" // Options: "Full" or "Test"
    }
}
```

### Steps to Execute a Solution

#### Configure the Solution

Open the `appsettings.json` file and modify the Day, SolutionType, and RunType values according to the puzzle you want to run.

#### Build and Run

```bash
dotnet run
```

This command will build the project and execute the specified solution.

## Project Structure

- `Solutions/`: Contains individual solution files for each day's puzzle.
- `Inputs/`: Stores input files (.txt) for both test and full runs.
- `Models/`: Includes data models and classes used across different solutions.
- `Services`/: Contains helper services like file loaders and parsers.
- `appsettings.json`: Configuration file to select which solution to run.

## Configuration Details

The application uses the IOptions pattern for configuration, allowing easy access to settings throughout the application.

### `SolutionOptions` Class

```csharp
public class SolutionOptions
{
    public int Day { get; set; }
    public SolutionType SolutionType { get; set; }
    public RunType RunType { get; set; }
}
```

### Enums

```csharp
public enum SolutionType
{
    First,
    Second
}

public enum RunType
{
    Full,
    Test
}
```

## Contributing

While this project is primarily for personal development and learning, contributions are welcome! If you have suggestions for improvements or spot any issues, please feel free to open an issue or submit a pull request.

This project is licensed under the MIT License. See the LICENSE file for details.

## Acknowledgments

- [Advent of Code](https://adventofcode.com/) by Eric Wastl for creating these fantastic challenges.
