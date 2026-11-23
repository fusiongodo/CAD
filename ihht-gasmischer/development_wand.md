# Development-Wand (Variante A „Senkrechte Wand“)

Stand 23.11.2026. Quelle `quelle/teile/wand_*.py` und `quelle/ihht/wand.py`, Onshape-Modul `development_wand.fs`,
Prüfung und Druckdateien `quelle/development_wand_3mf.py` → `druck/development_wand_{1,2,3}_*.3mf`.
Vorlage: ihht-pi `CAD/produktkasten-skizzen/development_wand_3d.html`, Funktion `A()`.

## Aufbau

- **Wand** 240 × 210 × 6, steht auf der Grundplatte. Hinten ein Randsteg (4 breit, 10 tief, unten 12 hoch), ein Quersteg bei Z 100
  und je ein Ringsteg um die QV-Ausschnitte. Die Vorderseite ist flach.
- **Lochraster** M3 (Ø 3,4), Abstand 10, Spalten X ±5 … ±105, Zeilen Z 26 … 206 (194 Bohrungen). Wo Stege oder Ausschnitte sind, fehlen Bohrungen.
- **QV-Aufnahmen** bei X ±58, Achse Z 165: Ausschnitt Ø 75 für das Hohlrad und ein Feld 91 × 41 für die 4 Getriebepfosten.
  Die Rückplatte des QV liegt vorn flach an. Gehalten wird es von 4 × M3 × 20 von hinten durch die Wand in die Ambosse.
  Das passt für Rahmen v2 + Getriebe v1 und für QV v3, weil beide dieselbe Rückplatte haben.
- **Grundplatte** 240 × 150 × 6 als L, Y −70 … 80. Alle Schrauben kommen von unten, die Köpfe sind versenkt.
- **4 Knotenbleche**, 81 hoch und 70 tief, an jedem Ende eins vorn und eins hinten. Die hinteren sind für den Randsteg ausgespart.
- **2 Träger** für Pi und Pico (Pi-Lochbild 58 × 49, Dome M2,5). Sie sitzen hinten auf dem Raster.
- **Module**: OCS hi (122) und OCS lo (82) vorn direkt unter den QVs (Z 85–116), Mischer und OCS Mix unten (Z 30–60).
  Pi und Pico sitzen hinten unten.

Prüfung (`development_wand_3mf.py`): Alle Teile sind wasserdicht und je ein Körper. Es gibt keine Überschneidung im Modul,
auch nicht mit den echten QV-v3-Teilen und den echten v2-Teilen (gen_rahmen + gen_getriebe) an beiden Plätzen.

## Druck (Bambu, 256 × 256, alle ohne Stützen, 0,20 / 4 Wände / 40 % / Brim 5)

1. `development_wand_1_wand.3mf`: Wand mit der Vorderseite nach unten.
2. `development_wand_2_grundplatte_traeger.3mf`: Grundplatte und 2 Träger.
3. `development_wand_3_knotenbleche.3mf`: 4 Knotenbleche, jeweils mit der Steg-Außenseite nach unten.

## Schrauben

| Verbindung | Anzahl | Teil |
|---|---|---|
| Wand ↔ Grundplatte (von unten in den Randsteg, selbstschneidend) | 6 | M3 × 12 Zylinderkopf |
| Knotenblech ↔ Grundplatte (von unten, Mutter oben) | 8 | M3 × 12 + Mutter M3 |
| Knotenblech vorn ↔ Wand ↔ Knotenblech hinten | 4 | M3 × 20 + Mutter M3 |
| QV ↔ Wand (von hinten in die Ambosse, ersetzen die 4 äußeren M3 × 12) | 8 | M3 × 20 |
| Träger ↔ Wand (Mutter vorn) | 8 | M3 × 12 + Mutter M3 |
| Pi / Pico ↔ Träger (selbstschneidend) | 8 | M2,5 × 6 |
| OCS hi / lo / Mix, Mischer (durch das Raster) | 8 | Kabelbinder 2,5 × 100 |

Summe: 22 × M3 × 12, 12 × M3 × 20, 20 × Mutter M3, 8 × M2,5 × 6, 8 Kabelbinder.

## Montage

1. Die Wand auf die Grundplatte stellen, mit dem Randsteg nach hinten. Von unten 6 × M3 × 12 in den unteren Randsteg schrauben.
2. Die 4 Knotenbleche ansetzen. Die Wand-Flansche mit 4 × M3 × 20 verbinden (durch vorn, Wand und hinten, Mutter hinten).
   Die Grundplatten-Flansche mit 8 × M3 × 12 von unten verschrauben, Mutter oben. Erst alles handfest anziehen, dann fest.
3. Die Träger hinten mit je 4 × M3 × 12 befestigen, Mutter vorn. Pi kommt links (von vorn gesehen) auf X −55, Pico rechts auf X +55.
   Die Boards mit M2,5 × 6 aufschrauben.
4. Am QV die Getriebeplatte mit dem NEMA abnehmen (4 Pfosten-Schrauben vorn) und die 4 äußeren Amboss-Schrauben herausdrehen.
   Das QV von vorn mit dem Hohlrad durch den Ausschnitt an die Wand legen und 4 × M3 × 20 von hinten in die Ambosse schrauben.
   Danach die Getriebeplatte von hinten ansetzen (Pfosten durch das Feld) und die 4 Pfosten-Schrauben von vorn eindrehen.
5. Die OCS und den Mischer vorn mit je 2 Kabelbindern durch das Raster befestigen.

## Offen

- Das Lochbild des BTT SKR Pico ist nicht geprüft. Laut Händler entspricht es dem Pi (58 × 49). Vor dem Druck der Träger am Board nachmessen.
- Die unteren QV-Schrauben (y −33,5) erreicht man nur bei abgenommenem Getriebe.
