using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace CodeMentorAI.API.Services;

public interface IAIService
{
    Task<string> GenerateCodeSuggestion(string code, string language, string context = "");
    Task<string> FindBugs(string code, string language);
    Task<string> RefactorCode(string code, string language);
    Task<string> ExplainCode(string code, string language, string level = "intermediate");
    Task<string> GenerateRoadmap(string userLevel, string[] preferredLanguages, string goals);
    Task<string> CreateCodingChallenge(string difficulty, string topic);
    Task<string> GenerateBuggyCode(string language, string difficulty);
    Task<string> CompleteCode(string incompleteCode, string language);
}

public class GoogleAIService : IAIService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly ILogger<GoogleAIService> _logger;

    public GoogleAIService(HttpClient httpClient, IConfiguration configuration, ILogger<GoogleAIService> logger)
    {
        _httpClient = httpClient;
        _apiKey = configuration["GoogleAI:ApiKey"] ?? throw new ArgumentNullException("GoogleAI:ApiKey not configured");
        _logger = logger;
    }

    public async Task<string> GenerateCodeSuggestion(string code, string language, string context = "")
    {
        var prompt = $@"You are an expert {language} developer and code reviewer. Analyze this code and provide specific, actionable suggestions for improvement.

Code to analyze:
```{language}
{code}
```

Context: {context}

Please provide a detailed analysis in this exact format:

## 🔍 Code Analysis

### 📊 Code Quality Score: [X/10]

### ✨ Strengths:
- [List what's good about the code]

### 🚀 Improvement Suggestions:
1. **[Category]**: [Specific suggestion with example]
2. **[Category]**: [Specific suggestion with example]

### 🛡️ Security & Best Practices:
- [Security considerations]
- [Best practice recommendations]

### 🎯 Performance Optimizations:
- [Performance improvements if applicable]

Keep suggestions practical and include code examples where helpful.";

        return await CallGeminiAPI(prompt);
    }

    public async Task<string> FindBugs(string code, string language)
    {
        var prompt = $@"You are a senior software engineer specializing in bug detection. Analyze this {language} code for bugs, errors, and potential issues.

Code to analyze:
```{language}
{code}
```

Please identify issues in this exact JSON format:

```json
{{
  ""summary"": ""Found X potential issues in your code"",
  ""bugs"": [
    {{
      ""line"": 5,
      ""type"": ""Logic Error"",
      ""severity"": ""High"",
      ""description"": ""Detailed description of the issue"",
      ""fix"": ""How to fix this issue"",
      ""example"": ""Code example of the fix""
    }}
  ],
  ""overallAssessment"": ""Overall code quality assessment""
}}
```

Focus on:
- Syntax errors
- Logic errors  
- Runtime errors
- Performance issues
- Security vulnerabilities
- Type safety issues
- Memory leaks
- Null pointer exceptions

If no bugs are found, return a positive assessment with preventive suggestions.";

        return await CallGeminiAPI(prompt);
    }

    public async Task<string> RefactorCode(string code, string language)
    {
        var prompt = $@"You are an expert {language} developer. Refactor this code to improve readability, maintainability, performance, and follow best practices.

Original Code:
```{language}
{code}
```

Please provide the refactored code in this exact format:

## 🔄 Code Refactoring

### ✨ Refactored Code:
```{language}
[Your improved code here]
```

### 🚀 Improvements Made:
1. **[Category]**: [Explanation of change]
2. **[Category]**: [Explanation of change]

### 📈 Benefits:
- [Benefit 1]
- [Benefit 2]
- [Benefit 3]

### 💡 Additional Recommendations:
- [Future improvement suggestions]

Focus on:
- Code readability and clarity
- Performance optimizations
- Modern language features
- Design patterns
- Error handling
- Code organization";

        return await CallGeminiAPI(prompt);
    }

    public async Task<string> ExplainCode(string code, string language, string level = "intermediate")
    {
        var levelDescriptions = new Dictionary<string, string>
        {
            ["beginner"] = "Explain like I'm completely new to programming. Use simple terms, analogies, and avoid jargon.",
            ["intermediate"] = "Explain with moderate technical detail, assuming basic programming knowledge.",
            ["expert"] = "Provide detailed technical explanation with advanced concepts, patterns, and implementation details."
        };

        var prompt = $@"You are a programming instructor. {levelDescriptions.GetValueOrDefault(level, levelDescriptions["intermediate"])}

Explain this {language} code:

```{language}
{code}
```

Please structure your explanation like this:

## 🧠 Code Explanation ({level} level)

### 🎯 What This Code Does:
[High-level purpose and functionality]

### 🔧 How It Works:
[Step-by-step breakdown]

### 🏗️ Key Concepts:
- **[Concept 1]**: [Explanation]
- **[Concept 2]**: [Explanation]

### 📚 Learning Points:
- [Important takeaway 1]
- [Important takeaway 2]

### 🚀 Next Steps:
[Suggestions for further learning or improvements]

Make it engaging and educational!";

        return await CallGeminiAPI(prompt);
    }

    public async Task<string> GenerateRoadmap(string userLevel, string[] preferredLanguages, string goals)
    {
        var prompt = $@"Create a personalized learning roadmap for a {userLevel} developer.

Preferred Languages: {string.Join(", ", preferredLanguages)}
Goals: {goals}

Generate a comprehensive roadmap in this JSON format:

```json
{{
  ""roadmap"": {{
    ""title"": ""Personalized Learning Path"",
    ""duration"": ""12 weeks"",
    ""description"": ""Tailored roadmap description"",
    ""milestones"": [
      {{
        ""week"": 1,
        ""title"": ""Milestone title"",
        ""description"": ""What you'll learn this week"",
        ""topics"": [""Topic 1"", ""Topic 2""],
        ""projects"": [""Project 1"", ""Project 2""],
        ""resources"": [""Resource 1"", ""Resource 2""],
        ""estimatedHours"": 10
      }}
    ],
    ""skillsToGain"": [""Skill 1"", ""Skill 2""],
    ""careerOutcomes"": [""Outcome 1"", ""Outcome 2""]
  }}
}}
```

Make it practical, achievable, and aligned with current industry demands.";

        return await CallGeminiAPI(prompt);
    }

    public async Task<string> CreateCodingChallenge(string difficulty, string topic)
    {
        var prompt = $@"Create a {difficulty} level coding challenge about {topic}.

Generate a complete challenge in this JSON format:

```json
{{
  ""challenge"": {{
    ""title"": ""Challenge Title"",
    ""description"": ""Detailed problem description"",
    ""difficulty"": ""{difficulty}"",
    ""topic"": ""{topic}"",
    ""timeLimit"": ""30 minutes"",
    ""examples"": [
      {{
        ""input"": ""Example input"",
        ""output"": ""Expected output"",
        ""explanation"": ""Why this output""
      }}
    ],
    ""constraints"": [""Constraint 1"", ""Constraint 2""],
    ""hints"": [""Hint 1"", ""Hint 2""],
    ""testCases"": [
      {{
        ""input"": ""Test input"",
        ""expectedOutput"": ""Expected result"",
        ""isHidden"": false
      }}
    ],
    ""solution"": {{
      ""approach"": ""Solution approach explanation"",
      ""timeComplexity"": ""O(n)"",
      ""spaceComplexity"": ""O(1)"",
      ""code"": ""Sample solution code""
    }}
  }}
}}
```

Make it engaging, educational, and appropriately challenging.";

        return await CallGeminiAPI(prompt);
    }

    public async Task<string> GenerateBuggyCode(string language, string difficulty)
    {
        var prompt = $@"Generate {difficulty} level buggy {language} code for a bug hunt game.

Create realistic, educational bugs that developers commonly make.

Return in this JSON format:

```json
{{
  ""title"": ""Bug Hunt Challenge Title"",
  ""description"": ""What this code is supposed to do"",
  ""buggyCode"": ""Code with intentional bugs"",
  ""correctCode"": ""Fixed version of the code"",
  ""bugs"": [
    {{
      ""line"": 5,
      ""type"": ""Bug Type"",
      ""severity"": ""High/Medium/Low"",
      ""hint"": ""Helpful hint for finding the bug"",
      ""explanation"": ""Detailed explanation of the bug and fix""
    }}
  ],
  ""difficulty"": ""{difficulty}"",
  ""estimatedTime"": ""5-10 minutes""
}}
```

Bug types to include based on difficulty:
- Easy: Syntax errors, typos, simple logic errors
- Medium: Off-by-one errors, scope issues, type mismatches
- Hard: Race conditions, memory leaks, complex logic errors

Make bugs realistic and educational!";

        return await CallGeminiAPI(prompt);
    }

    public async Task<string> CompleteCode(string incompleteCode, string language)
    {
        var prompt = $@"Complete this {language} code by filling in the missing parts:

```{language}
{incompleteCode}
```

Provide the completion in this JSON format:

```json
{{
  ""completedCode"": ""Full working code"",
  ""explanation"": ""Explanation of what was added and why"",
  ""keyPoints"": [""Important point 1"", ""Important point 2""],
  ""alternatives"": [
    {{
      ""approach"": ""Alternative approach name"",
      ""code"": ""Alternative implementation"",
      ""pros"": [""Advantage 1""],
      ""cons"": [""Disadvantage 1""]
    }}
  ]
}}
```

Focus on:
- Writing clean, readable code
- Following best practices
- Adding proper error handling
- Including helpful comments
- Suggesting alternative approaches";

        return await CallGeminiAPI(prompt);
    }

    private async Task<string> CallGeminiAPI(string prompt)
    {
        try
        {
            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.7,
                    topK = 40,
                    topP = 0.95,
                    maxOutputTokens = 2048
                }
            };

            var json = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _logger.LogInformation("🤖 Calling Gemini API...");

            var response = await _httpClient.PostAsync(
                $"https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent?key={_apiKey}",
                content);

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var responseObj = JsonSerializer.Deserialize<JsonElement>(responseJson);
                
                if (responseObj.TryGetProperty("candidates", out var candidates) &&
                    candidates.GetArrayLength() > 0)
                {
                    var firstCandidate = candidates[0];
                    if (firstCandidate.TryGetProperty("content", out var contentProp) &&
                        contentProp.TryGetProperty("parts", out var parts) &&
                        parts.GetArrayLength() > 0)
                    {
                        var firstPart = parts[0];
                        if (firstPart.TryGetProperty("text", out var text))
                        {
                            var result = text.GetString() ?? "No response generated";
                            _logger.LogInformation("✅ Gemini API response received");
                            return result;
                        }
                    }
                }
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("❌ Gemini API error: {StatusCode} - {Content}", response.StatusCode, errorContent);
            }

            return "I apologize, but I'm having trouble processing your request right now. Please try again in a moment.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Exception calling Gemini API");
            return "I'm experiencing technical difficulties. Please try again later.";
        }
    }
}