# Drucklog IHHT-Gasmischer

Ein Eintrag je Druck: was gedruckt wurde, aus welcher Datei, und wo es eingebaut ist. Neueste oben.
Claude trägt jeden Druck ein, sobald Alex ihn meldet oder ein Foto ihn zeigt, und fragt nach, wenn Variante oder Einbauort fehlen.
„?“ heißt: nicht notiert, nicht raten.

Dateien: `druck/…` liegt in diesem Repo, `ihht-pi/…` in `~/Desktop/projects/ihht-retrofit/ihht-pi/CAD/quetschventil/druck/`.

## Was gerade eingebaut ist (Stand 25.11.2026 abends)

| Platz | Teil | Version / Variante | Quelle |
|---|---|---|---|
| Wand, Grundplatte, 4 Knotenbleche | Development-Wand | v2.2 / v2.3 (orange) | Fotos 25.11. |
| QV links (hi) | Rahmen v2, Steg 6200, Gegenlagerscheibe, Getriebe v1 | Welle e ?, Hülse ?; Getriebeplatte + Hohlrad vom 24.11. oder Nachtdruck: ? | Alex 25.11. |
| QV rechts (lo) | wie links | Welle e ?, Hülse ?; welches der beiden das ältere Getriebe hat: ? | Alex 25.11. |
| OCS lo (klein), rechts der Mitte | Steckhalter Lo | Version ? (Schraube sitzt in einem selbst gebohrten Loch), M2-Löcher 2,4 | Foto 25.11. |
| OCS hi (groß), links der Mitte | Steckhalter Hi | v3.1 (dunkles Filament), OCS mit Rohr zur Wand eingeclipst, sitzt zu tief | Foto von oben 25.11. |
| OCS Mix, rechts unten | Steckhalter Mix | v3.1 gedruckt, passt aufs Raster; montiert: ? | Alex 25.11. |
| T-Stück | Druckteil | v3.1 gedruckt: ?; montiert: ? | – |
| SKR Pico | direkt auf dem Lochraster verschraubt (Rückseite der Wand, ohne den Pico-Träger aus dem Modell), USB-C und 2 Adern Versorgung angeschlossen | – | Foto 25.11. abends |

## Drucke

| Datum | Datei | Teile / Variante | gedruckt | eingebaut | Ergebnis |
|---|---|---|---|---|---|
| 25.11. abends | ihht-pi `welle_zapfen_6200_e3_5.3mf` | Welle mit Zapfen e 3,5 + Distanzring (V9), 1 Stück | ja | noch nicht (Alex fragte nach dem Ring) | – |
| 26.11. | `druck/development_wand_t.3mf` | T-Stück v3.4: druckt ohne Stützen (Tüllen liegen flach auf dem Bett, Beine unter den Schraubstellen), 1 Stück | übergeben, noch nicht gedruckt (40 min, 14 g) | – | Ersetzt v3.3 |
| 26.11. | `druck/o2_mundstueck_und_t.3mf` | Eine Platte: O2-Mundstück v2 + T-Stück v3.4, alles ohne Stützen | übergeben, noch nicht gedruckt (1 h 53, 45 g) | – | Mundstück unverändert |
| 26.11. | `druck/o2_mundstueck_und_t.3mf` (alter Stand) | Eine Platte: O2-Mundstück v2 + T-Stück v3.3 (mit Baumstützen) | T-Stück gedruckt (Mundstück: ?) | nein | T-Stück unbrauchbar: Stützen auch innen in den Kanälen → v3.4 | Gleiche Teile wie die beiden Einzeldateien darunter |
| 26.11. | `druck/development_wand_t.3mf` (alter Stand) | T-Stück v3.3: Bohrung 5, Tüllen Ø 7 mit Wellen bis 7,5, 1 Stück | durch v3.4 ersetzt | – | – |
| 26.11. | `druck/o2_mundstueck.3mf` | O2-Mundstück mit Reservoirbeutel v2 (mit Gewinde): Grundkörper, Ventilring, Mundrohr | übergeben, noch nicht gedruckt (1 h 23) | – | – |
| 26.11. | `druck/qv_welle_m8kern_e3_5.3mf` | Welle mit Zapfen e 3,5 und M8-Kern (M8 × 70 von vorn durch das 6200) + Distanzring, 1 Stück | übergeben, noch nicht gedruckt (41 min) | – | Die ihht-pi-Datei `welle_zapfen_6200_e3_5.3mf` ist seit 26.11. dieselbe Geometrie; der Druck vom 25.11. hat noch das M3-Kernloch |
| 26.11. | `druck/quetschventil_v3.3mf` | Quetschventil v3 (M8-Kern, 2 × 608, 2 × 6905, e 4,5): Rückwand, Steg, Getriebeplatte, Hohlrad, Welle, Klemmhülse, Ritzel, 2 Distanzscheiben. Ambosse und Motor vom alten Ventil | übergeben, NICHT drucken | – | Braucht 608 und 6905. Vorhanden sind 2 × 6203 und 6200 (Alex 26.11.), die passen hier nicht |
| 25.11. | `druck/development_wand_hi_mix.3mf` | Steckhalter Hi + Mix v3.2 (Rohr zur Wand, Tülle Y −22) | übergeben, noch nicht gedruckt | – | – |
| 25.11. | `druck/development_wand_hi_mix_t.3mf` | Steckhalter Hi + Mix v3.1, T-Stück v3.1 | ja (Halter sicher, T-Stück ?) | Hi ja | Mix passt aufs Raster. Hi: OCS mit Rohr zur Wand eingesetzt, dafür zu niedrig → v3.2 |
| 25.11. | `druck/development_wand_ocs_alle.3mf` | Steckhalter Hi, Mix, Lo, T-Stück v3.0 | ja | nein | Clip hält die Platine perfekt. Für Rohr vorn zu hoch, Löcher passten in dieser Lage nicht → v3.1 |
| 24./25.11. nachts | ihht-pi, Datei ? | ein komplettes QV (Rahmen v2, Steg 6200, Getriebe, Welle e ?) | ja | ja, Seite ? | dichtet gerade so, geschätzt 97 % |
| 24.11. | ihht-pi `getriebe_v1.3mf` oder `getriebe_v1_ohne_welle.3mf`: ? | Getriebeplatte, Hohlrad, Ritzel (Welle e 3,5 nur in der ersten Datei) | ja | ja, im anderen QV | dieses QV dichtet nicht, es fehlen 1 bis 2 mm |
| vor 25.11. | `druck/development_wand_1_wand.3mf`, `_2_…`, `_3_…` | Wand, Grundplatte, Träger, Knotenbleche | ja | ja | steht, Raster stimmt mit dem Modell |

**Lagerhülsen (Alex 26.11.):** Es gibt zwei Hülsen über den 6203 (ihht-pi `huelse_v1.3mf`, Druck 23.11.). Laut Alex funktioniert die dünnere besser.
Laut Quelle haben beide 2,5 Wand und unterscheiden sich nur in der Bohrung: „eng“ = Bohrung 40,2, außen 45,2, Bund ohne Kerbe; „weit“ = Bohrung 40,4, außen 45,4, eine Kerbe im Bund.
Die dünnere wäre danach „eng“ (ohne Kerbe). Am Teil nicht nachgemessen; ob Alex einen anderen, nicht notierten Ring meint: ?

Ältere Drucke (vor dem 24.11.) stehen in `development_wand.md` und in ihht-pi `VERSUCHE.md`, dort ohne Einbau-Status.
