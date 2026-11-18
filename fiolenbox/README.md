# Fiolenbox

Box für 12 Fiolen (Ø 16 × 38 mm), 4 × 3, mit Stülpdeckel zum Aufrasten.

| Datei | Inhalt |
|---|---|
| `quelle/fiolenbox_v2.py` | **Aktuell.** CadQuery-Modell v2, alle Maße oben als Parameter |
| `quelle/fiolenbox_v1.py` | v1 (Röhrchen, Einsteck-Deckel) |
| `quelle/ansicht_bauen.py`, `ansicht_template.html` | baut `ansicht/index.html` (3D-Ansicht, offline) |
| `druck/fiolenbox_v2.3mf`, `quelle/fiolenbox_v2_3mf.py` | Druckdatei P1S: Unterteil stehend + Deckel kopfüber, 0,2 mm, 3 Wände, 15 % Infill, kein Support |
| `druck/fiolenbox_v*_*.stl/.step` | Unterteil und Deckel (Deckel in geschlossener Lage, zum Drucken umdrehen) |

## v2 (Skizze, 18.11.2026)
- Feedback zu v1: Löcher in einer durchgehenden Ebene bis zur Oberkante, mit der Wand verbunden; Deckel klippt außen über das Unterteil
- Unterteil = Block mit 12 Löchern Ø 17 bis 1,5 mm über Boden, oben 0,8-mm-Fase; im Slicer Wände um jedes Loch + Infill (Stege zwischen den Löchern nur 2 mm → ohnehin massiv)
- Oben 7 mm hoher Hals (1,85 mm zurückgesetzt), Absatz bei z 17 = Anschlag für den Deckel; Halskante 0,8 mm angefast als Einführschräge
- Deckel: Haube, Wand 1,6, sitzt bündig auf dem Absatz; Rastnase innen (0,6 × 30 mm, Längsseiten) in Rille am Hals (0,45 tief, Mitte z 20) → 0,35 mm Überdeckung
- Außen 82,4 × 63,4 × 42,1 mm; Fiolen ragen 15,5 mm über die Ebene
- Geprüft im Modell: geschlossen keine Überschneidung, Rastung über 0,3…3,5 mm Hub
- Druck: Unterteil normal, Deckel kopfüber; keine Überhänge, kein Support

## v1 (Skizze, 18.11.2026)
- Außen 81,6 × 62,6 × 42,1 mm, Wände 1,6 (Unterteil) / 1,2 (Deckel), Boden 1,5
- Steckplätze: Röhrchen Ø 17 innen (0,5 mm Spiel radial), 14 mm hoch, Raster 19 mm
- Unterteil 24 mm hoch → Fiolen schauen 15,5 mm heraus
- Deckel: Einsteckrand 7 mm tief, 0,25 mm Spiel je Seite; auf den Längsseiten je eine Rastnase (0,6 × 30 mm) in Rille (0,45 mm tief) der Unterteil-Innenwand, 3,5 mm unter der Kante → 0,35 mm Überdeckung beim Aufschieben
- Geprüft im Modell: geschlossen keine Überschneidung, Deckel rastet über 0,3…3,5 mm Hub
- Druck: Unterteil normal, Deckel kopfüber (Deckelplatte aufs Bett); keine Überhänge über 45°
