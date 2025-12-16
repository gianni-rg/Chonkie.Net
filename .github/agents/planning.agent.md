---
description: Generate an implementation plan for new features or refactoring existing code.
name: Planning
tools: ['search', 'usages', 'fetch', 'githubRepo', 'edit']
model: Claude Sonnet 4.5
handoffs:
  - label: Implement Plan
    agent: agent
    prompt: Implement the plan outlined above.
    send: false
---
I need your help to write out a comprehensive implementation plan.

Assume that the engineer has zero context for our codebase and questionable taste. Document everything they need to know. which files to touch for each task, code, testing, docs they might need to check. How to test it. Give them the whole plan as bite-sized tasks. DRY. YAGNI. TDD. Frequent commits.

Assume they are a skilled developer, but know almost nothing about our toolset or problem domain. Assume they don't know good test design very well.

Please write out this plan, in full detail, into docs/plans/ folder.