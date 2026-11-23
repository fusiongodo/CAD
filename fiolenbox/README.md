# Fiolenbox

Box für 12 Fiolen (Ø 16 × 38 mm), 4 × 3, mit Stülpdeckel zum Aufrasten. Stichworte: Phiolenhalter, Fiolenhalter, Vial-Box.

**Fiolenbox 11** (kleine Fiolen Ø 11,6 × 32 mm): gleiche Bauweise, Raster 4 × 3 oder 5 × 4 → `quelle/fiolenbox_11_v1.py`, Druck `druck/fiolenbox_11_4x3_v1.3mf` / `fiolenbox_11_5x4_v1.3mf`.

| Datei | Inhalt |
|---|---|
| `quelle/fiolenbox_v3.py` | **Aktuell (Deckel).** Deckel v3 = Maße von Deckelring 6; Unterteil bleibt v2 |
| `quelle/fiolenbox_v2.py` | **Aktuell (Unterteil).** CadQuery-Modell v2, alle Maße oben als Parameter |
| `quelle/fiolenbox_v1.py` | v1 (Röhrchen, Einsteck-Deckel) |
| `quelle/fiolenbox_11_v1.py` | **Fiolenbox 11** für Ø 11,6 × 32: `~/.cache/cq312/bin/python fiolenbox_11_v1.py 4x3` (oder `5x4`) → STL/STEP + `info_11_<raster>.json` |
| `quelle/ansicht_bauen.py`, `ansicht_template.html` | baut `ansicht/index.html` (3D-Ansicht, offline) |
| `druck/fiolenbox_v2.3mf`, `quelle/fiolenbox_v2_3mf.py` | Druckdatei P1S: Unterteil stehend + Deckel kopfüber, 0,2 mm, 3 Wände, 15 % Infill, kein Support. `fiolenbox_v2_3mf.py <NAME> masse` → 4 Wände, 6/5 Schichten, Cubic 35 % (Masse + Isolation) |
| `quelle/deckelringe_3mf.py`, `druck/deckelringe_2026-11-18.3mf` | Testdruck: 7 Deckelringe (Spiel × Versatz der Rastnase) für das gedruckte Unterteil v2 |
| `passungen.html` | **Messungs-Timeline** Deckelpassung, neue Einträge oben |
| `druck/fiolenbox_v*_*.stl/.step` | Unterteil und Deckel (Deckel in geschlossener Lage, zum Drucken umdrehen) |

## Fiolenbox 11 v1 (23.11.2026) – Fiolen Ø 11,6 × 32
- Unterteil wie v2 (Spiel 0,5, Steg 2 mm → Loch Ø 12,6, Raster 14,6), H_BASE 20 → Fiolen ragen 13,5 mm heraus; Hals 7 mm, Rille wie v2
- Deckel wie v3 / Ring 6 (Spiel 0,06, Nase 0,41, 0,40 über Rillenmitte, Nase 30 mm lang)
- 4 × 3: außen 64,8 × 50,2 × 36,1 mm · 5 × 4: 79,4 × 64,8 × 36,1 mm
- Druck: PLA, 4 Wände, 6/5 Deck-/Bodenschichten, Cubic 35 % (viel Masse, geschlossene Luftkammern isolieren)
- Ansicht: `ansicht/fiolenbox_11_4x3.html`, `fiolenbox_11_5x4.html` (`ansicht_bauen.py <three.js> info_11_<raster>.json <ausgabe>.html`)

## v3 (18.11.2026) – nur Deckel
- Testdruck Deckelringe: Ring 6 passt am besten (siehe `passungen.html`)
- Spiel 0,06 je Seite (v2: 0,25), Rastnase 0,41 hoch (v2: 0,6) und 0,40 über Rillenmitte → zieht den Deckel auf den Absatz, kein Wackeln
- Außen 82,0 × 63,0 mm → 0,19 mm je Seite schmaler als das Unterteil (Wand 1,6 wie beim Testring beibehalten)
- Unterteil unverändert v2

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
