export interface Transaction {
    transactionId: number;
    productId: number;
    productName: string;
    companyName: string; 
    transactionType: string; 
    quantity: number;
    totalAmount: number;
    profit: number;
    createdAt: string;
}