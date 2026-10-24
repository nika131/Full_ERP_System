import { useState, type FormEvent } from 'react';
import { X, Send, Bug, Lightbulb, Mail } from 'lucide-react';
type Kind = 'bug' | 'idea';
const SUPPORT = 'support@tenexerp.com';
export default function FeedbackDialog({ kind, onClose }: { kind: Kind; onClose: () => void }) {
  const [body, setBody] = useState('');
  const [details, setDetails] = useState('');
  const send = (e: FormEvent) => {
    e.preventDefault();
    const subject = kind === 'bug' ? 'TenexERP problem report' : 'TenexERP feature suggestion';
    const text = `${body.trim()}\n\n${details.trim()}\n\nPage: ${location.href}\nSubmitted from TenexERP website.`;
    window.location.href = `mailto:${SUPPORT}?subject=${encodeURIComponent(subject)}&body=${encodeURIComponent(text)}`;
    onClose();
  };
  return <div role="presentation" className="fixed inset-0 z-[100] flex items-center justify-center bg-slate-950/55 px-4 py-6 backdrop-blur-sm" onMouseDown={e=>{if(e.target===e.currentTarget)onClose();}}><div role="dialog" aria-modal="true" aria-label={kind==='bug'?'Report a problem':'Suggest a feature'} className="w-full max-w-lg rounded-3xl bg-white p-6 text-left shadow-2xl sm:p-8"><div className="flex items-start justify-between"><div className="flex items-center gap-3"><span className="rounded-xl bg-emerald-100 p-3 text-emerald-700">{kind==='bug'?<Bug/>:<Lightbulb/>}</span><div><h2 className="text-xl font-bold text-slate-900">{kind==='bug'?'Report a problem':'Share an idea'}</h2><p className="text-sm text-slate-500">We'd love to hear from you</p></div></div><button type="button" className="rounded-xl p-2 hover:bg-slate-100" onClick={onClose} aria-label="Close"><X size={20}/></button></div><form onSubmit={send} className="mt-7 space-y-4"><label className="block text-sm font-semibold text-slate-700">{kind==='bug'?'What went wrong?':'What would make TenexERP better?'}<textarea required autoFocus minLength={8} maxLength={4000} rows={5} value={body} onChange={e=>setBody(e.target.value)} placeholder={kind==='bug'?'Tell us what you were trying to do and what happened…':'Describe the functionality you would like to see…'} className="mt-2 w-full rounded-xl border border-slate-300 p-3 font-normal outline-none focus:border-emerald-600"/></label><label className="block text-sm font-semibold text-slate-700">Extra details (optional)<input maxLength={350} value={details} onChange={e=>setDetails(e.target.value)} placeholder="Steps to reproduce, expected behavior…" className="mt-2 w-full rounded-xl border border-slate-300 p-3 font-normal outline-none focus:border-emerald-600"/></label><div className="rounded-xl bg-amber-50 p-3 text-xs leading-5 text-amber-900"><Mail className="mr-1 inline" size={14}/> This opens your email app with a prepared message. Review it and press Send there. Direct in-app sending will be available after a secure support endpoint is configured.</div><button type="submit" disabled={body.trim().length<8} className="flex w-full items-center justify-center gap-2 rounded-xl bg-[#153f34] px-4 py-3 font-bold text-white hover:bg-[#23614c] disabled:opacity-50"><Send size={16}/> Open prepared email</button></form></div></div>;
}
