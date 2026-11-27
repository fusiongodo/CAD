# Fiolenbox

Box für 12 Fiolen (Ø 16 × 38 mm), 4 × 3, mit Stülpdeckel zum Aufrasten. Stichworte: Phiolenhalter, Fiolenhalter, Vial-Box.

**Fiolenbox 11** (kleine Fiolen Ø 11,6 × 32 mm): gleiche Bauweise, Raster 4 × 3 oder 5 × 4 → `quelle/fiolenbox_11_v1.py`, Druck `druck/fiolenbox_11_4x3_v1.3mf` / `fiolenbox_11_5x4_v1.3mf`.

| Datei | Inhalt |
|---|---|
| `quelle/fiolenbox_v3.py` | **Aktuell (Deckel).** Deckel v3 = Maße von Deckelring 6; Unterteil bleibt v2 |
| `quelle/fiolenbox_v2.py` | **Aktuell (Unterteil).** CadQuery-Modell v2, alle Maße oben als Parameter |
| `quelle/fiolenbox_v1.py` | v1 (Röhrchen, Einsteck-Deckel) |
| `quelle/fiolenbox_raster_v1.py` | **Beliebiges Raster, auch groß:** `fiolenbox_raster_v1.py <16|11> <SPALTENxREIHEN>` → STL/STEP + `info_<fiole>_<raster>.json`; Seiten ≥ 100 mm: 2 Rastnasen je Seite auf allen 4 Seiten, Deckelplatte 2,0 |
| `quelle/sammelbox_v3.py`, `sammelbox_v3_3mf.py` | **Aktuell: Sammelbox 3 × 3** für 9 große Fiolenboxen, Verschluss Klappe + Zunge |
| `quelle/sammelbox_v2.py`, `sammelbox_v2_3mf.py` | Sammelbox v2 mit Schubriegeln (nicht gedruckt, ersetzt durch v3) |
| `quelle/sammelbox_v1.py` | Sammelbox v1 mit festem Clip (nicht gedruckt, ersetzt durch v2) |
| `quelle/fiolenbox_11_v1.py` | **Fiolenbox 11** für Ø 11,6 × 32: `~/.cache/cq312/bin/python fiolenbox_11_v1.py 4x3` (oder `5x4`) → STL/STEP + `info_11_<raster>.json` |
| `quelle/ansicht_bauen.py`, `ansicht_template.html` | baut `ansicht/index.html` (3D-Ansicht, offline) |
| `druck/fiolenbox_v2.3mf`, `quelle/fiolenbox_v2_3mf.py` | Druckdatei P1S: Unterteil stehend + Deckel kopfüber, 0,2 mm, 3 Wände, 15 % Infill, kein Support. `fiolenbox_v2_3mf.py <NAME> masse` → Deckel 3 Wände, Unterteil 2 Wände, 6/5 Schichten, Cubic 35 % (Masse + Isolation) |
| `quelle/deckelringe_3mf.py`, `druck/deckelringe_2026-11-18.3mf` | Testdruck: 7 Deckelringe (Spiel × Versatz der Rastnase) für das gedruckte Unterteil v2 |
| `passungen.html` | **Messungs-Timeline** Deckelpassung, neue Einträge oben |
| `druck/fiolenbox_v*_*.stl/.step` | Unterteil und Deckel (Deckel in geschlossener Lage, zum Drucken umdrehen) |

## Sammelbox 3 × 3 v3 (27.11.2026) – Klappe mit Mund am Deckel, Zunge an der Wanne
- Idee Alex: Am Deckel hängt eine frei drehbare Klappe mit einem Fenster (Mund), an der Wanne steht eine Zunge heraus (Steg 3 dick, 15 breit). Die Zunge ist ein eigenes Teil und wird variiert, bis es gut sitzt. Befestigt wird mit M2-Schrauben (max. 20 lang), nichts ist lose
- Vorher: 2 Recherchen zu Verschlüssen (Opus-Agent, 26./27.11.). Kern: Die Federung gehört in ein kleines Tauschteil, nicht in die großen Teile. Schubriegel v2 verworfen (keine Raste, 3 Ösen müssen fluchten, 8 Handgriffe)
- Wanne und Deckel wie v2, Boden jetzt geschlossen (Alex 26.11.), keine Ösen mehr. Je Verschluss 2 Laschen am Deckel und 2 an der Wanne; 4 Verschlüsse an den langen Seiten bei ±¼ der Länge
- Schraube: M2 × 20 quer durch Lasche 1 (3 dick, Loch 2,5), das Teil dazwischen (12 breit) und in Lasche 2 (5 dick, Kernloch 1,8). Ab M2 × 16 greift sie
- Klappe: 3 dick, unten 21,6 breit, Mund 15,6 × 3,7. Unter dem Mund ein Steg 1,2 dick, der unter der Zunge liegt: das hält den Deckel (Formschluss). Oben ein Hebel: Hebel zum Deckel drücken, dann schwenkt die Klappe unten auf (bis etwa 45°)
- Zunge: Klotz 12 breit zwischen den Wannen-Laschen, daraus der Steg 15 breit, 3 dick, 8 lang. Unterseite folgt der Bahn des Klappenstegs + 0,3 Luft. Darunter eine Raste (6 breit, in der Mitte), die die Klappe zuhält. Rasthöhe 0,6 / 0,8 / 1,0 = Zunge 1 / 2 / 3 (Kennpunkte), Überdeckung 0,3 / 0,5 / 0,7
- Druck: Klappe mit der Außenseite aufs Bett, Zunge mit der Oberseite aufs Bett. Keine Stützen
- Geprüft im Modell: Klappe schwenkt 0 bis 40° frei, zugeklappt sperrt sie den Deckel, aufgeklappt hebt er frei ab. Slicer ohne Fehler. Am Teil nichts geprüft
- Offen: Spiel der Zunge um ihre Schraube (der Deckel kann rechnerisch bis etwa 0,5 mm anheben); ob die Raste nach Abzug von Lochspiel noch greift
- `quelle/sammelbox_v3.py` (Modell + Prüfung), `quelle/sammelbox_v3_3mf.py` (Testdatei; Mini-Box über `MINI` im Modell-Skript)
- **Testserie:** `druck/testserie_klappe_zunge.3mf` (1 h 07, 38 g). Alex 27.11.: Hohlräder + Griff sind schon gedruckt, also heraus; Test darf bis 1 h dauern. Inhalt: Mini-Box 57,6 × 39,6 (Wanne + Deckel, gleiche Wände, Hals und Laschen wie die große, 1 Verschluss je langer Seite), Klappen 2 × Nr. 1 (Steg 1,2) + 2 × Nr. 2 (Steg 0,9; Marken auf dem Hebel), Zungen je 2 × Nr. 1, 2, 3. Braucht 4 × M2 × 16 bis 20. Die großen Teile (Wanne, Deckel) erst nach dem Test als Druckdatei
- Onshape: Tab `sammelbox_16_3x3_v3` in claude_chassis (Import von `druck/sammelbox_16_3x3_v3_gesamt.step`)

## Sammelbox 3 × 3 v2 (26.11.2026) – Verschluss mit 4 Schubriegeln
- Anlass (Alex): Beim festen Clip weiß niemand, wie stark er sein muss; ein Fehldruck der großen Teile wäre teuer. Lose Clips gehen verloren. Also: Verschluss tauschbar, aber fest am Deckel
- Wanne und Deckel wie v1, aber ohne Rastnasen. Der Deckel liegt mit 0,20 Spiel auf dem Hals
- Je Riegel 2 Ösen am Deckel und 1 Öse an der Wanne (lange Seiten, bei ±¼ der Länge). Der Riegel ist ein Vierkantstab 4,4 × 4,0 × 41, er läuft durch die Deckel-Ösen und schiebt sich in die Wannen-Öse (Hub 11,4). Das hält formschlüssig, eine Federkraft gibt es nicht
- Eine M3-Schraube (Kernloch 2,6, von oben in den Riegel, zwischen den Deckel-Ösen) ist Griff und Anschlag: der Riegel kann nicht herausfallen. Tauschen: Schraube heraus, Riegel ziehen
- Ösenloch 4,8 × 4,6. Riegel in 3 Maßen mit Kennpunkten: 1 = 4,4 × 4,0, 2 = 4,2 × 3,8, 3 = 4,6 × 4,2
- Außen 254,0 × 212,6 (mit Ösen) × 46,3 mm
- `quelle/sammelbox_v2.py` (Modell + Prüfung), `quelle/sammelbox_v2_3mf.py [Riegel-Nr]` (Druckdateien)
- Onshape: Dokument claude_chassis, Tab `sammelbox_16_3x3_v2` (Import von `druck/sammelbox_16_3x3_v2_gesamt.step`, Stand 32e4b93: Wanne, Deckel, 4 Riegel zugeschoben). Ein Import, kein Sync: nach Änderungen neu hochladen
- Geprüft im Modell: offen hebt der Deckel frei ab, zugeschoben sperrt er; 9 Boxen ohne Überschneidung. Slicer ohne Fehler. Am Teil nichts geprüft
- Offen: Der Riegel hat keine Raste, er hält nur über Reibung in seiner Stellung
- Nicht gedruckt, ersetzt durch v3. Druckdateien gelöscht (stehen in Commit 595ade7), Skripte bleiben

## Sammelbox 3 × 3 v1 (26.11.2026) – Box für 9 große Fiolenboxen
- Wanne + Stülpdeckel für 3 × 3 Fiolenboxen v2/v3 (je 82,4 × 63,4 × 42,1), einlagig, Deckel nach oben → `quelle/sammelbox_v1.py`
- Außen 254,0 × 197,0 × 46,3 mm (Druckbett 256: 1 mm Rand je Seite). Mulde 248,4 × 191,4 × 12,4: 0,4 Luft je Box und Richtung
- Mulde nur 12,4 tief: die Unterteile der Fiolenboxen schauen 4,6 heraus. Im Boden unter jeder Box ein Loch Ø 22 zum Herausdrücken von unten
- Clip: 8 Rastnasen (2 je Seite, alle 4 Seiten), Spiel 0,20 je Seite (nicht 0,06, siehe `passungen.html`), Nase 0,55 hoch, 0,40 über Rillenmitte
- Wände dünn, weil das Bett die Länge begrenzt: Hals 1,4, Deckel 1,2, Deckelplatte 2,0, Boden 1,6
- Geprüft im Modell: 9 Boxen (Nennmaß) ohne Überschneidung, sitzend 4,4 mm³ Vorspannung, frei ab 5 mm Hub. Slicer ohne Fehler. Am Teil nichts geprüft
- Nicht gedruckt, ersetzt durch v2. Druckdateien gelöscht (stehen in Commit 87bfc2d), Skript `quelle/sammelbox_v1.py` bleibt
- **Fiolenbox 11 (26.11., Alex):** Deckel saß bombenfest, Box aufgebrochen. Ursache offen, siehe `passungen.html`; die Dateien `fiolenbox_11_*` so nicht mehr drucken

## 10 × 10 v1 (25.11.2026) – 100 Plätze, beide Fiolengrößen
- 16 mm (Ø 16 × 38): außen 196,4 × 196,4 × 42,5 mm · 11 mm (Ø 11,6 × 32): 152,4 × 152,4 × 36,5 mm
- Unterteil wie v2 / Fiolenbox 11, Deckel-Rastung Ring 6
- 8 Rastnasen (2 je Seite bei ±¼ Seitenlänge, alle 4 Seiten) statt 2 – mehr Last (100 Fiolen), lange Seiten federn stark
- Deckelplatte 2,0 mm (Spannweite ~190 bzw. ~150 mm)
- Geprüft im Modell: sitzend 12,7 mm³ Vorspannung (= 8 × Ring-6-Wert), Rastung über ~1,5 mm Hub, frei ab 5 mm
- Ansicht: `ansicht/fiolenbox_16_10x10.html`, `fiolenbox_11_10x10.html`
- Druck (Masse-Profil): passt nicht zusammen auf eine Platte → je Teil eine 3MF, `druck/fiolenbox_<16|11>_10x10_v1_unterteil.3mf` / `_deckel.3mf`

## Fiolenbox 11 v1 (23.11.2026) – Fiolen Ø 11,6 × 32
- Unterteil wie v2 (Spiel 0,5, Steg 2 mm → Loch Ø 12,6, Raster 14,6), H_BASE 20 → Fiolen ragen 13,5 mm heraus; Hals 7 mm, Rille wie v2
- Deckel wie v3 / Ring 6 (Spiel 0,06, Nase 0,41, 0,40 über Rillenmitte, Nase 30 mm lang)
- 4 × 3: außen 64,8 × 50,2 × 36,1 mm · 5 × 4: 79,4 × 64,8 × 36,1 mm
- Druck: PLA, Deckel 3 Wände, Unterteil 2 Wände (Löcher), 6/5 Deck-/Bodenschichten, Cubic 35 % (viel Masse, geschlossene Luftkammern isolieren)
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
