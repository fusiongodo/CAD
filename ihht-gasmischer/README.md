# IHHT-Gasmischer – Chassis & Druckteile

Stand: 15.11.2026 (v4). Alle Teile sind Onshape-FeatureScripts (je ein Feature, keine Parameter).

## Inhalt

| Datei | Was |
|---|---|
| `quelle/chassis_v4.fs` | **Aktuell.** Entwicklungs-Chassis v4 (2 NEMA, Ventilpaare über Zahnräder gekoppelt), ein Feature „IHHT Entwicklung v4“ mit allen Teilen. Änderungen siehe unten |
| `quelle/gen4.py`, `build4.py` | Generator für chassis_v4.fs: `python3 gen4.py chassis_v4.fs parts_v4.json`; Prüfung + STL + Viewer-Daten: `python3 build4.py parts_v4.json <ordner>` |
| `druck/v4_*.stl` | Rahmen, Motorbrücke, Regal, Splitter hi/lo (Lage wie im Modell); Zahnradkupplung und Zahnradnabe in Drucklage (je 2× drucken) |
| `ansicht/index.html` | 3D-Ansicht v4 im Browser (Teile ein/aus, Kamera und Motorbrücke verschieben) |
| `quelle/chassis_v3.fs` | Entwicklungs-Chassis v3 (4 NEMA), ein Feature „IHHT Entwicklung v3“ mit allen Teilen (Rahmen, Regal, Motorbrücke, Kamerasäule, Splitter v3, Kamerahalter v3, Kupplungen, Dummies). Änderungen siehe unten |
| `quelle/gen3.py` | Generator für chassis_v3.fs: `python3 gen3.py chassis_v3.fs parts_v3.json` (Splitter-v3- und Kamerahalter-v3-Patch sind eingearbeitet) |
| `druck/v3_*.stl` | Rahmen, Motorbrücke, Kamerasäule, Regal v3 (aus dem Generator, Lage wie im Modell) |
| `quelle/chassis_v2.fs` | Entwicklungs-Chassis v2 (Rotameter mit Drehknopf unten): Rahmen 1 mm Wand / 1 mm Boden + Randsteg, Regal, Motorbrücke 1 mm mit Distanzringen, alte Kupplungen, Kamerasäule, alte Splitter, Dummies; Feature „IHHT Ablage“ mit den v1-Teilen |
| `quelle/splitter_v3.fs` | Splitter hi + lo v3 (ersetzt die beiden unteren Splitter aus chassis_v2) |
| `quelle/kamerahalter_v3.fs` | Kamerahalter v3 (ersetzt den Halter aus chassis_v2) |
| `quelle/kupplung_v1.fs` | Steckkupplung NEMA 17 ↔ LZQ, 4 Stück |
| `quelle/gen.py`, `gen2.py`, `mkgeom.py` | Generator (Python) für chassis_v2.fs: `python3 gen2.py chassis_v2.fs parts.json` |
| `quelle/splitter_v3.patch`, `kamerahalter_v3.patch` | Änderungen gegenüber gen2.py für Splitter v3 / Kamerahalter v3 |
| `quelle/kupplung_v1_3mf.py` | baut die Kupplungs-3MF (Bambu-Vorlage nötig) |
| `druck/` | 3MF-Dateien (Bambu Studio, Einstellungen eingebettet) |

## Chassis v4 – Änderungen gegenüber v3

Ein NEMA steuert jetzt zwei LZQ: Zufuhr direkt, Ablass über ein Zahnradpaar gegenläufig. Alles andere (Kamera, Höhen, Schraubstellen, Verstellwege) bleibt wie v3.

- **Anordnung (x):** Ablass hi −44,65 · Zufuhr hi −22,65 · Zufuhr lo +22,65 · Ablass lo +44,65. Die Zufuhr-LZQ und die beiden NEMAs bleiben auf ±22,65 (45,3 mm Abstand wie v3). Die Ablass-LZQ rücken auf **22 mm Achsabstand** an ihre Zufuhr (2 mm Luft zwischen den Körpern).
- **Motor auf der Zufuhr, Zahnrad auf dem Ablass:** Das Flankenspiel liegt damit auf dem Ablass, nicht auf dem Ventil, das die Mischung einstellt.
- **Zahnräder:** Stirnräder 1:1, direkt kämmend (kein Zwischenrad), 16 Zähne, Modul 1,375, Eingriffswinkel 20°, Kopfkreis Ø 24,75, 14 mm breit, Flankenspiel 0,15 mm. Beide Ventile haben Rechtsgewinde → eins schließt, während das andere öffnet.
  - **Zahnradkupplung** (2×): Zahnrad + Kupplung in einem Teil. NEMA-Bohrung Ø 5,32 / Fläche 2,22 (14 mm tief), LZQ-Bohrung Ø 4,50 / Fläche 1,06 – Passmaße aus Kupplung v1.
  - **Zahnradnabe** (2×): sitzt auf der Ablass-Spindel (gleiche LZQ-Passung) und läuft hinten auf einem **Lagerzapfen Ø 6 × 12 mm** der Motorbrücke. Zapfenbohrung Nennmaß Ø 7,0 (Schätzwert für Laufsitz, noch nicht gedruckt).
  - Beide stehend drucken, Zahnrad unten (Brückenseite auf dem Bett), ohne Support.
- **Motorbrücke v4:** 2 Motor-Lochbilder statt 4, dazu 2 Lagerzapfen bei x ±44,65. Fuß, Langloch und Verstellweg wie v3.
- **Rahmen v4:** 134 mm breit (v3: 188), Länge 253 mm und alle Höhen unverändert. Kamera-Schienen unverändert.
- **Splitter hi/lo v4:** Innentüllen 22 mm statt 45,3 mm auseinander, sonst wie v3 (gleiche Tüllen, gleiche Höhen).
- **Regal v4:** schmaler, trägt nur noch den Sammler Mix.
- **Ablass-Adapter entfallen:** Bei 22 mm Achsabstand kollidieren sie mit dem Sammler Mix. Die obere Tülle der Ablass-LZQ bläst frei ab; bei Bedarf Schlauch direkt aufstecken.
- **Neu drucken:** Rahmen, Motorbrücke, Regal, Splitter hi + lo, 2 Zahnradkupplungen, 2 Zahnradnaben. **Weiterverwendbar:** Sammler Mix, Kamerasäule v3, Kamerahalter v3.
- **Zusammenbau:** Zufuhr ganz zu und Ablass ca. 80 % auf (oder umgekehrt), dann Naben so aufstecken, dass die Zähne greifen. Die LZQ haben in den Ø-11-Wandlöchern ±0,75 mm Spiel – damit das Zahnspiel einstellen, bevor die Muttern festgezogen werden.
- **Kamera:** Die äußeren Rohre liegen jetzt bei ±44,65 statt ±67,95 → bei 90 mm Abstand reichen 63° horizontaler Bildwinkel (v3: 89°). Die Zahnräder (Oberkante z 47,4) verdecken bei 90 mm Abstand nichts vom Kugelbereich; bei 142 mm liegt die Sichtgrenze wie in v3 bei ca. z 34 (dort begrenzt ohnehin die Motorbrücke).
- Geprüft im Modell (Python/Manifold): keine Überschneidungen, Zahnflanken 0,07 mm Luft je Seite, Zapfen 0,5 mm radial. Das FeatureScript selbst ist noch nicht in Onshape gelaufen (neu darin: `prismOp` für das Zahnradprofil).

## Chassis v3 – Änderungen gegenüber v2

- **Alles 10 mm höher** (`LIFT`): Rotameter, Motoren, Kupplungsachse, Splitter, Regal, Kamera. NEMA-Unterkante 12,9 mm über dem Boden → 6-Pin-Stecker zeigt nach unten (Motor um 180° gedreht montieren), 10 mm für Stecker + Kabelbogen.
- **Neu drucken:** Rahmen, Motorbrücke, Kamerasäule. **Unverändert weiterverwendbar:** Splitter v3, Sammler, Ablässe, Kamerahalter v3, Kupplungen. Regal: altes passt weiter, neues hat Schraubdome nach unten.
- **Schraubstellen für Spax 3×12** (Vorbohr-Ø 2,6 wie bisher, alles Sacklöcher, nichts sticht mehr durch den Boden):
  - Motorbrücke: Schiene am Rahmen 9 mm hoch, Loch 7,5 mm tief, Schraubenspitze 2,5 mm über dem Tisch
  - Kamera- und Splitter-Schienen am Rahmen: 17 mm hoch, Loch 10,5 mm tief
  - Regal-Säulen: Loch 12,5 mm tief; Splitter-Schienen auf dem Regal: Dome Ø 8 nach unten, Loch 10,5 mm tief
  - NEMA-Verschraubung (M3 in der Brückenplatte) unverändert
- **Brückenfuß (Langloch-Steg):** 12,6 × 5,5 mm statt 8 × 4 mm, 4,6 mm Material je Seite des Langlochs (v2: 2,3), 4,3 mm an beiden Langloch-Enden (v2: 1,3), dazu Außenwange 12 mm hoch als L-Profil. Verstellweg ±10 mm bleibt.
- **Kamera:** Säule/Schiene 10 mm höher. Abstand Linse–Rotameter jetzt 90–142 mm (v2: bis 112 mm), in jeder Stellung mindestens 2 Schrauben je Seite. Fuß 46 mm lang mit 38-mm-Langloch, Schienenlöcher im 8-mm-Raster (y −152 … −104). Ganz hinten ragt der Fuß 34 mm über die Rahmenvorderkante.
- **Rahmen 253 mm lang** (vorne +10 mm) – passt gerade noch aufs P1S-Bett (256), ohne Brim drucken.
- Geprüft im Modell (Python/Manifold): keine Überschneidungen in Nennlage, Kamera in beiden Endlagen frei, Boden unter allen Schraublöchern geschlossen. Das FeatureScript selbst ist noch nicht in Onshape gelaufen.

## Festgelegte Maße (aus Testdrucken)

- **Kupplung NEMA-Seite:** Bohrung Ø 5,32, Abflachung 2,22 mm von der Achse
- **Kupplung LZQ-Seite:** Bohrung Ø 4,50, Abflachung 1,06 mm von der Achse; Außen-Ø 9, 0,28-mm-Schichten, stehend, NEMA-Seite unten
- **Innentüllen Splitter → Rotameter:** Wellenscheitel Ø 11,25 (Kern 10,75), Bohrung Ø 6,5 Tropfenprofil
- **Splitter-Kanal:** 8 mm, volles Tropfenprofil (druckt ohne Support); Lo-Außentülle auf z 30 (+4 mm)
- **Kamerahalter:** Platte 1 mm, Hälse Ø 10, M2-Loch nominal Ø 3,0 durchgehend (gedruckt ≈ 2,2–2,4), Mutter hinten
- **Kleine Löcher schrumpfen im Druck stark** (≈ 0,3–0,8 mm) → Maße oben sind Nennmaße fürs CAD

## Offen

- v4: Zahnradpaar und Lagerzapfen-Passung (Ø 6 in Ø 7,0) im Druck prüfen; Spindelhub der LZQ über den Stellweg messen (Zahnräder vertragen ±5 mm Versatz)

- Prüfhülsen-Raster (32 Stück) zur Feinabstimmung der Kupplung – Ergebnis steht aus
- Kammer hinter der Lo-Außentülle (Ø 20,4) braucht noch Support → ggf. Tropfendach
