# Fiolenbox

Box für 12 Fiolen (Ø 16 × 38 mm), 4 × 3, mit Stülpdeckel zum Aufrasten.

| Datei | Inhalt |
|---|---|
| `quelle/fiolenbox_v1.py` | CadQuery-Modell, alle Maße oben als Parameter |
| `quelle/ansicht_bauen.py`, `ansicht_template.html` | baut `ansicht/index.html` (3D-Ansicht, offline) |
| `druck/fiolenbox_v1_*.stl/.step` | Unterteil und Deckel (Deckel in geschlossener Lage, zum Drucken umdrehen) |

## v1 (Skizze, 18.11.2026)
- Außen 81,6 × 62,6 × 42,1 mm, Wände 1,6 (Unterteil) / 1,2 (Deckel), Boden 1,5
- Steckplätze: Röhrchen Ø 17 innen (0,5 mm Spiel radial), 14 mm hoch, Raster 19 mm
- Unterteil 24 mm hoch → Fiolen schauen 15,5 mm heraus
- Deckel: Einsteckrand 7 mm tief, 0,25 mm Spiel je Seite; auf den Längsseiten je eine Rastnase (0,6 × 30 mm) in Rille (0,45 mm tief) der Unterteil-Innenwand, 3,5 mm unter der Kante → 0,35 mm Überdeckung beim Aufschieben
- Geprüft im Modell: geschlossen keine Überschneidung, Deckel rastet über 0,3…3,5 mm Hub
- Druck: Unterteil normal, Deckel kopfüber (Deckelplatte aufs Bett); keine Überhänge über 45°
