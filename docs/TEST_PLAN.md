# Test Plan

## Automated Results — 2026-11-24

Run in `C:\Users\LenOvo\AppData\Local\Temp\opencode\lwalida-build` because the live editor was locked.

| Suite | Result | Evidence |
|---|---|---|
| Batch MVP build | Exit 0 | `build4.log` |
| Project validation | 0 errors, 1 warning | `build4.log` |
| EditMode tests | 13 passed, 0 failed | `tests-edit2.xml` |
| PlayMode smoke test | 1 passed, 0 failed | `tests-play.xml` |

The single validation warning is the missing Arabic title PNG; the menu falls back to built-in font text.

### EditMode coverage

- Progression requirements combine memories and flags.
- All four memories are required.
- Memory IDs are unique.
- Objective changes with progress.
- Level order and world widths are stable.
- Amina/Allal dialogue links and awards validate.
- Intro/Home cutscene lines are non-blank.
- Save JSON round-trips and rejects unknown scenes.
- Each generated gameplay scene contains bootstrap, marker, player, camera and interactables.

### PlayMode coverage

- `01_MainMenu` boots one `GameRoot`.
- `GameManager`, catalog and `UIRoot` initialize.
- Mode is `Menu`, menu is visible and time scale is not paused.

## Manual Checklist — Not Yet Run

Mark these only after actually playing. A full route playthrough is still outstanding.

### Boot and menu

- [ ] Fresh start opens the main menu without console errors.
- [ ] Continue is disabled with no valid save.
- [ ] New Journey starts the intro and resets memories/flags.
- [ ] Continue loads a valid save; a corrupt save shows a safe message.
- [ ] Settings and Credits open/close; Esc returns correctly.
- [ ] Exit asks for confirmation.

### Movement and interaction

- [ ] A/D, arrows and mouse work; player stays inside bounds.
- [ ] E interacts with the nearest visible prompt.
- [ ] J opens/closes the journal and pauses movement.
- [ ] Esc closes the topmost modal before pause.
- [ ] Idle hint appears after inactivity.

### Café Maure

- [ ] Photograph is collectible once and repeated inspection is safe.
- [ ] Café exit is locked before `cafe_01`.
- [ ] Journal shows the café memory and no duplicate reward.

### Oudaya

- [ ] Wrong doors give specific mismatch feedback.
- [ ] Camera mode opens only near doors; Esc closes it.
- [ ] Correct door awards `door_01` once and closes camera.
- [ ] Amina conversation choices work; `amina_01` is awarded.
- [ ] Souk exit unlocks only with `door_01` and `amina_01`.

### Souk

- [ ] Allal conversation unlocks recording.
- [ ] R before Allal gives the correct locked message.
- [ ] R in the zone shows three-second progress; Esc cancels cleanly.
- [ ] Completion awards `souk_01` once and reports placeholder audio honestly.
- [ ] Journal plays the sound clip and stops when closed.
- [ ] Home/Hassan exits remain locked until all four memories exist.

### Hassan and Home

- [ ] Hassan Tower is reachable and its exit respects the four-memory gate.
- [ ] Home cutscene plays, knock sound occurs and memories are shown.
- [ ] Skip asks for confirmation and preserves `home_seen`.
- [ ] Credits appear and return to the main menu.

### Settings and accessibility

- [ ] All five volume sliders affect the correct channels.
- [ ] Subtitles toggle does not break dialogue.
- [ ] Fullscreen applies outside the editor.
- [ ] Reduced flashes suppresses camera flash.
- [ ] Reduced motion removes camera sway/bobbing.
- [ ] Settings persist across restarts.

### Save and failure paths

- [ ] Checkpoint saves provide a valid Continue after returning to menu.
- [ ] No save is written during dialogue.
- [ ] Scene IDs missing from build settings show a clear error and recover.
- [ ] Missing audio causes no exceptions.
- [ ] Missing catalog falls back without crashing.

## Standalone Build — Not Run

1. Install **Windows Build Support (Mono)** in Unity Hub.
2. Run `LWalida > Build Windows`.
3. Launch `Builds/Windows/LWalida.exe` outside the editor.
4. Repeat the route checks with no custom assets and again with supplied replacements.
5. Confirm save path writes under the player's persistent data directory.
