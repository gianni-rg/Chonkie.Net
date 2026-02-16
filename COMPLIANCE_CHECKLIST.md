# Licensing Compliance Checklist for Maintainers

This document provides a checklist for maintaining licensing and contribution compliance in Chonkie.Net.  
This checklist should be reviewed and updated as the project evolves.

## Pre-Release Checklist

Before releasing a new version, ensure:

### License & Legal Files

- [ ] LICENSE file is up-to-date with current copyright years
- [ ] NOTICE file reflects all third-party dependencies
- [ ] CONTRIBUTING.md links are valid and current
- [ ] CODE_OF_CONDUCT.md is unchanged unless updated intentionally

### Dependency Compliance

- [ ] Run dependency license audit: `dotnet list package`
- [ ] Check for any new GPL/AGPL/SSPL licensed dependencies
- [ ] Verify all dependencies use compatible licenses (Apache 2.0, MIT, BSD, etc.)
- [ ] Update NOTICE if any new dependencies added
- [ ] Review parent/transitive dependency tree

### Source Code Headers

- [ ] Core library files have Apache license headers
- [ ] New files added include license headers (per LICENSE_HEADERS.md)
- [ ] No file is missing copyright attribution unintentionally
- [ ] File headers use correct copyright year range

### Documentation Review

- [ ] README.md references license correctly
- [ ] Contributing guidelines are accurately described
- [ ] Examples in documentation are accurate and tested
- [ ] Breaking changes are documented if applicable

### Contributor Recognition

- [ ] Update CONTRIBUTORS.md with new contributors
- [ ] Add significant contributors to release notes
- [ ] Verify contributor names and GitHub usernames are correct
- [ ] Include links to contributor profiles where appropriate

## Ongoing Maintenance

### Month/Quarterly Tasks

- [ ] Review open issues related to licensing or contributions
- [ ] Check if any contributor disputes or concerns have emerged
- [ ] Verify CODE_OF_CONDUCT is being upheld in community interactions
- [ ] Update dependency licenses if packages have major updates

### Annual Review

- [ ] Review and update copyright year range (currently 2025-2026)
- [ ] Consider if license terms need clarification
- [ ] Review Code of Conduct for continued relevance
- [ ] Plan for contributor recognition program
- [ ] Update any legal references if open-source law has changed

## When Processing Pull Requests

### For All PRs

- [ ] Verify contributor has read and agrees to CONTRIBUTING.md
- [ ] Confirm Code of Conduct has been reviewed
- [ ] Check PR template was filled out completely
- [ ] Legal agreement section was acknowledged

### For Code Changes

- [ ] New source files have proper license headers
- [ ] Existing file modifications don't warrant new copyright lines (unless significant)
- [ ] No problematic dependencies are imported
- [ ] Code style matches AGENTS.md guidelines

### For Documentation Changes

- [ ] Changes don't contradict existing license terms
- [ ] Examples are accurate and tested
- [ ] Links to license files are correct
- [ ] Attribution is preserved for external content

### Before Merge

- [ ] All CI checks pass
- [ ] Code review approved
- [ ] All tests pass with good coverage
- [ ] Documentation is updated
- [ ] Contributor is added to CONTRIBUTORS.md (if first-time contributor)

## Handling License-Related Issues

### If a contributor disputes licensing

1. Review the PR history and discussions
2. Check Apache 2.0 Section 5 (Submission of Contributions)
3. Document the issue in a private discussion with maintainers
4. Follow your project's governance on dispute resolution

### If a problematic dependency is discovered

1. Assess whether it's a direct or transitive dependency
2. Check if it's actually used in the published library
3. Plan removal or upgrade in next version
4. Document in CHANGELOG if applicable
5. Communicate with users if it affects their projects

### If Code of Conduct is violated

1. Review CODE_OF_CONDUCT.md enforcement section
2. Follow the graduated response procedure
3. Document incident with dates and references
4. Maintain confidentiality of complainant if requested

## Compliance Automation

### Recommended Tools to Integrate

#### License Header Validation

```powershell
# Apache Rat - checks for license headers in all files
# Install: https://rat.apache.org/
# Run: rat.exe .
```

#### Dependency License Checks

```powershell
# .NET built-in tool
dotnet list package

# Third-party tools
# - NuGet audit
# - FOSSA
# - Black Duck
# - WhiteSource
```

#### Automated Compliance

- Use GitHub Actions to validate license headers on PR
- Use dependabot to monitor dependency updates
- Use FOSSA or similar for continuous license compliance

## Documentation Reference

Key files for license compliance:

- `LICENSE` - Full Apache 2.0 text
- `NOTICE` - Attribution and third-party notices
- `LICENSE_HEADERS.md` - Header templates for C# and Python files
- `CONTRIBUTING.md` - Contribution guidelines with implicit CLA
- `CODE_OF_CONDUCT.md` - Community standards
- `CONTRIBUTORS.md` - Attribution list

## Quick Reference

### Apache 2.0 Key Points

- Permissive license allows commercial use
- Requires notice and changes to be documented
- Includes explicit patent grants
- Contributors implicitly license under those terms
- No formal CLA document needed

### Most Common Questions

**Q: Do I need a formal CLA?**
A: No. Apache 2.0 Section 5 covers this implicitly via pull request submission.

**Q: Can I use GPL code?**
A: No. GPL and Apache 2.0 don't mix well. Use only compatible licenses.

**Q: How do I handle third-party code?**
A: Add proper attribution to NOTICE file and include original license.

**Q: What about transitive dependencies?**
A: Monitor but typically not your responsibility unless they're direct deps.

**Q: Can I change the license?**
A: Only for new versions. Existing code remains under current license.

## Emergency Contacts

If critical licensing issues arise:

1. Document the issue thoroughly
2. Contact project maintainers immediately
3. Do not merge code until resolved
4. Consider consulting FOSS legal resources

## Additional Resources

- [Apache License 2.0 FAQ](https://www.apache.org/foundation/license-faq.html)
- [SPDX License List](https://spdx.org/licenses/)
- [Open Source Initiative Licenses](https://opensource.org/licenses/)
- [Software Package Data Exchange (SPDX)](https://spdx.org/)
