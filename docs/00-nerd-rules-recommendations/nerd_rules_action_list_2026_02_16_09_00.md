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

- [ ] **#2-CR** Implement Polly retry policy in integration services  
  - Add: NuGet package `Polly`
  - Update: GitHubService, HarvestService, AzurePipelinesService
  - Config: 3x retry, exponential backoff, 429/500/503 handling
  - Estimated: **1-2 hours**
  - Risk if delayed: Service cascading failures, poor resilience

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

- [ ] **#4-REC** Consolidate service registration extensions
  - Files: All `ServiceCollectionExtensions.cs` in integration packages
  - Issue: Inconsistent naming and registration patterns
  - Action: Standardize to `AddGitHubIntegration()`, `AddHarvestIntegration()`
  - Add: XML documentation
  - Estimated: **1.5 hours**
  - Status: Ready to implement

- [ ] **#5-REC** Add API documentation to integration services
  - Files: GitHubService.cs, HarvestService.cs, AzurePipelinesService.cs
  - Action: Add XML `/// <summary>`, `/// <param>`, `/// <returns>`, `/// <exception>`
  - Add: Code examples in service READMEs
  - Estimated: **2 hours**
  - Status: Ready to implement

- [ ] **#6-REC** Standardize DTOs and model classes
  - Files: All model files in integration packages
  - Issue: Inconsistent naming conventions, null-safety patterns
  - Action: Apply PascalCase consistently, use `#nullable enable`
  - Add: `[JsonPropertyName]` attributes for API mapping
  - Estimated: **1 hour**
  - Status: Ready to implement

- [ ] **#7-REC** Unified logging strategy across packages
  - All services should accept `ILogger<T>` via DI
  - Define logging categories: "GitHub.API", "Harvest.API", etc.
  - Log entry/exit for public methods (Debug level)
  - Document expected log output in README
  - Estimated: **2 hours**
  - Status: Ready to implement

---

## 🟡 LOW-HANGING FRUITS (Easy Wins) - 3 Items

Quick wins with high value. Can be completed in parallel.

- [ ] **#8-LHF** Add nullable reference type enforcement audit
  - Action: Review all `.cs` files for null-handling improvements
  - Add: Null-coalescing operators where needed
  - Files affected: All public services
  - Estimated: **30 minutes**
  - Blocker: None

- [ ] **#9-LHF** Create CONTRIBUTING.md guidelines
  - Content: Running demo, adding packages, git workflow, version bumping
  - Include: Code review checklist from nerd rules
  - Location: Project root
  - Estimated: **45 minutes**
  - Blocker: None

- [ ] **#10-LHF** Update .gitignore for build artifacts
  - Verify: nupkg/, bin/, obj/, .vs/, .vscode/ excluded
  - Verify: *.user files ignored
  - Estimated: **15 minutes**
  - Blocker: None

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

- [ ] **#23-CLEAN** Mark BlazorServerCircuitHandler as deprecated
  - Update: NuGet PackageCongrats metadata
  - Note: Point users to `TheNerdCollective.Blazor.Reconnect`
  - Estimated: **15 minutes**
  - Timeline: Remove in next major version

- [ ] **#24-CLEAN** Verify no dead code exists
  - Action: Run code analysis tools (Roslyn analyzers)
  - Check: Unused methods, fields, namespaces
  - Estimated: **15 minutes**
  - Status: ✅ Already clean (no issues detected)

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
Progress: 0/3 items

☐ #1-CR  Add test infrastructure
☐ #2-CR  Implement Polly retry policy
☐ #3-CR  Security validation for options
```

**Phase 2 - RECOMMENDED** (Target: Next Sprint)
```
Progress: 0/4 items

☐ #4-REC  Consolidate service registration
☐ #5-REC  Add API documentation
☐ #6-REC  Standardize DTOs
☐ #7-REC  Unified logging
```

**Phase 3 - EASY WINS** (Target: Backlog)
```
Progress: 0/3 items

☐ #8-LHF   Nullable reference type audit
☐ #9-LHF   Create CONTRIBUTING.md
☐ #10-LHF  Update .gitignore
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
