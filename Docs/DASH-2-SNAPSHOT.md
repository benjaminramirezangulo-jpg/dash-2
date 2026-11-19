# dash-2 publication snapshot

Prepared on 19 November 2026. This is a playable development snapshot, not a claim of a completed 9/10 game.

## Included changes after Second Cut

- Distinct chain, directional-ribbon, reflected-shot, beam-breach, pursuit and core trial rules.
- Directional gates require a forward dash within the marked width; walking and reverse crossings do not count.
- A final guard kill and gate crossing can count in the same simulation tick.
- Spinner counter shields resist ordinary shots and dashes; returned fire breaks them.
- Beam warnings begin when the player approaches, rather than activating remotely before the encounter is visible.
- Beam-breach and pursuit trials permit escape without killing every remaining enemy.
- Trial-specific mastery requirements are shown in the HUD and trial cards.
- Version 3 saves and recordings keep earlier rules/records separate.

## Verified results

- 95 deterministic model regression checks passed, zero failures.
- 115 checks passed in the actual Unity Windows player, zero failures. These cover input timing, progression, practice, saves, replay, camera bounds, rendered monochrome output, button text, audio and sustained performance.
- Ordinary-input recordings complete all six trials. The controller does not change health, positions, enemy state, timers or objectives.
- The most recent dense rendering fixture measured 2,279 frames with 376 peak projectiles: about 4.24 ms average, 4.86 ms p99 and 5.98 ms maximum; zero measured frames exceeded 16 ms. This is one machine and fixture, not a universal hardware guarantee. The stress fixture uses invulnerability; completion recordings do not.
- Reports and the six completion recordings are tracked under `TestResults/`.

## Open items

The optional mastery search found no-damage, under-par S recordings for five trials. It did not establish an S recording for Redline in 64 attempts. That is a failed controller search, not proof that the rank is impossible; its counter requirements and practical attainability still need review. The unsuccessful attempts are retained in `TestResults/mastery-search.csv` rather than represented as a passing test.

The last complete rubric review scored Second Cut 8.25/10, displayed as 8.3. These later rule changes have not received a full visual/listening/human-control review, so no new overall rating is assigned.

The supplied Unity 6000.6.7f1 Mono runtime and serialized assets are retained. Gameplay is compiled against its exact libraries; levels, meshes and UI are generated at runtime. A fresh Editor asset build was not validated because the installed Editor had no activated license. An inherited ComputeBuffer warning occurs at shutdown. Continuous hands-on mouse/keyboard feel and subjective audio quality remain unverified.

The `SECOND-CUT-REVIEW.md` and earlier iteration notes are historical checkpoints. Their references to an active quality goal describe that checkpoint, not a current claim of completion.
