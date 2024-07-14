using System.Text.Json;
using System.Text.Json.Serialization;

namespace Api;

public class AliceService(StatusManager statusManager)
{
    public ResponseDto HandleCommand(AliceCommandDto commandDto)
    {
        statusManager.Wake();

        var command = commandDto.Request.Command;
        var text = command.Length > 0 ? $"Попросила {command}" : "";
        var response = new Response("Готово. " + text, true);
        return new ResponseDto(commandDto.Version, commandDto.Session, response);
    }
}

public record AliceCommandDto
{
    [JsonPropertyName("version")] public required string Version { get; init; }

    [JsonPropertyName("session")] public required JsonElement Session { get; init; }

    [JsonPropertyName("request")] public required Request Request { get; init; }
}

public record Request
{
    [JsonPropertyName("command")] public required string Command { get; init; }
}

public record ResponseDto
{
    [JsonPropertyName("version")] public string Version { get; init; }

    [JsonPropertyName("session")] public JsonElement Session { get; init; }

    [JsonPropertyName("response")] public Response Response { get; init; }

    public ResponseDto(string version, JsonElement session, Response response)
    {
        Version = version;
        Session = session;
        Response = response;
    }
}

public record Response
{
    [JsonPropertyName("text")] public string Text { get; }

    [JsonPropertyName("end_session")] public bool EndSession { get; }

    public Response(string text, bool endSession)
    {
        Text = text;
        EndSession = endSession;
    }
}