# Gesichtsfoto-Gestell (v4)

Tischgestell für reproduzierbare Gesichts-, Lippen- und Zungenfotos mit der **Hauptkamera des iPhone 14 Pro Max** (ohne Hülle).
Abstand Objektiv → Stirnpolster: **250,0 mm**. Alles gedruckt, inklusive Schrauben und Muttern (M10 × 1,5, Rändelköpfe, werkzeuglos).

3D-Ansicht: `ansicht/index.html` im Browser öffnen (drehen, zoomen, Kinnschale klappen, Kamerablick).

## Teile (`druck/`, bereits in Druckorientierung, ohne Stützmaterial)

| Datei | Anzahl | Hinweis |
|---|---|---|
| `1_kopfrahmen.stl` | 1 | Grundplatte, 2 verjüngte Fachwerk-Säulen, Kinnsockel mit Zungen-Mulde, Gabel; Innengewinde M10 oben in den Säulen |
| `2_kopfbuegel_kopfueber.stl` | 1 | kopfüber drucken; optional Farbwechsel ab 5 mm → Polster farbig |
| `3_kinnhebel.stl` | 1 | klappbare Kinnschale, aufrecht drucken (Brim empfohlen) |
| `4_traeger_mit_wiege.stl` | 1 | T-Träger mit Handy-Wiege und Zunge |
| `5_gelenkbolzen_M10x82.stl` | 1 | glatter Schaft, Gewinde nur am Ende |
| `6_gabelschraube_M10x31_2x.stl` | 2 | Köpfe wechselseitig einsetzen |
| `7_buegelschraube_M10x20_2x.stl` | 2 | greifen ins Säulen-Gewinde, **nur handfest** |
| `8_mutter_M10_3x.stl` | 3 | Rändelmuttern |

Schrauben/Muttern: stehend (Kopf unten), 100 % Infill oder ≥ 5 Wände. Erst eine Gabelschraube + Mutter als Probe drucken; klemmt es, Schrauben im Slicer in X/Y auf 98 % skalieren.

## Benutzung

- **Gesicht-Modus:** Kinnschale aufrecht (lehnt 5° gegen Anschlag, Übertotpunkt → hält unter Last).
- **Zungen-Modus:** Schale nach vorn klappen (~85°); Kinn liegt auf der festen Mulde, 32 mm tiefer, 12 mm weiter hinten → gleiche Mundöffnung jeden Tag.
- Gestell mit der Kopfseite bündig an die Tischkante stellen und so weit erhöhen, dass die Kinnschale bei leicht vorgebeugtem Sitzen auf Kinnhöhe liegt.
- Kamera: Hauptkamera, Blitz im dunklen Raum, Auslösen mit 3-s-Timer. Fadenkreuze auf beiden Säulen (Objektivhöhe) als Passmarken für die Bildausrichtung.

## Geprüft (im Modell)

- Stirnkontakt exakt 250,00 mm, Schläfenpolster lichte Weite 124 mm (federnd)
- keine Überschneidungen zwischen Teilen, Handy, Köpfen und Muttern; Gewinde Bolzen/Mutter überschneidungsfrei
- Klappweg der Kinnschale frei von −5° bis ~85°
- Sichtlinien Objektiv → Gesicht frei (außer untersten ~5 mm Kinn in der Schale), im Zungen-Modus komplett frei

## Persönliche Maße (Parameter in `quelle/gestell_v4.py`)

| Parameter | Wert | Bedeutung |
|---|---|---|
| `CHIN_DROP` | 150 | Stirnpolster-Mitte → Kinnauflage (senkrecht) – **nachmessen** |
| `TEMPLE_GAP` | 124 | lichter Abstand der Schläfenpolster |
| `TONGUE_DROP` / `TONGUE_BACK` | 32 / 12 | Lage der Zungen-Mulde relativ zur Kinnschale |
| `LENS_DX` / `LENS_DZ` | 15 / 22 | Lage der Hauptkamera im Kamerabuckel (geschätzt) |

Neu bauen: `python quelle/build_v4.py` (benötigt `cadquery`, `trimesh`, `manifold3d`, `shapely`).
