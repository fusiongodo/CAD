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

## Parameter und Ergebnis (Passungen, Varianten)

Regel (Alex 26.11.): Zu jeder gedruckten Variante eines Teils stehen hier die Maße, die Druckeinstellungen, der Commit und das Ergebnis am Teil.
Ohne das wissen wir in 2 Wochen nicht mehr, was gedruckt wurde, und drucken alles neu. Maße in mm, Soll aus der Quelle (nicht nachgemessen, außer es steht da).

| Teil / Variante | Datei, Commit | Maße (Soll) | Druck | Ergebnis am Teil | Nächster Schritt |
|---|---|---|---|---|---|
| Welle mit Zapfen, M8-Kern, **Variante 1** | `druck/qv_welle_m8kern_e3_5.3mf`, Commit e506d08 (26.11.), Quelle `quelle/qv2/gen_getriebe.py` (`M8_*`) | e 3,5. Loch für M8 × 70 in 3 Stufen: **8,3** frei durch den Exzenter (`M8_FREI`), **8,0** im 10-mm-Zapfen für das 6200 (`M8_ZAPF`, Wand 1,0), **7,2** Kernloch hinten 12 lang, selbstschneidend (`M8_SELF`). Lochgrund z −30,25 | stehend auf dem hinteren Zapfen Ø 14, 0,2 Schicht, 4 Wände, 40 % Füllung, Arachne, Brim 5, keine Stützen | **26.11. (Alex): M8 geht nicht hinein, viel zu eng. Sie klemmt schon ganz oben am Einstieg**, also in der Stufe 8,0 im Zapfen (Einstieg von vorn). Alex versucht, sie mit Werkzeug einzudrehen; Ausgang: ? | Variante 2 mit weiteren Löchern. Maße noch offen (Backlog) |
| Lagerhülse dünn, Wand 2,0 / 1,5 | `druck/qv_huelsen_duenn.3mf`, Commit c28ca96, Quelle `quelle/qv_huelsen_3mf.py` | Bohrung 40,4, Länge 24 + Bund 1 (innen Ø 35). Wand 2,0 (außen 44,4, 2 Punkte) und 1,5 (außen 43,4, 1 Punkt) | Bund unten, 0,2 Schicht, 4 Wände, Arachne, kein Brim | noch nicht gedruckt | – |
| Hohlrad mit Kragen + Steckgriff, Variante 1 | `druck/qv_hohlrad_griff.3mf`, Commit 8040341, Quelle `quelle/qv2/gen_getriebe.py` (`KR_*`, `GR_*`) | Kragen R 34,4 bis 37,6. Schlitze Ø 6,2, Zapfen Ø 5,8, Abstand 20, Griff 4,6 dick, Griff steckt radial | Scheibe unten, 0,2 Schicht, 4 Wände, 40 %, kein Brim | nicht gedruckt. Verworfen (Alex 26.11.): in der Rad-Ebene kein Platz für den Griff, Zugang nur parallel zur Achse | Variante 2: Griff axial aufstecken |
| M8-Akkuschrauber-Aufsatz, Variante 1 | `druck/m8_akkuschrauber_aufsatz.3mf`, Quelle `quelle/m8_akkuschrauber_aufsatz_3mf.py` | Kopf Ø 20 × 10, Tasche SW 13,4 × 6,5 tief. Schaft A SW 8,5 × 28 (Bohrfutter), Schaft B SW 6,25 × 26 (Bit) | Tasche unten, 0,2 Schicht, 8 Wände, 40 %, kein Brim, keine Stützen | noch nicht gedruckt | – |
| T-Stück v3.4 | `druck/development_wand_t.3mf`, Commit 703be89, Quelle `quelle/ihht/wand.py` (`TS_*`) | Bohrung 5,0, Tüllen Ø 7,0, Wellen Ø 7,5, Spitze 6,2, Mutterschacht SW 5,9 | Vorderseite unten, 0,2 Schicht, 5 Wände, 50 %, kein Brim, keine Stützen | aus dem Backlog gestrichen, Ergebnis nicht gemeldet | – |

## Drucke

| Datum | Datei | Teile / Variante | gedruckt | eingebaut | Ergebnis |
|---|---|---|---|---|---|
| 25.11. abends | ihht-pi `welle_zapfen_6200_e3_5.3mf` | Welle mit Zapfen e 3,5 + Distanzring (V9), 1 Stück | ja | noch nicht (Alex fragte nach dem Ring) | – |
| 26.11. | `druck/m8_akkuschrauber_aufsatz.3mf` | Akkuschrauber-Aufsatz für die M8 (Sechskantkopf SW 13), 2 Varianten: Schaft SW 8,5 fürs Bohrfutter, Schaft SW 6,25 für den Bithalter | übergeben, noch nicht gedruckt (33 min, 11 g) | – | Alex: M8 mit dem Akkuschrauber eindrehen statt 200 Handbewegungen |
| 26.11. | `druck/qv_hohlrad_griff.3mf` | 2 × Hohlrad mit Kragen und Steckschlitzen + 1 Steckgriff (Gabel, 2 Zapfen). Ersetzt beide Hohlräder | übergeben, NICHT drucken (2 h 54, 95 g) | – | **Verworfen (Alex 26.11.): Griff steckt radial in der Rad-Ebene, dort ist kein Platz. Zugang nur parallel zur Achse, mit wenig Winkel → Variante 2.** Alex: Hohlrad mit abnehmbarem Griff, um den Exzenter mit mehr Kraft von Hand zu drehen |
| 26.11. | `druck/qv_huelsen_duenn.3mf` | Lagerhülsen dünner, 4 Stück: 2 × Wand 2,0 (außen 44,4, 2 Punkte) + 2 × Wand 1,5 (außen 43,4, 1 Punkt); Punkte vertieft oben auf dem Bodensaum. Bohrung 40,4 | übergeben, noch nicht gedruckt (38 min, 31 g) | – | Alex: auch die dünnere vorhandene Hülse ist noch zu stark. Deren Wand ist nicht gemessen, darum 2 Stufen |
| 26.11. | `druck/development_wand_t.3mf` | T-Stück v3.4: druckt ohne Stützen (Tüllen liegen flach auf dem Bett, Beine unter den Schraubstellen), 1 Stück | aus dem Backlog gestrichen (Alex 26.11.; ob gedruckt oder nicht mehr nötig: ?) (40 min, 14 g) | – | Ersetzt v3.3 |
| 26.11. | `druck/o2_mundstueck_und_t.3mf` | Eine Platte: O2-Mundstück v2 + T-Stück v3.4, alles ohne Stützen | aus dem Backlog gestrichen (Alex 26.11.; ob gedruckt oder nicht mehr nötig: ?) (1 h 53, 45 g) | – | Mundstück unverändert |
| 26.11. | `druck/o2_mundstueck_und_t.3mf` (alter Stand) | Eine Platte: O2-Mundstück v2 + T-Stück v3.3 (mit Baumstützen) | T-Stück gedruckt (Mundstück: ?) | nein | T-Stück unbrauchbar: Stützen auch innen in den Kanälen → v3.4 | Gleiche Teile wie die beiden Einzeldateien darunter |
| 26.11. | `druck/development_wand_t.3mf` (alter Stand) | T-Stück v3.3: Bohrung 5, Tüllen Ø 7 mit Wellen bis 7,5, 1 Stück | durch v3.4 ersetzt | – | – |
| 26.11. | `druck/o2_mundstueck.3mf` | O2-Mundstück mit Reservoirbeutel v2 (mit Gewinde): Grundkörper, Ventilring, Mundrohr | aus dem Backlog gestrichen (Alex 26.11.; ob gedruckt oder nicht mehr nötig: ?) (1 h 23) | – | – |
| 26.11. | `druck/qv_welle_m8kern_e3_5.3mf` | Welle mit Zapfen e 3,5 und M8-Kern (M8 × 70 von vorn durch das 6200) + Distanzring, 1 Stück | gedruckt (Anzahl ?) | nein | Die ihht-pi-Datei `welle_zapfen_6200_e3_5.3mf` ist seit 26.11. dieselbe Geometrie; der Druck vom 25.11. hat noch das M3-Kernloch **Ergebnis (Alex 26.11.): M8 geht nicht hinein, Loch viel zu eng → weiter machen. Maße siehe „Parameter und Ergebnis“.** |
| 26.11. | `druck/quetschventil_v3.3mf` | Quetschventil v3 (M8-Kern, 2 × 608, 2 × 6905, e 4,5): Rückwand, Steg, Getriebeplatte, Hohlrad, Welle, Klemmhülse, Ritzel, 2 Distanzscheiben. Ambosse und Motor vom alten Ventil | übergeben, NICHT drucken | – | Braucht 608 und 6905. Vorhanden sind 2 × 6203 und 6200 (Alex 26.11.), die passen hier nicht |
| 25.11. | `druck/development_wand_hi_mix.3mf` | Steckhalter Hi + Mix v3.2 (Rohr zur Wand, Tülle Y −22) | übergeben, noch nicht gedruckt | – | – |
| 25.11. | `druck/development_wand_hi_mix_t.3mf` | Steckhalter Hi + Mix v3.1, T-Stück v3.1 | ja (Halter sicher, T-Stück ?) | Hi ja | Mix passt aufs Raster. Hi: OCS mit Rohr zur Wand eingesetzt, dafür zu niedrig → v3.2 |
| 25.11. | `druck/development_wand_ocs_alle.3mf` | Steckhalter Hi, Mix, Lo, T-Stück v3.0 | ja | nein | Clip hält die Platine perfekt. Für Rohr vorn zu hoch, Löcher passten in dieser Lage nicht → v3.1 |
| 24./25.11. nachts | ihht-pi, Datei ? | ein komplettes QV (Rahmen v2, Steg 6200, Getriebe, Welle e ?) | ja | ja, Seite ? | dichtet gerade so, geschätzt 97 % |
| 24.11. | ihht-pi `getriebe_v1.3mf` oder `getriebe_v1_ohne_welle.3mf`: ? | Getriebeplatte, Hohlrad, Ritzel (Welle e 3,5 nur in der ersten Datei) | ja | ja, im anderen QV | dieses QV dichtet nicht, es fehlen 1 bis 2 mm |
| vor 25.11. | `druck/development_wand_1_wand.3mf`, `_2_…`, `_3_…` | Wand, Grundplatte, Träger, Knotenbleche | ja | ja | steht, Raster stimmt mit dem Modell |

**Lagerhülsen (Alex 26.11.):** Es gibt zwei Hülsen über den 6203 (ihht-pi `huelse_v1.3mf`, Druck 23.11.). Laut Alex funktioniert die dünnere besser.
Laut Quelle haben beide 2,5 Wand und unterscheiden sich nur in der Bohrung: „eng“ = Bohrung 40,2, außen 45,2, Bund ohne Kerbe; „weit“ = Bohrung 40,4, außen 45,4, eine Kerbe im Bund.
Alex (26.11.) sieht aber an den Teilen deutlich verschiedene Wandstärken und an beiden eine Kerbe (wohl die Naht). Die Quelle passt also nicht zu den Teilen; welche Wand die zwei Hülsen wirklich haben: ? (nicht gemessen).

Ältere Drucke (vor dem 24.11.) stehen in `development_wand.md` und in ihht-pi `VERSUCHE.md`, dort ohne Einbau-Status.
