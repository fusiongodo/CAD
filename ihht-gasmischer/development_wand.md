# Development-Wand (Variante A „Senkrechte Wand“)

Stand 25.11.2026, Anordnung v2.4.

- **Quelle:** `quelle/teile/wand_*.py` und `quelle/ihht/wand.py`
- **Onshape-Modul:** `development_wand.fs`
- **Prüfung und Druckdateien:** `quelle/development_wand_3mf.py` schreibt `druck/development_wand_{1,2,3}_*.3mf`
- **Vorlage:** ihht-pi `CAD/produktkasten-skizzen/development_wand_3d.html`, Funktion `A()`

## Verlauf

- **v1 (23.11.):** QV bei X ±58, OCS liegen waagerecht unter den QVs, Wand 240 × 210. Verworfen, weil die Schläuche kreuz und quer liefen und für das Y kein Platz war.
- **v2 (23.11.):** Das Gas fließt von außen nach innen und von oben nach unten. QV bei X ±73, Wand 246 × 246. Die OCS standen hochkant mit den Tüllen nach vorn.
  Verworfen (Alex): Vom QV zur nach vorn zeigenden Tülle hätte der Schlauch auf etwa 25 mm seitlich mehr als 180° drehen müssen und wäre geknickt.
- **v2.1 (24.11.):**
  - OCS hi und lo stehen hochkant parallel zur Wand, die Tüllen zeigen seitlich zum QV. Der untere QV-Schlauch läuft gerade auf den Eingang.
  - Die Ausgänge gehen mit je einem U-Bogen zu einem T-Stück in der Mitte, das T-Stück gerade nach unten auf OCS Mix.
  - OCS Mix steht quer zur Wand auf der Grundplatte. Dadurch geht der Patientenschlauch vor allen anderen Schläuchen nach vorn weg.
  - Alle Schläuche liegen in einer Ebene (Y −22) und kreuzen sich nicht. Jeder Bogen hat R ≥ 25 (etwa 2 × Außendurchmesser des Schlauchs 6/12).
- **v2.2 (24.11.):** Alex hat beide OCS ausgemessen.
  - OCS hi ist der 3F 3.0 (125 × 22, Tüllen 85 auseinander), OCS lo der 3FRL 2.0 (79 × 24, Tüllen 38,5).
  - Beide sitzen jetzt mit der Bauteilseite (Stecker) zur Wand und dem Rohr nach vorn auf Steckhaltern. Die Platine wird auf Zapfen gesteckt.
  - Die untere hi-Tülle liegt 17 tiefer als vorher. Ein gerader U-Bogen zum T hätte nur R 18. Deshalb lädt der Bogen links weit aus (bis X −84)
    und läuft mit einer flachen S-Kurve von unten in den T-Arm, überall R 25.

- **v2.3 (25.11.):** Beim Aufbau gefunden: Die QV-Schraube oben außen am QV hi (X −99, Z 231,5) liegt 4,6 von der Fasenkante.
  Der 4 breite Fasen-Steg hinten deckte den Schraubenkopf etwa 2 mm ab, die M3 × 20 ging nicht hinein.
  - Gedruckte Wand v2.2: Steg dort wegknipsen oder die Schraube weglassen. 3 von 4 Schrauben halten das QV.
  - Neu: Freiraum Ø 7 hinten um jede QV-Schraube (`QV_KOPF_D`, klinkt den Fasen-Steg aus).
  - Die Prüfung testet jetzt auch die Schraubenköpfe hinten (Ø 6 über die Stegtiefe), nicht nur die Löcher.

- **v2.4 (25.11., aktuell):** Die Rastleisten am OCS-Steckhalter Hi brachen beim Aufstecken ab (1,6 dick, quer zu den Schichten gedruckt).
  - Leisten jetzt 2,2 dick, Wulst bleibt 0,6 über der Platinenkante, Flansch X −26,4 … 0,4.
  - Druck auf der Seite (linke Leiste aufs Bett): Die Schichten laufen längs durch die Leisten. Die obere Leiste braucht Baumstützen (nur vom Bett).
  - Eigene Datei `druck/development_wand_ocshalter_hi.3mf`, 0,28 mm, 5 Wände, 50 %, etwa 25 min. Aus Platte 3 herausgenommen.

## Fluss (X quer, Z Höhe über Tisch)

| Strecke | Tüllen / Höhen | frei | Bogen | Zuschnitt ca. |
|---|---|---|---|---|
| Zufuhr außen → QV | hi links, lo rechts, waagerecht durch das QV | – | – | – |
| QV oben (Ablass) → Durchlass | Z 222,5 → Durchlass Ø 18 bei X ±13,5 | 36 | R 25 | 60 + hinten nach Bedarf |
| QV unten → OCS-Eingang | Z 173,5, gerade, Tülle hi X −31, lo X +30,5 | 6 | gerade | QV-Schlauch 15 länger lassen |
| OCS hi Ausgang → T | Z 88,5 → U-Bogen links bis X −84 und Z 38,5, S-Kurve hoch in den T-Arm links Z 53 | 146 | R 25 | 160 |
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
- **QV-Aufnahmen** bei X ±73, Achse Z 198: Ausschnitt Ø 75 für das Hohlrad und ein Feld 91 × 41 für die 4 Getriebepfosten (Ecken mit r 5,5 um die Pfosten gerundet).
  Die Rückplatte des QV liegt vorn flach an. Gehalten wird es von 4 × M3 × 20 von hinten durch die Wand in die Ambosse.
  Das passt für Rahmen v2 + Getriebe v1 und für QV v3.
- **2 Ablass-Durchlässe** Ø 18 bei X ±13,5, Z 222,5.
- **Grundplatte** 246 × 160 × 6 als L, Y −80 … 80. Alle Schrauben kommen von unten, die Köpfe sind versenkt.
  Vorn hat sie 2 Kabelbinder-Schlitzpaare für OCS Mix (Y −40 und −72) mit Nut auf der Unterseite.
- **4 Knotenbleche**, 81 hoch und 70 tief, an jedem Ende eins vorn und eins hinten.
- **2 Träger** für Pi und Pico (Pi-Lochbild 58 × 49, Dome M2,5), hinten unten.
- **2 OCS-Steckhalter** (`wand_ocshalter.py`): Flansch 4 an der Wand mit 2 × M3 × 16 auf dem Raster.
  Darauf stehen Stützen Ø 5 bis an die Platinen-Rückseite, die Steckergehäuse (6 hoch) bleiben frei.
  - **Lo (3FRL 2.0):** 4 Zapfen Ø 3,4 stramm in die Löcher Ø 3,5 (quer 18,5, längs 72). Am J2-Ende sind es die Langlöcher, das Längenmaß ist also unkritisch.
    Die inneren runden Löcher (65,5) bleiben frei.
  - **Hi (3F 3.0):** Die Löcher habe ich nur vom Foto (Paar A 25 vom J2-Ende, Paar B versetzt bei 97 / 101). Die Platine ist gegenüber dem Foto um 180° gedreht (Alex):
    Die Tüllen zeigen links, das J2-Ende ist unten. Nur 1 Zapfen Ø 2,6 sitzt im Loch A an der Gegenkante,
    die anderen 3 Stützen sind flach. Zwei Randleisten mit Rastwulst (Z 143,5 … 153,5) halten die Platine. Einfach von vorn eindrücken.
- **T-Halter:** Flansch 40 × 24 mit Finnen über und unter dem T-Stück. Unten sitzt eine Gabel für den Abgang, sie hält das T in X und Y.
- **Kaufteil T-Stück 6 mm:** Arme etwa ±18, Abgang nach unten höchstens 15 lang, sonst stößt es auf OCS Mix.
- **Lage der OCS:** Tüllen- und Rohrachse auf Y −22. J2-Ende: hi unten, lo oben.
  - OCS hi: X −24 … −2, Z 68,5 … 193,5, Platine Y −14,5 … −16
  - OCS lo: X 1 … 25, Z 116 … 195, Platine Y −13,7 … −15,3
  - Maße von Alex: hi Rohr 12 + Platine 1,5 = 13,5, dickste Stelle mit Stecker 20, Tüllen 7 über der Kante; lo dickste Stelle mit Stecker 25, Tüllen 5,5 über der Kante.

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
| `development_wand_3_knotenbleche.3mf` | 4 Knotenbleche, 2 OCS-Steckhalter | 5 | 40 % | 6 / 5 | 3 | 55 °C | 2 h 44 | 112 g |

Summe etwa 13,5 h und 540 g. Wand und Grundplatte passen nicht zusammen auf ein Bett, weil die Wand das Bett allein füllt.

## Schrauben und Kaufteile

| Verbindung | Anzahl | Teil |
|---|---|---|
| Wand ↔ Grundplatte (von unten in den Randsteg, selbstschneidend) | 6 | M3 × 12 Zylinderkopf |
| Knotenblech ↔ Grundplatte (von unten, Mutter oben) | 8 | M3 × 12 + Mutter M3 |
| Knotenblech vorn ↔ Wand ↔ Knotenblech hinten | 4 | M3 × 20 + Mutter M3 |
| QV ↔ Wand (von hinten in die Ambosse, ersetzen die 4 äußeren M3 × 12) | 8 | M3 × 20 |
| Träger ↔ Wand (Mutter vorn) | 8 | M3 × 12 + Mutter M3 |
| OCS-Steckhalter, T-Halter ↔ Wand (von vorn, Mutter hinten) | 6 | M3 × 16 + Mutter M3 |
| Pi / Pico ↔ Träger (selbstschneidend) | 8 | M2,5 × 6 |
| T-Stück, OCS Mix (Mix durch die Grundplatte) | 4 | Kabelbinder 2,5 × 150 |
| T-Stück für Schlauch 6 mm | 1 | Kaufteil, Abgang ≤ 15 |

Summe: 22 × M3 × 12, 6 × M3 × 16, 12 × M3 × 20, 26 × Mutter M3, 8 × M2,5 × 6, 4 Kabelbinder, 1 T-Stück.

## Montage

1. Die Wand auf die Grundplatte stellen, mit dem Randsteg nach hinten. Von unten 6 × M3 × 12 in den unteren Randsteg schrauben.
2. Die 4 Knotenbleche ansetzen. Die Wand-Flansche mit 4 × M3 × 20 verbinden (durch vorn, Wand und hinten, Mutter hinten).
   Die Grundplatten-Flansche mit 8 × M3 × 12 von unten verschrauben, Mutter oben. Erst alles handfest anziehen, dann fest.
3. Die Träger hinten mit je 4 × M3 × 12 befestigen, Mutter vorn. Pi kommt links (von vorn gesehen) auf X −55, Pico rechts auf X +55.
4. Die Halter vorn mit M3 × 16 anschrauben, Mutter hinten:
   - OCS-Steckhalter Hi: X −15, Z 116 und 156
   - OCS-Steckhalter Lo: X +15, Z 126 und 156
   - T-Halter: X ±15, Z 56
5. Am QV die Getriebeplatte mit dem NEMA abnehmen und die 4 äußeren Amboss-Schrauben herausdrehen.
   Das QV von vorn mit dem Hohlrad durch den Ausschnitt an die Wand legen und 4 × M3 × 20 von hinten eindrehen. Danach das Getriebe von hinten wieder ansetzen.
   QV hi kommt links hin, QV lo rechts. Den unteren Schlauch etwa 15 mm über das Amboss-Ende hinaus stehen lassen.
6. Die OCS montieren:
   - Die Stecker (J2 usw.) vorher einstecken, danach kommt man schlecht dran.
   - OCS hi mit den Tüllen nach links und dem J2-Ende unten auf den Steckhalter Hi drücken, bis beide Rastwülste einschnappen.
   - OCS lo mit den Tüllen nach rechts und dem J2-Ende (Langlöcher) oben auf die 4 Zapfen des Steckhalters Lo stecken.
   - Die Bauteilseite zeigt zur Wand, das Rohr nach vorn.
   - Den unteren QV-Schlauch direkt auf den oberen Eingang schieben.
7. Das T-Stück mit dem Abgang nach unten in die Gabel des T-Halters setzen und festbinden.
   OCS Mix quer zur Wand auf die Grundplatte stellen, mit den Tüllen nach oben und dem Eingang hinten. Mit 2 Kabelbindern durch die Grundplatte festbinden.
8. Die Schläuche verlegen und zuschneiden wie in der Tabelle „Fluss“:
   - OCS hi Ausgang mit großem U-Bogen nach links unten und von unten in den linken T-Arm, OCS lo Ausgang außen herum zum rechten T-Arm.
   - Vom T-Abgang zum OCS Mix ein Muffenstück von 20 mm.
   - Die Ablass-Schläuche durch die Durchlässe nach hinten führen.

## Offen

- Das Lochbild des BTT SKR Pico ist nicht geprüft. Laut Händler entspricht es dem Pi (58 × 49). Vor dem Druck der Träger am Board nachmessen.
- Die Löcher des OCS hi (3F 3.0) stammen nur vom Foto (etwa ±0,7). Deshalb gibt es dort nur 1 Zapfen und die Rastleisten.
  Passt der Zapfen nicht, abknipsen. Die Leisten halten die Platine dann allein.
- Die Tüllenachse sitzt auf Rohrmitte (hi 6, lo 6,7 vor der Platine). Das ist aus Rohrdicke und Gesamthöhe gerechnet, nicht direkt gemessen.
- Die unteren QV-Schrauben (y −33,5) erreicht man nur bei abgenommenem Getriebe.
- Biegeradius: R 25 entspricht etwa 2 × Außendurchmesser. Silikon 6/12 hält das. Vorher einmal um einen Ø 50-Gegenstand biegen und prüfen, ob der Schlauch frei bleibt.
