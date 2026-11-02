export const VERIFICATION_RESEND_COOLDOWN_MS = 60_000;

const keyFor = (email: string) =>
    `tenexerp:verification-resend:${email.trim().toLowerCase()}`;

export function getVerificationCooldownUntil(email: string): number {
    if (!email.trim()) return 0;
    try {
        const stored = Number(sessionStorage.getItem(keyFor(email)));
        return Number.isFinite(stored) ? stored : 0;
    } catch {
        return 0; // Private browsing/storage restrictions should not break signup.
    }
}

export function startVerificationCooldown(email: string): number {
    const until = Date.now() + VERIFICATION_RESEND_COOLDOWN_MS;
    try {
        sessionStorage.setItem(keyFor(email), String(until));
    } catch {
        // The current page still displays its countdown from the returned value.
    }
    return until;
}
