## v2.0.10 (patch)

Changes since v2.0.9:

- Move WrapText's line break into a helper to keep it under the complexity limit (Claude)
- Keep a paragraph's indentation when its first word has to be broken [patch] (Claude)
- End a run on OperationCanceledException only when the run was cancelled [patch] (Claude)
- Refuse a cyclic BorderElement.Child before detaching the old one [patch] (Claude)
- Merge remote-tracking branch 'origin/main' into claude/tui-167-refuse-cyclic-add (Claude)
- Guard the run's token source with a lock instead of catching ObjectDisposedException [patch] (Claude)
- Claim only the padding when wrapped text has no column to draw in [patch] (Claude)
- Dispose the token source each run links to the caller's token [patch] (Claude)
- Lay a swapped-back root out at the terminal's current size [patch] (Claude)
- Invalidate on layout property changes and re-arrange on a padding change [patch] (Claude)
- Refuse a second child in BorderElement [patch] (Claude)
- Refuse adding a container to itself or one of its descendants [patch] (Claude)

