import { useEffect, useRef, useState } from "react";
import { Check, ChevronDown } from "lucide-react";

export interface MultiSelectDropdownProps {
  options: { id: number; name: string }[];
  selectedIds: number[];
  onChange: (ids: number[]) => void;
  placeholder: string;
}

export function MultiSelectDropdown({ 
  options, selectedIds, onChange, placeholder 
}: { 
  options: { id: number, name: string }[], 
  selectedIds: number[], 
  onChange: (ids: number[]) => void, 
  placeholder: string 
}) {
  const [isOpen, setIsOpen] = useState(false);
  const dropdownRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (dropdownRef.current && !dropdownRef.current.contains(event.target as Node)) {
        setIsOpen(false);
      }
    };
    document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, []);

  const toggleOption = (id: number) => {
    if (selectedIds.includes(id)) {
      onChange(selectedIds.filter(v => v !== id));
    } else {
      onChange([...selectedIds, id]);
    }
  };

  return (
    <div className="relative w-full h-10" ref={dropdownRef}>
      <div 
        className="border border-slate-300 rounded bg-white w-full h-full px-3 py-2 text-sm flex justify-between items-center cursor-pointer"
        onClick={() => setIsOpen(!isOpen)}
      >
        <span className="truncate text-slate-600">
          {selectedIds.length > 0 
            ? `${selectedIds.length} selected` 
            : placeholder}
        </span>
        <ChevronDown size={16} className={`text-slate-400 shrink-0 transition-transform ${isOpen ? 'rotate-180' : ''}`} />
      </div>
      
      {isOpen && (
        <div className="absolute z-10 w-full mt-1 bg-white border border-slate-200 rounded shadow-lg max-h-60 overflow-y-auto">
          {options.length === 0 ? (
            <div className="px-3 py-3 text-sm text-slate-400 text-center">No options available</div>
          ) : (
            options.map(opt => (
              <div 
                key={opt.id} 
                className="px-3 py-2 flex items-center gap-2 hover:bg-slate-50 cursor-pointer text-sm"
                onClick={() => toggleOption(opt.id)}
              >
                <div className={`w-4 h-4 border rounded flex items-center justify-center shrink-0 transition-colors ${
                  selectedIds.includes(opt.id) 
                    ? 'bg-emerald-500 border-emerald-500 text-white' 
                    : 'border-slate-300 bg-white'
                }`}>
                  {selectedIds.includes(opt.id) && <Check size={12} strokeWidth={3} />}
                </div>
                <span className="truncate text-slate-700">{opt.name}</span>
              </div>
            ))
          )}
        </div>
      )}
    </div>
  );
}