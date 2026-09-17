export interface DashbaordStats {
    totalValue: number;
    totalCost: number;
    totalProfit: number;
    totalSales: number;
    lowStockCount: number;
    marginPrecentage: number;
    inventoryHealth: string;
}

export interface ChartData {
  date: string;
  revenue: number;
  profit: number;
}

export interface TopProduct {
    proeductName: string;
    revenue: number;
}