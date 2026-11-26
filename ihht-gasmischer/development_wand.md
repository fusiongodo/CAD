# Development-Wand (Variante A „Senkrechte Wand“)

Stand 26.11.2026, Anordnung v3.4.

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

- **v2.4 (25.11.):** Die Rastleisten am OCS-Steckhalter Hi brachen beim Aufstecken ab (1,6 dick, quer zu den Schichten gedruckt).
  - Leisten jetzt 2,2 dick, Wulst bleibt 0,6 über der Platinenkante, Flansch X −26,4 … 0,4.
  - Druck auf der Seite (linke Leiste aufs Bett): Die Schichten laufen längs durch die Leisten. Die obere Leiste braucht Baumstützen (nur vom Bett).
  - Eigene Datei `druck/development_wand_ocshalter_hi.3mf`, 0,28 mm, 5 Wände, 50 %, etwa 25 min. Aus Platte 3 herausgenommen.

- **v2.5 (25.11., verworfen):** Der v2.4-Druck passt nicht auf die Platine. Das Lochbild war links/rechts vertauscht: Oben stand links der höhere Pfosten, und der Zapfen saß unten rechts.
  - Ursache: Die Korrektur vom 24.11. hatte das Foto-Lochbild um 180° gedreht. Richtig ist nur die Spiegelung oben/unten (u → L − u, v bleibt).
  - Jetzt steht oben links der tiefere Pfosten (Z 165,2) und rechts der höhere (Z 169,4). Der Zapfen sitzt unten links (A1, X −20,8). Steckerzonen gehen mit.

- **v2.6 (25.11.):** v2.5 zurückgenommen. Mit dem nur oben/unten gespiegelten Lochbild läge der Auslass des OCS hi oben statt unten, die Platine müsste verkehrt herum drauf.
  - Die 180°-Drehung aus v2.4 ist richtig. Der gedruckte v2.4-Halter passt, Nachdruck nicht nötig.

- **v2.7 (25.11.):** Beide OCS sitzen, aber halten nicht richtig.
  - Hi: Stützen und Clip waren zu niedrig, die Platine sitzt real weiter vorn (Steckergehäuse höher als 6). Jetzt 2 mm höher (`vor=2.0`), der Wulst wandert mit.
  - Lo: Die Zapfen Ø 3,4 brachen beim Abnehmen alle ab. Jetzt 4 × M2 von vorn durch Platine und Stütze (Ø 7). Die Mutter wird seitlich in einen Schlitz 1,2 unter der Platine geschoben. M2 × 6 reicht, bis M2 × 14 geht. Der Flansch ist jetzt X 0,6 … 26.
  - Beide in `druck/development_wand_ocshalter.3mf` (0,28 mm, ca. 44 min, Baumstützen nur fürs Hi). Die alte `development_wand_ocshalter_hi.3mf` ist gelöscht.

- **v2.8 (25.11.):** Das OCS hi sitzt real umgekehrt: Rohr zur Wand, Platine (Bauteilseite) vorn. Deshalb waren die Stützen zu kurz, das Rohr stieß an den Flansch.
  - Lochbild von vorn laut Alex: oben das Einzelloch, darunter zwei auf einer Höhe, weit unten links höher, rechts tiefer. Das sind die Fotodaten ohne Drehung. Die Drehungen aus v2.4/v2.5 und `vor` aus v2.7 sind raus.
  - Modell: `_ocs(..., rohr_wand=-17,5)`. Die Platine sitzt mit der Rohrseite bei Y −17,5, das Rohr hat 1 Luft zum Flansch (Y −4). Die Tüllenachse liegt bei Y −11,5, die Schlauch-Dummies bleiben auf Y −22, real versetzt der Schlauch.
  - Rohrbreite 10 statt 12,5 angenommen, weil die Stützen Ø 5 real daneben passten. Stützen 13,5 lang, Clip entsprechend länger.

- **v2.9 (25.11.):** Lo-Halter: Sechskanttaschen (SW 4,2, 3 tief) hinten im Flansch für versenkte M2-Muttern, Wunsch Alex.
  - Mit Mutter in der Tasche: M2 × 14. Die Spitze bleibt in der Tasche, nicht an der Wand.
  - Der seitliche Mutterschlitz unter der Platine bleibt für kurze Schrauben (M2 × 6 … 10).

- **v3.1 (25.11.):** Die großen OCS (hi und Mix) sitzen mit dem Rohr nach vorn, Bauteilseite (Stecker) zur Wand. Foto von Alex am Aufbau: Tüllen links, J5 oben, J2 unten.
  - Beobachtung am v3.0-Druck: Der Clip hält die Platine perfekt. Der Halter war aber für „Rohr zur Wand“ gebaut (v2.8). Mit dem Rohr vorn stand das OCS zu weit vor der Wand,
    der Schlauch musste seitlich biegen, und der Halter passte nur verdreht, dann lagen die Schraublöcher neben dem Raster.
  - Modell: `_ocs(...)` ohne `rohr_wand`, Lochbild und Stecker um die Tüllenachse gekippt (u → 125 − u). Platine Y −14,5 … −16, Tüllenachse Y −22 wie QV-Schlauch und lo.
    Die Stützen sind 3 kürzer (10,5 statt 13,5), der Halter baut 18,4 statt 21,4 hoch. Schrauben unverändert auf dem Raster: Hi (−15, 116) / (−15, 156), Mix (75, 96) / (95, 66).
  - T-Stück: Alle drei Anschlüsse liegen jetzt auf Y −22. Das v3.0-T-Stück (Mix und hi auf Y −11,5) passt nicht mehr, neu drucken.
  - Durchfluss: Mit dieser Lage liegt der Pfeil-Einlass des OCS unten, das Gas kommt aber oben an. Die O2-Messung geht in beide Richtungen, ein Flusswert hätte das falsche Vorzeichen. Nicht getestet.
  - Druck: `druck/development_wand_hi_mix_t.3mf` (Halter Hi, Halter Mix, T-Stück; 0,20 mm, 5 Wände, 50 %, Baumstützen, 1 h 17). Der Lo-Halter bleibt.

- **Hohlrad mit Steckgriff (26.11.):** Alex will das Hohlrad von Hand mit mehr Kraft drehen können (z. B. Exzenter gegen einen Schlauch drücken, um den anderen zu bewegen). Griff abnehmbar, am Rad nur Löcher.
  - Platz: außen am Hohlrad sind nur 1,7 frei bis zu den Pfosten, innen läuft die Ritzelnabe, vorn sitzt die Wand. Darum ein Kragen hinten am Hohlrad (R 34,4 bis 37,6, z −17,6 bis −28,5), hinter der Wand.
  - 6 Paare Schlitze alle 60°, je Paar 2 parallele Schlitze Ø 6,2 im Abstand 20, nach hinten offen. Dazu 1 Loch für die M3 im Hals.
  - Griff: Gabel mit 2 Zapfen Ø 5,8, 4,6 dick, 115 lang, Griffteil 13 dick nach hinten. Er steckt gerade hinein, das Moment geht über beide Zapfen.
  - Im Modell geprüft: Kragen frei gegen Getriebeplatte, Rahmen, Motor. Griff schwenkt je Steckstelle etwa ±40° (dann stehen die Pfosten im Weg), danach ein Paar weiter stecken.
  - `quelle/qv_hohlrad_griff_3mf.py` → `druck/qv_hohlrad_griff.3mf`, ohne Stützen, 2 h 54. Am Teil nicht getestet. Offen: hält die 3,2 dicke Kragenwand die Handkraft?
- **v3.4 (26.11., aktuell):** T-Stück druckt ganz ohne Stützen.
  - Anlass (Alex): v3.3 mit Stützen gedruckt, der Slicer hat auch in die Kanäle Stützen gesetzt, nicht brauchbar.
  - Drucklage umgedreht: Vorderseite aufs Bett. Die 3 Tüllen liegen flach auf dem Bett, mit 0,2 tiefer Flachstelle unten (an den Wellen 0,45). Unter der Bohrung bleiben 0,8 Wand.
  - Block 2,2 flacher (vorn Y −25,3). Unter den 2 Schraubstellen stehen Beine bis aufs Bett. Flansch nur noch über Beinen und Block, 16,5 Brücke über dem hi-Schlauch.
  - Schrauben: M3 × 16 von hinten durch die Wand, Mutter von vorn in den Sechskant-Schacht (SW 5,9, 21 tief).
  - Anschlüsse, Bohrung 5 und Tüllen wie v3.3. Slicer ohne Stützen und ohne Brim: 40 min, 14 g. Am Teil nicht getestet.
- **v3.3 (26.11.):** T-Stück mit größeren Luftkanälen (Alex: „innen 5, außen 7, mit Wellen bis 7,5“).
  - Bohrung 5 statt 4. Tüllen Ø 7 statt Hals 6,6 / Wulst 7,5 / Spitze 6,2. Je Tülle 3 Sägezahn-Wellen bis Ø 7,5, steile Flanke zum Block, Spitze auf 6,2 angefast.
  - Block, Flansch, Schrauben und Lage der Anschlüsse unverändert. Wände um die Bohrung: 3,0 vorn und oben, 2,0 unten.
  - Druck: `druck/development_wand_t.3mf` (nur das T-Stück, mit Baumstützen, 33 min). Die älteren Platten mit T-Stück sind Stand v3.1.

- **v3.2 (25.11.):** Zurück auf „Rohr zur Wand, Stecker vorn“ für hi und Mix, aber mit der Tülle auf Y −22.
  - Beobachtung am v3.1-Druck (Foto von oben): Der Mix-Halter (U-Ausschnitt, Löcher diagonal) passt aufs Raster. Alex hat das große OCS mit dem Rohr zur Wand eingeclipst,
    nicht wie auf dem Foto zu v3.1 mit dem Rohr vorn. Im v3.1-Halter (Stützen 10,5) liegt das Rohr dann fast am Flansch, die Tülle sitzt deutlich tiefer als beim lo.
  - Der Clip nimmt die Platine in beiden Lagen auf. Deshalb ging es seit v2.8 dreimal hin und her. Festgelegt: Rohr zur Wand, Stecker vorn (Kabel steckbar), J2 oben, Pfeil-Einlass oben.
  - Modell: `rohr_wand = YS − 6 = −28`, Lochbild wie v2.8. Platine Y −28 … −29,5, Stützen 24 lang mit je einem 8 langen Steg zur Platinenmitte (`fuss`), Halter baut 31,9 hoch.
    Unter dem Rohr sind 12 frei, die Schraubenköpfe passen. Schrauben und Flansche unverändert. Das T-Stück von v3.1 bleibt (alle Anschlüsse Y −22).
  - Druck: `druck/development_wand_hi_mix.3mf` (nur Halter Hi und Mix).

- **Quetschventil als echte Teile in der Wand (25.11., Alex: Architekturfehler):** Die Wand zeigte die Ventile nur als Platzhalter,
  und das gebaute Ventil lag als eigene Generatoren im ihht-pi-Ordner mit von Hand kopiertem FeatureScript.
  - Die Quelle des gebauten Ventils (Rahmen v2, Getriebe v1, Gegenlager 6200, Welle mit Zapfen, Distanzring) steht jetzt im Builder: `quelle/qv2/gen_*.py`.
    `ihht/qv2.py` rechnet sie in Builder-Teile um, `teile()` liefert das ganze Ventil an einer beliebigen Stelle.
  - Zwei Abnehmer derselben Quelle: Modul „Quetschventil Gegenlager 6200 (Rahmen v2)“ (`teile/qv2_teile.py`) und die Development-Wand (`teile/wand_qv.py`, links und rechts).
    Eine Änderung in `qv2/` wirkt nach `build.py` in beiden. Die Dummies `dwQVL` / `dwQVR` sind entfallen.
  - Die Prüfung der Wand läuft jetzt mit den echten Ventil-Teilen (je 14, mit Lagern und Motor) gegen Wand, Halter, OCS und Schläuche. Der Umweg über den ihht-pi-Pfad ist raus.
  - Die Dateien `gen_*.py` im ihht-pi-Ordner laden nur noch die Quelle aus dem CAD-Repo, damit die Druck-Skripte dort weiter laufen.
  - Eingebaute Exzentrizität im Modell: 3,5 (`E_BAU` in `ihht/qv2.py`). Das QV v3 (M8-Kern) bleibt ein eigenes Modul und ist nicht gebaut.

- **Welle mit M8-Kern (26.11., Wunsch Alex):** Die Welle mit angedrucktem Zapfen bekommt auf der Hauptachse eine M8 × 70 (DIN 933) als Stahlkern.
  - Lager bleiben: 2 × 6203 auf dem Exzenter, 6200 vorn im Steg. Kein 608, kein 6905 (das wäre QV v3, die Lager sind nicht da).
  - Die Schraube wird von vorn durch das 6200 gesteckt. Bohrung 8,3 frei durch den Exzenter, im 10-mm-Zapfen 8,0 (Hülse mit 1,0 Wand zwischen M8 und 6200), hinten 12 lang Kernloch 7,2 (schneidet sich selbst ein).
  - Der Zapfen ist damit nur noch eine dünne Hülse um die Schraube. Im Slicer geprüft: Die Schichten des Zapfens werden voll gedruckt (Arachne).
  - Der Kopf liegt auf dem Innenring des 6200 und spannt 6200, Distanzring und beide 6203 gegen die Wellenschulter. Nur handfest anziehen, das Gewinde ist Kunststoff.
  - Grenze: geht nur bis e 3,7. Bei e 3,5 bleiben 0,8 Wand neben der Bohrung im Exzenter und im Zapfen. Wellen mit e 4,5 / 5,0 behalten das M3-Kernloch.
  - Nur die Welle ist neu (`QV_M8` in `qv2/gen_getriebe.py`, Standard an). Im Modell steht die Schraube als Dummy. Ungetestet.

- **Quetschventile am Aufbau (25.11., Beobachtung Alex):**
  - Aufbau: Ein QV läuft mit Getriebeplatte und Hohlrad der Vorversion (vom 24.11.), das andere ist komplett der Nachtdruck vom 24./25.11.
    Beide sind Rahmen v2 mit Steg 6200 und Gegenlagerscheibe (ihht-pi `CAD/quetschventil`, VERSUCHE.md V8), nicht QV v3. Welle (e) und Hülse nicht notiert.
  - Beobachtung: Das QV mit dem alten Getriebe dichtet nicht ganz. Das neue dichtet gerade so, geschätzt 97 %. Vermutung Alex: Druckstreuung, Exzentrizität etwas zu klein.
  - Der Steg war mit zu kurzen Schrauben befestigt. M3 × 20 passt im Durchmesser, die Bohrung im Amboss (12 tief) ist aber zu kurz. Alex bohrt nach.
  - Geändert: Die 4 Eckbohrungen der Ambosse gehen jetzt durch (`qv_amboss.py` und ihht-pi `gen_rahmen.py`). Noch nicht gedruckt.
  - Nachtrag: Auf einer Seite fehlen 1 bis 2 mm. Die Gegenlagerscheibe sitzt schräg auf ihren 2 Stiften, das Lager weicht in Kraftrichtung aus (Alex).
    Folge: Welle und Lagerzapfen als ein Teil, siehe ihht-pi VERSUCHE.md V9 (`druck/welle_zapfen_6200.3mf`). Noch nicht gedruckt.

- **v3.0 (25.11.):** OCS Mix ist ein zweiter großer 3F 3.0, nicht der kleine 3FRL. Das T-Stück wird gedruckt statt gekauft (Alex).
  - **OCS Mix** steht hochkant an der Wand rechts unter dem QV lo: X 73 … 95, Z 27 … 152, gleiche Lage wie hi (Rohr zur Wand, Tüllen nach links).
    Einlass oben Z 132, Auslass unten Z 47. Der Platz auf der Grundplatte ist frei, die Kabelbinder-Schlitze bleiben.
  - **Steckhalter Mix:** wie Hi (1 Zapfen, 3 Stützen, 2 Rastleisten). Unterschiede: Die 2 Schrauben sitzen neben dem Rohr auf (75, 96) und (95, 66),
    weil ein Zylinderkopf (3 hoch) unter dem Rohr nur 1,5 Luft hätte. Der Flansch hat einen U-Ausschnitt um die Träger-Mutter (75, 76).
  - **T-Stück als Druckteil** (`wand_halter.py`): Block mit Bohrungen Ø 4 zwischen lo-Auslass und Mix-Einlass, Flansch mit 2 × M3 auf (35, 116) und (65, 116).
    lo kommt von links (Y −22, Z 135), Mix geht nach rechts (Y −11,5, Z 132), hi kommt von unten (X 48,25, Y −11,5). Der Block gleicht den Versatz in Y und Z aus.
    3 Tüllen, 10 lang: Hals Ø 6,6, Wulst Ø 7,5, Spitze Ø 6,2. Der Schlauch 6 innen sitzt stramm, Kabelbinder hinter den Wulst.
  - **Schläuche:** lo → T und T → Mix sind gerade Muffen (je 2 mm Luft zwischen Tüllenspitzen). hi → T läuft mit U-Bogen nach links unten,
    waagerecht auf Z 38,5 unter hi und lo durch, steigt bei X 36 und geht mit einer S-Kurve von unten in den T-Abgang (216 mm frei, R 25).
    Der Patientenschlauch geht vom Mix-Auslass mit einem Bogen nach vorn weg.
  - T-Halter und T-Dummy sind entfallen. Der gedruckte T-Halter (Z 44 … 68, Mitte) muss ab, dort läuft jetzt der hi-Schlauch.
  - Druck: `druck/development_wand_mix_t.3mf` (Halter Mix auf der Seite, T-Stück mit dem Flansch unten, 0,20 mm, 5 Wände, 50 %, Baumstützen überall, 58 min).
    Alle vier Teile (Halter Hi, Lo, Mix und T-Stück) in einem Druck: `druck/development_wand_ocs_alle.3mf`, gleiche Einstellungen, 1 h 45, 53 g. Hi und Mix sind beide ungetestet (Wunsch Alex, Risiko bekannt).
  - **Kabel (nur geplant, nichts im CAD):** J2 = +12 V, RXD, TXD, GND, UART 9600 Baud (Datenblatt). Anschluss am SKR Pico, dort gibt es 12 V.
    Die 15-cm-Kabel gehen durch ein freies Rasterloch neben J2 (Kontakte dafür einzeln aus dem Gehäuse ziehen).
    Geschätzte Wege bis Pico-Mitte: Mix etwa 14 cm, lo etwa 16 cm, hi etwa 20 cm. Für hi und lo reicht das Kabel also nicht sicher.
    Vor dem Anschließen den TXD-Pegel messen (bei 5 V Spannungsteiler).

## Fluss (X quer, Z Höhe über Tisch)

| Strecke | Tüllen / Höhen | frei | Bogen | Zuschnitt ca. |
|---|---|---|---|---|
| Zufuhr außen → QV | hi links, lo rechts, waagerecht durch das QV | – | – | – |
| QV oben (Ablass) → Durchlass | Z 222,5 → Durchlass Ø 18 bei X ±13,5 | 36 | R 25 | 60 + hinten nach Bedarf |
| QV unten → OCS-Eingang | Z 173,5, gerade, Tülle hi X −31, lo X +30,5 | 6 | gerade | QV-Schlauch 15 länger lassen |
| OCS hi Ausgang → T | Z 88,5 → U-Bogen links bis X −59, waagerecht Z 38,5, hoch bei X 36, S-Kurve in den T-Abgang X 48,25 / Z 117,5 | 216 | R 25 | 235 |
| OCS lo Ausgang → T | Z 135, gerade, Tülle X 30,5 → T-Tülle X 32,5 | 2 | gerade | 18 (Muffe) |
| T → OCS Mix Eingang | Z 132, gerade, T-Tülle X 64 → Tülle X 66 | 2 | gerade | 19 (Muffe) |
| OCS Mix Ausgang → Patient | Z 47, Tülle X 66, Bogen nach vorn | – | R 25 | nach Bedarf |

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
- **3 OCS-Steckhalter** (`wand_ocshalter.py`, Mix siehe v3.0): Flansch 4 an der Wand mit 2 × M3 × 16 auf dem Raster.
  Darauf stehen Stützen Ø 5 bis an die Platinen-Rückseite, die Steckergehäuse (6 hoch) bleiben frei.
  - **Lo (3FRL 2.0):** 4 Zapfen Ø 3,4 stramm in die Löcher Ø 3,5 (quer 18,5, längs 72). Am J2-Ende sind es die Langlöcher, das Längenmaß ist also unkritisch.
    Die inneren runden Löcher (65,5) bleiben frei.
  - **Hi (3F 3.0):** Die Löcher habe ich nur vom Foto (Paar A 25 vom J2-Ende, Paar B versetzt bei 97 / 101). Die Platine ist gegenüber dem Foto um 180° gedreht (Alex):
    Die Tüllen zeigen links, das J2-Ende ist unten. Nur 1 Zapfen Ø 2,6 sitzt im Loch A an der Gegenkante,
    die anderen 3 Stützen sind flach. Zwei Randleisten mit Rastwulst (Z 143,5 … 153,5) halten die Platine. Einfach von vorn eindrücken.
- **T-Stück (Druckteil):** siehe v3.0. Ersetzt T-Halter und Kaufteil.
- **Lage der OCS:** Tüllen- und Rohrachse auf Y −22. J2-Ende: hi unten, lo oben.
  - OCS hi: X −24 … −2, Z 68,5 … 193,5, Platine Y −14,5 … −16 (seit v3.1 wieder Rohr vorn)
  - OCS lo: X 1 … 25, Z 116 … 195, Platine Y −13,7 … −15,3
  - OCS Mix: X 73 … 95, Z 27 … 152, Platine Y −14,5 … −16 (wie hi)
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
| OCS-Steckhalter (3), T-Stück ↔ Wand (von vorn, Mutter hinten) | 8 | M3 × 16 + Mutter M3 |
| Pi / Pico ↔ Träger (selbstschneidend) | 8 | M2,5 × 6 |
| Schläuche auf den Tüllen (OCS und T-Stück) | 9 | Kabelbinder 2,5 |

Summe: 22 × M3 × 12, 8 × M3 × 16, 12 × M3 × 20, 28 × Mutter M3, 8 × M2,5 × 6, 9 Kabelbinder.

## Montage

1. Die Wand auf die Grundplatte stellen, mit dem Randsteg nach hinten. Von unten 6 × M3 × 12 in den unteren Randsteg schrauben.
2. Die 4 Knotenbleche ansetzen. Die Wand-Flansche mit 4 × M3 × 20 verbinden (durch vorn, Wand und hinten, Mutter hinten).
   Die Grundplatten-Flansche mit 8 × M3 × 12 von unten verschrauben, Mutter oben. Erst alles handfest anziehen, dann fest.
3. Die Träger hinten mit je 4 × M3 × 12 befestigen, Mutter vorn. Pi kommt links (von vorn gesehen) auf X −55, Pico rechts auf X +55.
4. Die Halter vorn mit M3 × 16 anschrauben, Mutter hinten:
   - OCS-Steckhalter Hi: X −15, Z 116 und 156
   - OCS-Steckhalter Lo: X +15, Z 126 und 156
   - Steckhalter Mix: (75, 96) und (95, 66)
   - T-Stück: X 35 und 65, Z 116. Erst anschrauben, wenn die Muffen sitzen (Schritt 7).
5. Am QV die Getriebeplatte mit dem NEMA abnehmen und die 4 äußeren Amboss-Schrauben herausdrehen.
   Das QV von vorn mit dem Hohlrad durch den Ausschnitt an die Wand legen und 4 × M3 × 20 von hinten eindrehen. Danach das Getriebe von hinten wieder ansetzen.
   QV hi kommt links hin, QV lo rechts. Den unteren Schlauch etwa 15 mm über das Amboss-Ende hinaus stehen lassen.
6. Die OCS montieren:
   - Die Stecker (J2 usw.) vorher einstecken, danach kommt man schlecht dran.
   - OCS hi mit den Tüllen nach links und dem J2-Ende unten auf den Steckhalter Hi drücken, bis beide Rastwülste einschnappen.
   - OCS lo mit den Tüllen nach rechts und dem J2-Ende (Langlöcher) oben auf die 4 Zapfen des Steckhalters Lo stecken.
   - Die Bauteilseite zeigt zur Wand, das Rohr nach vorn.
   - Den unteren QV-Schlauch direkt auf den oberen Eingang schieben.
7. OCS Mix wie hi auf den Steckhalter Mix drücken (Tüllen nach links, Einlass oben).
   Auf beide waagerechten T-Tüllen je eine Muffe schieben. Das lose T-Stück erst auf die lo-Tülle, dann auf die Mix-Tülle stecken
   (notfalls den Mix dafür noch einmal ausclipsen). Danach das T-Stück anschrauben.
8. Die Schläuche verlegen und zuschneiden wie in der Tabelle „Fluss“:
   - OCS hi Ausgang mit U-Bogen nach links unten, unter hi und lo durch nach rechts und von unten auf den T-Abgang.
   - Patientenschlauch vom Mix-Auslass (unten) nach vorn.
   - Die Ablass-Schläuche durch die Durchlässe nach hinten führen.
   - Jede Tülle mit einem Kabelbinder sichern (die OCS-Tüllen sind für den 6-mm-Schlauch etwas zu dünn).

## Offen

- Hi-Halter: Die 2 Schraubenköpfe (X −15) liegen unter dem Rohr, dort ist nur 1,5 Luft. Beim Test prüfen. Falls es klemmt, Schrauben wie beim Mix neben das Rohr.
- Muffen am T-Stück: nur 2 mm Luft zwischen den Tüllenspitzen. Ob sich das montieren lässt, zeigt der Aufbau.
- Kabel hi und lo zum Pico sind mit 15 cm zu kurz (siehe v3.0).
- Das Lochbild des BTT SKR Pico ist nicht geprüft. Laut Händler entspricht es dem Pi (58 × 49). Vor dem Druck der Träger am Board nachmessen.
- Die Löcher des OCS hi (3F 3.0) stammen nur vom Foto (etwa ±0,7). Deshalb gibt es dort nur 1 Zapfen und die Rastleisten.
  Passt der Zapfen nicht, abknipsen. Die Leisten halten die Platine dann allein.
- Die Tüllenachse sitzt auf Rohrmitte (hi 6, lo 6,7 vor der Platine). Das ist aus Rohrdicke und Gesamthöhe gerechnet, nicht direkt gemessen.
- Die unteren QV-Schrauben (y −33,5) erreicht man nur bei abgenommenem Getriebe.
- Biegeradius: R 25 entspricht etwa 2 × Außendurchmesser. Silikon 6/12 hält das. Vorher einmal um einen Ø 50-Gegenstand biegen und prüfen, ob der Schlauch frei bleibt.
