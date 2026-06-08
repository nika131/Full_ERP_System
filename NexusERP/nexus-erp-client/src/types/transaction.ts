export interface Transaction {
    transactionId: number;
    productId: number;
    supplierId: number;
    userId: number;
    productName: string;
    companyName: string;
    transactionType: string; 
    quantity: number;
    unitPrice: number;
    totalAmount: number;
    profit: number;
    createdAt: string;
}