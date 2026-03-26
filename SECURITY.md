# Security Policy

## Supported Versions

| Version | Supported          |
|---------|--------------------|
| latest  | ✅ Yes              |
| < latest| ❌ No               |

Only the latest published version receives security updates.

## Reporting a Vulnerability

If you discover a security vulnerability in VoloGen, **please do not open a public issue**.

Instead, report it privately:

1. **Email**: Send details to **oleksandr.tsvirkun@gmail.com**
2. **GitHub**: Use [GitHub Security Advisories](https://github.com/OleksandrTsvirkun/VoloGen/security/advisories/new) to report privately

### What to include

- A description of the vulnerability
- Steps to reproduce (if applicable)
- Potential impact assessment
- Suggested fix (if you have one)

### Response timeline

- **Acknowledgement**: Within 48 hours
- **Initial assessment**: Within 1 week
- **Fix release**: As soon as practical, depending on severity

## Scope

VoloGen is a compile-time source generator. It does not execute at runtime,
handle network traffic, or process untrusted input in production. Security
concerns are most likely to involve:

- Generated code that could introduce vulnerabilities in consuming applications
- Supply-chain issues in dependencies
- Information disclosure through diagnostic messages

Thank you for helping keep VoloGen safe.
