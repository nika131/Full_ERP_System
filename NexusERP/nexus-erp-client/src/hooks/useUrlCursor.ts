import { useMemo } from 'react';
import { useUrlQuery } from './useUrlState';

type Cursor = { createdAt: string | null; id: number | null };
const FIRST: Cursor = { createdAt: null, id: null };

/** Cursor history is encoded in the link; even later pages can be shared directly. */
export function useUrlCursor() {
    const { params, patch } = useUrlQuery();
    const raw = params.get('cursorTrail');
    const history = useMemo<Cursor[]>(() => {
        if (!raw || raw.length > 8000) return [FIRST];
        try {
            const list: unknown = JSON.parse(raw);
            if (!Array.isArray(list) || list.length > 30) return [FIRST];
            const parsed = list.filter((c): c is Cursor => {
                if (!c || typeof c !== 'object') return false;
                const item = c as Partial<Cursor>;
                return (typeof item.createdAt === 'string' || item.createdAt === null) &&
                    (Number.isSafeInteger(item.id) || item.id === null);
            });
            return parsed.length ? [FIRST, ...parsed.filter((v, i) => !(i === 0 && v.id === null))] : [FIRST];
        } catch { return [FIRST]; }
    }, [raw]);
    const number = Number(params.get('cursorPage') ?? 1);
    const page = Number.isInteger(number) && number > 0 && number <= history.length ? number : 1;
    const cursor = history[page - 1] ?? FIRST;
    const next = (value: Cursor) => {
        if (!value.createdAt || !value.id || history.length >= 30) return;
        const nextHistory = [...history.slice(0, page), value];
        patch({ cursorTrail: JSON.stringify(nextHistory.slice(1)), cursorPage: page + 1 }, true);
    };
    const previous = () => patch({ cursorPage: page <= 2 ? null : page - 1 }, true);
    const reset = () => patch({ cursorTrail: null, cursorPage: null });
    return { cursor, page, next, previous, reset, isFirstPage: page === 1 };
}
