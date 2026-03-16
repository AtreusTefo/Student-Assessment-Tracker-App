# Daily Report – March 9, 2026

---

## What I Did Today

- Installed Angular frontend dependencies (`npm install`) for the `StudentApp` project
- Started the Angular development server (`ng serve`) on **http://localhost:4200**
- Updated `README.md` to document the Node.js PATH fix for Windows users
- Committed all local project files to Git and pushed them to the GitHub repository:
  **https://github.com/AtreusTefo/Student-Assessment-Tracker-App**

---

## What Was Completed

| Task | Status |
|------|--------|
| npm dependencies installed | ✅ Done |
| Angular dev server running (localhost:4200) | ✅ Done |
| README updated with Windows Node.js PATH instructions | ✅ Done |
| Initial Git commit (101 files) | ✅ Done |
| Code pushed to GitHub `main` branch | ✅ Done |

---

## Challenges Faced

### 1. `npm` Not Recognized in PowerShell
- **Problem:** Running `npm install` failed with `npm is not recognized` because Node.js was installed at `C:\Program Files\nodejs` but the folder was not in the current terminal session's PATH.
- **Fix:** Manually prepended the Node.js path for the session:
  ```powershell
  $env:PATH = "C:\Program Files\nodejs;" + $env:PATH
  ```
- **Permanent fix documented** in `README.md` under Prerequisites.

### 2. `git push` Rejected (Non-Fast-Forward)
- **Problem:** The GitHub repository had been initialized with a remote file (e.g., auto-generated README), making the remote history ahead of the local `master` branch.
- **Fix:** Pulled the remote `main` branch with `--allow-unrelated-histories` to merge the two unrelated commit trees.

### 3. Merge Conflicts After Pull
- **Problem:** After pulling, Git reported `add/add` conflicts in three files:
  - `README.md`
  - `StudentApp/angular.json`
  - `StudentApp/package-lock.json`
- **Fix:** Resolved all conflicts by keeping the local project versions (`git checkout --ours`), committed the merge, then pushed successfully to `main`.
