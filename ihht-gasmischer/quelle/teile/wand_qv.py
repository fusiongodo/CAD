# Development-Wand: die beiden Quetschventile als echte Teile (ihht/qv2.py), nicht als Platzhalter.
# Rückseite der Rahmenplatte liegt auf der Wand-Vorderseite (Y = 0), Achse bei X ±QX / Z QZ. Hohlrad, Getriebe und Motor
# stecken durch den Wandausschnitt nach hinten. Nicht als Druckteile der Wand geführt (printed=False).
from ihht import qv2
from ihht.wand import QX, QZ


def bauen():
    return (qv2.teile('dwQVL', -QX, QZ, printed=False, nema='dwNemaL')
            + qv2.teile('dwQVR', QX, QZ, printed=False, nema='dwNemaR'))
