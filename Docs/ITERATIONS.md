# Honest iteration log

## Baseline: v1, played by the user

4/10. The first delivery was correct in many testable ways, but correctness was overemphasized. The distant full-map view made speed feel weaker, repeated spawns and seal positions made encounters generic, and the feedback lacked character. Do not reuse its passing checks as proof of good feel.

## Second Cut: implementation and findings

- Replaced the static map view with a tracking orthographic camera and a batched world mesh. The UI no longer scales the whole arena. The avatar is now a masked head with an animated scarf; six enemy types have different silhouettes.
- Raised normal speed from 14 to 20 and dash from 44 to 64. The new dash travels eight units in 125 ms, can bend with input, and refunds a charge only on a kill. Wall-normal velocity is removed on contact so reversing is responsive.
- Authored six different courses with three sequential encounters, floor routes and individual relays. Added guards, flanking/opening punish windows, rushers, navigation around walls, timed parries, reflected shots, graze rewards and overdrive.
- Added sector splits, personal-best ghosts, recording/replay, checkpoint practice, mode-specific Hard/Nightmare records and a control guide.
- Added original synthesized sounds and a locally generated 168–183 BPM score. Audio capture records only this game's listener, not desktop/microphone input.

### Defects found during the work, not erased from the record

1. An exactly touching wall could trap the player through float rounding. Fixed with a small separation and explicit face-escape tests. Navigation also used a larger starting clearance than the player's body; fixed its near-edge visibility checks.
2. Residual velocity kept pushing into walls after a dash and delayed reversing. Fixed by removing blocked velocity components.
3. A guarded sentry could never turn through exactly 180 degrees when normalized vector interpolation was used. Replaced with bounded angular turning and added a regression.
4. Parry could reflect through a wall and used the pre-reflection endpoint on its first frame. Fixed both and tested them.
5. An early recording killed the final core before its advertised phases mattered. Added shielded post-pulse damage windows and phase health gates. Current completion evidence must include at least three core attacks.
6. The first mixed recording clipped when multiple returned bullets triggered the same sound at once. Added per-event spacing and an actual listener limiter; per-clip signal tests alone were insufficient.
7. A sustained run showed one 43 ms tail frame. Reused navigation storage, reduced tiny-bullet geometry, retained the mesh layout and removed unnecessary HUD allocations. The next dense run had zero frames over 16.67 ms, p99 about 5.1 ms, and a 5.9 ms maximum. Keep measuring after changes.
8. Native graphics capture still timed out, including a legacy swap-chain attempt. Native menu entry and pause were observed through the game's own snapshots. A later multi-action input sequence failed in the UI tool. Continuous hands-on mouse/keyboard feel is not established by those checks. Engine replays are valid rendered gameplay evidence, not a human playtest.
9. A close screenshot review found truncated practice/locked button labels. Widened the controls and added actual font measurement checks to every captured menu/overlay.
10. The first rendered frame of a retry could pan in from outside the room because the camera reset to the player before clamping. Camera reset now uses the same clamped viewport target as normal tracking.

### Current deductions to assess in the next review

- Six short trials still share a three-relay structure. Distinct routes/enemies help, but variety should be judged from successive runs rather than a feature list.
- The early trials are much easier for a predictive controller than the core. That controller is superhuman at visibility/path planning; its results are not human success rates. S-rank times must be reviewed for meaningful challenge.
- The avatar, impact marks and ink hierarchy are more readable, but remain deliberately abstract. Visual coherence and motion need review across whole runs.
- Audio has original generated content and measured signal/mix checks; do not claim a subjective listening review without one.
- An activated Editor license is still unavailable. The actual Unity Mono player is used for testing; a fresh asset rebuild remains unverified.

The 9/10 completion gate remains the one in `QUALITY-RUBRIC.md`. None of these changes automatically awards that score.
