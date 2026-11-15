# IHHT-Gasmischer – Chassis & Druckteile

Stand: 15.11.2026. Alle Teile sind Onshape-FeatureScripts (je ein Feature, keine Parameter).

## Inhalt

| Datei | Was |
|---|---|
| `quelle/chassis_v2.fs` | Entwicklungs-Chassis v2 (Rotameter mit Drehknopf unten): Rahmen 1 mm Wand / 1 mm Boden + Randsteg, Regal, Motorbrücke 1 mm mit Distanzringen, alte Kupplungen, Kamerasäule, alte Splitter, Dummies; Feature „IHHT Ablage“ mit den v1-Teilen |
| `quelle/splitter_v3.fs` | Splitter hi + lo v3 (ersetzt die beiden unteren Splitter aus chassis_v2) |
| `quelle/kamerahalter_v3.fs` | Kamerahalter v3 (ersetzt den Halter aus chassis_v2) |
| `quelle/kupplung_v1.fs` | Steckkupplung NEMA 17 ↔ LZQ, 4 Stück |
| `quelle/gen.py`, `gen2.py`, `mkgeom.py` | Generator (Python) für chassis_v2.fs: `python3 gen2.py chassis_v2.fs parts.json` |
| `quelle/splitter_v3.patch`, `kamerahalter_v3.patch` | Änderungen gegenüber gen2.py für Splitter v3 / Kamerahalter v3 |
| `quelle/kupplung_v1_3mf.py` | baut die Kupplungs-3MF (Bambu-Vorlage nötig) |
| `druck/` | 3MF-Dateien (Bambu Studio, Einstellungen eingebettet) |

## Festgelegte Maße (aus Testdrucken)

- **Kupplung NEMA-Seite:** Bohrung Ø 5,32, Abflachung 2,22 mm von der Achse
- **Kupplung LZQ-Seite:** Bohrung Ø 4,50, Abflachung 1,06 mm von der Achse; Außen-Ø 9, 0,28-mm-Schichten, stehend, NEMA-Seite unten
- **Innentüllen Splitter → Rotameter:** Wellenscheitel Ø 11,25 (Kern 10,75), Bohrung Ø 6,5 Tropfenprofil
- **Splitter-Kanal:** 8 mm, volles Tropfenprofil (druckt ohne Support); Lo-Außentülle auf z 30 (+4 mm)
- **Kamerahalter:** Platte 1 mm, Hälse Ø 10, M2-Loch nominal Ø 3,0 durchgehend (gedruckt ≈ 2,2–2,4), Mutter hinten
- **Kleine Löcher schrumpfen im Druck stark** (≈ 0,3–0,8 mm) → Maße oben sind Nennmaße fürs CAD

## Offen

- Prüfhülsen-Raster (32 Stück) zur Feinabstimmung der Kupplung – Ergebnis steht aus
- Kammer hinter der Lo-Außentülle (Ø 20,4) braucht noch Support → ggf. Tropfendach
