# Mini World – Technische Übersicht

Dieses Dokument ist für Entwickler gedacht, die verstehen oder erweitern wollen, wie die Spielwiese „Mini World – Unity Playground“ gebaut ist. Für den Workshop selbst gibt es eigene Anleitungen:

- [README_DE.md](README_DE.md) – Schnellstart, Bedienung, häufige Probleme
- [WORKSHOP_DE.md](WORKSHOP_DE.md) – Ablauf der 90 Minuten
- [CHALLENGES_DE.md](CHALLENGES_DE.md) – Aufgaben zum Ausprobieren

Alle Namen, Pfade und Werte hier stammen aus dem Code unter `Assets/StudentSandbox/Scripts/` und aus der erzeugten Szene.

---

## 1. Auf einen Blick

| Thema | Wert |
|---|---|
| Unity | 6000.3.23f1 |
| Render Pipeline | URP 17.3.0, Renderer im Modus Forward+ (`Assets/Settings/PC_RPAsset`, `PC_Renderer`) |
| Eingabe | Input System 1.20.0, „Active Input Handling“ = Input System Package |
| Tests | Unity Test Framework 1.6.0 (EditMode und PlayMode) |
| Szene | `Assets/StudentSandbox/Scenes/StudentSandbox.unity` – fertig erzeugt und im Repository |
| Ausgangskopie | `Assets/StudentSandbox/Scenes/Original/StudentSandbox_Original.unity` |
| Sicherungen | `Assets/StudentSandbox/Backups/StudentSandbox_Backup_<Datum>_<Uhrzeit>.unity` |
| Menü | `Tools > Student Sandbox > …` |
| Namespaces | `StudentSandbox` (Laufzeit), `StudentSandbox.EditorTools` (Editor), `StudentSandbox.Tests` |
| Szene | 2.466 GameObjects, 12 Schalter, 24 Lampen-Controller, 286 Erklär-Notizen (`InfoNote`) |
| Generierte Assets | 30 Prefabs, 80 Materialien, 3 Physik-Materialien, 17 Meshes, 1 Lighting-Settings-Asset, 1 Volume-Profil; dazu 2 eigene Shader (von Hand geschrieben, nicht generiert) |

Vor dem Workshop muss nichts erzeugt werden: Die Szene wurde im Batch-Modus generiert und liegt im Projekt.

---

## 2. Architektur

### 2.1 Assemblies

| Assembly (asmdef) | Ordner | Plattform | Verweise | Namespace |
|---|---|---|---|---|
| `StudentSandbox.Runtime` | `Scripts/Runtime` | alle | `Unity.InputSystem` | `StudentSandbox` |
| `StudentSandbox.Editor` | `Scripts/Editor` | nur Editor | Runtime, `Unity.RenderPipelines.Universal.Runtime`, `Unity.RenderPipelines.Core.Runtime` | `StudentSandbox.EditorTools` |
| `StudentSandbox.Tests.EditMode` | `Tests/EditMode` | nur Editor | Runtime, Editor, Test Runner | `StudentSandbox.Tests` |
| `StudentSandbox.Tests.PlayMode` | `Tests/PlayMode` | alle | Runtime, Test Runner | `StudentSandbox.Tests` |

Regeln:

- Laufzeit-Skripte benutzen `UnityEditor` nur innerhalb von `#if UNITY_EDITOR`. Das betrifft drei Stellen: `FreeFlyCamera.Start` (Kamera startet an der Scene View), `LampController.OnValidate` und `TrafficLightController.OnValidate` (Inspector-Änderungen sofort anzeigen). Der Validator prüft das (siehe Abschnitt 15).
- Die Runtime-Assembly setzt über `versionDefines` das Symbol `SANDBOX_INPUT_SYSTEM_PACKAGE`, sobald `com.unity.inputsystem` ab Version 1.0.0 installiert ist.
- Die Test-Assemblies sind nur mit `UNITY_INCLUDE_TESTS` aktiv und nicht `autoReferenced`.
- Die `.csproj`- und `.slnx`-Dateien im Projektordner erzeugt Unity selbst. Sie stehen in der `.gitignore`. Einen eigenen dotnet-Build oder Schnell-Check liefert das Repository nicht mit. Kompiliert und getestet wird nur über Unity.

### 2.2 Ordner

```
Assets/StudentSandbox/
  Scenes/            StudentSandbox.unity, Original/StudentSandbox_Original.unity
  Backups/           Sicherungen der Szene (nur .unity-Dateien; entsteht bei der ersten Sicherung)
  Scripts/
    Runtime/         Core, Camera, Interaction, Lighting, Physics (+ asmdef)
    Editor/          SandboxGenerator.cs (+ asmdef)
      Core/          SB, SandboxContext, SandboxPaths, SandboxKeys (M, PM, MK, P),
                     SandboxMaterials, SandboxMeshes, SandboxPrefabs.Core,
                     LightingSetup, CameraSetup, SandboxSceneView
      Builders/      WorldBuilder, CreativeZoneBuilder, PhysicsZoneBuilder,
                     LightZoneBuilder, InteractionZoneBuilder
      Prefabs/       SandboxPrefabs.Creative / .Physics / .Interactions / .Environment
      Inspectors/    eigene Inspectoren
      Workshop/      WorkshopWindow, WorkshopLessons, DayNightEditorUtility
      Validation/    SandboxValidator
      Dev/           SandboxScreenshots, SandboxHierarchyDump
  Tests/             EditMode/, PlayMode/
  Prefabs/           Creative (15), Physics (9), Interactions (3), Environment (3)
  Materials/         Palette (16), Environment (32), Effects (29), Sky (3)
  PhysicsMaterials/  Slippery, Normal, Bouncy
  Meshes/            17 Low-Poly-Meshes (LP_*.asset)
  Shaders/           GradientSky.shader, Text3D.shader
  Settings/          StudentSandbox_Lighting.lighting, StudentSandbox_PostProcessing.asset
  Documentation/     diese Anleitungen
```

Alle Asset-Pfade stehen an einer Stelle in `SandboxPaths`. Nur die PlayMode-Tests haben den Szenen-Pfad als eigene Konstante, weil ihre Assembly die Editor-Assembly nicht kennt. Der Generator schreibt nur unterhalb von `Assets/StudentSandbox` und ändert keine Projekteinstellungen.

---

## 3. Der Editor-Generator

### 3.1 Menüpunkte

| `Tools > Student Sandbox >` | Ruft auf | Wirkung |
|---|---|---|
| Workshop | `WorkshopWindow.ShowWindow` | Öffnet das Workshop-Fenster |
| Generate Sandbox | `SandboxGenerator.GenerateInteractive` | Baut alles neu, mit Rückfrage |
| Open Sandbox Scene | `SandboxGenerator.OpenSandboxScene` | Öffnet die Szene |
| Restore Original Scene | `SandboxGenerator.RestoreOriginalScene` | Setzt die Szene und die generierten Assets auf den Ausgangszustand zurück |
| Create Backup of Scene | `SandboxGenerator.BackupScene` | Kopiert die gespeicherte Szene nach `Backups/` |
| Frame World Overview | `WorkshopWindow.FrameWorldOverview` | Richtet die Scene View auf `Viewpoint_1_Overview` aus |
| Validate Sandbox | `SandboxValidator.ValidateFromMenu` | Prüfbericht in der Console und als Dialog |
| Developer > Capture Screenshots | `SandboxScreenshots` | PNGs aller Aussichtspunkte bei Tag (Übersicht und Lichtlabor auch bei Sonnenuntergang und Nacht) nach `Temp/SandboxScreenshots`. Im Play Mode gesperrt. Danach stellt das Werkzeug Sonne, Himmel und Lampen exakt wieder her. |
| Developer > Dump Hierarchy | `SandboxHierarchyDump` | Hierarchy mit Komponenten nach `Temp/StudentSandbox_Hierarchy.txt` |

### 3.2 Ablauf von `Generate`

`SandboxGenerator.Generate(makeBackupOfExistingScene, showProgress)` arbeitet diese Schritte der Reihe nach ab:

1. `SandboxPaths.EnsureAllFolders()` legt die Ordner an.
2. Falls gewünscht und vorhanden: Sicherung der alten Szene. Scheitert die Sicherung, bricht der Generator ab.
3. `SandboxMaterials.CreateAll()` – Materialien, Himmel, Schrift-Material, Physik-Materialien.
4. `SandboxMeshes.CreateAll()` – Low-Poly-Meshes.
5. Neue leere Szene (`NewSceneSetup.EmptyScene`).
6. `SandboxPrefabs.CreateAll()` – alle 30 Prefabs. Sie werden kurz in der neuen Szene gebaut, als Asset gespeichert und wieder gelöscht.
7. `SandboxContext.CreateRootGroups()` – die zehn Hauptgruppen `00_WORLD_ROOT` bis `09_DECORATION`.
8. `LightingSetup.Build` – Lighting-Settings-Asset, `Sun`, die drei Manager in `08_MANAGERS`, Global Volume, Nebel.
9. `WorldBuilder.BuildEnvironment` – Insel, Wasser, Wege.
10. `WorldBuilder.BuildHub` – der zentrale Platz.
11. `CreativeZoneBuilder`, `PhysicsZoneBuilder`, `LightZoneBuilder`, `InteractionZoneBuilder` – die vier Bereiche.
12. `WorldBuilder.BuildDecoration` – Bäume, Steine, Blumen, Wolken, Windmühle, Leuchtturm, Steg, Picknickplatz, Ufer.
13. `CameraSetup.Build` – fünf Aussichtspunkte und `Main Camera`.
14. `DayNight.Apply(TimeOfDay.Day)` – Startzustand „Tag“.
15. `SetFriendlyInspectorRotations` – setzt mit `TransformUtils.SetInspectorRotation` für jedes Transform Drehwerte zwischen -180 und 180. Der Inspector zeigt dann zum Beispiel bei `Ramp_Main` Rotation Z = -22 statt 338 und beim `Hammer` -75.
16. `RecordAllPrefabInstanceModifications` – per Skript geänderte Werte an Prefab-Instanzen werden als Overrides registriert, sonst gingen sie beim Speichern verloren.
17. Szene speichern, Ausgangskopie schreiben, `AssetDatabase.SaveAssets()` und `Refresh()`.
18. Nur interaktiv: Scene View auf die Übersicht ausrichten.

Die Reihenfolge hat einen Grund: `LightingSetup` legt `DayNightController` und `PhysicsResetter` an, bevor die Bereiche gebaut werden. Die Knöpfe im Physik- und Lichtlabor werden direkt mit diesen Objekten verbunden. Die Klasse `SandboxContext` reicht diese Referenzen und die Maße der Welt an alle Builder weiter.

Fehler werden abgefangen und in der Console gemeldet (`[Student Sandbox] Erzeugen fehlgeschlagen: …`). Interaktiv erscheint zusätzlich ein Dialog. `Generate` gibt dann `false` zurück.

### 3.3 Wiederholbarkeit

- Die Szene wird jedes Mal komplett neu aufgebaut.
- Assets werden an Ort und Stelle aktualisiert: Materialien (frisch gebaut und mit `EditorUtility.CopySerialized` komplett in das vorhandene Asset kopiert), Meshes (ebenfalls `CopySerialized`), Prefabs (`PrefabUtility.SaveAsPrefabAsset` über das vorhandene), Physik-Materialien, Volume-Profil und Lighting-Settings. Die GUIDs bleiben gleich. Verweise aus Sicherungen bleiben dadurch gültig.
- Zufällige Platzierungen (Bäume, Steine, Blumen, Wolken …) nutzen `System.Random` mit dem festen Startwert `WorldBuilder.Seed = 20261020`. Jede Erzeugung sieht gleich aus.
- Folge: Wer generierte Materialien oder Prefabs von Hand ändert (zum Beispiel die Farbe von `Materials/Palette/CoralRed`, die Transparenz eines Materials oder ein Prefab über „Overrides > Apply All“), verliert diese Änderung beim Neu-Erzeugen und beim Wiederherstellen der Ausgangsszene. Beide Rückfrage-Dialoge weisen darauf hin.

### 3.4 Sicherung, Ausgangskopie, Wiederherstellen

- `BackupScene()` kopiert die **gespeicherte** `.unity`-Datei (ohne `.meta`) nach `Backups/StudentSandbox_Backup_yyyy-MM-dd_HH-mm-ss.unity`. Gibt es den Namen schon, wird `_1`, `_2` … angehängt. Unity vergibt der Kopie eine neue GUID. Ist die Spielwiese gerade offen, fragt das Menü vorher, ob ungespeicherte Änderungen gespeichert werden sollen – gesichert wird immer der Stand auf der Festplatte.
- Nach jeder erfolgreichen Erzeugung überschreibt der Generator die Ausgangskopie `Scenes/Original/StudentSandbox_Original.unity`.
- `RestoreOriginalScene()` (Menü oder „Ausgangsszene wiederherstellen“ im Workshop-Fenster): Rückfrage → ungespeicherte Änderungen speichern (optional) → Sicherung der aktuellen Szene → leere Szene öffnen → `ResetGeneratedAssets()` → Ausgangskopie über `StudentSandbox.unity` kopieren → neu importieren und öffnen. Die GUID der Szene bleibt dabei erhalten.
- `ResetGeneratedAssets()` baut alle generierten Assets neu auf: Materialien (auch Palette-Farben, Transparenz und hineingezogene Texturen), Meshes, Prefabs (auch nach „Overrides > Apply All“) und das Post-Processing-Profil (Tonemapping, Bloom, Vignette und Color Adjustments bekommen ihre Werte zurück; selbst hinzugefügte Effekte bleiben im Profil). Die GUIDs bleiben gleich, Szene und Ausgangskopie passen also weiter dazu.
- Ist gerade die Ausgangskopie selbst offen, werden ihre Änderungen verworfen und nie in die Kopie gespeichert. Öffnet jemand `Scenes/Original/StudentSandbox_Original.unity`, bietet ein Dialog an, stattdessen die richtige Spielwiese zu öffnen (`SandboxSceneView`).
- Neu-Erzeugen bei vorhandener Szene zeigt drei Möglichkeiten: „Sicherung anlegen und neu erzeugen“, „Abbrechen“, „Nur Szene öffnen“.
- Sicherungen und Ausgangskopie enthalten **nur die Szene**. Geänderte Prefab- oder Material-Assets werden nicht gesichert. „Restore Original Scene“ und „Generate Sandbox“ setzen sie auf den Ausgangszustand zurück. Wer eigene Asset-Änderungen behalten will, braucht Git.
- Erzeugen, Wiederherstellen und Sichern sind im Play Mode gesperrt. Das gilt für die Menüpunkte (auch „Create Backup of Scene“) und für die Knöpfe im Workshop-Fenster („Sicherung anlegen“, „Ausgangsszene wiederherstellen“, „Spielwiese neu erzeugen“).

### 3.5 Batch-Modus

```
Unity.exe -batchmode -projectPath <Pfad> -executeMethod StudentSandbox.EditorTools.SandboxGenerator.GenerateBatch -quit
```

`GenerateBatch` legt standardmäßig **keine** Szenen-Sicherung an, im Batch-Modus ist Git die Sicherung. Mit dem zusätzlichen Argument `-sandboxBackup` wird vorher eine Sicherung in `Backups/` angelegt (falls die Szene schon existiert). Der interaktive Weg über Menü oder Workshop-Fenster fragt bei vorhandener Szene immer nach und sichert vorher. Unity wird von der Methode selbst beendet: Exit-Code 0 bei Erfolg, 1 bei Fehler.

Weitere Einstiegspunkte (jeweils mit `-executeMethod`):

| Methode | Argumente | Ergebnis |
|---|---|---|
| `StudentSandbox.EditorTools.SandboxValidator.ValidateBatch` | `-sandboxReport <Datei>` (optional) | Prüfbericht als Text; Exit-Code 0 = keine Fehler, 1 = Fehler |
| `StudentSandbox.EditorTools.SandboxScreenshots.CaptureBatch` | `-sandboxShotDir <Ordner>` (optional) | PNGs (1600 × 900) aller Aussichtspunkte |
| `StudentSandbox.EditorTools.SandboxHierarchyDump.DumpBatch` | `-sandboxDump <Datei>`, `-sandboxDumpDepth <n>` (Standard 4) | Hierarchy mit Komponenten als Text |

Auf dem Entwicklungsrechner ist zusätzlich das Kommandozeilen-Werkzeug `unity` installiert:

```
unity run . -- -executeMethod StudentSandbox.EditorTools.SandboxGenerator.GenerateBatch
```

---

## 4. Szenenstruktur

### 4.1 Weltaufbau

```
                     04_PHYSICS_ZONE (0, 0, 25)
                               |
03_CREATIVE_ZONE ----- 02_CENTRAL_HUB ----- 05_LIGHT_ZONE
   (-25, 0, 0)            (0, 0, 0)           (25, 0, 0)
                               |
                   06_INTERACTION_ZONE (0, 0, -25)

   +z = Norden · Insel-Radius 42 m · Platz-Radius 9 m · Wege 4 m breit
```

Die Maße stehen als Konstanten in `SandboxContext`:

- Jeder Bereich ist 24 × 24 m groß (lokal x und z von -12 bis +12).
- Die Zonen-Wurzeln sind **nicht gedreht**. Lokale Achsen = Welt-Achsen.
- Der Boden der Bereiche endet auf y = 0.1, der Platz auf y = 0.12, die Wege auf y = 0.05.
- Der Eingang eines Bereichs zeigt immer zum zentralen Platz.

Flache Beläge liegen nie auf derselben Höhe. So gibt es kein Flimmern (Z-Fighting).

### 4.2 Hauptgruppen

| Gruppe | Untergruppen (Auswahl) |
|---|---|
| `00_WORLD_ROOT` | `Sun`, `Global Volume (Nachbearbeitung)` |
| `01_ENVIRONMENT` | `Island`, `Water`, `Paths` |
| `02_CENTRAL_HUB` | `Plaza`, `Fountain` (mit drehendem `Ornament`), `Benches`, `Plants`, `Lamps`, `Signs` |
| `03_CREATIVE_ZONE` | `StaticEnvironment` (u. a. `Pond`), `EditableObjects` (`Houses`, `Trees`, `Benches`, `Decoration`), `BuildArea` (`BuildingBlocks`, `ExampleTower`), `ColorPalette`, `Signs`, `EntranceArch` |
| `04_PHYSICS_ZONE` | `StaticEnvironment` (inkl. `EntranceArch`), `MarbleRun`, `DominoExperiment`, `ObstacleCourse`, `Controls`, `Signs`, `Decoration` |
| `05_LIGHT_ZONE` | `StaticEnvironment` (u. a. `Sundial`), `EntranceArch`, `Village`, `StreetLamps`, `ColorLab`, `ShadowLab`, `Controls`, `Signs` |
| `06_INTERACTION_ZONE` | `StaticEnvironment`, `DoorExperiment`, `TrafficLightExperiment`, `CrystalExperiment`, `PlatformExperiment`, `ControlDesk`, `Signs`, `EntranceArch` |
| `07_CAMERAS` | `Viewpoints` (`Viewpoint_1_Overview` … `Viewpoint_5_Interaction`), `Main Camera` |
| `08_MANAGERS` | `DayNightController`, `PhysicsResetter`, `SandboxHUD` |
| `09_DECORATION` | `Trees`, `Rocks`, `Flowers`, `Landmarks` (`Windmill`, `Lighthouse`, `Pier`, `Picnic`), `Clouds`, `Shore` |

Unbewegliche Umgebung ist mit `SB.MarkStatic` als statisch markiert (Batching, Occlusion): Insel, Wasser, Wege, Platz, Böden und Ränder der Bereiche, in `09_DECORATION` Bäume, Steine, Blumen, Steg, Picknickplatz und Ufer, im Baubereich auch Eingangsbogen, Schilder und Farbkasten, im Physiklabor der Eingangsbogen. Die Eingangsbögen von Licht- und Interaktionsbereich und die übrigen Schilder sind nicht statisch. Die Objekte unter `EditableObjects` und die Experimente sind ebenfalls nicht statisch.

### 4.3 Wichtige Objekte

Diese Namen nennen die Anleitungen, und viele davon suchen Workshop-Fenster, Validator und Tests direkt per Namen (die Liste, die der Validator prüft, steht in `SandboxValidator.WorkshopObjects`). Sie sollten nicht umbenannt werden.

| Bereich | Objekte |
|---|---|
| Creative | `Tree_Small_1`, `House_Red`, `Bench_Pond_1`, `BuildArea`, `ExampleTower`, `ColorPalette` |
| Physics | `PhysicsBall` (rot), `Ramp_Main`, `GoalZone`, `Goal_Backstop`, `Pin_1`–`Pin_3`, `Hammer`, `Domino_01`–`Domino_10`, `PhysicsBall_Blue`, `Course_Ramp`, `TargetZone`, `WallBlock_1/2`, `Bumper_1`, `SmallRamp_1`, `ObstacleCube_1`–`3`, `BouncyBall` |
| Light | `Sun` (in `00_WORLD_ROOT`), `ColorLight_Red/Green/Blue`, `ShadowSpotLight`, `ShadowSculpture`, `StreetLamp_1`–`6`, `TimeConsole` |
| Interaction | `AutomaticDoor`, `DoorSwitch`, `TreasureRoom`, `TrafficLight`, `PedestrianButton`, `RotatingCrystal` → `Crystal`, `Orbit_Ring`, `MovingPlatformSet` → `MovingPlatform`, `PointA`, `PointB`, `ControlDesk` |

Die Häuser haben eindeutige Namen: `House_Red` und `House_Blue` im Baubereich, `Village_House_Lavender` und `Village_House_Blue` im Dorf des Lichtlabors. Bei doppelten Namen würde „Zeig mir“ einfach das erste Objekt nehmen.

Kinder, die man leicht aus Versehen anklickt, haben eine eigene `InfoNote`, die auf das Eltern-Objekt verweist: die Glas-Kugel `Lamp` unter `ColorLight_Red/Green/Blue`, die Bodenscheibe `Marker` unter `PointA` und `PointB`, `Hammer_Head` unter `Hammer` und die Fahrbahn `Ramp_Surface` unter `Ramp_Main`. Die Notiz am Prefab `Signpost` erinnert daran, `Text_Front` und `Text_Front_Shadow` gemeinsam zu ändern.

### 4.4 Verdrahtung der Schalter

Alle Schalter sind `ClickableSwitch`-Komponenten. Die Aktionen sind dauerhafte Listener (`UnityEventTools.AddPersistentListener`) und im Inspector unter „On Pressed ()“ sichtbar.

| Schalter | Ort | On Pressed → |
|---|---|---|
| `ResetButton` | `04_PHYSICS_ZONE/Controls` | `PhysicsResetter.ResetAll` |
| `DominoButton` | `04_PHYSICS_ZONE/Controls` | `Hammer` → `DominoStarter.StartSwing` |
| `DayButton`, `SunsetButton`, `NightButton` | `05_LIGHT_ZONE/Controls` | `DayNightController.SetDay` / `SetSunset` / `SetNight` |
| `DoorSwitch` | `06_INTERACTION_ZONE/DoorExperiment` | `AutomaticDoor` → `DoorController.Toggle` |
| `PedestrianButton` (Kind von `TrafficLight`) | `TrafficLightExperiment` | `TrafficLightController.NextPhase` |
| `Desk_DoorButton` | `ControlDesk` | `DoorController.Toggle` |
| `Desk_TrafficButton` | `ControlDesk` | `TrafficLightController.NextPhase` |
| `Desk_TrafficAutoButton` | `ControlDesk` | `TrafficLightController.ToggleAutoCycle` |
| `Desk_CrystalButton` | `ControlDesk` | `Crystal` → `Rotator.ToggleRotation` |
| `Desk_PlatformButton` | `ControlDesk` | `MovingPlatform.ToggleMoving` |

---

## 5. Prefab-Erzeugung

### 5.1 Aufbau des Codes

`SandboxPrefabs` ist eine `static partial class`, verteilt auf fünf Dateien:

| Datei | Inhalt |
|---|---|
| `Core/SandboxPrefabs.Core.cs` | Namenslisten je Ordner, `FolderFor`, `PathFor`, `Load`, `CreateAll`, `NewRoot`, `Save` |
| `Prefabs/SandboxPrefabs.Creative.cs` | 15 Prefabs + gemeinsame Helfer (`AddBoxCollider`, `AddCapsuleCollider`, `AddConvexMeshCollider`, `CastNoShadow`, `RendererOf`) |
| `Prefabs/SandboxPrefabs.Physics.cs` | 9 Prefabs |
| `Prefabs/SandboxPrefabs.Interactions.cs` | 3 Prefabs |
| `Prefabs/SandboxPrefabs.Environment.cs` | 3 Prefabs |

`CreateAll()` ruft die partiellen Methoden `BuildCreativePrefabs`, `BuildPhysicsPrefabs`, `BuildInteractionPrefabs` und `BuildEnvironmentPrefabs` auf. Fast jede Bauanleitung legt mit `NewRoot` ein leeres Objekt an und baut die Kinder mit den `SB`-Helfern (Ausnahmen: `PhysicsBall` und `BouncyBall` entstehen aus einer Unity-Kugel per `GameObject.CreatePrimitive`, `PushButton` aus `SB.PushButton`). Gespeichert wird immer mit `Save` (`PrefabUtility.SaveAsPrefabAsset`, danach `DestroyImmediate`). Die Prefab-Namen stehen als Konstanten in `P`. Der Ordner ergibt sich aus den Namenslisten in `SandboxPrefabs.Core.cs`. Wer ein neues Prefab anlegt, muss den Namen in `P` **und** in der passenden Liste eintragen. Der Validator meldet sonst einen Fehler.

| Ordner | Prefabs |
|---|---|
| `Prefabs/Creative` | SmallTree, LargeTree, PineTree, Bush, SmallHouse, Bench, LampPost, Fence, Flower, Rock, ColoredCube, ColoredSphere, ColoredCylinder, Platform, DecorativePillar |
| `Prefabs/Physics` | PhysicsBall, BouncyBall, WallBlock, SmallRamp, TallRamp, Bumper, ObstacleCube, TargetZone, Domino |
| `Prefabs/Interactions` | PushButton, RotatingCrystal, MovingPlatformSet |
| `Prefabs/Environment` | Cloud, FlowerBed, Signpost |

In der Szene setzt `SB.Place` die Prefabs mit `PrefabUtility.InstantiatePrefab` ein. Die Instanzen bleiben mit dem Prefab verbunden (blaues Würfel-Symbol in der Hierarchy).

### 5.2 Konventionen

- Drehpunkt (Pivot) unten in der Mitte: y = 0 ist der Boden.
- „Vorne“ ist lokal **-z** (Haustür, Sitzrichtung der Bank, Beschriftungen).
- Die Wurzel hat Scale 1. Größen stecken in den Kindern.
- Sprechende Namen für Kinder: `Trunk`, `Crown`, `Roof`, `Walls`, `Door`, `Tile`, `Surface` …
- Mindestens ein Collider, damit Kugeln abprallen.
- Eine `InfoNote` auf der Wurzel. Im Prefab-Asset steht sie oben, denn bei Instanzen darf Unity die Reihenfolge der Komponenten nicht ändern.

### 5.3 Ausnahmen

| Prefab | Abweichung |
|---|---|
| `PhysicsBall`, `BouncyBall` | Die Kugel ist das Wurzel-Objekt selbst. Der Pivot liegt in der Kugelmitte, die Wurzel hat als Scale den Durchmesser (0.8 bzw. 0.7). Auf den Boden legen = Bodenhöhe + Radius (0.4 bzw. 0.35). |
| `Cloud` | Kein Collider, kein Schatten. |
| `MovingPlatformSet` | Wurzel unten, aber `PointA`, `PointB` und `MovingPlatform` liegen auf y = 0.6 (Mitte der Plattform). |
| `TargetZone` | Trigger-Collider auf der Wurzel; flaches Feld ohne Collider. |
| `Domino` | Box Collider und Rigidbody auf der Wurzel; das sichtbare Kind `Tile` hat keinen Collider. |
| `ObstacleCube` | Rigidbody auf der Wurzel, Collider am Kind `Cube` (zusammengesetzter Körper). |
| `Bumper` | Zusätzlicher kinematischer Rigidbody auf der Wurzel, damit das Skript `OnCollisionEnter` der Kind-Collider bekommt. |

---

## 6. Laufzeit-Komponenten

Alle Komponenten stehen im Menü „Add Component > Student Sandbox > …“. Die Feldnamen in der Tabelle sind so geschrieben, wie der Inspector sie zeigt. Alle Events sind parameterlose `UnityEvent`s.

Entfernt man im Inspector den Haken einer Komponente, ist sie wirklich aus: `ClickableSwitch` reagiert nicht auf Klicks und zeigt keinen Hover-Text, `GoalZone` zählt keine Kugel, `Bumper` stößt nicht. Die Skripte prüfen dafür `isActiveAndEnabled`, denn Unity meldet Trigger, Kollisionen und Raycast-Treffer auch bei ausgeschalteten Komponenten.

### 6.1 Kern und Kamera

| Klasse | Zweck | Wichtige Felder / Methoden |
|---|---|---|
| `SandboxInput` (statisch) | Einzige Stelle für Tastatur und Maus, siehe Abschnitt 7 | `GetKey`, `GetKeyDown`, `LeftClickDown`, `RightMouseHeld`, `MousePosition`, `MouseDelta`, `ScrollSteps`, `ActiveBackend` |
| `IClickable` | Interface: `OnClick()` – wird beim Linksklick aufgerufen | – |
| `IHoverable` | Interface: `HoverText`, `SetHovered(bool)` | – |
| `IResettable` | Interface: `ResetToStart()` – wird beim Reset aufgerufen | – |
| `InfoNote` | Erklär-Notiz im Inspector, ohne Wirkung im Spiel | Title, Text |
| `SandboxHUD` | IMGUI-Anzeige: Steuerungshilfe (Taste H), Hover-Text, kurze Meldungen. Lange Meldungen brechen um, der Kasten wächst mit dem Text. | Show Help, Ui Scale; `static ShowMessage(text, seconds)`, `LastMessage` |
| `Viewpoint` | Aussichtspunkt für Kamera und Scene View | Display Name, Focus Distance; `FocusPoint` |
| `FreeFlyCamera` | Flug-Kamera im Play Mode, Tasten 1–5 fliegen zu den Viewpoints. Im Editor startet sie dort, wohin die Scene View gerade schaut (in einem Build an ihrer gespeicherten Position). | Move Speed (10), Fast Multiplier (3), Min Height (0.5), Look Sensitivity, Invert Y, Start At Scene View (an), Viewpoints, Fly Duration (0.8 s); `FlyTo(int)` |

### 6.2 Interaktion

| Klasse | Zweck | Wichtige Felder / Methoden |
|---|---|---|
| `ClickInteractor` | Auf `Main Camera`. Raycast von der Maus aus in jedem Frame, Trigger werden ignoriert. Bei gedrückter rechter Maustaste wird nichts angeklickt. Sucht `IClickable`/`IHoverable` mit `GetComponentInParent` und überspringt ausgeschaltete Komponenten. | Max Distance (250), Clickable Layers; `CurrentHoverText` |
| `ClickableSwitch` | Knopf: `Press()` zählt, animiert den Deckel und löst „On Pressed“ aus (nur wenn die Komponente an ist) | Label, On Pressed, Button Cap, Press Depth, Press Duration, Hover Scale, Press Count |
| `DoorController` | Öffnet und schließt sanft. Geschlossen = Pose beim Start (`Awake`). | Door, Motion (Slide/Rotate), Slide Offset, Open Angle, Hinge Axis, Open Speed, Start Open, Is Open, On Opened, On Closed; `Open`, `Close`, `Toggle` |
| `TrafficLightController` | Zustandsmaschine mit einer Liste von Phasen. Tauscht die Lampen-Materialien. Ändert man „Current Phase“ oder die Häkchen Red/Yellow/Green einer Phase im Inspector, zeigen die Lampen das sofort (Edit und Play Mode). | Red/Yellow/Green Lamp, Red/Yellow/Green On/Off, Phases (Name, Red, Yellow, Green, Duration), Auto Cycle, Current Phase, On Phase Changed; `NextPhase`, `SetPhase(int)`, `ToggleAutoCycle` |
| `Rotator` | Dreht gleichmäßig um eine lokale Achse | Speed (0–720 °/s, Standard 45), Rotation Axis, Is Rotating; `ToggleRotation`, `StartRotating`, `StopRotating`, `DoubleSpeed` |
| `MovingPlatform` | Fährt mit SmoothStep zwischen zwei Punkten hin und her, startet immer bei Point A | Point A, Point B, Speed (2 m/s), Wait Time (1 s), Is Moving; `ToggleMoving` |

Die Standard-Ampel hat vier Phasen: Rot (4 s), Rot-Gelb (1,5 s), Grün (4 s), Gelb (1,5 s).

### 6.3 Licht

| Klasse | Zweck | Wichtige Felder / Methoden |
|---|---|---|
| `DayNightController` | Wechselt Tag → Sonnenuntergang → Nacht. Setzt Sonne, Umgebungslicht, Himmel, Nebelfarbe und alle Lampen mit „Follow Day Night“. Taste T im Play Mode. „Apply On Start“ ist aus: Beim Drücken von Play bleibt das Licht so, wie es im Editor eingestellt ist. | Sun, Time Of Day, Day / Sunset / Night (je ein `LightingPreset`), Control Fog, Apply On Start (aus), Allow Keyboard Switch, On Time Changed; `SetDay`, `SetSunset`, `SetNight`, `NextTimeOfDay`, `Apply(TimeOfDay)` |
| `LightingPreset` | Daten einer Tageszeit | Display Name, Sun Rotation, Sun Color, Sun Intensity, Sky Color, Equator Color, Ground Color, Skybox, Fog Color, Lamps On |
| `LampController` | Schaltet ein Licht und tauscht das Material der leuchtenden Teile | Lamp Light, Glow Renderers, On Material, Off Material, Is On, Follow Day Night; `TurnOn`, `TurnOff`, `Toggle`, `SetOn(bool)` |

### 6.4 Physik

| Klasse | Zweck | Wichtige Felder / Methoden |
|---|---|---|
| `ResettableObject` | Merkt sich in `Awake` Position und Drehung; `ResetToStart` setzt zurück und stoppt jede Bewegung | – |
| `PhysicsBall` (erbt von `ResettableObject`) | Markiert eine Kugel; holt sie zurück, wenn sie unter die Grenze fällt | Display Name, Auto Reset When Fallen, Fall Limit Y (-5) |
| `GoalZone` | Trigger-Ziel: Material wechselt auf grün, Licht an, Meldung, Event | Required Ball, Indicator, Idle Material, Reached Material, Success Light, Success Message, On Goal Reached, Goal Reached |
| `Bumper` | Stößt Rigidbodys beim Aufprall waagerecht weg (`ForceMode.VelocityChange`), Kopf pulsiert | Bounce Strength (5), Visual, Pulse Scale |
| `DominoStarter` | Schwingt den kinematischen Hammer (`MoveRotation` in `FixedUpdate`). Automatisch nur einmal direkt nach Play, nach einem Reset nur über `StartSwing` (DOMINO-Knopf). | Hammer, Swing Axis, Swing Angle (100°), Swing Speed (90 °/s), Start Automatically, Start Delay (1,5 s); `StartSwing` |
| `PhysicsResetter` | Taste R und RESET-Knopf: ruft `ResetToStart` auf allen `IResettable` auf aktiven GameObjects auf | Allow Keyboard Reset, Reset Message; `ResetAll`, `LastResetCount` |

Wer welche Komponente in der Szene benutzt:

- `Rotator`: `Crystal`, `Orbit_Ring`, `TreasureStar`, `Ornament` (Brunnen), `Windmill/Blades`.
- `LampController` (24×): Fenster der Häuser, alle Laternen, `Stage_Marquee`, `Windmill`, `Lighthouse`.
- `IResettable`: rote und blaue Kugel, `BouncyBall`, Kegel, Dominosteine, Hindernis-Würfel (alle über `ResettableObject`), beide `GoalZone`s, der Hammer (`DominoStarter`).

---

## 7. Eingabesystem

Das Projekt steht auf „Active Input Handling = Input System Package“. Alle Sandbox-Skripte lesen Tastatur und Maus **nur** über `SandboxInput`. Die Sandbox benutzt kein Input-Action-Asset (das `Assets/InputSystem_Actions.inputactions` aus der Projektvorlage bleibt unbenutzt). Die Klasse fragt direkt `Keyboard.current` und `Mouse.current` ab.

`SandboxInput` hat drei Varianten, die beim Kompilieren ausgewählt werden:

| Bedingung | Backend | `ActiveBackend` |
|---|---|---|
| `ENABLE_INPUT_SYSTEM && SANDBOX_INPUT_SYSTEM_PACKAGE` | Input System (wird in diesem Projekt benutzt) | „Input System Package“ |
| sonst `ENABLE_LEGACY_INPUT_MANAGER` | alter Input Manager (`Input.GetKey` …, Mausbewegung × 10) | „Input Manager (alt)“ |
| sonst | keine Eingabe; der Editor funktioniert trotzdem | „keins“ |

Steht ein Projekt auf „Both“, gewinnt das Input System.

Tasten (`SandboxKey`) und wer sie benutzt:

| Eingabe | Komponente | Wirkung |
|---|---|---|
| W A S D, Q / E, Shift | `FreeFlyCamera` | fliegen, runter / hoch, schneller |
| rechte Maustaste halten | `FreeFlyCamera` | umschauen (Cursor wird gesperrt) |
| Mausrad | `FreeFlyCamera` | Fluggeschwindigkeit × 1,15 bzw. ÷ 1,15 |
| 1–5 (auch Ziffernblock) | `FreeFlyCamera` | zu Viewpoint 1–5 fliegen |
| Esc | `FreeFlyCamera` | Cursor freigeben |
| Linksklick | `ClickInteractor` | `IClickable.OnClick()` |
| R | `PhysicsResetter` | Physik zurücksetzen |
| T | `DayNightController` | nächste Tageszeit |
| H | `SandboxHUD` | Hilfe ein/aus |
| Leertaste | – | in `SandboxKey` vorhanden, aber noch von keinem Skript benutzt |

Die Kamera nutzt `Time.unscaledDeltaTime`, fliegt also auch bei Time Scale 0. Klicks und Tasten kommen nur an, wenn das Game-Fenster den Fokus hat.

---

## 8. Materialstrategie

- **Shader-Wahl:** `SandboxMaterials.LitShader` nimmt „Universal Render Pipeline/Lit“, wenn eine Render Pipeline aktiv ist, sonst „Standard“.
- **Eigenschaften:** Gesetzt werden `_BaseColor` und `_Color`, `_Smoothness` und `_Glossiness`, `_Metallic`. Leuchtende Materialien bekommen `_EmissionColor` × Intensität und das Keyword `_EMISSION`. GPU Instancing ist an.
- **Keine Texturen:** Alle Materialien sind reine Farben (Hex-Werte in `SandboxMaterials.Defs`). Einzige Ausnahme ist `Text3D`: Es nutzt die Schrift-Textur von `LegacyRuntime.ttf`.
- **In-place-Update:** Jedes Material wird frisch gebaut und mit `EditorUtility.CopySerialized` komplett in das vorhandene Asset kopiert (`SandboxMaterials.Commit`). Dabei verschwinden auch von Hand geänderte Dinge wie Transparenz oder hineingezogene Texturen. Die GUID bleibt.
- **Zustandswechsel** (Ampel, Lampen, Ziel) tauschen über `sharedMaterial` ganze Material-Assets aus, zum Beispiel `Traffic_RedOn` ↔ `Traffic_RedOff`. Es entstehen keine Material-Instanzen.

| Ordner | Anzahl | Inhalt |
|---|---|---|
| `Materials/Palette` | 16 | Farben für die Schülerin: CoralRed, SoftYellow, MintGreen, SkyBlue, Lavender, WarmOrange, LightWood, DarkWood, GrassGreen, StoneGray, WarmWhite, Turquoise, PinkBlossom, Charcoal, LeafGreen, BerryPurple |
| `Materials/Environment` | 32 | `Env_*`: Gras, Sand, Wasser, Dächer, Wände, Metall, Böden … |
| `Materials/Effects` | 29 | Leuchtende Materialien (`Glow_*`, `Traffic_*`, `Goal_*`, `Neon_*`, `Button_*`, `Crystal`) und `Text3D` |
| `Materials/Sky` | 3 | `Sky_Day`, `Sky_Sunset`, `Sky_Night` |

Im Code werden Materialien nur über die Konstanten in `M` angesprochen (`SB.Mat(M.CoralRed)`). Ein unbekannter Schlüssel wirft eine `ArgumentException`.

Eigene Shader in `Assets/StudentSandbox/Shaders/`:

- `StudentSandbox/GradientSky` – Farbverlauf oben/Horizont/unten, Sonnenglanz, optionale Sterne (`_StarDensity`). Die Sonnenrichtung kommt vom Haupt-Licht.
- `StudentSandbox/Text3D` – wie Unitys „GUI/Text Shader“, aber mit Tiefentest (`ZTest LEqual`). Unter URP scheint der eingebaute Schrift-Shader sonst durch Wände.

---

## 9. Low-Poly-Meshes

`SandboxMeshes.CreateAll()` erzeugt 17 Meshes per Code und speichert sie als `Meshes/LP_*.asset`. Alle Flächen sind flach schattiert: Jedes Dreieck hat eigene Eckpunkte. Das ergibt den facettierten Look. Vorhandene Assets werden mit `EditorUtility.CopySerialized` überschrieben, die GUID bleibt.

| Art | Meshes (Konstanten in `MK`) | Konvention |
|---|---|---|
| zentriert | `LP_Sphere`, `LP_BlobA/B/C`, `LP_Rock`, `LP_Crystal`, `LP_Star`, `LP_Torus`, `LP_ArrowBoard` | liegen um den Ursprung, etwa 1 m Durchmesser |
| Sockel | `LP_Cone6`, `LP_Cone8`, `LP_Pyramid`, `LP_Prism`, `LP_Wedge`, `LP_Cylinder6/8/12` | stehen auf y = 0, Höhe 1, Grundfläche 1 × 1 |

`LP_Wedge` ist die Rampe: hohe Kante bei x = -0.5, flaches Ende bei x = +0.5. Die drei Blobs sind leicht verbeulte, einmal unterteilte Ikosaeder (wie `LP_Sphere`) mit unterschiedlichem Startwert (für Baumkronen, Büsche, Wolken).

---

## 10. Physikstrategie

### 10.1 Grundlagen

- 1 Unity-Einheit = 1 Meter. Massen in kg.
- Projekteinstellungen sind Unity-Standard und wurden nicht geändert: Schwerkraft (0, -9,81, 0), Fixed Timestep 0,02 s, Bounce Threshold 2.
- Alle Objekte haben echte Maße: Kugel Ø 0,8 m, Dominostein 1,2 × 0,6 × 0,2 m, Abstand der Dominos 0,65 m.

### 10.2 Physik-Materialien

| Material | Dynamic Friction | Static Friction | Bounciness | Combine |
|---|---|---|---|---|
| `Slippery` | 0.02 | 0.02 | 0 | Friction: Minimum |
| `Normal` | 0.5 | 0.6 | 0 | Average |
| `Bouncy` | 0.4 | 0.4 | 0.9 | Bounce: Maximum |

`Normal` liegt auf Böden, Rampen, Bahnen, Mauern und Kugeln. `Bouncy` haben der Flummi und der Bumper. `Slippery` wird im Generator nicht benutzt und liegt zum Ausprobieren bereit.

### 10.3 Rigidbody-Einstellungen

| Objekt | Mass | Collider | Kollisionserkennung | Sonstiges |
|---|---|---|---|---|
| `PhysicsBall`, `PhysicsBall_Blue` | 1 | Sphere | Continuous Dynamic | Interpolate, Linear Damping 0.05, Angular Damping 0.4 |
| `BouncyBall` | 0.5 | Sphere (Bouncy) | Continuous Dynamic | wie oben |
| `Pin_1`–`3` | 0.2 | konvexer Mesh Collider am Kind `Pin_Body` | Continuous Speculative | Interpolate, `ResettableObject` |
| `Domino_01`–`10` | 0.3 | Box auf der Wurzel | Discrete (Standard) | Interpolate, `ResettableObject` |
| `ObstacleCube_1`–`3` | 0.5 | Box am Kind | Discrete (Standard) | Interpolate, `ResettableObject` |
| `Hammer` | kinematisch | Box an `Hammer_Arm` und `Hammer_Head` | – | bewegt über `MoveRotation` |
| `MovingPlatform` | kinematisch | Box auf `MovingPlatform` selbst | – | bewegt über `MovePosition` in `FixedUpdate` |
| `Bumper_1` | kinematisch | konvexe Mesh Collider (Bouncy) | Continuous Speculative | keine Schwerkraft |

Kinematische Körper werden von Skripten bewegt, schieben aber andere Rigidbodys an. Darum funktionieren Hammer und Plattform zuverlässig mit der Physik.

### 10.4 Murmelbahn, Ziele, Parcours

- `Ramp_Main` ist ein leeres Drehpunkt-Objekt oben am Startturm (Rotation Z = -22). Bahn 8 m lang, 2,4 m breit, Bande 0,5 m hoch. Die Stützpfosten reichen tief unter den Boden, damit beim Drehen keine Lücke sichtbar wird.
- `PhysicsBall` startet 0,9 m unterhalb der Rampenkante, 5 cm über der Fläche. Sie braucht etwa 4 s bis ins Ziel.
- Getestet: Rotation Z = -12, -15, -22 und -30 – die Kugel erreicht jedes Mal das Ziel.
- Die Ziel-Erkennung: `GoalZone` (Box Collider mit „Is Trigger“) zählt nur Objekte mit `PhysicsBall`-Komponente. Das Haupt-Ziel hat „Required Ball“ = rote Kugel. `TargetZone` im Parcours lässt das Feld leer und akzeptiert jede Kugel, auch den Flummi.
- Die drei Kegel `Pin_1`–`Pin_3` stehen **im** Zielbereich, hinter dem Ziel-Trigger. Die Kugel löst also immer zuerst das Ziel aus und räumt danach die Kegel ab. `Goal_Backstop` hält die Kugel auf.
- Parcours: `PhysicsBall_Blue` rollt von `Course_Ramp` (TallRamp) Richtung Südwand und verfehlt `TargetZone`. Die Aufgabe ist, Mauern, Bumper, kleine Rampe oder Würfel so umzustellen, dass die Kugel ins Ziel rollt.

### 10.5 Sicherheitsnetz

`PhysicsBall` prüft in `FixedUpdate`, ob die Kugel unter y = -5 gefallen ist (zum Beispiel ins Wasser – das Wasser hat keinen Collider). Dann springt sie an ihren Start zurück und das HUD meldet „… ist zurück am Start“. Das gilt nur für Kugeln, nicht für Dominos oder Würfel.

---

## 11. Reset-Verhalten

- **Startposition:** `ResettableObject.Awake` speichert Position und Drehung beim Drücken von Play. Änderungen aus dem Edit Mode bleiben also erhalten, der Reset kehrt nur zum Zustand bei Play-Beginn zurück.
- **Auslöser:** Taste R, der Knopf `ResetButton`, „Alles zurücksetzen“ im Inspector von `PhysicsResetter` oder „Physik zurücksetzen“ im Workshop-Fenster (nur im Play Mode).
- **Was passiert:** `PhysicsResetter.ResetAll` sucht alle `MonoBehaviour`s mit `IResettable` auf aktiven GameObjects und ruft `ResetToStart()` auf:
  - Kugeln, Kegel, Dominos, Würfel: zurück an den Start, Geschwindigkeit 0, `WakeUp()`.
  - `GoalZone`: wieder rot, Erfolgslicht aus.
  - `DominoStarter`: Hammer zurück in die Ausholposition. Danach **wartet** er. „Start Automatically“ (nach „Start Delay“ = 1,5 s) gilt nur einmal direkt nach Play. Erst der Knopf `DominoButton` (Aufschrift DOMINO) lässt ihn wieder schwingen. Drückt man DOMINO, obwohl der Hammer schon geschwungen hat, meldet das HUD „Erst RESET drücken (Taste R)“.
- **Nicht zurückgesetzt:** Tür, Ampel, Rotatoren, Plattform, Lampen, Tageszeit.
- **Eigene Objekte:** Ein Würfel, dem man im Workshop einen Rigidbody gibt, wird nur zurückgesetzt, wenn er zusätzlich „Add Component > Student Sandbox > Resettable Object“ bekommt.
- **Play Mode beenden:** Alle Änderungen aus dem Play Mode gehen verloren (normales Unity-Verhalten).

---

## 12. Licht

- **Nur Echtzeit-Licht, nichts wird gebacken.** `LightingSetup` legt `Settings/StudentSandbox_Lighting.lighting` an (Baked GI und Realtime GI aus) und weist es der Szene zu.
- **Sonne:** `Sun` in `00_WORLD_ROOT`, Directional Light mit weichen Schatten (Strength 0.75). Sie ist `RenderSettings.sun`.
- **Umgebungslicht:** Ambient Mode Trilight. Sky, Equator und Ground Color kommen aus dem jeweiligen Preset.
- **Himmel:** drei Materialien mit dem Shader `StudentSandbox/GradientSky`. Fehlt der Shader, fällt der Generator auf Unitys „Skybox/Procedural“ zurück.
- **Nebel:** linear von 70 bis 230 m, Farbe je Tageszeit (wenn „Control Fog“ an ist).
- **Reflexionen:** Default Reflection Mode Skybox, Intensität 0.5. Nach jedem Wechsel ruft `Apply` `DynamicGI.UpdateEnvironment()` auf.
- **Play übernimmt den Editor-Zustand:** Weil „Apply On Start“ aus ist, bleiben Änderungen an der Sonne (Rotation, Color, Intensity) und an „Is On“ der Lampen beim Drücken von Play erhalten. Erst ein Tageszeit-Wechsel setzt die Sonne wieder auf die Preset-Werte: die Knöpfe TAG / SONNENUNTERGANG / NACHT im Inspector, Tag / Sonnenuntergang / Nacht im Workshop-Fenster, die 3D-Knöpfe TAG / ABEND / NACHT im Lichtlabor oder Taste T. Auch jede Änderung im Inspector des `DayNightController` (zum Beispiel Sun Intensity im Preset Day) wendet die aktuelle Tageszeit sofort neu an.
- **Undo im Edit Mode:** Ein Tageszeit-Wechsel lässt sich mit Strg+Z vollständig zurücknehmen, also Sonne, Lampen, Himmel, Umgebungslicht und Nebel (`DayNightEditorUtility` zeichnet auch die RenderSettings auf).

| Preset | Sun Rotation | Sun Intensity | Himmel | Lampen |
|---|---|---|---|---|
| Tag | (50, 35, 0) | 1.35 | `Sky_Day` | aus |
| Sonnenuntergang | (9, 70, 0) | 1.0 | `Sky_Sunset` | an |
| Nacht | (55, 210, 0) | 0.22, bläulich | `Sky_Night` (mit Sternen) | an |

Weitere Lichter:

| Licht | Typ | Schatten | Werte |
|---|---|---|---|
| `LampLight` in jeder Laterne | Point | keine | Intensity 4, Range 10 |
| `ColorLight_Red/Green/Blue` | Point | weich | Intensity 5, Range 8 |
| `ShadowSpotLight` | Spot | weich | Intensity 18, Range 14, Inner / Outer Spot Angle 36 / 60 (im URP-Inspector ein Doppel-Schieberegler) |
| `SuccessLight` in den Zielen | Point | keine | nur an, wenn das Ziel erreicht ist |

Leuchtende Teile (Glühbirnen, Fenster, Neon) sind emissive Materialien. Mit dem Bloom im Post-Processing wirken sie hell.

**Post-Processing:** `Global Volume (Nachbearbeitung)` in `00_WORLD_ROOT`, Profil `Settings/StudentSandbox_PostProcessing.asset`:

- Tonemapping Neutral
- Bloom: Threshold 1.0, Intensity 0.55, Scatter 0.6
- Vignette: Intensity 0.16, Smoothness 0.4
- Color Adjustments: Saturation +14, Contrast +9

**Kamera:** `Main Camera` mit Field of View 55, Far Clip 600 m, Post-Processing an, Anti-Aliasing SMAA.

---

## 13. Render-Pipeline-Kompatibilität

- Gebaut und getestet ist die Spielwiese nur mit **URP** (Forward+).
- Materialien folgen der aktiven Pipeline: URP Lit oder Built-in Standard (`SandboxMaterials.LitShader`). Die Farben werden in beide Property-Namen geschrieben.
- Die beiden eigenen Shader sind einfache, unbeleuchtete `CGPROGRAM`-Shader ohne URP-spezifischen Pass. Getestet sind sie nur unter URP.
- Die Editor-Assembly verweist fest auf URP (`UniversalAdditionalLightData`, `UniversalAdditionalCameraData`, `Volume`, `Bloom` …). Ohne URP-Paket kompiliert der Generator nicht. Die Runtime-Assembly braucht URP nicht.
- Der Renderer des Projekts nutzt SSAO. Das ist eine Projekteinstellung und wurde nicht verändert.

---

## 14. Editor-Werkzeuge

| Klasse | Aufgabe |
|---|---|
| `WorkshopWindow` | IMGUI-Fenster mit Bereichs-Knöpfen („Zum Baubereich“ …, „Zur Weltübersicht“), sechs Lektionen mit „Zeig mir“-Knöpfen und Häkchen, Tageszeit-Knöpfen und Werkzeugen (Sicherung, Wiederherstellen, Neu erzeugen, „Anleitung öffnen“ → `WORKSHOP_DE.md`). Erkennt den Prefab-Modus: Dann erscheint „Du bearbeitest gerade ein Prefab“ mit dem Knopf „Zurück zur Spielwiese“, und die Bereichs-Knöpfe sind gesperrt. „Ausgewähltes Objekt zeigen“ erklärt, wenn ein Prefab im Project-Fenster ausgewählt ist. |
| `WorkshopLessons` | Inhalt der sechs Lektionen (`WorkshopLesson`, `ShowMeTarget`). „Zeig mir: Sonne“ wählt die Sonne nur aus, die Scene View bleibt stehen. Lektion 4 hat „Blaues Licht“ (`ColorLight_Blue`), Lektion 5 „Punkt B“ (`PointB`). Häkchen liegen in den EditorPrefs unter `StudentSandbox.Lesson<n>.Step<m>`. |
| `DayNightEditorUtility` | Tageszeit und Lampen im Edit Mode schalten – mit Undo für Controller, Sonne, Lampen und RenderSettings (Himmel, Umgebungslicht, Nebel), inklusive Prefab-Overrides und `MarkSceneDirty`. |
| `SandboxSceneView` | `[InitializeOnLoad]`: Beim Öffnen der Spielwiese wird die Scene View auf die Übersicht ausgerichtet. Beim Öffnen der Ausgangskopie erscheint der Dialog „Das ist die Ausgangskopie“. Außerdem `FrameViewpoint`, `Select`, `SelectAndFrame`, `FindInScene`. |
| Eigene Inspectoren | `DayNightController` (TAG / SONNENUNTERGANG / NACHT), `InfoNote` (Notiz-Karte), `DoorController`, `TrafficLightController`, `ClickableSwitch` (zeigt die On-Pressed-Einträge, Test-Knopf im Play Mode), `Rotator`, `PhysicsResetter`, `MovingPlatform`, `LampController`. Darunter steht jeweils der normale Inspector (bei `InfoNote` stattdessen die zugeklappte Gruppe „Text bearbeiten“). |

---

## 15. Validierung und Tests

### 15.1 Validator

`Tools > Student Sandbox > Validate Sandbox` öffnet bei Bedarf die Szene, schreibt den Bericht in die Console und zeigt eine Zusammenfassung. Jede Zeile ist OK, WARNUNG oder FEHLER. Ganz oben im Bericht steht das Ergebnis: BESTANDEN (keine Fehler, Warnungen sind erlaubt) oder NICHT BESTANDEN.

| Kategorie | Geprüft wird (Auswahl) |
|---|---|
| Dateien | Szene, Ausgangskopie, alle Ordner, Volume-Profil; Warnung, wenn `Documentation/` fehlt |
| Prefabs | alle Namen aus `P` eingetragen und ladbar, keine fehlenden Skripte, Collider vorhanden (außer `Cloud`), Pflicht-Komponenten wie der Trigger von `TargetZone` |
| Materialien | mindestens 10 Palette-Materialien, alle Schlüssel vorhanden, keine kaputten Shader, Himmel, Physik-Materialien, Bounciness von `Bouncy` |
| Meshes | alle 17 Meshes vorhanden |
| Skripte | richtige Assembly für jede Datei, kein `UnityEditor` außerhalb von `#if UNITY_EDITOR`, Editor-Assembly nicht im Build |
| Hierarchie | zehn Hauptgruppen vorhanden und nicht leer, Bereiche nicht verschoben/gedreht/skaliert (Warnung), keine fehlenden Skripte, mindestens 20 Info Notes |
| Kamera | genau eine `MainCamera` mit `FreeFlyCamera` und `ClickInteractor`, 5 Viewpoints, genau ein Audio Listener |
| Licht | DayNightController mit Directional Light, Himmel je Preset, Nacht dunkler als Tag, HUD, Resetter, Lampen, farbige Lichter, Spot Light mit Schatten |
| Physik | Böden auf y = 0.1, rote Kugel liegt auf der Rampe, Ziele sind Trigger, Required Ball, 10 Dominos mit Abstand, Hammer kinematisch |
| Interaktion | Tür, Schalter verdrahtet, Ampel mit Phasen und Materialien, Kristall, Plattform kinematisch |
| Schalter | jeder `ClickableSwitch` hat Collider und gültige On-Pressed-Einträge |
| Darstellung | kein Renderer ohne Material oder mit kaputtem (pinkem) Shader |
| Namen | keine Standard-Namen wie „Cube (3)“ (Warnung) |
| Workshop | alle 30 Objekte, die Workshop-Fenster und Tests per Namen suchen, sind vorhanden und eindeutig |

Echtes Ergebnis im Batch-Modus: **BESTANDEN – 0 Fehler, 0 Warnungen, 65 Prüfungen OK.**

### 15.2 Tests

**EditMode, `SandboxEditModeTests` (8 Tests, alle bestanden):** Szene öffnet sich, Validator ohne Fehler, Baum lässt sich verschieben, Materialwechsel wirkt, Tag/Nacht ändert das Licht, Szene übersteht erneutes Öffnen, alle Prefabs vorhanden, Palette hat mindestens 10 Materialien. Die Tests öffnen die Szene in `[OneTimeSetUp]` und laden sie am Ende neu von der Festplatte. Es wird nichts gespeichert.

**EditMode, `SandboxRestoreTests` (1 Test, `[Explicit]`, bestanden, wenn gezielt gestartet):** `RestoreOriginalSceneResetsSceneAndAssets` ändert das Dach-Material `Env_RoofRed` (Farbe und Transparenz) und verschiebt `Tree_Small_1`, speichert, stellt die Ausgangsszene wieder her und prüft Szene, Material und die neue Sicherung. Danach löscht er seine Sicherung wieder. Weil er Dateien schreibt, läuft er bei „Run All“ nicht mit.

**PlayMode (15 Tests, alle bestanden):** BallStartsRolling, BallReachesGoal, BallReachesGoalWithSteeperRamp (-30), BallReachesGoalWithFlatterRamp (-15), BallReachesGoalWithVeryFlatRamp (-12), ResetRestoresBall, DominoesFall, DominoButtonRestartsHammerAfterReset, DayNightWorksInPlayMode, DoorSwitchOpensDoor, TrafficLightAdvances, RotatorSpeedControlsRotation, MovingPlatformMoves, SwitchesAreClickableByRaycast, BlueBallTriggersTargetZoneInItsPath. Die Szene wird mit `EditorSceneManager.LoadSceneAsyncInPlayMode` geladen. Sie muss dafür nicht in den Build Settings stehen. In einem Player-Build werden diese Tests übersprungen.

**Ausführen:**

1. In Unity: `Window > General > Test Runner`, Reiter EditMode oder PlayMode, „Run All“.
2. Kommandozeile (ohne `-quit`, Unity beendet sich nach den Tests selbst):

```
Unity.exe -batchmode -projectPath <Pfad> -runTests -testPlatform EditMode -testResults editmode.xml
Unity.exe -batchmode -projectPath <Pfad> -runTests -testPlatform PlayMode -testResults playmode.xml
Unity.exe -batchmode -projectPath <Pfad> -executeMethod StudentSandbox.EditorTools.SandboxValidator.ValidateBatch -sandboxReport report.txt -quit
```

3. Mit dem installierten `unity`-Werkzeug: `unity test . --mode EditMode` und `unity test . --mode PlayMode`. Den Wiederherstellungs-Test gezielt starten: `unity test . --mode EditMode --filter RestoreOriginalSceneResetsSceneAndAssets`.

Nicht automatisch geprüft: Das Workshop-Fenster und die eigenen Inspectoren wurden kompiliert, aber nie auf dem Bildschirm gesehen (der Batch-Modus hat keine Oberfläche). Echte Maus- und Tastatureingaben im Play Mode wurden nicht simuliert. Klicks sind über Raycast-Tests und direkte `Press()`-Aufrufe abgedeckt.

---

## 16. Erweiterbarkeit für die KI-Demo

In der KI-Demo beschreibt die Schülerin eine Idee auf Deutsch, und ein Coding-Agent setzt sie um. Damit das sauber klappt, gelten diese Regeln.

### 16.1 Wo neuer Code hinkommt

| Was | Ordner | Assembly / Namespace |
|---|---|---|
| Neues Spielverhalten | `Assets/StudentSandbox/Scripts/Runtime/<Bereich>/` | `StudentSandbox.Runtime`, Namespace `StudentSandbox` |
| Editor-Werkzeuge, Generator-Erweiterungen | `Assets/StudentSandbox/Scripts/Editor/…` | `StudentSandbox.Editor`, Namespace `StudentSandbox.EditorTools` |
| Tests | `Assets/StudentSandbox/Tests/EditMode` bzw. `PlayMode` | Test-Assemblies |

Neue Dateien in diesen Ordnern landen automatisch in der richtigen Assembly. Der Validator prüft die Zuordnung.

Checkliste für neue Laufzeit-Skripte:

- Eingaben nur über `SandboxInput`. Für eine neue Taste: Wert in `SandboxKey` ergänzen und in **beiden** Zuordnungen (`ToKeyControl` und `ToKeyCode`) eintragen.
- Kein `UnityEditor` außerhalb von `#if UNITY_EDITOR`.
- `[AddComponentMenu("Student Sandbox/…")]` setzen, damit die Komponente im Add-Component-Menü neben den anderen steht.
- Felder als `[SerializeField]` mit `[Tooltip]` auf Deutsch. Unity zeigt `jumpHeight` als „Jump Height“ an.
- Rückmeldung an den Spieler mit `SandboxHUD.ShowMessage("…", 2f)`.
- Bei Bedarf eine `InfoNote` an das neue Objekt hängen.

### 16.2 Andockpunkte

| Andockpunkt | Wie |
|---|---|
| `IClickable` | `OnClick()` implementieren; das Objekt selbst oder eines seiner Kinder braucht einen Collider, der kein Trigger ist (`ClickInteractor` sucht vom getroffenen Collider aus mit `GetComponentInParent`). `ClickInteractor` findet es automatisch und überspringt es, wenn die Komponente ausgeschaltet ist. |
| `IHoverable` | `HoverText` erscheint neben dem Mauszeiger als „Klick: …“. |
| `IResettable` | `ResetToStart()` wird bei Taste R aufgerufen. Für einfache Objekte reicht die Komponente `ResettableObject`. |
| `ClickableSwitch` → „On Pressed“ | Jede öffentliche `void`-Methode ohne Parameter (oder mit einem einfachen Parameter) kann im Inspector eingetragen werden. |
| `GoalZone` → „On Goal Reached“ | Ziel erreicht |
| `DoorController` → „On Opened“, „On Closed“ | Tür ganz offen bzw. ganz zu |
| `TrafficLightController` → „On Phase Changed“ | Ampel hat gewechselt; danach `IsRed`, `IsYellow`, `IsGreen` abfragen |
| `DayNightController` → „On Time Changed“ | Tageszeit gewechselt; `CurrentTime` abfragen |

Fertige Methoden für Events: `DoorController.Open/Close/Toggle`, `TrafficLightController.NextPhase/ToggleAutoCycle`, `Rotator.ToggleRotation/StartRotating/StopRotating/DoubleSpeed`, `MovingPlatform.ToggleMoving`, `LampController.TurnOn/TurnOff/Toggle`, `DayNightController.SetDay/SetSunset/SetNight/NextTimeOfDay`, `DominoStarter.StartSwing`, `PhysicsResetter.ResetAll`.

### 16.3 Szene von Hand ändern oder Generator erweitern?

- **Für die Demo:** Das neue Skript schreiben und in der offenen Szene auf ein Objekt legen. Das geht schnell und ist im Inspector sichtbar. Achtung: „Spielwiese neu erzeugen“ und „Ausgangsszene wiederherstellen“ überschreiben die Szene (eine Sicherung wird vorher angelegt). Neue Skripte und neu angelegte eigene Assets bleiben erhalten, zurückgesetzt werden nur die generierten Assets.
- **Dauerhaft:** Den passenden Builder erweitern. Die `SB`-Helfer halten den Stil einheitlich:

| Helfer | Zweck |
|---|---|
| `SB.Group(name, parent, pos, yaw)` | leeres Ordnungs-Objekt |
| `SB.Box`, `SB.Prim`, `SB.MeshObj` | Würfel, Unity-Grundkörper, Low-Poly-Mesh mit Material und Collider-Wahl |
| `SB.Place(prefab, parent, pos, yaw, scale, name)` | Prefab-Instanz einsetzen |
| `SB.Recolor(root, childName, materialKey)` | Material eines Kindes tauschen |
| `SB.PointLight`, `SB.SpotLight` | Lichter |
| `SB.Text`, `SB.Sign`, `SB.EntranceArch` | 3D-Schrift, Schild, Eingangsbogen |
| `SB.PushButton(...)` | fertiger Knopf mit `ClickableSwitch` |
| `SB.Listen(event, action)` | Event dauerhaft verdrahten (sichtbar im Inspector) |
| `SB.Note(go, title, text)` | Erklär-Notiz |
| `SB.MarkStatic`, `SB.NoShadows`, `SB.SetPhysicsMaterial` | Feinschliff |

Werden Prefab-Instanzen per Skript verändert, müssen die Änderungen als Overrides gespeichert werden (`PrefabUtility.RecordPrefabInstancePropertyModifications`). Der Generator erledigt das am Ende für die ganze Szene. Neue Objektnamen, die das Workshop-Fenster oder Tests nutzen sollen, gehören in `SandboxValidator.WorkshopObjects`.

### 16.4 Beispiele für die Demo

Diese fünf Beispiel-Aufgaben stammen aus der ursprünglichen Aufgabenstellung. Die KI-Demo ist nur vorbereitet: Keine der Aufgaben ist schon eingebaut.

1. **„Erstelle einen Schalter, der alle Bäume blau einfärbt.“**
   Skript (zum Beispiel `TreePainter`) mit einem Material-Feld (`SkyBlue` aus `Materials/Palette`) und der Methode `PaintTreesBlue()`. Sie sucht alle Objekte, deren Name mit `Tree_` beginnt, und setzt bei allen Renderern außer `Trunk` das `sharedMaterial`. Einen `PushButton` aus `Prefabs/Interactions` aufstellen (er hat schon einen `ClickableSwitch`) und `PaintTreesBlue` unter „On Pressed“ eintragen.
2. **„Füge einen Würfel hinzu, der beim Anklicken springt.“**
   Skript, das `IClickable` umsetzt und in `OnClick()` `Rigidbody.AddForce(Vector3.up * 5f, ForceMode.VelocityChange)` aufruft. Auf einen `ColoredCube` mit Rigidbody legen, dazu `ResettableObject` für Taste R. `ClickInteractor` findet das Skript über den Collider am Kind `Cube`.
3. **„Erweitere die vorhandene Tür um ein rotes und grünes Statuslicht.“**
   Neben `AutomaticDoor` zwei kleine Kugeln mit Point Light und `LampController` setzen (On/Off Material `Traffic_RedOn`/`Traffic_RedOff` bzw. `Traffic_GreenOn`/`Traffic_GreenOff`, „Follow Day Night“ aus). Am `DoorController` unter „On Opened“ Grün `TurnOn` und Rot `TurnOff` eintragen, unter „On Closed“ umgekehrt. Das geht sogar ohne neuen Code. Ein Skript braucht es erst, wenn die Lichter schon während der Bewegung wechseln sollen.
4. **„Erstelle einen neuen Gegenstand, der zwischen zwei Positionen schwebt.“**
   Vorlage ist `MovingPlatform`: zwei leere Objekte als Punkte (Geschwister, keine Kinder des Gegenstands) und `Mathf.SmoothStep` zwischen ihnen. Als Gegenstand passt ein Low-Poly-Mesh wie `LP_Crystal` oder ein `ColoredSphere`, ein `Rotator` lässt ihn zusätzlich drehen. Ohne neuen Code: `MovingPlatform` direkt auf das Objekt legen und Wait Time auf 0 stellen (ohne Rigidbody bewegt es sich in `Update`).
5. **„Lass bei Erreichen der Zielzone ein kleines Feuerwerk aus Partikeln erscheinen.“**
   Neues Skript mit einem `ParticleSystem` und einer Methode `Celebrate()`. In `GoalZone` unter „On Goal Reached“ eintragen. Unter URP braucht das Partikel-Material den Shader „Universal Render Pipeline/Particles/Unlit“. Taste R setzt das Ziel zurück, das Feuerwerk kann also mehrmals starten.

Weitere nützliche Andockpunkte: „On Phase Changed“ der Ampel, `Rotator` und `MovingPlatform` als Vorlagen für eigenes Verhalten, die `SB`-Helfer für dauerhafte Erweiterungen im Generator.

---

## 17. Bekannte Einschränkungen

- **Schatten in der Ferne:** Das URP-Asset des Projekts (`Assets/Settings/PC_RPAsset`) hat eine Schattendistanz von 50 m. Sie wurde absichtlich nicht geändert, weil sie global gilt. Von der Übersichts-Kamera aus haben weit entfernte Objekte deshalb keine Schatten. In den Bereichen ist alles verschattet.
- **SSAO** ist im Renderer des Projekts aktiv und wurde nicht verändert.
- **Farbige Lichter** im Farben-Labor und das `ShadowSpotLight` wirken am besten bei Sonnenuntergang oder Nacht. Am Tag überstrahlt die Sonne sie.
- **Schrift** auf Schildern ist `TextMesh` (alte 3D-Schrift, kein zusätzliches Paket). Sie ist unbeleuchtet und nachts genauso hell wie am Tag.
- **Tageszeit-Knöpfe überschreiben die Sonne:** Eigene Änderungen an der Sonne überstehen das Drücken von Play. Jeder Tageszeit-Wechsel (Knöpfe oder Taste T) setzt sie aber auf die Werte des Presets zurück. Im Edit Mode holt Strg+Z den vorherigen Zustand zurück.
- **Eingabe:** Das Projekt nutzt nur das Input System. Der Zweig für den alten Input Manager wird in diesem Projekt nicht kompiliert und ist daher nicht getestet.
- **Fokus:** Klicks und Tasten im Play Mode kommen nur an, wenn das Game-Fenster den Fokus hat (einmal hineinklicken).
- **Ungetestet auf dem Bildschirm:** Workshop-Fenster (IMGUI) und eigene Inspectoren wurden nur kompiliert, nicht angesehen.
- **Tür mit „Rotate“:** `AutomaticDoor` ist ein einzelner Würfel mit Pivot in der Mitte. Stellt man „Motion“ auf Rotate, dreht sich die Tür um ihre Mitte, nicht um eine Kante. Für eine echte Drehtür bräuchte es ein leeres Scharnier-Objekt als „Door“.
- **Build Settings:** In den Build Settings steht nur `Assets/Scenes/SampleScene.unity`. Für einen eigenständigen Build müsste die Spielwiese dort eingetragen werden. Für den Play Mode im Editor ist das nicht nötig.
- **Sicherungen** enthalten nur die Szene, keine geänderten Prefab- oder Material-Assets. „Restore Original Scene“ setzt diese Assets zurück, eigene Änderungen daran sind danach nur noch über Git zu retten (siehe Abschnitt 3.4).
- **Kamera-Start an der Scene View** gibt es nur im Editor. Ist keine Scene View offen oder steht sie im 2D-Modus, startet die Kamera an ihrer gespeicherten Position.
