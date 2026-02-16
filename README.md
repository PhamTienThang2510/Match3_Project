Dưới đây là một phiên bản **README.md chuyên nghiệp**, tối ưu để đưa lên GitHub và gắn vào CV intern Unity Dev.
Nội dung viết theo hướng clean, technical, rõ kiến trúc — phù hợp portfolio.

Bạn có thể copy nguyên khối này vào `README.md`.

---

# 🎮 Match-3 Prototype (Unity)

A small but fully functional **Match-3 game prototype** built with Unity.
The project focuses on clean board logic, match detection, scoring rules, and time-based gameplay mechanics.

> Swap adjacent tiles to form matches.
> Matches grant points and bonus time.
> When the timer reaches zero — the game ends.

---

## 🚀 Features

* Standard match-3 grid with gravity and refill
* Drag / swipe input handling per tile
* Horizontal & vertical match detection (supports T/L overlaps)
* Scoring system based on run length
* Time bonus system tied to scoring
* Combo chaining (auto re-check after refill)
* Tweened animations using DOTween
* Simple Pause & Game Over UI
* Inspector-configurable gameplay parameters

---

## 🧠 Gameplay Rules

* Swap two adjacent tiles (horizontal or vertical).
* A contiguous run of **N** tiles of the same type yields:

```
Points = N - 2
```

Examples:

* 3 in a row → 1 point

* 4 in a row → 2 points

* 5 in a row → 3 points

* T- or L-shaped matches count as multiple runs.

* Each point grants a time bonus:

```
Time bonus = points × timeBonusPerPoint
```

* The timer counts down from `initialTime`.
* When time reaches zero → Game Over.

---

## 🏗 Project Architecture

### 📁 Assets/_Scripts/Board.cs

Core gameplay controller.

Responsibilities:

* Board initialization
* Swap validation and animation
* Match detection
* Destruction
* Collapse (gravity system)
* Refill logic
* Combo chain processing
* Reporting points to GameManager

Uses:

* Coroutine state flow
* 2D grid array (GameObject[,])
* DOTween for animation

---

### 📁 Assets/_Scripts/Dot.cs

Per-tile behavior.

Responsibilities:

* Stores logical position (row, column)
* Stores tile type
* Handles pointer input
* Computes swipe direction
* Requests swap from Board

---

### 📁 Assets/_Scripts/GameManager.cs

Game state controller.

Responsibilities:

* Score tracking
* Timer countdown
* Pause handling
* Game Over handling
* UI updates
* Adds time bonus when scoring

Exposed method:

```
AddPoints(int amount)
```

---

## ⚙ Inspector Configuration

### Board Component

* `width`, `height` — board dimensions
* `dots` — tile prefab array

---

### GameManager Component

* `initialTime`
* `maxTime`
* `timeBonusPerPoint`
* `pointsText`
* `timeText`
* `pausePanel`
* `gameOverPanel`

If Board reference is not manually assigned, it will auto-find at runtime.

---

## 🌀 Game Loop Flow

```
Player Swipe
    ↓
Validate Swap
    ↓
Animate Swap
    ↓
Check Matches
    ↓
If No Match → Revert
If Match →
    Destroy
    Collapse
    Refill
    Re-check (Combo Chain)
    ↓
Return to Idle
```

---

## 🧩 Dependencies

* DOTween (Demigiant)
* TextMeshPro (Unity Package)
* Main Camera must be tagged: `MainCamera`

---

## 🛠 How to Run

1. Open project in Unity Editor.
2. Ensure DOTween is installed and initialized.
3. Verify prefab and UI references in Inspector.
4. Press Play.

### Debug with Visual Studio 2022

* Open `Assembly-CSharp.sln`
* Debug → Start Debugging (F5)

---

## 🧪 Implemented Mechanics

* Initial board generation avoids starting matches.
* Manhattan-distance swap validation.
* Match detection with horizontal & vertical runs.
* T/L overlaps handled correctly via Distinct match filtering.
* Combo chain logic via recursive coroutine.
* Time bonus clamped by `maxTime`.

---

## 🔮 Possible Extensions

* Special tiles (bomb, line clear, color bomb)
* Combo multiplier system
* Highscore persistence
* Particle & floating score feedback
* Object pooling for performance
* ScriptableObject tile definitions
* Unit tests for match detection logic

---

## 💡 Technical Highlights (For Recruiters)

* Clean separation between Board logic and GameManager state
* Coroutine-based state machine
* Deterministic match detection algorithm
* Grid-based data model (2D array)
* Tween-driven animation system
* Configurable gameplay via Inspector

---

## 🎯 Purpose of This Project

This prototype demonstrates:

* Understanding of grid-based game architecture
* Coroutine flow control
* Gameplay state management
* Animation integration (DOTween)
* Clean code structure suitable for extension

Designed as a foundational system that can scale into a production-ready Match-3 framework.

---

## 📜 License

Add your preferred license (MIT recommended for portfolio projects).

---



Chọn 1 style, mình build lại bản nâng cấp cho bạn.
