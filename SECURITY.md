# Security Policy

## Supported Versions

Any version above v2.* may receive security patches. 

## Reporting a Vulnerability
We strongly encourage responsible disclosure. If you discover a security issue in DevToys or any of its official components:

### 🔒 1. Report privately via GitHub Security Advisories
Use the “Report a vulnerability” button in the repository’s Security → Advisories section.
This opens a private workspace where you can safely share details with the maintainers.
### 📧 2. Alternative contact (if GitHub is unavailable)
If you cannot use GitHub's advisory system, you may contact the maintainers at:
support[at]velersoftware.com
Please include:
- A clear description of the issue
- Steps to reproduce
- Impact assessment
- Any relevant proof‑of‑concept
- Suggested remediation (if known)

We aim to acknowledge reports within 72 hours.

## Coordinated Disclosure Process
Once a vulnerability is reported:
- The maintainers investigate and validate the issue.
- If confirmed, a private GitHub Security Advisory is created (or continued).
- A fix is developed in a private fork if needed.
- A CVE identifier is requested through GitHub’s CNA program.
- A patched release is published.
- The advisory is made public with full details and credits.
We follow a responsible disclosure model:
- We do not publish details until a fix is available.
- Researchers are credited in the advisory unless they request anonymity.

## Scope

### In scope
- DevToys core application
- DevToys extension installation and loading mechanisms
- DevToys extension manifest parsing
- DevToys official website or update channels
- DevToys macOS, Windows, and Linux distributions

### Out of scope
- Third‑party extensions not maintained by the DevToys team
- Issues requiring physical access to a device
- Social engineering attacks
- Vulnerabilities in external dependencies unless they directly impact DevToys usage

## Public Advisories
All published advisories, including CVEs, are available in the repository’s
Security → Advisories section and in the GitHub Advisory Database.
