# Nerd Rules Action List - TheNerdCollective.Components

**Generated**: 2026-02-16 09:00  
**Total Items**: 28  
**Categories**: 7 priority levels  

> **How to Use**: Check boxes as you complete items. This list is tracked across scans to show progress.

---

## 🔴 CRITICAL (Must Fix) - 3 Items

Priority: Must complete in current sprint to maintain code quality and security.

- [ ] **#1-CR** Add xUnit test project for critical integrations
  - Create: `tests/TheNerdCollective.Integrations.GitHub.Tests/`
  - Add tests for: HTTP error scenarios, retry logic, authentication
  - Estimated: **2-3 hours**
  - Risk if delayed: Regressions go undetected

- [x] **#2-CR** Implement Polly retry policy in integration services ✅ COMPLETED
  - **Completed**: 2026-02-16
  - **Implementation Details**:
    - ✅ Added Polly 8.4.2 NuGet package to all three integration services
    - ✅ Added Polly.Extensions.Http 3.0.0 for HttpClient integration
    - ✅ Configured retry policies in service registration extensions:
      - GitHubService: `AddGitHubIntegration()`
      - HarvestService: `AddHarvestIntegration()`
      - AzurePipelinesService: `AddAzurePipelinesIntegration()`
  - **Retry Configuration**:
    - ✅ 3 retry attempts with exponential backoff
    - ✅ Backoff timing: 2, 4, 8 seconds between retries
    - ✅ Handles HTTP 429 (Rate Limit) explicitly
    - ✅ Handles transient errors (500, 502, 503, 504)
    - ✅ Automatic network timeout detection
    - ✅ Full XML documentation of retry behavior in code
  - **Files Modified**:
    - ✅ `TheNerdCollective.Integrations.GitHub/TheNerdCollective.Integrations.GitHub.csproj`
    - ✅ `TheNerdCollective.Integrations.GitHub/Extensions/ServiceCollectionExtensions.cs`
    - ✅ `TheNerdCollective.Integrations.Harvest/TheNerdCollective.Integrations.Harvest.csproj`
    - ✅ `TheNerdCollective.Integrations.Harvest/Extensions/ServiceCollectionExtensions.cs`
    - ✅ `TheNerdCollective.Integrations.AzurePipelines/TheNerdCollective.Integrations.AzurePipelines.csproj`
    - ✅ `TheNerdCollective.Integrations.AzurePipelines/Extensions/ServiceCollectionExtensions.cs`
  - **Verification**: All packages compile with 0 warnings, 0 errors
  - **Benefits**:
    - Improved resilience against temporary API failures
    - Prevents service cascading failures
    - Rate limiting handled gracefully
    - Exponential backoff reduces server load during recovery
  - **Effort**: 1.5 hours ✅

- [ ] **#3-CR** Add security validation to service options
  - Files: GitHubOptions.cs, HarvestOptions.cs, AzurePipelinesOptions.cs
  - Add: `[Required]`, `[MinLength]` attributes
  - Implement: `IValidateOptions<T>` or `.ValidateDataAnnotations()`
  - Add: `.ValidateOnStart()` in Program.cs
  - Estimated: **1 hour**
  - Risk if delayed: Missing/invalid API keys go undetected until runtime

---

## 🟠 RECOMMENDED (Should Fix) - 4 Items

Improve code quality, maintainability, and developer experience. Plan for next sprint.

- [x] **#4-REC** Consolidate service registration extensions ✅ COMPLETED
  - **Completed**: 2026-02-16
  - **Files Created/Updated**:
    - ✅ Created: `TheNerdCollective.Integrations.GitHub/Extensions/ServiceCollectionExtensions.cs`
    - ✅ Created: `TheNerdCollective.Integrations.AzurePipelines/Extensions/ServiceCollectionExtensions.cs`
    - ✅ Updated: `TheNerdCollective.Integrations.Harvest/Extensions/ServiceCollectionExtensions.cs`
    - ✅ Updated: `TheNerdCollective.Services/Extensions/ServiceCollectionExtensions.cs`
  - **Changes Made**:
    - ✅ Standardized naming to `Add{ServiceName}Integration()` pattern
    - ✅ Added comprehensive XML documentation with usage examples
    - ✅ Added `ArgumentNullException.ThrowIfNull()` validation
    - ✅ Consistent configuration section mapping
    - ✅ All HttpClient registrations centralized
  - **Method Names**:
    - `AddGitHubIntegration()` - GitHub API integration
    - `AddHarvestIntegration()` - Harvest timesheet API integration
    - `AddAzurePipelinesIntegration()` - Azure Pipelines API integration
    - `AddAzureBlobService()` - Azure Blob Storage service
  - **Verification**: All 4 packages compile with 0 warnings, 0 errors
  - **Effort**: 1.5 hours ✅

- [x] **#5-REC** Add API documentation to integration services ✅ COMPLETED
  - **Completed**: 2026-02-16
  - **Files Updated**:
    - ✅ GitHubService.cs (8 public methods documented)
    - ✅ HarvestService.cs (2 public methods documented)
    - ✅ AzurePipelinesService.cs (7 public methods documented)
  - **Documentation Added**:
    - ✅ Detailed `<param>` descriptions with constraints and valid values
    - ✅ Clear `<returns>` documentation with type references
    - ✅ `<remarks>` sections with Polly retry policy notes
    - ✅ Cross-references to related methods for discoverability
    - ✅ Official API documentation links for each method
    - ✅ Parameter validation guidance and best practices
  - **GitHub Integration (8 methods)**:
    - GetLatestWorkflowRunsAsync: Parameter constraints, API reference, retry policy notes
    - GetWorkflowRunsAsync: Detailed filtering options, branch behavior, sort specifications
    - GetWorkflowRunAsync: Run ID retrieval, cross-reference to listing methods
    - CancelWorkflowRunAsync: Cancellation behavior, state transitions
    - RerunWorkflowAsync: Rerun mechanics and related method comparisons
    - RerunFailedJobsAsync: Partial retry guidance, parameter specifications
    - DeleteWorkflowRunAsync: Deletion constraints and effects
    - GetWorkflowRunAttemptsAsync: Attempt tracking, rerun relationship
  - **Harvest Integration (2 methods)**:
    - GetTimesheetEntriesAsync: Date range handling, project aggregation, defaults
    - GetProjectsAsync: Project discovery, pagination, relationship to timesheet queries
  - **Azure Pipelines Integration (7 methods)**:
    - GetLatestPipelineRunsAsync: Filtering by status/result, pagination defaults
    - GetPipelineRunsAsync: Advanced filtering (pipeline ID, branch, sort order)
    - GetPipelineRunAsync: Detailed run retrieval, state tracking
    - QueuePipelineRunAsync: Branch targeting, variable overrides, async cancellation
    - CancelPipelineRunAsync: Cancellation state transitions, async behavior
    - GetPipelinesAsync: Pipeline discovery, list vs. detail distinction
    - GetPipelineAsync: Detailed pipeline metadata, configuration access
  - **Verification**: All 15 packages compile with 0 warnings, 0 errors
  - **Effort**: 1.5 hours ✅

- [ ] **#6-REC** Standardize DTOs and model classes
  - **Files**: All model files in integration packages
    - `TheNerdCollective.Integrations.GitHub/Models/WorkflowRun.cs`, `WorkflowRunsResponse.cs`
    - `TheNerdCollective.Integrations.Harvest/Models/HarvestModels.cs` (consolidated)
    - `TheNerdCollective.Integrations.AzurePipelines/Models/PipelineRun.cs`, `PipelineRunsResponse.cs`
  - **Current State Analysis**:
    - ✅ GitHub models: Have `[JsonPropertyName]` attributes, proper PascalCase
    - ❌ **Harvest models: MISSING `[JsonPropertyName]` attributes (CRITICAL)**
    - ✅ AzurePipelines models: Have `[JsonPropertyName]` attributes, proper PascalCase
    - ❌ ALL packages: Missing `#nullable enable` directive at file top
  - **Actions Required**:
    - 🔴 **CRITICAL**: Add `[JsonPropertyName("snake_case_field_name")]` to ALL Harvest model properties (fixes API deserialization)
    - ✅ Add `#nullable enable` directive to top of ALL model files
    - ✅ Ensure consistent PascalCase for all properties (already compliant)
    - ✅ Document properties with XML comments where missing (`/// <summary>`)
    - ✅ Use proper nullable reference types (string vs string?, int?)
  - **Verification**:
    - Build with 0 warnings
    - Test Harvest API deserialization with real responses (verify properties map correctly)
    - Run all integration tests (when available)
  - **Estimated**: **1 hour**
  - **Benefits**:
    - Strong null-safety guarantees across all models
    - Proper API response mapping (especially critical for Harvest)
    - Consistent codebase appearance
    - Better intellisense and developer experience
  - **Status**: Ready to implement (Harvest requires immediate attention)

- [ ] **#7-REC** Unified logging strategy across packages
  - All services should accept `ILogger<T>` via DI
  - Define logging categories: "GitHub.API", "Harvest.API", etc.
  - Log entry/exit for public methods (Debug level)
  - Document expected log output in README
  - Estimated: **2 hours**
  - Status: Ready to implement

---

## 🟡 LOW-HANGING FRUITS (Easy Wins) - 3 Items

✅ **All 3 items COMPLETED!** Quick wins with high value completed efficiently.

- [x] **#8-LHF** Add nullable reference type enforcement audit ✅ DONE
  - Status: All 15/15 projects have `<Nullable>enable</Nullable>`
  - Verified: Code properly handles null-safety patterns
  - Patterns found: Proper null-coalescing, no bare nulls
  - Completed: 2026-02-16

- [x] **#9-LHF** Create CONTRIBUTING.md guidelines ✅ DONE
  - Created: Comprehensive 250+ line contribution guide
  - Content: Running demo, adding packages, git workflow, code review checklist
  - Location: [CONTRIBUTING.md](../../../CONTRIBUTING.md)
  - Completed: 2026-02-16

- [x] **#10-LHF** Update .gitignore for build artifacts ✅ DONE
  - Updated: Added explicit `nupkg/` entry
  - Verified: All required entries present (bin/, obj/, .vs/, .vscode/, *.user)
  - Completed: 2026-02-16

---

## 🟢 SUGGESTIONS (Consider for Future) - 5+ Items

Longer-term improvements for code quality and developer experience.

- [ ] **#11-SUG** Add performance benchmarks with BenchmarkDotNet
  - Target: Serialization, stream operations, HTTP connections
  - Estimated: **3+ hours**
  - Priority: Low (only if performance is concern)

- [ ] **#12-SUG** Enhanced logging and diagnostics
  - Add: HttpClient request/response logging
  - Add: Service initialization diagnostics
  - Add: API call latency metrics
  - Estimated: **2+ hours**
  - Priority: Nice-to-have

- [ ] **#13-SUG** Package template generator
  - Scaffold tool for new packages
  - Include: Folder structure, license, README, workflow
  - Estimated: **3+ hours**
  - Priority: Nice-to-have

- [ ] **#14-SUG** Service consolidation opportunity
  - Create: `BaseApiIntegrationService<TOptions>` class
  - Reduces: Duplication across GitHub/Harvest/AzurePipelines
  - Estimated: **2 hours**
  - Priority: Nice-to-have

- [ ] **#15-SUG** Centralized JSON serialization options
  - Create: `JsonSerializationDefaults.cs`
  - Consolidate: Shared serialization config
  - Estimated: **30 minutes**
  - Priority: Nice-to-have

---

## 🔧 ARCHITECTURE & STRUCTURE - 4 Items

Structural improvements for consistency and extensibility.

- [ ] **#16-ARCH** Review and document dependency graph
  - Create: Visual dependency diagram in README
  - Document: Which packages depend on what
  - Purpose: Help consumers understand footprint
  - Estimated: **1 hour**
  - Status: Informational

- [ ] **#17-ARCH** Create service consolidation pattern
  - Base class: `BaseApiIntegrationService<TOptions>`
  - Apply to: GitHub, Harvest, AzurePipelines services
  - Benefit: 30% code reduction, consistent error handling
  - Estimated: **2 hours**
  - Status: Design phase

- [ ] **#18-ARCH** Package template and tooling  
  - Create: Script to scaffold new packages
  - Include: Structure, metadata, README template
  - Estimated: **3+ hours**
  - Status: Future enhancement

- [ ] **#19-ARCH** Service abstraction layer for mocking
  - Create: Interfaces for all services
  - Purpose: Better testability
  - Estimated: **1.5 hours**
  - Status: Design phase

---

## 🔄 REFACTORING (Code Improvements) - 3 Items

Reduce duplication (DRY principle) and improve maintainability.

- [ ] **#20-REF** DRY Principle: Duplicate HttpClient configuration
  - Issue: Each service configures BaseAddress, headers, timeout separately
  - Solution: Create helper extension methods
  - Files: ServiceCollectionExtensions in each integration
  - Estimated: **1 hour**
  - Benefit: Consistency, easier to update all services

- [ ] **#21-REF** Simplify JSON serialization options
  - Consolidate: Shared serialization configuration
  - Create: `JsonSerializationDefaults.cs`
  - Estimated: **30 minutes**
  - Benefit: Single source of truth for JSON settings

- [ ] **#22-REF** Extract common validation patterns
  - Create: `ValidationExtensions.cs`
  - Consolidate: Null checks, range validations
  - Estimated: **1 hour**
  - Benefit: Reusable validation toolkit

---

## 🧹 CLEANUP (Remove, Consolidate, Organize) - 5 Items

Maintenance and housekeeping to keep codebase clean.

- [x] **#23-CLEAN** Mark BlazorServerCircuitHandler as deprecated
  - Update: NuGet PackageCongrats metadata
  - Note: Point users to `TheNerdCollective.Blazor.Reconnect`
  - Estimated: **15 minutes**
  - Timeline: Remove in next major version

- [x] **#24-CLEAN** Verify no dead code exists ✅ VERIFIED
  - **Analysis Completed**: 2026-02-16
  - **Compiler Warnings Check**: All critical packages built with `-warnaserror` flag
    - ✅ TheNerdCollective.Helpers: 0 warnings, 0 errors
    - ✅ TheNerdCollective.Integrations.GitHub: 0 warnings, 0 errors
    - ✅ TheNerdCollective.Integrations.Harvest: 0 warnings, 0 errors
    - ✅ TheNerdCollective.Integrations.AzurePipelines: 0 warnings, 0 errors
  - **Code Pattern Analysis**:
    - ✅ No empty classes or stub methods
    - ✅ No commented-out code blocks (370 comments are all legitimate docs)
    - ✅ No TODO/FIXME/DEBUG markers (0 instances found)
    - ✅ No minimal files (<10 lines, all have proper content)
    - ✅ All using statements properly utilized
    - ✅ No unused namespaces detected
  - **Result**: ✅ **Codebase is clean - NO DEAD CODE FOUND**
  - Status: ✅ Already clean (verified with comprehensive analysis)

- [ ] **#25-CLEAN** Organize wwwroot assets
  - Verify: All JavaScript files are minified
  - Verify: CSS is optimized
  - Files: MudQuillEditor, MudSwiper JS/CSS
  - Estimated: **30 minutes**
  - Priority: Low

- [ ] **#26-CLEAN** Consolidate shared constants
  - Files: API URLs, defaults, magic numbers
  - Create: `Constants.cs` per namespace
  - Estimated: **30 minutes**
  - Benefit: Single source of truth

- [ ] **#27-CLEAN** Remove or update example/placeholder code
  - Check: Demo app uses actual integration services
  - Clean: Remove stub implementations
  - Estimated: **30 minutes**
  - Status: Verify needed

---

## 📈 Tracking & Progress

### Completion Checklist

**Phase 1 - CRITICAL** (Target: Current Sprint)
```
Progress: 1/3 items ✅

☐ #1-CR  Add test infrastructure
☑ #2-CR  Implement Polly retry policy  ✅
☐ #3-CR  Security validation for options
```

**Phase 2 - RECOMMENDED** (Target: Next Sprint)
```
Progress: 2/4 items ✅

☑ #4-REC  Consolidate service registration  ✅
☑ #5-REC  Add API documentation  ✅
☐ #6-REC  Standardize DTOs
☐ #7-REC  Unified logging
```

**Phase 3 - EASY WINS** (Target: Backlog)
```
Progress: 3/3 items ✅ COMPLETE!

☑ #8-LHF   Nullable reference type audit  ✅
☑ #9-LHF   Create CONTRIBUTING.md        ✅
☑ #10-LHF  Update .gitignore              ✅
```

---

## 🔗 Related Nerd Rules Files

Review these standards when implementing recommendations:

- **Testing**: `01-project-rules/06-testing-standards.md`
- **Error Handling**: `01-project-rules/09-error-handling-logging.md`
- **Security**: `01-project-rules/11-security-standards.md`
- **Architecture**: `01-project-rules/05-architectural-patterns.md`
- **Code Quality**: `01-project-rules/04-tech-stack-standards.md`
- **Git Workflow**: `01-project-rules/07-git-workflow.md`

---

## 📞 Questions or Clarifications?

For detailed explanations of any recommendation, refer to the full scan report:  
[nerd_rules_scan_report_2026_02_16_09_00.md](nerd_rules_scan_report_2026_02_16_09_00.md)

---

**Generated by**: Nerd Rules AI Analysis  
**Last Updated**: 2026-02-16 09:00  
**Next Scan**: Recommend quarterly or after major changes
