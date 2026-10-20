import { useState } from 'react';
import { Link } from 'react-router-dom';
import { ArrowRight, ArrowUpRight, BarChart3, Boxes, Check, ChevronDown, ClipboardList, Clock3, Cloud, CreditCard, FileText, LayoutDashboard, LockKeyhole, LogIn, Mail, Menu, PackageCheck, ShieldCheck, Sparkles, Store, Sun, Users, Zap } from 'lucide-react';

const features = [
  { icon: Boxes, title: 'Know what’s in stock', text: 'Track products, inventory movements, and low-stock alerts without getting lost in spreadsheets.' },
  { icon: CreditCard, title: 'Keep checkout moving', text: 'Give your team a focused point-of-sale experience for everyday transactions.' },
  { icon: BarChart3, title: 'See the bigger picture', text: 'Understand sales, business performance, and the details behind your numbers.' },
  { icon: Users, title: 'Your team, your rules', text: 'Create roles with carefully chosen permissions for each employee.' },
  { icon: Store, title: 'Manage multiple locations', text: 'Keep stores and their operations organized in a single workspace.' },
  { icon: ClipboardList, title: 'Less manual admin', text: 'Stay on top of shifts, records, and daily tasks from one convenient place.' },
];
// Specific examples are revealed inside the card when a visitor opens it.
const featureDetails = [
  ['Find products and current quantities in one place.', 'Review low-stock alerts and record stock changes.', 'Keep product information organized for your team.'],
  ['Use a focused point-of-sale flow for everyday purchases.', 'Track receipts and payment activity.', 'Manage cashier shifts and cash movements.'],
  ['See performance summaries and trends.', 'Review sales transactions and reports.', 'Spot products that contribute to revenue.'],
  ['Create employee roles with precise permissions.', 'Review company join requests before granting access.', 'Keep work relevant to each team member.'],
  ['Organize your stores in one company workspace.', 'Maintain store details and locations.', 'Review operations without jumping between separate systems.'],
  ['Keep an eye on shifts and activity records.', 'Manage employee-related tasks from your workspace.', 'Spend less time gathering updates from different places.'],
] as const;

const questions = [
  ['Is TenexERP only for large companies?', 'Not at all. TenexERP is being built with small and growing businesses in mind. Start with the tools you need and add more as your team grows.'],
  ['Do I need technical experience?', 'No specialist setup knowledge is required to explore the platform. We focus on a clear workspace and familiar business workflows.'],
  ['What happens after the introductory month?', 'Paid plans are planned but not available yet. We’ll explain the choices and terms before any subscription or charge begins.'],
  ['Does it work with my staff?', 'Yes. You can invite colleagues to your company and decide what each role is allowed to view or change.'],
];
export default function Landing() {
  const [mobile, setMobile] = useState(false);
  const [opened, setOpened] = useState<number | null>(0);
  const [expandedFeature, setExpandedFeature] = useState<number | null>(null);
  return <div className="min-h-screen bg-[#f9faf6] text-[#142e28] selection:bg-emerald-200">
    <header className="sticky top-0 z-30 border-b border-[#e6ece5]/80 bg-[#f9faf6]/90 backdrop-blur-xl">
      <div className="mx-auto flex h-20 max-w-7xl items-center justify-between gap-4 px-5 sm:px-8">
        <Link to="/" className="flex items-center gap-2.5 text-2xl font-black tracking-[-.055em]" aria-label="TenexERP home"><span className="flex h-10 w-10 items-center justify-center rounded-xl bg-[#163f35] text-white"><Boxes size={23}/></span>Tenex<span className="text-emerald-600">ERP</span></Link>
        <nav className="hidden items-center gap-8 text-sm font-semibold text-slate-600 md:flex"><a className="hover:text-emerald-700" href="#features">Features</a><a className="hover:text-emerald-700" href="#how-it-works">How it works</a><a className="hover:text-emerald-700" href="#questions">FAQ</a></nav>
        <div className="hidden items-center gap-3 md:flex"><Link to="/login" className="rounded-full px-4 py-2.5 text-sm font-semibold hover:bg-slate-100">Log in</Link><Link to="/register" className="flex items-center gap-2 rounded-full bg-[#123f33] px-5 py-3 text-sm font-semibold text-white shadow-lg shadow-emerald-950/10 transition hover:-translate-y-0.5 hover:bg-[#215b4a]">Start exploring free <ArrowUpRight size={16}/></Link></div>
        <button type="button" className="rounded-xl p-2 md:hidden" onClick={() => setMobile(!mobile)} aria-label="Toggle menu"><Menu/></button>
      </div>
      {mobile && <nav className="flex flex-col gap-3 border-t bg-white p-5 text-sm font-semibold md:hidden"><a onClick={() => setMobile(false)} href="#features">Features</a><a onClick={() => setMobile(false)} href="#how-it-works">How it works</a><a onClick={() => setMobile(false)} href="#questions">FAQ</a><Link to="/login">Log in</Link><Link className="text-emerald-700" to="/register">Start exploring free <ArrowRight size={15} className="inline" aria-hidden="true"/></Link></nav>}
    </header>
    <main>
      <section className="relative overflow-hidden px-5 pb-20 pt-20 sm:px-8 lg:pb-28 lg:pt-28"><div className="pointer-events-none absolute -right-56 top-0 h-[700px] w-[700px] rounded-full bg-[#e6f3ce] opacity-60 blur-3xl"/><div className="relative mx-auto grid max-w-7xl items-center gap-14 lg:grid-cols-[.95fr_1.05fr]">
        <div className="relative"><div className="mb-7 inline-flex items-center gap-2 rounded-full border border-emerald-200 bg-white/80 px-4 py-2 text-xs font-bold text-emerald-800"><Sparkles size={15}/> Made for businesses ready to grow</div><h1 className="max-w-2xl text-[clamp(3rem,6.6vw,6.5rem)] font-black leading-[.99] tracking-[-.075em]">Run your business.<span className="block text-[#3a9c70]">Not your spreadsheets.</span></h1><p className="mt-8 max-w-xl text-lg leading-8 text-[#53665e]">Sales, stock, stores, and your team—working together in one simple place. Spend less time keeping up and more time moving forward.</p><div className="mt-9 flex flex-wrap items-center gap-3"><Link to="/register" className="inline-flex items-center gap-3 rounded-2xl bg-[#123f33] px-7 py-4 font-bold text-white shadow-xl shadow-emerald-900/15 transition hover:-translate-y-1 hover:bg-[#215b4a]">Get started free <ArrowRight size={18}/></Link><a href="#how-it-works" className="rounded-2xl border border-[#cfdbd0] bg-white px-7 py-4 font-bold transition hover:border-emerald-400">See how it works</a></div><div className="mt-7 flex flex-wrap items-center gap-5 text-sm font-medium text-[#597166]"><span className="inline-flex items-center gap-2"><Check size={16} className="text-emerald-600"/> Introductory month</span><span className="inline-flex items-center gap-2"><Check size={16} className="text-emerald-600"/> No payment setup yet</span><span className="inline-flex items-center gap-2"><Check size={16} className="text-emerald-600"/> Start small</span></div></div>
        <div className="relative min-w-0"><div className="absolute -left-10 -top-8 h-36 w-36 rounded-full bg-[#cce7b2] blur-2xl"/><div className="relative rounded-[30px] border border-[#d5e3d7] bg-white/80 p-4 shadow-[0_35px_100px_rgba(18,63,51,.15)] backdrop-blur sm:p-7"><div className="overflow-hidden rounded-2xl border border-[#e4ebe5] bg-white"><div className="flex items-center gap-2 border-b border-slate-100 px-5 py-4"><span className="h-2.5 w-2.5 rounded-full bg-[#ef9a90]"/><span className="h-2.5 w-2.5 rounded-full bg-[#eacb76]"/><span className="h-2.5 w-2.5 rounded-full bg-[#8bcaa8]"/><span className="ml-4 text-xs font-bold text-slate-400">TENEXERP / WORKSPACE PREVIEW</span></div><div className="grid grid-cols-[65px_1fr] sm:grid-cols-[125px_1fr]"><div className="space-y-6 bg-[#133d33] px-4 py-6 text-white"><div className="h-7 w-7 rounded-lg bg-emerald-300/25 p-1"><LayoutDashboard size={19}/></div>{[Boxes,BarChart3,Store,Users].map((Icon,i)=><div key={i} className="flex items-center gap-2 text-xs text-emerald-100"><Icon size={18}/><span className="hidden sm:inline">{['Inventory','Reports','Stores','People'][i]}</span></div>)}</div><div className="min-w-0 bg-[#fbfdfb] p-4 sm:p-6"><div className="flex items-center justify-between"><div><div className="text-xs font-semibold text-slate-400">Tuesday overview</div><div className="mt-1 text-xl font-extrabold text-slate-800 sm:text-2xl">Good morning <Sun size={19} className="inline align-[-2px] text-amber-500" aria-label="sunshine"/></div></div><span className="rounded-full bg-emerald-100 px-3 py-1 text-[10px] font-bold text-emerald-800">Your workspace</span></div><div className="mt-6 grid grid-cols-2 gap-3"><div className="rounded-xl border border-slate-100 bg-white p-4"><div className="text-xs font-medium text-slate-500">Today's activity</div><div className="mt-3 text-3xl font-extrabold text-slate-800">128</div><div className="text-xs text-emerald-700">Example transactions</div></div><div className="rounded-xl border border-slate-100 bg-white p-4"><div className="text-xs font-medium text-slate-500">Stock items</div><div className="mt-3 text-3xl font-extrabold text-slate-800">842</div><div className="text-xs text-slate-400">Example products</div></div></div><div className="mt-3 rounded-xl border border-slate-100 bg-white p-4"><div className="mb-5 flex justify-between text-xs"><span className="font-bold">Sales at a glance</span><span className="text-slate-400">Illustration</span></div><div className="flex h-28 items-end justify-between gap-2">{[35,55,42,76,62,90,72,97,79,92,68,100].map((h,i)=><div key={i} className="w-full rounded-t-md bg-gradient-to-t from-[#177657] to-[#a8d5a3]" style={{height:`${h}%`}}/>)}</div><div className="mt-2 flex justify-between text-[10px] text-slate-400"><span>Mon</span><span>Wed</span><span>Fri</span><span>Sun</span></div></div><div className="mt-3 flex gap-3 rounded-xl bg-[#f0f7ed] p-3 text-xs font-semibold text-[#266a4d]"><PackageCheck size={18}/> Your stock and sales, together.</div></div></div></div></div><div className="absolute -bottom-7 -left-2 hidden items-center gap-3 rounded-2xl border border-emerald-100 bg-white px-5 py-4 shadow-xl sm:flex"><div className="rounded-xl bg-emerald-50 p-3 text-emerald-600"><Zap size={23}/></div><div><div className="text-sm font-extrabold">Less busywork</div><div className="text-xs text-slate-500">More room to grow</div></div></div></div>
      </div></section>
      <section className="border-y border-[#e4ece4] bg-white px-5 py-8 sm:px-8"><div className="mx-auto flex max-w-7xl flex-wrap items-center justify-center gap-x-14 gap-y-5 text-sm font-bold text-[#668073]"><span>EVERYDAY BUSINESS, SIMPLIFIED</span><span className="inline-flex items-center gap-2"><ShieldCheck size={19}/> Role-based access</span><span className="inline-flex items-center gap-2"><Cloud size={19}/> Connected workspace</span><span className="inline-flex items-center gap-2"><Clock3 size={19}/> Time back in your day</span></div></section>
      <section id="features" className="scroll-mt-20 px-5 py-24 sm:px-8">
        <div className="mx-auto max-w-7xl">
          <div className="mx-auto max-w-3xl text-center">
            <span className="block text-center text-sm font-bold uppercase tracking-[.22em] text-emerald-600">One clear place</span>
            <h2 className="mx-auto mt-4 text-center text-4xl font-black tracking-[-.05em] text-[#173d31] sm:text-5xl">All the moving parts. Finally in sync.</h2>
            <p className="mx-auto mt-5 max-w-2xl text-center text-lg leading-7 text-[#61756b]">Everything you need to feel more in control of a busy day—without making business software feel like another full-time job.</p>
          </div>
          <div className="mt-11 grid items-start gap-5 md:grid-cols-2 lg:grid-cols-3">
            {features.map(({ icon: Icon, title, text }, i) => {
              const expanded = expandedFeature === i;
              return (
                <article key={title} className={`overflow-hidden rounded-[26px] border bg-white text-left shadow-sm transition-colors ${expanded ? 'border-emerald-400 shadow-lg shadow-emerald-900/10' : 'border-[#e2e9e0] hover:border-emerald-300 hover:shadow-md'}`}>
                  <button
                    type="button"
                    className="block w-full cursor-pointer p-7 text-left focus-visible:outline-2 focus-visible:outline-offset-[-4px] focus-visible:outline-emerald-500"
                    aria-expanded={expanded}
                    aria-controls={`feature-details-${i}`}
                    onClick={() => setExpandedFeature(expanded ? null : i)}
                  >
                    <span className={`inline-flex rounded-2xl p-4 ${i % 3 === 0 ? 'bg-[#e6f6da] text-[#217254]' : i % 3 === 1 ? 'bg-[#e6eeff] text-[#4a72a9]' : 'bg-[#fff3dc] text-[#a37332]'}`}><Icon size={25}/></span>
                    <span className="mt-7 block text-xl font-extrabold tracking-tight">{title}</span>
                    <span className="mt-3 block text-[15px] leading-7 text-slate-500">{text}</span>
                    <span className="mt-5 flex items-center gap-2 text-sm font-bold text-emerald-700">
                      {expanded ? 'Show less' : 'See how it helps'}
                      <ChevronDown size={17} className={`transition-transform duration-200 ${expanded ? 'rotate-180' : ''}`}/>
                    </span>
                  </button>
                  {expanded && (
                    <div id={`feature-details-${i}`} className="border-t border-emerald-100 bg-[#f4faf1] px-7 pb-7 pt-5">
                      <p className="mb-3 text-sm font-bold text-[#255843]">What you can do</p>
                      <ul className="space-y-3 text-sm leading-6 text-[#49665a]">
                        {featureDetails[i].map(detail => (
                          <li key={detail} className="flex items-start gap-2">
                            <Check size={17} className="mt-1 shrink-0 text-emerald-600"/>
                            <span>{detail}</span>
                          </li>
                        ))}
                      </ul>
                      <Link to="/register" className="mt-5 inline-flex items-center gap-2 font-bold text-emerald-800 underline-offset-4 hover:underline">
                        Explore TenexERP <ArrowRight size={16}/>
                      </Link>
                    </div>
                  )}
                </article>
              );
            })}
          </div>
        </div>
      </section>
      <section id="how-it-works" className="scroll-mt-20 bg-[#113f34] px-5 py-24 text-white sm:px-8"><div className="mx-auto max-w-7xl"><div className="grid gap-12 lg:grid-cols-[.9fr_1.1fr]"><div><span className="text-sm font-bold uppercase tracking-[.22em] text-emerald-300">A simpler way forward</span><h2 className="mt-4 text-4xl font-black tracking-[-.045em] sm:text-5xl">Make room for what really matters.</h2><p className="mt-5 max-w-lg leading-8 text-emerald-100/75">You didn’t start a business to spend your day switching tabs and chasing numbers. Bring your daily operations together and make your next decision with more confidence.</p><Link to="/register" className="mt-8 inline-flex items-center gap-3 rounded-xl bg-[#c9efb7] px-6 py-4 font-extrabold text-[#143c31] hover:bg-white">Create your account <ArrowRight size={18}/></Link></div><div className="space-y-4">{[['01','Create your space','Sign up and create your company workspace.'],['02','Bring your team together','Set up the people and permissions that fit the way you work.'],['03','Get on with business','Track stock, sales, and the work that keeps everything moving.']].map(([n,h,p])=><div key={n} className="flex gap-6 rounded-2xl border border-white/15 bg-white/5 p-6"><div className="text-3xl font-black text-[#b8e5a5]">{n}</div><div><h3 className="text-xl font-bold">{h}</h3><p className="mt-2 text-sm leading-6 text-emerald-100/75">{p}</p></div></div>)}</div></div></div></section>
      <section id="questions" className="scroll-mt-20 px-5 py-24 sm:px-8">
        <div className="mx-auto max-w-4xl">
          <div className="mx-auto mb-10 max-w-2xl text-center">
            <span className="block text-center text-sm font-bold uppercase tracking-[.22em] text-emerald-600">Good to know</span>
            <h2 className="mx-auto mt-4 text-center text-4xl font-black tracking-[-.04em] sm:text-5xl">A few things you might be wondering.</h2>
            <p className="mx-auto mt-5 text-center leading-7 text-slate-500">We're building practical software for real people running real businesses.</p>
          </div>
          <div className="space-y-3 text-left">
            {questions.map(([q, a], i) => (
              <div key={q} className="overflow-hidden rounded-2xl border border-[#dfe8df] bg-white">
                <h3>
                  <button
                    type="button"
                    className="flex w-full cursor-pointer items-center justify-between gap-5 px-6 py-5 text-left font-bold focus-visible:outline-2 focus-visible:outline-offset-[-2px] focus-visible:outline-emerald-500"
                    onClick={() => setOpened(opened === i ? null : i)}
                    aria-expanded={opened === i}
                    aria-controls={`faq-answer-${i}`}
                  >
                    {q}<ChevronDown className={`shrink-0 transition-transform ${opened === i ? 'rotate-180' : ''}`} size={18}/>
                  </button>
                </h3>
                {opened === i && <p id={`faq-answer-${i}`} className="px-6 pb-6 text-sm leading-7 text-slate-600">{a}</p>}
              </div>
            ))}
          </div>
        </div>
      </section>
      <section className="px-5 pb-24 sm:px-8">
        <div className="mx-auto flex max-w-7xl flex-col items-center overflow-hidden rounded-[32px] bg-[#dff1d6] px-6 py-16 text-center sm:px-16">
          <span className="inline-flex items-center justify-center gap-2 text-center text-sm font-bold text-emerald-700"><LockKeyhole size={17}/> Get started without payment setup</span>
          <h2 className="mx-auto mt-4 w-full max-w-2xl text-center text-4xl font-black tracking-[-.05em] sm:text-5xl">Your next chapter starts with a simpler day.</h2>
          <p className="mx-auto mt-5 w-full max-w-lg text-center leading-7 text-[#486753]">Discover what it feels like when the little things run smoother. Early-access trial terms will be confirmed in your account as we launch subscriptions.</p>
          <Link to="/register" className="mt-8 inline-flex items-center justify-center gap-2 rounded-2xl bg-[#143f33] px-8 py-4 text-center font-bold text-white transition hover:bg-[#24634e] focus-visible:outline-2 focus-visible:outline-offset-4 focus-visible:outline-emerald-700">Start exploring TenexERP <ArrowRight size={18}/></Link>
        </div>
      </section>
    </main>
    <footer className="border-t border-[#e3eae0] bg-white px-5 py-12 sm:px-8">
      <div className="mx-auto grid w-full max-w-7xl grid-cols-1 items-start gap-x-12 gap-y-8 text-left md:grid-cols-[minmax(0,1fr)_auto] md:items-center">
        <div className="min-w-0">
          <Link to="/" className="inline-flex items-center gap-2.5 text-xl font-black tracking-[-.045em] text-[#173d31]" aria-label="TenexERP home">
            <span className="flex h-10 w-10 items-center justify-center rounded-xl bg-[#163f35] text-white"><Boxes size={21}/></span>
            <span>Tenex<span className="text-emerald-600">ERP</span></span>
          </Link>
          <p className="mt-3 max-w-sm text-left text-sm leading-6 text-slate-500">Helping growing businesses find their flow.</p>
        </div>
        <nav aria-label="Footer navigation" className="grid grid-cols-2 gap-x-6 gap-y-4 text-sm font-semibold text-slate-600 sm:grid-cols-4">
          <Link to="/privacy-policy" className="inline-flex items-center gap-2 whitespace-nowrap transition hover:text-emerald-700 focus-visible:underline"><ShieldCheck size={17} aria-hidden="true"/> Privacy</Link>
          <Link to="/terms-of-service" className="inline-flex items-center gap-2 whitespace-nowrap transition hover:text-emerald-700 focus-visible:underline"><FileText size={17} aria-hidden="true"/> Terms</Link>
          <a href="mailto:support@tenexerp.com" className="inline-flex items-center gap-2 whitespace-nowrap transition hover:text-emerald-700 focus-visible:underline"><Mail size={17} aria-hidden="true"/> Contact</a>
          <Link to="/login" className="inline-flex items-center gap-2 whitespace-nowrap transition hover:text-emerald-700 focus-visible:underline"><LogIn size={17} aria-hidden="true"/> Log in</Link>
        </nav>
        <div className="border-t border-[#e3eae0] pt-6 text-left text-xs text-slate-500 md:col-span-2">
          © {new Date().getFullYear()} TenexERP. Made for the businesses that make every day happen.
        </div>
      </div>
    </footer>
  </div>;
}
