import { useEffect, useState } from 'react';
import { Check, Copy } from 'lucide-react';

export function CopyCodeButton({ code }: { code: string }) {
    const [copied, setCopied] = useState(false);
    const [error, setError] = useState(false);
    useEffect(() => { setCopied(false); setError(false); }, [code]);
    const copy = async () => {
        try {
            await navigator.clipboard.writeText(code);
            setCopied(true);
            setError(false);
        } catch {
            setCopied(false);
            setError(true);
        }
    };
    return <div className="inline-flex flex-col gap-1">
        <button type="button" onClick={() => { void copy(); }}
            className={`inline-flex items-center gap-2 rounded-lg border px-3 py-2 text-sm font-semibold transition-all active:scale-95 focus-visible:ring-2 focus-visible:ring-emerald-500 ${copied ? 'border-emerald-500 bg-emerald-600 text-white shadow-md' : 'border-amber-400 bg-amber-100 text-amber-950 hover:bg-amber-200'}`}
            aria-live="polite">
            {copied ? <Check size={16}/> : <Copy size={16}/>} {copied ? 'Copied!' : 'Copy code'}
        </button>
        {error && <span role="alert" className="text-xs text-red-700">Copy failed. Select the code and copy it manually.</span>}
    </div>;
}
