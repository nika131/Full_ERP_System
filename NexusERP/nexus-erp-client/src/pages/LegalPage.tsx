import { Link } from 'react-router-dom';
import type { ReactNode } from 'react';

interface LegalPageProps {
    title: string;
    children: ReactNode;
}

export default function LegalPage({ title, children }: LegalPageProps) {
    return (
        <div className="min-h-screen bg-gray-50">
            <header className="border-b border-gray-200 bg-white">
                <div className="max-w-4xl mx-auto px-6 py-4 flex items-center justify-between">
                    <Link
                        to="/login"
                        className="text-xl font-bold tracking-wide text-emerald-600"
                    >
                        NEXUS ERP
                    </Link>

                    <nav className="flex items-center gap-4 text-sm">
                        <Link
                            to="/privacy-policy"
                            className="text-gray-600 hover:text-emerald-600"
                        >
                            Privacy Policy
                        </Link>

                        <Link
                            to="/terms-of-service"
                            className="text-gray-600 hover:text-emerald-600"
                        >
                            Terms of Service
                        </Link>

                        <a
                            href="mailto:support@tenexerp.com"
                            className="text-gray-600 hover:text-emerald-600"
                        >
                            Support
                        </a>
                    </nav>
                </div>
            </header>

            <main className="max-w-4xl mx-auto px-6 py-10">
                <div className="bg-white border border-gray-200 rounded-xl shadow-sm p-6 md:p-10">
                    <h1 className="text-3xl font-bold text-gray-900">
                        {title}
                    </h1>

                    <p className="text-sm text-gray-500 mt-1 mb-8">
                        TenexERP · Last updated: September 30, 2026
                    </p>

                    <div className="space-y-6 text-gray-700 leading-7">
                        {children}
                    </div>
                </div>
            </main>

            <footer className="border-t border-gray-200 bg-white">
                <div className="max-w-4xl mx-auto px-6 py-6 text-sm text-gray-500 flex flex-wrap gap-x-5 gap-y-2">
                    <span>© {new Date().getFullYear()} TenexERP</span>

                    <Link
                        to="/privacy-policy"
                        className="hover:text-emerald-600"
                    >
                        Privacy Policy
                    </Link>

                    <Link
                        to="/terms-of-service"
                        className="hover:text-emerald-600"
                    >
                        Terms of Service
                    </Link>

                    <a
                        href="mailto:support@tenexerp.com"
                        className="hover:text-emerald-600"
                    >
                        Support
                    </a>
                </div>
            </footer>
        </div>
    );
}