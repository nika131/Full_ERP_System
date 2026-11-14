
import { Info } from 'lucide-react';

type Props = {
  name: string;
  deletedRecently?: boolean;
};

export function HistoricalUserName({
  name,
  deletedRecently = false,
}: Props) {
  return (
    <span className="inline-flex items-center gap-1">
      <span>{name}</span>

      {deletedRecently && (
        <span
          className="inline-flex cursor-help items-center text-slate-400"
          title="This employee was deleted. Their name will be hidden from historical records 30 days after deletion."
          aria-label="Recently deleted employee"
        >
          <Info size={14} aria-hidden="true" />
        </span>
      )}
    </span>
  );
}
