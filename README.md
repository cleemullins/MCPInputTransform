# MCPInputTransform

A simple Model Context Protocol (MCP) Server in C# that transforms input text based on transformation prompts. Intended for transforming lists and other text into various card formats.

## Features

The server provides a single tool called `TransformText` with the following capabilities:

### TransformText Method

**Parameters:**
- `inputText` (string): The input text to transform
- `transformPrompt` (string): The prompt describing how to transform the text

**Supported Transformations:**
- **Uppercase/Upper case**: Converts text to uppercase
- **Lowercase/Lower case**: Converts text to lowercase  
- **Reverse**: Reverses the character order
- **Word count/Words**: Counts words and lists them
- **Length/Count**: Returns character count
- **Custom prompts**: Applies a default transformation with prompt context

## Usage

### Running the Server

```bash
dotnet run
```

The server communicates via stdin/stdout using the MCP (Model Context Protocol) JSON-RPC format.

### Example Requests

1. **Initialize the server:**
```json
{"jsonrpc": "2.0", "id": 1, "method": "initialize", "params": {}}
```

2. **List available tools:**
```json
{"jsonrpc": "2.0", "id": 2, "method": "tools/list", "params": {}}
```

3. **Transform text to uppercase:**
```json
{"jsonrpc": "2.0", "id": 3, "method": "tools/call", "params": {"name": "TransformText", "arguments": {"inputText": "Hello World", "transformPrompt": "convert to uppercase"}}}
```

4. **Count words:**
```json
{"jsonrpc": "2.0", "id": 4, "method": "tools/call", "params": {"name": "TransformText", "arguments": {"inputText": "This is a test", "transformPrompt": "count words"}}}
```

## Requirements

- .NET 8.0 or higher
- Newtonsoft.Json package (automatically restored)

## Building

```bash
dotnet build
```

## Protocol

This server implements the Model Context Protocol (MCP) specification for communication with AI assistants and other MCP clients.
