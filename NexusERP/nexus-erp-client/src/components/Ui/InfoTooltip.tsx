
import { useEffect, useId, useRef, useState } from 'react';
import { createPortal } from 'react-dom';
import { CircleAlert } from 'lucide-react';

interface InfoTooltipProps {
  text: string;
}

export function InfoTooltip({ text }: InfoTooltipProps) {
  const id = useId();
  const buttonRef = useRef<HTMLButtonElement>(null);
  const [open, setOpen] = useState(false);
  const [position, setPosition] = useState({
    top: 0,
    left: 0,
    below: false,
  });

  const show = () => {
    const rect = buttonRef.current?.getBoundingClientRect();
    if (!rect) return;

    const width = Math.min(288, window.innerWidth - 24);
    const half = width / 2;

    const center = Math.max(
      half + 12,
      Math.min(window.innerWidth - half - 12, rect.left + rect.width / 2)
    );

    const below = rect.top < 160;

    setPosition({
      left: center,
      top: below ? rect.bottom + 9 : rect.top - 9,
      below,
    });

    setOpen(true);
  };

  useEffect(() => {
    if (!open) return;

    const close = () => setOpen(false);

    const onKeyDown = (event: KeyboardEvent) => {
      if (event.key === 'Escape') close();
    };

    const onPointerDown = (event: PointerEvent) => {
      if (!buttonRef.current?.contains(event.target as Node)) {
        close();
      }
    };

    document.addEventListener('keydown', onKeyDown);
    document.addEventListener('pointerdown', onPointerDown);
    window.addEventListener('scroll', close, true);
    window.addEventListener('resize', close);

    return () => {
      document.removeEventListener('keydown', onKeyDown);
      document.removeEventListener('pointerdown', onPointerDown);
      window.removeEventListener('scroll', close, true);
      window.removeEventListener('resize', close);
    };
  }, [open]);

  return (
    <>
      <button
        ref={buttonRef}
        type="button"
        aria-label="More information"
        aria-describedby={open ? id : undefined}
        aria-expanded={open}
        onMouseEnter={show}
        onMouseLeave={() => setOpen(false)}
        onFocus={show}
        onBlur={() => setOpen(false)}
        onClick={show}
        className="inline-flex h-6 w-6 shrink-0 items-center justify-center rounded-full text-slate-400 transition-colors hover:text-slate-600 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-emerald-600"
      >
        <CircleAlert size={16} strokeWidth={2} aria-hidden="true" />
      </button>

      {open &&
        createPortal(
          <div
            id={id}
            role="tooltip"
            className="pointer-events-none fixed z-[9999] w-max max-w-[min(18rem,calc(100vw-1.5rem))] rounded-xl border border-slate-700 bg-slate-900 px-3 py-2.5 text-left text-xs font-normal leading-5 text-white shadow-xl"
            style={{
              left: position.left,
              top: position.top,
              transform: position.below
                ? 'translateX(-50%)'
                : 'translate(-50%, -100%)',
            }}
          >
            {text}
          </div>,
          document.body
        )}
    </>
  );
}
