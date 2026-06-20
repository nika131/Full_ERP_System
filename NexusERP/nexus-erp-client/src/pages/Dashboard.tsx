import { useEffect, useMemo, useState } from "react";
import type { TopProduct, DashbaordStats } from "../types/dashboard";
import type { Transaction } from "../types/transaction";
import { dashboaredService } from "../api/dashboardService";
import { CursorDataTable, type ColumnDef } from "../components/Ui/CursorDataTable";
import { AlertCircle, DollarSign, Package, TrendingUp } from "lucide-react";
import type { ChartData } from "recharts/types/state/chartDataSlice";
import { AreaChart, Area, CartesianGrid, ResponsiveContainer, Tooltip, XAxis, YAxis, Bar, BarChart } from "recharts";

type TransactionCursorState = {
  createdAt: string | null;
  transactionId: number | null;
};

export default function Dashboard() {
  const [stats, setStats] = useState<DashbaordStats | null>(null);
  const [chartData, setChartData] = useState<ChartData[]>([]);
  const [topProducts, SetTopProducts] = useState<TopProduct[]>([]);
  const [isLoadingStats, setIsLoadingStats] = useState(true);

  const [transactions, setTransactions] = useState<Transaction[]>([]);
  const [isLoadingLedger, setIsloadingLedger] = useState(true);
  
  const [cursorHistory, setCursorHistory] = useState<TransactionCursorState[]>([{ createdAt: null, transactionId: null }]);
  const [currentIndex, setCurrentIndex] = useState(0);
  const [hasMorePages, setHasMorePages] = useState(false);
  
  const [searchTerm, setSearchTerm] = useState('');

  useEffect(() => {
    const loadDashboardData = async () => {
      try {
        setIsLoadingStats(true);
        const [statsData, chartRes, topProdRes] = await Promise.all([
          dashboaredService.getStatistics(),
          dashboaredService.getChartData(),
          dashboaredService.getTopProducts(),
        ]);
        setStats(statsData);
        setChartData(chartRes);
        SetTopProducts(topProdRes);
      } catch (err) {
        console.error("Failed to load Dashboard data", err);
      } finally {
        setIsLoadingStats(false);
      }
    };
    loadDashboardData();
  }, []);

  useEffect(() => {
    const controller = new AbortController();
    const timer = setTimeout(() => {
      loadLedger(controller.signal);
    }, 300);
    return () => { clearTimeout(timer); controller.abort(); };
  }, [currentIndex, searchTerm]);

  const loadLedger = async (signal: AbortSignal) => {
    try {
      setIsloadingLedger(true);
      const currentCursor = cursorHistory[currentIndex];
      
      const numericSearchId = searchTerm && !isNaN(Number(searchTerm)) ? Number(searchTerm) : null;

      const data = await dashboaredService.getTransactions(
        10, 
        currentCursor.createdAt, 
        currentCursor.transactionId, 
        null,
        null, 
        numericSearchId, 
        "All", 
        signal
      );
      
      if (!signal.aborted) {
        setTransactions(data.items);
        setHasMorePages(data.hasMorePages);
        
        if (data.hasMorePages && cursorHistory.length === currentIndex + 1) {
          setCursorHistory(prev => [
            ...prev,
            { 
              createdAt: data.nextCreatedAt ?? null, 
              transactionId: data.nextTransactionId ?? null 
            } 
          ]);
        }
      }
    } catch (err: any) {
      if (!signal.aborted && err.name !== 'CanceledError') console.error(err);
    } finally {
      if(!signal.aborted){
        setIsloadingLedger(false);
      }
    }
  };

  const handleSearchChange = (val: string) => {
    setSearchTerm(val);
    setCursorHistory([{ createdAt: null, transactionId: null }]);
    setCurrentIndex(0);
  };

  const handleNext = () => setCurrentIndex(prev => prev + 1);
  const handlePrevious = () => setCurrentIndex(prev => prev - 1);

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

      {/* TOP ZONE: KPI CARDS */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        {/* Card 1: Total Value */}
        <div className="bg-white p-6 rounded-lg shadow-sm border border-slate-200 flex items-center space-x-4">
          <div className="p-3 bg-emerald-50 rounded-full text-emerald-600">
            <DollarSign size={24} />
          </div>
          <div>
            <p className="text-sm font-medium text-slate-500">Inventory Value</p>
            <h3 className="text-2xl font-bold text-slate-800">
              {isLoadingStats ? '...' : `$${stats?.totalValue.toLocaleString()}`}
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
              {isLoadingStats ? '...' : `${stats?.marginPrecentage.toFixed(1)}%`}
            </h3>
          </div>
        </div>

        {/* Card 4: Alerts */}
        <div className="bg-white p-6 rounded-lg shadow-sm border border-slate-200 flex items-center space-x-4">
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
          </div>
        </div>
      </div>

      {/* MIDDLE ZONE: CHARTS */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-4">
        
        {/* Left Side: 7-Day Revenue Trend */}
        <div className="lg:col-span-2 bg-white p-6 rounded-lg shadow-sm border border-slate-200">
          <h3 className="text-lg font-bold text-slate-800 mb-6">7-Day Revenue & Profit Trend</h3>
          <div className="h-72 min-h-75 w-full">
            {isLoadingStats ? (
              <div className="w-full h-full flex items-center justify-center text-slate-400">Loading chart data...</div>
            ) : (
              <ResponsiveContainer width="100%" height="100%">
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

        {/* Right Side: Top 5 Products Bar Chart */}
        <div className="bg-white p-6 rounded-lg shadow-sm border border-slate-200">
          <h3 className="text-lg font-bold text-slate-800 mb-6">Top Products by Revenue</h3>
          <div className="h-72 min-h-75 w-full">
            {isLoadingStats ? (
              <div className="w-full h-full flex items-center justify-center text-slate-400">Loading top products...</div>
            ) : (
              <ResponsiveContainer width="100%" height="100%">
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

      {/* BOTTOM ZONE: TRANSACTION LEDGER */}
      <div className="space-y-4">
        <div className="flex justify-between items-center">
          <h3 className="text-lg font-bold text-slate-800">Transaction Ledger</h3>
          <div className="flex bg-white p-1 rounded-md shadow-sm border border-slate-200 w-72">
            <input 
              type="text" 
              placeholder="Search by Transaction ID..." 
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