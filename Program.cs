using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// OpenAI API ayarları
builder.Services.AddHttpClient("OpenAI", client =>
{
    client.BaseAddress = new Uri("https://api.openai.com/v1/");
    client.DefaultRequestHeaders.Add("Authorization", "Bearer xxxx");
});
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("https://localhost:3000", "https://outlook.office.com") // İzin verilen kaynaklar
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); // Gerekliyse çerezleri destekler
    });
});

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors();
app.MapPost("/write_corporate_text", async (IHttpClientFactory factory, OpenAiRequest prompt) =>
{
    // Request body'den gelen metni al
    var httpClient = factory.CreateClient("OpenAI");

    // OpenAI API'ye istek
    var openAiPayload = new
    {
        model = "gpt-4", // Model seçimi
        messages = new[]
        {
            new
            {
                role = "system",
                content =
                    "Sen profesyonel bir yazım asistanısın. Gönderilen metni kurumsal ve resmi bir dilde yeniden yaz."
            },
            new { role = "user", content = prompt.Prompt }
        },
        max_tokens = 500,
        temperature = 0.7
    };

    var response = await httpClient.PostAsJsonAsync("chat/completions", openAiPayload);

    if (!response.IsSuccessStatusCode)
    {
        return Results.Problem("OpenAI API çağrısı başarısız oldu.");
    }

    var responseBody = await response.Content.ReadFromJsonAsync<OpenAiResponse>();

    var kurumsalMetin = responseBody?.Choices?.FirstOrDefault()?.Message?.Content;

    return Results.Ok(new { OrijinalMetin = prompt, transformedText = kurumsalMetin });
});

app.MapPost("/translator_corporate_text", async (IHttpClientFactory factory, OpenAiRequest prompt) =>
{
    // Request body'den gelen metni al
    var httpClient = factory.CreateClient("OpenAI");

    // OpenAI API'ye istek
    var openAiPayload = new
    {
        model = "gpt-4", // Model seçimi
        messages = new[]
        {
            new
            {
                role = "system",
                content =
                    "Sen profesyonel bir yazım ve tercüme asistanısın. Gönderilen metni kurumsal ve resmi bir dilde ingilizce  yeniden yaz."
            },
            new { role = "user", content = prompt.Prompt }
        },
        max_tokens = 500,
        temperature = 0.7
    };

    var response = await httpClient.PostAsJsonAsync("chat/completions", openAiPayload);

    if (!response.IsSuccessStatusCode)
    {
        return Results.Problem("OpenAI API çağrısı başarısız oldu.");
    }

    var responseBody = await response.Content.ReadFromJsonAsync<OpenAiResponse>();

    var kurumsalMetin = responseBody?.Choices?.FirstOrDefault()?.Message?.Content;

    return Results.Ok(new { OrijinalMetin = prompt, transformedText = kurumsalMetin });
});

app.MapPost("/write_corporate_sample_text", async (IHttpClientFactory factory, OpenAiRequest prompt) =>
{
    // Request body'den gelen metni al
    var httpClient = factory.CreateClient("OpenAI");

    // OpenAI API'ye istek
    var openAiPayload = new
    {
        model = "gpt-4", // Model seçimi
        messages = new[]
        {
            new
            {
                role = "system",
                content =
                    "Sen profesyonel bir yazım asistanısın. Gönderilen anahtar kelimeler ile 500 karakterle kurumsal ve resmi bir dilde metin yaz."
            },
            new { role = "user", content = prompt.Prompt }
        },
        max_tokens = 500,
        temperature = 0.7
    };

    var response = await httpClient.PostAsJsonAsync("chat/completions", openAiPayload);

    if (!response.IsSuccessStatusCode)
    {
        return Results.Problem("OpenAI API çağrısı başarısız oldu.");
    }

    var responseBody = await response.Content.ReadFromJsonAsync<OpenAiResponse>();

    var kurumsalMetin = responseBody?.Choices?.FirstOrDefault()?.Message?.Content;

    return Results.Ok(new { OrijinalMetin = prompt, transformedText = kurumsalMetin });
});

app.UseHttpsRedirection();
app.Run();

// OpenAI API Request Modeli
public record OpenAiRequest
{
    public string Prompt { get; set; }
}

// OpenAI API Cevap Modeli
public class OpenAiResponse
{
    [JsonPropertyName("choices")] 
    public List<Choice> Choices { get; set; }
}

public class Choice
{
    [JsonPropertyName("message")] 
    public Message Message { get; set; }
}

public class Message
{
    [JsonPropertyName("content")] 
    public string Content { get; set; }
}
