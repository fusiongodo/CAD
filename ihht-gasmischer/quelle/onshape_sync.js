// Onshape-Sync: bringt alle Feature Studios auf den Stand eines Commits.
// Ausfuehren im Chrome-Tab auf cad.onshape.com (angemeldet):  await ihhtSync('<commit>')
//   - laedt onshape.json und die .fs von raw.githubusercontent.com (Repo fusiongodo/CAD)
//   - schreibt jedes geaenderte Feature Studio und setzt die Features der zugeordneten Part Studios auf die neue Microversion
//   - meldet je Modul Status, Volumen vorher/nachher (mm3) und Feature-Fehler
//   - Module ohne featureStudio werden nur mit {anlegen: ['datei.fs']} neu angelegt (IDs danach in onshape.json eintragen)
window.ihhtSync = async function (commit, opt) {
  opt = opt || {};
  const RAW = `https://raw.githubusercontent.com/fusiongodo/CAD/${commit}/ihht-gasmischer/quelle/`;
  const cfg = await fetch(RAW + 'onshape.json').then(r => r.json());
  const base = `/d/${cfg.dokument.d}/w/${cfg.dokument.w}`;
  const xsrf = decodeURIComponent((document.cookie.match(/XSRF-TOKEN=([^;]+)/) || [])[1] || '');
  const H = { 'Content-Type': 'application/json', 'Accept': 'application/json', 'X-XSRF-TOKEN': xsrf };
  const get = u => fetch('/api' + u).then(r => r.json());
  const post = async (u, b) => {
    const r = await fetch('/api' + u, { method: 'POST', headers: H, body: JSON.stringify(b) });
    if (!r.ok) throw new Error(u + ' -> ' + r.status + ' ' + (await r.text()).slice(0, 300));
    return r.json();
  };
  const vol = async ps => {
    const m = await get(`/partstudios${base}/e/${ps}/massproperties`);
    const a = m.bodies && m.bodies['-all-'];
    return a ? Math.round(a.volume[0] * 1e10) / 10 : null;
  };
  const out = [];
  for (const [datei, m] of Object.entries(cfg.module)) {
    const row = { datei };
    out.push(row);
    try {
      const src = await fetch(RAW + datei).then(r => r.text());
      if (!m.featureStudio) {
        if (!(opt.anlegen || []).includes(datei)) { row.status = 'nicht zugeordnet'; continue; }
        m.featureStudio = (await post(`/featurestudios${base}`, { name: m.name })).id;
        m.partStudios = [(await post(`/partstudios${base}`, { name: m.name })).id];
        row.neu = { featureStudio: m.featureStudio, partStudios: m.partStudios };
      }
      const E = m.featureStudio;
      const cur = await get(`/featurestudios${base}/e/${E}`);
      if (cur.contents === src && !row.neu) { row.status = 'unverändert'; continue; }
      row.vorher = [];
      for (const ps of m.partStudios) row.vorher.push(await vol(ps));
      await post(`/featurestudios${base}/e/${E}`, { contents: src, serializationVersion: cur.serializationVersion,
        sourceMicroversion: cur.sourceMicroversion, rejectMicroversionSkew: false });
      const emv = (await get(`/documents${base}/elements?elementId=${E}`))[0].microversionId;
      const ns = `e${E}::m${emv}`;
      row.nachher = []; row.fehler = [];
      for (const ps of m.partStudios) {
        let fl = await get(`/partstudios${base}/e/${ps}/features`);
        const ids = fl.features.filter(f => (f.message.namespace || '').startsWith('e' + E + '::')).map(f => f.message.featureId);
        if (!ids.length) {
          const r = await post(`/partstudios${base}/e/${ps}/features`, { feature: { type: 134, typeName: 'BTMFeature',
            message: { featureType: m.var, name: m.name, parameters: [], suppressed: false, namespace: ns } },
            serializationVersion: fl.serializationVersion, sourceMicroversion: fl.sourceMicroversion, rejectMicroversionSkew: false });
          ids.push(r.feature.message.featureId);
        } else for (const fid of ids) {
          fl = await get(`/partstudios${base}/e/${ps}/features`);
          const f = fl.features.find(x => x.message.featureId === fid);
          f.message.namespace = ns;
          await post(`/partstudios${base}/e/${ps}/features/featureid/${encodeURIComponent(fid)}`, { feature: f,
            serializationVersion: fl.serializationVersion, sourceMicroversion: fl.sourceMicroversion, rejectMicroversionSkew: false });
        }
        fl = await get(`/partstudios${base}/e/${ps}/features`);
        for (const s of fl.featureStates) if (ids.includes(s.key) && s.value.message.featureStatus !== 'OK') row.fehler.push(s.value.message.featureStatus);
        row.nachher.push(await vol(ps));
      }
      row.status = 'aktualisiert';
    } catch (e) { row.status = 'FEHLER: ' + e.message; }
  }
  return out;
};
