# Asset Guide

This project runs with procedural placeholders. Authored assets are optional but take precedence when assigned correctly.

## Runtime Content Registry

`Assets/_Game/Resources/AssetCatalog.asset` holds every runtime sprite, clip, prefab, memory and dialogue reference. `GameRoot` loads it through `Resources.Load<AssetCatalog>("AssetCatalog")`.

Replacement rule:

1. Assign an authored asset to the matching `AssetCatalog` field in the Inspector.
2. The editor builder only fills fields that are **null**.
3. Re-running `LWalida > Build or Update MVP Scenes` will not overwrite a non-null assignment.
4. Never rename or delete the catalog asset unless you update `LWalidaPaths.CatalogPath`.

If the catalog is missing at runtime, the game falls back to narrative data and colour-only placeholders.

## Art Locations

| Content | Path |
|---|---|
| Supplied environment JPGs | `Assets/_Game/Art/Environments/` |
| Generated placeholder PNGs | `Assets/_Game/Generated/Placeholders/Art/` |
| Memory/dialogue assets | `Assets/_Game/Data/` |
| Prefabs | `Assets/_Game/Prefabs/` |
| Scenes | `Assets/_Game/Scenes/` |

## Supplied Environment Art

Current files:

| File | Dimensions | Usage |
|---|---|---|
| `cafe_maure.jpg` | 800×600 | Café Maure |
| `oudaya_street.jpg` | 3840×2566 | Oudaya |
| `souk.jpg` | 1216×832 | Souk |
| `hassan_sunset.jpg` | 1536×1024 | Hassan Tower |
| `home_evening.jpg` | 1536×1024 | Home |

The composer tiles/crops these to fill a wide scene. For final art, prefer **3840×1080** or **1920×1080** panoramic images with a safe focal area near the centre. Keep the same filenames to avoid reassignment, or update `AssetCatalog.envCafe`, `envOudaya`, `envSouk`, `envHassan`, `envHome`.

Import settings used by the builder: **Sprite (2D and UI)**, Single, 100 pixels/unit, max size 4096, compressed HQ, mipmaps enabled. Re-run the builder after replacing a file if import settings need to be normalized.

## Procedural Placeholder Art

Generated at 100 pixels/unit with centre pivots.

| Category | Examples | Size |
|---|---|---|
| Sky gradients | `skyCafe` … `skyHome` | 512×256 |
| Far skylines | `farCafe` … `farHome` | 1024×360 |
| Ground strip | `groundStrip` | 512×128 |
| Characters | `playerIdle`, `aminaIdle`, `allalIdle`, `neighborIdle` | 110×210 |
| Dialogue portraits | `portraitPlayer`, `portraitAmina`, `portraitAllal`, `portraitNeighbor` | 256×256 |
| Doors | `doorCorrect`, `doorWrong01`, `doorWrong02` | 160×320 |
| Small props | `photoProp` 120×90, `referencePhoto` 120×150, `teaSet` 90×60, `flowerpot` 70×90 | varies |
| Large props | `bougainvillea` 160×220, `stall` 260×220 | varies |
| Markers | `exitMarker`, `soundMarker` | 64×96 |
| Memory cards | `memoryCafe`, `memoryDoor`, `memoryAmina`, `memorySouk` | 512×384 |
| Branding/UI | `logoEmblem` 256×256, `menuBackground` 1920×1080 | varies |
| Nine-slice panels | `panelDark`, `panelLight` | 64×64 |
| Textures | `paperTexture` 512×512, `zelligePattern` 256×256, `vignette` 512×512, `grain` 256×256 | varies |
| Icons | camera, journal, record, lock, audio | 96×96 |
| Solid pixel | `whitePixel` | 8×8 |

To replace placeholders without touching code:

- Put authored sprites anywhere under `Assets/_Game/Art/` and assign them to the matching `AssetCatalog` fields, **or**
- Replace a generated PNG at the same path and keep its filename. `Generate Missing Placeholders` never overwrites an existing file.

Do **not** assign a sprite with a bottom pivot unless you also adjust `LevelComposer.Place`; the composer positions sprites using their bounds height and expects a centre pivot.

## Panels and Nine-Slice

`panelDark` and `panelLight` are used as 9-sliced UI panels. If you replace them with authored panels, set the sprite border in the Sprite Editor and keep the 64×64 base size or update the border values. Transparent padding is acceptable.

## Animation and Sprite Sheets

No sprite-sheet animation is currently wired. Characters use a single idle sprite with procedural bobbing. If you add idle/walk sheets:

- Use equal cells and a transparent background.
- Document frame count and cell size here before wiring them.
- Create Animation/Animator assets and assign them to prefabs; do not assume the existing `SpriteRenderer` swaps frames automatically.
- Keep feet aligned to the same baseline in every frame.

## Audio

Placeholder files: `Assets/_Game/Generated/Placeholders/Audio/`

- Format: 16-bit mono WAV, 22050 Hz.
- Music: `menuMusic.wav` (24 s), `explorationMusic.wav` (32 s), `endingMusic.wav` (24 s).
- Ambience: `ambienceCafe.wav`, `ambienceOudaya.wav`, `ambienceSouk.wav`, `ambienceHassan.wav`, `ambienceHome.wav` (12 s each, looped).
- Effects: `sfxShutter.wav`, `sfxKnock.wav`, `sfxFootstep.wav`, `sfxUiClick.wav`, `sfxMemoryUnlock.wav`, `sfxRecordComplete.wav`.

Replacement options:

1. Replace the WAV at the same path. The loader looks for `.wav` at that exact path.
2. For OGG or a different filename, assign the clip directly to the corresponding `AssetCatalog` field in the Inspector. The loader does not search for OGG variants automatically.
3. Long music/ambience should use compressed streaming; short effects should use decompress-on-load for low latency.
4. Normalize levels and avoid clipping. The master and channel sliders multiply clip volumes.

### Voice

There are no voice clips. Dialogue nodes have an optional `voice` field, and the audio manager plays it if assigned. Do not generate fake Darija recordings. If real voice acting is added, record it with permission, document the performers and keep subtitles enabled by default.

## Data Assets

Memories: `Assets/_Game/Data/Memories/cafe_01.asset`, `door_01.asset`, `amina_01.asset`, `souk_01.asset`.

Dialogue: `Assets/_Game/Data/Dialogue/amina.asset`, `amina_after.asset`, `allal.asset`, `allal_after.asset`.

Editing rules:

- Keep memory IDs stable: `cafe_01`, `door_01`, `amina_01`, `souk_01`.
- Keep dialogue node IDs stable and make every link point to an existing node.
- Terminal nodes must set `endConversation`.
- Use `LWalida > Validate Project` after edits.
- Saving a dialogue node with an award memory is idempotent: collecting the same memory twice is ignored.

## Prefabs and Scenes

Prefabs are generated only when missing: `Player`, `NPC`, `Door`, `Prop`, `Exit`, `RecordingZone`.

Scenes are generated only when missing by **Build or Update**; use **Rebuild MVP Scenes** to overwrite the eight generated scenes intentionally. `Assets/Scenes/SampleScene.unity` is preserved.

Scene composition should remain in `LevelComposer` where possible so direct Play and generated scenes match. If you edit a generated scene manually, subsequent **Build or Update** runs will preserve it; **Rebuild** will discard the edits.

## Version Control Note

Unity `.meta` files must be committed with their assets. Large supplied images and future music/voice files may exceed normal Git limits; if the team uses Git rather than Plastic, consider Git LFS for `*.jpg`, `*.png`, `*.wav` and `*.ogg`. Do not add generated `Library/`, `Temp/`, `Logs/`, `Builds/` or `UserSettings/` content. See `.gitignore`.

## Credits and Licensing

Supplied environment JPGs have no documented provenance in the repository. Confirm authorship and usage rights before publishing. See `ASSET_CREDITS.md`.
