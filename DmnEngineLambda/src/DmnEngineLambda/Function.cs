using Amazon.Lambda.Core;
using net.adamec.lib.common.dmn.engine.parser;
using net.adamec.lib.common.dmn.engine.engine.definition;
using net.adamec.lib.common.dmn.engine.engine.execution.context;
using System.Text.Json;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace DmnEngineLambda;

public class Function
{
    public object FunctionHandler(DmnRequest req, ILambdaContext context)
    {
        context.Logger.LogInformation("Received DMN execution request.");

        if (string.IsNullOrWhiteSpace(req.DmnXml))
        {
            context.Logger.LogError("DMN XML is empty.");
            return new { success = false, error = "DMN XML is required." };
        }

        if (string.IsNullOrWhiteSpace(req.DecisionName))
        {
            context.Logger.LogError("Decision Name is empty.");
            return new { success = false, error = "Decision Name is required." };
        }

        try
        {
            context.Logger.LogInformation($"Parsing DMN XML (Length: {req.DmnXml.Length})...");
            // Defaulting to 1.3ext as it is the most capable parser in the library
            var model = DmnParser.ParseString13ext(req.DmnXml);
            
            context.Logger.LogInformation("Creating DMN Definition...");
            var def = DmnDefinitionFactory.CreateDmnDefinition(model);
            
            context.Logger.LogInformation("Creating Execution Context...");
            var ctx = DmnExecutionContextFactory.CreateExecutionContext(def);

            if (req.Inputs != null)
            {
                context.Logger.LogInformation($"Processing {req.Inputs.Count} input parameters...");
                foreach (var kvp in req.Inputs)
                {
                    var parsedValue = ParseValue(kvp.Value);
                    ctx.WithInputParameter(kvp.Key, parsedValue);
                }
            }

            context.Logger.LogInformation($"Executing Decision '{req.DecisionName}'...");
            var result = ctx.ExecuteDecision(req.DecisionName);

            context.Logger.LogInformation("Execution successful.");
            return new
            {
                success = true,
                outputs = result.FirstResultVariables.ToDictionary(v => v.Name, v => v.Value)
            };
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"Error executing DMN: {ex.Message}");
            context.Logger.LogError(ex.StackTrace);
            return new { success = false, error = ex.Message };
        }
    }

    private object? ParseValue(object? value)
    {
        if (value is JsonElement element)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.String:
                    return element.GetString();
                case JsonValueKind.Number:
                    if (element.TryGetInt32(out var i)) return i;
                    if (element.TryGetInt64(out var l)) return l;
                    if (element.TryGetDouble(out var d)) return d;
                    return element.GetRawText(); // Fallback
                case JsonValueKind.True:
                    return true;
                case JsonValueKind.False:
                    return false;
                case JsonValueKind.Null:
                    return null;
                default:
                    return element.ToString();
            }
        }
        return value;
    }
}

public record DmnRequest(
    string DmnXml,
    string DecisionName,
    Dictionary<string, object>? Inputs
);