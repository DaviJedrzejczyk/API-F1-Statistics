## Mandatory Code Readability, Explicit Typing, and Debugging Standards

All C# code developed or modified by agents must prioritize **clarity, readability, maintainability, and ease of debugging**, following the coding conventions already established in the project.

Compact code is not necessarily good code. Do not reduce the number of lines at the expense of readability or maintainability.

### 1. Mandatory Explicit Typing

- Prefer explicit type declarations for all local variables.
- Avoid using `var`, even when the compiler can easily infer the type.
- Do not use `dynamic` to bypass proper type definitions.
- Avoid `object` when a specific type is known.
- Use the actual types of entities, DTOs, interfaces, collections, and response objects.
- Preserve existing contracts and generic response types.
- Use explicit types whenever they improve readability and debugging.

**Avoid:**

```csharp
var response = await _carDataClient.GetHighSpeedsSession(sessionKey, minimumSpeed);
var drivers = await _driverService.GetAllDriversSession(sessionKey);
var carDatas = response.Itens.ToList();
```

**Prefer:**

```csharp
DataResponse<CarData> carDataResponse = await _carDataClient.GetHighSpeedsSession(sessionKey, minimumSpeed);

DataResponse<Driver> driversResponse = await _driverService.GetAllDriversSession(sessionKey);

List<CarData> carDatas = carDataResponse.Itens.ToList();
```

Exceptions are allowed only when explicit typing is technically impractical or significantly reduces readability.

Anonymous types should be avoided when a concrete DTO or another appropriate type is available.

### 2. Code Spacing and Visual Organization

- Separate logical blocks using blank lines.
- Do not write multiple independent operations on the same line.
- Maintain clear spacing between validations, queries, transformations, and persistence operations.
- Always use curly braces `{ }` for conditional statements and loops, even when the body contains a single statement.
- Avoid excessively compressed code blocks.
- Do not introduce unnecessary blank lines between directly related instructions.
- Follow the indentation and visual organization already established in the project.

**Avoid:**

```csharp
if (!response.HasSuccess) return response;
List<CarData> carDatas = response.Itens.ToList();
if (carDatas.Count == 0) return response;
Response result = await SaveCarDatas(carDatas);
return result;
```

**Prefer:**

```csharp
if (!response.HasSuccess)
{
    return response;
}

List<CarData> carDatas = response.Itens.ToList();

if (carDatas.Count == 0)
{
    return response;
}

Response saveResponse = await SaveCarDatas(carDatas);

return saveResponse;
```

### 3. Debugging-Oriented Development

Code must be structured to allow developers to easily inspect intermediate values during execution.

- Prefer intermediate variables when they represent meaningful processing steps.
- Avoid chaining multiple complex operations into a single expression.
- Do not combine queries, filtering, transformations, and persistence into a single statement.
- Use descriptive variable names that clearly communicate their purpose.
- Structure operations so that breakpoints can be placed at important execution steps.
- Avoid unnecessary temporary variables that provide no meaningful debugging or readability benefits.
- Prefer straightforward control flow over overly compact expressions.

**Avoid:**

```csharp
return ResponseFactory.CreateInstance().CreateSuccessDataResponse(response.Itens.GroupBy(x => x.DriverNumber).Select(x => x.MaxBy(y => y.Speed)!).ToList());
```

**Prefer:**

```csharp
List<CarData> carDatas = response.Itens
    .GroupBy(carData => carData.DriverNumber)
    .Select(driverGroup => driverGroup.MaxBy(carData => carData.Speed)!)
    .ToList();

DataResponse<CarData> successResponse = ResponseFactory.CreateInstance()
    .CreateSuccessDataResponse(carDatas);

return successResponse;
```

### 4. Variable Naming Conventions

- Use descriptive names consistent with the project's domain.
- Avoid generic names such as `obj`, `temp`, `result1`, `data1`, or similar names without a clear justification.
- In LINQ expressions, prefer meaningful parameter names instead of `x`, `y`, `a`, or `b`, especially in complex operations.
- Clearly distinguish external API responses, database responses, and entities intended for persistence.
- Follow the naming conventions established in the existing codebase.
- Keep identifiers in English, consistent with the current project.

### 5. LINQ and Expression Complexity

- Use LINQ when it improves readability and maintainability.
- Avoid long method chains that make execution flow difficult to understand or debug.
- Split complex LINQ operations into meaningful steps when appropriate.
- Prefer `foreach` when processing requires multiple validations, side effects, or complex logic for individual elements.
- Do not automatically replace all LINQ expressions with traditional loops, or vice versa.
- Avoid nested ternary expressions and excessively compact conditional statements.
- Prefer explicit intermediate results over deeply nested expressions.

### 6. Method and Service Organization

- Maintain one primary responsibility per method.
- Separate validation, querying, transformation, and persistence into identifiable logical blocks.
- Extract helper methods when doing so improves readability, reuse, or reduces complexity.
- Do not create unnecessary helper methods solely to reduce method length.
- Preserve the existing project architecture, including Services, DAO, DTOs, Interfaces, Unit of Work, and ResponseFactory.
- Use `async` and `await` consistently for asynchronous operations.
- Preserve established exception handling and response patterns without hiding failures.
- Avoid introducing unnecessary abstractions or additional architectural layers.

### 7. Mandatory Agent Restrictions

Agents must not perform refactoring solely to compact code, reduce line count, or replace explicitly declared types with inferred types.

When creating or modifying a class, the agent must:

1. Inspect existing coding conventions within the same architectural layer.
2. Prioritize explicit typing and descriptive variable names.
3. Organize code with consistent spacing between logical blocks.
4. Keep important processing steps easy to inspect and debug.
5. Avoid unnecessary abstractions, method chaining, and complex expressions.
6. Preserve existing functionality, contracts, and architectural boundaries.
7. Review code formatting and readability before completing the implementation.

**Mandatory priority:** Readability, maintainability, and ease of debugging must take precedence over code conciseness, provided that correctness, security, and required performance are not compromised.

These rules apply to both newly created code and modifications to existing code, while respecting the scope of the assigned task.