# Flappy Bird — Unity Remake

A complete remake of the classic *Flappy Bird* arcade game, built from scratch in
Unity 6 (`6000.3.20f1`) with the Universal Render Pipeline in 2D mode.

You control a small bird that is constantly pulled down by gravity. Every tap
pushes it upward, and the world scrolls past from right to left. Endless pairs of
green pipes come toward you, each pair separated by a narrow gap. Steering the
bird through a gap scores one point; touching a pipe or hitting the ground ends
the run immediately. There are no lives and no levels — the only goal is to beat
your own best score, which the game stores permanently on your machine.

On top of the original formula, this version adds a bird colour selector, a
feather burst on impact, and milestone praise messages that appear as you climb
the scoreboard. Every sprite and sound effect in the project is generated
procedurally by an editor script, so the repository contains no third-party art.

---

## Table of contents

- [How to play](#how-to-play)
- [Choosing your bird](#choosing-your-bird)
- [Core gameplay: flying through the pipes](#core-gameplay-flying-through-the-pipes)
- [The score counter](#the-score-counter)
- [Milestone cheers](#milestone-cheers)
- [Crashing: the feather burst](#crashing-the-feather-burst)
- [Progression and rewards](#progression-and-rewards)
- [Running the project](#running-the-project)
- [Project structure](#project-structure)
- [Design document](#design-document)

---

## How to play

| Action | Input |
| --- | --- |
| Start a run | Left mouse click, `Space`, or `Up Arrow` |
| Flap (fly upward) | Left mouse click, `Space`, or `Up Arrow` |
| Restart after a crash | Click the **RESTART** button |

The bird never moves horizontally — the world moves instead. Your only control is
*when* to flap, and the entire challenge comes from timing those taps so the bird
lines up with the next gap.

---

## Choosing your bird

![Bird selection screen](docs/images/ready-screen.png)

Before the first flap, the ready screen shows a **CHOOSE BIRD** row with six
selectable birds. Each option is a different colour — yellow, red, orange, green,
light blue and purple — and the currently selected one is marked with a bright
white outline.

All six birds are purely cosmetic. **They are identical in size, hitbox, weight
and flight behaviour**, so the choice never makes the game easier or harder. It
is there to let you personalise your run. Your pick is saved locally, so the game
remembers your favourite bird the next time you play.

The bird also gently bobs up and down on this screen, which signals that the game
is waiting for your first input rather than already running.

---

## Core gameplay: flying through the pipes

![Flying between the pipes](docs/images/gameplay-pipes.png)

This is the heart of the game. Pipes arrive in pairs — one hanging from the top
of the screen, one rising from the bottom — with a gap between them. The vertical
position of that gap is randomised for every pair, so no two runs are the same.

Your objective is to **guide the bird cleanly through each gap without touching
anything**. As shown above, the bird has to thread the narrow opening between the
upper and lower pipe.

The run ends the moment the bird touches:

- the body or lip of any pipe,
- the ground at the bottom of the screen, or
- the ceiling at the top of the screen.

The bird also tilts as it moves: it angles upward right after a flap and rotates
nose-down as it falls, which gives you a readable visual cue about your current
vertical speed.

---

## The score counter

![Score counter during play](docs/images/score-hud.png)

The large number at the top of the screen is your **live score**. It counts how
many pipe pairs you have successfully passed so far in the current run — in the
screenshot above, the bird has cleared one pipe pair, so the counter reads `1`.

A point is awarded the instant the bird passes the horizontal centre of a pipe
pair, and a short chime plays to confirm it. When the run ends, the game over
panel shows your final score next to your all-time **BEST** score. If you beat
your previous record, the new value is saved automatically and will still be
there the next time you launch the game.

---

## Milestone cheers

![NICE milestone message](docs/images/cheer-nice.png)

**Every 5 points, a praise message pops up on screen** to celebrate your progress
and keep you pushing for one more pipe. The message scales up, drifts upward and
fades out, so it never blocks your view of the next gap.

The wording gets stronger the further you get:

| Score reached | Message |
| --- | --- |
| 5 | **NICE** |
| 10 | **WOW** |
| 15 | **EPIC** |
| 20 | **LEGENDARY** |
| 25 | **UNSTOPPABLE** |
| 30 | **GODLIKE** |
| 40+ | **MYTHIC** |

Each tier also has its own colour, so the message becomes visibly more impressive
as your score climbs. From 10 points onward, the cheer is accompanied by a
firework burst around the bird.

---

## Crashing: the feather burst

![Feather burst on impact](docs/images/feather-burst.png)

When the bird hits a pipe or the ground, it does not simply stop. A **cloud of
small feathers bursts out of the point of impact**, as shown above. Twelve
feathers scatter outward, spin, flutter sideways as they fall under gravity, and
fade out.

The feathers are tinted to match the bird colour you selected, so a green bird
loses green feathers. The effect gives the crash a satisfying, readable moment of
impact instead of an abrupt freeze.

At the same time the pipes stop scrolling, a crash sound plays, and the bird drops
out of the sky. After a short delay the game over panel appears with your score,
your best score, an earned medal, and the **RESTART** button.

---

## Progression and rewards

The longer you survive, the more the game reacts to your performance:

| Score | What happens |
| --- | --- |
| Every 5 | A milestone cheer message and a celebration sound |
| 10 | Fireworks start bursting around the bird; **bronze medal** unlocked |
| 15 | A dragon joins the run and shoots horizontal fireballs you must dodge |
| 20 | The bird changes to an alternate design; **silver medal** unlocked |
| 30 | **Gold medal** unlocked |
| 40 | **Platinum medal** unlocked |

Medals are shown on the game over panel and are based on the score of that run.

---

## Running the project

1. Install **Unity Hub**, then install editor version **6000.3.20f1** exactly.
   Opening the project with a different Unity version triggers an upgrade that can
   stop the 2D sprites from rendering, which looks like an empty blue screen.
2. Download the project with the green **Code** button → **Download ZIP**, then
   unzip it. Do not run the project from inside the ZIP viewer.
3. In Unity Hub choose **Add** → **Add project from disk** and select the unzipped
   folder. Confirm the editor version next to the project reads `6000.3.20f1`.
4. Open the project and **wait for the first import to finish**. The first launch
   compiles every shader and can take several minutes; the progress bar in the
   bottom right must be empty before you press Play.
5. Open the scene `Assets/Scenes/SampleScene.unity`.
6. Run the menu item **Flappy Bird → Generate Art And Audio**. This regenerates
   every sprite and sound and creates `Assets/Resources`, which holds the dragon,
   the alternate bird, the spark particles and the celebration sounds.
7. Press **Play** (`Ctrl+P`).

### Troubleshooting

**The Game view is empty and only shows the blue sky**

- Confirm the editor version is `6000.3.20f1`. This is by far the most common cause.
- Let the import finish, then close and reopen Unity so every shader is loaded.
- Open `Assets/Scenes/SampleScene.unity` explicitly — an empty new scene looks similar.
- Check the **Console** tab; red errors there explain what failed to load.

**The dragon or the celebration sounds are missing**

- Run **Flappy Bird → Generate Art And Audio**. Without it `Assets/Resources` does
  not exist and those extras are skipped on purpose.

---

## Project structure

| Path | Contents |
| --- | --- |
| `Assets/Scripts` | All gameplay, UI, audio and effect scripts |
| `Assets/Editor/GenerateFlappyBirdArt.cs` | Editor tool that generates every sprite and sound |
| `Assets/Art`, `Assets/Audio` | Generated sprites and WAV files (committed) |
| `Assets/Resources/FlappyArt` | Art loaded at runtime by name |
| `Assets/Scenes/SampleScene.unity` | The single game scene |
| `Assets/Data/GameConfig.asset` | Tuning values for the bird, pipes and rewards |
| `docs/` | Documentation and screenshots |

### Key scripts

| Script | Responsibility |
| --- | --- |
| `GameManager.cs` | Game state machine, scoring, milestones, restart |
| `BirdController.cs` | Gravity, flapping, tilt, animation, collision reporting |
| `PipeSpawner.cs` / `PipePair.cs` | Pooling, spawning and scrolling of pipe pairs |
| `GameUI.cs` | Score HUD, ready and game over panels, cheer messages, medals |
| `BirdSelect.cs` | The six-colour bird picker on the ready screen |
| `GameEffects.cs` | Feather burst, sparks and fireworks |
| `Dragon.cs` / `Fireball.cs` | Late-game dragon hazard |
| `GameConfig.cs` | ScriptableObject holding all tuning values |

---

## Design document

The full design breakdown — pillars, mechanics, tuning tables, systems
architecture and content plan — lives in **[docs/GDD.md](docs/GDD.md)**.
