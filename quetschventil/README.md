# Exzenter-Quetschventil

Stand: 21.11.2026 (v1). Onshape-FeatureScript, ein Feature „Exzenter-Quetschventil v1“ mit Dialog-Parametern.
Teile und Planung: ihht-pi `Materialliste/exzenter-quetschventil.md`, `docs/exzenter-quetschventil/offene-punkte.html`.

| Datei | Was |
|---|---|
| `quelle/quetschventil_v1.fs` | Rahmen mit beiden Ambossen in einem Teil (NEMA-17-Aufnahme) als Variantenreihe in der Quetschhöhe, Steg vorn (Langlöcher, passt auf alle Rahmen), Exzenter-Reihe in der Exzentrizität, Griffscheibe mit Knauf, Lager-Dummies |

## Auslegung v1

- Lager 2 × 6203-2RS (HARY, 17 × 40 × 12) nebeneinander auf dem Exzenterzapfen, direkt auf der NEMA-Welle.
- NEMA-Bohrung nach Konsens (ihht-pi `CAD/passungen`, 21.11.2026): Grundbohrung **locker** Ø 5,50 / Abflachung 2,25 + **genau 2 dünne Quetschstege** 0,45 / 0,2 (je eine Schicht) auf der Abflachung. Der frühere Presssitz Ø 5,26 / 2,10 + Stege alle 2 mm ist beim Abziehen zersplittert; ganz ohne Stege wackelt es.
- Exzenter als Variantenreihe im Zapfen-Ø (Standard 16,8 / 16,9 / 17,0 / 17,1), Kennung 1–4 Punkte auf der Unterseite. Variante 1 in Einbaulage, die anderen darunter.
- Ambosse: je 2 Kabelbinder-Schlitzpaare (4 × 1,8 mm) links und rechts neben den Führungsstegen, für Kabelbinder bis 3,6 mm Breite.
- Schlauch 6/12 Silikon, oben und unten, waagerecht. Exzentrizität 3,5, Quetschhöhe 5 → zu 5, offen 12, Neutralstellung beide 8,5 (halber Weg). Die Summe beider Spalte ist konstant (17), deshalb geht „Ambossabstand = 40 + 1 × Schlauch-Ø“ (Summe 12) nicht: dann wäre der eine nie offen, wenn der andere zu ist.
- Ambossfläche 28,5 mm von der Motorachse (20 + 3,5 + 5), Ambossabstand 57. Führungsstege für den Schlauch nur außerhalb der Lagerbahn (|x| ≥ 26), Innenabstand 15,6 (flach gedrückt ≈ 14,1).
- Drehmoment: Hebelarm beim Schließen = e · cos φ; 17 Ncm entsprechen ≈ 49 N (Spalt 8), 69 N (Spalt 6), 94 N (Spalt 5,5).

## Testreihe (Stand 21.11.)

| Rahmen | Punkte | Quetschhöhe | Überpressung (6/12, 2 × Wand = 6) | offener Spalt |
|---|---|---|---|---|
| V1 | 1 | 5,5 | 0,5 | 12,5 |
| V2 | 2 | 5,0 | 1,0 | 12,0 |
| V3 | 3 | 4,5 | 1,5 | 11,5 |
| V4 | 4 | 4,0 | 2,0 | 11,0 |

Exzenter (Exzentrizität 3,5) für alle Rahmen gleich, Zapfen-Ø als eigene Reihe 16,8 / 16,9 / 17,0 / 17,1.

## Montage

Schläuche von vorn zwischen die Führungsstege legen, Kabelbinder durch die Schlitze, dann Exzenter mit Lagern und Griffscheibe auf die Welle (Stifte in die Löcher), zuletzt den Steg anschrauben.

## Schrauben

- Motor: 4 × M3 × 6–8, Köpfe in der Rahmenplatte versenkt.
- Steg vorn: 4 × M3 × 12 (oder Spax 3 × 12) in die Amboss-Stirnseiten. Der Steg (6 mm) hält die Ambosse gegen Auseinanderfedern.
- Griffscheibe: 1 × M3 × 12 durch den Knauf in den Zapfen.
- Befestigung Rahmen: 4 × M3 bei x ± 40,5 / y ± 15.

## Exzenter-Reihe (21.11., Rahmen h 5 bleibt)

Der Rahmen legt die Summe beider Spalte fest (2 × (h + e Rahmen) = 17). Nur der Exzenter wird getauscht:

| Exzenter | Punkte | e | Spalt zu | Überpressung | Spalt offen |
|---|---|---|---|---|---|
| (gedruckt 21.11.) | – | 3,5 | 5,0 | 1,0 | 12,0 |
| V1 | 1 | 3,25 | 5,25 | 0,75 | 11,75 |
| V2 | 2 | 3,0 | 5,5 | 0,5 | 11,5 |
| V3 | 3 | 2,75 | 5,75 | 0,25 | 11,25 |

Kleineres e = weniger Quetschkraft und kürzerer Hebelarm → weniger Drehmoment. Zapfen alle Ø 16,9. NEMA-Bohrung locker Ø 5,50 / 2,25 + 2 dünne Quetschstege 0,45 / 0,2.

