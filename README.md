# dash-2

A high-speed monochrome Unity action game, developed from [hiwhatsup67/dash](https://github.com/hiwhatsup67/dash). This repository keeps the original layout: the Unity project is at the root and the complete portable Windows game is in `Executable/`.

This snapshot includes the Third Cut trial rules. The last full quality review was **8.3/10** for Second Cut; the newer rules have passed regression/runtime checks but have not received a new full quality rating. See `Docs/DASH-2-SNAPSHOT.md` for verified results and remaining limitations.

## Play

On a Windows PC, choose **Code > Download ZIP** on GitHub, extract the entire ZIP, then open `Executable/DashAndFight.exe`. Keep its data directory, DLLs and Mono runtime beside it. No Unity Editor/account is needed to play. Use the title screen's guide and trial cards for objectives. There are six authored courses and a three-phase final core.

| Trial | Main objective | S-rank mastery requirement |
| --- | --- | --- |
| Slipstream | Clear encounters and take relays | Build a five-kill chain |
| Crosscut | Clear each sector and dash through its ribbon in the marked direction | Three directional gate cuts |
| The Spindle | Return projectiles to break spinner shields | Three counter breaks |
| Razorwire | Dash through directed beam gates; enemy cleanup is optional | Two crossings while beams are live |
| Redline | Reach checkpoints and escape while hunters pursue | Three rusher counters |
| Terminal Velocity | Clear the route and counter a three-phase core | Complete three core phases |

S also requires no damage, the par time, and at least 78% movement time. Ordinary completion unlocks the next trial; S is optional. Redline's S-rank attainability still needs stronger evidence; see the snapshot notes.

| Input | Action |
| --- | --- |
| WASD | Move at full speed |
| Mouse / left button | Aim / hold to shoot |
| Space / right button | Dash; movement input bends the dash |
| Left Shift | Precision movement |
| F | Timed parry; close visible bullets return to enemies |
| R | Instant retry |
| Escape | Pause / resume / back |
| C after death or pause | Checkpoint practice |
| Tab | Larger map |
| Enter | Continue / retry / next course |

The guide is available from the title screen. A guarded sentry blocks frontal shots and cuts: flank it, punish its tell or return a shot. The core opens after a pulse and requires three separate counters. Dash kills refill a charge. Moving grazes, kills and successful parries feed FLOW; overdrive adds speed and stronger shots. Walls remain solid. Hard allows two hits; Nightmare allows one.

## What changed from the 4/10 build

- Following orthographic camera and batched mesh drawing; the distant whole-arena presentation is replaced.
- 20-unit running and 64-unit dash, eight units over 125 ms. Base acceleration reaches speed within 67 ms. Input buffering and simulation run at 120 Hz.
- Controlled dash turning, immediate wall reversals, close parry timing, front guards, angular aim tracking, navigation around cover, graze/overdrive economy and source-specific death explanations.
- Six authored routes: Slipstream, Crosscut, The Spindle, Razorwire, Redline and Terminal Velocity. Sequential encounters replace shared scatter layouts.
- Masked scarf avatar, distinct sentry/spinner/hunter/sniper/rusher/core silhouettes, trails, muzzle flashes, impact rings and restrained shake.
- Original procedural sound design and rhythm score, plus the supplied hurt cue. Event spacing and a listener limiter prevent stacked effects from clipping.
- Sector splits, ranks, best-run ghosts, validated input recordings, checkpoint practice, separate Hard/Nightmare records, volume/music/aim/ghost/flash/shake settings and a control guide.

## Persistence

Version 3 progress is saved separately from earlier versions in `velocity-v3.json` under Unity's persistent data directory (Windows: `%USERPROFILE%\AppData\LocalLow\INACAP\Dash and Fight`). Mode-specific ghost and replay files live beside it. Old recordings are rejected because the trial rules changed. Saves replace atomically with a backup, validate numeric fields and recover from corrupt data. Practice never writes records. Tests use isolated files.

## Reproduce checks

From the project root, run:

```powershell
.\Tools\run-all.ps1
```

The compiler is Roslyn 4.14.0 at `D:\DashWork\roslyn\tasks\net472\csc.exe`; all scripts accept `-Compiler` to override it. On another PC, supply the path to an installed Roslyn compiler, for example `.\Tools\run-all.ps1 -Compiler 'D:\tools\roslyn\csc.exe'`. The compiler is a development dependency and is not needed to play. The pipeline compiles the production model tests, finds ordinary-input completion recordings for six courses, compiles all gameplay source against the exact supplied Unity 6000.6.7f1 Mono player libraries and runs graphical integration/style/audio/performance checks in that real player. A failed suite returns nonzero. Reports are in `TestResults/`.

`build-player.ps1` replaces the managed gameplay assembly and copies StreamingAssets. Geometry and UI are runtime-generated. The engine and serialized assets come from the supplied player; this is not a fresh full Unity asset build. Open `Assets/Scenes/Game.unity` in an activated Unity 6000.6.7f1 Editor for normal editing. `Velocity > Build Windows Player` / `-executeMethod VelocityBuild.Windows` is the full-build entry point. Editor import/full-build validation remains unverified because no license was activated on this machine.

## Inspect a real input recording

```powershell
.\Executable\DashAndFight.exe --dash-replay D:\DashWork\dash\TestResults\stage-6.replay
```

Replay uses the exact production input frames, not a health/position/enemy-state override. Add `--capture-dir D:\DashWork\captures` to export PNG frames and this game's own mixed audio. No microphone or desktop audio is recorded. `--dash-stage 1` opens a course directly. Test-only fixtures that edit state are explicitly identified in the runtime suite; they are not completion witnesses.

## Source map

- `VelocityModel.cs`: fixed-step rules, swept movement/projectiles, encounter definitions, combat, navigation and objectives.
- `VelocityGame.cs`: native input bridge, menus, HUD, progression and practice.
- `VelocityView.cs`: reusable ink mesh, camera, silhouettes and effects.
- `VelocitySound.cs` / `VelocityAudioCapture.cs`: synthesized score/effects, listener dynamics and optional game-only audio capture.
- `VelocityRecording.cs` / `VelocityReplayPlayer.cs`: validated replay data, ghosts and real-time replay.
- `VelocitySave.cs`: validated atomic persistence.
- `VelocityRuntimeTests.cs`, `Tests/VelocityTests.cs`, `Tests/FlowWitness.cs`: engine integration, regressions and ordinary-input recordings.

The original scene/assets/components remain as a reference. `GameManager.Awake` starts this campaign and deactivates the old roots. Tune the new model, not the old `PlayerMovement` / `PlayerJump` scripts.

## Fair limits

Automated checks establish behavior, frame pacing and rendered output. They do not automatically award a quality score. Native desktop capture timed out, and a multi-action UI input sequence failed; only native menu entry and pause were established through snapshots. Continuous hands-on input feel, subjective audio listening and an external playtest are not claimed. The inherited player emits a ComputeBuffer teardown warning after shutdown. Current deductions and the work still needed for 9/10 are recorded in the review.
