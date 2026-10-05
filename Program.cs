#pragma warning disable OPENAI001

using DotNetEnv;
using OpenAI.Responses;


// ==========================
// 1. SETUP
// ==========================

// Load .env file
Env.Load();

// Get API key
string apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")!;

// Create OpenAI client
ResponsesClient client = new(apiKey);


// ==========================
// 2. JSON SCHEMA
// ==========================

BinaryData schema = BinaryData.FromString("""
{
  "type": "object",
  "properties": {
    "customerName": {
      "type": ["string", "null"]
    },
    "Length": {
      "type": "number"
    },
    "Width": {
      "type": "number"
    },
    "Material": {
      "type": "string"
    }
  },
  "required": [
    "customerName",
    "Length",
    "Width",
    "Material"
  ],
  "additionalProperties": false
}
""");


// ==========================
// 3. REQUEST OPTIONS
// ==========================

CreateResponseOptions options = new()
{
    Model = "gpt-5.6-sol",

    Instructions = """
    You are a sales order assistant.

    You receive customer orders written in normal human language
    or as images.

    Extract the order details and convert them into the required
    JSON format.

    Do not invent information that is not present.
    """,

    TextOptions = new ResponseTextOptions
    {
        TextFormat = ResponseTextFormat.CreateJsonSchemaFormat(
            "my_response",
            schema,
            jsonSchemaIsStrict: true
        )
    }
};


// ==========================
// 4. GET IMAGE
// ==========================

Console.Write("Insert image path: ");
string filePath = Console.ReadLine()!;

// Read image file
byte[] imageBytes = File.ReadAllBytes(filePath);

// Convert image into BinaryData
BinaryData imageData = new(imageBytes, "image/png");


// ==========================
// 5. ADD USER MESSAGE
// ==========================

options.InputItems.Add(
    ResponseItem.CreateUserMessageItem(
        new[]
        {
            ResponseContentPart.CreateInputTextPart(
                "Extract the customerName, Length, Width, and Material from this order."
            ),

            ResponseContentPart.CreateInputImagePart(imageData)
        }
    )
);


// ==========================
// 6. SEND REQUEST
// ==========================

ResponseResult response =
    await client.CreateResponseAsync(options);


// ==========================
// 7. PRINT RESULT
// ==========================

Console.WriteLine();
Console.WriteLine("Result:");
Console.WriteLine(response.GetOutputText());