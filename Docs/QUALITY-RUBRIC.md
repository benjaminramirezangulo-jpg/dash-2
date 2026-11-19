# Quality gate: 9/10 within the fast, minimal top-down action brief

The user's played assessment of v1 is 4/10. Passing regression tests did not establish good game feel. This rubric is fixed before v2 implementation and must not be loosened to match the result.

| Category | Weight | V1 baseline | A 9 requires |
| --- | --- | --- | --- |
| Speed and flow | 30% | 4 | Fast screen-space movement, continuous purposeful routes, aggressive ability economy, minimal waiting/backtracking, readable camera, no forced hit-stop interrupting control |
| Control and mechanical depth | 20% | 5 | Immediate predictable inputs, steerable collision-safe dash, a meaningful timing mechanic, consistent aiming, explicit feedback for unavailable actions, practice tools |
| Combat and fairness | 15% | 4 | Distinct enemy tells/silhouettes, visible threats, meaningful attack patterns, no unseen unavoidable damage, precise hitboxes, informative deaths |
| Level design and difficulty | 15% | 3 | Six authored challenges with distinct routes, encounter escalation, skill demonstrations, fair recovery windows and challenging medal thresholds; not six arrangements of the same arena |
| Audiovisual feedback | 10% | 3 | Deliberate monochrome hierarchy, legible avatar and HUD, motion/impact animation, responsive layered sounds and coherent music; no visual noise hiding gameplay |
| Replay and completeness | 10% | 4 | Immediate retries, ranks/splits/best ghosts, practice without corrupting records, complete campaign, persistence/settings and clear onboarding |

Weighted v1 baseline: **3.95/10**, consistent with the user's 4/10 playtest.

## Scoring anchors

1: broken or absent. 3: crude prototype. 5: functional but ordinary/repetitive. 7: good with clear flaws. 8: strong with only modest shortcomings. 9: excellent within this focused brief, repeatedly enjoyable and convincing under close inspection. 10: exceptional, no meaningful improvement identified. Feature count alone never earns points.

## Completion rule

- Weighted score at least 9.0; no category below 8.0.
- Scores must cite current code, actual rendered gameplay and concrete play/route observations. Automated correctness is a prerequisite, not the quality rating.
- All new and retained relevant regression/integration checks must pass; all six stages need ordinary-input completion evidence without invulnerability or state edits.
- A sustained graphical stress sample must meet the frame budget; inspect the real player at multiple resolutions.
- Keep a dated iteration log with defects and deductions. Do not erase an observed shortcoming merely because another feature was added.
- Mark unknowns as unknown. Do not claim manual input feel, audio listening, external player validation or an Editor asset rebuild without having performed it.
- If the evidence supports less than 9, keep the goal active and continue improving; do not award 9 to end the task.

## Iteration 0 observations

Tiny player in distant map view. Repeated seal locations and enemy spawn list. Arbitrary map traversal and end-of-stage cleanup. Generic diamonds overlap the player silhouette. Only two enemies actually move, and hunters run straight into walls. Scoring has no split/ghost feedback. The short performance sample and static palette tests do not measure feel. UI descriptions dominate while moment-to-moment feedback is weak. All are deductions to address.
