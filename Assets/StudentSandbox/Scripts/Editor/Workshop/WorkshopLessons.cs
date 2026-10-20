namespace StudentSandbox.EditorTools
{
    /// <summary>Ein Knopf "Zeig mir: …" – wählt ein Objekt in der Szene aus und zeigt es in der Scene View.</summary>
    public sealed class ShowMeTarget
    {
        /// <summary>Text auf dem Knopf (nach "Zeig mir: ").</summary>
        public readonly string Label;
        /// <summary>Genauer Name des GameObjects in der Hierarchy.</summary>
        public readonly string ObjectName;
        /// <summary>false = nur auswählen, die Scene View bleibt, wo sie ist (z.B. für die Sonne hoch am Himmel).</summary>
        public readonly bool Frame;

        public ShowMeTarget(string label, string objectName, bool frame = true)
        {
            Label = label;
            ObjectName = objectName;
            Frame = frame;
        }
    }

    /// <summary>Eine Lektion im Workshop-Fenster.</summary>
    public sealed class WorkshopLesson
    {
        public string Title;
        /// <summary>Name des Aussichtspunkts (siehe <see cref="CameraSetup"/>), zu dem "Zum Bereich" springt.</summary>
        public string Area;
        public string Explanation;
        public string Goal;
        public string[] Steps;
        public string Challenge;
        public string AdultTip;
        public ShowMeTarget[] ShowMe;
    }

    /// <summary>
    /// Inhalt der sechs Lektionen. Jeder Schritt dauert höchstens ein bis zwei Minuten.
    /// Alle Objektnamen gibt es wirklich in der erzeugten Szene.
    /// </summary>
    public static class WorkshopLessons
    {
        /// <summary>EditorPrefs-Schlüssel für das Häkchen eines Schritts (Lektion und Schritt beginnen bei 1).</summary>
        public static string StepKey(int lessonNumber, int stepNumber)
        {
            return "StudentSandbox.Lesson" + lessonNumber + ".Step" + stepNumber;
        }

        public static readonly WorkshopLesson[] All =
        {
            // ------------------------------------------------------------------ 1
            new WorkshopLesson
            {
                Title = "1. Objekte bewegen",
                Area = CameraSetup.CreativeName,
                Explanation =
                    "Eine Scene ist deine 3D-Welt. Alles darin ist ein GameObject: ein Baum, ein Haus, ein Licht, sogar die Kamera.\n" +
                    "Links in der Hierarchy siehst du alle GameObjects als Liste. Rechts im Inspector siehst du, was das ausgewählte Objekt kann.\n" +
                    "Ganz oben im Inspector steht immer der Transform: Position (wo steht es?), Rotation (wie ist es gedreht?) und Scale (wie groß ist es?).",
                Goal = "Du kannst dich in der Scene View bewegen und Objekte verschieben, drehen und größer oder kleiner machen.",
                Steps = new[]
                {
                    "Klicke oben auf „Zum Bereich“. Die Scene View zeigt jetzt den Baubereich.",
                    "Schau dich um: Halte die rechte Maustaste gedrückt und fliege mit W A S D. Mit dem Mausrad zoomst du.",
                    "Klicke auf „Zeig mir: Baum“. Jetzt ist „Tree_Small_1“ ausgewählt. Im Inspector siehst du seinen Transform.",
                    "Drücke W (Verschieben) und zieh an einem der bunten Pfeile. Schau zu, wie sich die Zahlen bei Position ändern.",
                    "Drücke E (Drehen) und dreh den Baum an den farbigen Ringen.",
                    "Drücke R (Skalieren) und zieh am grauen Würfel in der Mitte: Der Baum wird größer oder kleiner.",
                    "Drücke Strg+Z – der letzte Schritt wird rückgängig gemacht. Probier alles noch einmal mit dem Haus oder der Bank.",
                },
                Challenge = "Mach den Baum genau doppelt so groß (Scale X, Y und Z = 2) und stell ihn direkt neben das rote Haus.",
                AdultTip =
                    "Erkläre kurz Hierarchy, Scene View und Inspector (höchstens 2 Minuten) und zeig einmal das Fliegen mit rechter Maustaste + WASD. " +
                    "Dann übernimmt sie die Maus – ab jetzt nur noch Tipps geben, nicht selbst klicken.",
                ShowMe = new[]
                {
                    new ShowMeTarget("Baum", "Tree_Small_1"),
                    new ShowMeTarget("Haus", "House_Red"),
                    new ShowMeTarget("Bank", "Bench_Pond_1"),
                },
            },

            // ------------------------------------------------------------------ 2
            new WorkshopLesson
            {
                Title = "2. Eigene Welt gestalten",
                Area = CameraSetup.CreativeName,
                Explanation =
                    "Ein Prefab ist eine Vorlage – wie ein Stempel. In der Hierarchy haben Prefabs ein blaues Würfel-Symbol.\n" +
                    "Im Project-Fenster findest du die Prefabs im Ordner Assets/StudentSandbox/Prefabs/Creative (zum Beispiel SmallTree, PineTree, Bench, Flower).\n" +
                    "Ein Material bestimmt Farbe und Oberfläche. Fertige Farben liegen im Ordner Assets/StudentSandbox/Materials/Palette.",
                Goal = "Du kannst Prefabs in die Welt ziehen, Objekte kopieren und ihnen eine neue Farbe geben.",
                Steps = new[]
                {
                    "A1: Öffne im Project-Fenster Assets/StudentSandbox/Prefabs/Creative und zieh 3 Bäume (SmallTree oder PineTree) in die Scene View.",
                    "A1: Verschieb das rote Haus „House_Red“ an einen neuen Platz (Taste W).",
                    "A1: Mach einen Baum größer (Taste R).",
                    "A1: Dreh die Bank „Bench_Pond_1“ in eine andere Richtung (Taste E).",
                    "A1: Wähle die Bank aus und drücke Strg+D. Schieb die Kopie ein Stück zur Seite.",
                    "A1: Zieh ein Material aus Materials/Palette auf ein Objekt in der Scene View, zum Beispiel auf ein Dach.",
                    "A1: Bau eine eigene kleine Deko, zum Beispiel ein paar Blumen (Flower) oder einen Zaun (Fence).",
                    "A2: Klicke auf „Zeig mir: Beispiel-Turm“ und schau dir den „ExampleTower“ im BAUPLATZ an.",
                    "A2: Bau im BAUPLATZ einen ungewöhnlichen Turm aus Würfeln, Zylindern und Kugeln (ColoredCube, ColoredCylinder, ColoredSphere).",
                },
                Challenge =
                    "Bau den verrücktesten Turm, der trotzdem gut aussieht – mit mindestens 3 Farben. " +
                    "Gib ihm in der Hierarchy einen eigenen Namen (Objekt auswählen und F2 drücken).",
                AdultTip =
                    "Zeig einmal, wie man ein Prefab aus dem Project-Fenster in die Szene zieht und ein Material auf ein Objekt fallen lässt. " +
                    "Danach übernimmt sie die Maus und entscheidet selbst, wie ihre Welt aussieht.",
                ShowMe = new[]
                {
                    new ShowMeTarget("Bauplatz", "BuildArea"),
                    new ShowMeTarget("Beispiel-Turm", "ExampleTower"),
                    new ShowMeTarget("Farbkasten", "ColorPalette"),
                },
            },

            // ------------------------------------------------------------------ 3
            new WorkshopLesson
            {
                Title = "3. Physik ausprobieren",
                Area = CameraSetup.PhysicsName,
                Explanation =
                    "Physik heißt: Unity rechnet aus, wie Dinge fallen, rollen und zusammenstoßen. Das nennt man Simulation.\n" +
                    "Ein Rigidbody macht ein Objekt „echt“: Es hat ein Gewicht und die Schwerkraft zieht es nach unten.\n" +
                    "Ein Collider ist eine unsichtbare Hülle. Damit berührt das Objekt andere Objekte.\n" +
                    "Die Simulation läuft nur im Play Mode. Wichtig: Was du im Play Mode änderst, ist nach dem Beenden wieder weg!",
                Goal = "Du weißt, was Rigidbody, Collider und Schwerkraft sind, und kannst ein Physik-Experiment verändern.",
                Steps = new[]
                {
                    "Klicke auf „Zeig mir: Kugel“. Im Inspector siehst du: Die PhysicsBall hat einen Rigidbody und einen Sphere Collider.",
                    "Drücke Play und schau zu: Die Kugel rollt die Rampe hinunter ins Ziel. Auf dem Bildschirm erscheint „Ziel erreicht!“. " +
                    "(Die Kamera startet dort, wo deine Scene View hinschaut. Siehst du das Labor nicht? Klick ins Game-Fenster und drück die Taste 3.)",
                    "Beende den Play Mode wieder (noch einmal auf Play drücken).",
                    "Klicke auf „Zeig mir: Rampe“ und ändere bei „Ramp_Main“ die Rotation Z, zum Beispiel auf -30 (steiler) oder -15 (flacher).",
                    "Drücke wieder Play. Rollt die Kugel jetzt schneller oder langsamer? Mit der Taste R setzt du alles zurück.",
                    "Beende Play. Verschieb im „ObstacleCourse“ die Hindernisse, bis die blaue Kugel „PhysicsBall_Blue“ in die „TargetZone“ rollt. Teste mit Play.",
                    "Drücke Play und schau den Dominosteinen zu: Kurz nach dem Start stößt der „Hammer“ sie an. " +
                    "Danach: Taste R (alles zurück) und dann der DOMINO-Knopf – der Hammer schwingt noch einmal.",
                },
                Challenge =
                    "Finde den steilsten Winkel, bei dem die rote Kugel trotzdem im Ziel landet. " +
                    "Oder: Bau mit WallBlock und Bumper einen ganz neuen Weg für die blaue Kugel.",
                AdultTip =
                    "Erkläre in einem Satz: Rigidbody = Gewicht und Schwerkraft, Collider = Hülle zum Anstoßen. " +
                    "Dann übernimmt sie die Maus. Lass sie vor jedem Play raten: „Was glaubst du, passiert jetzt?“",
                ShowMe = new[]
                {
                    new ShowMeTarget("Kugel", "PhysicsBall"),
                    new ShowMeTarget("Rampe", "Ramp_Main"),
                    new ShowMeTarget("Ziel", "GoalZone"),
                    new ShowMeTarget("Hammer", "Hammer"),
                    new ShowMeTarget("Blaue Kugel", "PhysicsBall_Blue"),
                },
            },

            // ------------------------------------------------------------------ 4
            new WorkshopLesson
            {
                Title = "4. Licht und Schatten",
                Area = CameraSetup.LightName,
                Explanation =
                    "Ohne Licht wäre alles schwarz. Es gibt drei wichtige Lichtarten:\n" +
                    "Directional Light – wie die Sonne: Es leuchtet überall gleich, nur seine Drehung zählt.\n" +
                    "Point Light – wie eine Glühbirne: Es leuchtet in alle Richtungen.\n" +
                    "Spot Light – wie eine Taschenlampe: Es leuchtet in einem Kegel.\n" +
                    "Wichtige Einstellungen an der Light-Komponente: Color (Farbe), Intensity (Helligkeit) und Shadows (Schatten).",
                Goal = "Du kannst Lichter auswählen und ihre Farbe, Helligkeit und Richtung ändern – und siehst, wie Schatten entstehen.",
                Steps = new[]
                {
                    "Klicke auf „Zeig mir: Tageszeit“. Klicke im Inspector auf TAG, SONNENUNTERGANG und NACHT (oder unten bei Werkzeuge).",
                    "Stell NACHT ein. Klicke auf „Zeig mir: Rotes Licht“ und ändere bei der Light-Komponente die Color.",
                    "Wähle „ColorLight_Blue“ und verschieb es mit W. Wo sich zwei Lichtfarben mischen, entsteht eine neue Farbe!",
                    "Wähle „ShadowSpotLight“ und dreh es mit E. Schau, wie der Schatten der Figur „ShadowSculpture“ wandert.",
                    "Ändere beim ShadowSpotLight Intensity und den Lichtkegel (im Inspector: Inner / Outer Spot Angle). Was passiert?",
                    "Stell wieder TAG ein. Wähle die Sonne „Sun“ und ändere Rotation X: Die Schatten werden länger oder kürzer. " +
                    "(Klickst du später TAG, ABEND oder NACHT, stellt sich die Sonne wieder auf die Tageszeit ein.)",
                },
                Challenge =
                    "Mach eine Disco-Stimmung: Stell NACHT ein und gib den drei Farblichtern ganz neue Farben. " +
                    "Oder gib einer Laterne („StreetLamp_1“ → Kind-Objekt LampLight) deine Lieblingsfarbe.",
                AdultTip =
                    "Erkläre kurz Sonne (Directional), Glühbirne (Point) und Taschenlampe (Spot), dann übernimmt sie die Maus. " +
                    "Licht bringt die schnellsten Wow-Momente – lass ihr hier ruhig Zeit zum Ausprobieren.",
                ShowMe = new[]
                {
                    new ShowMeTarget("Sonne", "Sun", false),
                    new ShowMeTarget("Tageszeit", "DayNightController"),
                    new ShowMeTarget("Rotes Licht", "ColorLight_Red"),
                    new ShowMeTarget("Blaues Licht", "ColorLight_Blue"),
                    new ShowMeTarget("Spot-Lampe", "ShadowSpotLight"),
                    new ShowMeTarget("Laterne", "StreetLamp_1"),
                },
            },

            // ------------------------------------------------------------------ 5
            new WorkshopLesson
            {
                Title = "5. Interaktionen testen",
                Area = CameraSetup.InteractionName,
                Explanation =
                    "Ein GameObject bekommt seine Fähigkeiten durch Components – das sind die Bausteine im Inspector.\n" +
                    "Ein Script ist eine Component, die jemand programmiert hat, zum Beispiel der DoorController, der eine Tür bewegt.\n" +
                    "Ein Event verbindet zwei Dinge: Wenn du auf einen Schalter klickst (eine Benutzer-Aktion), ruft der Schalter eine Funktion bei einem anderen Objekt auf.",
                Goal = "Du verstehst, wie ein Schalter über ein Event etwas auslöst, und kannst das Verhalten im Inspector ändern.",
                Steps = new[]
                {
                    "Klicke auf „Zeig mir: Tür“. Schau dir im Inspector die Component Door Controller an.",
                    "Klicke auf „Zeig mir: Schalter“. Unter „On Pressed ()“ steht: AutomaticDoor → DoorController.Toggle. Das ist das Event.",
                    "Drücke Play und klicke im Game-Fenster auf den Schalter. Die Tür geht auf – noch ein Klick, und sie geht zu. " +
                    "(Tipp: Taste 5 fliegt zum Interaktionsbereich.)",
                    "Beende Play. Stell bei der Tür Open Speed auf 0.2 und teste noch einmal. Jetzt ist sie ganz langsam.",
                    "Drücke Play und klicke auf den Ampel-Knopf („PedestrianButton“ an der Ampel oder AMPEL am Kontrollpult). Die Ampel schaltet weiter.",
                    "Beende Play. Wähle „Crystal“ und ändere beim Rotator die Speed von 45 auf 90. Teste mit Play: doppelt so schnell!",
                    "Starte und stoppe im Play Mode die Plattform mit dem PLATTFORM-Knopf am Kontrollpult.",
                    "Beende Play. Verschieb „PointB“ der Plattform und drücke Play: Die Plattform fährt eine neue Strecke.",
                },
                Challenge =
                    "Gib einem Knopf eine neue Aufgabe: Wähle „Desk_CrystalButton“, klicke unter On Pressed auf +, " +
                    "zieh die Tür „AutomaticDoor“ in das neue Feld und wähle DoorController → Toggle. Teste mit Play!",
                AdultTip =
                    "Erkläre das Prinzip „Klick → Event → Funktion“ einmal am Türschalter und zeig, wo das Event im Inspector steht. " +
                    "Dann übernimmt sie die Maus und probiert die anderen Knöpfe selbst aus.",
                ShowMe = new[]
                {
                    new ShowMeTarget("Tür", "AutomaticDoor"),
                    new ShowMeTarget("Schalter", "DoorSwitch"),
                    new ShowMeTarget("Ampel", "TrafficLight"),
                    new ShowMeTarget("Kristall", "Crystal"),
                    new ShowMeTarget("Plattform", "MovingPlatform"),
                    new ShowMeTarget("Punkt B", "PointB"),
                    new ShowMeTarget("Kontrollpult", "ControlDesk"),
                },
            },

            // ------------------------------------------------------------------ 6
            new WorkshopLesson
            {
                Title = "6. Eigene Herausforderung",
                Area = CameraSetup.OverviewName,
                Explanation =
                    "Jetzt bist du die Spieleentwicklerin! Gestalte einen kleinen Abenteuerpark mit allem, was du heute gelernt hast.\n" +
                    "Es gibt kein Richtig oder Falsch. Wichtig ist nur, dass dir dein Park gefällt.",
                Goal = "Du planst eine eigene Idee, baust sie und stellst sie vor.",
                Steps = new[]
                {
                    "Überleg dir eine Idee: Was soll es in deinem Abenteuerpark geben? Such dir einen freien Platz dafür.",
                    "Platziere mindestens 3 Objekte (Prefabs aus dem Project-Fenster).",
                    "Ändere bei mindestens einem Objekt die Farbe (Material aus Materials/Palette).",
                    "Probier eine Physik-Reaktion: Gib einem ColoredCube mit „Add Component“ einen Rigidbody, stell ihn in die Luft und drücke Play.",
                    "Passe ein Licht an, zum Beispiel die Farbe einer Laterne (LampPost → LampLight → Color).",
                    "Starte eine Interaktion: Drücke Play und klicke auf einen Knopf, der etwas bewegt.",
                    "Speichere deine Szene mit Strg+S und leg unten bei Werkzeuge eine Sicherung an.",
                    "Stell deinen Park deinem Bruder vor: Was hast du gebaut? Was war am schwierigsten?",
                },
                Challenge =
                    "Gib deinem Park einen Namen: Rechtsklick in die Hierarchy → Create Empty, nenne das neue Objekt wie deinen Park " +
                    "und zieh alle deine Objekte hinein.",
                AdultTip =
                    "Lass sie entscheiden und halte dich zurück. Frag lieber „Was soll passieren?“ statt Lösungen vorzugeben – sie hat die Maus. " +
                    "Wenn noch Zeit ist: Zeig zum Schluss als Ausblick, wie eine KI ein kleines Skript erklären oder schreiben kann.",
                ShowMe = new[]
                {
                    new ShowMeTarget("Bauplatz", "BuildArea"),
                    new ShowMeTarget("Farbkasten", "ColorPalette"),
                },
            },
        };
    }
}
