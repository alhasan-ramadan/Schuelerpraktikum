# Mini World – Unity Playground

Diese Anleitung ist für dich als Betreuer. Die Spielwiese ist eine fertige 3D-Insel mit einem zentralen Platz und vier Lernbereichen: Bauen, Physik, Licht und Interaktionen. Alles liegt unter `Assets/StudentSandbox/`.

| | |
|---|---|
| Unity-Version | 6000.3.23f1, Universal Render Pipeline (Forward+) |
| Eingabe | Input System Package |
| Arbeitsszene | `Assets/StudentSandbox/Scenes/StudentSandbox.unity` |
| Startpunkt im Editor | Menü **Tools > Student Sandbox > Workshop** |

## Schnellstart

1. Projekt im Unity Hub mit Unity 6000.3.23f1 öffnen.
2. Menü **Tools > Student Sandbox > Workshop** öffnen.
3. Im Workshop-Fenster **Spielwiese öffnen** klicken (der Knopf erscheint nur, wenn die Szene noch nicht offen ist).
4. **▶ Play starten** klicken, einmal ins Game-Fenster klicken und mit den Tasten 1 bis 5 durch die Bereiche fliegen.
5. Play beenden, Lektion **1. Objekte bewegen** wählen und die Maus an deine Schwester abgeben.

## Vorbereitung am Vorabend

- [ ] Projekt öffnen und warten, bis Unity fertig importiert und kompiliert hat. In der Console stehen keine roten Fehler.
- [ ] **Tools > Student Sandbox > Workshop** öffnen und einmal ganz durchscrollen. Das Fenster wurde bisher nur kompiliert und noch nie auf einem Bildschirm angesehen (alle Tests liefen im Batch Mode ohne Oberfläche). So fällt dir vorher auf, falls etwas komisch aussieht.
- [ ] **Spielwiese öffnen**. Die Scene View springt automatisch auf die Weltübersicht.
- [ ] **Tools > Student Sandbox > Validate Sandbox** ausführen. Erwartet: keine Fehler.
- [ ] Einmal Play drücken, ins Game-Fenster klicken und prüfen:
  - Das Game-Fenster zeigt dieselbe Stelle wie die Scene View (direkt nach dem Öffnen die Weltübersicht).
  - Die rote Kugel rollt die Rampe hinunter, nach etwa 4 Sekunden erscheint oben ein grüner Kasten „Ziel erreicht! Die rote Kugel ist angekommen.“ Danach räumt die Kugel die drei Kegel ab.
  - Etwa 1,5 Sekunden nach dem Start schwingt der Hammer los und stößt die Dominosteine an.
  - R drücken: Alles steht wieder am Start, der Hammer wartet. Ein Klick auf den Knopf **DOMINO** im Physiklabor (Taste 3) lässt ihn wieder schwingen.
  - Die Tasten 1–5, T und H reagieren.
  - Im Interaktionsbereich (Taste 5) öffnet ein Klick auf den Knopf **TÜR** die Tür.
- [ ] Play beenden. Im Workshop-Fenster unter **Tageszeit** auf **Tag** stellen und mit Strg+S speichern. So, wie die Szene gespeichert ist, sieht sie auch beim Drücken von Play aus.
- [ ] Hast du beim Testen Schritte abgehakt? Dann pro Lektion **Häkchen zurücksetzen** klicken. Die Häkchen speichert Unity auf diesem PC (EditorPrefs), nicht in der Szene. „Ausgangsszene wiederherstellen“ löscht sie deshalb nicht.
- [ ] Optional: Im Workshop-Fenster **Sicherung anlegen** und den Stand in Git committen.
- [ ] Optional: Fenster anordnen (Scene und Game nebeneinander, Inspector rechts, siehe [WORKSHOP_DE.md](WORKSHOP_DE.md)). Unter **Edit > Preferences > Colors > Playmode tint** eine kräftige Farbe wählen, dann sieht man sofort, wenn Play läuft.

---

## 1. Wie öffne ich die Spielwiese?

1. Unity Hub öffnen und das Projekt (den Repository-Ordner) mit Unity 6000.3.23f1 starten.
2. Menü **Tools > Student Sandbox > Workshop** wählen. Es öffnet sich das Fenster „Student Sandbox – Workshop“. Am besten dockst du es neben dem Inspector an.
3. Steht oben „Die Spielwiese ist gerade nicht geöffnet.“, klickst du auf **Spielwiese öffnen**.

Andere Wege zur Szene:

- Menü **Tools > Student Sandbox > Open Sandbox Scene**
- Im Project-Fenster `Assets/StudentSandbox/Scenes/StudentSandbox.unity` doppelklicken

Hat die gerade offene Szene ungespeicherte Änderungen, fragt Unity vorher nach. Nach dem Öffnen richtet sich die Scene View von selbst auf die Weltübersicht aus.

Das Workshop-Fenster hat von oben nach unten fünf Abschnitte:

| Abschnitt | Inhalt |
|---|---|
| Willkommen | Status (Edit Mode oder Play Mode), Knopf **▶ Play starten** / **■ Play beenden**. Ist ein Prefab im Prefab-Modus offen, steht hier „Du bearbeitest gerade ein Prefab“ mit dem Knopf **Zurück zur Spielwiese**. |
| Lektionen | sechs Lektionen, jede mit Fortschritt („3 von 7 erledigt“) |
| Bereich auswählen | **Zum Baubereich**, **Zum Physiklabor**, **Zum Lichtlabor**, **Zum Interaktionsbereich**, **Zur Weltübersicht** (im Prefab-Modus grau) |
| Aufgaben der Lektion | **Zum Bereich**, Knöpfe **Zeig mir: …** (wählen das Objekt aus und zoomen hin; **Zeig mir: Sonne** wählt die Sonne nur aus, die Scene View bleibt stehen), Erklärung, Lernziel, Schritte zum Abhaken, Herausforderung, Tipp für Erwachsene |
| Werkzeuge | **Ausgewähltes Objekt zeigen** (wie Taste F; bei einem Prefab im Project-Fenster erklärt es, dass man es erst in die Scene View ziehen muss), Tageszeit (**Tag** / **Sonnenuntergang** / **Nacht**), **Physik zurücksetzen**, **Hilfe**, **Sicherung anlegen**, **Ausgangsszene wiederherstellen**, **Spielwiese neu erzeugen**, **Anleitung öffnen** (öffnet [WORKSHOP_DE.md](WORKSHOP_DE.md)) |

## 2. Wie generiere ich sie, falls sie noch nicht existiert?

Normalerweise musst du nichts erzeugen: Die Szene ist fertig und liegt im Projekt. Falls sie fehlt:

1. Play Mode beenden.
2. **Tools > Student Sandbox > Generate Sandbox** wählen. Alternativ im Workshop-Fenster **Spielwiese erzeugen** klicken (dieser Knopf erscheint nur, wenn es keine Szene gibt).
3. Ein Fortschrittsbalken zeigt die Schritte (Materialien, Meshes, Prefabs, Bereiche, Kameras). Am Ende meldet ein Dialog „Fertig!“, die Szene ist gespeichert und offen.

Was dabei passiert:

- Geschrieben wird nur unter `Assets/StudentSandbox/`.
- Materialien, Low-Poly-Meshes und Prefabs werden angelegt oder auf den Ausgangszustand zurückgesetzt. Materialien werden dabei komplett zurückgesetzt, nicht nur die Farben.
- Die Szene wird als `Scenes/StudentSandbox.unity` gespeichert, dazu eine unberührte Kopie als `Scenes/Original/StudentSandbox_Original.unity`.
- Die Welt sieht nach jedem Erzeugen gleich aus (fester Zufalls-Startwert).
- Gibt es die Szene schon, fragt Unity: **Sicherung anlegen und neu erzeugen**, **Abbrechen** oder **Nur Szene öffnen**. Beim Neu-Erzeugen wird die alte Szene vorher nach `Assets/StudentSandbox/Backups/` kopiert.

**Kommandozeile** (vorher den Unity-Editor schließen, ein Projekt kann nicht zweimal gleichzeitig offen sein):

```
Unity.exe -batchmode -projectPath <Projektpfad> -executeMethod StudentSandbox.EditorTools.SandboxGenerator.GenerateBatch -quit
```

Exit-Code 0 bedeutet Erfolg, 1 bedeutet Fehler (Details im Log). Der Batch-Lauf legt keine Szenen-Sicherung an, hier ist Git die Sicherung. Willst du trotzdem eine Kopie in `Backups/`, hängst du `-sandboxBackup` an den Befehl an. Über das Menü oder das Workshop-Fenster wird dagegen immer gefragt und vorher gesichert.

Prüfen und testen:

```
Unity.exe -batchmode -projectPath <Projektpfad> -executeMethod StudentSandbox.EditorTools.SandboxValidator.ValidateBatch -sandboxReport report.txt
Unity.exe -batchmode -projectPath <Projektpfad> -runTests -testPlatform EditMode -testResults editmode.xml
Unity.exe -batchmode -projectPath <Projektpfad> -runTests -testPlatform PlayMode -testResults playmode.xml
```

Im Editor laufen die Tests über den Test Runner (**Window > General > Test Runner**). Letzter Stand: Validator ohne Fehler und ohne Warnungen, EditMode 8/8 und PlayMode 15/15 bestanden. Dazu kommt der Test `RestoreOriginalSceneResetsSceneAndAssets` (Klasse `SandboxRestoreTests`). Er schreibt Dateien und läuft deshalb nur, wenn man ihn gezielt startet, zum Beispiel mit der Unity-Kommandozeile: `unity test . --mode EditMode --filter RestoreOriginalSceneResetsSceneAndAssets`. Auch er hat bestanden. Mehr dazu in [TECHNICAL_OVERVIEW_DE.md](TECHNICAL_OVERVIEW_DE.md).

## 3. Welche Szene muss ich öffnen?

Nur diese: **`Assets/StudentSandbox/Scenes/StudentSandbox.unity`**

| Datei | Wofür | Öffnen? |
|---|---|---|
| `Scenes/StudentSandbox.unity` | die Arbeitsszene für den Workshop | ja |
| `Scenes/Original/StudentSandbox_Original.unity` | unberührte Vorlage für „Ausgangsszene wiederherstellen“ | nein. Öffnet man sie doch, bietet ein Dialog an, die richtige Spielwiese zu öffnen. |
| `Backups/StudentSandbox_Backup_<Datum>_<Uhrzeit>.unity` | automatische und manuelle Sicherungen | nur zum Nachschauen |
| `Assets/Scenes/SampleScene.unity` | Beispielszene der URP-Projektvorlage | nein |

Play startet immer die gerade offene Szene. Die Knöpfe **Zum Bereich** und **Zeig mir** im Workshop-Fenster funktionieren nur, wenn `StudentSandbox.unity` die aktive Szene ist.

## 4. Welche Eingaben gibt es im Play Mode?

Zuerst einmal ins Game-Fenster klicken, sonst kommen Tasten und Klicks nicht an. Oben links zeigt das Spiel eine Steuerungshilfe (Taste H blendet sie aus und ein).

Die Kamera startet dort, wohin die Scene View gerade schaut. Also erst mit dem Workshop-Fenster (z. B. **Zum Physiklabor**) oder durch Navigieren in der Scene View einen Bereich einstellen, dann Play drücken: Das Game-Fenster zeigt dieselbe Stelle. Das gilt nur im Editor (Haken **Start At Scene View** an der Free Fly Camera, standardmäßig an).

| Eingabe | Wirkung |
|---|---|
| W A S D | fliegen (vor, links, zurück, rechts) |
| Q / E | runter / hoch |
| Shift halten | dreimal so schnell |
| rechte Maustaste halten + Maus bewegen | umschauen (der Mauszeiger verschwindet so lange) |
| Mausrad | Fluggeschwindigkeit ändern (1 bis 40 m/s, Start: 10) |
| Linksklick | Knopf drücken; über einem Knopf erscheint „Klick: …“ |
| 1 bis 5 (auch Ziffernblock) | zum Aussichtspunkt fliegen: 1 Übersicht, 2 Baubereich, 3 Physiklabor, 4 Lichtlabor, 5 Interaktionsbereich |
| R | Physik zurücksetzen (Kugeln, Kegel, Dominosteine, Hammer, Ziele); der Hammer wartet danach auf den Knopf DOMINO |
| T | Tageszeit weiterschalten: Tag → Sonnenuntergang → Nacht → Tag |
| H | Steuerungshilfe ein / aus |
| Esc | Mauszeiger freigeben |

Die Kamera fliegt nie tiefer als y = 0,5 m, sie kann also nicht im Boden verschwinden.

**Anklickbare Knöpfe in der Welt:**

| Bereich | Objekt in der Hierarchy | Aufschrift | Was passiert |
|---|---|---|---|
| Physiklabor | `ResetButton` | RESET | alles zurücksetzen (wie Taste R) |
| Physiklabor | `DominoButton` | DOMINO | der Hammer schwingt los (von selbst schwingt er nur einmal, 1,5 Sekunden nach Play) |
| Lichtlabor | `DayButton`, `SunsetButton`, `NightButton` | TAG, ABEND, NACHT | Tageszeit umschalten |
| Interaktionsbereich | `DoorSwitch` | TÜR | Tür auf / zu |
| Interaktionsbereich | `PedestrianButton` (gelber Kasten am Ampelmast) | DRÜCKEN | Ampel eine Phase weiter |
| Interaktionsbereich | `Desk_DoorButton` | TÜR | Tür auf / zu |
| Interaktionsbereich | `Desk_TrafficButton` | AMPEL | Ampel eine Phase weiter |
| Interaktionsbereich | `Desk_TrafficAutoButton` | AUTO | Ampel-Automatik an / aus |
| Interaktionsbereich | `Desk_CrystalButton` | KRISTALL | Kristall dreht sich / stoppt |
| Interaktionsbereich | `Desk_PlatformButton` | PLATTFORM | Plattform fährt / stoppt |

Achtung beim Wechsel: Im Edit Mode (Scene View) bedeuten die Tasten etwas anderes. Dort fliegt man mit rechter Maustaste + W A S D, und ohne rechte Maustaste sind W, E, R die Werkzeuge Verschieben, Drehen, Skalieren. Der Knopf **Hilfe** im Workshop-Fenster zeigt diese Tasten.

## 5. Wo befinden sich die vier Bereiche?

Draufsicht, Norden ist +z:

```
                 04 Physiklabor
                   (0, 0, 25)
                       |
 03 Baubereich ---- 02 Platz ---- 05 Lichtlabor
  (-25, 0, 0)      (0, 0, 0)       (25, 0, 0)
                       |
              06 Interaktionsbereich
                   (0, 0, -25)
```

Jeder Bereich ist 24 × 24 m groß. Sein Eingang mit Torbogen und Namen zeigt immer zum Platz.

| Bereich | Gruppe in der Hierarchy | Mitte (Welt) | Farbe | Was gibt es dort? | Taste |
|---|---|---|---|---|---|
| Zentraler Platz | `02_CENTRAL_HUB` | (0, 0, 0) | hell / neutral | Springbrunnen mit drehendem Kristall (`Ornament`), 4 Bänke, 4 Laternen, Hecke, Schild „MINI WORLD“ (`WelcomeSign`), Wegweiser (`Signpost_Hub`) | 1 (Übersicht der ganzen Insel) |
| Baubereich (CREATIVE) | `03_CREATIVE_ZONE` | (-25, 0, 0), Westen | Mintgrün | Park mit Teich; Häuser, Bäume, Bänke und Deko in `EditableObjects`; Bauplatz `BuildArea` mit Bausteinen (`BuildingBlocks`) und `ExampleTower`; Farbkasten `ColorPalette` | 2 |
| Physiklabor (PHYSICS) | `04_PHYSICS_ZONE` | (0, 0, 25), Norden | Orange | Murmelbahn (`StartTower`, `Ramp_Main`, rote Kugel `PhysicsBall`, Ziel `GoalZone`; hinter dem Ziel-Auslöser stehen im Zielbereich die Kegel `Pin_1` bis `Pin_3`); Domino (`Hammer`, `Domino_01` bis `Domino_10`); Hindernis-Parcours `ObstacleCourse` (blaue Kugel `PhysicsBall_Blue`, `TargetZone`, Mauern, Bumper, Flummi `BouncyBall`); Knöpfe RESET und DOMINO | 3 |
| Lichtlabor (LIGHT) | `05_LIGHT_ZONE` | (25, 0, 0), Osten | Lavendel | Dorfplatz mit 6 Laternen (`StreetLamp_1` bis `_6`), 2 Häusern (`Village_House_Lavender`, `Village_House_Blue`) und Sonnenuhr (`Sundial`); Farben-Labor `ColorLab` mit `ColorLight_Red`, `_Green`, `_Blue` auf einer weißen Bühne; Schatten-Labor `ShadowLab` mit `ShadowSpotLight` und `ShadowSculpture`; Tageszeit-Pult mit TAG, ABEND, NACHT | 4 |
| Interaktionsbereich (INTERACTIONS) | `06_INTERACTION_ZONE` | (0, 0, -25), Süden | Türkis | Schiebetür `AutomaticDoor` mit Schalter `DoorSwitch` (dahinter der `TreasureRoom`); Ampel `TrafficLight`; drehender Kristall (`RotatingCrystal`, Kind `Crystal`); fahrende Plattform (`MovingPlatformSet` mit `MovingPlatform`, `PointA`, `PointB`); Kontrollpult `ControlDesk` mit 5 Knöpfen | 5 |

Rund um die Bereiche liegt `09_DECORATION`: Windmühle (Nordosten), Leuchtturm (Südwesten), Steg mit Boot (Südosten), Picknickplatz (Nordwesten), dazu Bäume, Steine, Blumen und Wolken.

## 6. Welche Aufgaben soll die Schülerin erledigen?

Das Workshop-Fenster führt sie durch sechs Lektionen. Jede hat Schritte zum Abhaken und Knöpfe, die das passende Objekt zeigen.

| Lektion | Bereich | Darum geht es |
|---|---|---|
| 1. Objekte bewegen | Baubereich | Scene View, Hierarchy, Inspector; Baum, Haus und Bank verschieben, drehen, skalieren |
| 2. Eigene Welt gestalten | Baubereich | Prefabs in die Welt ziehen, kopieren (Strg+D), Materialien tauschen; Aufgaben A1 (Park gestalten) und A2 (eigener Turm auf dem Bauplatz) |
| 3. Physik ausprobieren | Physiklabor | Rigidbody und Collider; Rampenwinkel ändern, blaue Kugel ins Ziel lenken, Dominos |
| 4. Licht und Schatten | Lichtlabor | Tag und Nacht, Lichtfarbe, Spot Light drehen, Schatten beobachten |
| 5. Interaktionen testen | Interaktionsbereich | Schalter, Events, Tür langsamer machen, Ampel, Kristall doppelt so schnell, Plattform-Strecke |
| 6. Eigene Herausforderung | ganze Insel | Abenteuerpark: mindestens 3 Objekte platzieren, eine Farbe ändern, eine Physik-Reaktion, ein Licht anpassen, eine Interaktion auslösen; danach speichern, sichern und vorstellen |

- Den 90-Minuten-Ablauf mit Zeiten, Erklärsätzen für dich und Schritt-für-Schritt-Aufgaben findest du in [WORKSHOP_DE.md](WORKSHOP_DE.md).
- Zusätzliche Aufgaben, sortiert von sehr einfach bis Bonus, stehen in [CHALLENGES_DE.md](CHALLENGES_DE.md).
- Optional zum Schluss: die KI-Demo (Bonus in [WORKSHOP_DE.md](WORKSHOP_DE.md)). Sie ist nicht eingebaut, nur vorbereitet: Deine Schwester beschreibt eine Idee in einfachen Worten, ein Coding-Agent schreibt das Skript. Die fünf Beispiel-Aufträge sind „Erstelle einen Schalter, der alle Bäume blau einfärbt.“, „Füge einen Würfel hinzu, der beim Anklicken springt.“, „Erweitere die vorhandene Tür um ein rotes und grünes Statuslicht.“, „Erstelle einen neuen Gegenstand, der zwischen zwei Positionen schwebt.“ und „Lass bei Erreichen der Zielzone ein kleines Feuerwerk aus Partikeln erscheinen.“ Wo sie im Code andocken, steht in [TECHNICAL_OVERVIEW_DE.md](TECHNICAL_OVERVIEW_DE.md).

Deine Rolle: kurz erklären (ein bis drei Sätze), dann übernimmt sie die Maus. Unter jeder Lektion steht im Workshop-Fenster ein „Tipp für Erwachsene“.

## 7. Welche GameObjects kann sie gefahrlos verändern?

Grundregel: Alles, was ein Experiment oder Deko ist, darf sie verschieben, drehen, skalieren, kopieren, umfärben und in den Komponenten verstellen. Änderungen im Edit Mode speichert Strg+S. Nichts davon ist endgültig, siehe [Frage 8](#8-wie-kann-ich-die-ursprüngliche-szene-wiederherstellen).

| Bereich | Objekte | Was sie ändern kann |
|---|---|---|
| Baubereich | alles unter `03_CREATIVE_ZONE/EditableObjects`: `House_Red`, `House_Blue`, `Tree_Small_1` bis `_3`, `Tree_Large_1`, `Tree_Pine_1` und `_2`, `Bench_Pond_1` und `_2`, `LampPost_1` und `_2`, `Fence_1` bis `_3`, `Flower_1` bis `_4`, `Rock_1` und `_2`, `FlowerBed_1`, `DecorativePillar_1`, `Bush_1` bis `_3` | alles, auch löschen (Entf) |
| Baubereich | `BuildArea` mit `BuildingBlocks` (`Block_...`) und `ExampleTower`; neue Prefabs aus `Assets/StudentSandbox/Prefabs/Creative` | stapeln, bauen, neue Objekte dazustellen |
| Physiklabor | `Ramp_Main` | Rotation Z (getestet von -12 bis -30, Start: -22; der Inspector zeigt -22, nicht 338) |
| Physiklabor | `PhysicsBall`, `Pin_1` bis `Pin_3`, `Domino_01` bis `Domino_10`, `Hammer` | Rigidbody (Mass, Use Gravity), Abstände der Dominos, Domino Starter (Swing Speed, Start Delay) |
| Physiklabor | alles in `ObstacleCourse`: `WallBlock_1`, `WallBlock_2`, `Bumper_1`, `SmallRamp_1`, `ObstacleCube_1` bis `_3`, `TargetZone`, `BouncyBall`, `PhysicsBall_Blue` | verschieben und drehen, bis die blaue Kugel ins Ziel rollt |
| Lichtlabor | `ColorLight_Red`, `ColorLight_Green`, `ColorLight_Blue`, `ShadowSpotLight`, `ShadowSculpture`, `StreetLamp_1` bis `_6` (Kind `LampLight`), `Village_House_Lavender`, `Village_House_Blue` | Position, Rotation, Color, Intensity, Range, Inner / Outer Spot Angle, Material; bei Laternen und Häusern **Is On** im Lamp Controller |
| Lichtlabor | `00_WORLD_ROOT/Sun` | Rotation, Color, Intensity. Das bleibt beim Drücken von Play erhalten. Erst ein Tageszeit-Knopf, Taste T oder eine Änderung im Inspector des `DayNightController` setzt die Sonne auf die Werte der Tageszeit zurück. |
| Interaktionsbereich | `AutomaticDoor`, `DoorSwitch`, `Desk_...Button`, `TrafficLight`, `Crystal`, `MovingPlatform`, `PointA`, `PointB` | Open Speed, Slide Offset, On Pressed, Phases, Current Phase, Auto Cycle, Speed, Rotation Axis, Wait Time; Punkte verschieben. Bei der Ampel wechseln die Lampen sofort mit, wenn sie Current Phase oder die Haken Red, Yellow, Green einer Phase ändert (auch im Edit Mode). |
| Platz und Deko | `02_CENTRAL_HUB` (Bänke, Laternen, Blumenbeete, Text von `WelcomeSign/Title`, Wegweiser `Signpost_Hub`: pro Schild `Text_Front` und `Text_Front_Shadow` ändern), `09_DECORATION` (Bäume, Steine, Wolken, Windmühlen-Flügel `Blades` mit Rotator) | alles |

Komponenten ausschalten ist auch erlaubt: Entfernt sie den Haken vor einer Komponente, ist sie wirklich aus. Ein **Bumper** stößt dann nicht mehr, eine **Goal Zone** meldet kein Ziel, ein **Clickable Switch** reagiert nicht auf Klicks und zeigt kein „Klick: …“. Haken wieder setzen, und alles geht wie vorher.

Manche Teile wählt man in der Scene View leicht aus Versehen an, zum Beispiel die Glas-Kugel `Lamp` unter `ColorLight_Red`, `_Green` und `_Blue` oder die Bodenmarkierung `Marker` unter `PointA` und `PointB`. Deren Notiz im Inspector sagt ihr, dass sie das Eltern-Objekt darüber auswählen soll.

**Lieber nicht anfassen.** Kaputt geht dabei nichts, was sich nicht wiederherstellen lässt, aber es stört den Ablauf:

| Was | Warum | Falls es doch passiert |
|---|---|---|
| `08_MANAGERS`: `DayNightController`, `PhysicsResetter`, `SandboxHUD` (Objekte nicht löschen, Komponenten nicht entfernen; die Tageszeit nur über die Knöpfe stellen) | steuern Tageszeit, Reset mit R und die Bildschirmanzeige | Strg+Z oder Ausgangsszene wiederherstellen |
| `07_CAMERAS/Main Camera` mit Camera, Free Fly Camera und Click Interactor | ohne sie keine Steuerung und keine Klicks im Play Mode | wie oben |
| `07_CAMERAS/Viewpoints/Viewpoint_1_Overview` bis `Viewpoint_5_Interaction` | Tasten 1–5 und alle „Zum Bereich“-Knöpfe | wie oben |
| Hauptgruppen `00_WORLD_ROOT` bis `09_DECORATION` umbenennen; `03_` bis `06_` verschieben, drehen oder skalieren | die Bereiche hängen an diesen Gruppen, der Validator meldet es | Strg+Z oder Werte zurücksetzen |
| Objekte umbenennen, die das Workshop-Fenster sucht (z. B. `Tree_Small_1`, `House_Red`, `PhysicsBall`, `Ramp_Main`, `GoalZone`, `Sun`, `ColorLight_Blue`, `AutomaticDoor`, `DoorSwitch`, `Crystal`, `MovingPlatform`, `PointB`) | **Zeig mir** findet sie sonst nicht | mit F2 zurückbenennen |
| `00_WORLD_ROOT/Global Volume (Nachbearbeitung)` | Nachbearbeitung (Bloom, Vignette, Farben) | Strg+Z |
| Prefab-Dateien in `Prefabs/` oder Materialien in `Materials/` direkt bearbeiten; bei einer Prefab-Kopie im Inspector **Overrides > Apply All** | ändert alle Kopien in der ganzen Welt auf einmal | **Ausgangsszene wiederherstellen** oder **Spielwiese neu erzeugen** setzt Prefabs und Materialien zurück |
| `Scenes/Original/StudentSandbox_Original.unity` | Vorlage für die Wiederherstellung; beim Öffnen fragt ein Dialog, ob du lieber die richtige Spielwiese öffnen willst | Ungespeicherte Änderungen daran verwirft **Ausgangsszene wiederherstellen**. Wurde sie gespeichert: **Spielwiese neu erzeugen** legt sie neu an. |

## 8. Wie kann ich die ursprüngliche Szene wiederherstellen?

Vom kleinsten zum größten Schritt:

**a) Rückgängig machen:** Strg+Z (Strg+Y wiederholt). Gut für die letzten paar Schritte. Das klappt auch nach einem Klick auf **Tag**, **Sonnenuntergang** oder **Nacht**: Strg+Z holt Sonne, Lampen, Himmel, Umgebungslicht und Nebel zurück.

**b) Ausgangsszene wiederherstellen** (setzt die Szene und alle erzeugten Assets zurück):

1. Play beenden.
2. Im Workshop-Fenster unter Werkzeuge **Ausgangsszene wiederherstellen** klicken, oder Menü **Tools > Student Sandbox > Restore Original Scene**.
3. Dialog „Ausgangsszene wiederherstellen?“ mit **Ja, wiederherstellen** bestätigen.
4. Unity fragt nach ungespeicherten Änderungen. Wählst du Speichern, landen sie in der Sicherung. Ohne Speichern sind sie weg, die Sicherung enthält dann den zuletzt gespeicherten Stand.
5. Die aktuelle Szene wird als `Backups/StudentSandbox_Backup_<Datum>_<Uhrzeit>.unity` gesichert. Danach setzt Unity alle Materialien (auch Palette-Farben, Transparenz und hineingezogene Texturen), Meshes, Prefabs (z. B. nach **Overrides > Apply All**) und das Nachbearbeitungs-Profil zurück, ersetzt die Arbeitsszene durch die Original-Kopie und öffnet sie. Ein Dialog nennt den Pfad der Sicherung.

Gesichert wird nur die Szene, nicht die alten Materialien und Prefabs. Eigene neue Dateien, zum Beispiel ein selbst angelegtes Material, bleiben liegen. Ist gerade die Original-Kopie selbst offen und verändert, werden diese Änderungen verworfen und nie in die Vorlage gespeichert.

**c) Spielwiese neu erzeugen** (baut alles aus dem Code neu, auch die Original-Kopie): Im Workshop-Fenster **Spielwiese neu erzeugen** klicken, oder **Tools > Student Sandbox > Generate Sandbox**. Dann **Sicherung anlegen und neu erzeugen** wählen. Die alte Szene landet vorher in `Backups/`. Das brauchst du nur, wenn b) nicht hilft, zum Beispiel weil jemand die Original-Kopie verändert und gespeichert hat.

**d) Eine Sicherung zurückholen:** Im Project-Fenster `Assets/StudentSandbox/Backups/` öffnen und die gewünschte Sicherung doppelklicken. Zum Nachschauen reicht das. Soll sie wieder die Arbeitsszene werden: **File > Save As…** und `Assets/StudentSandbox/Scenes/StudentSandbox.unity` überschreiben. Vorher am besten **Sicherung anlegen** klicken, damit auch der aktuelle Stand gesichert ist.

**e) Git** (wenn die Spielwiese eingecheckt ist): Play beenden, eine andere Szene öffnen oder Unity schließen, dann:

```
git restore Assets/StudentSandbox/Scenes/StudentSandbox.unity
```

Mit `git restore Assets/StudentSandbox` holst du auch Materialien und Prefabs zurück. Neue Dateien (zum Beispiel Sicherungen) bleiben dabei liegen.

## 9. Wie erkläre ich die wichtigsten Komponenten?

Jedes GameObject besteht aus Komponenten, die im Inspector untereinander stehen. Viele Objekte haben ganz oben eine farbige Notiz-Karte (Komponente **Info Note**). Dort steht in einfachen Worten, was das Objekt ist und was man ausprobieren kann. Lass sie diese Notizen ruhig vorlesen.

### Transform

- **Sag:** „Jedes Objekt hat eine Position (wo steht es?), eine Rotation (wie ist es gedreht?) und eine Scale (wie groß ist es?).“
- **Zeig:** `Tree_Small_1` im Baubereich (Workshop: **Zeig mir: Baum**).
- **Probier:** Scale X, Y und Z auf 2 setzen, der Baum ist doppelt so groß.

### Mesh Renderer und Material

- **Sag:** „Der Mesh Filter ist die Form, der Mesh Renderer malt sie auf den Bildschirm, und das Material bestimmt Farbe und Oberfläche.“
- **Zeig:** `House_Red` aufklappen, Kind `Roof` wählen. Im Mesh Renderer steht unter Materials das Dach-Material.
- **Probier:** Ein Material aus `Assets/StudentSandbox/Materials/Palette` auf das Dach in der Scene View ziehen. Die elf beschrifteten Farbkugeln in `ColorPalette` zeigen, wie die meisten der 16 Palette-Materialien aussehen.

### Collider

- **Sag:** „Ein Collider ist eine unsichtbare Hülle. Ohne sie würden Dinge durcheinander hindurchfallen.“ Und: „Mit Haken bei Is Trigger wird der Collider zu einem unsichtbaren Bereich, der merkt, wenn etwas hineinrollt.“
- **Zeig:** `PhysicsBall` (Sphere Collider) und `GoalZone` (Box Collider mit **Is Trigger**).
- **Probier:** Play drücken: Die Kugel rollt in den Ziel-Bereich hinein, ohne abzuprallen. Die Bodenplatte des Ziels wechselt die Farbe, ein grünes Licht geht an und es erscheint „Ziel erreicht! …“. Erst danach rollt sie in die Kegel dahinter.

### Rigidbody

- **Sag:** „Ein Rigidbody macht ein Objekt echt: Es hat ein Gewicht und die Schwerkraft zieht es nach unten.“
- **Zeig:** `PhysicsBall` (Mass 1, Use Gravity), einen Kegel `Pin_1`, den `Hammer` (Is Kinematic: Ein Skript bewegt ihn, er schubst aber trotzdem andere Objekte).
- **Probier:** Bei der roten Kugel den Haken **Use Gravity** entfernen und Play drücken. Oder einem `ColoredCube` über **Add Component > Rigidbody** einen Rigidbody geben, ihn in die Luft stellen und Play drücken.

### Light

- **Sag:** „Directional Light ist wie die Sonne, Point Light wie eine Glühbirne, Spot Light wie eine Taschenlampe.“
- **Zeig:** `Sun` (Directional), `ColorLight_Red` (Point), `ShadowSpotLight` (Spot). Wichtige Felder: Color, Intensity, Range, beim Spot Light zusätzlich der Lichtkegel. Den stellt im Inspector der Doppel-Regler **Inner / Outer Spot Angle** ein.
- **Probier:** Tageszeit auf **Nacht**, dann die Farbe von `ColorLight_Red` ändern. Wo sich Rot und Grün überlagern, wird es gelb. Wichtig: das Objekt `ColorLight_Red` selbst wählen, nicht die Glas-Kugel `Lamp` darunter (dort sitzt kein Licht).

### Eigene Skripte: Door Controller und Rotator

- **Sag:** „Ein Skript ist eine Komponente, die jemand programmiert hat. Die Felder im Inspector sind die Stellschrauben.“
- **Zeig:** `AutomaticDoor` mit **Door Controller** (Open Speed, Slide Offset, Motion) und `Crystal` mit **Rotator** (Speed, Rotation Axis, Is Rotating).
- **Probier:** Open Speed auf 0.2 (Tür braucht 5 Sekunden), Speed des Kristalls von 45 auf 90.

Die Sandbox-Komponenten haben über ihren Feldern kleine Test-Knöpfe. Im Play Mode gibt es zum Beispiel beim Door Controller **Öffnen**, **Schließen**, **Umschalten**, beim Rotator jederzeit **Doppelt so schnell**, **Halb so schnell** und **Stopp** / **Start**.

Wenn du ihr Code zeigen willst: `Assets/StudentSandbox/Scripts/Runtime/Interaction/Rotator.cs` doppelklicken. Der Kern ist eine Zeile pro Bild:

```csharp
transform.Rotate(rotationAxis.normalized, speed * Time.deltaTime, Space.Self);
```

### Event „On Pressed ()“

- **Sag:** „Ein Event ist eine Wenn-dann-Regel: Wenn jemand auf den Knopf klickt, dann ruft der Knopf eine Funktion bei einem anderen Objekt auf.“
- **Zeig:** `DoorSwitch` wählen. In der Komponente **Clickable Switch** steht unter **On Pressed ()** links das Objekt `AutomaticDoor` und rechts die Funktion `DoorController.Toggle`. Eine blaue Karte darüber fasst es zusammen: „Beim Klicken passiert: AutomaticDoor → DoorController.Toggle“.
- **Probier:** `Desk_CrystalButton` wählen, unter On Pressed auf **+** klicken, `AutomaticDoor` in das neue Feld ziehen und als Funktion **DoorController > Toggle** wählen. Im Play Mode öffnet der Knopf KRISTALL jetzt auch die Tür.

## 10. Welche Probleme können beim Ausprobieren auftreten?

| Problem | Ursache | Lösung |
|---|---|---|
| Änderungen sind nach dem Play Mode weg | Alles, was im Play Mode geändert wird, setzt Unity beim Beenden zurück. | Bauen und verändern immer im Edit Mode. Das Workshop-Fenster zeigt im Play Mode eine orange Warnung. |
| Objekt ist verschwunden | gelöscht, verschoben oder sehr klein skaliert | Strg+Z. Oder den Namen ins Suchfeld der Hierarchy tippen, Objekt anklicken und in der Scene View F drücken (oder **Ausgewähltes Objekt zeigen**). |
| „Zeig mir“ meldet „… wurde nicht gefunden“ | Objekt umbenannt oder gelöscht (ist eine andere Szene offen, sind die Knöpfe stattdessen grau) | Strg+Z oder mit F2 zurückbenennen |
| Kugel fällt herunter oder bleibt liegen | Physik ist durcheinander | Taste R oder Knopf RESET. Kugeln, die ins Wasser fallen (unter y = -5), kommen von selbst zurück an den Start. |
| Eigener Würfel mit Rigidbody kommt bei R nicht zurück | R setzt nur Objekte mit einer Reset-Komponente zurück (z. B. **Physics Ball** oder **Resettable Object**) | Play beenden, dem Würfel **Add Component > Student Sandbox > Resettable Object** geben |
| Rote Kugel erreicht nach Rampen-Änderung das Ziel nicht | Winkel zu extrem, oder nur die Fahrbahn gedreht | `Ramp_Main` wählen (nicht `Ramp_Surface`), Rotation Z zwischen -12 und -30 |
| Blaue Kugel im Hauptziel zählt nicht | `GoalZone` akzeptiert nur die rote Kugel (Feld **Required Ball**) | Die blaue Kugel gehört in die `TargetZone`. Feld Required Ball leeren, dann zählt jede Kugel. |
| Hammer schwingt nach R nicht mehr, oder DOMINO meldet „Erst RESET drücken (Taste R)“ | Von selbst schwingt der Hammer nur einmal, kurz nach Play. Nach einem Reset wartet er. Die Meldung kommt, wenn er schon geschwungen ist. | R oder RESET drücken, dann den Knopf DOMINO klicken |
| In der Scene View verirrt, man sieht nur Himmel oder Wasser | zu weit weggeflogen oder gezoomt | Workshop: **Zur Weltübersicht** oder einen Bereichs-Knopf. Menü: **Tools > Student Sandbox > Frame World Overview**. |
| Game-Fenster zeigt nach Play eine ganz andere Stelle | Die Kamera startet dort, wohin die Scene View gerade schaut. | Tasten 1 bis 5, oder vor Play im Workshop einen Bereichs-Knopf klicken. Soll Play immer an der Weltübersicht starten: bei `Main Camera` in der Free Fly Camera den Haken **Start At Scene View** entfernen. |
| Kamera bewegt sich im Play Mode nicht oder nur ganz langsam | Das Game-Fenster hat keinen Fokus, oder das Mausrad hat das Tempo auf 1 m/s gedrückt. | Einmal ins Game-Fenster klicken, Mausrad nach oben drehen |
| Klick auf einen Knopf reagiert nicht | Game-Fenster nicht fokussiert, rechte Maustaste gedrückt (dann sind Klicks aus), Zeiger nicht auf dem Knopf, Edit Mode, oder der Haken vor **Clickable Switch** ist aus | Ins Game-Fenster klicken, rechte Maustaste loslassen, zielen, bis „Klick: …“ erscheint. Haken der Komponente prüfen. Notlösung im Play Mode: Knopf in der Hierarchy wählen und im Inspector **Jetzt drücken (Test)** klicken. |
| Bumper stößt nicht, Ziel meldet nichts | Der Haken vor der Komponente **Bumper** bzw. **Goal Zone** ist aus. Dann ist sie wirklich abgeschaltet. | Haken im Inspector wieder setzen |
| Ampel schaltet nicht von allein | **Auto Cycle** ist am Anfang aus | Knopf AUTO am Kontrollpult oder Haken bei Auto Cycle |
| Sonne springt auf andere Werte zurück | Ein Tageszeit-Knopf (im Workshop-Fenster, im Inspector des `DayNightController` oder TAG / ABEND / NACHT im Lichtlabor), Taste T oder jede andere Änderung im Inspector des `DayNightController` wendet die Tageszeit neu an. Play allein ändert nichts, weil **Apply On Start** aus ist. | Im Edit Mode Strg+Z. Dauerhaft geht es im `DayNightController` unter Day, Sunset oder Night (Sun Rotation, Sun Color, Sun Intensity). |
| Szene ist dunkel oder orange | Tageszeit steht auf Nacht oder Sonnenuntergang (wird mit der Szene gespeichert und bleibt auch im Play Mode so) | Workshop: Tageszeit **Tag** |
| Farbige Lichter oder der Spot-Schatten sind kaum zu sehen | Bei Tag überstrahlt die Sonne alles | Sonnenuntergang oder Nacht einstellen |
| Schatten fehlen in der Ferne | Die Projekt-Einstellung `Assets/Settings/PC_RPAsset` hat 50 m Schattendistanz (Shadows > Max Distance). Sie wurde absichtlich nicht geändert, weil sie fürs ganze Projekt gilt. | Näher heranfliegen (Tasten 2 bis 5). Innerhalb eines Bereichs gibt es überall Schatten. |
| Viele Objekte haben plötzlich eine andere Farbe | Das Material selbst wurde geändert (z. B. Farbe im Material-Inspector), nicht einem Objekt ein anderes zugewiesen | Strg+Z. Sonst **Ausgangsszene wiederherstellen**, das setzt auch die Materialien zurück. |
| Objekte sind pink | Der Shader passt nicht zur Render Pipeline, meist weil keine URP-Pipeline zugewiesen ist | **Project Settings > Graphics**: Default Render Pipeline = `PC_RPAsset`. Unter **Quality** darf beim aktiven Level kein anderes Pipeline-Asset stehen (leer lassen oder ein URP-Asset wie `PC_RPAsset` bzw. `Mobile_RPAsset`). Danach notfalls **Spielwiese neu erzeugen**. |
| Knöpfe im Workshop-Fenster sind grau | Play Mode läuft (Sichern, Wiederherstellen, Neu-Erzeugen gehen nur im Edit Mode) oder die Spielwiese ist nicht offen (dann sind die Bereichs-Knöpfe, **Zum Bereich** und **Zeig mir** grau). **Physik zurücksetzen** geht umgekehrt nur im Play Mode. | Play beenden bzw. **Spielwiese öffnen** |
| Scene View zeigt nur noch ein einzelnes Objekt, das Workshop-Fenster meldet „Du bearbeitest gerade ein Prefab“ | Ein Prefab ist im Prefab-Modus offen (z. B. nach einem Doppelklick auf ein Prefab im Project-Fenster) | Workshop: **Zurück zur Spielwiese**, oder den Pfeil oben links in der Hierarchy |
| Schrift auf Schildern ist nachts genauso hell | Die Schilder nutzen 3D-Text (TextMesh) ohne Beleuchtung. | Bekannte Einschränkung, kein Fehler |
| Workshop-Fenster ist weg | geschlossen | **Tools > Student Sandbox > Workshop** |

## Fachbegriffe

| Begriff | Einfach erklärt |
|---|---|
| Szene (Scene) | eine 3D-Welt, die als Datei gespeichert wird; hier `StudentSandbox.unity` |
| GameObject | jedes Ding in der Szene: Baum, Haus, Licht, sogar die Kamera |
| Hierarchy | die Liste links mit allen GameObjects der Szene, nach Bereichen sortiert |
| Inspector | das Feld rechts; zeigt, woraus das ausgewählte Objekt besteht und was man einstellen kann |
| Komponente (Component) | ein Baustein im Inspector, der einem Objekt eine Fähigkeit gibt, zum Beispiel Rigidbody oder Light |
| Prefab | eine Vorlage wie ein Stempel; in der Hierarchy mit blauem Würfel-Symbol |
| Asset | eine Datei im Project-Fenster: Prefab, Material, Skript, Szene |
| Skript (Script) | eine selbst programmierte Komponente, zum Beispiel der Door Controller |
| Event | eine Wenn-dann-Verbindung: Klick auf den Knopf, dann bewegt sich die Tür |
| Play Mode | das Spiel läuft (Physik, Klicks, Steuerung); im Edit Mode wird gebaut. Was im Play Mode geändert wird, ist danach wieder weg. |

## Weitere Dokumente

- [WORKSHOP_DE.md](WORKSHOP_DE.md): der 90-Minuten-Ablauf mit Erklärungen und Aufgaben
- [CHALLENGES_DE.md](CHALLENGES_DE.md): zusätzliche Herausforderungen nach Schwierigkeit
- [TECHNICAL_OVERVIEW_DE.md](TECHNICAL_OVERVIEW_DE.md): Architektur, Generator, Tests und Erweiterung
