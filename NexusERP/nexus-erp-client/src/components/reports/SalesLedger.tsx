import { useEffect, useState } from 'react';
import { createPortal } from 'react-dom';
import { useQuery } from '@tanstack/react-query';
import { ChevronLeft, ChevronRight, Search, X } from 'lucide-react';
import apiClient from '../../api/apiClient';
import type { DashboardFilters } from '../../pages/Dashboard';

type ReceiptRow = {
  receiptId: number;
  receiptNumber: string;
  createdAt: string;
  storeName: string;
  cashierName: string;
  paymentMethod: string;
  total: number;
  profit: number;
};

type ProductRow = {
  itemId: number;
  receiptId: number;
  receiptNumber: string;
  createdAt: string;
  productId: number | null;
  productName: string;
  productDeletedRecently: boolean;
  quantity: number;
  unitPrice: number;
  total: number;
  profit: number;
};

type LedgerPage =
  | { mode: 'receipts'; items: ReceiptRow[]; page: number; hasMorePages: boolean }
  | { mode: 'products'; items: ProductRow[]; page: number; hasMorePages: boolean };

type ProductOption = { productId: number; name: string };

type ReceiptDetail = {
  receiptId: number;
  receiptNumber: string;
  createdAt: string;
  storeName: string;
  cashierName: string;
  paymentMethod: string;
  subTotal: number;
  cartDiscountPercentage: number;
  totalVatAmount: number;
  finalTotal: number;
  profit: number;
  lines: {
    itemId: number;
    productId: number | null;
    productName: string;
    productDeletedRecently: boolean;
    quantity: number;
    unitPrice: number;
    marketDiscountPercentage: number;
    manualItemDiscountPercentage: number;
    lineTotal: number;
  }[];
};

function money(amount: number) {
  return `${amount < 0 ? '−' : ''}$${Math.abs(amount).toLocaleString(undefined, {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  })}`;
}

function dateTime(value: string) {
  return new Date(value).toLocaleString(undefined, {
    year: 'numeric', month: 'short', day: 'numeric',
    hour: '2-digit', minute: '2-digit', second: '2-digit',
  });
}

function localMidnightUtc(day: string | null, addDay = false) {
  if (!day) return null;
  const local = new Date(`${day}T00:00:00`);
  if (!Number.isFinite(local.getTime())) return null;
  if (addDay) local.setDate(local.getDate() + 1);
  return local.toISOString();
}

function salesParams(filters: DashboardFilters, productIds: number[], search: string, page: number) {
  const params = new URLSearchParams();
  params.set('page', String(page));
  params.set('pageSize', '10');
  params.set('utcOffsetMinutes', String(new Date().getTimezoneOffset()));

  const fromUtc = localMidnightUtc(filters.startDate);
  const toUtcExclusive = localMidnightUtc(filters.endDate, true);
  if (fromUtc) params.set('fromUtc', fromUtc);
  if (toUtcExclusive) params.set('toUtcExclusive', toUtcExclusive);
  if (filters.startHour) params.set('startHour', filters.startHour);
  if (filters.endHour) params.set('endHour', filters.endHour);

  for (const id of filters.storeIds) params.append('storeIds', String(id));
  for (const id of filters.employeeIds) params.append('employeeIds', String(id));
  for (const id of filters.categoryIds) params.append('categoryIds', String(id));
  for (const id of filters.supplierIds) params.append('supplierIds', String(id));
  for (const id of productIds) params.append('productIds', String(id));
  if (search.trim()) params.set('search', search.trim());
  return params;
}

export function SalesLedger({ filters }: { filters: DashboardFilters }) {
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState('');
  const [productTerm, setProductTerm] = useState('');
  const [debouncedProductTerm, setDebouncedProductTerm] = useState('');
  const [productMenuOpen, setProductMenuOpen] = useState(false);
  const [selectedProducts, setSelectedProducts] = useState<ProductOption[]>([]);
  const [receiptId, setReceiptId] = useState<number | null>(null);

  const productIds = selectedProducts.map(p => p.productId);
  const productMode = filters.categoryIds.length > 0 || filters.supplierIds.length > 0 || productIds.length > 0;

  useEffect(() => {
    const timer = window.setTimeout(() => setDebouncedProductTerm(productTerm), 250);
    return () => window.clearTimeout(timer);
  }, [productTerm]);

  useEffect(() => setPage(1), [filters, selectedProducts, search]);

  useEffect(() => {
    if (receiptId === null) return;
    const closeOnEsc = (event: KeyboardEvent) => {
      if (event.key === 'Escape') setReceiptId(null);
    };
    document.addEventListener('keydown', closeOnEsc);
    return () => document.removeEventListener('keydown', closeOnEsc);
  }, [receiptId]);

  const pageQuery = useQuery({
    queryKey: ['sales-ledger', filters, productIds, search, page],
    queryFn: async ({ signal }) => {
      const params = salesParams(filters, productIds, search, page);
      const { data } = await apiClient.get<LedgerPage>(`/sales-ledger?${params.toString()}`, { signal });
      return data;
    },
  });

  const productsQuery = useQuery({
    queryKey: ['sales-ledger', 'product-options', debouncedProductTerm],
    queryFn: async ({ signal }) => {
      const { data } = await apiClient.get<ProductOption[]>('/sales-ledger/products', {
        params: { search: debouncedProductTerm }, signal,
      });
      return data;
    },
    enabled: productMenuOpen,
    staleTime: 60_000,
  });

  const detailQuery = useQuery({
    queryKey: ['sales-ledger', 'receipt', receiptId],
    queryFn: async ({ signal }) => {
      const { data } = await apiClient.get<ReceiptDetail>(`/sales-ledger/receipts/${receiptId}`, { signal });
      return data;
    },
    enabled: receiptId !== null,
    staleTime: 60_000,
  });

  const chooseProduct = (product: ProductOption) => {
    setSelectedProducts(old => old.some(x => x.productId === product.productId) ? old : [...old, product]);
    setProductTerm('');
    setProductMenuOpen(false);
  };

  const pageData = pageQuery.data;
  const detail = detailQuery.data;
  const itemDiscounts = detail
    ? detail.subTotal - detail.lines.reduce((n, x) => n + x.lineTotal, 0) : 0;
  const cartDiscounts = detail
    ? detail.lines.reduce((n, x) => n + x.lineTotal, 0) - detail.finalTotal : 0;

  return (
    <section className="space-y-4 text-left" aria-label="Sales ledger">
      {/* Compact toolbar: title + mode on the left, both search controls aligned on the right */}
      <div className="flex flex-col gap-3 xl:flex-row xl:items-center xl:justify-between">
        <div className="flex shrink-0 items-center gap-2.5">
          <h3 className="!m-0 text-lg font-bold text-slate-800">Sales Ledger</h3>
          <span className="rounded-full bg-slate-100 px-2.5 py-1 text-xs font-medium text-slate-600">
            {productMode ? 'Products' : 'Receipts'}
          </span>
        </div>

        <div className="flex w-full flex-col gap-2 sm:flex-row xl:w-auto xl:items-center">
          <div className="relative w-full sm:min-w-0 sm:flex-1 xl:w-56 xl:flex-none" onBlur={event => {
            if (!event.currentTarget.contains(event.relatedTarget as Node | null)) setProductMenuOpen(false);
          }}>
            <label htmlFor="ledger-product-search" className="sr-only">Filter by product</label>
            <input
              id="ledger-product-search"
              type="search"
              autoComplete="off"
              value={productTerm}
              onFocus={() => setProductMenuOpen(true)}
              onChange={event => { setProductTerm(event.target.value); setProductMenuOpen(true); }}
              placeholder="Filter products..."
              className="h-10 w-full rounded-lg border border-slate-300 bg-white px-3 text-sm text-slate-800 outline-none focus:border-emerald-500"
            />
            {productMenuOpen && (
              <div className="tenex-scroll absolute left-0 right-0 top-full z-40 mt-1 max-h-52 overflow-y-auto rounded-lg border border-slate-200 bg-white py-1 shadow-xl">
                {productsQuery.isPending ? (
                  <p className="px-3 py-2 text-xs text-slate-500">Loading products...</p>
                ) : productsQuery.isError ? (
                  <p className="px-3 py-2 text-xs text-red-600">Failed to load products.</p>
                ) : productsQuery.data?.length ? (
                  productsQuery.data.filter(p => !productIds.includes(p.productId)).map(p => (
                    <button
                      type="button" key={p.productId}
                      onClick={() => chooseProduct(p)}
                      className="block w-full px-3 py-2 text-left text-sm text-slate-700 hover:bg-emerald-50"
                    >{p.name}</button>
                  ))
                ) : (
                  <p className="px-3 py-2 text-xs text-slate-500">No sold products found.</p>
                )}
              </div>
            )}
          </div>

          <div className="relative w-full sm:min-w-0 sm:flex-1 xl:w-72 xl:flex-none">
            <Search size={16} className="pointer-events-none absolute left-3 top-3 text-slate-400" aria-hidden="true" />
            <input
              type="search"
              aria-label="Search receipts or products"
              value={search}
              onChange={event => setSearch(event.target.value)}
              placeholder={productMode ? 'Product or receipt...' : 'Receipt ID or number...'}
              className="h-10 w-full rounded-lg border border-slate-300 bg-white pl-9 pr-3 text-sm text-slate-800 outline-none focus:border-emerald-500"
            />
          </div>
        </div>
      </div>

      {selectedProducts.length > 0 && (
        <div className="flex max-w-full flex-wrap items-center gap-1.5">
          {selectedProducts.map(p => (
            <button
              key={p.productId}
              type="button"
              onClick={() => setSelectedProducts(old => old.filter(x => x.productId !== p.productId))}
              className="inline-flex items-center gap-1 rounded-full border border-emerald-200 bg-emerald-50 px-2.5 py-1 text-xs font-semibold text-emerald-800 hover:bg-emerald-100"
              aria-label={`Remove ${p.name} filter`}
            >{p.name}<X size={12} /></button>
          ))}
          <button type="button" onClick={() => setSelectedProducts([])} className="px-2 py-1 text-xs font-medium text-slate-600 hover:underline">Clear all</button>
        </div>
      )}

      <div className="overflow-hidden rounded-xl border border-slate-200 bg-white shadow-sm">
        <div className="tenex-scroll overflow-x-auto">
          <table className="w-full min-w-[850px] text-left text-sm">
            <thead className="border-b border-slate-200 bg-slate-50 text-xs uppercase tracking-wide text-slate-500">
              {productMode ? (
                <tr>
                  <th className="px-4 py-3">Sold at</th><th className="px-4 py-3">Receipt</th>
                  <th className="px-4 py-3">Product</th><th className="px-4 py-3 text-right">Qty</th>
                  <th className="px-4 py-3 text-right">Unit price</th>
                  <th className="px-4 py-3 text-right">Total</th><th className="px-4 py-3 text-right">Profit</th>
                </tr>
              ) : (
                <tr>
                  <th className="px-4 py-3">Sold at</th><th className="px-4 py-3">Receipt</th>
                  <th className="px-4 py-3">Store</th><th className="px-4 py-3">Cashier</th>
                  <th className="px-4 py-3">Payment</th>
                  <th className="px-4 py-3 text-right">Total</th><th className="px-4 py-3 text-right">Profit</th>
                </tr>
              )}
            </thead>
            <tbody className="divide-y divide-slate-100">
              {pageQuery.isPending ? (
                <tr><td colSpan={7} className="p-8 text-center text-slate-500">Loading sales...</td></tr>
              ) : pageQuery.isError ? (
                <tr><td colSpan={7} className="p-8 text-center text-red-600">Couldn't load sales. <button type="button" className="underline" onClick={() => void pageQuery.refetch()}>Retry</button></td></tr>
              ) : !pageData?.items.length ? (
                <tr><td colSpan={7} className="p-8 text-center text-slate-500">No matching sales found.</td></tr>
              ) : pageData.mode === 'receipts' ? (
                pageData.items.map(r => (
                  <tr key={r.receiptId} onClick={() => setReceiptId(r.receiptId)} className="cursor-pointer hover:bg-slate-50">
                    <td className="whitespace-nowrap px-4 py-3 text-slate-700">{dateTime(r.createdAt)}</td>
                    <td className="px-4 py-3">
                      <div className="flex items-baseline gap-2 whitespace-nowrap">
                        <button type="button" onClick={() => setReceiptId(r.receiptId)} className="font-semibold text-emerald-700 hover:underline" title="View full receipt">
                          #{r.receiptNumber}
                        </button>
                        <span className="text-xs text-slate-400">ID {r.receiptId}</span>
                      </div>
                    </td>
                    <td className="px-4 py-3">{r.storeName}</td><td className="px-4 py-3">{r.cashierName}</td>
                    <td className="px-4 py-3">{r.paymentMethod}</td>
                    <td className="px-4 py-3 text-right font-semibold">{money(r.total)}</td>
                    <td className="px-4 py-3 text-right font-semibold text-emerald-700">{money(r.profit)}</td>
                  </tr>
                ))
              ) : (
                pageData.items.map(r => (
                  <tr key={r.itemId} className="hover:bg-slate-50">
                    <td className="whitespace-nowrap px-4 py-3 text-slate-700">{dateTime(r.createdAt)}</td>
                    <td className="px-4 py-3">
                      <div className="flex items-baseline gap-2 whitespace-nowrap">
                        <button type="button" onClick={() => setReceiptId(r.receiptId)} className="font-semibold text-emerald-700 hover:underline" title="View full receipt">
                          #{r.receiptNumber}
                        </button>
                        <span className="text-xs text-slate-400">ID {r.receiptId}</span>
                      </div>
                    </td>
                    <td className="px-4 py-3 font-medium text-slate-700">{r.productName}{r.productDeletedRecently && <span className="ml-1 cursor-help text-slate-400" title="This product was deleted and will be removed from the catalog after 30 days.">ⓘ</span>}</td>
                    <td className="px-4 py-3 text-right">{r.quantity}</td>
                    <td className="px-4 py-3 text-right">{money(r.unitPrice)}</td>
                    <td className="px-4 py-3 text-right font-semibold">{money(r.total)}</td>
                    <td className="px-4 py-3 text-right font-semibold text-emerald-700">{money(r.profit)}</td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
        <div className="flex items-center justify-end gap-2 border-t border-slate-200 bg-slate-50 px-4 py-2.5">
          <span className="mr-1 text-xs font-medium text-slate-500">Page {page}</span>
          <button
            type="button"
            aria-label="Previous page"
            title="Previous page"
            disabled={page === 1 || pageQuery.isFetching}
            onClick={() => setPage(p => Math.max(1, p - 1))}
            className="flex h-8 w-8 items-center justify-center rounded-lg border border-slate-200 bg-white text-slate-600 hover:bg-slate-100 disabled:cursor-not-allowed disabled:opacity-40"
          ><ChevronLeft size={17} /></button>
          <button
            type="button"
            aria-label="Next page"
            title="Next page"
            disabled={!pageData?.hasMorePages || pageQuery.isFetching}
            onClick={() => setPage(p => p + 1)}
            className="flex h-8 w-8 items-center justify-center rounded-lg border border-slate-200 bg-white text-slate-600 hover:bg-slate-100 disabled:cursor-not-allowed disabled:opacity-40"
          ><ChevronRight size={17} /></button>
        </div>
      </div>

      {receiptId !== null && createPortal(
        <div className="fixed inset-0 z-[100] flex items-center justify-center bg-slate-950/60 p-3 sm:p-6" onMouseDown={event => {
          if (event.target === event.currentTarget) setReceiptId(null);
        }}>
          <div role="dialog" aria-modal="true" aria-label="Receipt details" className="flex max-h-[calc(100dvh-1.5rem)] w-full max-w-xl flex-col overflow-hidden rounded-2xl bg-white text-left shadow-2xl">
            <div className="flex shrink-0 items-center justify-between border-b border-slate-200 px-5 py-4">
              <strong className="text-lg text-slate-800">Receipt details</strong>
              <button type="button" onClick={() => setReceiptId(null)} className="rounded-lg p-2 text-slate-500 hover:bg-slate-100" aria-label="Close receipt"><X size={20} /></button>
            </div>
            <div className="tenex-scroll min-h-0 overflow-y-auto p-5 sm:p-6">
              {detailQuery.isPending ? <p className="text-sm text-slate-500">Loading receipt...</p> : detailQuery.isError ? <p className="text-sm text-red-600">Could not load receipt.</p> : detail && (
                <div className="space-y-4 text-sm">
                  <div className="text-center">
                    <h4 className="!m-0 text-xl font-extrabold text-slate-800">{detail.storeName}</h4>
                    <p className="mt-1 font-mono text-sm">#{detail.receiptNumber} · ID {detail.receiptId}</p>
                    <p className="mt-1 text-xs text-slate-500">{dateTime(detail.createdAt)} · {detail.cashierName}</p>
                  </div>
                  <div className="space-y-3 border-y border-slate-200 py-4">
                    {detail.lines.map(l => (
                      <div key={l.itemId} className="flex justify-between gap-4">
                        <div>
                          <p className="font-semibold text-slate-800">{l.productName}{l.productDeletedRecently && <span className="ml-1 cursor-help text-slate-400" title="This product was deleted and will be removed from the catalog after 30 days.">ⓘ</span>}</p>
                          <p className="text-xs text-slate-500">{l.quantity} × {money(l.unitPrice)}</p>
                          {l.marketDiscountPercentage > 0 && <p className="text-xs text-amber-700">Market discount {l.marketDiscountPercentage}%</p>}
                          {l.manualItemDiscountPercentage > 0 && <p className="text-xs text-amber-700">Manual discount {l.manualItemDiscountPercentage}%</p>}
                        </div>
                        <span className="shrink-0 font-semibold text-slate-800">{money(l.lineTotal)}</span>
                      </div>
                    ))}
                  </div>
                  <div className="space-y-2">
                    <SummaryRow label="Original subtotal" value={detail.subTotal} />
                    {itemDiscounts > 0.0001 && <SummaryRow label="Item discounts" value={-itemDiscounts} />}
                    {cartDiscounts > 0.0001 && <SummaryRow label={`Cart discount (${detail.cartDiscountPercentage}%)`} value={-cartDiscounts} />}
                    <SummaryRow label="VAT (included)" value={detail.totalVatAmount} />
                    <div className="border-t border-slate-200 pt-3"><SummaryRow label="Final total" value={detail.finalTotal} strong /></div>
                    <SummaryRow label="Profit (after VAT)" value={detail.profit} strong />
                  </div>
                  <div className="text-center text-xs font-semibold text-emerald-700">Paid by {detail.paymentMethod}</div>
                </div>
              )}
            </div>
          </div>
        </div>, document.body
      )}
    </section>
  );
}

function SummaryRow({ label, value, strong = false }: { label: string; value: number; strong?: boolean }) {
  return (
    <div className={`flex items-center justify-between gap-4 ${strong ? 'font-bold text-slate-900' : 'text-slate-600'}`}>
      <span>{label}</span><span className="shrink-0">{money(value)}</span>
    </div>
  );
}
