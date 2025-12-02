# DMN Engine Lambda

This project implements an AWS Lambda function capable of parsing and executing DMN (Decision Model and Notation) models using the `net.adamec.lib.common.dmn.engine` library. It allows you to evaluate decisions dynamically by providing the DMN XML definition and input parameters.

## Features

- **DMN Parsing**: Supports parsing of DMN 1.3 (and compatible) XML definitions.
- **Decision Execution**: Executes specific decisions within a DMN model.
- **Dynamic Inputs**: Accepts dynamic input parameters for decision evaluation.
- **JSON Integration**: Built with `System.Text.Json` for efficient serialization and deserialization.

## Technology Stack

- **Framework**: .NET 8.0
- **AWS SDK**: `Amazon.Lambda.Core`, `Amazon.Lambda.Serialization.SystemTextJson`
- **DMN Engine**: `net.adamec.lib.common.dmn.engine`

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [AWS CLI](https://aws.amazon.com/cli/) configured with appropriate permissions.
- [Amazon.Lambda.Tools](https://github.com/aws/aws-extensions-for-dotnet-cli) global tool.

```bash
dotnet tool install -g Amazon.Lambda.Tools
```

## Getting Started

### 1. Clone the repository

```bash
git clone <repository-url>
cd dmn-engine
```

### 2. Restore dependencies

```bash
dotnet restore DmnEngineLambda/src/DmnEngineLambda/DmnEngineLambda.csproj
```

## Usage

The Lambda function accepts a JSON payload containing the DMN XML, the decision name to execute, and input parameters.

### Request Payload

| Field | Type | Description |
|-------|------|-------------|
| `DmnXml` | `string` | The full XML content of the DMN model. |
| `DecisionName` | `string` | The `name` (or ID) of the decision to execute. |
| `Inputs` | `object` | Key-value pairs representing input variables for the DMN. |

### Example Request

```json
{
    "DmnXml": "<definitions ...> ... </definitions>",
    "DecisionName": "Greeting",
    "Inputs": {
        "name": "Tester"
    }
}
```

### Response Payload

**Success:**

```json
{
    "success": true,
    "outputs": {
        "greeting": "Hello Tester"
    }
}
```

**Error:**

```json
{
    "success": false,
    "error": "Error message details..."
}
```

## Deployment

To deploy the function to AWS Lambda, navigate to the project directory and use the `dotnet lambda` tool.

```bash
cd DmnEngineLambda/src/DmnEngineLambda
dotnet lambda deploy-function
```

Follow the prompts to select an IAM role and configure the function.

## Testing

To run the unit tests (if available):

```bash
cd DmnEngineLambda/test/DmnEngineLambda.Tests
dotnet test
```

## Local Testing with Mock Tool

You can use the AWS .NET Mock Lambda Test Tool to test the function locally.

1.  Open the project in Visual Studio or VS Code.
2.  Run the project using the "Mock Lambda Test Tool" launch profile.
3.  Paste the example payload into the tool's request window and execute.
