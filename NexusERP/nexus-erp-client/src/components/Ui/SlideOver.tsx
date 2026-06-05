import { type ReactNode, useEffect } from 'react';
import { X } from 'lucide-react';

interface SlideOverProps {
    isOpen: boolean;
    onClose: () => void;
    title: string;
    children: ReactNode;
}

export function SlideOver({ isOpen, onClose, title, children }: SlideOverProps) {
    useEffect(() => {
        if (isOpen) {
            document.body.style.overflow = 'hidden';
        } else {
            document.body.style.overflow = 'unset';
        }

        return () => {
            document.body.style.overflow = 'unset';
        };
    }, [isOpen] );

    if (!isOpen) return null;

    return (
        <div className="fixed inset-0 z-50 overflow-hidden">
            {/* Darkened overlay background */}
            <div 
                className="absolute inset-0 bg-slate-900/50 transition-opacity"
                onClick={onClose}
            />

            {/* Slide-over panel */}
            <div className="fixed inset-y-0 right-0 max-w-md w-full flex">
                <div className="w-full h-full bg-white shadow-2xl flex flex-col animate-in slide-in-from-right duration-300">
                
                {/* Header */}
                <div className="px-6 py-4 border-b border-slate-200 flex justify-between items-center bg-slate-50">
                    <h2 className="text-lg font-semibold text-slate-800">{title}</h2>
                    <button 
                    onClick={onClose}
                    className="p-2 text-slate-400 hover:text-slate-600 hover:bg-slate-200 rounded-full transition-colors"
                    >
                    <X size={20} />
                    </button>
                </div>

                {/* Dynamic Content (The Form) */}
                <div className="flex-1 overflow-y-auto p-6">
                    {children}
                </div>

                </div>
            </div>
        </div>
    );
}