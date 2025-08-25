# FamilyAuditorCore - Abstracted Family Validation Library

## Overview

FamilyAuditorCore is now a standalone business logic library that validates family information against configurable rules without direct dependencies on the Revit API. This makes it:

- **Testable**: Can be unit tested without Revit
- **Portable**: Could potentially work with other CAD systems
- **Maintainable**: Clear separation of concerns between business logic and Revit-specific code

## Architecture

### Core Components

1. **Models**
   - `FamilyInfo`: Encapsulates all family data needed for validation
   - `ValidationResult`: Contains validation outcome and detailed error messages
   - `RuleViolation`: Represents a specific rule violation

2. **Abstractions**
   - `IFamilyRuleHandler`: Interface for rule-specific validation logic
   - `IFamilyDataExtractor`: Interface for extracting data from CAD systems
   - `FamilyRuleHandlerFactory`: Creates appropriate rule handlers

3. **Core Classes**
   - `FamilyValidator`: Main orchestrator for family validation
   - `Rule`, `RuleType`, `RulesContainer`: Define validation rules

### How It Works

```
CAD System (Revit) → IFamilyDataExtractor → FamilyInfo → FamilyValidator → ValidationResult
```

## Usage Examples

### Direct Validation with Pre-extracted Data

```csharp
// Create family info (this would come from your data extractor)
var familyInfo = new FamilyInfo
{
    FilePath = "MyFamily.rfa",
    FileSizeBytes = 5 * 1024 * 1024, // 5 MB
    ElementCount = 75,
    ParameterCount = 25,
    ImportedInstanceCount = 0,
    MaterialCount = 3,
    DetailLineCount = 10,
    VertexCount = 200,
    HasElementsWithoutSubcategory = false
};

// Set up rules
var rulesContainer = new RulesContainer("MyProject");
rulesContainer.SetDefaultRules(); // Uses sensible defaults

// Validate
var validator = new FamilyValidator();
var result = validator.ValidateFamily(familyInfo, rulesContainer);

if (!result.IsValid)
{
    foreach (var error in result.ErrorMessages)
    {
        Console.WriteLine($"Validation Error: {error}");
    }
}
```

### Validation with Data Extractor (for Revit integration)

```csharp
// This would be implemented in CollabAPIMEP projects
IFamilyDataExtractor revitExtractor = new RevitFamilyDataExtractor();

// Validate directly from Revit objects
var result = validator.ValidateFamily(
    familyDocument,    // Revit Document object
    @"C:\Families\MyFamily.rfa",
    rulesContainer,
    revitExtractor
);
```

## Available Rules

The library supports these validation rules:

1. **File Size**: Checks if family file size exceeds limit
2. **Element Count**: Validates number of geometry elements
3. **Parameter Count**: Checks number of family parameters
4. **Imported Instances**: Ensures no imported geometry
5. **Subcategory**: Verifies all geometry has subcategories assigned
6. **Material Count**: Limits number of materials in family
7. **Detail Lines**: Controls number of symbolic lines
8. **Vertex Count**: Limits geometric complexity

## Integration with Revit

To integrate with Revit, implement `IFamilyDataExtractor`:

```csharp
public class RevitFamilyDataExtractor : IFamilyDataExtractor
{
    public FamilyInfo ExtractFamilyInfo(object familyDocument, string filePath)
    {
        var doc = (Document)familyDocument;
        
        return new FamilyInfo
        {
            FilePath = filePath,
            FileSizeBytes = GetFileSize(filePath),
            ElementCount = GetElementCount(doc),
            ParameterCount = GetParameterCount(doc.FamilyManager),
            // ... extract other properties
        };
    }
    
    public bool CanProcess(object familyDocument)
    {
        return familyDocument is Document doc && doc.IsFamilyDocument;
    }
    
    // Implementation details...
}
```

## Benefits of This Architecture

1. **No Revit Dependencies**: Core validation logic is independent
2. **Easy Testing**: Can unit test all business logic without Revit
3. **Clear Interfaces**: Well-defined contracts between layers
4. **Extensible**: Easy to add new rules or support other CAD systems
5. **Performance**: Data extraction happens once, rules can be applied multiple times
6. **Error Handling**: Structured error reporting with detailed messages

## Migration from Old Code

The old `IRuleHandler` interface that worked directly with Revit objects:

```csharp
// OLD - Direct Revit dependency
bool IsRuleViolated(Rule rule, string pathname, FamilyManager familyManager, 
                   Document familyDocument, out string errorMessage);
```

Is replaced by the new abstracted interface:

```csharp
// NEW - Works with abstracted data
bool IsRuleViolated(Rule rule, FamilyInfo familyInfo, out string errorMessage);
```

This change eliminates all direct Revit API dependencies from the core validation logic while maintaining the same functionality.