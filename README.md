# 🎮 Match-3 Puzzle Game (Unity)

A configurable grid-based Match-3 puzzle game built with Unity.
Includes animated swaps, combo chains, hint system, shuffle logic, timer-based gameplay, scoring, and audio support.

---

# ✨ Features

## 🎯 Core Gameplay

* Grid-based match-3 board with configurable width and height (`Board.cs`).
* Random board initialization that avoids immediate 3+ matches.
* Smooth animated swaps using DOTween.
* Swap validation with automatic revert if no match is produced.

---

## 🧩 Tile / Piece System

* Tile prefab system with multiple types (`Dot.dotType`).
* Mouse / touch swipe input to select and swap tiles.
* Adjacency validation before swap attempt (`Board.TrySwap`).

---

## 🔄 Matching, Combos & Scoring

* Detects horizontal and vertical runs of ≥ 3 (`FindMatchRuns()`).
* Supports chain reactions (combos).
* Scoring system:

  * 3 tiles → 1 point
  * 4 tiles → 2 points
  * 5 tiles → 3 points
* Time bonus per point (configurable in `GameManager`).
* Points added via `GameManager.AddPoints()`.

---

## 🧱 Board Maintenance

* Removes matched tiles (`DestroyMatches()`).
* Collapses columns (falling tiles).
* Refills empty cells with new random tiles.
* Shuffle system when no possible moves are found.
* Fallback full board rebuild after several failed shuffle attempts.

---

## 💡 Hint System / UX

* Automatic hint system after inactivity.
* Finds possible moves and animates two tiles (shake + scale pulse).
* Hint timer resets after successful move.
* Shake animation restricted to X-axis for clear visual feedback.

---

## ⏱ Game Flow & UI

* Timer-based gameplay:

  * `initialTime`
  * `maxTime`
  * Countdown system
* Time bonus added when scoring.
* Pause / Resume system.
* Game Over handling.
* Restart support.
* UI integrated with TextMeshPro (`TextMeshProUGUI`).

---

## 🔊 Audio

* AudioManager Singleton.
* Background music (loop).
* Swap SFX (plays only on successful swap).
* Volume controls & mute toggle.
* Fade in / fade out support (if enabled).

---

## 🏗 Architecture

* Singleton pattern for:

  * `GameManager`
  * `AudioManager`
* Inspector-exposed serialized fields for easy tuning.
* DOTween used for:

  * Swap animations
  * Falling tiles
  * Hint shake & scale
  * Shuffle animations

---

# 📁 Important Files

### `Board.cs`

Handles:

* Board creation
* Swap logic
* Match detection
* Collapse & refill
* Shuffle
* Hint system

---

### `Dot.cs`

Handles:

* Tile behavior
* Swipe input detection
* Swap requests

---

### `GameManager.cs`

Handles:

* Score system
* Timer logic
* Pause / Resume
* Game Over
* UI bindings

---

### `AudioManager.cs`

Handles:

* Background music
* Swap SFX
* Volume & mute
* Fade effects

---

# 📦 Requirements / Dependencies

* **Unity** (Specify your Unity Editor version here)
* **DOTween** (for animations)
* **TextMeshPro** (for UI text)
* Audio files (e.g., `background.ogg`, `swap.ogg`)
* Tile prefabs assigned in Inspector

---

# 🚀 Quick Start

1. Open the project in Unity.
2. Install DOTween (if not already installed).
3. Open the main gameplay scene.
4. Assign:

   * Tile prefabs to `Board`
   * UI references to `GameManager`
   * Audio clips to `AudioManager`
5. Press **Play**.

---

# 🎮 Controls

* Click / Tap a tile and swipe to swap.
* Swap must be adjacent.
* Esc key toggles Pause.

---

# ⚙ Configuration (Inspector Tuning)

### Board

* Width / Height
* HintDelay
* Hint shake strength
* Hint scale amount
* Swap animation duration
* Fall duration
* Max shuffle attempts

### GameManager

* Initial time
* Max time
* Time bonus per point
* UI references

### AudioManager

* Music volume
* SFX volume
* Background clip
* Swap clip

---

# 🧮 Scoring & Time Bonus Logic

For each match:

```
Points = (MatchedTiles - 2)
```

Example:

* 3 tiles → 1 point
* 4 tiles → 2 points
* 5 tiles → 3 points

Time bonus is applied per point earned.

---

# ⚠ Known Limitations / TODO

* No special tiles (bomb, striped, etc.) yet.
* No visual particle effects on match.
* No persistent save system.
* No level progression system.

---

# 🏆 Credits

Developed using:

* Unity Engine
* DOTween
* TextMeshPro

---

# 📜 License

Specify your license here (MIT / Personal / Educational / etc.)

