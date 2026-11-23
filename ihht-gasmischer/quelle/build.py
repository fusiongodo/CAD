# Baut alle FeatureScripts aus teile/*.py + Bibliothek ihht/.
#   python3 build.py            -> schreibt alle .fs neu (nur geaenderte Dateien werden angefasst)
#   python3 build.py --liste    -> zeigt Module und Teile
# Ein Modul = ein Feature Studio in Onshape (Zuordnung in onshape.json, Upload mit onshape_sync.js).
# Neues Teil: Datei in teile/ mit bauen() anlegen und hier in ein Modul eintragen.
import sys, os, importlib
here = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, here)
from ihht.emit import fs_text

MODULE = [
    dict(datei='chassis_v6.fs', titel='Chassis v6 (2 mm)', var='chassisV6', prefix='c6_', kommentar=False,
         teile=['rahmen', 'regal', 'motorbruecke', 'kameraportal', 'kamerahalter']),
    dict(datei='splitter_v5_qv.fs', titel='Splitter v5 Quetschventil', var='splitterV5QV', prefix='qv_',
         teile=['splitter_hi_qv', 'splitter_lo_qv']),
    dict(datei='splitter_lo_o2.fs', titel='Splitter lo O2-Tüllen', var='splitterLoO2', prefix='lo2_',
         teile=['splitter_lo_o2']),
    dict(datei='sammler_mix_acryl_v1.fs', titel='Sammler Mix Acryl', var='sammlerMixAcrylV1', prefix='acr_',
         teile=['sammler_mix_acryl']),
    dict(datei='splitter_v4_lzq.fs', titel='Splitter v4 LZQ', var='splitterV4LZQ', prefix='v4_',
         teile=['splitter_hi', 'splitter_lo']),
    dict(datei='quetschventil_v3.fs', titel='Quetschventil v3 (M8-Kern, 608 + 6905)', var='quetschventilV3', prefix='qv3_',
         teile=['qv_welle', 'qv_klemmhuelse', 'qv_hohlrad', 'qv_ritzel', 'qv_getriebeplatte', 'qv_rueckwand', 'qv_steg',
                'qv_amboss', 'qv_distanzscheibe', 'qv_dummies']),
    dict(datei='development_wand.fs', titel='Development-Wand (Lochraster, 2 QV)', var='developmentWand', prefix='dw_',
         teile=['wand_platte', 'wand_grundplatte', 'wand_knotenblech', 'wand_traeger', 'wand_dummies']),
]
OHNE_FS = ['sammler_mix', 'zahnrad', 'dummies']        # nur fuer Ansicht/Pruefung, kein eigenes Feature Studio

def teil(name):
    return importlib.import_module('teile.' + name)

def modul_text(m):
    parts, extra, nach = [], [], []
    for t in m['teile']:
        mod = teil(t)
        parts += mod.bauen()
        if hasattr(mod, 'fs_zusatz'):
            e, n = mod.fs_zusatz(); extra += e; nach += n
    komm = None
    if m.get('kommentar', True):
        komm = '// %s: erzeugt von build.py aus %s (Bibliothek ihht/). Nicht von Hand aendern.' % (
            m['titel'], ', '.join('teile/%s.py' % t for t in m['teile']))
    return fs_text(m['titel'], m['var'], m['prefix'], parts, komm, extra, nach), parts

def alle_teile():
    out = {}
    for t in [t for m in MODULE for t in m['teile']] + OHNE_FS:
        for p in teil(t).bauen():
            out[p.name] = p
    return out

if __name__ == '__main__':
    import json
    zuordnung = json.load(open(os.path.join(here, 'onshape.json')))['module']
    for m in MODULE:
        z = zuordnung.get(m['datei'])
        assert z and z['var'] == m['var'] and z['name'] == m['titel'], 'onshape.json passt nicht zu Modul ' + m['datei']
    for m in MODULE:
        text, parts = modul_text(m)
        if '--liste' in sys.argv:
            print('%-26s %s' % (m['datei'], ', '.join(p.name for p in parts))); continue
        path = os.path.join(here, m['datei'])
        alt = open(path).read() if os.path.exists(path) else None
        if text != alt: open(path, 'w').write(text)
        print('%-26s %-10s %s' % (m['datei'], 'unverändert' if text == alt else 'NEU', ', '.join(p.name for p in parts)))
