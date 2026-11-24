# Development-Wand (Variante A „Senkrechte Wand“)

Stand 24.11.2026, Anordnung v2.1.

- **Quelle:** `quelle/teile/wand_*.py` und `quelle/ihht/wand.py`
- **Onshape-Modul:** `development_wand.fs`
- **Prüfung und Druckdateien:** `quelle/development_wand_3mf.py` schreibt `druck/development_wand_{1,2,3}_*.3mf`
- **Vorlage:** ihht-pi `CAD/produktkasten-skizzen/development_wand_3d.html`, Funktion `A()`

## Verlauf

- **v1 (23.11.):** QV bei X ±58, OCS liegen waagerecht unter den QVs, Wand 240 × 210. Verworfen, weil die Schläuche kreuz und quer liefen und für das Y kein Platz war.
- **v2 (23.11.):** Das Gas fließt von außen nach innen und von oben nach unten. QV bei X ±73, Wand 246 × 246. Die OCS standen hochkant mit den Tüllen nach vorn.
  Verworfen (Alex): Vom QV zur nach vorn zeigenden Tülle hätte der Schlauch auf etwa 25 mm seitlich mehr als 180° drehen müssen und wäre geknickt.
- **v2.1 (24.11., aktuell):**
  - OCS hi und lo stehen hochkant parallel zur Wand, die Tüllen zeigen seitlich zum QV. Der untere QV-Schlauch läuft gerade auf den Eingang.
  - Die Ausgänge gehen mit je einem U-Bogen zu einem T-Stück in der Mitte, das T-Stück gerade nach unten auf OCS Mix.
  - OCS Mix steht quer zur Wand auf der Grundplatte. Dadurch geht der Patientenschlauch vor allen anderen Schläuchen nach vorn weg.
  - Alle Schläuche liegen in einer Ebene (Y −22) und kreuzen sich nicht. Jeder Bogen hat R ≥ 25 (etwa 2 × Außendurchmesser des Schlauchs 6/12).

## Fluss (X quer, Z Höhe über Tisch)

| Strecke | Tüllen / Höhen | frei | Bogen | Zuschnitt ca. |
|---|---|---|---|---|
| Zufuhr außen → QV | hi links, lo rechts, waagerecht durch das QV | – | – | – |
| QV oben (Ablass) → Durchlass | Z 222,5 → Durchlass Ø 18 bei X ±13,5 | 36 | R 25 | 60 + hinten nach Bedarf |
| QV unten → OCS-Eingang | Z 173,5, gerade, Tülle hi X −31,5, lo X +30 | 6–7 | gerade | QV-Schlauch 15 länger lassen |
| OCS hi Ausgang → T | Z 105,2 → T-Arm links Z 53 (U-Bogen nach außen) | 99 | R 26 | 115 |
| OCS lo Ausgang → T | Z 135 → T-Arm rechts Z 53 (Bogen außen herum) | 126 | R 25 | 140 |
| T-Abgang → OCS Mix Eingang | Z 38 → Z 35, gerade | 3 | gerade | 20 (Muffe) |
| OCS Mix Ausgang → Patient | Z 35, 38,5 vor dem Eingang, hoch und nach vorn | – | R 25 | nach Bedarf |

Die Spalte „frei“ ist die Länge der Mittellinie zwischen den Tüllenspitzen. Für den Zuschnitt kommen je Ende etwa 8 mm Aufstecklänge dazu.

## Aufbau

- **Wand** 246 × 246 × 6, steht auf der Grundplatte. Hinten ein Randsteg (4 breit, 10 tief, unten 12 hoch), ein Quersteg bei Z 140
  und je ein Ringsteg um die QV-Ausschnitte. Die Vorderseite ist flach.
  Oben links (von vorn, über QV hi) hat sie eine 45°-Fase mit 38 mm Schenkellänge und Randsteg. Grund: Der P1S sperrt vorn links 18 × 28 mm
  und prüft die Hülle des Teils, eine rechteckige Ausklinkung reicht nicht. Die QV-Platte steht dort als Dreieck etwa 20 × 20 frei,
  die obere äußere QV-Schraube bleibt (Lochrand 2,9 mm von der Fase).
- **Lochraster** M3 (Ø 3,4), Abstand 10, Spalten X ±5 … ±105, Zeilen Z 26 … 236 (278 Bohrungen).
- **QV-Aufnahmen** bei X ±73, Achse Z 198: Ausschnitt Ø 75 für das Hohlrad und ein Feld 91 × 41 für die 4 Getriebepfosten.
  Die Rückplatte des QV liegt vorn flach an. Gehalten wird es von 4 × M3 × 20 von hinten durch die Wand in die Ambosse.
  Das passt für Rahmen v2 + Getriebe v1 und für QV v3.
- **2 Ablass-Durchlässe** Ø 18 bei X ±13,5, Z 222,5.
- **Grundplatte** 246 × 160 × 6 als L, Y −80 … 80. Alle Schrauben kommen von unten, die Köpfe sind versenkt.
  Vorn hat sie 2 Kabelbinder-Schlitzpaare für OCS Mix (Y −40 und −72) mit Nut auf der Unterseite.
- **4 Knotenbleche**, 81 hoch und 70 tief, an jedem Ende eins vorn und eins hinten.
- **2 Träger** für Pi und Pico (Pi-Lochbild 58 × 49, Dome M2,5), hinten unten.
- **2 Rückenhalter** (Hi, Lo, gleiches Teil): Flansch 24 × 60 mit 2 Finnen an den Rändern. Die Finnen geben 14 mm Abstand, das OCS liegt mit der Rohrseite daran an.
  Die 4 Schlitze für Kabelbinder liegen bei 8 / 22 / 38 / 52 ab der Unterkante. Nimm hi 8 + 38 und lo 8 + 38, die Schlitze bei 22 liegen auf Höhe einer Tülle.
- **T-Halter:** Flansch 40 × 24 mit Finnen über und unter dem T-Stück. Unten sitzt eine Gabel für den Abgang, sie hält das T in X und Y.
- **Kaufteil T-Stück 6 mm:** Arme etwa ±18, Abgang nach unten höchstens 15 lang, sonst stößt es auf OCS Mix.
- **Lage der OCS:** OCS hi bei X −24 … −1, OCS lo bei X 1 … 25, beide Y −14 … −37. Den Abstand der Rohrmitte zur Rohrseite (etwa 8) habe ich geschätzt.

Prüfung (`development_wand_3mf.py`):
- Alle Teile sind wasserdicht und je ein Körper.
- Es gibt keine Überschneidung im Modul. Das gilt auch für die Schlauch-Dummies und die echten QV-v2- und QV-v3-Teile an beiden Plätzen.
- Alle Schrauben der Knotenbleche, Träger und Halter sitzen auf Rasterbohrungen.

## Druck (Bambu P1S, PLA, texturierte PEI, alle ohne Stützen)

Für alle Platten gilt: 0,20 mm, Gyroid, Lüfter erst ab Schicht 4, erste Schicht 50 mm/s. Tür zu, Deckel einen Spalt offen.
Alle drei Platten habe ich mit Bambu Studio im P1S-Profil gesliced (24.11.), es gab keine Fehler.

| Datei | Teile | Wände | Infill | Deckel / Boden | Brim | Bett | Zeit | PLA |
|---|---|---|---|---|---|---|---|---|
| `development_wand_1_wand.3mf` | Wand (Vorderseite unten, Fase vorn links) | 3 | 15 % | 5 / 4 | – | 65 °C | 6 h 37 | 249 g |
| `development_wand_2_grundplatte_traeger.3mf` | Grundplatte (hinten), 2 Träger, T-Halter | 3 | 20 % | 5 / 4 | 5 | 60 °C | 4 h 09 | 177 g |
| `development_wand_3_knotenbleche.3mf` | 4 Knotenbleche, 2 Rückenhalter | 5 | 40 % | 6 / 5 | 3 | 55 °C | 2 h 40 | 113 g |

Summe etwa 13,5 h und 540 g. Wand und Grundplatte passen nicht zusammen auf ein Bett, weil die Wand das Bett allein füllt.

## Schrauben und Kaufteile

| Verbindung | Anzahl | Teil |
|---|---|---|
| Wand ↔ Grundplatte (von unten in den Randsteg, selbstschneidend) | 6 | M3 × 12 Zylinderkopf |
| Knotenblech ↔ Grundplatte (von unten, Mutter oben) | 8 | M3 × 12 + Mutter M3 |
| Knotenblech vorn ↔ Wand ↔ Knotenblech hinten | 4 | M3 × 20 + Mutter M3 |
| QV ↔ Wand (von hinten in die Ambosse, ersetzen die 4 äußeren M3 × 12) | 8 | M3 × 20 |
| Träger ↔ Wand (Mutter vorn) | 8 | M3 × 12 + Mutter M3 |
| Rückenhalter, T-Halter ↔ Wand (von vorn, Mutter hinten) | 6 | M3 × 16 + Mutter M3 |
| Pi / Pico ↔ Träger (selbstschneidend) | 8 | M2,5 × 6 |
| OCS hi / lo, T-Stück, OCS Mix (Mix durch die Grundplatte) | 8 | Kabelbinder 2,5 × 150 |
| T-Stück für Schlauch 6 mm | 1 | Kaufteil, Abgang ≤ 15 |

Summe: 22 × M3 × 12, 6 × M3 × 16, 12 × M3 × 20, 26 × Mutter M3, 8 × M2,5 × 6, 8 Kabelbinder, 1 T-Stück.

## Montage

1. Die Wand auf die Grundplatte stellen, mit dem Randsteg nach hinten. Von unten 6 × M3 × 12 in den unteren Randsteg schrauben.
2. Die 4 Knotenbleche ansetzen. Die Wand-Flansche mit 4 × M3 × 20 verbinden (durch vorn, Wand und hinten, Mutter hinten).
   Die Grundplatten-Flansche mit 8 × M3 × 12 von unten verschrauben, Mutter oben. Erst alles handfest anziehen, dann fest.
3. Die Träger hinten mit je 4 × M3 × 12 befestigen, Mutter vorn. Pi kommt links (von vorn gesehen) auf X −55, Pico rechts auf X +55.
4. Die Halter vorn mit M3 × 16 anschrauben, Mutter hinten:
   - Rückenhalter Hi: X −15, Z 126 und 156
   - Rückenhalter Lo: X +15, Z 126 und 156
   - T-Halter: X ±15, Z 56
5. Am QV die Getriebeplatte mit dem NEMA abnehmen und die 4 äußeren Amboss-Schrauben herausdrehen.
   Das QV von vorn mit dem Hohlrad durch den Ausschnitt an die Wand legen und 4 × M3 × 20 von hinten eindrehen. Danach das Getriebe von hinten wieder ansetzen.
   QV hi kommt links hin, QV lo rechts. Den unteren Schlauch etwa 15 mm über das Amboss-Ende hinaus stehen lassen.
6. Die OCS montieren:
   - OCS hi mit den Tüllen nach links an den Rückenhalter Hi binden, OCS lo mit den Tüllen nach rechts an den Rückenhalter Lo. Die Rohrseite zeigt zur Wand, der Eingang ist oben.
   - Den unteren QV-Schlauch direkt auf den oberen Eingang schieben.
7. Das T-Stück mit dem Abgang nach unten in die Gabel des T-Halters setzen und festbinden.
   OCS Mix quer zur Wand auf die Grundplatte stellen, mit den Tüllen nach oben und dem Eingang hinten. Mit 2 Kabelbindern durch die Grundplatte festbinden.
8. Die Schläuche verlegen und zuschneiden wie in der Tabelle „Fluss“:
   - OCS hi Ausgang mit U-Bogen nach außen zum linken T-Arm, OCS lo Ausgang außen herum zum rechten T-Arm.
   - Vom T-Abgang zum OCS Mix ein Muffenstück von 20 mm.
   - Die Ablass-Schläuche durch die Durchlässe nach hinten führen.

## Offen

- Das Lochbild des BTT SKR Pico ist nicht geprüft. Laut Händler entspricht es dem Pi (58 × 49). Vor dem Druck der Träger am Board nachmessen.
- Die Tüllenlage der OCS stammt aus den Datenblatt-Zeichnungen, beim hi mit ±5 Toleranz. Die Höhe der Tüllenachse über der Platine habe ich geschätzt.
  Wenn der Eingang nicht genau auf der Höhe des QV-Schlauchs liegt, das OCS am Rückenhalter verschieben. Die Kabelbinder lassen das zu.
- Die unteren QV-Schrauben (y −33,5) erreicht man nur bei abgenommenem Getriebe.
- Biegeradius: R 25 entspricht etwa 2 × Außendurchmesser. Silikon 6/12 hält das. Vorher einmal um einen Ø 50-Gegenstand biegen und prüfen, ob der Schlauch frei bleibt.
