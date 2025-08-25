# Copilot Instructions for CollabAPIMEP Repository

## Repository Overview

**CollabAPIMEP** is a Family Auditor Revit Plugin that validates Revit family files against configurable rules during the loading process. The plugin integrates with Autodesk Revit (versions 2022-2025) and provides a WPF-based user interface for rule management and validation results.

### High-Level Repository Information
- **Repository Type**: Revit Add-in Plugin Development
- **Primary Languages**: C# (.NET Framework 4.8, .NET Framework 4.7.2, .NET 8)
- **UI Framework**: WPF with XAML
- **Architecture**: MVVM pattern with abstracted business logic
- **Target Platforms**: Windows (Revit 2022, 2023, 2024, 2025)
- **Repository Size**: 7 main projects + unit tests + installer
- **Key Dependencies**: Revit API, WixSharp (installer), NUnit (testing), Firebase Auth

## Build and Development Instructions

### Prerequisites
- Visual Studio 2019 or later
- .NET Framework 4.8 SDK
- .NET 8 SDK (for 2025 version)
- Autodesk Revit installed (for testing and debugging)
- NuGet Package Manager

### Build Process

**IMPORTANT: Always restore NuGet packages before building**
# 1. Restore NuGet packages (ALWAYS run this first)
nuget restore

# 2. Build all projects
msbuild /p:Configuration=Release
# OR for specific configurations:
msbuild /p:Configuration="Release Admin"
msbuild /p:Configuration="Release User"
### Project Configuration Details

The solution uses **conditional compilation symbols** that are critical for proper builds:
- `ADMIN` - Enables administrative features
- `UNIT_TEST` - Enables testing features
- `DEBUG` - Debug builds

**Build Configurations Available:**
- `Release Admin` - Full administrative version
- `Release User` - Limited user version  
- `Debug` - Development version

### Testing

**Unit Tests are integrated with RevitTestFramework:**
# Unit tests require special Revit Test Framework runner
# Tests are in UnitTest project with packages:
# - NUnit 4.1.0
# - RevitTestFramework 1.19.23
# - MSTest.TestAdapter 2.2.10
**Important Testing Notes:**
- Unit tests require Revit to be installed
- Tests use specific test project files: `TestProject1.rvt`, `TestProject2.rvt`
- Test configuration requires `app.config` for assembly binding redirects

### Installation and Packaging

The installer is built using **WixSharp** framework:
# Build installer (run after successful project builds)
cd Installer_WPF
dotnet run
# Generates: FamilyAuditor_Admin.msi and FamilyAuditor_User.msi
## Project Architecture and Layout

### Core Projects Structure
??? CollabAPIMEP_2022/          # Revit 2022 version (.NET Framework 4.8)
??? CollabAPIMEP_2023/          # Revit 2023 version (.NET Framework 4.8)
??? CollabAPIMEP_2024/          # Revit 2024 version (.NET Framework 4.8)
??? CollabAPIMEP_2025/          # Revit 2025 version (.NET 8)
??? FamilyAuditorCore/          # Business logic library (.NET Framework 4.8)
??? UnitTest/                   # Unit tests project
??? Installer_WPF/              # WixSharp installer project
??? CollabAPIMEP/               # Shared code directory
### Key Architecture Components

**1. FamilyAuditorCore (Business Logic)**
- **Purpose**: Abstracted family validation without Revit API dependencies
- **Key Interfaces**: `IFamilyDataExtractor`, `IFamilyRuleHandler`
- **Main Classes**: `FamilyValidator`, `RulesContainer`, `Rule`
- **Location**: `FamilyAuditorCore/`

**2. Revit Integration Layer**
- **Purpose**: Bridges Revit API with business logic
- **Key Classes**: `FamilyLoadHandler`, `RevitFamilyDataExtractor`
- **Location**: `CollabAPIMEP/Services/`

**3. UI Layer (WPF/XAML)**
- **Pattern**: MVVM with `BaseViewModel`, `MainViewModel`
- **Key Files**: `MainWindow.xaml`, `ViewModels/MainViewModel.cs`
- **Styling**: `styles/stylesMerged.xaml`

### Shared Code Structure
CollabAPIMEP/
??? ViewModels/                 # MVVM ViewModels
??? Views/                      # WPF Windows and UserControls
??? Services/                   # Business services and data extraction
??? Models/                     # Data models and request handling
??? Helpers/                    # Utility classes and converters
??? Commands/                   # ICommand implementations
??? styles/                     # XAML styling resources
??? resources/                  # Add-in manifests and test files
### Configuration Files

- **Directory.Build.props**: Global MSBuild properties (sets RevitApiVersion=2024.0.0)
- **packages.config**: NuGet package references (UnitTest project)
- **app.config**: Assembly binding redirects for unit tests
- ***.addin files**: Revit add-in manifest files for each version

### Critical Dependencies

**Revit API References** (version-specific):
- RevitAPI.dll, RevitAPIUI.dll (from Revit installation)

**NuGet Packages:**
- `Microsoft.Xaml.Behaviors.Wpf` v1.1.135 (WPF behaviors)
- `Newtonsoft.Json` v13.0.3 (JSON serialization)
- `CommunityToolkit.Mvvm` v8.3.2+ (MVVM helpers)
- `WixSharp` (installer framework)

## Common Build Issues and Workarounds

### 1. Missing Revit API References
**Problem**: Build fails with missing Revit API references
**Solution**: Ensure Revit is installed and references point to correct installation path

### 2. XAML Behaviors Assembly Not Found
**Problem**: Runtime error about Microsoft.Xaml.Behaviors.dll
**Solution**: The installer includes this assembly from NuGet packages folder:packages/Microsoft.Xaml.Behaviors.Wpf.1.1.122/lib/net462/Microsoft.Xaml.Behaviors.dll
### 3. Conditional Compilation Issues
**Problem**: Features not working as expected
**Solution**: Verify build configuration includes correct defines:
- Admin builds require `ADMIN` symbol
- Test builds require `UNIT_TEST` symbol

### 4. Assembly Binding Redirects
**Problem**: Runtime exceptions with assembly loading
**Solution**: Ensure app.config includes binding redirects for:
- System.Runtime.CompilerServices.Unsafe
- Microsoft.Extensions.Configuration.EnvironmentVariables

## Development Guidelines

### Code Organization Principles
1. **Separation of Concerns**: Business logic in FamilyAuditorCore, Revit integration in Services
2. **MVVM Pattern**: ViewModels handle UI logic, Models contain data, Views are XAML-only
3. **Abstraction**: Use interfaces (`IFamilyDataExtractor`, `IFamilyRuleHandler`) for testability
4. **Version Targeting**: Each Revit version has its own project with shared code

### Adding New Rules
1. Define rule in `FamilyAuditorCore/RuleType.cs`
2. Implement handler in `FamilyAuditorCore/Abstractions/FamilyRuleHandlers.cs`
3. Update rule factory and validation logic
4. Add UI elements if needed

### Testing Strategy
- Core business logic: Unit tests in FamilyAuditorCore
- Revit integration: RevitTestFramework tests in UnitTest project
- UI testing: Manual testing with Revit installed

### Debugging Notes
- **TODO**: Family closing bypasses rules (see FamilyLoadHandler.cs line ~150)
- Debug builds enable detailed logging to Documents/Family Auditor folder
- Event-driven architecture between FamilyLoadHandler and ViewModels

## Instructions Maintenance Protocol

### When to Update These Instructions

**MANDATORY: Update this file (.github/copilot-instructions.md) when making any of the following changes:**

1. **Architectural Changes**:
   - Moving classes between projects (e.g., `FamilyValidator` to different namespace)
   - Renaming core classes (`RulesContainer`, `Rule`, `FamilyLoadHandler`)
   - Changing key interfaces (`IFamilyDataExtractor`, `IFamilyRuleHandler`)
   - Modifying the MVVM pattern implementation

2. **Project Structure Changes**:
   - Adding/removing projects from the solution
   - Changing target frameworks (.NET versions)
   - Modifying shared code organization
   - Adding new Revit version support

3. **Build Process Changes**:
   - New conditional compilation symbols
   - Different build configurations
   - Changed NuGet packages or versions
   - Modified installer generation process

4. **Critical Dependencies Changes**:
   - Upgrading major NuGet packages
   - Changing Revit API version requirements
   - Adding new external dependencies

### How to Update Instructions

**Follow this process when updating instructions:**

1. **Identify Impact**: Determine which sections are affected by your code changes
2. **Update Relevant Sections**: Modify the affected parts of this file
3. **Verify Accuracy**: Ensure all referenced classes, files, and processes still exist
4. **Test Instructions**: Validate that the build/development instructions still work
5. **Commit Together**: Include instruction updates in the same commit as code changes

### Specific Maintenance Examples

**Example 1**: If you rename `FamilyLoadHandler` to `FamilyManager`:
- Update all references in "Key Architecture Components"
- Update the class name in "Debugging Notes" 
- Update any file path references

**Example 2**: If you add a new Revit 2026 project:
- Add new project to "Core Projects Structure"
- Update conditional compilation symbols if needed
- Update installer section if new files are added

**Example 3**: If you change build process to use different NuGet restore command:
- Update "Build Process" section with new commands
- Update "Critical Instructions for Agents"
- Verify and update any workarounds that may be affected

## Critical Instructions for Agents

**ALWAYS:**
1. Run `nuget restore` before any build operations
2. Use appropriate build configuration (Release Admin/User vs Debug)
3. Verify conditional compilation symbols match intended functionality
4. Test changes against specific Revit version if modifying API integration
5. Check assembly binding redirects if adding new NuGet packages
6. **UPDATE THIS INSTRUCTIONS FILE** if making architectural or structural changes

**NEVER:**
1. Remove existing assembly binding redirects without testing
2. Modify Revit API integration without understanding version differences
3. Change shared code without considering impact on all Revit versions
4. Skip testing installer generation if modifying file outputs
5. **Leave instructions outdated** after making structural changes

**Trust these instructions** - this repository has complex multi-version targeting and Revit API integration that requires specific build processes. Only perform additional research if instructions are incomplete or found to be incorrect. **If you make changes that affect the accuracy of these instructions, you MUST update this file accordingly.**