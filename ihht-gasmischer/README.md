# IHHT-Gasmischer – Chassis & Druckteile

Stand: 15.11.2026 (v5). Alle Teile sind Onshape-FeatureScripts (je ein Feature, keine Parameter).

## Inhalt

| Datei | Was |
|---|---|
| `quelle/chassis_v5.fs` | **Aktuell.** Entwicklungs-Chassis v5 (2 NEMA, Zahnradpaare, Kameraportal, Rahmen 250 mm), ein Feature „IHHT Entwicklung v5“ mit allen Teilen. Änderungen siehe unten |
| `quelle/gen5.py`, `build5.py` | Generator für chassis_v5.fs: `python3 gen5.py chassis_v5.fs parts_v5.json`; Prüfung + STL + Viewer-Daten: `python3 build5.py parts_v5.json <ordner>` |
| `druck/v5_*.stl` | Rahmen, Motorbrücke, Regal (Lage wie im Modell); Kameraportal, Kamerahalter, Zahnradkupplung, Zahnradnabe in Drucklage (Zahnradteile je 2×) |
| `druck/v4_splitter_*.stl` | Splitter hi/lo mit 22 mm Tüllenabstand – in v5 unverändert, nur 13 mm näher an der Wand montiert |
| `ansicht/index.html` | 3D-Ansicht v5 im Browser (Teile ein/aus; Kamera-Abstand, Kamera-Höhe, Motorbrücke verschieben; Sichtlinien) |
| `quelle/chassis_v4.fs`, `gen4.py`, `build4.py` | Entwicklungs-Chassis v4 (erste Fassung mit 2 NEMA und Zahnradpaaren; Kamerasäule v3, Rahmen 253 mm) |
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
| `quelle/zahnrad_v5_3mf.py` | baut `druck/zahnrad_v5.3mf` (2 Zahnradkupplungen + 2 Zahnradnaben, 0,20 mm) aus den v5-STLs |
| `quelle/passungsraster_3mf.py` | baut das Passungs-Raster für die D-Bohrungen als 3MF. Die gedruckte Fassung enthielt wegen doppelter Objekt-IDs zweimal die NEMA-Platte (Skript inzwischen korrigiert) |
| `quelle/druck2_3mf.py`, `druck/druck2_2026-11-17.3mf` | Druck 2: LZQ-Platte A+B (Ø 4,14–4,34 × SW 3,08–3,20), 18 NEMA- und 18 LZQ-Steg-Zylinder |
| `passungen.html` | **Messungs-Timeline** aller Passungstests (NEMA/LZQ), neue Einträge oben |
| `quelle/nachtdruck_3mf.py`, `druck/nachtdruck_2026-11-16.3mf`, `ansicht/sockel_kennung.png` | Nachtdruck 16./17.11.: LZQ-Raster A+B, NEMA-Platte A+B, 27 Steg-Zylinder; Lesehilfe Sockel |
| `quelle/passungsraster_lzq_3mf.py`, `druck/passungsraster_lzq.3mf`, `ansicht/passungsraster_lzq.png` | LZQ-Raster 7 × 7, 6 mm dick, 0,20-mm-Schichten; Draufsicht mit Beschriftung |
| `druck/` | 3MF-Dateien (Bambu Studio, Einstellungen eingebettet) |

## Chassis v5 – Änderungen gegenüber v4

- **Hinten kürzer:** Zwischen Tüllenspitze LZQ und Tüllenspitze Splitter/Sammler bleiben 10 mm (v4: 23 mm unten, 25 mm oben). Splitter rücken 13 mm, Sammler Mix 15 mm zur Wand; Regal y 27…79. Die Teile selbst sind unverändert.
- **Rahmen 250 mm lang** (y −168…82): hinten 13 mm kürzer, vorne 10 mm länger für die Kameraschienen. Breite 134 mm wie v4.
- **NEMAs 24 mm weiter weg von den LZQ** (Motor-Vorderseite 65 mm vor der LZQ-Front). Zahnradkupplung und Zahnradnabe sind dafür **48 mm lang** (v4: 24): Zahnrad direkt am Motor, dann Nabe Ø 11, vorne Ø 9. Die LZQ-Bohrung reicht 12 mm über die Spindelspitze hinaus (Platz für den Spindelhub). Lagerzapfen der Brücke 16 mm lang.
- **Kamera:** sitzt auf z 83 = Mitte zwischen unterer (z 35) und oberer (z 131) Tüllenachse; der Halter lässt sich ±12 mm (10 % der LZQ-Höhe) in der Höhe verstellen.
- **Kameraportal** statt Kamerasäule: flache 3-mm-Platte mit zwei Beinen außen (x ±49…61) und Querträger über den Motoren, Füße 34 mm nach vorn neben den NEMAs. Schienen bei x ±55, Löcher im 8-mm-Raster (y −162…−98), in jeder Stellung mindestens 2 Schrauben je Seite. Abstand Linse–LZQ-Front **84…146 mm** (Nennlage 100).
- **Kamerahalter v5:** nur noch Platte 50 × 25 mm mit den vier Hälsen für die Pi-Cam (M2 durch + Mutter wie v3), 2 × M3 in den Höhen-Langlöchern. Lampenhalter entfällt.
- **Sicht bis zur Spindelachse** (Punkt, an dem die Spindel in den LZQ-Körper eintritt, z 35): Bei Kamerahöhe Mitte frei über den ganzen Abstandsbereich (Grenze 148 mm, dann verdeckt die NEMA-Oberkante). In der tiefsten Stellung (−12 mm) frei bis 111 mm, in der höchsten überall.
- **Bildwinkel**, den die Kamera braucht (alle vier Rohre, ganzer Kugelbereich): bei 84 mm ca. 66° × 65°, bei 100 mm 57° × 56°, bei 146 mm 41° × 40°.
- **Motorbrücke:** Verstellweg −5…+2 mm (die Kameraschienen enden 6 mm vor der Brückenplatte).
- **Neu drucken gegenüber v4:** Rahmen, Regal, Motorbrücke, 2 Zahnradkupplungen, 2 Zahnradnaben, Kameraportal, Kamerahalter. **Unverändert:** Splitter hi/lo v4, Sammler Mix. Kamerasäule v3 und Kamerahalter v3 passen nicht mehr.
- **Drucklage:** Kameraportal mit der Rückseite aufs Bett (Füße und Knotenbleche stehen nach oben), Kamerahalter flach, Zahnradteile stehend mit dem Zahnrad unten – alles ohne Support.
- Geprüft im Modell (Python/Manifold): keine Überschneidungen in Nennlage sowie mit Kamera ganz vorn/hinten, Höhe ±12 mm und Brücke −5/+2 mm. Das FeatureScript ist noch nicht in Onshape gelaufen (neu: `prismOp`, `prismXOp`).

## Chassis v4 – Änderungen gegenüber v3

Ein NEMA steuert jetzt zwei LZQ: Zufuhr direkt, Ablass über ein Zahnradpaar gegenläufig. Alles andere (Kamera, Höhen, Schraubstellen, Verstellwege) bleibt wie v3.

- **Anordnung (x):** Ablass hi −44,65 · Zufuhr hi −22,65 · Zufuhr lo +22,65 · Ablass lo +44,65. Die Zufuhr-LZQ und die beiden NEMAs bleiben auf ±22,65 (45,3 mm Abstand wie v3). Die Ablass-LZQ rücken auf **22 mm Achsabstand** an ihre Zufuhr (2 mm Luft zwischen den Körpern).
- **Motor auf der Zufuhr, Zahnrad auf dem Ablass:** Das Flankenspiel liegt damit auf dem Ablass, nicht auf dem Ventil, das die Mischung einstellt.
- **Zahnräder:** Stirnräder 1:1, direkt kämmend (kein Zwischenrad), 16 Zähne, Modul 1,375, Eingriffswinkel 20°, Kopfkreis Ø 24,75, 14 mm breit, Flankenspiel 0,15 mm. Beide Ventile haben Rechtsgewinde → eins schließt, während das andere öffnet.
  - **Zahnradkupplung** (2×): Zahnrad + Kupplung in einem Teil. NEMA-Bohrung Ø 5,41 / Abflachung 2,18 (14 mm tief) mit Quetschrillen 0,15 / 0,4, LZQ-Bohrung Ø 4,49 / Abflachung 1,15 mit Quetschrillen 0,15 / 0,4, LZQ-Abflachung 6 mm lang (Stand 19.11., zweiter Versuch; der Druck vom 18.11. mit Ø 5,26 / 2,10 und Ø 4,34 / 1,07 ging nicht auf die LZQ). Druckdatei mit 2 Kupplungen + 2 Naben: `druck/zahnrad_v5.3mf` (0,20 mm, Pflicht für diese Passmaße).
  - **Zahnradnabe** (2×): sitzt auf der Ablass-Spindel (gleiche LZQ-Passung mit Quetschrillen) und läuft hinten auf einem **Lagerzapfen Ø 6 × 12 mm** der Motorbrücke. Zapfenbohrung Nennmaß Ø 7,0 (Schätzwert für Laufsitz, noch nicht gedruckt).
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

- **NEMA-Seite (0,20-mm-Schichten), aktueller Stand:** glatte Bohrung **Ø 5,26 / Abflachung 1,95**; mit Opferstegen Ø 5,26 / 2,10 + Stege 0,15 tief (Abstand 2, Höhe 0,4–0,6). Alle Messungen: `passungen.html`
- **Faustregel Bohrungen (P1S, PLA, 0,4-Düse, 0,20 mm, stehend):** Loch im CAD ≈ gemessener Wellen-Ø + 0,18–0,2; Opferstege erst ab 0,15 mm Tiefe druckbar
- **Zahnradkupplung/-nabe v5 (Stand 19.11.):** NEMA Ø 5,41 / 2,18 + Quetschrillen 0,15 / 0,4; LZQ Ø 4,49 / 1,15 + Quetschrillen 0,15 / 0,4; Rillen alle 2 mm. Druck vom 18.11. (Ø 5,26 / 2,10 bzw. 4,34 / 1,07, Rillen 0,20 bzw. 0,15) ging nicht auf die LZQ (Quelle: ihht-pi `CAD/passungen/`)
- **LZQ-Seite:** glatte D-Bohrung ohne Rillen noch nicht ermittelt. Alt: Ø 4,50 / 1,06 – zu viel Spiel. Spindel gemessen ca. Ø 4, über die Abflachung ca. 3 mm (4 gleiche LZQ; ein fünftes mit dickerer Spindel wird nicht verwendet)
- **Kupplung allgemein:** Außen-Ø 9, stehend drucken, NEMA-Seite unten; Passmaße gelten nur für die Schichthöhe, mit der getestet wurde
- **Innentüllen Splitter → Rotameter:** Wellenscheitel Ø 11,25 (Kern 10,75), Bohrung Ø 6,5 Tropfenprofil
- **Splitter-Kanal:** 8 mm, volles Tropfenprofil (druckt ohne Support); Lo-Außentülle auf z 30 (+4 mm)
- **Kamerahalter:** Platte 1 mm, Hälse Ø 10, M2-Loch nominal Ø 3,0 durchgehend (gedruckt ≈ 2,2–2,4), Mutter hinten
- **Kleine Löcher schrumpfen im Druck stark** (≈ 0,3–0,8 mm) → Maße oben sind Nennmaße fürs CAD

## Offen

- v5: Zahnradpaar und Lagerzapfen-Passung (Ø 6 in Ø 7,0) im Druck prüfen; Spindelhub der LZQ über den Stellweg messen (Zahnräder vertragen ±5 mm Versatz, LZQ-Bohrung 12 mm Hub)

- **Druck 2 auswerten** (`druck/druck2_2026-11-17.3mf`): LZQ-Platte von der abgeschrägten Ecke lesen (Einzelkerben = Reihen = Ø 4,14–4,34, Doppelkerben = Spalten = SW 3,08–3,20). Zylinder am Sockel: Einzelkerben = Tiefe (1–6 = 0,15–0,40), Löcher = Höhe (1–3 = 0,2/0,4/0,6); Zylinder-Ø 11 = NEMA, Ø 9 = LZQ. Ergebnisse in `passungen.html`.
- **Zahnradkupplung/-nabe mit Quetschrillen drucken und prüfen** (`druck/zahnrad_v5.3mf`): sitzen sie nach dem ersten Aufschieben spielfrei? Zu locker → Mitte zwischen 18.11. und 19.11. nehmen: in gen5.py `WIDEN_D, WIDEN_FLAT = 0.075, 0.04`.
- Spindelhub prüfen: LZQ-Abflachung und Rillen sitzen nur auf 6,5 mm an der Nennlage der Spindelspitze; wandert die Spindel tiefer in die Bohrung, verliert sie den Formschluss bzw. der runde Spindelteil läuft gegen die Abflachung.
- **Erstes Passungs-Raster (ausgewertet):** `druck/passungsraster.3mf`, Skript `quelle/passungsraster_3mf.py`. NEMA-Ergebnis siehe „Festgelegte Maße“; die „LZQ-Platte“ war wegen doppelter Objekt-IDs in der 3MF eine zweite NEMA-Platte.
  - Zwei Platten 64 × 64 × 5 mm, je 5 × 5 D-Löcher, 0,20-mm-Schichten, ohne Stützen, ohne Brim. Unten 0,5-mm-Fase, Abflachung beginnt darüber.
  - Lesen: abgeschrägte Ecke vorn links = Reihe 1 / Spalte 1. Kerben am linken Rand = Reihe, Kerben am vorderen Rand = Spalte (je 1–5). Abflachungen zeigen nach hinten.
  - NEMA-Platte hat eine abgeschrägte Ecke, LZQ-Platte eine zweite diagonal gegenüber.

  | | Reihe 1 | Reihe 2 | Reihe 3 | Reihe 4 | Reihe 5 | | Spalte 1 | Spalte 2 | Spalte 3 | Spalte 4 | Spalte 5 |
  |---|---|---|---|---|---|---|---|---|---|---|---|
  | | Bohrungs-Ø | | | | | | Abstand Fläche–Achse | | | | |
  | **NEMA** (Welle Ø 5) | 5,02 | 5,10 | 5,18 | 5,26 | 5,34 | | 2,05 | 2,10 | 2,15 | 2,20 | 2,25 |
  | **LZQ** (Spindel Ø 4) | 4,17 | 4,25 | 4,33 | 4,41 | 4,49 | | 0,90 | 0,95 | 1,00 | 1,05 | 1,10 |

  - Rückmeldung je Platte: „Reihe x, Spalte y“ für das Loch, in dem der Ø leicht läuft und die Fläche spaltfrei anliegt.
- Kammer hinter der Lo-Außentülle (Ø 20,4) braucht noch Support → ggf. Tropfendach
