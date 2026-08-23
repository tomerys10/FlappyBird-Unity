# Flappy Bird (Unity 6000.3.20f1)

Clone of the original Flappy Bird gameplay: tap/space to flap, dodge pipes, score,
game over with best score and medals.

## Open the project on a new computer

1. Install **Unity Hub**, then install editor version **6000.3.20f1** exactly.
   Opening the project with a different Unity version upgrades it and can stop the
   2D sprites from rendering, which looks like an empty blue screen.
2. Download the project: green **Code** button → **Download ZIP**, then unzip.
   Do not run the project from inside the ZIP viewer.
3. Unity Hub → **Add** → **Add project from disk** → pick the unzipped folder.
   Make sure the editor version next to the project says `6000.3.20f1`.
4. Open the project and **wait** for the first import to finish. The first open
   compiles every shader and can take several minutes. The progress bar at the
   bottom right must be empty before you press Play.
5. In the Project window open `Assets/Scenes/SampleScene.unity`.
6. Menu **Flappy Bird → Generate Art And Audio**. This creates
   `Assets/Resources`, which is not stored in the repository and holds the dragon,
   the alternate bird, the sparks and the celebration sounds.
7. Press **Play** (`Ctrl+P`) or the triangle in the top toolbar.

## Controls

- Mouse click, Space, or Up Arrow to flap and to start a run

## Features

- Ready → Playing → Game Over flow with a best score saved in `PlayerPrefs`
- Bird colour picker on the ready screen
- Feather burst when the bird is hit
- Cheer titles every 5 points: NICE, WOW, EPIC, LEGENDARY, UNSTOPPABLE, GODLIKE, MYTHIC
- Fireworks from score 10, dragon from score 15, alternate bird from score 20
- Bronze, silver, gold and platinum medals on the game over screen

## Troubleshooting

**The Game view is empty and only shows the blue sky**

- Check the editor version is `6000.3.20f1`. This is the most common cause.
- Let the import finish, then close and reopen Unity once so all shaders load.
- Open `Assets/Scenes/SampleScene.unity` explicitly; a new empty scene looks similar.
- Look at the **Console** tab. Red errors there explain what failed to load.

**The dragon or the celebration sounds are missing**

- Run **Flappy Bird → Generate Art And Audio**. Without it `Assets/Resources`
  does not exist and those extras are skipped on purpose.

## Project layout

- `Assets/Scripts` — gameplay, UI, audio and effects
- `Assets/Editor/GenerateFlappyBirdArt.cs` — generates all art and audio
- `Assets/Art`, `Assets/Audio` — generated sprites and sounds (committed)
- `Assets/Scenes/SampleScene.unity` — the only scene
- `Assets/Data/GameConfig.asset` — tuning values for the bird, pipes and rewards
