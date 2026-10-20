# Workshop-Anleitung: Mini World – Unity Playground

**90 Minuten Unity – für eine Schülerin (13 Jahre) und ihren Bruder**

Diese Anleitung führt durch den Praktikumstag in Unity. Sie ist für den Erwachsenen geschrieben, der den Workshop begleitet (im Text „der Bruder“).
Die Schritte unter **„Jetzt übernimmt sie die Maus“** stehen in Du-Form. Du kannst sie vorlesen, oder sie liest sie selbst.

Weitere Dateien in diesem Ordner:

- `README_DE.md` – Einrichtung, Steuerung, Wiederherstellen, Fehlerhilfe (für den Bruder)
- `CHALLENGES_DE.md` – zusätzliche Aufgaben, falls Zeit übrig ist
- `TECHNICAL_OVERVIEW_DE.md` – wie die Spielwiese technisch aufgebaut ist

Im Workshop-Fenster öffnet der Knopf **„Anleitung öffnen“** genau diese Datei.

---

## Auf einen Blick

| Zeit | Phase | Bereich | Lektion im Workshop-Fenster |
|---|---|---|---|
| 0:00–0:10 | 1 · Unity entdecken | Übersicht, Baubereich | „1. Objekte bewegen“, Schritte 1–3 |
| 0:10–0:30 | 2 · Eigene Welt bauen | Baubereich | „1. Objekte bewegen“ (Rest) und „2. Eigene Welt gestalten“ |
| 0:30–0:50 | 3 · Physik | Physiklabor | „3. Physik ausprobieren“ |
| 0:50–1:05 | 4 · Licht | Lichtlabor | „4. Licht und Schatten“ |
| 1:05–1:20 | 5 · Interaktionen | Interaktionsbereich | „5. Interaktionen testen“ |
| 1:20–1:30 | 6 · Eigene Herausforderung | freie Wahl | „6. Eigene Herausforderung“ |
| danach | Abschluss & Reflexion, optional KI-Demo | – | – |

### Die Insel

| Bereich | Lage | Farbe | Play Mode: Taste | Workshop-Fenster: Knopf |
|---|---|---|---|---|
| Übersicht (ganze Insel) | – | – | 1 | „Zur Weltübersicht“ |
| Baubereich (CREATIVE) | Westen | Mintgrün | 2 | „Zum Baubereich“ |
| Physiklabor (PHYSICS) | Norden | Orange | 3 | „Zum Physiklabor“ |
| Lichtlabor (LIGHT) | Osten | Lavendel | 4 | „Zum Lichtlabor“ |
| Interaktionsbereich (INTERACTIONS) | Süden | Türkis | 5 | „Zum Interaktionsbereich“ |

In der Mitte liegt der zentrale Platz mit Brunnen und Wegweiser. Jeder Bereich hat einen Eingangsbogen mit seinem Namen.

---

## Vorbereitung (5 Minuten vorher)

Die Szene ist schon fertig erzeugt und gespeichert. Du musst nichts generieren.

1. Projekt in Unity öffnen. Dann **Tools > Student Sandbox > Open Sandbox Scene** (oder im Project-Fenster Doppelklick auf `Assets/StudentSandbox/Scenes/StudentSandbox.unity`). Die Scene View springt automatisch zur Weltübersicht. Nicht die Kopie in `Scenes/Original` öffnen – das ist die unberührte Ausgangsszene. Öffnet man sie doch, fragt ein Fenster, ob stattdessen die richtige Spielwiese geöffnet werden soll („Richtige Spielwiese öffnen“).
2. Console prüfen: Es dürfen keine roten Fehler da sein.
3. **Tools > Student Sandbox > Workshop** öffnen und andocken (siehe „Bildschirm-Layout“).
4. Im Workshop-Fenster auf „Zum Physiklabor“ klicken, dann einmal Play drücken und kurz zuschauen. Die Play-Kamera startet dort, wo die Scene View gerade hinschaut (sonst ins Game-Fenster klicken und Taste 3 drücken). Die rote Kugel rollt in etwa 4 Sekunden ins Ziel, oben erscheint „Ziel erreicht! Die rote Kugel ist angekommen.“, und der Hammer stößt die Dominosteine um. Play wieder beenden.
5. Im Workshop-Fenster unter **Werkzeuge > Tageszeit** auf „Tag“ stellen. Play übernimmt das Licht genau so, wie es im Editor eingestellt ist.
6. Hat vorher schon jemand geübt? Dann in jeder Lektion „Häkchen zurücksetzen“ klicken und bei Bedarf „Ausgangsszene wiederherstellen“ (setzt auch Materialien und Prefabs zurück).
7. Unter **Werkzeuge** auf „Sicherung anlegen“ klicken (oder **Tools > Student Sandbox > Create Backup of Scene**).
8. Eine Maus mit Mausrad anschließen. Das Fliegen mit gedrückter rechter Maustaste klappt mit einem Touchpad schlecht.
9. Optional: **Edit > Preferences > Colors > Playmode tint** auf eine kräftige Farbe stellen. Dann sieht man sofort, wenn der Play Mode läuft.
10. Optional: **Tools > Student Sandbox > Validate Sandbox** – die Zusammenfassung sollte keine Fehler melden.

## Bildschirm-Layout

So sieht sie alles gleichzeitig:

```
+-------------+-----------------------+-----------------------+-------------+
|  Hierarchy  |      Scene View       |      Game-Fenster     |  Inspector  |
|             |   (bauen, Edit Mode)  |  (spielen, Play Mode) |  Workshop-  |
|             |                       |                       |  Fenster    |
+-------------+-----------------------+-----------------------+  (als Tab)  |
|                 Project-Fenster                             |             |
+-------------------------------------------------------------+-------------+
```

1. Den Reiter **Game** mit der Maus an den rechten Rand der Scene View ziehen und loslassen. Jetzt liegen Scene und Game nebeneinander.
2. Der **Inspector** bleibt rechts. Das **Workshop-Fenster** als Reiter neben den Inspector (oder neben das Project-Fenster) ziehen.
3. Durcheinander geraten? **Window > Layouts > Default** stellt das Standard-Layout wieder her.

## Regeln für den Begleiter

- **Kurz erklären, dann gehört die Maus ihr.** Höchstens 2–3 Minuten am Stück reden. Danach nur noch Tipps geben, nicht selbst klicken.
- **Vor jedem Play fragen:** „Was glaubst du, passiert jetzt?“
- **Gebaut wird im Edit Mode.** Alles, was im Play Mode verändert wird, ist nach dem Beenden wieder weg. Das passiert jedem einmal – kurz erklären, dann weiter.
- **Fehler sind erlaubt.** Strg+Z macht den letzten Schritt rückgängig. Notfalls gibt es die Sicherung und „Ausgangsszene wiederherstellen“. Das setzt Szene, Materialien und Prefabs zurück und legt vorher selbst eine Sicherung an.
- **Notiz-Karten lesen lassen.** Fast jedes wichtige Objekt hat im Inspector (meist gleich unter dem Transform) eine farbige Notiz-Karte (Komponente Info Note), die erklärt, was man ausprobieren kann.
- **Prefabs nicht „anwenden“.** Bei Prefab-Objekten gibt es oben im Inspector das Menü „Overrides“. Dort nicht „Apply All“ klicken – das würde die Vorlage für alle Kopien ändern. Ist es doch passiert, holt „Ausgangsszene wiederherstellen“ die alten Vorlagen zurück – dann ist aber auch die Szene wieder im Anfangszustand.
- **Häkchen setzen.** Im Workshop-Fenster hat jeder Schritt einer Lektion ein Kästchen. Abhaken macht Spaß und zeigt den Fortschritt.

---

## Phase 1 – Unity entdecken (10 Minuten, 0:00–0:10)

**Ziel:** Sie kann sich in der Scene View bewegen, Objekte auswählen und weiß, wofür Hierarchy, Inspector und Play da sind.

### Das erklärt der Bruder

- **Game Engine:** Eine Game Engine ist ein Programm, mit dem man Spiele baut. Sie zeichnet die 3D-Welt, rechnet Physik und Licht aus und reagiert auf Maus und Tastatur. Unity ist so eine Game Engine.
- **Scene:** Eine Scene ist eine 3D-Welt, wie ein Level in einem Spiel. Unsere Scene heißt „StudentSandbox“ – das ist die ganze Insel.
- **GameObject:** Alles in der Scene ist ein GameObject: ein Baum, eine Lampe, sogar die Kamera. Ein GameObject kann Kind-Objekte haben, zum Beispiel Stamm und Krone eines Baums.
- **Hierarchy:** Die Hierarchy (links) ist die Liste aller GameObjects. Mit dem kleinen Pfeil klappt man eine Gruppe auf. Die Gruppen 00 bis 09 sortieren die Insel: 03 ist der Baubereich, 04 das Physiklabor und so weiter.
- **Inspector:** Der Inspector (rechts) zeigt, was das ausgewählte Objekt ist und kann. Ganz oben steht immer der Transform mit Position, Rotation und Scale.
- **Play:** Mit dem Play-Knopf startet das Spiel. Dann läuft die Physik und man kann Knöpfe anklicken. Ein zweiter Klick beendet das Spiel.

### Jetzt übernimmt sie die Maus

1. **(1 Min)** Klicke im Workshop-Fenster auf die Lektion „1. Objekte bewegen“. Klicke dann unter „Bereich auswählen“ auf „Zur Weltübersicht“. Du siehst die ganze Insel.
2. **(2 Min)** Fliegen: Halte die Maus über die Scene View, **halte die rechte Maustaste gedrückt** und fliege mit **W A S D**. **Q / E** = runter / hoch, **Shift** = schneller, **Mausrad** = zoomen. Flieg einmal zum Brunnen in der Mitte.
3. **(2 Min)** Klicke auf „Zum Baubereich“. Klicke in der Scene View auf einen Baum. In der Hierarchy wird sein Name markiert (zum Beispiel `Tree_Small_1`), und im Inspector siehst du seine Notiz-Karte und den Transform. Klicke danach ein Haus und eine Bank an.
4. **(2 Min)** Klappe in der Hierarchy `03_CREATIVE_ZONE` > `EditableObjects` > `Houses` auf. Mach einen **Doppelklick auf `House_Red`**: Die Scene View fliegt zum roten Haus. Probier auch: Objekt auswählen, Maus über die Scene View, Taste **F** drücken.
5. **(3 Min)** Klicke im Workshop-Fenster auf **„▶ Play starten“**. Das Game-Fenster zeigt zuerst dieselbe Stelle wie deine Scene View. Klicke einmal in das Game-Fenster. Drücke **2, 3, 4, 5** und **1**: Die Kamera fliegt zu den Bereichen. Rechte Maustaste halten = umschauen, W A S D = fliegen, **H** = Hilfe ein/aus. Danach **„■ Play beenden“**.

### Was sie sehen sollte

- Das ausgewählte Objekt hat in der Scene View einen orangen Umriss.
- Im Game-Fenster steht oben links die Box „Steuerung“. Beim Fliegen mit den Tasten 1 bis 5 erscheint kurz der Name des Bereichs, zum Beispiel „Baubereich (Creative)“.
- Im Workshop-Fenster steht beim Status „Play Mode – das Spiel läuft …“.

### Falls etwas schiefgeht

| Problem | Lösung |
|---|---|
| Man sieht nur Himmel oder Wasser | Workshop-Fenster: „Zur Weltübersicht“ (oder **Tools > Student Sandbox > Frame World Overview**) |
| W A S D bewegt die Ansicht nicht | Die rechte Maustaste muss gedrückt bleiben. Ohne sie wechseln W, E, R nur das Werkzeug. |
| Im Game-Fenster bewegt sich die Kamera nicht | Einmal in das Game-Fenster klicken – es braucht den Fokus. |
| Der Mauszeiger ist weg | Rechte Maustaste loslassen oder **Esc** drücken. |
| Beim Baum ist nur die Krone (`Crown`) ausgewählt | Bei Prefabs wählt der erste Klick das ganze Objekt, ein zweiter Klick ein Einzelteil. Das ganze Objekt in der Hierarchy anklicken. |

---

## Phase 2 – Eigene Welt bauen (20 Minuten, 0:10–0:30)

**Ziel:** Sie kann Objekte verschieben, drehen und skalieren, Prefabs in die Welt ziehen und Objekten eine neue Farbe geben.

### Das erklärt der Bruder

- **Position:** Die Position sagt, wo ein Objekt steht: X = links/rechts, Y = hoch/runter, Z = vor/zurück. Die Zahlen sind Meter.
- **Rotation:** Die Rotation sagt, wie das Objekt gedreht ist, in Grad. Rotation Y = 90 ist eine Vierteldrehung, wie bei einem Drehstuhl.
- **Scale:** Scale ist die Größe. 1 = normal, 2 = doppelt so groß, 0.5 = halb so groß.
- **Prefab:** Ein Prefab ist eine Vorlage, wie ein Stempel. Man kann beliebig viele Kopien davon in die Welt setzen. In der Hierarchy erkennt man Prefabs am blauen Würfel-Symbol.
- **Material:** Ein Material bestimmt, wie eine Oberfläche aussieht – vor allem die Farbe. Man zieht es einfach mit der Maus auf ein Objekt.

### Jetzt übernimmt sie die Maus

1. **(2 Min) Baum verschieben:** Klicke in Lektion 1 auf „Zeig mir: Baum“. Jetzt ist `Tree_Small_1` ausgewählt. Drücke **W** (Verschieben) und zieh an einem der farbigen Pfeile (rot = X, grün = Y, blau = Z). Schau zu, wie sich die Zahlen bei Position ändern.
2. **(2 Min) Haus vergrößern:** Klicke auf „Zeig mir: Haus“ (`House_Red`). Drücke **R** (Skalieren) und zieh am grauen Würfel in der Mitte. Oder tippe im Inspector bei Scale X, Y und Z jeweils `1.5` ein.
3. **(2 Min) Bank drehen:** Klicke auf „Zeig mir: Bank“ (`Bench_Pond_1`). Drücke **E** (Drehen) und zieh am grünen Ring. Oder ändere Rotation Y von `180` auf `90`. Drücke dann **Strg+D**: Es entsteht eine Kopie. Schieb sie mit W daneben.
4. **(1 Min) Rückgängig:** Drücke **Strg+Z** ein paar Mal und schau, was passiert. **Strg+Y** stellt es wieder her.
5. **(3 Min) Neue Objekte platzieren:** Wähle im Workshop-Fenster Lektion „2. Eigene Welt gestalten“. Öffne im Project-Fenster `Assets/StudentSandbox/Prefabs/Creative`. Zieh drei Bäume (`SmallTree`, `PineTree` oder `LargeTree`) in die Scene View auf die Wiese.
6. **(3 Min) Materialien ändern:** Öffne im Project-Fenster `Assets/StudentSandbox/Materials/Palette`. Zieh ein Material, zum Beispiel `SkyBlue`, in der Scene View genau auf das Dach (`Roof`) des roten Hauses. Klicke auf „Zeig mir: Farbkasten“: Im Regal zeigt jede Kugel eine Farbe mit ihrem Namen.
7. **(3 Min) Eigene Deko (Aufgabe A1):** Bau dir etwas Schönes, zum Beispiel ein paar Blumen (`Flower`), einen Zaun (`Fence`) oder eine Laterne (`LampPost`).
8. **(1 Min) Beispiel-Turm ansehen (Aufgabe A2):** Klicke auf „Zeig mir: Beispiel-Turm“ (`ExampleTower` im BAUPLATZ). Er besteht aus Würfel, Zylinder, gedrehtem Würfel und Kugel.
9. **(3 Min) Eigenen Turm bauen:** Auf dem Holzpodest liegen Bausteine: `Block_Cube_Red`, `Block_Cube_Yellow`, `Block_Cube_Mint`, `Block_Sphere_Blue`, `Block_Sphere_Lavender`, `Block_Cylinder_Yellow`, `Block_Cylinder_Orange`, `Block_Pillar`, daneben im Sand `Block_Platform`. Stapel daraus einen eigenen, verrückten Turm mit mindestens 3 Farben. Gib ihm einen Namen: Objekt auswählen, **F2** drücken.

**Tipps für den Bruder:**
- Die „Zeig mir“-Knöpfe für Baum, Haus und Bank stehen in Lektion 1, die für Bauplatz, Beispiel-Turm und Farbkasten in Lektion 2.
- Beim Stapeln von der Seite schauen. Profi-Trick: Beim Verschieben die Taste **V** gedrückt halten – dann rastet das Teil an einer Ecke eines anderen Objekts ein.

### Was sie sehen sollte

- Die Zahlen im Transform ändern sich beim Ziehen mit.
- Neue Objekte erscheinen unten in der Hierarchy mit blauem Prefab-Symbol.
- Das Dach hat die neue Farbe. Die anderen Häuser bleiben, wie sie sind.

### Falls etwas schiefgeht

| Problem | Lösung |
|---|---|
| Nur ein Teil (`Roof`, `Crown`, `Seat` …) hat sich bewegt | Strg+Z. Dann das ganze Objekt in der Hierarchy anklicken (eine Ebene höher). |
| Ein Objekt ist verschwunden | Strg+Z. Oder oben in der Hierarchy den Namen ins Suchfeld tippen, Objekt anklicken, Maus über die Scene View, **F** drücken. |
| Neues Objekt schwebt oder steckt im Boden | Position Y auf `0.1` stellen – das ist die Bodenhöhe in den Bereichen. |
| Material landet auf dem falschen Teil | Strg+Z. Langsamer ziehen: Beim Loslassen muss das richtige Teil unter der Maus sein. |
| Material auf die Hierarchy gezogen, nichts passiert | Direkt in die Scene View auf das Objekt ziehen. |
| Plötzlich haben viele Objekte eine andere Farbe | Die Material-Datei selbst wurde verändert (oder bei „Overrides“ wurde „Apply All“ geklickt). Sofort Strg+Z. Hilft das nicht: „Ausgangsszene wiederherstellen“ setzt auch die Materialien und Prefabs zurück. |
| W, E, R reagieren nicht | Die Maus muss über der Scene View sein. |
| Die Scene View zeigt nur noch ein einzelnes Objekt | Ein Prefab wurde geöffnet (Prefab-Modus, z. B. nach Doppelklick im Project-Fenster). Im Workshop-Fenster auf „Zurück zur Spielwiese“ klicken oder oben in der Hierarchy auf den Pfeil nach links. |

---

## Phase 3 – Physik (20 Minuten, 0:30–0:50)

**Ziel:** Sie weiß, was Schwerkraft, Collider und Rigidbody sind, und kann ein Physik-Experiment verändern und neu testen.

### Das erklärt der Bruder

- **Schwerkraft:** Die Schwerkraft zieht alles nach unten. Auf der Erde sind das 9,81 m/s² – das steht auch auf der Tafel im Physiklabor.
- **Collider:** Ein Collider ist eine unsichtbare Hülle um ein Objekt. Nur mit Collider können Dinge zusammenstoßen oder aufeinander liegen.
- **Rigidbody:** Ein Rigidbody sorgt dafür, dass Unity ein Objekt wie einen echten Körper behandelt: Es hat ein Gewicht (Mass), fällt nach unten und kann geschubst werden. Ohne Rigidbody bleibt ein Objekt einfach in der Luft stehen.
- **Simulation:** Simulation heißt: Unity rechnet viele Male pro Sekunde aus, wie sich alles bewegt. Das passiert nur im Play Mode.
- **Trigger:** Das Ziel ist ein Trigger – ein Collider, durch den man hindurchrollt. Er merkt nur, dass etwas hineingekommen ist.

### Jetzt übernimmt sie die Maus

1. **(2 Min) Kugel ansehen:** Wähle Lektion „3. Physik ausprobieren“ und klicke auf „Zum Bereich: Physiklabor“. Klicke auf „Zeig mir: Kugel“. Im Inspector siehst du bei `PhysicsBall`: Sphere Collider, Rigidbody (Mass `1`, Haken bei Use Gravity) und Physics Ball.
2. **(2 Min) Kugel rollen lassen:** Drücke Play. Die Kamera startet dort, wo deine Scene View hinschaut – also direkt bei der Kugel. (Siehst du das Labor nicht? Ins Game-Fenster klicken und Taste **3** drücken.) Die rote Kugel rollt vom Startturm die Rampe hinunter und in etwa 4 Sekunden ins Ziel. Kurz nach dem Start schwingt auch der Hammer und stößt die Dominosteine an. Beende Play.
3. **(2 Min) Rampe verändern:** Klicke auf „Zeig mir: Rampe“. Ausgewählt ist jetzt `Ramp_Main`. Ändere im Inspector **Rotation Z** von `-22` auf `-30` (steiler).
4. **(3 Min) Erneut testen:** Drücke Play. Rollt die Kugel schneller? Mit **R** setzt du alles zurück und schaust noch einmal. Beende Play, stell `-15` ein (flacher) und teste noch einmal mit Play. Danach Play wieder beenden. Zwischen `-12` und `-30` kommt die Kugel sicher im Ziel an.
5. **(2 Min) Hindernis verschieben:** Klappe in der Hierarchy `04_PHYSICS_ZONE` > `ObstacleCourse` auf. Wähle `ObstacleCube_1` und stell ihn auf Position X = `-9`, Z = `-4`. Drücke Play: Die blaue Kugel rollt die große Rampe hinunter. Was passiert mit dem Würfel? Beende Play und drücke Strg+Z, damit der Würfel wieder aus dem Weg ist.
6. **(2 Min) Das Rätsel ansehen:** Klicke auf „Zeig mir: Blaue Kugel“ (`PhysicsBall_Blue`) und drücke Play. Die Kugel rollt am roten Zielfeld `TargetZone` vorbei bis zur Mauer. Beende Play.
7. **(3 Min) Zielzone erreichen:** Verschieb (W) und dreh (E) die Bauteile `WallBlock_1`, `WallBlock_2`, `Bumper_1` und `SmallRamp_1`, bis die blaue Kugel in die TargetZone rollt. Nach jedem Versuch: Play, zuschauen, Play beenden, nachbessern.
8. **(3 Min) Dominos:** Drücke Play und schau den 10 Dominosteinen zu. Drücke **R**: Alle Steine stehen wieder, und der Hammer wartet. Klicke auf den Knopf **DOMINO** – jetzt schwingt der Hammer noch einmal. Beende Play. Schieb `Domino_05` zur Seite (Position Z um `0.5` ändern) oder dreh ihn (Rotation Y = `45`). Klappt die Kettenreaktion noch?

**Tipps für den Bruder zum Rätsel (Schritt 7):**
- Eine schräg gestellte Mauer (Rotation Y = `45`) in der Bahn der blauen Kugel lenkt sie nach Osten, Richtung Ziel. Die genaue Stelle muss sie ausprobieren – dabei lernt sie am meisten.
- Notlösung, die sicher klappt (im Test geprüft): die `TargetZone` selbst in den Weg stellen, Position X = `-9`, Z = `-5.7`.

### Was sie sehen sollte

- Meldung „Ziel erreicht! Die rote Kugel ist angekommen.“ Die Zielplatte wird grün, und ein grünes Licht geht an. Danach rollt die Kugel weiter, stößt die drei Kegel `Pin_1` bis `Pin_3` im Zielbereich um, und die Wand `Goal_Backstop` stoppt sie.
- Alle 10 Dominosteine fallen nacheinander um. Der letzte lehnt am Fangbrett.
- Bei **R** erscheint „Zurückgesetzt!“, und alles steht wieder am Start. Der Hammer wartet auf den DOMINO-Knopf.
- Erreicht die blaue Kugel die TargetZone: Das Feld wird grün, und es erscheint „Geschafft! Eine Kugel ist im Ziel.“

### Falls etwas schiefgeht

| Problem | Lösung |
|---|---|
| Nur die Fahrbahn hat sich gedreht, die Rampe hängt schief | `Ramp_Surface` statt `Ramp_Main` erwischt. Strg+Z, dann „Zeig mir: Rampe“. |
| Die Kugel kommt nicht mehr an | Rotation Z von `Ramp_Main` zurück auf `-22`. |
| Die Kugel fällt ins Wasser | Kein Problem: Sie kommt von selbst an den Start zurück. |
| Nach Play sind alle Änderungen weg | Sie wurden im Play Mode gemacht. Play beenden, dann noch einmal ändern. |
| Nach R fallen die Dominosteine nicht mehr | Richtig so: Nach einem Reset wartet der Hammer. Auf DOMINO klicken. |
| DOMINO-Knopf zeigt „Erst RESET drücken (Taste R)“ | Der Hammer hat schon geschwungen. Erst R drücken, dann DOMINO. |
| Klick auf RESET oder DOMINO wirkt nicht | Einmal ins Game-Fenster klicken, rechte Maustaste loslassen. Über dem Knopf muss „Klick: …“ erscheinen. |
| Die blaue Kugel rollt ins Ziel der Murmelbahn, aber nichts passiert | Dieses Ziel zählt nur die rote Kugel (Feld Required Ball an der `GoalZone`). |
| Auch die rote Kugel löst das Ziel nicht mehr aus | Bei `GoalZone` wurde der Haken vor der Komponente Goal Zone entfernt – dann ist sie wirklich aus. Haken wieder setzen. |
| Taste R tut nichts | Läuft Play? Hat das Game-Fenster den Fokus? |

---

## Phase 4 – Licht (15 Minuten, 0:50–1:05)

**Ziel:** Sie kennt drei Lichtarten, kann Farbe, Helligkeit und Richtung eines Lichts ändern und sieht, wie Schatten entstehen.

### Das erklärt der Bruder

- **Lichtquellen:** Ein Directional Light ist wie die Sonne: Es leuchtet überall gleich, nur seine Drehung zählt. Ein Point Light ist wie eine Glühbirne und leuchtet in alle Richtungen. Ein Spot Light ist wie eine Taschenlampe und leuchtet in einem Kegel.
- **Schatten:** Ein Schatten entsteht, wo ein Objekt das Licht verdeckt. Dreht oder verschiebt man das Licht, wandert der Schatten mit.
- **Lichtfarbe:** Die Farbe stellt man an der Light-Komponente bei **Color** ein. Lichtfarben mischen sich anders als Malfarben: Rotes und grünes Licht ergeben Gelb.
- **Intensität:** **Intensity** ist die Helligkeit. Bei einem Point Light sagt **Range**, wie weit das Licht reicht.
- Wichtig: Licht ändert sich sofort in der Scene View. Dafür braucht man kein Play. Und was man im Edit Mode am Licht einstellt, bleibt auch nach dem Drücken von Play so.

### Jetzt übernimmt sie die Maus

1. **(1 Min)** Wähle Lektion „4. Licht und Schatten“ und klicke auf „Zum Bereich: Lichtlabor“.
2. **(3 Min) Tag und Nacht vergleichen:** Klicke auf „Zeig mir: Tageszeit“ (`DayNightController`). Klicke im Inspector nacheinander auf **TAG**, **SONNENUNTERGANG** und **NACHT**. Was ändert sich? (Sonne, Himmel, Laternen, Fenster.) Die gleichen Knöpfe gibt es im Workshop-Fenster unter Werkzeuge > Tageszeit.
3. **(3 Min) Farbe einer Lampe:** Lass NACHT eingestellt. Klicke auf „Zeig mir: Laterne“ (`StreetLamp_1` in `05_LIGHT_ZONE` > `StreetLamps`), klappe sie in der Hierarchy auf und wähle das Kind-Objekt **`LampLight`**. Klicke bei der Light-Komponente auf **Color** und wähle deine Lieblingsfarbe. Danach: „Zeig mir: Rotes Licht“ (`ColorLight_Red`) und auch dort die Color ändern.
4. **(3 Min) Lichtquelle verschieben:** Klicke auf „Zeig mir: Blaues Licht“ (`ColorLight_Blue` in der Gruppe `ColorLab`). Verschieb es mit **W**. Schau auf die weiße Kugel und die weiße Rückwand der Bühne: Wo sich Farben mischen, entstehen neue Farben, und die Schatten werden bunt.
5. **(3 Min) Schatten beobachten:** Klicke auf „Zeig mir: Spot-Lampe“ (`ShadowSpotLight`). Dreh die Lampe mit **E**. Der Schatten der Figur `ShadowSculpture` wandert über die weiße Platte. Ändere auch Intensity und den Lichtkegel. Der heißt im Inspector **Inner / Outer Spot Angle** und ist ein Schieberegler mit zwei Griffen.
6. **(2 Min) Sonne drehen:** Stell wieder **TAG** ein. Klicke auf „Zeig mir: Sonne“ (`Sun`). Die Sonne wird nur ausgewählt, die Scene View bleibt, wo sie ist. Ändere **Rotation X**: `50` = Sonne hoch, kurze Schatten; `10` = Sonne tief, lange Schatten. Schau auf die Sonnenuhr auf dem Dorfplatz. Deine Sonne bleibt auch im Play Mode so. Erst ein Tageszeit-Knopf (TAG, SONNENUNTERGANG, NACHT) stellt sie wieder auf die Tageszeit zurück.

Im Play Mode geht die Tageszeit mit **Taste T** oder mit den Knöpfen am Tageszeit-Pult: **TAG**, **ABEND** (Sonnenuntergang) und **NACHT**.

### Was sie sehen sollte

- Bei NACHT: dunkler Himmel mit Sternen, Laternen und Fenster leuchten, die Lämpchen am Bühnendach gehen an.
- Die farbigen Lichter färben die weißen Objekte auf der Bühne. Rot und Grün zusammen ergeben Gelb.
- Das Spot Light macht einen Lichtkegel. Der Schatten der Skulptur bewegt sich, wenn die Lampe gedreht wird.

### Falls etwas schiefgeht

| Problem | Lösung |
|---|---|
| Nur die Glaskugel bewegt sich, das Licht bleibt stehen | Das Kind-Objekt `Lamp` wurde erwischt (seine Notiz-Karte sagt das auch). Strg+Z, dann `ColorLight_Blue` in der Hierarchy wählen. |
| Die farbigen Lichter sieht man kaum | Bei Tag überstrahlt die Sonne alles. NACHT oder SONNENUNTERGANG einstellen. |
| Laternenfarbe geändert, aber die Laterne bleibt dunkel | Laternen leuchten nur bei Sonnenuntergang und Nacht (Lamp Controller, Haken Follow Day Night). Auf NACHT stellen oder beim Lamp Controller auf „An“ klicken. |
| Die Sonne ist plötzlich wieder anders gedreht | Ein Tageszeit-Knopf wurde gedrückt (im Inspector, im Workshop-Fenster, am Tageszeit-Pult oder Taste T) oder eine Einstellung am DayNightController geändert. Beides stellt die Sonne auf die Werte der Tageszeit. Für eine dauerhafte Änderung im DayNightController unter Day die Sun Rotation ändern. |
| Aus Versehen die Tageszeit gewechselt | Strg+Z – das holt auch Himmel, Umgebungslicht und Nebel zurück. |
| Die Scene View ist ohne Licht und Schatten | In der Leiste oben in der Scene View das Licht-Symbol (Lighting) einschalten. |
| In der Weltübersicht fehlen weit entfernte Schatten | Normal: Schatten werden nur bis 50 m Entfernung gezeichnet (Projekteinstellung). Näher heranfliegen. |

---

## Phase 5 – Interaktionen (15 Minuten, 1:05–1:20)

**Ziel:** Sie versteht, wie ein Klick über ein Event eine Funktion bei einem anderen Objekt auslöst, und kann das Verhalten im Inspector ändern.

### Das erklärt der Bruder

- **Komponenten:** Ein GameObject bekommt seine Fähigkeiten durch Komponenten – die Bausteine im Inspector. Die Tür hat zum Beispiel einen Mesh Renderer (Aussehen), einen Box Collider (Hülle) und einen Door Controller (Verhalten).
- **Skripte:** Ein Skript ist eine selbst programmierte Komponente. Der Door Controller ist so ein Skript: Er bewegt die Tür. Seine Einstellungen, zum Beispiel Open Speed, kann man im Inspector ändern, ohne zu programmieren.
- **Events:** Ein Event verbindet zwei Dinge: „Wenn das passiert, mach das.“ Beim Schalter steht im Inspector unter **On Pressed ()**, welches Objekt reagiert und welche Funktion aufgerufen wird.
- **Benutzeraktionen:** Eine Benutzeraktion ist etwas, das die Spielerin tut – hier ein Linksklick im Game-Fenster. Die Kamera erkennt den Klick und drückt den Knopf.

### Jetzt übernimmt sie die Maus

1. **(2 Min) Tür und Schalter ansehen:** Wähle Lektion „5. Interaktionen testen“ und klicke auf „Zum Bereich: Interaktionsbereich“. Klicke auf „Zeig mir: Tür“ (`AutomaticDoor`) und schau dir die Komponente Door Controller an. Klicke dann auf „Zeig mir: Schalter“ (`DoorSwitch`). Unter **On Pressed ()** steht links `AutomaticDoor` und rechts die Funktion `DoorController.Toggle`. Das ist das Event.
2. **(2 Min) Tür öffnen:** Drücke Play. Die Kamera startet bei deiner Scene View (sonst ins Game-Fenster klicken und Taste **5**). Klicke im Game-Fenster mit der linken Maustaste auf den Knopf **TÜR** neben der Tür. Die Tür fährt zur Seite, dahinter liegt die Schatzkammer mit einem goldenen Stern. Noch ein Klick, und sie geht zu. Am Kontrollpult gibt es denselben Knopf noch einmal (`Desk_DoorButton`).
3. **(2 Min) Tür ganz langsam:** Beende Play. Stell bei `AutomaticDoor` **Open Speed** auf `0.2` und teste noch einmal: Jetzt braucht die Tür 5 Sekunden. Beende Play. Wer mag: **Slide Offset** auf `0, 3, 0` – dann fährt die Tür nach oben (mit Play testen, danach Play wieder beenden).
4. **(3 Min) Ampel umschalten:** Drücke Play. Klicke auf den gelben Kasten „DRÜCKEN“ am Ampelmast (`PedestrianButton`) oder auf **AMPEL** am Kontrollpult. Die Ampel schaltet weiter: Rot → Rot-Gelb → Grün → Gelb. Der Knopf **AUTO** lässt sie von allein schalten. Beende Play. Klicke auf „Zeig mir: Ampel“ (`TrafficLight`) und stell beim Traffic Light Controller **Current Phase** auf `2`: Die Ampel zeigt sofort Grün, auch ohne Play.
5. **(3 Min) Drehgeschwindigkeit:** Klicke auf „Zeig mir: Kristall“ (`Crystal`). Ändere beim Rotator **Speed** von `45` auf `90` (oder klicke im Inspector auf „Doppelt so schnell“). Drücke Play: Der Kristall dreht sich doppelt so schnell. Der Knopf **KRISTALL** am Kontrollpult stoppt und startet ihn.
6. **(3 Min) Plattform:** Klicke im Play Mode auf **PLATTFORM**: Die Plattform hält an oder fährt weiter. Beende Play. Klicke auf „Zeig mir: Punkt B“ (`PointB`) und verschieb den Punkt mit **W** ein Stück. Drücke Play: Die Plattform fährt eine neue Strecke. Beende Play. Probier auch bei „Zeig mir: Plattform“ (`MovingPlatform`) **Speed** und **Wait Time** und teste mit Play.

**Bonus, falls Zeit bleibt (vorher Play beenden):**
- Gib einem Knopf eine neue Aufgabe: Wähle `Desk_CrystalButton`, klicke unter On Pressed auf **+**, zieh `AutomaticDoor` aus der Hierarchy in das neue leere Feld und wähle als Funktion **DoorController > Toggle**. Im Play Mode öffnet der KRISTALL-Knopf jetzt auch die Tür.
- Ein Event ganz ohne Programmieren: Wähle im Physiklabor die `GoalZone`. Klicke bei **On Goal Reached ()** auf **+**, zieh `DayNightController` (Gruppe `08_MANAGERS`) hinein und wähle **DayNightController > SetNight**. Wenn die rote Kugel ankommt, wird es Nacht.
- Komponente ausschalten: Wähle `DoorSwitch` und entferne den Haken vor der Komponente **Clickable Switch**. Im Play Mode reagiert der Knopf jetzt nicht mehr – kein Klick, kein „Klick: …“-Text. Danach den Haken wieder setzen.

### Was sie sehen sollte

- Fährt die Maus über einen Knopf, steht daneben „Klick: …“, zum Beispiel „Klick: Tür öffnen / schließen“. Beim Klicken sinkt der Knopf kurz ein.
- Die Tür gleitet in die Wand. In der Schatzkammer dreht sich der goldene Stern.
- Kurze Meldungen wie „Ampel: Automatik AN“, „Drehung: AUS“ oder „Plattform stoppt“.
- Im Inspector zeigt der Door Controller im Play Mode die Knöpfe „Öffnen“, „Schließen“ und „Umschalten“ und wie weit die Tür offen ist.

### Falls etwas schiefgeht

| Problem | Lösung |
|---|---|
| Der Klick auf einen Knopf wirkt nicht | Einmal ins Game-Fenster klicken (Fokus). Rechte Maustaste loslassen. Die Maus muss genau über dem Knopf sein – dann erscheint „Klick: …“. Erscheint nie „Klick: …“: Ist der Haken vor Clickable Switch gesetzt? |
| Der Mauszeiger ist weg | **Esc** drücken. |
| Open Speed geändert, aber nichts ist anders | Wurde im Play Mode geändert und ist wieder weg. Im Edit Mode ändern, dann Play. |
| Die Tür bewegt sich gar nicht mehr | Slide Offset steht auf `0, 0, 0`. Zurück auf `2.6, 0, 0`. |
| Die ganze Plattform-Anlage ist verrutscht | `MovingPlatformSet` statt `PointB` erwischt. Strg+Z, dann „Zeig mir: Punkt B“. |
| Nur die leuchtende Scheibe am Boden hat sich bewegt | Das Kind-Objekt `Marker` wurde erwischt (seine Notiz-Karte sagt das auch). Strg+Z, dann `PointB` wählen. |
| Die Plattform startet woanders | Sie startet im Play Mode immer bei PointA. |
| Eine Zeile unter On Pressed ist weg | Strg+Z. Notfalls „Ausgangsszene wiederherstellen“. |

---

## Phase 6 – Eigene Herausforderung (10 Minuten, 1:20–1:30)

**Ziel: „Gestalte einen kleinen Abenteuerpark.“** Sie plant eine eigene Idee, baut sie und stellt sie vor. Es gibt kein Richtig oder Falsch.

Der Park ist fertig, wenn diese 5 Punkte erfüllt sind:

- [ ] mindestens **3 Objekte** platziert (Prefabs aus dem Project-Fenster)
- [ ] bei mindestens einem Objekt die **Farbe** geändert
- [ ] eine **Physik-Reaktion** ausprobiert
- [ ] ein **Licht** angepasst
- [ ] eine **Interaktion** gestartet

### Das erklärt der Bruder

Nichts Neues – jetzt ist sie die Spieleentwicklerin. Frag lieber „Was soll passieren?“ als Lösungen vorzugeben.

### Jetzt übernimmt sie die Maus

1. **(1 Min)** Wähle Lektion „6. Eigene Herausforderung“. Überleg dir eine Idee und such einen freien Platz, zum Beispiel über „Zeig mir: Bauplatz“ oder auf der Wiese im Baubereich.
2. **(2 Min)** Platziere mindestens 3 Objekte aus `Assets/StudentSandbox/Prefabs/Creative`. Auch `Prefabs/Physics` und `Prefabs/Interactions` haben spannende Teile, zum Beispiel `BouncyBall`, `PushButton` oder `RotatingCrystal`.
3. **(1 Min)** Gib einem Objekt eine neue Farbe aus `Materials/Palette`.
4. **(2 Min) Physik:** Zieh einen `ColoredCube` in die Szene. Wähle ihn aus, klicke im Inspector ganz unten auf **Add Component**, tippe `Rigidbody` und wähle **Rigidbody** (nicht „Rigidbody 2D“). Stell Position Y auf `3`. Drücke Play: Der Würfel fällt herunter.
5. **(1 Min) Licht:** Beende Play. Ändere die Farbe einer Laterne (`LampPost` > Kind-Objekt `LampLight` > Color). Zum Ansehen auf NACHT stellen.
6. **(1 Min) Interaktion:** Drücke Play und klicke auf einen Knopf, der etwas bewegt. Für Profis (im Edit Mode, danach mit Play testen): Ein `PushButton` aus `Prefabs/Interactions` hat eine leere On-Pressed-Liste. Zieh dort das Kind-Objekt `Crystal` eines `RotatingCrystal` hinein und wähle **Rotator > ToggleRotation**.
7. **(1 Min)** Beende Play. Speichere mit **Strg+S** und klicke im Workshop-Fenster unter Werkzeuge auf „Sicherung anlegen“.
8. **(1 Min)** Stell deinen Park vor: Was hast du gebaut? Was war am schwierigsten?

Zusatz-Herausforderung aus Lektion 6: Rechtsklick in die Hierarchy > **Create Empty**, das neue Objekt wie den Park benennen und alle eigenen Objekte hineinziehen.

### Was sie sehen sollte

- Der Würfel fällt im Play Mode auf den Boden und bleibt liegen.
- Im Workshop-Fenster füllt sich der Fortschrittsbalken der Lektion.
- Nach „Sicherung anlegen“ meldet ein Fenster, wo die Sicherung liegt (Ordner `StudentSandbox/Backups`).

### Falls etwas schiefgeht

| Problem | Lösung |
|---|---|
| Der Würfel fällt nicht | „Rigidbody 2D“ erwischt, oder beim Rigidbody fehlt der Haken bei Use Gravity, oder Is Kinematic ist angehakt. |
| Taste R holt den Würfel nicht zurück | R setzt nur vorbereitete Physik-Objekte zurück. Play beenden setzt alles zurück. Wer mag: **Add Component > Resettable Object** hinzufügen. |
| Der neue Knopf tut nichts | Seine On-Pressed-Liste ist leer. **+** klicken, Objekt hineinziehen, Funktion wählen. |
| Die Sicherung fragt nach dem Speichern | „Speichern und sichern“ wählen, sonst fehlt der neueste Stand. |

---

## Abschluss & Reflexion (ca. 5 Minuten)

Drei Fragen zum Schluss. Ist die Zeit knapp, reicht eine davon.

1. **Was hat dir am meisten Spaß gemacht – und warum?**
2. **Was war schwierig, und wie hast du es trotzdem geschafft?**
3. **Wenn du ein eigenes Spiel bauen würdest: Was würde darin passieren?** (Die Antwort ist ein guter Start für die KI-Demo.)

---

## Bonus: KI-Demo (optional, 10–15 Minuten)

**Idee:** Sie beschreibt in einfachen Worten eine neue Funktion. Ein KI-Coding-Agent (zum Beispiel Claude Code) schreibt das C#-Skript. Danach baut sie es selbst in die Szene ein und testet es. So sieht sie, dass hinter den Komponenten im Inspector echter Code steckt. In der Spielwiese ist dafür nichts fertig eingebaut. Sie ist nur so vorbereitet, dass neue Skripte leicht andocken können.

### Ablauf

1. **Sicherung:** Play Mode beenden (im Play Mode ist der Knopf gesperrt). Dann im Workshop-Fenster „Sicherung anlegen“ (oder einen Git-Commit machen).
2. **Idee aufschreiben:** Sie sagt, *was* passieren soll, nicht *wie*. Beispiel: „Lass bei Erreichen der Zielzone ein kleines Feuerwerk aus Partikeln erscheinen.“
3. **Auftrag an die KI:** Der Bruder gibt den Satz weiter und ergänzt die technischen Hinweise (siehe unten).
4. **Warten:** Unity kompiliert das neue Skript kurz (Ladekreis unten rechts).
5. **Einbauen:** Sie fügt die neue Komponente mit **Add Component** hinzu und verbindet sie im Inspector, zum Beispiel über ein Event mit **+**.
6. **Testen und reden:** Play drücken. Danach gemeinsam ins Skript schauen: „Welche Zeile macht was? Wo taucht das im Inspector auf?“

### Technische Hinweise für den Auftrag an die KI

- Neue Skripte nach `Assets/StudentSandbox/Scripts/Runtime/` legen, Namespace `StudentSandbox`.
- Tastatur und Maus nur über `StudentSandbox.SandboxInput` abfragen (zum Beispiel `SandboxInput.GetKeyDown(SandboxKey.Space)`). Das Projekt nutzt nur das Input System Package; die alte `UnityEngine.Input`-Klasse funktioniert hier nicht.
- Wenn möglich eine öffentliche Methode ohne Parameter anbieten. Die kann sie dann im Inspector an ein vorhandenes Event hängen.
- Details zu Aufbau und Erweiterung stehen in `TECHNICAL_OVERVIEW_DE.md`.

### Beispiel-Ideen und wo sie andocken

| Ihre Idee | Daran kann man anknüpfen |
|---|---|
| „Erstelle einen Schalter, der alle Bäume blau einfärbt.“ | Prefab `PushButton` aus `Prefabs/Interactions` mit Event **On Pressed ()**; die Farbe gibt es schon als Material `SkyBlue` in `Materials/Palette` |
| „Füge einen Würfel hinzu, der beim Anklicken springt.“ | Prefab `ColoredCube` plus Rigidbody (**Add Component**); Schnittstelle `IClickable` – die Main Camera (Click Interactor) meldet den Klick |
| „Erweitere die vorhandene Tür um ein rotes und grünes Statuslicht.“ | Events **On Opened ()** und **On Closed ()** am Door Controller von `AutomaticDoor` |
| „Erstelle einen neuen Gegenstand, der zwischen zwei Positionen schwebt.“ | Vorlage: Komponente Moving Platform (`MovingPlatformSet` mit `PointA` und `PointB`) oder Rotator |
| „Lass bei Erreichen der Zielzone ein kleines Feuerwerk aus Partikeln erscheinen.“ | Event **On Goal Reached ()** an der `GoalZone` (oder an der `TargetZone`) |

Halte die Wünsche klein. Eine Funktion, die nach 10 Minuten läuft, ist besser als eine große, die nicht fertig wird.

---

## Kleines Glossar

| Begriff | Bedeutung |
|---|---|
| Game Engine | Programm zum Bauen von Spielen (hier: Unity) |
| Scene | eine 3D-Welt, wie ein Level |
| GameObject | jedes Ding in der Scene: Baum, Lampe, Kamera … |
| Hierarchy | Liste aller GameObjects (links) |
| Inspector | zeigt die Eigenschaften des ausgewählten Objekts (rechts) |
| Transform | Position, Rotation und Scale eines Objekts |
| Komponente (Component) | ein Baustein im Inspector, der einem Objekt eine Fähigkeit gibt |
| Prefab | Vorlage, aus der man viele Kopien machen kann (blaues Symbol) |
| Material | bestimmt Farbe und Oberfläche |
| Collider | unsichtbare Hülle für Zusammenstöße |
| Trigger | Collider, durch den man hindurchkommt; er meldet nur „etwas ist drin“ |
| Rigidbody | macht ein Objekt zum Physik-Körper mit Gewicht und Schwerkraft |
| Light | Lichtquelle: Directional (Sonne), Point (Glühbirne), Spot (Taschenlampe) |
| Skript | selbst programmierte Komponente, z. B. Door Controller |
| Event | „Wenn das passiert, mach das“ – z. B. On Pressed an einem Knopf |
| Play Mode / Edit Mode | Spiel läuft / Welt wird gebaut. Änderungen im Play Mode gehen verloren. |

## Notfall-Karte

| Was ist los? | Das hilft |
|---|---|
| Etwas falsch gemacht | **Strg+Z** (rückgängig), **Strg+Y** (wiederholen) |
| Objekt nicht zu finden | Name ins Suchfeld der Hierarchy tippen, anklicken, **F** in der Scene View |
| In der Scene View verflogen | Workshop-Fenster: „Zur Weltübersicht“ |
| Kugeln und Dominos durcheinander (Play Mode) | Taste **R** oder Knopf RESET im Physiklabor, danach startet DOMINO den Hammer |
| Mauszeiger weg (Play Mode) | **Esc** |
| Steuerung vergessen (Play Mode) | Taste **H** |
| Klicks und Tasten wirken nicht (Play Mode) | Einmal ins Game-Fenster klicken |
| Prefab-Modus statt Spielwiese | Workshop-Fenster: „Zurück zur Spielwiese“ |
| Szene oder Farben ganz kaputt | Workshop-Fenster > Werkzeuge > „Ausgangsszene wiederherstellen“ (setzt auch Materialien und Prefabs zurück, legt vorher eine Sicherung an) |
