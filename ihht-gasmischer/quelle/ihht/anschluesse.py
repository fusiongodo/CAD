# Zentrale Anschluss-Bibliothek: jeder Schlauchanschluss hat hier GENAU EINE Definition.
# Teile nennen nur den Typ ('o2', 'cpap', 'lzq', 'qv'). Wer hier ein Mass aendert und `python3 build.py`
# laufen laesst, aendert den Anschluss an allen Teilen, die ihn verwenden.
#
# Jeder Anschluss liegt auf einer Y-Achse und kann:
#   koerper(x, z, y0, d)                  -> Koerper-Ops; ab Wandflaeche y0 in Richtung d (+1 = +Y, -1 = -Y)
#   bohrung(x, z, y0, d, y_kanal, tief)   -> Schnitt-Ops; Durchgang von der Spitze bis zur Kanalachse (+ tief)
#   laenge, r_max                         -> Laenge ab Wand, groesster Radius
from .geo import C, Cy, K, T

class Wellentuelle:
    """Schlauchtuelle mit Saegezahn-Wellen (Stufe zur Wand, Rampe zur Spitze)."""
    def __init__(s, r_innen, r_tal, r_spitze, ansatz, wellen, plateau, rampe, laenge):
        s.r_innen, s.r_tal, s.r_spitze, s.ansatz, s.wellen, s.plateau, s.rampe, s.laenge = r_innen, r_tal, r_spitze, ansatz, wellen, plateau, rampe, laenge
        s.r_max = r_spitze
    def koerper(s, x, z, y0, d=1):
        ops = [Cy(x, y0 - d * 0.5, y0 + d * s.laenge, z, 2 * s.r_tal)]
        for w in range(s.wellen):
            a = y0 + d * (s.ansatz + w * (s.plateau + s.rampe))
            ops.append(Cy(x, a, a + d * s.plateau, z, 2 * s.r_spitze))                               # Stufe + Plateau
            ops.append(K(x, z, a + d * s.plateau, a + d * (s.plateau + s.rampe), s.r_spitze, s.r_tal))   # Rampe zur Spitze hin
        return ops
    def bohrung(s, x, z, y0, d, y_kanal, tief=0.0):
        ya, yb = y_kanal - d * tief, y0 + d * (s.laenge + 1)
        return [Cy(x, min(ya, yb), max(ya, yb), z, 2 * s.r_innen)]
    def koerper_achse(s, p0, n):
        """Wie koerper(), aber auf beliebiger Achse: p0 = Mitte der Wandflaeche, n = Einheitsvektor zur Spitze."""
        pt = lambda t: tuple(p0[i] + n[i] * t for i in range(3))
        ops = [C(pt(-0.5), pt(s.laenge), 2 * s.r_tal)]
        for w in range(s.wellen):
            a = s.ansatz + w * (s.plateau + s.rampe)
            ops.append(C(pt(a), pt(a + s.plateau), 2 * s.r_spitze))
            ops.append(('cone', pt(a + s.plateau), pt(a + s.plateau + s.rampe), s.r_spitze, s.r_tal))
        return ops
    def bohrung_achse(s, p0, n, t0):
        """Bohrung r_innen auf der Achse von p0 + n*t0 bis 1 mm ueber die Spitze."""
        pt = lambda t: tuple(p0[i] + n[i] * t for i in range(3))
        return [C(pt(t0), pt(s.laenge + 1), 2 * s.r_innen)]

class Stecktuelle:
    """Kurze Tuelle mit 3 flachen Wellen und 45°-Fase am Sockel (steckt im Schlauchstueck zum LZQ)."""
    def __init__(s, od, laenge, bohr, tropfen, ueber=0.0):
        s.od, s.laenge, s.bohr, s.tropfen, s.ueber = od, laenge, bohr, tropfen, ueber
        s.rd = 0.25 if od > 9 else 0.15
        s.r_max = od / 2 + s.rd
    def koerper(s, x, z, y0, d=-1):
        r, rd = s.od / 2, s.rd
        ytip, ybase = y0 + d * s.laenge, y0 - d * 0.5
        ops = [Cy(x, ybase, ytip, z, s.od)]
        for k in range(3):
            a = ytip - d * (0.3 + k * 2.4)
            ops.append(K(x, z, a, a - d * 1.8, r, r + rd))                  # Rampe (von der Spitze her)
            ops.append(Cy(x, a - d * 1.8, a - d * 2.1, z, s.od + 2 * rd))   # kurzes Plateau, dann Stufe
        ops.append(K(x, z, ybase - d * 0.3, ybase + d * 2.2, r + 2.0, r))   # Fase 45°
        return ops
    def bohrung(s, x, z, y0, d, y_kanal, tief=0.0):
        ytip = y0 + d * (s.laenge + 1)
        if s.tropfen: return [T((x, ytip, z), (x, y_kanal, z), s.bohr)]     # voller Tropfen passt in die Tuelle
        return [Cy(x, ytip, y_kanal - d * s.ueber, z, s.bohr)]

class GlattTuelle:
    """Glatte Tuelle ohne Wellen, 45°-Fase an der Wurzel (gegen Abbrechen), Einfuehrfase an der Spitze."""
    def __init__(s, od, bohr, laenge, wurzel_fase, spitze_fase):
        s.od, s.bohr, s.laenge, s.wurzel, s.spitze = od, bohr, laenge, wurzel_fase, spitze_fase
        s.r_max = od / 2
    def koerper(s, x, z, y0, d=-1):
        r = s.od / 2
        return [Cy(x, y0 - d * 0.5, y0 + d * (s.laenge - s.spitze), z, s.od),
                K(x, z, y0 + d * (s.laenge - s.spitze), y0 + d * s.laenge, r, r - s.spitze),   # Einfuehrfase
                K(x, z, y0 - d * 0.3, y0 + d * s.wurzel, r + s.wurzel + 0.3, r)]               # Wurzel-Fase 45°
    def bohrung(s, x, z, y0, d, y_kanal, tief=0.0):
        return [Cy(x, y0 + d * (s.laenge + 1), y_kanal, z, s.bohr)]

ANSCHLUSS = {
    # duenne O2-Tuelle (Standard-O2-Schlauch): Kern Ø 7, Wellen Ø 7,5, Bohrung Ø 5,2, 12 mm
    'o2':     Wellentuelle(r_innen=2.6, r_tal=3.5, r_spitze=3.75, ansatz=2.0, wellen=4, plateau=0.5, rampe=2.0, laenge=12.0),
    # CPAP-Schlauch 22 mm: Kern Ø 22,8, Wellen Ø 24,4, Bohrung Ø 20,4, 16,2 mm
    'cpap':   Wellentuelle(r_innen=10.2, r_tal=11.4, r_spitze=12.2, ansatz=3.0, wellen=4, plateau=0.8, rampe=2.5, laenge=16.2),
    # CPAP-Schlauch 22 mm nach dem CPAP-auf-CPAP-Adapter im Onshape-Dokument „IHHT Retrofit“ (segm1 Ø 22,5, Wulst Ø 24);
    # Y-Split (getestet) hat 21,4/23, 'cpap' oben ist mit 22,8/24,4 groesser als beide. Bohrung Ø 20,4, 16,2 mm.
    'cpap_pi': Wellentuelle(r_innen=10.2, r_tal=11.25, r_spitze=12.0, ansatz=3.0, wellen=4, plateau=0.8, rampe=2.5, laenge=16.2),
    # Schlauchstueck zum LZQ-Rotameter: Kern Ø 10,75 (Wellenscheitel 11,25, Ring-Test zwischen Nr. 3 und 4), Tropfen Ø 6,5
    'lzq':    Stecktuelle(od=10.75, laenge=12.0, bohr=6.5, tropfen=True),
    # Quetschventil-Silikonschlauch 6/12: aussen Ø 8, innen Ø 6, glatt, Wurzel-Fase 2 mm
    'qv':     GlattTuelle(od=8.0, bohr=6.0, laenge=12.0, wurzel_fase=2.0, spitze_fase=0.5),
    # Altstand (nur Sammler Mix v4): O2-Stecktuelle Ø 7 / Bohrung Ø 5, 10 mm
    'o2_alt': Stecktuelle(od=7.0, laenge=10.0, bohr=5.0, tropfen=False, ueber=0.5),
}
def anschluss(typ):
    return ANSCHLUSS[typ] if isinstance(typ, str) else typ
