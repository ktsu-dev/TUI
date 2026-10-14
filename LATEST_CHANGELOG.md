## v2.0.10 (patch)

Changes since v2.0.9:

- Merge remote-tracking branch 'origin/main' into claude/tui-167-refuse-cyclic-add (Claude)
- Guard the run's token source with a lock instead of catching ObjectDisposedException [patch] (Claude)
- Claim only the padding when wrapped text has no column to draw in [patch] (Claude)
- Dispose the token source each run links to the caller's token [patch] (Claude)
- Lay a swapped-back root out at the terminal's current size [patch] (Claude)
- Invalidate on layout property changes and re-arrange on a padding change [patch] (Claude)
- Refuse a second child in BorderElement [patch] (Claude)
- Refuse adding a container to itself or one of its descendants [patch] (Claude)

