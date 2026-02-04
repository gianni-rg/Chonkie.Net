# Development Notes

This file contains quick development notes and reminders for maintaining the Chonkie.NET project.

## Maintaining Documentation

### Files to Update

When making progress on the port, remember to update these files:

1. **MASTER_ROADMAP.md** - The master plan document
   - Update phase progress (checkboxes)
   - Update progress bars in section 12.1
   - Update current status in section 12.2
   - Add entries to section 12.4 (Recent Changes)

2. **CHANGELOG.md** - Version history
   - Add entries under [Unreleased] as features are completed
   - Move items to versioned releases when publishing

3. **README.md** - Project overview
   - Update status badge
   - Update current phase
   - Add notable features as they're completed

### Update Process

When completing a task or milestone:

```bash
# 1. Update MASTER_ROADMAP.md
#    - Mark task as complete [x]
#    - Update progress percentage
#    - Add entry to "Recent Changes"

# 2. Update CHANGELOG.md
#    - Add entry under [Unreleased] > Added/Changed/Fixed

# 3. Commit changes
git add MASTER_ROADMAP.md CHANGELOG.md README.md
git commit -m "docs: update progress for [task name]"
```

## Progress Tracking Format

### MASTER_ROADMAP.md Progress Bars

Each phase has a 10-segment progress bar using these characters:
- `⬜` - Not started (white)
- `🟦` - In progress (blue)
- `✅` - Complete (green)

Example:
```markdown
Phase 1:  ✅✅✅✅✅🟦⬜⬜⬜⬜  50%  <- 5 complete, 1 in progress, 4 not started
```

### Task Status

Use these conventions:
- `[ ]` - Not started
- `[~]` - In progress (optional, or use bold)
- `[x]` - Complete

### Phase Status Indicators

In phase headers:
- `⬜ NOT STARTED` - Phase hasn't begun
- `🟦 IN PROGRESS` - Currently working on this phase
- `✅ COMPLETE` - Phase finished

## Version Numbers

Follow Semantic Versioning (SemVer):
- `MAJOR.MINOR.PATCH`
- Pre-release: `0.1.0-beta.1`, `0.9.0-rc.1`
- First stable: `1.0.0`

### Milestone Versions

- `0.1.0` - Foundation complete (Week 2)
- `0.5.0` - Mid-project (Week 11)
- `0.9.0` - Release candidate (Week 17)
- `1.0.0` - Stable release (Week 18)

## Quick Commands

### Update Date in Documents

```bash
# Current date format: October 21, 2025
# Update in MASTER_ROADMAP.md:
#   **Last Updated:** [DATE]
# Update in CHANGELOG.md:
#   ### [Version] - [DATE]
```

### Check Markdown Lint

The project may have markdown linting configured. Common issues to watch:
- Blank lines around headings
- Blank lines around lists
- Trailing spaces
- Fenced code block languages

## Git Commit Conventions

Use conventional commits for clear history:

- `feat:` - New feature
- `fix:` - Bug fix
- `docs:` - Documentation changes
- `test:` - Test additions or changes
- `refactor:` - Code refactoring
- `perf:` - Performance improvements
- `chore:` - Maintenance tasks
- `ci:` - CI/CD changes

Examples:
```bash
git commit -m "feat: implement CharacterTokenizer"
git commit -m "docs: update MASTER_ROADMAP.md for Phase 1 progress"
git commit -m "test: add unit tests for Chunk type"
```

## Weekly Review Checklist

At the end of each week:

- [ ] Update MASTER_ROADMAP.md progress percentages
- [ ] Update CHANGELOG.md with completed work
- [ ] Review and update milestone dates if needed
- [ ] Check if README.md status needs updating
- [ ] Commit all documentation updates
- [ ] Review next week's tasks

## Phase Completion Checklist

When completing a phase:

- [ ] All tasks marked as complete `[x]`
- [ ] Progress bar shows 100% `✅✅✅✅✅✅✅✅✅✅`
- [ ] Success criteria reviewed and met
- [ ] Phase status changed to `✅ COMPLETE`
- [ ] Overall progress percentage updated
- [ ] CHANGELOG.md updated with phase achievements
- [ ] Git tag created (for milestone versions)

Example:
```bash
git tag -a v0.1.0 -m "Phase 1: Foundation complete"
git push origin v0.1.0
```

## Resources

### Documentation Templates

When creating new documentation:
- Use existing files as templates
- Maintain consistent formatting
- Include table of contents for long documents
- Use relative links between documents

### Useful Links

- Original Chonkie: https://github.com/chonkie-inc/chonkie
- Chonkie Docs: https://docs.chonkie.ai
- .NET Documentation: https://learn.microsoft.com/dotnet/
- SemVer: https://semver.org/
- Conventional Commits: https://www.conventionalcommits.org/

## Known Issues

### C# 14 Extension Members Compiler Warnings

**Date:** February 4, 2026  
**Affected Files:**
- `src/Chonkie.Core/Extensions/TokenizerExtensions.cs`
- `src/Chonkie.Tokenizers/CharacterTokenizer.cs`
- `src/Chonkie.Tokenizers/WordTokenizer.cs`

**Issue:**
The C# compiler is reporting warnings suggesting to make instance extension members `static`, which is incorrect according to Microsoft's C# 14 documentation. The syntax used in the codebase follows the official C# 14 extension member specification:

```csharp
extension(ITokenizer tokenizer)  // Instance extensions - NOT static
{
    public string TokenizerName => ...;  // Correct: no static modifier
    public bool IsEmpty(string text) { ... }  // Correct: no static modifier
}

extension(ITokenizer)  // Static extensions
{
    public static int MaxTokenLength => ...;  // Correct: static modifier
}
```

**Impact:**
- The code compiles but shows warnings in the IDE
- This appears to be a tooling issue with C# 14 preview support or a Roslyn compiler issue
- The functionality works correctly at runtime

**Resolution:**
These warnings can be safely ignored. They are false positives. The code follows the official Microsoft documentation for C# 14 extension members:
- [Extension members documentation](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/extension-methods)
- [C# 14 What's New](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-14#extension-members)

**Workaround:**
If the warnings become problematic, we can temporarily revert to traditional extension method syntax using the `this` parameter, but this would lose the C# 14 features like extension properties and static extension members.

---

**Last Updated:** February 4, 2026
