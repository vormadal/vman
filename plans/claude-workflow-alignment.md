# Claude Workflow Alignment Plan

## Problem
The `claude-code-review.yml` workflow runs automatically on PR events (opened, synchronize, ready_for_review, reopened) without any explicit trigger. This conflicts with the stated design in PR #31 which focuses on "@claude mention" workflows.

## Feedback
From review comment 2779935733:
> PR description focuses on an "@claude mention" workflow, but this additional workflow runs automatically on PR open/sync/reopen/ready_for_review and will invoke Claude without any mention trigger. If automatic reviews are intended, document this behavior in the PR description; otherwise, add an `if` condition (or remove this workflow) to align behavior with the stated design.

## Solution
Add an `if` condition to require a label trigger for automatic code reviews. This:
1. Aligns with the mention-based workflow design
2. Gives repository owners control over when automatic reviews run
3. Provides a clear opt-in mechanism (add "claude-review" label to PR)

## Changes
1. Add `if` condition to `claude-review` job that checks for "claude-review" label
2. Update inline comments to document this behavior
3. Keep the workflow file but make it opt-in via label

## Implementation
- Modify `.github/workflows/claude-code-review.yml`
- Add condition: `if: contains(github.event.pull_request.labels.*.name, 'claude-review')`
- Update comments to explain the label-based trigger
