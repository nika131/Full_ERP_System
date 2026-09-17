import { useEffect, useMemo, useState } from "react";
import type { Transaction } from "../types/transaction";
import { CursorDataTable, type ColumnDef } from "../components/Ui/CursorDataTable";
import { AlertCircle, DollarSign, FilterX, Package, TrendingUp } from "lucide-react";
import { AreaChart, Area, CartesianGrid, ResponsiveContainer, Tooltip, XAxis, YAxis, Bar, BarChart } from "recharts";
import { useDashboardStatsQuery, useChartDataQuery, useTopProductsQuery, useTransactionsQuery } from "../hooks/queries/useDashboardQueries";
import { StoreMapCanvas } from "../components/maps/StoreMapCanvas";
import { useLookupStoresQuery } from "../hooks/queries/useStoreQueries";
import { useLookupCategoriesQuery } from "../hooks/queries/useCategoryQueries";
import { useSupplierLookupQuery } from "../hooks/queries/useSupplierQueries";
import { useLookupUsersQuery } from "../hooks/queries/useHrQueries";
import DatePicker from "react-datepicker";
import { useNavigate } from "react-router-dom";
import { MultiSelectDropdown } from "../components/Ui/MultiSelectDropdown";

type TransactionCursorState = {
  createdAt: string | null;
  transactionId: number | null;
};

export type DashboardFilters = {
  startDate: string | null;
  endDate: string | null;
  startHour: string | null;
  endHour: string | null;
  storeIds: number[];
  categoryIds: number[];
  supplierIds: number[];
  employeeIds: number[];
};

// Generates an array of ["00:00", "00:30", "01:00" ... "23:30"]
const TIME_OPTIONS = Array.from({ length: 48 }, (_, i) => {
  const hour = Math.floor(i / 2).toString().padStart(2, '0');
  const minute = i % 2 === 0 ? '00' : '30';
  return `${hour}:${minute}`;
});

const defaultEnd = new Date();
const defaultStart = new Date();
defaultStart.setDate(defaultEnd.getDate() - 7);

export default function Dashboard() {
  const [dateRange, setDateRange] = useState<[Date | null, Date | null]>(() => {
    const saved = sessionStorage.getItem('dashboardFilters');
    if (saved) {
      const parsed = JSON.parse(saved) as DashboardFilters;
      return [
        parsed.startDate ? new Date(parsed.startDate) : defaultStart,
        parsed.endDate ? new Date(parsed.endDate) : defaultEnd
      ]
    }
    return [defaultStart, defaultEnd]
  });
  const [startDate, endDate] = dateRange;

  const [globalFilters, setGlobalFilters] = useState<DashboardFilters>(() => {
    const saved = sessionStorage.getItem('dashboardFilters');
    if (saved) {
        const parsed = JSON.parse(saved);
        return {
            startDate: parsed.startDate || defaultStart.toISOString().split('T')[0],
            endDate: parsed.endDate || defaultEnd.toISOString().split('T')[0],
            startHour: parsed.startHour || '',
            endHour: parsed.endHour || '',
            storeIds: Array.isArray(parsed.storeIds) ? parsed.storeIds : [],
            categoryIds: Array.isArray(parsed.categoryIds) ? parsed.categoryIds : [],
            supplierIds: Array.isArray(parsed.supplierIds) ? parsed.supplierIds : [],
            employeeIds: Array.isArray(parsed.employeeIds) ? parsed.employeeIds : []
        };
    }

    return {
        startDate: defaultStart.toISOString().split('T')[0],
        endDate: defaultEnd.toISOString().split('T')[0],
        startHour: '',
        endHour: '',
        storeIds: [],
        categoryIds: [],
        supplierIds: [],
        employeeIds: []
    };
  });

  const [cursorHistory, setCursorHistory] = useState<TransactionCursorState[]>([{ createdAt: null, transactionId: null }]);
  const [currentIndex, setCurrentIndex] = useState(0);

  const [searchTerm, setSearchTerm] = useState(() => {
    return sessionStorage.getItem('dashboaredSearch') || '';
  });

  useEffect(() => {
    sessionStorage.setItem('dashboardFilters', JSON.stringify(globalFilters))
  }, [globalFilters])

  useEffect(() => {
    sessionStorage.setItem('dashboardSearch', searchTerm)
  }, [searchTerm])

  const [mapCenter] = useState<[number, number]>([41.7151, 44.8271]);

  const { data: stats, isLoading: isStatsLoading } = useDashboardStatsQuery(globalFilters);
  const { data: chartData = [], isLoading: isChartLoading } = useChartDataQuery(globalFilters);
  const { data: topProducts = [], isLoading: isTopProductsLoading } = useTopProductsQuery(globalFilters);

  const { data: categories = [] } = useLookupCategoriesQuery();
  const { data: suppliers = [] } = useSupplierLookupQuery();
  const { data: storesLookup = [] } = useLookupStoresQuery();
  const { data: employees = [] } = useLookupUsersQuery();

  const navigate = useNavigate();

  const isLoadingStats = isStatsLoading || isChartLoading || isTopProductsLoading;

  const currentCursor = cursorHistory[currentIndex]

  const { data: transactionsData, isLoading: isLoadingLedger } = useTransactionsQuery(
    10,
    currentCursor.createdAt,
    currentCursor.transactionId,
    searchTerm,
    globalFilters
  );

  useEffect(() => {
    if (transactionsData?.hasMorePages && cursorHistory.length === currentIndex + 1) {
      setCursorHistory(prev => [
        ...prev,
        { 
          createdAt: transactionsData.nextCreatedAt ?? null, 
          transactionId: transactionsData.nextTransactionId ?? null 
        } 
      ]);
    }
  }, [transactionsData, currentIndex, cursorHistory.length]);

  useEffect(() => {
    setCursorHistory([{ createdAt: null, transactionId: null }]);
    setCurrentIndex(0);
  }, [globalFilters]);

  const handleSearchChange = (val: string) => {
    setSearchTerm(val);
    setCursorHistory([{ createdAt: null, transactionId: null }]);
    setCurrentIndex(0);
  };

  const handleNext = () => setCurrentIndex(prev => prev + 1);
  const handlePrevious = () => setCurrentIndex(prev => prev - 1);

  const clearFilters = () => {
    const today = new Date()
    setDateRange([today, today]);
    setSearchTerm('')

    setGlobalFilters({
      startDate: today.toISOString().split('T')[0],
      endDate: today.toISOString().split('T')[0],
      startHour: '',
      endHour: '',
      storeIds: [],
      categoryIds: [],
      supplierIds: [],
      employeeIds: [],
    })

    sessionStorage.removeItem('dashboardSearch')
  }

  const transactions = transactionsData?.items || [];
  const hasMorePages = transactionsData?.hasMorePages || false;

  const toggleStoreFilter = (storeId: number) => {
    setGlobalFilters(prev => {
      const isSelected = prev.storeIds.includes(storeId);

      return {
        ...prev,
        storeIds: isSelected
          ? prev.storeIds.filter(id => id !== storeId)
          : [...prev.storeIds, storeId]
      };
    })

    setCursorHistory([{ createdAt: null, transactionId: null }])
    setCurrentIndex(0)
  }

  const isSingleDay = globalFilters.startDate !== '' && globalFilters.startDate === globalFilters.endDate;

  const columns = useMemo<ColumnDef<Transaction>[]>(() => [
    { 
      header: 'Date', 
      accessor: 'createdAt',
      render: (t) => new Date(t.createdAt).toLocaleDateString()
    },
    { 
      header: 'Type', 
      accessor: 'transactionType',
      render: (t) => (
        <span className={`px-2 py-1 rounded text-xs font-semibold ${t.transactionType === 'Sale' ? 'bg-emerald-100 text-emerald-700' : 'bg-slate-100 text-slate-700'}`}>
          {t.transactionType}
        </span>
      )
    },
    { header: 'Product', accessor: 'productName', className: 'text-slate-500' },
    { 
      header: 'Qty', 
      accessor: 'quantity',
      className: 'text-right font-medium'
    },
    { 
      header: 'Total', 
      accessor: 'totalAmount',
      className: 'text-right',
      render: (t) => `$${t.totalAmount.toFixed(2)}`
    },
    { 
      header: 'Profit', 
      accessor: 'profit',
      className: 'text-right text-emerald-600 font-medium',
      render: (t) => t.profit > 0 ? `+$${t.profit.toFixed(2)}` : '-'
    }
  ], []);

  return (
    <div className="space-y-8">
      {/* HEADER */}
      <div>
        <h2 className="text-2xl font-bold text-slate-800">Business Overview</h2>
        <p className="text-slate-500 text-sm">Real-time inventory and financial metrics.</p>
      </div>

      {/* GLOBAL FILTERS BAR */}
      <div className="bg-white p-3 rounded-lg shadow-sm border border-slate-200 flex flex-wrap items-center gap-3 mb-6">
  
        {/* Date Filters */}
        <div className="flex-1 min-w-50 border border-slate-300 rounded bg-white h-10 overflow-hidden">
          <DatePicker
              selectsRange={true}
              startDate={startDate}
              endDate={endDate}
              onChange={(update: [Date | null, Date | null]) => {
                  setDateRange(update);
                  const [newStart, newEnd] = update;
                  setGlobalFilters(prev => ({
                    ...prev,
                    startDate: newStart ? newStart.toISOString().split('T')[0] : '',
                    endDate: newEnd ? newEnd.toISOString().split('T')[0] : ''
                  }))
              }}
              maxDate={new Date()} 
              placeholderText="Select date range..."
              wrapperClassName="w-full"
              className="w-full p-2 text-sm text-left outline-none bg-transparent"
          />
        </div>

        {/* Time Window Filters */}
        <div className="flex-1 min-w-[220px] flex items-center border border-slate-300 rounded bg-white h-10 px-1 hover:border-emerald-400 transition-colors focus-within:border-emerald-500 focus-within:ring-1 focus-within:ring-emerald-500">
          <select 
            value={globalFilters.startHour || ''}
            onChange={(e) => {
              setGlobalFilters(prev => ({ ...prev, startHour: e.target.value }));
              setCursorHistory([{ createdAt: null, transactionId: null }]);
              setCurrentIndex(0);
            }}
            className="w-full text-sm outline-none bg-transparent text-slate-700 cursor-pointer text-center appearance-none px-2"
          >
            <option value="">Start Time</option>
            {TIME_OPTIONS.map(time => (
              <option 
                key={`start-${time}`} 
                value={time} 
                // RESTRICTION: Disable any start time that is later than the selected end time
                disabled={globalFilters.endHour ? time > globalFilters.endHour : false}
              >
                {time}
              </option>
            ))}
          </select>
          
          <span className="text-slate-300 font-medium px-1">-</span>
          
          <select 
            value={globalFilters.endHour || ''}
            onChange={(e) => {
              setGlobalFilters(prev => ({ ...prev, endHour: e.target.value }));
              setCursorHistory([{ createdAt: null, transactionId: null }]);
              setCurrentIndex(0);
            }}
            className="w-full text-sm outline-none bg-transparent text-slate-700 cursor-pointer text-center appearance-none px-2"
          >
            <option value="">End Time</option>
            {TIME_OPTIONS.map(time => (
              <option 
                key={`end-${time}`} 
                value={time}
                // RESTRICTION: Disable any end time that is earlier than the selected start time
                disabled={globalFilters.startHour ? time < globalFilters.startHour : false}
              >
                {time}
              </option>
            ))}
          </select>
        </div>

        {/* Category Filter */}
        <div className="flex-1 min-w-40">
          <MultiSelectDropdown 
            placeholder="All Categories"
            options={categories.map(c => ({ id: c.categoryId, name: c.name }))}
            selectedIds={globalFilters.categoryIds}
            onChange={(ids) => {
              setGlobalFilters(prev => ({ ...prev, categoryIds: ids }));
              setCursorHistory([{ createdAt: null, transactionId: null }]);
              setCurrentIndex(0);
            }}
          />
        </div>

        {/*Supplier Filter*/}
        <div className="flex-1 min-w-40">
          <MultiSelectDropdown 
            placeholder="All Suppliers"
            options={suppliers.map(s => ({ id: s.supplierId, name: s.companyName }))}
            selectedIds={globalFilters.supplierIds}
            onChange={(ids) => {
              setGlobalFilters(prev => ({ ...prev, supplierIds: ids }));
              setCursorHistory([{ createdAt: null, transactionId: null }]);
              setCurrentIndex(0);
            }}
          />
        </div>
        
        {/*Store Filter*/}
        <div className="flex-1 min-w-40">
          <MultiSelectDropdown 
            placeholder="All Stores"
            options={storesLookup.map(s => ({ id: s.storeId, name: s.name }))}
            selectedIds={globalFilters.storeIds}
            onChange={(ids) => {
              setGlobalFilters(prev => ({ ...prev, storeIds: ids }));
              setCursorHistory([{ createdAt: null, transactionId: null }]);
              setCurrentIndex(0);
            }}
          />
        </div>

        {/* Employee Custom Dropdown */}
        <div className="flex-1 min-w-35">
          <MultiSelectDropdown 
            placeholder="All Employees"
            options={employees.map((e: any) => ({ id: e.userId, name: e.fullName || e.username }))}
            selectedIds={globalFilters.employeeIds}
            onChange={(ids) => {
              setGlobalFilters(prev => ({ ...prev, employeeIds: ids }));
              setCursorHistory([{ createdAt: null, transactionId: null }]);
              setCurrentIndex(0);
            }}
          />
        </div>

        {/*Clear filter */}
        <button
          onClick={clearFilters}
          title="Clear all filters"
          className="h-10 w-10 shrink-0 bg-slate-50 text-slate-500 rounded border border-slate-300 flex items-center justify-center hover:bg-red-50 hover:text-red-600 hover:border-red-300 transition-colors"
        >
          <FilterX size={18} />
        </button>

      </div>

      {/* TOP ZONE: KPI CARDS */}
      <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-4 gap-4">
        {/* Card 1: Total Sale */}
        <div className="bg-white p-6 rounded-lg shadow-sm border border-slate-200 flex items-center space-x-4">
          <div className="p-3 bg-emerald-50 rounded-full text-emerald-600">
            <DollarSign size={24} />
          </div>
          <div>
            <p className="text-sm font-medium text-slate-500">Total Sales</p>
            <h3 className="text-2xl font-bold text-slate-800">
              {isLoadingStats ? '...' : `$${stats?.totalSales?.toLocaleString()}`}
            </h3>
          </div>
        </div>

        {/* Card 2: Total Profit */}
        <div className="bg-white p-6 rounded-lg shadow-sm border border-slate-200 flex items-center space-x-4">
          <div className="p-3 bg-emerald-50 rounded-full text-emerald-600">
            <TrendingUp size={24} />
          </div>
          <div>
            <p className="text-sm font-medium text-slate-500">Total Profit</p>
            <h3 className="text-2xl font-bold text-slate-800">
              {isLoadingStats ? '...' : `$${stats?.totalProfit.toLocaleString()}`}
            </h3>
          </div>
        </div>

        {/* Card 3: Margin */}
        <div className="bg-white p-6 rounded-lg shadow-sm border border-slate-200 flex items-center space-x-4">
          <div className="p-3 bg-blue-50 rounded-full text-blue-600">
            <Package size={24} />
          </div>
          <div>
            <p className="text-sm font-medium text-slate-500">Profit Margin</p>
            <h3 className="text-2xl font-bold text-slate-800">
              {isLoadingStats ? '...' : `${stats?.marginPrecentage?.toFixed(1)}%`}
            </h3>
          </div>
        </div>

        {/* Card 4: Alerts */}
        <div 
          onClick={() => navigate('/inventory', { state: { triggerLowStock: true } })}
          title="Click to view low stock products"
          className={`bg-white p-6 rounded-lg shadow-sm border border-slate-200 flex items-center space-x-4 cursor-pointer transition-all duration-200 hover:-translate-y-1 hover:shadow-md active:translate-y-0 active:shadow-sm ${
            stats?.lowStockCount && stats.lowStockCount > 0 ? 'hover:border-red-300' : 'hover:border-emerald-300'
          }`}
        >
          <div className={`p-3 rounded-full ${stats?.lowStockCount && stats.lowStockCount > 0 ? 'bg-red-50 text-red-600' : 'bg-emerald-50 text-emerald-600'}`}>
            <AlertCircle size={24} />
          </div>
          <div>
            <p className="text-sm font-medium text-slate-500">Health Status</p>
            <h3 className={`text-xl font-bold ${stats?.lowStockCount && stats.lowStockCount > 0 ? 'text-red-600' : 'text-emerald-600'}`}>
              {isLoadingStats ? '...' : stats?.inventoryHealth}
            </h3>
            {stats && stats.lowStockCount > 0 && (
              <p className="text-xs text-red-500 font-medium mt-1">{stats.lowStockCount} items low on stock</p>
            )}

            <p className="text-[11px] text-slate-400 font-normal mt-0.5 sm:hidden">
              Tap to view list &rarr;
            </p>
          </div>
        </div>
      </div>

      {/* MIDDLE ZONE: CHARTS */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-4">
        
        {/* Left Side: Revenue Trend */}
        <div className="lg:col-span-2 bg-white p-6 rounded-lg shadow-sm border border-slate-200">
          <h3 className="text-lg font-bold text-slate-800 mb-6">Revenue & Profit Trend</h3>
          <div className="h-72 w-full relative">
            <div className="absolute inset-0">
              {isLoadingStats ? (
                <div className="w-full h-full flex items-center justify-center text-slate-400">Loading chart data...</div>
              ) : (
                <ResponsiveContainer width="99%" height="100%" minWidth={1} minHeight={1}>
                  <AreaChart data={chartData} margin={{ top: 10, right: 10, left: -20, bottom: 0 }}>
                    <defs>
                      <linearGradient id="colorRevenue" x1="0" y1="0" x2="0" y2="1">
                        <stop offset="5%" stopColor="#0ea5e9" stopOpacity={0.3}/>
                        <stop offset="95%" stopColor="#0ea5e9" stopOpacity={0}/>
                      </linearGradient>
                      <linearGradient id="colorProfit" x1="0" y1="0" x2="0" y2="1">
                        <stop offset="5%" stopColor="#10b981" stopOpacity={0.3}/>
                        <stop offset="95%" stopColor="#10b981" stopOpacity={0}/>
                      </linearGradient>
                    </defs>
                    <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="#e2e8f0" />
                    <XAxis dataKey="date" axisLine={false} tickLine={false} tick={{ fill: '#64748b', fontSize: 12 }} dy={10} />
                    <YAxis axisLine={false} tickLine={false} tick={{ fill: '#64748b', fontSize: 12 }} tickFormatter={(value) => `$${value}`} />
                    <Tooltip 
                        contentStyle={{ borderRadius: '8px', border: '1px solid #e2e8f0', boxShadow: '0 4px 6px -1px rgb(0 0 0 / 0.1)' }}
                        // Dynamic logic applied to the tooltip title
                        labelFormatter={(label) => `${isSingleDay ? 'Time' : 'Date'}: ${label}`}
                        formatter={(value: any) => {
                          if(value == undefined || value == null) return ['N/A', "Value"];
                          return [`$${Number(value).toFixed(2)}`]
                      }}
                    />
                    <Area type="monotone" dataKey="revenue" name="Revenue" stroke="#0ea5e9" strokeWidth={3} fillOpacity={1} fill="url(#colorRevenue)" />
                    <Area type="monotone" dataKey="profit" name="Profit" stroke="#10b981" strokeWidth={3} fillOpacity={1} fill="url(#colorProfit)" />
                  </AreaChart>
                </ResponsiveContainer>
              )}
            </div>
          </div>
        </div>

        {/* Right Side: Top 5 Products Bar Chart */}
        <div className="bg-white p-6 rounded-lg shadow-sm border border-slate-200">
          <h3 className="text-lg font-bold text-slate-800 mb-6">Top Products by Revenue</h3>
          <div className="h-72 min-h-72 w-full ">
              {isLoadingStats ? (
                <div className="w-full h-full flex items-center justify-center text-slate-400">Loading top products...</div>
              ) : (
                <ResponsiveContainer width="99%" height="100%" minWidth={1} minHeight={1} initialDimension={{ width: 520, height: 288 }}>
                  <BarChart data={topProducts} layout="vertical" margin={{ top: 0, right: 0, left: 10, bottom: 0 }}>
                    <CartesianGrid strokeDasharray="3 3" horizontal={true} vertical={false} stroke="#e2e8f0" />
                    <XAxis type="number" hide />
                    <YAxis dataKey="productName" type="category" axisLine={false} tickLine={false} tick={{ fill: '#475569', fontSize: 12 }} width={80} />
                    <Tooltip 
                      cursor={{fill: '#f1f5f9'}}
                      contentStyle={{ borderRadius: '8px', border: '1px solid #e2e8f0', boxShadow: '0 4px 6px -1px rgb(0 0 0 / 0.1)' }}
                      formatter={(value: any) => {
                          if (value == undefined || value == null) return ['N/A', "value"];
                          return [`$${Number(value).toLocaleString()}`, 'Revenue']
                      } }
                          
                    />
                    <Bar dataKey="revenue" fill="#10b981" radius={[0, 4, 4, 0]} barSize={24} />
                  </BarChart>
                </ResponsiveContainer>
              )}
          </div>
        </div>

      </div>

      {/* SPATIAL STORE MAP */}
      <div className="bg-white p-6 rounded-lg shadow-sm border border-slate-200">
          <div className="flex justify-between items-center mb-6">
              <div>
                  <h3 className="text-lg font-bold text-slate-800">Operational Territory</h3>
                  <p className="text-xs text-slate-500">
                    {globalFilters.storeIds.length > 0
                      ? `${globalFilters.storeIds.length} stores selected`
                      : `Showing all ${storesLookup.length} active locations`} 
                  </p>
              </div>
          </div>
          <div className="h-125 w-full relative rounded-lg overflow-hidden">
              <StoreMapCanvas 
                center={mapCenter} 
                stores={storesLookup} 
                selectedStoreIds={globalFilters.storeIds}
                onStoreClick={toggleStoreFilter}/>
          </div>
      </div>

      {/* BOTTOM ZONE: TRANSACTION LEDGER */}
      <div className="space-y-4">
        <div className="flex flex-col sm:flex-row justify-between sm:items-center gap-4">
          <h3 className="text-lg font-bold text-slate-800">Transaction Ledger</h3>

          <div className="flex bg-white p-1 rounded-md shadow-sm border border-slate-200 w-full sm:w-72">
            <input 
              type="text" 
              placeholder="Search ID or Product Name..." 
              value={searchTerm}
              onChange={(e) => handleSearchChange(e.target.value)}
              className="w-full px-3 py-2 outline-none text-sm bg-transparent"
            />
          </div>
        </div>

        <CursorDataTable 
          data={transactions}
          columns={columns}
          isLoading={isLoadingLedger}
          hasMorePages={hasMorePages}
          isFirstPage={currentIndex === 0}
          onNext={handleNext}
          onPrevious={handlePrevious}
        />
      </div>
    </div>
  );
}
