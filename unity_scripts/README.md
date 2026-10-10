# Sonari Unity 3D Gameplay Scripts

This directory contains the original C# scripts powering player locomotion, sign language dictionary integration, quest progression, and NPC dialogues for the Sonari 3D adventure game.

---

## 📁 Directory Structure & Key Components

### 1. UI_Elements/ (Core Game Logic & HUD)
* **SignDictionary.cs & SignDictionary_Change.cs**: Handles the in-game American Sign Language dictionary UI, visual guide overlays, and gesture reference swapping.
* **GameManager.cs & GameData.cs**: Central game state coordinator handling persistence, saves, and scene life cycles.
* **QuestMenuController.cs, QuestData.cs, & QuestMenuAnimator.cs**: In-game interactive quest tracker and mission objectives.
* **CutsceneManager.cs & CutsceneTrigger.cs**: Narrative sequencing and story cutscenes.
* **CurrencyManager.cs & ProgressionRewards.cs**: In-game reward mechanics for completing sign challenges.
* **LevelsSwitch.cs & PauseMenu.cs**: Scene transition controllers and settings menu.

### 2. script/ (NPC & Environment Interactions)
* **knightInteraction.cs**: Knight NPC interaction and quest triggers.
* **witchInteraction.cs**: Witch NPC dialogue and magic sign challenges.
* **pheonixInteraction.cs**: Phoenix NPC dialogue and guidance.
* **chestInteraction.cs**: Interactive treasure chests rewarded upon successful sign execution.
* **oundary.cs & 	eleport.cs**: World bounds safety checking and portal fast-travel.
* **NPCArrowGuide.cs**: Dynamic 3D waypoint navigation arrows pointing towards active objectives.

### 3. StarterAssets/ & SUPER Character Controller/
* **ThirdPersonController.cs & SUPERCharacterAIO.cs**: 3D third-person character movement, jumping, ground checks, and camera orbit.
* **StarterAssetsInputs.cs & UICanvasControllerInput.cs**: Cross-platform input mapping linking Python virtual key events to Unity player locomotion.

### 4. TerrainDemoScene_URP/, Phantom Studios/, & Kevin Iglesias/
* Lighting, procedural atmosphere, and character animation event helpers.
