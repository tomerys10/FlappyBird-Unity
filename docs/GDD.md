# Game Design Document — Flappy Bird (Unity Remake)

| Field | Value |
| --- | --- |
| Working title | Flappy Bird — Unity Remake |
| Genre | Arcade / endless side-scroller |
| Platform | PC (Windows, macOS, Linux) — Unity standalone |
| Engine | Unity `6000.3.20f1`, Universal Render Pipeline (2D Renderer) |
| Orientation | Landscape, single fixed screen |
| Players | Single player |
| Session length | 10 seconds to a few minutes per run |
| Target audience | All ages; casual players and score chasers |
| Art style | Procedurally generated pixel art, 32 pixels per unit |

---

## 1. Overview

### 1.1 High concept

A one-button endless arcade game. The player keeps a bird airborne by tapping,
and threads it through an unbroken stream of pipe gaps. One touch ends the run.
The whole design is built around a single readable rule and an immediately
restartable failure state, so the player always feels the crash was their own
fault and wants one more attempt.

### 1.2 Design pillars

1. **One input, total clarity.** The player only ever decides *when* to tap.
   Nothing else is under their control, so every outcome is unambiguous.
2. **Instant retry.** Failure costs seconds, not progress. The restart loop is
   short enough that frustration never accumulates.
3. **Readable feedback.** Every event — a point, a milestone, a crash — has a
   distinct visual and audio response so the player never has to guess what
   happened.
4. **Self-contained content.** All art and audio are generated in-editor by
   code, so the project has no external asset dependencies.

### 1.3 Player experience goals

The player should feel tense but never cheated. Difficulty is constant rather
than escalating, which means improvement comes purely from the player's own
timing skill. Milestone messages and medals give that improvement a visible
shape, turning a raw number into a sense of personal progress.

---

## 2. Core gameplay

### 2.1 The loop

```
Ready screen  →  choose bird colour  →  tap to start
      ↑                                       ↓
      │                              Playing: flap, dodge, score
      │                                       ↓
  Restart  ←  Game Over: feathers, score, best, medal
```

### 2.2 Controls

| Action | Input | Notes |
| --- | --- | --- |
| Start run | Left click / `Space` / `Up Arrow` | Same input as flapping |
| Flap | Left click / `Space` / `Up Arrow` | Sets vertical velocity to a fixed value |
| Restart | Click **RESTART** | Only active on the game over panel |

Clicks that land on UI elements are filtered out so pressing a colour swatch does
not accidentally start the run.

### 2.3 Bird physics

The bird sits at a fixed horizontal position. Flapping overwrites the vertical
velocity with a constant upward value rather than adding force, which makes every
tap behave identically regardless of how fast the bird was already falling. This
is what gives the control its precise, predictable feel.

| Property | Value | Purpose |
| --- | --- | --- |
| Flap velocity | `5.6` | Upward speed applied on every tap |
| Gravity scale | `2.7` | Downward acceleration |
| Max fall speed | `-10` | Terminal velocity, keeps falls recoverable |
| Rotate up angle | `28°` | Nose-up tilt just after a flap |
| Rotate down angle | `-90°` | Nose-dive tilt at terminal velocity |
| Rotate lerp | `8` | Smoothing speed between the two tilt extremes |
| Flap animation | `10` fps | Three-frame wing cycle |
| Idle bob | `0.18` amplitude, `3.4` speed | Ready-screen hover, signals "not started" |

Collision uses a circle collider that is deliberately smaller than the drawn
sprite, so near-misses read as skill rather than as unfair hits.

### 2.4 World and obstacles

| Property | Value | Purpose |
| --- | --- | --- |
| Scroll speed | `2.55` units/sec | Pace of the world moving left |
| Pipe spawn interval | `1.35` sec | Horizontal spacing between pairs |
| Pipe gap | `2.25` units | Vertical opening the bird must fit through |
| Gap centre range | `-1.15` to `2.05` | Randomised vertical placement per pair |
| Spawn X | `7.5` | Just off the right edge of the camera |
| Despawn X | `-8.5` | Just off the left edge; the pair returns to the pool |
| Ground Y | `-4.7` | Kill floor, placed at the visible bottom edge |
| Ceiling Y | `4.85` | Kill ceiling, placed at the visible top edge |

Pipe pairs are pooled and recycled rather than instantiated and destroyed, which
keeps allocation flat during long runs.

The ground and ceiling are deliberately aligned to the visible screen edges. An
earlier build placed the kill floor above the visible ground, which made the bird
die in mid-air; the kill volumes are now repositioned at runtime from the config
so they always match what the player can see.

### 2.5 Scoring

One point is awarded the moment the bird crosses the horizontal centre of a pipe
pair. Each pair can only score once per pass. The live score is displayed in
large type at the top of the screen; the best score persists in `PlayerPrefs`
under the key `FlappyBird.BestScore` and is updated whenever a run beats it.

### 2.6 Fail state

Contact with any object tagged `Hazard` — pipes, ground, ceiling, dragon
fireballs — ends the run. On failure the game:

1. Switches state to `GameOver`.
2. Fires the feather burst at the bird's position.
3. Stops pipe spawning and world scrolling.
4. Deactivates the dragon.
5. Lets the bird fall under increased gravity.
6. Plays the hit sound.
7. Shows the game over panel after `0.85` seconds.

The short delay before the panel appears lets the player see the crash and the
feathers land, so the failure feels resolved rather than cut off.

---

## 3. Features

### 3.1 Bird colour selection

The ready screen presents six bird options: yellow, red, orange, green, light
blue and purple. The selected swatch is highlighted with a full-opacity white
frame while the others are dimmed.

The feature is **purely cosmetic and strictly balance-neutral**. All six birds
share the same sprite dimensions, collider radius, mass, flap velocity and
gravity, so the choice cannot make a run easier or harder. The selection is
written to `PlayerPrefs` and restored on the next launch.

### 3.2 Milestone cheers

Every 5 points a bold praise message appears above the bird, scales up, drifts
upward and fades out over `1.15` seconds. It is drawn with an outline so it stays
legible against pipes, and it never overlaps the next gap.

| Score | Message | Colour intent |
| --- | --- | --- |
| 5 | NICE | Warm yellow |
| 10 | WOW | Warm yellow |
| 15 | EPIC | Warm yellow |
| 20 | LEGENDARY | Gold |
| 25 | UNSTOPPABLE | Cyan |
| 30 | GODLIKE | Green |
| 40+ | MYTHIC | Violet |

The escalating wording and colour give the player a sense of rank without adding
any mechanical complexity.

### 3.3 Feather burst

On impact, twelve feather particles spawn at the collision point. Each feather
gets a randomised outward direction, spin rate, scale and lifetime, accelerates
downward under its own gravity, flutters sideways on a sine wave, and fades to
transparent as its life expires.

Feathers are tinted with the player's selected bird colour, so the effect always
matches the bird that just crashed. Like the pipes, the feather pool is
preallocated and reused.

### 3.4 Fireworks and celebration audio

From 10 points onward, each milestone also triggers a firework burst of spark
particles around the bird plus a celebratory arpeggio, layering extra reward on
top of the cheer message for players who are doing well.

### 3.5 Dragon hazard

At 15 points a dragon flies in from the right, tracks the bird's altitude at a
slow follow speed, and fires horizontal fireballs on a timer. Because it follows
slowly and only shoots horizontally, the player can always escape by changing
altitude — the hazard raises tension without introducing unavoidable deaths.

The dragon only activates if its generated art is present. Without it the hazard
is skipped entirely, so the player is never killed by an invisible projectile.

### 3.6 Medals

| Score | Medal |
| --- | --- |
| 10–19 | Bronze |
| 20–29 | Silver |
| 30–39 | Gold |
| 40+ | Platinum |

Medals appear on the game over panel and reflect the score of that run.

---

## 4. Systems architecture

### 4.1 State machine

`GameManager` is a singleton that owns the authoritative game state:

| State | Behaviour |
| --- | --- |
| `Ready` | Bird bobs in place, colour picker visible, world static, waiting for input |
| `Playing` | Gravity active, pipes spawning and scrolling, scoring enabled |
| `GameOver` | Scrolling stopped, bird falling, panel shown after a delay |

### 4.2 Script responsibilities

| Script | Responsibility |
| --- | --- |
| `GameManager` | State machine, score, milestones, restart, scene bootstrapping |
| `GameState` | State enum |
| `GameConfig` | ScriptableObject holding every tuning value |
| `BirdController` | Gravity, flapping, tilt, frame animation, collision reporting |
| `FlapInput` | Input abstraction across mouse and keyboard |
| `PipeSpawner` | Pool management and spawn timing |
| `PipePair` | Per-pair placement, scrolling, scoring trigger |
| `ScrollRepeater` | Seamless looping of the background and ground strips |
| `GameUI` | HUD, ready and game over panels, cheer messages, medals |
| `BirdSelect` | Six-colour picker and its persistence |
| `GameEffects` | Pooled sparks, feathers, fireworks |
| `Dragon`, `Fireball` | Late-game hazard and its projectiles |
| `GameAudio` | Flap, point, hit and death sound playback |
| `SpriteLibrary` | Runtime sprite/material/audio loading with safe fallbacks |

### 4.3 Data-driven tuning

Every number in section 2 lives in `Assets/Data/GameConfig.asset`, a
ScriptableObject. Difficulty can be retuned in the Inspector without touching a
single line of code, which keeps balancing iteration fast.

### 4.4 Robustness decisions

Several defensive choices exist so the project behaves identically on a fresh
clone as it does on the original machine:

- Scene sprites use materials that are reassigned at runtime, because a package
  material that fails to load would otherwise leave objects invisible.
- The bird rebuilds its sprite from `Resources` at runtime and falls back to a
  procedurally drawn sprite if the asset is missing, so it can never disappear.
- Pipes fall back to a code-drawn sprite rather than spawning invisible-but-lethal
  colliders.
- The dragon refuses to activate without its art, preventing invisible kills.
- Kill volumes are repositioned from the config at startup so they always match
  the visible screen edges.

---

## 5. Art and audio

### 5.1 Art

All sprites are generated by `Assets/Editor/GenerateFlappyBirdArt.cs` at a
resolution of 32 pixels per unit, with point filtering and no compression to keep
edges crisp.

| Asset | Size (px) | Notes |
| --- | --- | --- |
| Bird | 34 × 24 | Three flap frames, transparent background, no outline box |
| Pipe | 52 × 320 | Body plus lip, top-centre pivot |
| Background | 576 × 384 | Sky gradient with clouds and hills |
| Ground | 336 × 112 | Scrolling strip |
| Medals | 48 × 48 | Four metal variants |
| Panel | 220 × 140 | Game over backing |
| Dragon | 64 × 48 | Late-game hazard |
| Fireball | 20 × 20 | Dragon projectile |
| Spark | 10 × 10 | Firework and feather particle base |

Camera background is a flat cyan (`78, 192, 202`) matching the sky, with an
orthographic size of `5`.

### 5.2 Audio

Sound effects are synthesised as raw WAV data by the same editor script:

| Clip | Synthesis | Trigger |
| --- | --- | --- |
| `flap` | Short 780 Hz tone | Every flap |
| `point` | Two-tone 980 → 1320 Hz | Passing a pipe pair |
| `hit` | Filtered noise burst | Collision |
| `die` | Falling tone 420 → 110 Hz | Bird hitting the ground |
| `wow` | Four-note arpeggio | Milestone every 5 points |
| `firework` | Layered burst | Fireworks from score 10 |

---

## 6. UI

| Screen | Elements |
| --- | --- |
| Ready | Best score, `TAP / SPACE` prompt, bobbing bird, **CHOOSE BIRD** picker |
| Playing | Large score counter at the top; transient cheer messages |
| Game Over | `GAME OVER`, final score, best score, medal, **RESTART** button |

The HUD is deliberately minimal during play — only the score is permanently
visible — so nothing competes with the pipes for the player's attention.

---

## 7. Scope and future work

### 7.1 Delivered

- Complete ready → playing → game over loop with persistent best score
- Pooled, randomised endless pipe generation
- Six-colour cosmetic bird selection
- Milestone cheer messages with escalating tiers
- Feather burst on impact, tinted to the selected bird
- Fireworks, alternate bird design and dragon hazard as score-gated rewards
- Four-tier medal system
- Fully procedural art and audio pipeline

### 7.2 Possible extensions

- Mobile touch build with resolution-independent UI scaling
- Day / night background variants driven by score
- Local leaderboard with named entries
- Accessibility options for reduced particle effects and adjustable gravity
- Unlockable bird skins tied to medal tiers
