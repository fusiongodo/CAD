# Anleitung für andere Chats: FeatureScript ins CAD-Repo pushen

Gilt für alle Release-Valve-Skripte (`release_valve*.fs`).

## Ziel
- Repo: `github.com/fusiongodo/CAD`, Branch `main`
- Lokaler Klon auf dem Mac: `/Users/alexanderhallmann/Desktop/projects/CAD`
- Ablage: `release_valve/quelle/<name>.fs`. Ein Teil entspricht einer Datei, Namen in snake_case, z. B. `release_valve_a.fs` oder `release_valve_teller.fs`
- Daneben liegt `release_valve/drehventil_recherche.html` (Recherche zum Drehventil). Nicht überschreiben.

## Ablauf
1. Den Ordner `/Users/alexanderhallmann/Desktop/projects/CAD` freigeben lassen (`device_request_folder_access`) und die `.fs` mit `device_commit_files` nach `release_valve/quelle/` schreiben.
2. Git auf dem Mac ausführen, über Desktop Commander (`start_process`). `device_bash` nicht verwenden: Die Linux-VM hat keinen SSH-Schlüssel und kann die Git-Lock-Dateien nicht löschen.
3. Vor dem Commit holen, denn andere Chats pushen ebenfalls in dieses Repo:

```sh
cd /Users/alexanderhallmann/Desktop/projects/CAD
export GIT_SSH_COMMAND="ssh -o BatchMode=yes"
git pull --rebase -q origin main
git add release_valve/quelle/<name>.fs
git commit -q -m "Release Valve: <kurze Beschreibung>" -m "Co-Authored-By: Claude <noreply@anthropic.com>"
git push origin main
git status -sb | head -1   # Erwartung: "## main...origin/main" ohne [ahead]
```

## Regeln
- Nur `git add` mit explizitem Pfad, nie `git add .`. Im Ordner liegen viele ungetrackte 3MF-, G-Code- und STL-Dateien mit bis zu 25 MB.
- Kein `--force`. Wird der Push abgelehnt, erneut `git pull --rebase` ausführen und dann pushen.
- Der Remote läuft über den SSH-Alias `github-cad` (`git@github-cad:fusiongodo/CAD.git`). Diesen Alias nicht ändern.
- Commits laufen unter Alex' Git-Namen, Claude steht als Co-Autor drin.
- Gewinde immer als Rechtsgewinde: In Onshape `opHelix` mit `"clockwise": true`. Vor dem Push mit der `helixIsRightHand`-Prüfung kontrollieren.
- Alex bekommt die `.fs` zusätzlich wie gewohnt als Datei im Chat zum Kopieren.
