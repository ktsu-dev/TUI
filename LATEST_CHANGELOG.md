## v2.0.10 (patch)

Changes since v2.0.9:

- Guard the run's token source with a lock instead of catching ObjectDisposedException [patch] (Claude)
- Claim only the padding when wrapped text has no column to draw in [patch] (Claude)
- Dispose the token source each run links to the caller's token [patch] (Claude)
- Invalidate on layout property changes and re-arrange on a padding change [patch] (Claude)
- Refuse a second child in BorderElement [patch] (Claude)

