import { useCallback, type Dispatch, type SetStateAction } from 'react';
import { useSearchParams } from 'react-router-dom';

type UrlValue = string | number | boolean | null | undefined;

/** URL is the canonical state. One atomic update can change filters and reset pagination together. */
let pendingUpdate: { pathname: string; params: URLSearchParams } | null = null;

export function useUrlQuery() {
    const [params, setParams] = useSearchParams();

    const patch = useCallback((changes: Record<string, UrlValue>, push = false) => {
        const pathname = window.location.pathname;
        const next = new URLSearchParams(pendingUpdate?.pathname === pathname ? pendingUpdate.params : window.location.search);
        for (const [key, value] of Object.entries(changes)) {
            if (value === null || value === undefined || value === '' || value === false) next.delete(key);
            else next.set(key, value === true ? '1' : String(value));
        }
        pendingUpdate = { pathname, params: next };
        // React Router's setSearchParams does not queue sequential functional updates.
        // Share one pending object across hooks so a search change and page reset are atomic.
        queueMicrotask(() => { if (pendingUpdate?.params === next) pendingUpdate = null; });
        setParams(next, { replace: !push, preventScrollReset: true });
    }, [setParams]);

    return { params, patch };
}

export function useUrlString(key: string, defaultValue = ''): [string, Dispatch<SetStateAction<string>>] {
    const { params, patch } = useUrlQuery();
    const value = params.get(key) ?? defaultValue;
    const setValue = useCallback<Dispatch<SetStateAction<string>>>((change) => {
        const previous = new URLSearchParams(window.location.search).get(key) ?? defaultValue;
        const next = typeof change === 'function' ? change(previous) : change;
        patch({ [key]: next === defaultValue ? null : next }, ['dialog', 'tab', 'view', 'page', 'roleEditor', 'requestLeave'].includes(key));
    }, [defaultValue, key, patch]);
    return [value, setValue];
}

export function useUrlNumber(key: string, defaultValue = 1): [number, Dispatch<SetStateAction<number>>] {
    const [raw, setRaw] = useUrlString(key, String(defaultValue));
    const parsed = Number(raw);
    const value = Number.isSafeInteger(parsed) && parsed >= 1 && parsed <= 100000 ? parsed : defaultValue;
    const setValue = useCallback<Dispatch<SetStateAction<number>>>((change) => {
        setRaw(prev => String(typeof change === 'function' ? change(Number(prev) || defaultValue) : change));
    }, [setRaw, defaultValue]);
    return [value, setValue];
}

export function useUrlBoolean(key: string, defaultValue = false): [boolean, Dispatch<SetStateAction<boolean>>] {
    const [raw, setRaw] = useUrlString(key, defaultValue ? '1' : '0');
    const setValue = useCallback<Dispatch<SetStateAction<boolean>>>((change) => {
        setRaw(prev => (typeof change === 'function' ? change(prev === '1') : change) ? '1' : '0');
    }, [setRaw]);
    return [raw === '1', setValue];
}
