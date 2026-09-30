#pragma warning disable OPENAI001

using DotNetEnv;
using OpenAI.Responses;

// loads the .env file so we can get the api key from it
Env.Load();

// gets the api key from the .env file
string apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");

// creates the openai client we use to actually talk to the api
ResponsesClient client = new(apiKey);

Console.Write("You: ");

// gets what I typed in the console
string userInput = Console.ReadLine()!;

// this is the json format I want the model to answer in
// in this example I want an answer and a confidence number
BinaryData schema = BinaryData.FromString("""
{
  "type": "object",
  "properties": {
    "customerName": {
      "type": ["string","null"]
    },
    "Length": {
      "type": "number"
    },
    "Width": {
      "type": "number"
    },
    "Material":{
    "type": "string"
    }
  },
  "required": ["customerName", "Length","Width","Material"],
  "additionalProperties": false
}
""");

// here I define the options for the request
CreateResponseOptions options = new()
{
    // which model I want to use
    Model = "gpt-5.6-sol",
    // gives instructions to the model about its role
    Instructions = """
    You are a sales order assistant.

    You receive emails written in normal human language.
    Extract the order details from the email and convert them
    into the required JSON format.

    Do not invent information that is not present in the email.
    """,
    // tells the model I want the answer in the json format I made above
    TextOptions = new ResponseTextOptions
    {
        TextFormat = ResponseTextFormat.CreateJsonSchemaFormat(
            "my_response",
            schema,
            jsonSchemaIsStrict: true
        )
    }
};

// adds what the user typed as the actual user message
options.InputItems.Add( ResponseItem.CreateUserMessageItem(userInput));

// sends the request to openai and waits for the response
ResponseResult response = await client.CreateResponseAsync(options);

// gets only the text part from the full response object and prints it
Console.WriteLine(response.GetOutputText());