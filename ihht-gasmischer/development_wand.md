# Development-Wand (Variante A „Senkrechte Wand“)

Stand 23.11.2026 (Anordnung v2). Quelle `quelle/teile/wand_*.py` und `quelle/ihht/wand.py`, Onshape-Modul `development_wand.fs`,
Prüfung und Druckdateien `quelle/development_wand_3mf.py` → `druck/development_wand_{1,2,3}_*.3mf`.
Vorlage: ihht-pi `CAD/produktkasten-skizzen/development_wand_3d.html`, Funktion `A()`.

## Verlauf

- **v1:** QV bei X ±58, OCS waagerecht unter den QVs, Wand 240 × 210. Alex hat das verworfen, weil die Schläuche kreuz und quer liefen und das Y-Stück keinen Platz hatte.
- **v2 (aktuell):** Das Gas fließt von außen nach innen und von oben nach unten, kein Schlauch kreuzt einen anderen.
  - QV bei X ±73.
  - Wand 246 × 246, das ist mit Brim 3 das Maximum für die Druckplatte.
  - OCS hochkant in der Mitte, Y-Stück und OCS Mix darunter.

## Fluss (X quer, Z Höhe; Tüllen zeigen nach vorn)

| Strecke | Tüllen / Höhen | Schlauch ca. |
|---|---|---|
| Zufuhr außen → QV | hi links, lo rechts, waagerecht durch das QV | – |
| QV oben (Ablass) → 2 Durchlässe Ø 14 bei X ±10 | Z 222,5 | 75 mm bis zur Wand, dann nach hinten |
| QV unten → OCS-Eingang (oben) | Z 173,5 (QV-Schlauch und OCS-Eingang auf gleicher Höhe), OCS hi X −12,5, OCS lo X +12,5 | je 50 |
| OCS hi Ausgang → Y | Z 105,2 → Y-Eingang Z 77 | 50 |
| OCS lo Ausgang → Y | Z 135 → Y-Eingang Z 77 | 80 |
| Y-Ausgang → OCS Mix Eingang | Z 47 → X −18 / Z 28,5 | 40 |
| OCS Mix Ausgang → Patient | X +20,5 / Z 28,5, nach rechts unten | – |

Die Längen sind grob gerechnet: Mittellinie plus Bogen. Beim Ablängen etwa 10 mm zugeben.

## Aufbau

- **Wand** 246 × 246 × 6, steht auf der Grundplatte. Hinten ein Randsteg (4 breit, 10 tief, unten 12 hoch), ein Quersteg bei Z 140
  und je ein Ringsteg um die QV-Ausschnitte. Die Vorderseite ist flach.
- **Lochraster** M3 (Ø 3,4), Abstand 10, Spalten X ±5 … ±105, Zeilen Z 26 … 236 (280 Bohrungen). Wo Stege, Ausschnitte oder Durchlässe sind, fehlen Bohrungen.
- **QV-Aufnahmen** bei X ±73, Achse Z 198: Ausschnitt Ø 75 für das Hohlrad und ein Feld 91 × 41 für die 4 Getriebepfosten.
  Die Rückplatte des QV liegt vorn flach an. Gehalten wird es von 4 × M3 × 20 von hinten durch die Wand in die Ambosse.
  Das passt für Rahmen v2 + Getriebe v1 und für QV v3, weil beide dieselbe Rückplatte haben.
  Getriebeplatte und NEMA reichen hinten bis X ±118.
- **2 Ablass-Durchlässe** Ø 14 bei X ±10, Z 222,5, je einer für einen Schlauch 6/12.
- **Grundplatte** 246 × 150 × 6 als L, Y −70 … 80. Alle Schrauben kommen von unten, die Köpfe sind versenkt.
- **4 Knotenbleche**, 81 hoch und 70 tief, an jedem Ende eins vorn und eins hinten. Die hinteren sind für den Randsteg ausgespart.
- **2 Träger** für Pi und Pico (Pi-Lochbild 58 × 49, Dome M2,5). Sie sitzen hinten unten auf dem Raster.
- **4 Modulhalter**, alle dasselbe Teil: L-Winkel 60 lang mit 2 × M3 auf dem Raster. Die Finne ist 30 tief und hat 4 Schlitze für Kabelbinder.
  - **Hi** und **Lo:** Die Finne steht senkrecht außen neben dem OCS, das OCS hängt mit 2 Kabelbindern daran.
  - **Y** und **Mix:** Die Finne liegt waagerecht, Y-Stück bzw. OCS Mix liegen darauf.
  - Die Schlitze sitzen bei 8 / 22 / 38 / 52 entlang der Finne. Nimm jeweils 2, die nicht auf Höhe einer Tülle liegen: hi 8 + 38, lo 8 + 38, Y und Mix 22 + 38.

Prüfung (`development_wand_3mf.py`): Alle Teile sind wasserdicht und je ein Körper. Es gibt keine Überschneidung im Modul,
auch nicht mit den echten QV-v3-Teilen und den echten v2-Teilen (gen_rahmen + gen_getriebe) an beiden Plätzen.
Alle Schrauben der Knotenbleche, Träger und Halter sitzen auf Rasterbohrungen.

## Druck (Bambu, 256 × 256, alle ohne Stützen, 0,20 / 4 Wände / 40 %)

1. `development_wand_1_wand.3mf`: Wand mit der Vorderseite nach unten, Brim 3. Sie füllt die Platte.
2. `development_wand_2_grundplatte_traeger.3mf`: Grundplatte, 2 Träger und die Halter Y und Mix, Brim 5.
3. `development_wand_3_knotenbleche.3mf`: 4 Knotenbleche (Steg-Außenseite unten) und die Halter Hi und Lo, Brim 5.

## Schrauben

| Verbindung | Anzahl | Teil |
|---|---|---|
| Wand ↔ Grundplatte (von unten in den Randsteg, selbstschneidend) | 6 | M3 × 12 Zylinderkopf |
| Knotenblech ↔ Grundplatte (von unten, Mutter oben) | 8 | M3 × 12 + Mutter M3 |
| Knotenblech vorn ↔ Wand ↔ Knotenblech hinten | 4 | M3 × 20 + Mutter M3 |
| QV ↔ Wand (von hinten in die Ambosse, ersetzen die 4 äußeren M3 × 12) | 8 | M3 × 20 |
| Träger ↔ Wand (Mutter vorn) | 8 | M3 × 12 + Mutter M3 |
| Modulhalter ↔ Wand (von vorn, Mutter hinten) | 8 | M3 × 16 + Mutter M3 |
| Pi / Pico ↔ Träger (selbstschneidend) | 8 | M2,5 × 6 |
| OCS hi / lo / Mix, Y-Stück an den Haltern | 8 | Kabelbinder 2,5 × 150 |

Summe: 22 × M3 × 12, 8 × M3 × 16, 12 × M3 × 20, 28 × Mutter M3, 8 × M2,5 × 6, 8 Kabelbinder.

## Montage

1. Die Wand auf die Grundplatte stellen, mit dem Randsteg nach hinten. Von unten 6 × M3 × 12 in den unteren Randsteg schrauben.
2. Die 4 Knotenbleche ansetzen. Die Wand-Flansche mit 4 × M3 × 20 verbinden (durch vorn, Wand und hinten, Mutter hinten).
   Die Grundplatten-Flansche mit 8 × M3 × 12 von unten verschrauben, Mutter oben. Erst alles handfest anziehen, dann fest.
3. Die Träger hinten mit je 4 × M3 × 12 befestigen, Mutter vorn. Pi kommt links (von vorn gesehen) auf X −55, Pico rechts auf X +55.
   Die Boards mit M2,5 × 6 aufschrauben.
4. Die 4 Modulhalter vorn mit je 2 × M3 × 16 anschrauben, Mutter hinten.
   - Hi: Löcher X −15 / Z 96 und 126, Finne links.
   - Lo: Löcher X +15 / Z 126 und 156, Finne rechts.
   - Y: Löcher X ±15 / Z 56.
   - Mix: Löcher X ±15 / Z 26.
5. Am QV die Getriebeplatte mit dem NEMA abnehmen (4 Pfosten-Schrauben vorn) und die 4 äußeren Amboss-Schrauben herausdrehen.
   Das QV von vorn mit dem Hohlrad durch den Ausschnitt an die Wand legen und 4 × M3 × 20 von hinten in die Ambosse schrauben.
   Danach die Getriebeplatte von hinten ansetzen (Pfosten durch das Feld) und die 4 Pfosten-Schrauben von vorn eindrehen.
   QV hi kommt links hin, QV lo rechts.
6. Die Module an die Halter binden:
   - OCS hi und OCS lo hochkant, Eingang oben, Tüllen nach vorn.
   - Das Y-Stück mit den Eingängen nach oben auf den Halter Y.
   - OCS Mix waagerecht auf den Halter Mix, Eingang links, Ausgang rechts.
7. Die Schläuche verlegen wie in der Tabelle „Fluss“. Die Ablass-Schläuche gehen durch die Durchlässe nach hinten.

## Offen

- Das Lochbild des BTT SKR Pico ist nicht geprüft. Laut Händler entspricht es dem Pi (58 × 49). Vor dem Druck der Träger am Board nachmessen.
- Die unteren QV-Schrauben (y −33,5) erreicht man nur bei abgenommenem Getriebe.
- Das Y-Stück ist bisher nur ein Bauraum-Dummy (30 × 30 × 15).
- Die Tüllenlage der OCS stammt aus den Datenblatt-Zeichnungen (hi ±5 Toleranz). Die Höhe der Halter Hi/Lo deckt das ab.
