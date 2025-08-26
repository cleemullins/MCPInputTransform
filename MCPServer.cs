using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;

namespace MCPInputTransform;

public class MCPServer
{
    private readonly Dictionary<string, Func<JObject, Task<object>>> _methods;

    public MCPServer()
    {
        _methods = new Dictionary<string, Func<JObject, Task<object>>>();
        RegisterMethods();
    }

    private void RegisterMethods()
    {
        // Register MCP protocol methods
        _methods["initialize"] = HandleInitialize;
        _methods["tools/list"] = HandleToolsList;
        _methods["tools/call"] = HandleToolCall;

        // Register our custom method
        _methods["TransformText"] = HandleTransformText;
    }

    public async Task StartAsync()
    {
        Console.Error.WriteLine("MCP Input Transform Server starting...");
        
        try
        {
            // Read from stdin and write to stdout for MCP communication
            while (true)
            {
                var line = await Console.In.ReadLineAsync();
                if (line == null) break;

                if (string.IsNullOrWhiteSpace(line)) continue;

                try
                {
                    var request = JsonConvert.DeserializeObject<JObject>(line);
                    var response = await ProcessRequest(request);
                    
                    if (response != null)
                    {
                        var responseJson = JsonConvert.SerializeObject(response);
                        Console.WriteLine(responseJson);
                        await Console.Out.FlushAsync();
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Error processing request: {ex.Message}");
                    var errorResponse = new
                    {
                        jsonrpc = "2.0",
                        error = new
                        {
                            code = -32603,
                            message = "Internal error",
                            data = ex.Message
                        }
                    };
                    var errorJson = JsonConvert.SerializeObject(errorResponse);
                    Console.WriteLine(errorJson);
                    await Console.Out.FlushAsync();
                }
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Server error: {ex.Message}");
        }
    }

    private async Task<object?> ProcessRequest(JObject? request)
    {
        if (request == null) return null;

        var method = request["method"]?.ToString();
        var id = request["id"];
        var parameters = request["params"] as JObject;

        if (string.IsNullOrEmpty(method))
        {
            return new
            {
                jsonrpc = "2.0",
                error = new
                {
                    code = -32600,
                    message = "Invalid Request"
                },
                id = id
            };
        }

        if (_methods.TryGetValue(method, out var handler))
        {
            try
            {
                var result = await handler(parameters ?? new JObject());
                return new
                {
                    jsonrpc = "2.0",
                    result = result,
                    id = id
                };
            }
            catch (Exception ex)
            {
                return new
                {
                    jsonrpc = "2.0",
                    error = new
                    {
                        code = -32603,
                        message = "Internal error",
                        data = ex.Message
                    },
                    id = id
                };
            }
        }
        else
        {
            return new
            {
                jsonrpc = "2.0",
                error = new
                {
                    code = -32601,
                    message = "Method not found"
                },
                id = id
            };
        }
    }

    private Task<object> HandleInitialize(JObject parameters)
    {
        return Task.FromResult<object>(new
        {
            protocolVersion = "1.0.0",
            capabilities = new
            {
                tools = new { }
            },
            serverInfo = new
            {
                name = "MCPInputTransform",
                version = "1.0.0"
            }
        });
    }

    private Task<object> HandleToolsList(JObject parameters)
    {
        return Task.FromResult<object>(new
        {
            tools = new[]
            {
                new
                {
                    name = "TransformText",
                    description = "Transforms an input string using a transformation prompt",
                    inputSchema = new
                    {
                        type = "object",
                        properties = new
                        {
                            inputText = new
                            {
                                type = "string",
                                description = "The input text to transform"
                            },
                            transformPrompt = new
                            {
                                type = "string", 
                                description = "The prompt describing how to transform the text"
                            }
                        },
                        required = new[] { "inputText", "transformPrompt" }
                    }
                }
            }
        });
    }

    private async Task<object> HandleToolCall(JObject parameters)
    {
        var name = parameters["name"]?.ToString();
        var arguments = parameters["arguments"] as JObject;

        if (name == "TransformText")
        {
            return await HandleTransformText(arguments ?? new JObject());
        }

        throw new InvalidOperationException($"Unknown tool: {name}");
    }

    private async Task<object> HandleTransformText(JObject parameters)
    {
        var inputText = parameters["inputText"]?.ToString();
        var transformPrompt = parameters["transformPrompt"]?.ToString();

        if (string.IsNullOrEmpty(inputText))
        {
            throw new ArgumentException("inputText parameter is required");
        }

        if (string.IsNullOrEmpty(transformPrompt))
        {
            throw new ArgumentException("transformPrompt parameter is required");
        }

        // For this simple implementation, we'll create a basic transformation
        // In a real implementation, this would likely call an AI service
        var transformedText = await TransformTextAsync(inputText, transformPrompt);

        return new
        {
            content = new[]
            {
                new
                {
                    type = "text",
                    text = transformedText
                }
            }
        };
    }

    private async Task<string> TransformTextAsync(string inputText, string transformPrompt)
    {
        // Simple implementation - in reality this would call an AI service
        // For now, we'll provide a basic transformation based on the prompt
        
        await Task.Delay(100); // Simulate some processing time

        return $"Transformed text based on prompt '{transformPrompt}':\n\n{ApplyBasicTransformation(inputText, transformPrompt)}";
    }

    private string ApplyBasicTransformation(string inputText, string transformPrompt)
    {
        // Basic transformation logic - this is a placeholder for actual AI transformation
        var prompt = transformPrompt.ToLowerInvariant();
        
        if (prompt.Contains("uppercase") || prompt.Contains("upper case"))
        {
            return inputText.ToUpperInvariant();
        }
        else if (prompt.Contains("lowercase") || prompt.Contains("lower case"))
        {
            return inputText.ToLowerInvariant();
        }
        else if (prompt.Contains("reverse"))
        {
            return new string(inputText.Reverse().ToArray());
        }
        else if (prompt.Contains("words") || prompt.Contains("word"))
        {
            var words = inputText.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            return $"Word count: {words.Length}\nWords: {string.Join(", ", words)}";
        }
        else if (prompt.Contains("length") || prompt.Contains("count"))
        {
            return $"Text length: {inputText.Length} characters";
        }
        else
        {
            // Default transformation - add prefix and suffix based on the prompt
            return $"[{transformPrompt}] {inputText} [END]";
        }
    }
}